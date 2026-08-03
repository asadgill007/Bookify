// Clicks the Google Sign-In button on the live login screen by trying
// candidate positions, verifying each attempt via the widget tree for the
// graceful-degradation snackbar (no GOOGLE_CLIENT_ID in this build).
// Usage: CDP_PORT=<port> node scripts/click_google.mjs
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';

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
  } else if (m.method === 'Runtime.consoleAPICalled' && ['error', 'assert'].includes(m.params.type)) {
    consoleEvents.push(m.params.args.map((a) => a.value ?? a.description ?? '').join(' '));
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

const evalJs = async (expression) => {
  const r = await send('Runtime.evaluate', { expression, returnByValue: true, awaitPromise: true });
  return r.result ? r.result.value : undefined;
};

async function dumpTree() {
  try {
    const vws = new WebSocket(findVmUrl());
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

const vp = await evalJs('({ w: window.innerWidth, h: window.innerHeight })');
console.log('VIEWPORT:', JSON.stringify(vp));
const cx = Math.round(vp.w / 2);

let tree = await dumpTree();
console.log('ON_LOGIN:', tree.includes('LoginScreen'));

const candidates = [
  [cx, 555], [cx, 565], [cx, 575], [cx, 585], [cx, 595], [cx, 605], [cx, 615],
];
let result = null;
for (const [x, y] of candidates) {
  await clickAt(x, y);
  await sleep(2500);
  tree = await dumpTree();
  const cancelled = tree.includes('Google sign-in was cancelled.');
  const notConfigured = tree.includes('not configured for this build');
  console.log(`click (${x},${y}): cancelled=${cancelled} notConfigured=${notConfigured}`);
  if (cancelled || notConfigured) {
    result = { x, y, cancelled, notConfigured };
    break;
  }
}
console.log('RESULT:', JSON.stringify(result));
console.log('CONSOLE_ERRORS:', JSON.stringify(consoleEvents));
console.log('EXCEPTIONS:', JSON.stringify(exceptions));
process.exit(0);
