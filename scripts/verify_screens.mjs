// In-app route verification in the running Edge instance. Navigates via the
// history API (go_router listens to popstate), captures console errors and
// dumps the widget tree per route to confirm each screen renders.
// Usage: CDP_PORT=<port> node scripts/verify_screens.mjs
import { readFileSync, writeFileSync } from 'node:fs';
import { resolve } from 'node:path';

const CDP_PORT = Number(process.env.CDP_PORT || 54976);
const RUN_LOG = resolve('flutter_edge_run.log');
const ROUTES = [
  '/login', '/register', '/', '/categories', '/search',
  '/help', '/favorites', '/profile', '/settings', '/appointments', '/chat',
];

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

// VM service URL from the flutter run log (stable across app reloads).
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
const consoleErrors = [];
ws.onmessage = (ev) => {
  const m = JSON.parse(ev.data);
  if (m.id && pending.has(m.id)) {
    pending.get(m.id)(m.result);
    pending.delete(m.id);
  } else if (m.method === 'Runtime.consoleAPICalled' && ['error', 'assert'].includes(m.params.type)) {
    consoleErrors.push(m.params.args.map((a) => a.value ?? a.description ?? '').join(' '));
  } else if (m.method === 'Runtime.exceptionThrown') {
    const d = m.params.exceptionDetails || {};
    consoleErrors.push('EXCEPTION: ' + ((d.exception && (d.exception.description || d.exception.value)) || d.text || ''));
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

// VM service helper.
async function dumpScreens() {
  const vmUrl = findVmUrl();
  if (!vmUrl) return [];
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
  const dump = await vsend('ext.flutter.debugDumpApp', { isolateId });
  vws.close();
  const text = (dump && (dump.data || dump.result)) || '';
  const names = [
    'SplashScreen', 'OnboardingScreen', 'LoginScreen', 'RegisterScreen',
    'HomeScreen', 'HelpCenterScreen', 'CategoriesScreen', 'SearchScreen',
    'ProfileScreen', 'SettingsScreen', 'FavoritesScreen', 'ChatScreen',
    'AppointmentsScreen', 'GoogleSignInButton', 'ContactSupportScreen',
  ];
  return names.filter((s) => text.includes(s));
}

const report = { routes: {} };
for (const route of ROUTES) {
  await evalJs(`(() => {
    history.pushState({}, '', ${JSON.stringify(route)});
    window.dispatchEvent(new PopStateEvent('popstate', { state: null }));
    return true;
  })()`);
  await sleep(5000);
  const url = await evalJs('location.href');
  const screens = await dumpScreens();
  report.routes[route] = { url, screens };
  console.log(JSON.stringify({ route, screens }));
}
report.consoleErrors = consoleErrors;
writeFileSync(resolve('scripts', 'edge_shots', 'verify_screens.json'), JSON.stringify(report, null, 2));
console.log('CONSOLE_ERRORS:', JSON.stringify(consoleErrors));
process.exit(0);
