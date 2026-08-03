// Debug: print the render tree window + ancestor chain around a target text.
// Usage: CDP_PORT=<port> node scripts/render_inspect.mjs <text>
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';

const CDP_PORT = Number(process.env.CDP_PORT || 54976);
const RUN_LOG = resolve('flutter_edge_run.log');
const TARGET = process.argv[2] || 'Sign in with Google';
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
const dump = await vsend('ext.flutter.debugDumpRenderTree', { isolateId });
const render = (dump && (dump.data || dump.result)) || '';
const lines = render.split('\n');
const idx = lines.findIndex((l) => l.includes(`"${TARGET}"`));
console.log('TARGET_INDEX:', idx, 'of', lines.length);
if (idx === -1) {
  console.log('not found');
  process.exit(1);
}
console.log('=== paragraph line ===');
console.log(lines[idx].slice(0, 500));
console.log('=== ancestor chain ===');
const paraIndent = lines[idx].match(/^ */)[0].length;
let need = paraIndent;
for (let i = idx - 1; i >= 0; i--) {
  const indent = lines[i].match(/^ */)[0].length;
  if (indent < need) {
    console.log(`indent=${indent}`, lines[i].slice(0, 260));
    need = indent;
    if (indent === 0) break;
  }
}
console.log('=== window around target (30 lines) ===');
for (let i = Math.max(0, idx - 30); i <= idx; i++) {
  console.log(lines[i].slice(0, 200));
}
process.exit(0);
