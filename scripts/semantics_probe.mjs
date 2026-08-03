// Enables Flutter web semantics and dumps the semantic DOM so screens and
// buttons (e.g. "Sign in with Google") can be located for interaction.
// Usage: CDP_PORT=<port> node scripts/semantics_probe.mjs [appUrl] [route]
const APP_URL = process.argv[2] || 'http://localhost:8090';
const ROUTE = process.argv[3] || '/login';
const CDP_PORT = Number(process.env.CDP_PORT || 54976);

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

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
await send('Page.enable');

const evalJs = async (expression) => {
  const r = await send('Runtime.evaluate', { expression, returnByValue: true, awaitPromise: true });
  return r.result ? r.result.value : undefined;
};

// Navigate to the route and wait for render.
await send('Page.navigate', { url: `${APP_URL}${ROUTE}` });
await sleep(20000);

// Enable semantics by clicking the placeholder element Flutter inserts.
const enabled = await evalJs(`(async () => {
  const el = document.querySelector('flt-semantics-placeholder');
  if (!el) return 'no-placeholder';
  el.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true }));
  el.click();
  return 'clicked';
})()`);
console.log('SEMANTICS_ENABLE:', enabled);
await sleep(6000);

// Dump semantic elements: text, aria-label, role, tag.
const dump = await evalJs(`(() => {
  const out = [];
  const seen = new Set();
  const walk = (root) => {
    for (const el of root.querySelectorAll('flt-semantics, [aria-label], [role], flt-semantics-node')) {
      const label = el.getAttribute('aria-label') || el.getAttribute('label') || '';
      const text = (el.textContent || '').trim().slice(0, 80);
      const role = el.getAttribute('role') || el.tagName;
      const key = label + '|' + text + '|' + role;
      if (!seen.has(key)) {
        seen.add(key);
        if (label || text) out.push({ tag: el.tagName, role, label: label.slice(0, 60), text });
      }
    }
  };
  walk(document);
  return { count: out.length, items: out.slice(0, 120) };
})()`);
console.log('SEMANTICS:', JSON.stringify(dump, null, 1));
process.exit(0);
