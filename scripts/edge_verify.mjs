// Edge verification harness for the Bookify Flutter web app.
// Attaches to a running Edge instance (the one launched by `flutter run -d edge`)
// via the Chrome DevTools Protocol, reloads the app and reports console/runtime
// errors, navigation results and responsive screenshots.
//
// Usage:
//   EDGE_ATTACH=1 CDP_PORT=<port> node scripts/edge_verify.mjs [appUrl]
//   Example: EDGE_ATTACH=1 CDP_PORT=54976 node scripts/edge_verify.mjs http://localhost:8090
import { mkdirSync, writeFileSync } from 'node:fs';
import { resolve } from 'node:path';

const APP_URL = process.argv[2] || 'http://localhost:8090';
const CDP_PORT = Number(process.env.CDP_PORT || 9222);
const SHOT_DIR = resolve('scripts', 'edge_shots');

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));
mkdirSync(SHOT_DIR, { recursive: true });

// ── Minimal CDP client over the built-in WebSocket ─────────────────────────
class CdpClient {
  constructor(url) {
    this.ws = new WebSocket(url);
    this.id = 0;
    this.pending = new Map();
    this.events = [];
    this.listeners = new Map();
  }
  async open() {
    await new Promise((res, rej) => {
      this.ws.onopen = res;
      this.ws.onerror = rej;
    });
    this.ws.onmessage = (ev) => {
      const msg = JSON.parse(ev.data);
      if (msg.id && this.pending.has(msg.id)) {
        const { resolve, reject } = this.pending.get(msg.id);
        this.pending.delete(msg.id);
        msg.error ? reject(new Error(msg.error.message)) : resolve(msg.result);
      } else if (msg.method) {
        const ls = this.listeners.get(msg.method) || [];
        ls.forEach((fn) => fn(msg.params));
        this.events.push(msg);
      }
    };
  }
  send(method, params = {}) {
    const id = ++this.id;
    return new Promise((resolve, reject) => {
      this.pending.set(id, { resolve, reject });
      this.ws.send(JSON.stringify({ id, method, params }));
    });
  }
  on(method, fn) {
    if (!this.listeners.has(method)) this.listeners.set(method, []);
    this.listeners.get(method).push(fn);
  }
  close() {
    try {
      this.ws.close();
    } catch {}
  }
}

async function waitForTarget() {
  for (let i = 0; i < 60; i++) {
    try {
      const res = await fetch(`http://127.0.0.1:${CDP_PORT}/json`);
      const targets = await res.json();
      const page = targets.find(
        (t) => t.type === 'page' && t.url.includes('localhost'),
      );
      if (page) return page.webSocketDebuggerUrl;
    } catch {}
    await sleep(500);
  }
  throw new Error(`CDP endpoint on port ${CDP_PORT} never became reachable`);
}

// ── Main verification ───────────────────────────────────────────────────────
const report = {
  appUrl: APP_URL,
  cdpPort: CDP_PORT,
  startedAt: new Date().toISOString(),
  consoleErrors: [],
  exceptions: [],
  networkFailures: [],
  httpErrors: [],
  routes: {},
  screenshots: [],
};

const wsUrl = await waitForTarget();
const cdp = new CdpClient(wsUrl);
await cdp.open();

cdp.on('Runtime.consoleAPICalled', (p) => {
  const type = p.type || 'log';
  const text = (p.args || [])
    .map((a) => (a.value === undefined ? a.description || '' : String(a.value)))
    .join(' ');
  if (type === 'error' || type === 'assert') report.consoleErrors.push(text);
});
cdp.on('Runtime.exceptionThrown', (p) => {
  const d = p.exceptionDetails || {};
  report.exceptions.push(
    (d.exception && (d.exception.description || d.exception.value)) ||
      d.text ||
      'unknown exception',
  );
});
cdp.on('Log.entryAdded', (p) => {
  if (p.entry && p.entry.level === 'error') {
    report.consoleErrors.push(`[log] ${p.entry.text}`);
  }
});
cdp.on('Network.loadingFailed', (p) => {
  if (p.errorText && p.errorText !== 'net::ERR_ABORTED') {
    report.networkFailures.push(p.errorText);
  }
});
cdp.on('Network.responseReceived', (p) => {
  if (p.response && p.response.status >= 400) {
    report.httpErrors.push({
      status: p.response.status,
      url: p.response.url,
    });
  }
});

await cdp.send('Page.enable');
await cdp.send('Runtime.enable');
await cdp.send('Log.enable');
await cdp.send('Network.enable');
await cdp.send(
  'Page.addScriptToEvaluateOnNewDocument',
  {
    source: `
      window.__bookifyErrors = [];
      window.addEventListener('error', (e) => window.__bookifyErrors.push(String(e.message)));
      window.addEventListener('unhandledrejection', (e) => window.__bookifyErrors.push('unhandledrejection: ' + String(e.reason)));
    `,
  },
);

async function evaluate(expression) {
  const r = await cdp.send('Runtime.evaluate', {
    expression,
    returnByValue: true,
    awaitPromise: true,
  });
  return r.result ? r.result.value : undefined;
}

async function waitForFlutter(timeoutMs = 120000) {
  // Poll until the Flutter engine has attached a view AND rendered a frame
  // (a canvas element appears once the first frame is drawn).
  const start = Date.now();
  let last = null;
  while (Date.now() - start < timeoutMs) {
    last = await evaluate(`({
      ready: document.readyState,
      title: document.title,
      url: location.href,
      flutterView: !!document.querySelector('flutter-view'),
      glassPane: !!document.querySelector('flt-glass-pane'),
      canvasCount: document.querySelectorAll('canvas').length,
      bodyHtmlLen: document.body ? document.body.innerHTML.length : 0,
      errors: window.__bookifyErrors || [],
    })`);
    if (last.flutterView && (last.canvasCount > 0 || last.glassPane)) {
      return last;
    }
    await sleep(3000);
  }
  return last;
}

async function visit(route) {
  await cdp.send('Page.navigate', { url: `${APP_URL}${route}` });
  const state = await waitForFlutter(60000);
  report.routes[route] = state;
  // Wait a little longer for the app to settle, then capture a screenshot.
  await sleep(8000);
  await screenshot(`route-${route.replace(/[/]/g, '_') || 'home'}`);
}

async function screenshot(label) {
  const shot = await cdp.send('Page.captureScreenshot', { format: 'png' });
  const file = resolve(SHOT_DIR, `${label}.png`);
  writeFileSync(file, Buffer.from(shot.data, 'base64'));
  report.screenshots.push(file);
}

// Reload so startup happens while error listeners are active.
await cdp.send('Page.reload', { ignoreCache: true });
report.initial = await waitForFlutter(150000);
await sleep(5000);
await screenshot('01-desktop');

// Responsive widths (device emulation does not need a window resize)
const widths = [
  ['02-tablet-768', 768, 1024],
  ['03-mobile-390', 390, 844],
];
for (const [label, w, h] of widths) {
  await cdp.send('Emulation.setDeviceMetricsOverride', {
    width: w,
    height: h,
    deviceScaleFactor: 1,
    mobile: label.includes('mobile'),
  });
  await sleep(4000);
  await screenshot(label);
}
await cdp.send('Emulation.clearDeviceMetricsOverride');
await sleep(1000);

// Key routes
for (const route of ['/login', '/register', '/', '/categories', '/help']) {
  await visit(route);
}

report.pageErrors = await evaluate('window.__bookifyErrors || []');
report.finishedAt = new Date().toISOString();

console.log(JSON.stringify(report, null, 2));
cdp.close();
process.exit(0);
