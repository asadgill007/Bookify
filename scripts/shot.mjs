// Capture screenshot from the live Edge instance.
// Usage: CDP_PORT=<port> node scripts/shot.mjs [outfile]
import { writeFileSync } from 'node:fs';
const CDP_PORT = Number(process.env.CDP_PORT || 51863);
const OUT = process.argv[2] || 'scripts/edge_shots/current.png';

const targets = await (await fetch(`http://127.0.0.1:${CDP_PORT}/json`)).json();
const page = targets.find((t) => t.type === 'page' && t.url.includes('localhost'));
const ws = new WebSocket(page.webSocketDebuggerUrl);
let id = 0;
const pending = new Map();
ws.onmessage = (ev) => {
  const m = JSON.parse(ev.data);
  if (m.id && pending.has(m.id)) { pending.get(m.id)(m.result); pending.delete(m.id); }
};
const send = (method, params = {}) => new Promise((res) => {
  const i = ++id; pending.set(i, res); ws.send(JSON.stringify({ id: i, method, params }));
});
await new Promise((r) => (ws.onopen = r));
await send('Page.enable');
await new Promise((r) => setTimeout(r, 800));
const shot = await send('Page.captureScreenshot', { format: 'png' });
if (shot && shot.data) {
  writeFileSync(OUT, Buffer.from(shot.data, 'base64'));
  console.log('SAVED:', OUT);
} else {
  console.log('CAPTURE_FAILED', JSON.stringify(shot).slice(0, 200));
}
process.exit(0);
