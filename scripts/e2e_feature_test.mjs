// E2E test: provider/business registration → onboarding → verification → booking
// Run: node scripts/e2e_feature_test.mjs
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

const stamp = Date.now();
const ownerEmail = `e2e.owner.${stamp}@bookify.test`;
const customerEmail = `e2e.customer.${stamp}@bookify.test`;
const bizName = `E2E Test Spa ${stamp}`;

console.log('═══ E2E: Provider/Business Registration & Verification Lifecycle ═══\n');

try {
  // 1. Register a business-owner account
  console.log('── Step 1: Register business owner ──');
  let r = await api('POST', '/auth/register', { body: {
    firstName: 'E2E', lastName: 'Owner', email: ownerEmail,
    password: 'E2ePass123!', confirmPassword: 'E2ePass123!', accountType: 'businessOwner'
  }});
  check('Register business owner → 201/200', [200, 201].includes(r.status), `HTTP ${r.status} ${r.text.slice(0, 120)}`);
  const ownerToken = r.json?.data?.accessToken;
  const ownerId = r.json?.data?.userId;
  check('Owner token issued', !!ownerToken);
  check('Owner role is BusinessOwner', r.json?.data?.role === 'BusinessOwner', `role=${r.json?.data?.role}`);
  check('Owner ID returned', !!ownerId, `id=${ownerId}`);

  // 2. Fetch categories, pick one
  console.log('\n── Step 2: Fetch categories ──');
  r = await api('GET', '/categories');
  check('Categories endpoint → 200', r.status === 200, `HTTP ${r.status}`);
  const cats = r.json?.data ?? [];
  check('At least 1 category seeded', Array.isArray(cats) && cats.length > 0, `${cats.length} categories`);
  const catId = cats[0]?.id;

  // 3. Create business with a category
  console.log('\n── Step 3: Create business (pending) ──');
  r = await api('POST', '/businesses', { token: ownerToken, body: {
    name: bizName, addressLine1: '123 Test Ave', city: 'Testville', postalCode: '12345',
    country: 'US', timeZone: 'UTC', currency: 'USD', description: 'E2E test business',
    email: ownerEmail, categoryIds: [catId]
  }});
  check('Create business → 201/200', [200, 201].includes(r.status), `HTTP ${r.status} ${r.text.slice(0, 150)}`);
  const businessId = r.json?.data?.id;
  const slug = r.json?.data?.slug;
  check('Business ID returned', !!businessId, `id=${businessId}`);
  check('Slug returned', !!slug, `slug=${slug}`);

  // 4. Set business hours
  console.log('\n── Step 4: Set business hours ──');
  // Mon..Sun -> System.DayOfWeek 1..6,0 (mirrors the Flutter onboarding app)
  const hours = Array.from({ length: 7 }, (_, i) => ({
    dayOfWeek: (i + 1) % 7, openTime: '09:00', closeTime: '17:00', isClosed: false
  }));
  r = await api('PUT', `/businesses/${businessId}/hours`, { token: ownerToken, body: { hours } });
  check('Set hours → 200', r.status === 200, `HTTP ${r.status} ${r.text.slice(0, 120)}`);

  // 5. Add a service
  console.log('\n── Step 5: Add service ──');
  r = await api('POST', `/businesses/${businessId}/services`, { token: ownerToken, body: {
    name: 'E2E Massage 60min', durationMinutes: 60, priceAmount: 90, currency: 'USD'
  }});
  check('Add service → 201/200', [200, 201].includes(r.status), `HTTP ${r.status} ${r.text.slice(0, 150)}`);
  const serviceId = r.json?.data?.serviceId;
  check('Service ID returned', !!serviceId, `serviceId=${serviceId}`);

  // 6. Add a staff/provider
  console.log('\n── Step 6: Add staff/provider ──');
  r = await api('POST', `/businesses/${businessId}/providers`, { token: ownerToken, body: {
    firstName: 'E2E', lastName: 'Staff', email: `e2e.staff.${stamp}@bookify.test`,
    title: 'Senior Therapist', bio: 'E2E test provider'
  }});
  check('Add provider → 201/200', [200, 201].includes(r.status), `HTTP ${r.status} ${r.text.slice(0, 150)}`);
  const providerId = r.json?.data?.providerId;
  check('Provider ID returned', !!providerId, `providerId=${providerId}`);

  // 7. Confirm business does NOT appear in public search (pending)
  console.log('\n── Step 7: Verify business hidden from public search (pending) ──');
  r = await api('GET', `/businesses?search=${encodeURIComponent(bizName)}`);
  check('Public search → 200', r.status === 200, `HTTP ${r.status}`);
  const searchResults = r.json?.data ?? [];
  const foundPending = searchResults.some(b => b.id === businessId);
  check('Pending business NOT in public search', !foundPending, `${searchResults?.length} results`);

  // 8. Admin login & verify
  console.log('\n── Step 8: Admin approve ──');
  r = await api('POST', '/auth/login', { body: { email: 'admin@bookify.com', password: 'Admin@123456' } });
  check('Admin login → 200', r.status === 200, `HTTP ${r.status} ${r.text.slice(0, 120)}`);
  const adminToken = r.json?.data?.accessToken;
  check('Admin token issued', !!adminToken);

  r = await api('POST', `/admin/businesses/${businessId}/verify`, { token: adminToken });
  check('Admin verify business → 200', r.status === 200, `HTTP ${r.status} ${r.text.slice(0, 150)}`);

  // 9. Confirm business NOW appears in public search
  console.log('\n── Step 9: Verify business appears in public search (approved) ──');
  r = await api('GET', `/businesses?search=${encodeURIComponent(bizName)}`);
  const foundApproved = (r.json?.data ?? []).some(b => b.id === businessId);
  check('Approved business NOW in public search', foundApproved, `${r.json?.data?.length} results`);

  // 10. Register customer
  console.log('\n── Step 10: Register customer ──');
  r = await api('POST', '/auth/register', { body: {
    firstName: 'E2E', lastName: 'Customer', email: customerEmail,
    password: 'E2ePass123!', confirmPassword: 'E2ePass123!', accountType: 'customer'
  }});
  check('Register customer → 201/200', [200, 201].includes(r.status), `HTTP ${r.status}`);
  const customerToken = r.json?.data?.accessToken;
  check('Customer token issued', !!customerToken);

  // 11. Book an appointment against the new business
  console.log('\n── Step 11: Book appointment ──');
  const start = new Date(Date.now() + 7 * 24 * 3600 * 1000);
  start.setUTCHours(10, 0, 0, 0);
  const end = new Date(start.getTime() + 60 * 60 * 1000);
  r = await api('POST', '/appointments', { token: customerToken, body: {
    providerId, serviceId, businessId,
    startTime: start.toISOString(), endTime: end.toISOString(),
    customerNotes: 'E2E booking test'
  }});
  check('Book appointment → 201/200', [200, 201].includes(r.status), `HTTP ${r.status} ${r.text.slice(0, 200)}`);
  const apptId = r.json?.data?.id ?? r.json?.data?.appointmentId;
  const bookingRef = r.json?.data?.bookingReference;
  check('Appointment ID returned', !!apptId, `id=${apptId}`);
  check('Booking reference returned', !!bookingRef, `ref=${bookingRef}`);

  // 12. Confirm appointment visible for customer
  console.log('\n── Step 12: Customer sees the appointment ──');
  if (apptId) {
    r = await api('GET', `/appointments/${apptId}`, { token: customerToken });
    check('Get appointment by id → 200', r.status === 200, `HTTP ${r.status}`);
    check('Appointment belongs to customer', r.json?.data?.id === apptId);
  }
} catch (e) {
  failed++;
  results.push(`  ❌ Unhandled exception: ${e.message}`);
}

console.log('\n═══ RESULTS ═══');
results.forEach(x => console.log(x));
console.log(`\n${passed} passed, ${failed} failed`);
process.exit(failed > 0 ? 1 : 0);
