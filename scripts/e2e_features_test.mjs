// E2E test: Reviews, Waitlist, and Recurring Bookings against the live API.
// Run: node scripts/e2e_features_test.mjs
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
  const headers = { 'Content-Type': 'application/json' };
  if (token) headers['Authorization'] = `Bearer ${token}`;
  const res = await fetch(`${BASE}${path}`, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
  });
  const text = await res.text();
  let json = null;
  try { json = JSON.parse(text); } catch { /* not json */ }
  return { status: res.status, json, text };
}

// Extract a paginated list from the ApiResponse envelope:
//   { data: { items: [...], totalCount, totalPages } } or { data: [...] }
function itemsOf(r) {
  const d = r?.json?.data;
  if (Array.isArray(d)) return d;
  if (d && Array.isArray(d.items)) return d.items;
  return [];
}

const stamp = Date.now();
const ownerEmail = `e2e.owner.${stamp}@bookify.test`;
const customerEmail = `e2e.customer.${stamp}@bookify.test`;
const bizName = `E2E Features Spa ${stamp}`;

console.log('═══ E2E: Reviews, Waitlist & Recurring Bookings ═══\n');

try {
  // ── Setup: owner + business + service + provider ──
  console.log('── Setup: register owner ──');
  let r = await api('POST', '/auth/register', { body: {
    firstName: 'E2E', lastName: 'Owner', email: ownerEmail,
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
    country: 'US', timeZone: 'UTC', currency: 'USD', description: 'E2E features test',
    email: ownerEmail, categoryIds: catId ? [catId] : []
  }});
  check('Create business → 201/200', [200, 201].includes(r.status), `HTTP ${r.status}`);
  const businessId = r.json?.data?.id;
  check('Business ID returned', !!businessId, `id=${businessId}`);

  const hours = Array.from({ length: 7 }, (_, i) => ({
    dayOfWeek: (i + 1) % 7, openTime: '09:00', closeTime: '17:00', isClosed: false
  }));
  r = await api('PUT', `/businesses/${businessId}/hours`, { token: ownerToken, body: { hours } });
  check('Set hours → 200', r.status === 200, `HTTP ${r.status}`);

  r = await api('POST', `/businesses/${businessId}/services`, { token: ownerToken, body: {
    name: 'E2E Massage 60min', durationMinutes: 60, priceAmount: 90, currency: 'USD'
  }});
  check('Add service → 201/200', [200, 201].includes(r.status), `HTTP ${r.status}`);
  const serviceId = r.json?.data?.serviceId;
  check('Service ID returned', !!serviceId, `serviceId=${serviceId}`);

  r = await api('POST', `/businesses/${businessId}/providers`, { token: ownerToken, body: {
    firstName: 'E2E', lastName: 'Staff', email: `e2e.staff.${stamp}@bookify.test`,
    title: 'Therapist', bio: 'E2E test provider'
  }});
  check('Add provider → 201/200', [200, 201].includes(r.status), `HTTP ${r.status}`);
  const providerId = r.json?.data?.providerId;
  check('Provider ID returned', !!providerId, `providerId=${providerId}`);

  // ── Setup: customer ──
  console.log('\n── Setup: register customer ──');
  r = await api('POST', '/auth/register', { body: {
    firstName: 'E2E', lastName: 'Customer', email: customerEmail,
    password: 'E2ePass123!', confirmPassword: 'E2ePass123!', accountType: 'customer'
  }});
  check('Register customer → 201/200', [200, 201].includes(r.status), `HTTP ${r.status}`);
  const customerToken = r.json?.data?.accessToken;
  check('Customer token issued', !!customerToken);

  // ── Part 1: REVIEWS ──
  console.log('\n── Part 1: Reviews ──');

  // Book an appointment so we can complete it and review it.
  const apptStart = new Date(Date.now() + 7 * 24 * 3600 * 1000);
  apptStart.setUTCHours(10, 0, 0, 0);
  const apptEnd = new Date(apptStart.getTime() + 60 * 60 * 1000);
  r = await api('POST', '/appointments', { token: customerToken, body: {
    providerId, serviceId, businessId,
    startTime: apptStart.toISOString(), endTime: apptEnd.toISOString(),
    customerNotes: 'E2E review test booking'
  }});
  check('Book appointment → 201/200', [200, 201].includes(r.status), `HTTP ${r.status} ${r.text.slice(0, 160)}`);
  const apptId = r.json?.data?.id ?? r.json?.data?.appointmentId;
  check('Appointment ID returned', !!apptId, `id=${apptId}`);

  // Owner confirms, starts, and completes the appointment.
  if (apptId) {
    r = await api('PUT', `/appointments/${apptId}/confirm`, { token: ownerToken });
    check('Owner confirms appointment → 200', r.status === 200, `HTTP ${r.status} ${r.text.slice(0, 120)}`);
    r = await api('PUT', `/appointments/${apptId}/start`, { token: ownerToken });
    check('Owner starts appointment → 200', r.status === 200, `HTTP ${r.status} ${r.text.slice(0, 120)}`);
    r = await api('PUT', `/appointments/${apptId}/complete`, { token: ownerToken });
    check('Owner completes appointment → 200', r.status === 200, `HTTP ${r.status} ${r.text.slice(0, 120)}`);

    r = await api('GET', `/appointments/${apptId}`, { token: customerToken });
    const apptStatus = r.json?.data?.status;
    check('Appointment status is Completed', apptStatus === 'Completed', `status=${apptStatus}`);
  }

  // Create a review for the completed appointment.
  r = await api('POST', `/reviews/appointments/${apptId}`, { token: customerToken, body: {
    rating: 5, comment: 'Amazing service, highly recommend!'
  }});
  check('Create review → 201/200', [200, 201].includes(r.status), `HTTP ${r.status} ${r.text.slice(0, 160)}`);
  const reviewId = r.json?.data;
  check('Review ID returned', typeof reviewId === 'string' && reviewId.length > 0, `id=${reviewId}`);

  // Confirm the review appears in the business review list.
  r = await api('GET', `/reviews?businessId=${businessId}&page=1&pageSize=20`);
  check('GET business reviews → 200', r.status === 200, `HTTP ${r.status}`);
  let reviews = itemsOf(r);
  let foundReview = reviews.some(x => x.id === reviewId);
  check('Review appears in business review list', foundReview, `${reviews.length} reviews total`);
  const listed = reviews.find(x => x.id === reviewId);
  check('Review rating = 5', listed?.rating === 5, `rating=${listed?.rating}`);
  check('Review has customer name', !!listed?.customerName, `name=${listed?.customerName}`);
  check('Review is verified purchase', listed?.isVerifiedPurchase === true, `verified=${listed?.isVerifiedPurchase}`);

  // Update the review (edit own review).
  r = await api('PUT', `/reviews/${reviewId}`, { token: customerToken, body: {
    rating: 4, comment: 'Great service, just a small wait.'
  }});
  check('Update review → 200', r.status === 200, `HTTP ${r.status} ${r.text.slice(0, 120)}`);

  r = await api('GET', `/reviews?businessId=${businessId}&page=1&pageSize=20`);
  const updated = itemsOf(r).find(x => x.id === reviewId);
  check('Review updated to rating 4', updated?.rating === 4, `rating=${updated?.rating}`);
  check('Review comment updated', updated?.comment === 'Great service, just a small wait.');

  // Vote + report (helpful/vote endpoint).
  r = await api('POST', `/reviews/${reviewId}/vote`, { token: customerToken, body: { isHelpful: true } });
  check('Vote helpful → 200', r.status === 200, `HTTP ${r.status} ${r.text.slice(0, 120)}`);

  // Delete the review.
  r = await api('DELETE', `/reviews/${reviewId}`, { token: customerToken });
  check('Delete review → 200', r.status === 200, `HTTP ${r.status} ${r.text.slice(0, 120)}`);

  r = await api('GET', `/reviews?businessId=${businessId}&page=1&pageSize=20`);
  const gone = itemsOf(r).some(x => x.id === reviewId);
  check('Review removed from business list', !gone, `${itemsOf(r).length} reviews remain`);

  // ── Part 2: WAITLIST ──
  console.log('\n── Part 2: Waitlist ──');

  const waitDate = new Date(Date.now() + 3 * 24 * 3600 * 1000);
  const waitDateStr = waitDate.toISOString().split('T')[0];

  r = await api('POST', '/waitlist/join', { token: customerToken, body: {
    businessId, providerId, serviceId,
    appointmentDate: waitDateStr,
    preferredStartTime: '09:00:00',
    preferredEndTime: '10:00:00',
    notes: 'Would love an early slot'
  }});
  check('Join waitlist → 201/200', [200, 201].includes(r.status), `HTTP ${r.status} ${r.text.slice(0, 160)}`);
  const entryId = r.json?.data?.entryId ?? r.json?.data?.id;
  const position = r.json?.data?.position;
  check('Waitlist entry ID returned', !!entryId, `entryId=${entryId}`);
  check('Waitlist position returned', typeof position === 'number', `position=${position}`);

  // Confirm it appears in "my waitlist".
  r = await api('GET', '/waitlist/my', { token: customerToken });
  check('GET my waitlist → 200', r.status === 200, `HTTP ${r.status}`);
  let waitlist = itemsOf(r);
  const foundEntry = waitlist.some(x => x.id === entryId);
  check('Entry appears in my waitlist', foundEntry, `${waitlist.length} entries`);
  const wl = waitlist.find(x => x.id === entryId);
  check('Waitlist entry has service name', !!wl?.serviceName, `service=${wl?.serviceName}`);
  check('Waitlist entry has status', !!wl?.status, `status=${wl?.status}`);

  // Leave the waitlist.
  r = await api('DELETE', `/waitlist/${entryId}`, { token: customerToken });
  check('Leave waitlist → 200', r.status === 200, `HTTP ${r.status} ${r.text.slice(0, 120)}`);

  r = await api('GET', '/waitlist/my', { token: customerToken });
  const stillThere = itemsOf(r).some(x => x.id === entryId);
  check('Entry removed from my waitlist', !stillThere, `${itemsOf(r).length} entries remain`);

  // ── Part 3: RECURRING BOOKINGS ──
  console.log('\n── Part 3: Recurring Bookings ──');

  // Weekly recurrence (recurrenceType 1), 4 occurrences, repeating on the
  // weekday of the series start.
  // NOTE: start 9 days out (a different day than the review appointment's
  // slot) so the series occurrences do not conflict with the appointment
  // booked for the review test.
  const recStart = new Date(Date.now() + 9 * 24 * 3600 * 1000);
  recStart.setUTCHours(14, 0, 0, 0);
  const recStartDate = recStart.toISOString().split('T')[0];
  const dayOfWeek = recStart.getUTCDay(); // 0=Sunday .. 6=Saturday (System.DayOfWeek)
  r = await api('POST', '/recurringbookings', { token: customerToken, body: {
    providerId, serviceId, businessId,
    recurrenceType: 1, // Weekly
    startTime: '14:00:00',
    endTime: '15:00:00',
    seriesStartDate: recStartDate,
    maxOccurrences: 4,
    interval: 1,
    daysOfWeek: [dayOfWeek],
    notes: 'Weekly massage series'
  }});
  check('Create recurring booking → 201/200', [200, 201].includes(r.status), `HTTP ${r.status} ${r.text.slice(0, 160)}`);

  // Confirm the series appears in my recurring bookings.
  r = await api('GET', '/recurringbookings?role=customer', { token: customerToken });
  check('GET recurring bookings → 200', r.status === 200, `HTTP ${r.status}`);
  let series = itemsOf(r);
  const recSeries = series.find(x =>
    x.businessName === bizName && x.serviceName === 'E2E Massage 60min');
  check('Recurring series appears in my list', !!recSeries, `${series.length} series`);
  check('Series is active', recSeries?.isActive === true, `isActive=${recSeries?.isActive}`);
  check('Series has weekly type', recSeries?.recurrenceType === 'Weekly', `type=${recSeries?.recurrenceType}`);
  check('Series occurrences created', (recSeries?.occurrencesCreated ?? 0) >= 1,
    `occurrencesCreated=${recSeries?.occurrencesCreated}`);
  const recId = recSeries?.id;
  check('Series ID returned', !!recId, `id=${recId}`);

  // Confirm multiple appointment occurrences were generated.
  r = await api('GET', `/appointments?role=customer&page=1&pageSize=50`, { token: customerToken });
  const allAppts = itemsOf(r);
  const generated = allAppts.filter(x => x.businessId === businessId && x.status === 'Pending');
  check('Recurring occurrences generated as appointments', generated.length >= 2,
    `${generated.length} pending appointments for business`);

  // Cancel the series.
  if (recId) {
    r = await api('PUT', `/recurringbookings/${recId}/cancel`, { token: customerToken });
    check('Cancel recurring series → 200', r.status === 200, `HTTP ${r.status} ${r.text.slice(0, 140)}`);
  }

  r = await api('GET', '/recurringbookings?role=customer', { token: customerToken });
  const afterCancel = itemsOf(r).find(x => x.id === recId);
  check('Series is now cancelled (isActive=false)', afterCancel?.isActive === false,
    `isActive=${afterCancel?.isActive}`);

  r = await api('GET', `/appointments?role=customer&page=1&pageSize=50`, { token: customerToken });
  const afterAppts = itemsOf(r).filter(x => x.businessId === businessId);
  const cancelled = afterAppts.filter(x => x.status === 'Cancelled');
  check('Future occurrences cancelled', cancelled.length >= 2,
    `${cancelled.length} cancelled appointments for business`);
} catch (e) {
  failed++;
  results.push(`  ❌ Unhandled exception: ${e.message}`);
}

console.log('\n═══ RESULTS ═══');
results.forEach(x => console.log(x));
console.log(`\n${passed} passed, ${failed} failed`);
process.exit(failed > 0 ? 1 : 0);
