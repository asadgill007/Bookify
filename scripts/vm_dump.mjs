// Connects to the Dart VM Service of the running Flutter web app and dumps
// the widget tree so the currently rendered screen can be identified.
// Usage: node scripts/vm_dump.mjs <vmServiceWsUrl>
import { resolve } from 'node:path';
import { writeFileSync } from 'node:fs';

const VM_URL = process.argv[2];
if (!VM_URL) {
  console.error('usage: node scripts/vm_dump.mjs <ws://127.0.0.1:PORT/TOKEN=/ws>');
  process.exit(1);
}

const ws = new WebSocket(VM_URL);
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
    ws.send(JSON.stringify({ jsonrpc: '2.0', id: i, method, params }));
  });

await new Promise((r) => (ws.onopen = r));

const vm = await send('getVM');
const isolates = vm.isolates || [];
console.log('isolates:', isolates.map((i) => i.name).join(', '));
if (!isolates.length) {
  console.log('NO ISOLATES');
  process.exit(0);
}
const isolateId = isolates[0].id;

const dump = await send('ext.flutter.debugDumpApp', { isolateId }).catch(
  (e) => ({ error: String(e) }),
);
const text =
  typeof dump === 'string' ? dump : (dump && (dump.data || dump.result)) || JSON.stringify(dump);
writeFileSync(resolve('scripts', 'edge_shots', 'widget-tree.txt'), text);
console.log('DUMP_LEN:', text.length);

// Find which screen widgets are present in the tree.
const screens = [
  'SplashScreen',
  'OnboardingScreen',
  'LoginScreen',
  'RegisterScreen',
  'HomeScreen',
  'HelpCenterScreen',
  'CategoriesScreen',
  'SearchScreen',
  'ProfileScreen',
  'SettingsScreen',
  'BusinessDetailScreen',
  'FavoritesScreen',
  'ChatScreen',
  'AppointmentsScreen',
];
const found = screens.filter((s) => text.includes(s));
console.log('SCREENS_FOUND:', JSON.stringify(found));
console.log('TREE_TAIL:', text.slice(-600));
process.exit(0);
