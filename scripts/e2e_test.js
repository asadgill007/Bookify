/* eslint-disable */
// Bookify E2E verification script — runs against http://localhost:5136
// Steps:
//  1. Verify seeded categories + verified-only public search
//  2. Register a NEW business owner
//  3. Onboard: create business with category, hours, service, provider
//  4. Confirm business is Pending and NOT in customer search
//  5. Admin logs in and approves the business
//  6. Confirm it now appears in customer search
//  7. Customer books an appointment against the new business
const BASE = 'http://localhost:5136/api/v1';

async function call(path, { method = 'GET', token, body } = {}) {
  const headers = { 'Content-Type': 'application/json' };
  if (token) headers['Authorization'] = 'Bearer ' + token;
  const res = await fetch(BASE + path, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
  });
  let json = null;
  try { json = await res.json(); } catch (_) {}
  return { status: res.status, json };
}

const step = (n, title) => console.log(`\n════ STEP ${n} — ${title} ════`);

// Unwrap API envelope: may be a flat array, { data: [...] } or
// { data: { items: [...] } } (paginated list).
function unwrapList(json) {
  if (!json) return [];
  if (Array.isArray(json)) return json;
  let d = json.data ?? json;
  if (Array.isArray(d)) return d;
  if (d && Array.isArray(d.items)) return d.items;
  return [];
}

(async () => {
  // ── STEP 1: Seeded data intact ──
  step(1, 'Seeded categories & verified-only public search');
  const cats = await call('/categories');
  const catList = unwrapList(cats.json);
  console.log(`Categories: ${catList.length}`);
  const spaCat = catList.find(c => c.slug === 'spa-massage') || catList[0];
  const dentalCat = catList.find(c => c.slug === 'dental-care') || catList[1];
  console.log(`  → using category: ${spaCat?.name} (${spaCat?.id})`);

  const search1 = await call('/businesses?page=1&pageSize=50');
  const bizList = unwrapList(search1.json);
  console.log(`Public search shows ${bizList.length} businesses (verified-only):`);
  bizList.slice(0, 6).forEach(b => console.log(`   - ${b.name} [verified=${b.isVerified}] slug=${b.slug}`));
  if (!bizList.every(b => b.isVerified)) {
    throw new Error('BUG: unverified business visible in public search!');
  }

  // ── STEP 2: Register a NEW business owner ──
  step(2, 'Register new business owner');
  const ts = Date.now();
  const ownerEmail = `owner${ts}@e2e.com`;
  const reg = await call('/auth/register', {
    method: 'POST',
    body: {
      firstName: 'E2E', lastName: 'Owner', email: ownerEmail,
      phoneNumber: '+1555000' + String(ts).slice(-7), password: 'Password123!',
      confirmPassword: 'Password123!', accountType: 'businessOwner',
    },
  });
  const regData = reg.json?.data ?? reg.json ?? {};
  const ownerToken = regData.accessToken;
  if (!ownerToken) throw new Error('Owner registration failed: ' + JSON.stringify(reg.json));
  console.log(`Registered owner ${ownerEmail} → role=${regData.role}`);

  // ── STEP 3: Onboard the business ──
  step(3, 'Onboard business (info, category, hours, service, provider)');
  const createBiz = await call('/businesses', {
    method: 'POST', token: ownerToken,
    body: {
      name: `E2E Luxe Wellness ${ts}`, description: 'Created during E2E verification.',
      email: ownerEmail, phoneNumber: '+15550001111',
      website: 'https://e2e.example.com',
      addressLine1: '1 E2E Street', city: 'Testville', state: 'TS',
      postalCode: '12345', country: 'US', timeZone: 'UTC', currency: 'USD',
      cancellationPolicy: 'Free cancellation 24h prior.',
      categoryIds: [spaCat.id, dentalCat.id],
      coverImageUrl: 'https://images.unsplash.com/photo-1544161515-4ab6ce6db834?w=800',
    },
  });
  const bizData = createBiz.json?.data ?? createBiz.json ?? {};
  const bizId = bizData.id;
  const bizSlug = bizData.slug;
  if (!bizId) throw new Error('Business creation failed: ' + JSON.stringify(createBiz.json));
  console.log(`Created business: ${bizData.id} slug=${bizSlug}`);

  const hours = await call(`/businesses/${bizId}/hours`, {
    method: 'PUT', token: ownerToken,
    body: { hours: [
      { dayOfWeek: 1, openTime: '09:00', closeTime: '18:00', isClosed: false },
      { dayOfWeek: 2, openTime: '09:00', closeTime: '18:00', isClosed: false },
      { dayOfWeek: 3, openTime: '09:00', closeTime: '18:00', isClosed: false },
      { dayOfWeek: 4, openTime: '09:00', closeTime: '18:00', isClosed: false },
      { dayOfWeek: 5, openTime: '09:00', closeTime: '18:00', isClosed: false },
    ]},
  });
  console.log(`Set business hours → HTTP ${hours.status}`);

  const service = await call(`/businesses/${bizId}/services`, {
    method: 'POST', token: ownerToken,
    body: { name: 'Signature Wellness Session', description: '60 min full-body session',
      durationMinutes: 60, priceAmount: 89.0, currency: 'USD', displayOrder: 1 },
  });
  const svcData = service.json?.data ?? service.json ?? {};
  const serviceId = svcData.serviceId;
  if (!serviceId) throw new Error('Service creation failed: ' + JSON.stringify(service.json));
  console.log(`Added service → ${serviceId}`);

  const staff = await call(`/businesses/${bizId}/providers`, {
    method: 'POST', token: ownerToken,
    body: { firstName: 'E2E', lastName: 'Therapist', email: `staff${ts}@e2e.com`,
      title: 'Senior Therapist', bio: 'E2E test staff member.',
      displayOrder: 1, serviceIds: [serviceId] },
  });
  const staffData = staff.json?.data ?? staff.json ?? {};
  const providerId = staffData.providerId;
  if (!providerId) throw new Error('Provider creation failed: ' + JSON.stringify(staff.json));
  console.log(`Added provider → ${providerId}`);

  // ── STEP 4: Confirm pending + hidden from search ──
  step(4, 'Business is Pending and hidden from customer search');
  const mine = await call('/businesses/mine', { token: ownerToken });
  const mineList = unwrapList(mine.json);
  const myBiz = mineList.find(b => b.id === bizId);
  console.log(`Owner sees status: ${myBiz?.verificationStatus} (isVerified=${myBiz?.isVerified})`);
  if (myBiz?.verificationStatus !== 'Pending') throw new Error('Expected Pending status!');

  const detailBySlug = await call(`/businesses/${bizSlug}`);
  console.log(`Public fetch of pending business by slug → HTTP ${detailBySlug.status} (should be 404)`);

  const searchHidden = await call(`/businesses?search=e2e&page=1&pageSize=50`);
  const hiddenList = unwrapList(searchHidden.json);
  const hiddenMatch = hiddenList.some(b => b.slug === bizSlug);
  console.log(`Business appears in public search while pending: ${hiddenMatch} (should be false)`);
  if (hiddenMatch) throw new Error('BUG: pending business leaked into public search!');

  // ── STEP 5: Admin approves ──
  step(5, 'Admin approves the business');
  const adminLogin = await call('/auth/login', {
    method: 'POST',
    body: { email: 'admin@bookify.com', password: 'Admin@123456' },
  });
  const adminData = adminLogin.json?.data ?? adminLogin.json ?? {};
  const adminToken = adminData.accessToken;
  if (!adminToken) throw new Error('Admin login failed');
  console.log('Admin logged in (admin@bookify.com)');

  const pendingList = await call('/admin/businesses?status=Pending&page=1&pageSize=50', { token: adminToken });
  const pendingBiz = unwrapList(pendingList.json).find(b => b.id === bizId);
  console.log(`Admin sees pending queue, contains new business: ${!!pendingBiz}`);

  const verify = await call(`/admin/businesses/${bizId}/verify`, { method: 'POST', token: adminToken });
  console.log(`Approve → HTTP ${verify.status} ${verify.json?.message ?? ''}`);

  const mineAfter = await call('/businesses/mine', { token: ownerToken });
  const afterList = unwrapList(mineAfter.json);
  console.log(`Owner sees status after approval: ${afterList.find(b => b.id === bizId)?.verificationStatus}`);

  // ── STEP 6: Now visible in customer search ──
  step(6, 'Business now appears in customer search');
  const searchVisible = await call(`/businesses?search=e2e&page=1&pageSize=50`);
  const visibleList = unwrapList(searchVisible.json);
  const visibleMatch = visibleList.some(b => b.slug === bizSlug);
  console.log(`Business in public search after approval: ${visibleMatch}`);
  if (!visibleMatch) throw new Error('BUG: approved business not in public search!');

  const detailPublic = await call(`/businesses/${bizSlug}`);
  const detailData = detailPublic.json?.data ?? detailPublic.json ?? {};
  console.log(`Public detail: ${detailData.name} | services=${detailData.services?.length} | providers=${detailData.providers?.length}`);

  // ── STEP 7: Customer books an appointment ──
  step(7, 'Customer books against the new business');
  const custReg = await call('/auth/register', {
    method: 'POST',
    body: { firstName: 'E2E', lastName: 'Customer', email: `cust${ts}@e2e.com`,
      password: 'Password123!', confirmPassword: 'Password123!', accountType: 'customer' },
  });
  const custData = custReg.json?.data ?? custReg.json ?? {};
  const custToken = custData.accessToken;
  console.log(`Customer registered: ${custData.email}`);

  // Find an available slot within the next 7 days (Mon-Fri + Sat availability)
  let slot = null;
  let dateStr = '';
  for (let i = 1; i <= 7; i++) {
    const day = new Date(Date.now() + i * 86400000);
    dateStr = day.toISOString().slice(0, 10);
    const slots = await call(`/providers/${providerId}/slots?serviceId=${serviceId}&date=${dateStr}`);
    const slotList = unwrapList(slots.json);
    const available = slotList.filter(s => s.isAvailable);
    console.log(`Provider slots on ${dateStr}: ${slotList.length} (${available.length} available)`);
    if (available.length > 0) { slot = available[0]; break; }
  }
  if (!slot) throw new Error('No available slots for new provider in the next 7 days!');

  const appt = await call('/appointments', {
    method: 'POST', token: custToken,
    body: {
      providerId, serviceId, businessId: bizId,
      startTime: slot.startTime, endTime: slot.endTime,
      customerNotes: 'E2E booking verification',
    },
  });
  const apptData = appt.json?.data ?? appt.json ?? {};
  console.log(`Appointment created → HTTP ${appt.status} ref=${apptData.bookingReference} status=${apptData.status} amount=${apptData.totalAmount} ${apptData.currency}`);
  if (!apptData.bookingReference) throw new Error('Booking failed: ' + JSON.stringify(appt.json));

  // ── DONE ──
  console.log('\n════════════════════════════════════════');
  console.log('✅ FULL E2E JOURNEY PASSED');
  console.log('   seeded data intact, onboarding → pending → approve → search → book');
  console.log('════════════════════════════════════════');
})().catch(e => {
  console.error('\n❌ E2E FAILED:', e.message);
  process.exit(1);
});
