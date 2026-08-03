// Read-only CDP probe: attaches to the running Edge instance and reports the
// current Flutter web app state in detail, plus a screenshot. Does NOT reload
// or navigate (so the debug session is not disturbed).
//
// Usage: CDP_PORT=<port> node scripts/edge_probe.mjs [appUrl]
import { mkdirSync, writeFileSync } from 'node:fs';
import { resolve } from 'node:path';

const CDP_PORT = Number(process.env.CDP_PORT || 9222);
const APP_URL = process.argv[2] || 'http://localhost:8090';

class CdpClient {
  constructor(url) {
    this.ws = new WebSocket(url);
    this.id = 0;
    this.pending = new Map();
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
  close() {
    try {
      this.ws.close();
    } catch {}
  }
}

async function main() {
  const res = await fetch(`http://127.0.0.1:${CDP_PORT}/json`);
  const targets = await res.json();
  const page = targets.find(
    (t) => t.type === 'page' && t.url.includes('localhost'),
  );
  if (!page) throw new Error('no page target found');
  console.log('target:', page.url);

  const cdp = new CdpClient(page.webSocketDebuggerUrl);
  await cdp.open();
  await cdp.send('Runtime.enable');
  await cdp.send('Page.enable');

  const expr = `({
    href: location.href,
    ready: document.readyState,
    title: document.title,
    flutterView: !!document.querySelector('flutter-view'),
    glassPane: !!document.querySelector('flt-glass-pane'),
    canvasCount: document.querySelectorAll('canvas').length,
    scriptCount: document.querySelectorAll('script').length,
    bodyHtmlLen: document.body ? document.body.innerHTML.length : 0,
    hasFlutter: typeof window._flutter !== 'undefined',
    hasBuildConfig: typeof window._flutter !== 'undefined' && !!window._flutter.buildConfig,
    buildConfig: typeof window._flutter !== 'undefined' && window._flutter.buildConfig ? JSON.stringify(window._flutter.buildConfig).slice(0, 400) : null,
    canvaskitState: window.flutterCanvasKitLoaded ? 'resolved' : (window.flutterCanvasKit ? 'loaded' : 'pending'),
    pageErrors: window.__bookifyErrors || [],
    resources: performance.getEntriesByType('resource').map(r => r.name.replace('http://localhost:8090', '')).slice(0, 40),
  })`;

  const r = await cdp.send('Runtime.evaluate', {
    expression: expr,
    returnByValue: true,
  });
  console.log('STATE:', JSON.stringify(r.result.value, null, 2));

  const shot = await cdp.send('Page.captureScreenshot', { format: 'png' });
  const file = resolve('scripts', 'edge_shots', 'probe-current.png');
  writeFileSync(file, Buffer.from(shot.data, 'base64'));
  console.log('SCREENSHOT:', file);

  cdp.close();
}

main().catch((e) => {
  console.error('probe failed:', e.message);
  process.exit(1);
});
