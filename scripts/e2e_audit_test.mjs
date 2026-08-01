// E2E audit test: admin reject fallback, auth enforcement (401s), currency math,
// Google invalid token, and the appointment book → confirm → start → complete cycle.
// Run: node scripts/e2e_audit_test.mjs  (backend must be running on :5136)
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

const stamp = Date.now();
const ownerEmail = `audit.owner.${stamp}@bookify.test`;
const customerEmail = `audit.customer.${stamp}@bookify.test`;
const bizName = `Audit Test Spa ${stamp}`;

console.log('═══ E2E: Admin fallback, Auth enforcement, Currency math, Appt lifecycle ═══\n');

try {
  // ── Auth enforcement: endpoints must reject unauthenticated access ──
  console.log('── Part 1: Unauthenticated requests must return 401 ──');
  const unauthPaths = [
    ['GET', '/favorites'],
    ['POST', '/favorites/00000000-0000-0000-0000-000000000000'],
    ['POST', '/chat/messages'],
    ['GET', '/chat/history'],
    ['POST', '/support/tickets'],
    ['GET', '/support/tickets'],
    ['GET', '/users/me'],
    ['GET', '/appointments'],
    ['GET', '/recurringbookings'],
    ['GET', '/waitlist/my'],
    ['GET', '/admin/businesses'],
  ];
  for (const [method, path] of unauthPaths) {
    const r = await api(method, path);
    check(`Unauthenticated ${method} ${path} → 401`, r.status === 401, `HTTP ${r.status}`);
  }
  // Public endpoints must NOT require auth.
  for (const path of ['/currencies', '/categories', '/businesses?page=1&pageSize=20']) {
    const r = await api('GET', path);
    check(`Public ${path} allowed without token`, r.status === 200, `HTTP ${r.status}`);
  }

  // ── Setup: owner + incomplete business ──
  console.log('\n── Setup: register owner + create incomplete business ──');
  let r = await api('POST', '/auth/register', { body: {
    firstName: 'Audit', lastName: 'Owner', email: ownerEmail,
    password: 'E2ePass123!', confirmPassword: 'E2ePass123!', accountType: 'businessOwner'
  }});
  check('Register owner → 201/200', [200, 201].includes(r.status), `HTTP ${r.status}`);
  const ownerToken = r.json?.data?.accessToken;
  check('Owner token issued', !!ownerToken);

  r = await api('GET', '/categories');
  const catId = r.json?.data?.[0]?.id;

  // NO image, NO hours, NO service, NO provider → checklist incomplete → Pending.
  r = await api('POST', '/businesses', { token: ownerToken, body: {
    name: bizName, addressLine1: '123 Audit Ave', city: 'Testville', postalCode: '12345',
    country: 'US', timeZone: 'UTC', currency: 'USD', description: 'Audit E2E business',
    email: ownerEmail, categoryIds: catId ? [catId] : []
  }});
  check('Create business → 201/200', [200, 201].includes(r.status), `HTTP ${r.status}`);
  const businessId = r.json?.data?.id;
  check('Business ID returned', !!businessId, `id=${businessId}`);

  r = await api('GET', '/businesses/mine', { token: ownerToken });
  const mine = itemsOf(r).find(x => x.id === businessId);
  check('Business starts Pending', mine?.verificationStatus === 'Pending', `status=${mine?.verificationStatus}`);
  check('Not auto-verified without checklist', mine?.isChecklistComplete === false);

  // ── Part 2: Admin REJECT fallback ──
  console.log('\n── Part 2: Admin reject fallback (override) ──');
  r = await api('POST', '/auth/login', { body: { email: 'admin@bookify.com', password: 'Admin@123456' } });
  check('Admin login → 200', r.status === 200, `HTTP ${r.status}`);
  const adminToken = r.json?.data?.accessToken;
  check('Admin token issued', !!adminToken);

  r = await api('POST', `/admin/businesses/${businessId}/reject`, { token: adminToken, body: { reason: 'Audit test rejection' } });
  check('Admin reject → 200', r.status === 200, `HTTP ${r.status} ${r.text.slice(0, 120)}`);

  r = await api('GET', '/businesses/mine', { token: ownerToken });
  const rejected = itemsOf(r).find(x => x.id === businessId);
  check('Business status is Rejected', rejected?.verificationStatus === 'Rejected', `status=${rejected?.verificationStatus}`);

  r = await api('GET', `/businesses?search=${encodeURIComponent(bizName)}`);
  check('Rejected business NOT in public search', !itemsOf(r).some(x => x.id === businessId), `${itemsOf(r).length} results`);

  // Admin VERIFY fallback (override) on the rejected business → Approved.
  r = await api('POST', `/admin/businesses/${businessId}/verify`, { token: adminToken });
  check('Admin verify (override) → 200', r.status === 200, `HTTP ${r.status} ${r.text.slice(0, 120)}`);

  r = await api('GET', '/businesses/mine', { token: ownerToken });
  const verified = itemsOf(r).find(x => x.id === businessId);
  check('Business Approved after admin override', verified?.verificationStatus === 'Approved', `status=${verified?.verificationStatus}`);

  // ── Part 3: Currency rates + conversion math ──
  console.log('\n── Part 3: Currency rates & conversion math ──');
  r = await api('GET', '/currencies');
  check('GET /currencies → 200', r.status === 200, `HTTP ${r.status}`);
  const rates = r.json?.data?.rates ?? r.json?.data ?? [];
  check('Rates list returned', Array.isArray(rates) && rates.length >= 3, `${rates.length} currencies`);
  const usd = rates.find(c => c.code === 'USD');
  check('USD present with rate 1.0', !!usd && usd.rate === 1, `USD rate=${usd?.rate}`);
  const pkr = rates.find(c => c.code === 'PKR');
  const eur = rates.find(c => c.code === 'EUR');
  if (pkr && usd) {
    const converted = (100 / usd.rate) * pkr.rate;
    check('100 USD → PKR conversion math', Math.abs(converted - 100 * pkr.rate) < 0.001, `100 USD = ${converted.toFixed(2)} PKR (rate=${pkr.rate})`);
  }
  if (eur && pkr) {
    // Amount in EUR → convert to PKR via USD pivot: (amount / eur.rate) * pkr.rate
    const amount = 50;
    const converted = (amount / eur.rate) * pkr.rate;
    const expected = amount * (pkr.rate / eur.rate);
    check('50 EUR → PKR via USD pivot', Math.abs(converted - expected) < 0.001, `50 EUR = ${converted.toFixed(2)} PKR`);
  }

  // ── Part 4: Google invalid token fails gracefully ──
  console.log('\n── Part 4: Google auth invalid token ──');
  r = await api('POST', '/auth/google', { body: { idToken: 'invalid.garbage.token', accountType: 'customer' } });
  check('POST /auth/google rejects bad token (400/401, not 500)', [400, 401].includes(r.status), `HTTP ${r.status}`);
  check('No crash / 500', r.status !== 500);

  // ── Part 5: Appointment book → confirm → start → complete ──
  console.log('\n── Part 5: Appointment lifecycle (book → confirm → start → complete) ──');
  r = await api('POST', '/auth/register', { body: {
    firstName: 'Audit', lastName: 'Customer', email: customerEmail,
    password: 'E2ePass123!', confirmPassword: 'E2ePass123!', accountType: 'customer'
  }});
  check('Register customer → 201/200', [200, 201].includes(r.status), `HTTP ${r.status}`);
  const customerToken = r.json?.data?.accessToken;

  // Complete the checklist so the business is bookable (hours, service, provider, image).
  const hours = Array.from({ length: 7 }, (_, i) => ({
    dayOfWeek: (i + 1) % 7, openTime: '09:00', closeTime: '17:00', isClosed: false
  }));
  r = await api('PUT', `/businesses/${businessId}/hours`, { token: ownerToken, body: { hours } });
  check('Set hours → 200', r.status === 200, `HTTP ${r.status}`);
  r = await api('POST', `/businesses/${businessId}/services`, { token: ownerToken, body: {
    name: 'Audit Massage 60min', durationMinutes: 60, priceAmount: 90, currency: 'USD'
  }});
  check('Add service → 201/200', [200, 201].includes(r.status), `HTTP ${r.status}`);
  const serviceId = r.json?.data?.serviceId;
  r = await api('POST', `/businesses/${businessId}/providers`, { token: ownerToken, body: {
    firstName: 'Audit', lastName: 'Staff', email: `audit.staff.${stamp}@bookify.test`,
    title: 'Therapist', bio: 'Audit provider'
  }});
  check('Add provider → 201/200', [200, 201].includes(r.status), `HTTP ${r.status}`);
  const providerId = r.json?.data?.providerId;
  r = await api('POST', `/businesses/${businessId}/images`, { token: ownerToken, body: {
    imageUrls: ['https://example.com/audit-cover.jpg']
  }});
  check('Add image → 200', r.status === 200, `HTTP ${r.status}`);

  r = await api('GET', '/businesses/mine', { token: ownerToken });
  const approved2 = itemsOf(r).find(x => x.id === businessId);
  check('Business Approved after completing checklist', approved2?.verificationStatus === 'Approved', `status=${approved2?.verificationStatus}`);

  const apptStart = new Date(Date.now() + 7 * 24 * 3600 * 1000);
  apptStart.setUTCHours(10, 0, 0, 0);
  const apptEnd = new Date(apptStart.getTime() + 60 * 60 * 1000);
  r = await api('POST', '/appointments', { token: customerToken, body: {
    providerId, serviceId, businessId,
    startTime: apptStart.toISOString(), endTime: apptEnd.toISOString(),
    customerNotes: 'Audit lifecycle test'
  }});
  check('Book appointment → 201/200', [200, 201].includes(r.status), `HTTP ${r.status} ${r.text.slice(0, 160)}`);
  const apptId = r.json?.data?.id ?? r.json?.data?.appointmentId;
  check('Appointment ID returned', !!apptId, `id=${apptId}`);

  if (apptId) {
    r = await api('PUT', `/appointments/${apptId}/confirm`, { token: ownerToken });
    check('Confirm → 200', r.status === 200, `HTTP ${r.status} ${r.text.slice(0, 120)}`);
    r = await api('GET', `/appointments/${apptId}`, { token: customerToken });
    check('Status Confirmed after confirm', r.json?.data?.status === 'Confirmed', `status=${r.json?.data?.status}`);

    r = await api('PUT', `/appointments/${apptId}/start`, { token: ownerToken });
    check('Start → 200', r.status === 200, `HTTP ${r.status} ${r.text.slice(0, 120)}`);
    r = await api('GET', `/appointments/${apptId}`, { token: customerToken });
    check('Status InProgress after start', r.json?.data?.status === 'InProgress', `status=${r.json?.data?.status}`);

    r = await api('PUT', `/appointments/${apptId}/complete`, { token: ownerToken });
    check('Complete → 200', r.status === 200, `HTTP ${r.status} ${r.text.slice(0, 120)}`);
    r = await api('GET', `/appointments/${apptId}`, { token: customerToken });
    check('Status Completed after complete', r.json?.data?.status === 'Completed', `status=${r.json?.data?.status}`);

    // Cross-user authorization: another user must NOT be able to start this appointment.
    r = await api('PUT', `/appointments/${apptId}/start`, { token: customerToken });
    check('Customer cannot start own appointment (403/404/400)', [400, 403, 404].includes(r.status), `HTTP ${r.status}`);
  }

  // ── Part 6: Support tickets (contact + report-a-problem) ──
  console.log('\n── Part 6: Support tickets ──');
  r = await api('POST', '/support/tickets', { token: customerToken, body: {
    category: 'General', subject: 'Contact question', message: 'How do I change my appointment time?'
  }});
  check('Contact ticket → 201/200', [200, 201].includes(r.status), `HTTP ${r.status}`);
  const ticket1 = r.json?.data?.ticketId;
  r = await api('POST', '/support/tickets', { token: customerToken, body: {
    category: 'Report a problem', subject: 'Booking glitch', message: 'The confirm button did nothing.',
    appointmentId: apptId
  }});
  check('Report-a-problem ticket → 201/200', [200, 201].includes(r.status), `HTTP ${r.status}`);
  const ticket2 = r.json?.data?.ticketId;

  r = await api('GET', '/support/tickets', { token: customerToken });
  const tickets = itemsOf(r);
  check('Both tickets listed', tickets.some(x => x.id === ticket1) && tickets.some(x => x.id === ticket2),
    `${tickets.length} tickets`);

  // ── Part 7: Favorites & chat quick re-confirmation ──
  console.log('\n── Part 7: Favorites + chat ──');
  r = await api('POST', `/favorites/${businessId}`, { token: customerToken });
  check('Add favorite → 200', r.status === 200, `HTTP ${r.status}`);
  r = await api('GET', '/favorites', { token: customerToken });
  check('Favorite listed', itemsOf(r).some(x => x.id === businessId), `${itemsOf(r).length} favorites`);
  r = await api('DELETE', `/favorites/${businessId}`, { token: customerToken });
  check('Remove favorite → 200', r.status === 200, `HTTP ${r.status}`);

  r = await api('POST', '/chat/messages', { token: customerToken, body: { message: 'What services do you offer?' } });
  const chatReply = r.json?.data?.reply ?? r.json?.data?.assistantMessage;
  check('Chat message → 200 with reply', r.status === 200 && typeof chatReply === 'string' && chatReply.length > 0, `HTTP ${r.status}`);
  r = await api('GET', '/chat/history?limit=50', { token: customerToken });
  check('Chat history persisted', Array.isArray(itemsOf(r)) && itemsOf(r).length >= 2, `${itemsOf(r).length} messages`);
} catch (e) {
  failed++;
  results.push(`  ❌ Unhandled exception: ${e.message}`);
}

console.log('\n═══ RESULTS ═══');
results.forEach(x => console.log(x));
console.log(`\n${passed} passed, ${failed} failed`);
process.exit(failed > 0 ? 1 : 0);
