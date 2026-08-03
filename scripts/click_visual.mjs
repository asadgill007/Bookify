// Visual click-through: decodes screenshot pixels (via zlib PNG inflate) to
// detect onboarding page changes by average color, advances to the login
// screen, and verifies LoginScreen + GoogleSignInButton in the widget tree.
// Usage: CDP_PORT=<port> node scripts/click_visual.mjs [appUrl]
import { readFileSync, writeFileSync } from 'node:fs';
import { inflateSync } from 'node:zlib';
import { resolve } from 'node:path';

const APP_URL = process.argv[2] || 'http://localhost:8090';
const CDP_PORT = Number(process.env.CDP_PORT || 54976);
const RUN_LOG = resolve('flutter_edge_run.log');
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

function findVmUrl() {
  const log = readFileSync(RUN_LOG, 'utf8');
  const m = log.match(/ws:\/\/127\.0\.0\.1:\d+\/[A-Za-z0-9]+=\/ws/g);
  return m ? m[m.length - 1] : null;
}

// PNG average color (RGB) — parses the first IDAT, inflates, averages pixels.
function pngAverage(buffer) {
  let idat = Buffer.alloc(0);
  let w = 0;
  let h = 0;
  let bitDepth = 8;
  let colorType = 2;
  let pos = 8;
  while (pos < buffer.length) {
    const len = buffer.readUInt32BE(pos);
    const type = buffer.toString('ascii', pos + 4, pos + 8);
    const data = buffer.subarray(pos + 8, pos + 8 + len);
    if (type === 'IHDR') {
      w = data.readUInt32BE(0);
      h = data.readUInt32BE(4);
      bitDepth = data[8];
      colorType = data[9];
    } else if (type === 'IDAT') {
      idat = Buffer.concat([idat, data]);
    }
    pos += 12 + len;
  }
  const raw = inflateSync(idat);
  const channels = colorType === 6 ? 4 : colorType === 2 ? 3 : colorType === 0 ? 1 : 3;
  const stride = w * channels + 1;
  let r = 0;
  let g = 0;
  let b = 0;
  let n = 0;
  for (let y = 0; y < h && y < 300; y += 4) {
    const rowStart = y * stride;
    for (let x = 1; x < w; x += 4) {
      const p = rowStart + x * channels;
      r += raw[p];
      g += raw[p + 1];
      b += raw[p + 2];
      n++;
    }
  }
  return { avg: [Math.round(r / n), Math.round(g / n), Math.round(b / n)], w, h };
}

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

async function screenshot() {
  const shot = await send('Page.captureScreenshot', { format: 'png' });
  return Buffer.from(shot.data, 'base64');
}

async function clickAt(x, y) {
  await send('Input.dispatchMouseEvent', { type: 'mouseMoved', x, y });
  await send('Input.dispatchMouseEvent', { type: 'mousePressed', x, y, button: 'left', clickCount: 1 });
  await send('Input.dispatchMouseEvent', { type: 'mouseReleased', x, y, button: 'left', clickCount: 1 });
}

async function dumpTree() {
  try {
    const vws = new WebSocket(findVmUrl());
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
    let isolateId = null;
    for (let i = 0; i < 15 && !isolateId; i++) {
      try {
        const vm = await vsend('getVM');
        if (vm.isolates && vm.isolates.length) isolateId = vm.isolates[0].id;
      } catch {}
      if (!isolateId) await sleep(1500);
    }
    if (!isolateId) { vws.close(); return ''; }
    const dump = await vsend('ext.flutter.debugDumpApp', { isolateId }).catch(() => null);
    vws.close();
    return (dump && (dump.data || dump.result)) || '';
  } catch {
    return '';
  }
}

await send('Page.navigate', { url: APP_URL });
for (let i = 0; i < 30; i++) {
  if ((await dumpTree()).includes('OnboardingScreen')) break;
  await sleep(2000);
}
const vp = await evalJs('({ w: window.innerWidth, h: window.innerHeight })');
const W = vp.w;
const H = vp.h;
const cx = Math.round(W / 2);
const nextY = H - 24 - 18 - 32 - 28;
console.log('VIEWPORT:', JSON.stringify(vp), 'nextY:', nextY);

let prevAvg = null;
for (let i = 0; i < 3; i++) {
  const before = pngAverage(await screenshot());
  await clickAt(cx, nextY);
  await sleep(2500);
  const after = pngAverage(await screenshot());
  const changed = prevAvg === null || Math.abs(after.avg[0] - before.avg[0]) + Math.abs(after.avg[1] - before.avg[1]) + Math.abs(after.avg[2] - before.avg[2]) > 8;
  console.log(`click ${i + 1}: avg before=${before.avg} after=${after.avg} changed=${changed}`);
  prevAvg = after.avg;
}

// Click "Sign In" on the last page.
const signInX = Math.round(cx + 90);
const signInY = H - 24 - 24;
const shotBefore = pngAverage(await screenshot());
await clickAt(signInX, signInY);
await sleep(5000);
const shotAfter = pngAverage(await screenshot());
console.log('signin click: avg before=', shotBefore.avg, 'after=', shotAfter.avg);

const tree = await dumpTree();
console.log('LOGIN_SCREEN:', tree.includes('LoginScreen'));
console.log('GOOGLE_BUTTON:', tree.includes('GoogleSignInButton'));
console.log('ONBOARDING:', tree.includes('OnboardingScreen'));
const finalShot = await screenshot();
writeFileSync(resolve('scripts', 'edge_shots', 'after-signin.png'), finalShot);
console.log('screenshot saved');
process.exit(0);
