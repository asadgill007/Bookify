// Click once and poll the widget tree for ~10s to catch the transient snackbar.
// Usage: CDP_PORT=<port> node scripts/click_poll.mjs [x] [y]
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';

const CDP_PORT = Number(process.env.CDP_PORT || 51863);
const X = Number(process.argv[2] || 513);
const Y = Number(process.argv[3] || 575);
const RUN_LOG = resolve('flutter_edge_run.log');
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

function findVmUrl() {
  try {
    const log = readFileSync(RUN_LOG, 'utf8');
    const m = log.match(/ws:\/\/127\.0\.0\.1:\d+\/[A-Za-z0-9]+=\/ws/g);
    return m ? m[m.length - 1] : null;
  } catch { return null; }
}

const targets = await (await fetch(`http://127.0.0.1:${CDP_PORT}/json`)).json();
const page = targets.find((t) => t.type === 'page' && t.url.includes('localhost'));
const ws = new WebSocket(page.webSocketDebuggerUrl);
let id = 0;
const pending = new Map();
const consoleErrors = [];
ws.onmessage = (ev) => {
  const m = JSON.parse(ev.data);
  if (m.id && pending.has(m.id)) { pending.get(m.id)(m.result); pending.delete(m.id); }
  else if (m.method === 'Runtime.consoleAPICalled' && ['error', 'assert'].includes(m.params.type)) {
    consoleErrors.push(m.params.args.map((a) => a.value ?? a.description ?? '').join(' '));
  }
};
const send = (method, params = {}) => new Promise((res) => {
  const i = ++id; pending.set(i, res); ws.send(JSON.stringify({ id: i, method, params }));
});
await new Promise((r) => (ws.onopen = r));
await send('Runtime.enable');

async function dumpTree() {
  try {
    const vmUrl = findVmUrl();
    if (!vmUrl) return '';
    const vws = new WebSocket(vmUrl);
    let vid = 0;
    const vpending = new Map();
    vws.onmessage = (ev) => {
      const m = JSON.parse(ev.data);
      if (m.id && vpending.has(m.id)) { vpending.get(m.id)(m.result); vpending.delete(m.id); }
    };
    const vsend = (method, params = {}) => new Promise((res) => {
      const i = ++vid; vpending.set(i, res); vws.send(JSON.stringify({ jsonrpc: '2.0', id: i, method, params }));
    });
    await new Promise((r) => (vws.onopen = r));
    let isolateId = null;
    for (let i = 0; i < 10 && !isolateId; i++) {
      try { const vm = await vsend('getVM'); if (vm.isolates && vm.isolates.length) isolateId = vm.isolates[0].id; } catch {}
      if (!isolateId) await sleep(800);
    }
    if (!isolateId) { vws.close(); return ''; }
    const dump = await vsend('ext.flutter.debugDumpApp', { isolateId }).catch(() => null);
    vws.close();
    return (dump && (dump.data || dump.result)) || '';
  } catch { return ''; }
}

// click
await send('Input.dispatchMouseEvent', { type: 'mouseMoved', x: X, y: Y });
await send('Input.dispatchMouseEvent', { type: 'mousePressed', x: X, y: Y, button: 'left', clickCount: 1 });
await send('Input.dispatchMouseEvent', { type: 'mouseReleased', x: X, y: Y, button: 'left', clickCount: 1 });
console.log(`CLICKED (${X},${Y})`);

let found = null;
for (let i = 0; i < 8 && !found; i++) {
  await sleep(800);
  const tree = await dumpTree();
  if (tree.includes('not configured for this build')) found = 'notConfigured';
  else if (tree.includes('Google sign-in was cancelled.')) found = 'cancelled';
  else if (tree.includes('Signing in')) found = 'signingIn';
  console.log(`poll ${i}: len=${tree.length} state=${found ?? 'none'}`);
}
console.log('RESULT:', found ?? 'NOTHING');
console.log('CONSOLE_ERRORS:', JSON.stringify(consoleErrors));
process.exit(0);
