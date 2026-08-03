// Comprehensive Google Sign-In debug capture on the LIVE Edge instance.
// Verifies: window.location.origin, whether a Google popup opens, console
// errors, network requests (especially accounts.google.com / oauth), and
// exceptions while clicking "Sign in with Google".
// Usage: CDP_PORT=<port> node scripts/edge_google_debug.mjs [appUrl]
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';

const APP_URL = process.argv[2] || 'http://localhost:8090';
const CDP_PORT = Number(process.env.CDP_PORT || 54976);
const RUN_LOG = resolve('flutter_edge_run.log');
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

function findVmUrl() {
  try {
    const log = readFileSync(RUN_LOG, 'utf8');
    const m = log.match(/ws:\/\/127\.0\.0\.1:\d+\/[A-Za-z0-9]+=\/ws/g);
    return m ? m[m.length - 1] : null;
  } catch {
    return null;
  }
}

const targets = await (await fetch(`http://127.0.0.1:${CDP_PORT}/json`)).json();
const page = targets.find((t) => t.type === 'page' && t.url.includes('localhost'));
if (!page) {
  console.log('NO_PAGE_TARGET');
  process.exit(1);
}
const ws = new WebSocket(page.webSocketDebuggerUrl);
let id = 0;
const pending = new Map();
const report = {
  origin: null,
  viewport: null,
  consoleErrors: [],
  exceptions: [],
  network: [],
  oauthRequests: [],
  popups: [],
  newTargets: [],
};
ws.onmessage = (ev) => {
  const m = JSON.parse(ev.data);
  if (m.id && pending.has(m.id)) {
    pending.get(m.id)(m.result);
    pending.delete(m.id);
  } else if (m.method === 'Runtime.consoleAPICalled' && ['error', 'assert', 'warning'].includes(m.params.type)) {
    report.consoleErrors.push({ type: m.params.type, text: m.params.args.map((a) => a.value ?? a.description ?? '').join(' ') });
  } else if (m.method === 'Runtime.exceptionThrown') {
    const d = m.params.exceptionDetails || {};
    report.exceptions.push((d.exception && (d.exception.description || d.exception.value)) || d.text || 'ex');
  } else if (m.method === 'Network.responseReceived') {
    const r = m.params.response;
    report.network.push({ status: r.status, url: r.url.slice(0, 160) });
    if (/accounts\.google\.com|oauth|googleapis|identity/.test(r.url)) {
      report.oauthRequests.push({ status: r.status, url: r.url.slice(0, 200) });
    }
  } else if (m.method === 'Network.requestWillBeSent') {
    if (/accounts\.google\.com|oauth|googleapis/.test(m.params.request.url)) {
      report.oauthRequests.push({ sent: m.params.request.url.slice(0, 200) });
    }
  } else if (m.method === 'Target.targetCreated') {
    const t = m.params.targetInfo;
    report.newTargets.push({ type: t.type, url: t.url.slice(0, 160) });
  }
};
const send = (method, params = {}) =>
  new Promise((res) => {
    const i = ++id;
    pending.set(i, res);
    ws.send(JSON.stringify({ id: i, method, params }));
  });
await new Promise((r) => (ws.onopen = r));
await send('Runtime.enable');
await send('Network.enable');
await send('Log.enable');
await send('Page.enable');
await send('Target.setDiscoverTargets', { discover: true });

const evalJs = async (expression) => {
  const r = await send('Runtime.evaluate', { expression, returnByValue: true, awaitPromise: true });
  return r.result && !r.exceptionDetails ? r.result.value : undefined;
};

async function dumpTree() {
  try {
    const vmUrl = findVmUrl();
    if (!vmUrl) return '';
    const vws = new WebSocket(vmUrl);
    let vid = 0;
    const vpending = new Map();
    vws.onmessage = (ev) => {
      const m = JSON.parse(ev.data);
      if (m.id && vpending.has(m.id)) {
        vpending.get(m.id)(m.result);
        vpending.delete(m.id);
      }
    };
    const vsend = (method, params = {}) =>
      new Promise((res) => {
        const i = ++vid;
        vpending.set(i, res);
        vws.send(JSON.stringify({ jsonrpc: '2.0', id: i, method, params }));
      });
    await new Promise((r) => (vws.onopen = r));
    let isolateId = null;
    for (let i = 0; i < 15 && !isolateId; i++) {
      try {
        const vm = await vsend('getVM');
        if (vm.isolates && vm.isolates.length) isolateId = vm.isolates[0].id;
      } catch {}
      if (!isolateId) await sleep(1500);
    }
    if (!isolateId) { vws.close(); return ''; }
    const dump = await vsend('ext.flutter.debugDumpApp', { isolateId }).catch(() => null);
    vws.close();
    return (dump && (dump.data || dump.result)) || '';
  } catch {
    return '';
  }
}

async function clickAt(x, y) {
  await send('Input.dispatchMouseEvent', { type: 'mouseMoved', x, y });
  await send('Input.dispatchMouseEvent', { type: 'mousePressed', x, y, button: 'left', clickCount: 1 });
  await send('Input.dispatchMouseEvent', { type: 'mouseReleased', x, y, button: 'left', clickCount: 1 });
}

// 1. Origin + viewport
report.origin = await evalJs('window.location.origin');
report.href = await evalJs('window.location.href');
report.viewport = await evalJs('({ w: window.innerWidth, h: window.innerHeight })');

// 2. Current screen via widget tree
let tree = await dumpTree();
report.onLogin = tree.includes('LoginScreen');
report.onOnboarding = tree.includes('OnboardingScreen');
report.hasGoogleButton = tree.includes('GoogleSignInButton');

// 3. If not on login, try direct navigation (app boots to onboarding when unauth)
if (!report.onLogin) {
  await send('Page.navigate', { url: `${APP_URL}/login` });
  await sleep(20000);
  tree = await dumpTree();
  report.onLoginAfterNav = tree.includes('LoginScreen');
  report.hasGoogleButtonAfterNav = tree.includes('GoogleSignInButton');
}

// 4. Click the Google button at several candidate positions, capturing console.
report.clicks = [];
const vp = report.viewport || { w: 1026, h: 656 };
const cx = Math.round(vp.w / 2);
for (const y of [545, 555, 565, 575, 585, 595]) {
  report.consoleErrors.length = 0;
  report.exceptions.length = 0;
  report.oauthRequests.length = 0;
  report.newTargets.length = 0;
  await clickAt(cx, y);
  await sleep(3500);
  tree = await dumpTree();
  const cancelled = tree.includes('Google sign-in was cancelled.');
  const notConfigured = tree.includes('not configured for this build');
  const loading = tree.includes('Signing in');
  report.clicks.push({ x: cx, y, cancelled, notConfigured, loading,
    consoleErrors: [...report.consoleErrors], exceptions: [...report.exceptions],
    oauth: [...report.oauthRequests], newTargets: [...report.newTargets] });
  if (cancelled || notConfigured) break;
}

console.log('=== EDGE_GOOGLE_DEBUG ===');
console.log(JSON.stringify(report, null, 2));
process.exit(0);
