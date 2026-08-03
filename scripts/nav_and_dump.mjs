// Navigates the running app in-app (history.pushState + popstate, no reload)
// then dumps the widget tree to identify the active screen.
// Usage: CDP_PORT=<port> VM_URL=<vmWsUrl> node scripts/nav_and_dump.mjs [route]
import { writeFileSync } from 'node:fs';
import { resolve } from 'node:path';

const ROUTE = process.argv[2] || '/login';
const CDP_PORT = Number(process.env.CDP_PORT || 54976);
const VM_URL = process.env.VM_URL;
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

// In-app navigation via history API + popstate (go_router listens to popstate).
const navResult = await evalJs(`(() => {
  try {
    history.pushState({}, '', '${ROUTE}');
    window.dispatchEvent(new PopStateEvent('popstate', { state: null }));
    return 'dispatched';
  } catch (e) {
    return 'error: ' + String(e);
  }
})()`);
console.log('NAV:', navResult);
await sleep(6000);
console.log('URL_NOW:', await evalJs('location.href'));

// Dump widget tree via the Dart VM Service.
let screens = [];
if (VM_URL) {
  const vws = new WebSocket(VM_URL);
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
  const text = (dump && (dump.data || dump.result)) || '';
  writeFileSync(resolve('scripts', 'edge_shots', 'widget-tree.txt'), text);
  const names = [
    'SplashScreen', 'OnboardingScreen', 'LoginScreen', 'RegisterScreen',
    'HomeScreen', 'HelpCenterScreen', 'CategoriesScreen', 'SearchScreen',
    'ProfileScreen', 'SettingsScreen', 'FavoritesScreen', 'ChatScreen',
    'AppointmentsScreen', 'GoogleSignInButton',
  ];
  screens = names.filter((s) => text.includes(s));
  vws.close();
}
console.log('SCREENS_FOUND:', JSON.stringify(screens));
process.exit(0);
