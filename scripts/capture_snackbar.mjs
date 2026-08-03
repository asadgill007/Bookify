// Captures the current state (login screen + Google snackbar) as a screenshot
// and extracts the snackbar text from the widget tree.
// Usage: CDP_PORT=<port> node scripts/capture_snackbar.mjs
import { readFileSync, writeFileSync } from 'node:fs';
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
ws.onmessage = (ev) => {
  const m = JSON.parse(ev.data);
  if (m.id && pending.has(m.id)) {
    pending.get(m.id)(m.result);
    pending.delete(m.id);
  }
};
const send = (method, params = {}) =>
  new Promise((res) => {
    const i = ++id;
    pending.set(i, res);
    ws.send(JSON.stringify({ id: i, method, params }));
  });
await new Promise((r) => (ws.onopen = r));
await send('Page.enable');

const shot = await send('Page.captureScreenshot', { format: 'png' });
const file = resolve('scripts', 'edge_shots', 'login-google-snackbar.png');
writeFileSync(file, Buffer.from(shot.data, 'base64'));
console.log('SCREENSHOT:', file);

// Widget tree snackbar text.
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
const dump = await vsend('ext.flutter.debugDumpApp', { isolateId });
const text = (dump && (dump.data || dump.result)) || '';
const m = text.match(/Google sign-in[^"\\n]*/);
console.log('SNACKBAR_TEXT:', m ? m[0] : 'not found');
writeFileSync(resolve('scripts', 'edge_shots', 'widget-tree-login.txt'), text);
process.exit(0);
