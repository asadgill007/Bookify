// Dumps the DOM structure of the running Flutter web app (top-level elements
// and all custom "flt-*" tags) to find interaction/semantics hooks.
// Usage: CDP_PORT=<port> node scripts/dom_probe.mjs [appUrl]
const APP_URL = process.argv[2] || 'http://localhost:8090';
const CDP_PORT = Number(process.env.CDP_PORT || 54976);

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

const info = await evalJs(`(() => {
  const all = document.querySelectorAll('*');
  const tags = {};
  all.forEach((el) => {
    const t = el.tagName.toLowerCase();
    tags[t] = (tags[t] || 0) + 1;
  });
  const fltTags = Object.keys(tags).filter((t) => t.startsWith('flt'));
  const top = Array.from(document.body.children).map((c) => ({
    tag: c.tagName.toLowerCase(),
    id: c.id,
    cls: (c.className || '').toString().slice(0, 60),
    html: c.outerHTML.slice(0, 180),
  }));
  return { allTags: Object.entries(tags).slice(0, 30), fltTags, top };
})()`);
console.log(JSON.stringify(info, null, 1));
process.exit(0);
