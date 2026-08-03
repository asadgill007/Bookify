// Click-through test: Onboarding → Login screen → Google Sign-In button.
// Drives the running Edge instance with real mouse input at computed
// coordinates, verifying each step via the Flutter widget tree.
// Usage: CDP_PORT=<port> node scripts/click_through.mjs [appUrl]
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';

const APP_URL = process.argv[2] || 'http://localhost:8090';
const CDP_PORT = Number(process.env.CDP_PORT || 54976);
const RUN_LOG = resolve('flutter_edge_run.log');
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

function findVmUrl() {
  const log = readFileSync(RUN_LOG, 'utf8');
  const m = log.match(/ws:\/\/127\.0\.0\.1:\d+\/[A-Za-z0-9]+=\/ws/g);
  return m ? m[m.length - 1] : null;
}

const targets = await (await fetch(`http://127.0.0.1:${CDP_PORT}/json`)).json();
const page = targets.find((t) => t.type === 'page' && t.url.includes('localhost'));
const ws = new WebSocket(page.webSocketDebuggerUrl);
let id = 0;
const pending = new Map();
const consoleEvents = [];
const exceptions = [];
ws.onmessage = (ev) => {
  const m = JSON.parse(ev.data);
  if (m.id && pending.has(m.id)) {
    pending.get(m.id)(m.result);
    pending.delete(m.id);
  } else if (m.method === 'Runtime.consoleAPICalled') {
    consoleEvents.push({ type: m.params.type, text: m.params.args.map((a) => a.value ?? a.description ?? '').join(' ') });
  } else if (m.method === 'Runtime.exceptionThrown') {
    const d = m.params.exceptionDetails || {};
    exceptions.push((d.exception && (d.exception.description || d.exception.value)) || d.text || '');
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
await send('Page.enable');

const evalJs = async (expression) => {
  const r = await send('Runtime.evaluate', { expression, returnByValue: true, awaitPromise: true });
  return r.result ? r.result.value : undefined;
};

async function withVm(fn) {
  const vmUrl = findVmUrl();
  if (!vmUrl) throw new Error('no VM url');
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
  // Wait for the isolate to register (app may be mid-restart).
  let isolateId = null;
  for (let i = 0; i < 20 && !isolateId; i++) {
    try {
      const vm = await vsend('getVM');
      if (vm.isolates && vm.isolates.length) isolateId = vm.isolates[0].id;
    } catch {}
    if (!isolateId) await sleep(1500);
  }
  if (!isolateId) {
    vws.close();
    throw new Error('no isolate available');
  }
  try {
    return await fn(vsend, isolateId);
  } finally {
    vws.close();
  }
}

async function dumpTree() {
  try {
    return await withVm(async (vsend, isolateId) => {
      const dump = await vsend('ext.flutter.debugDumpApp', { isolateId });
      return (dump && (dump.data || dump.result)) || '';
    });
  } catch {
    return '';
  }
}

async function clickAt(x, y) {
  await send('Input.dispatchMouseEvent', { type: 'mouseMoved', x, y });
  await send('Input.dispatchMouseEvent', { type: 'mousePressed', x, y, button: 'left', clickCount: 1 });
  await send('Input.dispatchMouseEvent', { type: 'mouseReleased', x, y, button: 'left', clickCount: 1 });
}

async function dumpTreeUntil(predicate, timeoutMs = 90000) {
  const start = Date.now();
  let tree = '';
  while (Date.now() - start < timeoutMs) {
    tree = await dumpTree();
    if (tree && predicate(tree)) return tree;
    await sleep(3000);
  }
  return tree;
}

// Boot fresh at the root so the unauth flow runs: splash → onboarding.
await send('Page.navigate', { url: APP_URL });
const vp = await evalJs('({ w: window.innerWidth, h: window.innerHeight })');
console.log('VIEWPORT:', JSON.stringify(vp));
const W = vp.w;
const H = vp.h;
const cx = Math.round(W / 2);

let tree = await dumpTreeUntil((t) => t.includes('OnboardingScreen'));
console.log('AFTER_BOOT onboarding:', tree.includes('OnboardingScreen'));

// Click "Next" 3 times to reach the last onboarding page.
let nextY = H - 24 - 18 - 32 - 28; // bottom padding + indicator + spacing + half button
for (let i = 0; i < 3; i++) {
  await clickAt(cx, nextY);
  await sleep(2500);
  tree = await dumpTree();
  console.log(`after click ${i + 1}: GetStarted=${tree.includes('Get Started')} Onboarding=${tree.includes('OnboardingScreen')}`);
}

// On the last page the button moved up (extra row below). Click "Sign In".
const signInY = H - 24 - 24; // bottom padding + half of the account row
const signInX = Math.round(cx + 90);
await clickAt(signInX, signInY);
tree = await dumpTreeUntil((t) => t.includes('LoginScreen'), 45000);
console.log('LOGIN_SCREEN:', tree.includes('LoginScreen'));
console.log('GOOGLE_BUTTON:', tree.includes('GoogleSignInButton'));
console.log('STILL_ONBOARDING:', tree.includes('OnboardingScreen'));

// Now locate the Google button via the render tree and click it.
const render = await withVm(async (vsend, isolateId) => {
  const dump = await vsend('ext.flutter.debugDumpRenderTree', { isolateId });
  return (dump && (dump.data || dump.result)) || '';
});
const lines = render.split('\n');
let idx = lines.findIndex((l) => l.includes('Sign in with Google'));
if (idx === -1) {
  console.log('RENDER: Google text not found; dumping nearby context');
  const anyGoogle = lines.findIndex((l) => l.toLowerCase().includes('google'));
  console.log('any-google line:', anyGoogle, anyGoogle >= 0 ? lines.slice(anyGoogle - 5, anyGoogle + 3).join('\n').slice(0, 800) : '');
} else {
  const win = lines.slice(Math.max(0, idx - 25), idx + 1).join('\n');
  const offsets = [...win.matchAll(/offset=Offset\(([-\d.]+), ([-\d.]+)\)/g)].map((m) => [parseFloat(m[1]), parseFloat(m[2])]);
  console.log('GOOGLE_OFFSETS:', JSON.stringify(offsets));
  let gx = 0;
  let gy = 0;
  for (const [ox, oy] of offsets) {
    gx += ox;
    gy += oy;
  }
  gx = Math.round(gx + 80);
  gy = Math.round(gy + 25);
  console.log('GOOGLE_CLICK_AT:', gx, gy);
  await clickAt(gx, gy);
  await sleep(5000);
  tree = await dumpTree();
  console.log('AFTER_GOOGLE_CLICK cancelled:', tree.includes('Google sign-in was cancelled.'));
  console.log('AFTER_GOOGLE_CLICK notConfigured:', tree.includes('not configured'));
  console.log('AFTER_GOOGLE_CLICK snackbarText:', (tree.match(/SnackBar[^\n]{0,120}/) || ['none'])[0].slice(0, 150));
}
console.log('CONSOLE_EVENTS:', JSON.stringify(consoleEvents.slice(-8)));
console.log('EXCEPTIONS:', JSON.stringify(exceptions));
process.exit(0);
