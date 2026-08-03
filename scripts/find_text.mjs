// Dumps the widget tree and render tree, then reports which screen is active
// and which candidate button texts are present.
// Usage: CDP_PORT=<port> node scripts/find_text.mjs
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
await send('Runtime.enable');

const evalJs = async (expression) => {
  const r = await send('Runtime.evaluate', { expression, returnByValue: true, awaitPromise: true });
  return r.result ? r.result.value : undefined;
};

async function dump(extension) {
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
  const d = await vsend(extension, { isolateId }).catch(() => null);
  vws.close();
  return (d && (d.data || d.result)) || '';
}

console.log('URL:', await evalJs('location.href'));
const widgetTree = await dump('ext.flutter.debugDumpApp');
writeFileSync(resolve('scripts', 'edge_shots', 'widget-tree.txt'), widgetTree);
const screenNames = ['SplashScreen', 'OnboardingScreen', 'LoginScreen', 'RegisterScreen', 'HomeScreen', 'SearchScreen', 'CategoriesScreen', 'HelpCenterScreen', 'ProfileScreen', 'GoogleSignInButton'];
console.log('SCREENS:', JSON.stringify(screenNames.filter((s) => widgetTree.includes(s))));

const renderTree = await dump('ext.flutter.debugDumpRenderTree');
const texts = ['Next', 'Get Started', 'Skip', 'Sign in with Google', 'Email', 'Password', 'Welcome Back', 'Sign In', 'Already have an account'];
console.log('TEXTS:', JSON.stringify(texts.map((t) => ({ t, found: renderTree.includes(`"${t}"`) }))));
// Print a small window around 'Next' if present.
const lines = renderTree.split('\n');
const idx = lines.findIndex((l) => l.includes('"Next"'));
if (idx >= 0) {
  console.log('NEXT_LINE:', lines[idx].slice(0, 300));
}
process.exit(0);
