// Locates a widget by its text in the Flutter render tree, computes its
// absolute position by summing parentData offsets, and clicks it.
// Usage: CDP_PORT=<port> node scripts/render_click.mjs <textToFind> [fallbackText]
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';

const CDP_PORT = Number(process.env.CDP_PORT || 54976);
const RUN_LOG = resolve('flutter_edge_run.log');
const TARGET = process.argv[2] || 'Next';
const FALLBACK = process.argv[3] || '';
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
await send('Runtime.enable');

const evalJs = async (expression) => {
  const r = await send('Runtime.evaluate', { expression, returnByValue: true, awaitPromise: true });
  return r.result ? r.result.value : undefined;
};

async function dumpRender() {
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
  const dump = await vsend('ext.flutter.debugDumpRenderTree', { isolateId }).catch(() => null);
  vws.close();
  return (dump && (dump.data || dump.result)) || '';
}

async function clickAt(x, y) {
  await send('Input.dispatchMouseEvent', { type: 'mouseMoved', x, y });
  await send('Input.dispatchMouseEvent', { type: 'mousePressed', x, y, button: 'left', clickCount: 1 });
  await send('Input.dispatchMouseEvent', { type: 'mouseReleased', x, y, button: 'left', clickCount: 1 });
}

function findTextPosition(render, text) {
  const lines = render.split('\n');
  let idx = -1;
  for (let i = 0; i < lines.length; i++) {
    if (lines[i].includes(`"${text}"`)) { idx = i; break; }
  }
  if (idx === -1) return null;
  // Walk up: the ancestor chain is exactly the sequence of lines above with
  // strictly decreasing indentation. Sum their parentData offsets.
  const paraIndent = lines[idx].match(/^ */)[0].length;
  const ancestors = [];
  let need = paraIndent;
  for (let i = idx - 1; i >= 0; i--) {
    const indent = lines[i].match(/^ */)[0].length;
    if (indent < need) {
      ancestors.push(i);
      need = indent;
      if (indent === 0) break;
    }
  }
  let x = 0;
  let y = 0;
  const addOffsets = (i) => {
    const m = lines[i].match(/offset=Offset\((-?[\d.]+), (-?[\d.]+)\)/);
    if (m) {
      x += parseFloat(m[1]);
      y += parseFloat(m[2]);
    }
  };
  addOffsets(idx);
  for (const a of ancestors) addOffsets(a);
  const size = lines[idx].match(/size=Size\(([\d.]+), ([\d.]+)\)/);
  const w = size ? parseFloat(size[1]) : 80;
  const h = size ? parseFloat(size[2]) : 25;
  return { x: Math.round(x + w / 2), y: Math.round(y + h / 2), cx: Math.round(x), cy: Math.round(y), w: Math.round(w), h: Math.round(h) };
}

const render = await dumpRender();
const pos = findTextPosition(render, TARGET) || (FALLBACK ? findTextPosition(render, FALLBACK) : null);
if (!pos) {
  console.log('NOT_FOUND:', TARGET, FALLBACK);
  console.log('render len:', render.length);
  process.exit(1);
}
console.log('TARGET:', TARGET, 'POS:', JSON.stringify(pos));
const vp = await evalJs('({ w: window.innerWidth, h: window.innerHeight })');
console.log('VIEWPORT:', JSON.stringify(vp));
const safeX = Math.max(5, Math.min(vp.w - 5, pos.x));
const safeY = Math.max(5, Math.min(vp.h - 5, pos.y));
console.log('CLICK_AT:', safeX, safeY);
await clickAt(safeX, safeY);
console.log('CLICKED');
await sleep(2500);
process.exit(0);
