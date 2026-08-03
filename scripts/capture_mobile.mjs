// Identifies 404s that occur under mobile-device emulation (mobile UA).
// Usage: CDP_PORT=<port> node scripts/capture_mobile.mjs [appUrl]
const APP_URL = process.argv[2] || 'http://localhost:8090';
const CDP_PORT = Number(process.env.CDP_PORT || 54976);

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

const targets = await (await fetch(`http://127.0.0.1:${CDP_PORT}/json`)).json();
const page = targets.find((t) => t.type === 'page' && t.url.includes('localhost'));
const ws = new WebSocket(page.webSocketDebuggerUrl);
let id = 0;
const pending = new Map();
const responses = [];
const consoleErrors = [];

ws.onmessage = (ev) => {
  const m = JSON.parse(ev.data);
  if (m.id && pending.has(m.id)) {
    pending.get(m.id)(m.result);
    pending.delete(m.id);
  } else if (m.method === 'Network.responseReceived') {
    const r = m.params.response;
    if (/localhost:(8090|5136)/.test(r.url)) {
      responses.push({ status: r.status, url: r.url.replace('http://localhost:5136', '[API]').replace('http://localhost:8090', '[WEB]') });
    }
  } else if (m.method === 'Runtime.consoleAPICalled' && ['error', 'assert'].includes(m.params.type)) {
    consoleErrors.push(m.params.args.map((a) => a.value ?? a.description ?? '').join(' '));
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
await send('Emulation.setDeviceMetricsOverride', {
  width: 390,
  height: 844,
  deviceScaleFactor: 3,
  mobile: true,
});
await send('Emulation.setUserAgentOverride', {
  userAgent:
    'Mozilla/5.0 (Linux; Android 13; Pixel 7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36 Edg/150.0.0.0',
});
await send('Page.reload', { ignoreCache: true });
await sleep(35000);

const nonOk = responses.filter((r) => r.status >= 400);
console.log('NON_OK_COUNT:', nonOk.length);
console.log('NON_OK:', JSON.stringify(nonOk, null, 1));
console.log('CONSOLE_ERRORS:', JSON.stringify(consoleErrors, null, 1));
await send('Emulation.clearDeviceMetricsOverride');
process.exit(0);
