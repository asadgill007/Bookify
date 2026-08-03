// Debug: which onboarding page is active, and does clicking advance it?
// Usage: CDP_PORT=<port> node scripts/click_debug.mjs [appUrl]
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';

const APP_URL = process.argv[2] || 'http://localhost:8090';
const CDP_PORT = Number(process.env.CDP_PORT || 54976);
const RUN_LOG = resolve('flutter_edge_run.log');
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));
const PAGES = ['Discover Premium', 'Book Instantly', 'Secure Payments', 'Digital Tickets'];

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

async function dumpTree() {
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
}

function activePage(tree) {
  const found = PAGES.filter((p) => tree.includes(p.replace(' ', '\\n')) || tree.includes(p));
  // Titles are stored as 'Discover Premium\nServices' — search for the first word.
  const byFirstWord = PAGES.filter((p) => tree.includes(p.split(' ')[0]));
  return { found, byFirstWord };
}

async function clickAt(x, y) {
  await send('Input.dispatchMouseEvent', { type: 'mouseMoved', x, y });
  await send('Input.dispatchMouseEvent', { type: 'mousePressed', x, y, button: 'left', clickCount: 1 });
  await send('Input.dispatchMouseEvent', { type: 'mouseReleased', x, y, button: 'left', clickCount: 1 });
}

await send('Page.navigate', { url: APP_URL });
await sleep(15000);
let tree = '';
for (let i = 0; i < 30; i++) {
  tree = await dumpTree();
  if (tree.includes('OnboardingScreen')) break;
  await sleep(2000);
}
console.log('BOOT page:', JSON.stringify(activePage(tree)));

// Try clicking at several y positions to find the Next button.
for (const y of [554, 540, 570, 520, 500, 480]) {
  await clickAt(513, y);
  await sleep(2500);
  tree = await dumpTree();
  console.log(`click y=${y} -> page:`, JSON.stringify(activePage(tree)));
  // Stop if we advanced to page 1.
  const ap = activePage(tree);
  if (ap.byFirstWord.includes('Book Instantly')) break;
}
process.exit(0);
