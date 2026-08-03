// Real Google button click test in the running Edge instance.
// Boots the app at /login, locates "Sign in with Google" in the render tree,
// clicks its center via CDP input, then confirms via the widget tree that the
// expected snackbar appears (Google OAuth is intentionally NOT configured in
// this build — no GOOGLE_CLIENT_ID was supplied).
// Usage: CDP_PORT=<port> node scripts/google_button_test.mjs [appUrl]
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

// ── VM service helpers ──────────────────────────────────────────────────────
async function withVm(fn) {
  const vmUrl = findVmUrl();
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
  const vm = await vsend('getVM');
  const isolateId = vm.isolates[0].id;
  try {
    return await fn(vsend, isolateId);
  } finally {
    vws.close();
  }
}

async function dumpTree() {
  return withVm(async (vsend, isolateId) => {
    const dump = await vsend('ext.flutter.debugDumpApp', { isolateId });
    return (dump && (dump.data || dump.result)) || '';
  });
}

async function dumpRender() {
  return withVm(async (vsend, isolateId) => {
    const dump = await vsend('ext.flutter.debugDumpRenderTree', { isolateId });
    return (dump && (dump.data || dump.result)) || '';
  });
}

// ── Boot at /login ──────────────────────────────────────────────────────────
await send('Page.navigate', { url: `${APP_URL}/login` });
await sleep(25000);
console.log('URL:', await evalJs('location.href'));
const viewport = await evalJs('({ w: window.innerWidth, h: window.innerHeight })');
console.log('VIEWPORT:', JSON.stringify(viewport));

// Confirm the login screen is showing.
const tree = await dumpTree();
console.log('HAS_LOGIN_SCREEN:', tree.includes('LoginScreen'));
console.log('HAS_GOOGLE_BUTTON:', tree.includes('GoogleSignInButton'));

// ── Locate the button in the render tree ────────────────────────────────────
const render = await dumpRender();
const lines = render.split('\n');
// RenderParagraph lines carry their text; we look for "Sign in with Google".
let paragraphLine = null;
for (let i = 0; i < lines.length; i++) {
  if (lines[i].includes('Sign in with Google')) {
    // The paragraph line and its ancestors contain the offsets; capture a window.
    paragraphLine = i;
    break;
  }
}
if (paragraphLine === null) {
  console.log('BUTTON_TEXT_NOT_FOUND_IN_RENDER_TREE');
  process.exit(1);
}
console.log('RENDER_CONTEXT:', lines.slice(Math.max(0, paragraphLine - 25), paragraphLine + 1).join('\n').slice(0, 4000));

// Extract all offsets from the window to estimate the global position.
const windowLines = lines.slice(Math.max(0, paragraphLine - 25), paragraphLine + 1).join('\n');
const offsets = [...windowLines.matchAll(/offset=Offset\(([-\d.]+), ([-\d.]+)\)/g)].map((m) => [parseFloat(m[1]), parseFloat(m[2])]);
console.log('OFFSETS:', JSON.stringify(offsets));
// The paragraph's own size (width/height) is in its line.
const sizeMatch = windowLines.match(/size=Size\(([\d.]+), ([\d.]+)\)/g);
console.log('SIZES:', JSON.stringify(sizeMatch));

// Simple approach: sum the last few offsets (each ancestor adds its offset).
let x = 0;
let y = 0;
for (const [ox, oy] of offsets) {
  x += ox;
  y += oy;
}
// Clamp to viewport and center the click on the button.
const cx = Math.min(viewport.w - 10, Math.max(10, x + 80));
const cy = Math.min(viewport.h - 10, Math.max(10, y + 25));
console.log('CLICK_AT:', cx, cy);

// ── Click the button ────────────────────────────────────────────────────────
await send('Input.dispatchMouseEvent', { type: 'mouseMoved', x: cx, y: cy });
await send('Input.dispatchMouseEvent', { type: 'mousePressed', x: cx, y: cy, button: 'left', clickCount: 1 });
await send('Input.dispatchMouseEvent', { type: 'mouseReleased', x: cx, y: cy, button: 'left', clickCount: 1 });
console.log('CLICKED');
await sleep(5000);

// ── Verify result via widget tree ───────────────────────────────────────────
const after = await dumpTree();
const snackbarText = after.match(/SnackBar[^]*?content: [^)]*\)/);
console.log('SNACKBAR_IN_TREE:', snackbarText ? snackbarText[0].slice(0, 200) : 'none');
const hasCancelSnackbar = after.includes('Google sign-in was cancelled.');
const hasConfiguredSnackbar = after.includes('not configured');
console.log('CANCELLED_SNACKBAR:', hasCancelSnackbar);
console.log('NOT_CONFIGURED_SNACKBAR:', hasConfiguredSnackbar);
console.log('CONSOLE_EVENTS:', JSON.stringify(consoleEvents.slice(-10)));
console.log('EXCEPTIONS:', JSON.stringify(exceptions));
process.exit(0);
