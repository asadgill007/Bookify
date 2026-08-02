// E2E test: Multi-language preference persistence (i18n).
// Verifies the language selector's backend persistence: set language → persists → survives re-fetch.
// Run: node scripts/e2e_i18n_test.mjs  (backend must be running on :5136)
const BASE = 'http://localhost:5136/api/v1';

let passed = 0;
let failed = 0;
const results = [];

function check(name, cond, detail = '') {
  if (cond) {
    passed++;
    results.push(`  ✅ ${name}${detail ? ` — ${detail}` : ''}`);
  } else {
    failed++;
    results.push(`  ❌ ${name}${detail ? ` — ${detail}` : ''}`);
  }
}

async function api(method, path, { token, body } = {}) {
  let attempt = 0;
  while (true) {
    const headers = { 'Content-Type': 'application/json' };
    if (token) headers['Authorization'] = `Bearer ${token}`;
    const res = await fetch(`${BASE}${path}`, {
      method,
      headers,
      body: body ? JSON.stringify(body) : undefined,
    });
    if (res.status !== 429 || attempt >= 15) {
      const text = await res.text();
      let json = null;
      try { json = JSON.parse(text); } catch { /* not json */ }
      return { status: res.status, json, text };
    }
    attempt++;
    await new Promise((r) => setTimeout(r, 5000));
  }
}

const stamp = Date.now();
const customerEmail = `i18n.customer.${stamp}@bookify.test`;

console.log('═══ E2E: Multi-language preference persistence ═══\n');

try {
  // ── Setup: register a customer ──
  console.log('── Setup: register customer ──');
  let r = await api('POST', '/auth/register', { body: {
    firstName: 'I18N', lastName: 'Customer', email: customerEmail,
    password: 'E2ePass123!', confirmPassword: 'E2ePass123!', accountType: 'customer'
  }});
  check('Register customer → 201/200', [200, 201].includes(r.status), `HTTP ${r.status}`);
  const customerToken = r.json?.data?.accessToken;
  check('Customer token issued', !!customerToken);

  // ── Part 1: Default language is English ──
  console.log('\n── Part 1: Default language ──');
  r = await api('GET', '/settings/preferences', { token: customerToken });
  check('GET /settings/preferences → 200', r.status === 200, `HTTP ${r.status}`);
  const defaultLang = r.json?.data?.language;
  check('Default language is "en"', defaultLang === 'en', `language=${defaultLang}`);

  // ── Part 2: Set language to Urdu ──
  console.log('\n── Part 2: Set language to Urdu (ur) ──');
  r = await api('PUT', '/settings/preferences', { token: customerToken, body: {
    language: 'ur', currency: 'USD', isDarkMode: false, isAmoledMode: false,
    notificationsEnabled: true, marketingEmails: false
  }});
  check('PUT /settings/preferences (ur) → 200', r.status === 200, `HTTP ${r.status} ${r.text.slice(0, 120)}`);

  // ── Part 3: Language persists after re-fetch (simulates app restart) ──
  console.log('\n── Part 3: Language persists (re-fetch = app restart) ──');
  r = await api('GET', '/settings/preferences', { token: customerToken });
  const persistedLang = r.json?.data?.language;
  check('Language persisted as "ur" after re-fetch', persistedLang === 'ur', `language=${persistedLang}`);

  // ── Part 4: Switch back to English ──
  console.log('\n── Part 4: Switch back to English (en) ──');
  r = await api('PUT', '/settings/preferences', { token: customerToken, body: {
    language: 'en', currency: 'USD', isDarkMode: false, isAmoledMode: false,
    notificationsEnabled: true, marketingEmails: false
  }});
  check('PUT /settings/preferences (en) → 200', r.status === 200, `HTTP ${r.status}`);

  r = await api('GET', '/settings/preferences', { token: customerToken });
  const finalLang = r.json?.data?.language;
  check('Language switched back to "en"', finalLang === 'en', `language=${finalLang}`);

  // ── Part 5: Unauthenticated access rejected ──
  console.log('\n── Part 5: Auth enforcement ──');
  r = await api('GET', '/settings/preferences');
  check('Unauthenticated GET /settings/preferences → 401', r.status === 401, `HTTP ${r.status}`);
  r = await api('PUT', '/settings/preferences', { body: { language: 'ur' } });
  check('Unauthenticated PUT /settings/preferences → 401', r.status === 401, `HTTP ${r.status}`);
} catch (e) {
  failed++;
  results.push(`  ❌ Unhandled exception: ${e.message}`);
}

console.log('\n═══ RESULTS ═══');
results.forEach(x => console.log(x));
console.log(`\n${passed} passed, ${failed} failed`);
process.exit(failed > 0 ? 1 : 0);