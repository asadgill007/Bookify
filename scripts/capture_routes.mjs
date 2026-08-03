// Per-route network + console capture in the running Edge instance.
// Usage: CDP_PORT=<port> node scripts/capture_routes.mjs [appUrl]
const APP_URL = process.argv[2] || 'http://localhost:8090';
const CDP_PORT = Number(process.env.CDP_PORT || 54976);
const ROUTES = process.argv[3] ? process.argv[3].split(',') : ['/login', '/register', '/', '/categories', '/help', '/search'];

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

const targets = await (await fetch(`http://127.0.0.1:${CDP_PORT}/json`)).json();
const page = targets.find((t) => t.type === 'page' && t.url.includes('localhost'));
const ws = new WebSocket(page.webSocketDebuggerUrl);
let id = 0;
const pending = new Map();
let state = { route: '?', responses: [], consoleErrors: [], exceptions: [], networkFailures: [] };
const perRoute = {};

ws.onmessage = (ev) => {
  const m = JSON.parse(ev.data);
  if (m.id && pending.has(m.id)) {
    pending.get(m.id)(m.result);
    pending.delete(m.id);
  } else if (m.method === 'Network.responseReceived') {
    const r = m.params.response;
    if (/localhost:(8090|5136)/.test(r.url) && r.status >= 400) {
      state.responses.push({ status: r.status, url: r.url });
    }
  } else if (m.method === 'Runtime.consoleAPICalled' && ['error', 'assert'].includes(m.params.type)) {
    state.consoleErrors.push(m.params.args.map((a) => a.value ?? a.description ?? '').join(' '));
  } else if (m.method === 'Runtime.exceptionThrown') {
    const d = m.params.exceptionDetails || {};
    state.exceptions.push((d.exception && (d.exception.description || d.exception.value)) || d.text || 'ex');
  } else if (m.method === 'Network.loadingFailed') {
    if (m.params.errorText && m.params.errorText !== 'net::ERR_ABORTED') {
      state.networkFailures.push(m.params.errorText);
    }
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
await send('Network.enable');
await send('Log.enable');
await send('Page.enable');

for (const route of ROUTES) {
  state = { route, responses: [], consoleErrors: [], exceptions: [], networkFailures: [] };
  await send('Page.navigate', { url: `${APP_URL}${route}` });
  await sleep(25000);
  perRoute[route] = state;
}

console.log(JSON.stringify(perRoute, null, 2));
process.exit(0);
