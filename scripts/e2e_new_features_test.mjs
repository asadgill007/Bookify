// E2E test: Support, Auto-Verification, AI Chat, Favorites, Currency, Google Auth, Filters.
// Run: node scripts/e2e_new_features_test.mjs  (backend must be running on :5136)
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
  // Retry on HTTP 429 (API rate limiter) with backoff so the suite stays green
  // when several scripts run back-to-back. The limiter window is 1 minute.
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

function itemsOf(r) {
  const d = r?.json?.data;
  if (Array.isArray(d)) return d;
  if (d && Array.isArray(d.items)) return d.items;
  return [];
}

// GET /currencies returns { data: { baseCurrency, fetchedAt, rates: [...] } }
function ratesOf(r) {
  const d = r?.json?.data;
  if (Array.isArray(d)) return d;
  if (d && Array.isArray(d.rates)) return d.rates;
  return [];
}

const stamp = Date.now();
const ownerEmail = `e2e2.owner.${stamp}@bookify.test`;
const customerEmail = `e2e2.customer.${stamp}@bookify.test`;
const bizName = `E2E New Features Spa ${stamp}`;

console.log('═══ E2E: Support, Auto-Verify, Chat, Favorites, Currency, Google, Filters ═══\n');

try {
  // ── Setup: owner + business (incomplete at first) ──
  console.log('── Setup: register owner ──');
  let r = await api('POST', '/auth/register', { body: {
    firstName: 'E2E2', lastName: 'Owner', email: ownerEmail,
    password: 'E2ePass123!', confirmPassword: 'E2ePass123!', accountType: 'businessOwner'
  }});
  check('Register business owner → 201/200', [200, 201].includes(r.status), `HTTP ${r.status}`);
  const ownerToken = r.json?.data?.accessToken;
  check('Owner token issued', !!ownerToken);

  r = await api('GET', '/categories');
  const catId = r.json?.data?.[0]?.id;
  check('Categories fetched', !!catId);

  r = await api('POST', '/businesses', { token: ownerToken, body: {
    name: bizName, addressLine1: '123 Test Ave', city: 'Testville', postalCode: '12345',
    country: 'US', timeZone: 'UTC', currency: 'USD', description: 'E2E new features test',
    email: ownerEmail, categoryIds: catId ? [catId] : [],
    latitude: 40.7128, longitude: -74.006
  }});
  check('Create business → 201/200', [200, 201].includes(r.status), `HTTP ${r.status}`);
  const businessId = r.json?.data?.id;
  check('Business ID returned', !!businessId, `id=${businessId}`);

  // ── Part A: AUTO-VERIFICATION CHECKLIST ──
  console.log('\n── Part A: Auto-Verification ──');

  // Incomplete: check /businesses/mine shows Pending + checklist missing items.
  r = await api('GET', '/businesses/mine', { token: ownerToken });
  check('GET /businesses/mine → 200', r.status === 200, `HTTP ${r.status}`);
  const mineIncomplete = itemsOf(r).find(x => x.id === businessId);
  check('Business listed in mine', !!mineIncomplete);
  check('Business starts Pending', mineIncomplete?.verificationStatus === 'Pending',
    `status=${mineIncomplete?.verificationStatus}`);
  check('Checklist is NOT complete initially', mineIncomplete?.isChecklistComplete === false,
    `isChecklistComplete=${mineIncomplete?.isChecklistComplete}`);
  const missing = mineIncomplete?.checklist?.filter(x => !x.isComplete) ?? [];
  check('Checklist has missing items', missing.length >= 2, `${missing.length} missing: ${missing.map(m => m.key).join(', ')}`);

  // Complete the checklist: hours → service → provider → image.
  const hours = Array.from({ length: 7 }, (_, i) => ({
    dayOfWeek: (i + 1) % 7, openTime: '09:00', closeTime: '17:00', isClosed: false
  }));
  r = await api('PUT', `/businesses/${businessId}/hours`, { token: ownerToken, body: { hours } });
  check('Set hours → 200', r.status === 200, `HTTP ${r.status}`);

  r = await api('POST', `/businesses/${businessId}/services`, { token: ownerToken, body: {
    name: 'E2E2 Massage', durationMinutes: 60, priceAmount: 90, currency: 'USD'
  }});
  check('Add service → 201/200', [200, 201].includes(r.status), `HTTP ${r.status}`);
  const serviceId = r.json?.data?.serviceId;
  check('Service ID returned', !!serviceId, `serviceId=${serviceId}`);

  r = await api('POST', `/businesses/${businessId}/providers`, { token: ownerToken, body: {
    firstName: 'E2E2', lastName: 'Staff', email: `e2e2.staff.${stamp}@bookify.test`,
    title: 'Therapist', bio: 'E2E test provider'
  }});
  check('Add provider → 201/200', [200, 201].includes(r.status), `HTTP ${r.status}`);
  const providerId = r.json?.data?.providerId;
  check('Provider ID returned', !!providerId, `providerId=${providerId}`);

  // Still Pending without an image (image is a hard requirement).
  r = await api('GET', '/businesses/mine', { token: ownerToken });
  const mineNoImage = itemsOf(r).find(x => x.id === businessId);
  check('Still Pending without image', mineNoImage?.verificationStatus === 'Pending',
    `status=${mineNoImage?.verificationStatus}`);
  const imageItem = mineNoImage?.checklist?.find(x => x.key.includes('image'));
  check('Image checklist item present and incomplete', imageItem && !imageItem.isComplete,
    `key=${imageItem?.key} complete=${imageItem?.isComplete}`);

  // Add the image → should auto-verify (Pending → Approved).
  r = await api('POST', `/businesses/${businessId}/images`, { token: ownerToken, body: {
    imageUrls: ['https://example.com/spa-cover.jpg']
  }});
  check('Add image → 200', r.status === 200, `HTTP ${r.status} ${r.text.slice(0, 120)}`);

  r = await api('GET', '/businesses/mine', { token: ownerToken });
  const mineVerified = itemsOf(r).find(x => x.id === businessId);
  check('Business AUTO-VERIFIED after checklist complete', mineVerified?.verificationStatus === 'Approved',
    `status=${mineVerified?.verificationStatus}`);
  check('Checklist now complete', mineVerified?.isChecklistComplete === true,
    `isChecklistComplete=${mineVerified?.isChecklistComplete}`);
  const stillMissing = mineVerified?.checklist?.filter(x => !x.isComplete) ?? [];
  check('No checklist items missing', stillMissing.length === 0,
    stillMissing.length ? `missing: ${stillMissing.map(m => m.key).join(', ')}` : 'all complete');

  // Business now appears in public search (verified-only).
  r = await api('GET', `/businesses?search=${encodeURIComponent(bizName)}&page=1&pageSize=20`);
  check('Public search → 200', r.status === 200, `HTTP ${r.status}`);
  const found = itemsOf(r).some(x => x.id === businessId);
  check('Auto-verified business appears in public search', found, `${itemsOf(r).length} results`);

  // ── Part B: SEARCH FILTERS ──
  console.log('\n── Part B: Search Filters (price / rating / radius / category) ──');
  r = await api('GET', `/businesses?search=${encodeURIComponent(bizName)}&priceMin=10&priceMax=200&page=1&pageSize=20`);
  check('Search with priceMin/priceMax → 200', r.status === 200, `HTTP ${r.status}`);
  check('Business survives price filter', itemsOf(r).some(x => x.id === businessId),
    `${itemsOf(r).length} results`);

  r = await api('GET', `/businesses?search=${encodeURIComponent(bizName)}&ratingMin=3&page=1&pageSize=20`);
  check('Search with ratingMin → 200', r.status === 200, `HTTP ${r.status}`);

  r = await api('GET', `/businesses?search=${encodeURIComponent(bizName)}&radiusKm=5000&latitude=40.7128&longitude=-74.0060&page=1&pageSize=20`);
  check('Search with radius/lat/lng → 200', r.status === 200, `HTTP ${r.status}`);
  const withDist = itemsOf(r).find(x => x.id === businessId);
  // Business is at the same coords as the search point, so the haversine
  // distance is 0 — the important part is that it is computed and returned.
  check('Distance computed and returned with radius filter', typeof withDist?.distanceKm === 'number',
    `distanceKm=${withDist?.distanceKm}`);

  r = await api('GET', `/businesses?search=${encodeURIComponent(bizName)}&priceMin=9999&page=1&pageSize=20`);
  check('Impossible price filter returns no results', !itemsOf(r).some(x => x.id === businessId),
    `${itemsOf(r).length} results`);

  // ── Setup: customer ──
  console.log('\n── Setup: register customer ──');
  r = await api('POST', '/auth/register', { body: {
    firstName: 'E2E2', lastName: 'Customer', email: customerEmail,
    password: 'E2ePass123!', confirmPassword: 'E2ePass123!', accountType: 'customer'
  }});
  check('Register customer → 201/200', [200, 201].includes(r.status), `HTTP ${r.status}`);
  const customerToken = r.json?.data?.accessToken;
  check('Customer token issued', !!customerToken);

  // ── Part C: FAVORITES ──
  console.log('\n── Part C: Favorites ──');
  r = await api('POST', `/favorites/${businessId}`, { token: customerToken });
  check('Add favorite → 200', r.status === 200, `HTTP ${r.status} ${r.text.slice(0, 120)}`);

  r = await api('GET', '/favorites', { token: customerToken });
  check('GET /favorites → 200', r.status === 200, `HTTP ${r.status}`);
  check('Favorite appears in list', itemsOf(r).some(x => x.id === businessId),
    `${itemsOf(r).length} favorites`);

  r = await api('GET', '/favorites/ids', { token: customerToken });
  check('GET /favorites/ids → 200', r.status === 200, `HTTP ${r.status}`);
  const ids = itemsOf(r);
  check('Favorite id present in ids list', Array.isArray(ids) && ids.includes(businessId),
    `ids=${JSON.stringify(ids)}`);

  r = await api('DELETE', `/favorites/${businessId}`, { token: customerToken });
  check('Remove favorite → 200', r.status === 200, `HTTP ${r.status} ${r.text.slice(0, 120)}`);

  r = await api('GET', '/favorites', { token: customerToken });
  check('Favorite removed from list', !itemsOf(r).some(x => x.id === businessId),
    `${itemsOf(r).length} favorites remain`);

  // ── Part D: AI CHAT ──
  console.log('\n── Part D: AI Chat (rule-based fallback) ──');
  r = await api('POST', '/chat/messages', { token: customerToken, body: { message: 'How do I cancel a booking?' } });
  check('POST /chat/messages → 200', r.status === 200, `HTTP ${r.status} ${r.text.slice(0, 140)}`);
  const reply = r.json?.data?.reply ?? r.json?.data?.assistantMessage;
  check('Chat returned a reply', typeof reply === 'string' && reply.length > 0,
    `reply="${(reply || '').slice(0, 60)}..."`);

  r = await api('GET', '/chat/history?limit=50', { token: customerToken });
  check('GET /chat/history → 200', r.status === 200, `HTTP ${r.status}`);
  const history = itemsOf(r);
  check('Chat history persisted (has messages)', Array.isArray(history) && history.length >= 2,
    `${history.length} messages`);

  // ── Part E: SUPPORT TICKETS ──
  console.log('\n── Part E: Support Tickets ──');
  r = await api('POST', '/support/tickets', { token: customerToken, body: {
    category: 'Payment', subject: 'Charge question', message: 'I was charged twice for my appointment.'
  }});
  check('Create support ticket → 201/200', [200, 201].includes(r.status), `HTTP ${r.status} ${r.text.slice(0, 120)}`);
  const ticketId = r.json?.data?.ticketId;
  check('Ticket ID returned', !!ticketId, `ticketId=${ticketId}`);

  r = await api('GET', '/support/tickets', { token: customerToken });
  check('GET /support/tickets → 200', r.status === 200, `HTTP ${r.status}`);
  check('Ticket appears in my tickets', itemsOf(r).some(x => x.id === ticketId),
    `${itemsOf(r).length} tickets`);

  // ── Part F: CURRENCIES ──
  console.log('\n── Part F: Currencies ──');
  r = await api('GET', '/currencies');
  check('GET /currencies → 200', r.status === 200, `HTTP ${r.status}`);
  const rates = ratesOf(r);
  check('Rates list returned', Array.isArray(rates) && rates.length >= 3, `${rates.length} currencies`);
  check('USD present with rate 1.0', rates.some(c => c.code === 'USD' && c.rate === 1),
    JSON.stringify(rates.slice(0, 2)));

  // ── Part G: GOOGLE AUTH ENDPOINT ──
  console.log('\n── Part G: Google Auth endpoint ──');
  // No real Google token available; the endpoint must reject cleanly (not 404/500).
  r = await api('POST', '/auth/google', { body: { idToken: 'invalid.garbage.token', accountType: 'customer' } });
  check('POST /auth/google exists and rejects bad token', [400, 401].includes(r.status),
    `HTTP ${r.status} ${r.text.slice(0, 100)}`);
} catch (e) {
  failed++;
  results.push(`  ❌ Unhandled exception: ${e.message}`);
}

console.log('\n═══ RESULTS ═══');
results.forEach(x => console.log(x));
console.log(`\n${passed} passed, ${failed} failed`);
process.exit(failed > 0 ? 1 : 0);
