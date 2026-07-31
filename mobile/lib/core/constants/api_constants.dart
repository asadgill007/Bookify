/// API configuration constants for the Bookify backend.
class ApiConstants {
  ApiConstants._();

  /// Base URL for the Bookify API.
  /// For Android emulator, use 10.0.2.2 instead of localhost.
  static const String baseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'http://localhost:5136',
  );

  /// API version prefix.
  static const String apiPrefix = '/api/v1';

  /// Full API base URL with version prefix.
  static String get apiBaseUrl => '$baseUrl$apiPrefix';

  // ── Auth endpoints ──
  static const String login = '/auth/login';
  static const String register = '/auth/register';
  static const String refreshToken = '/auth/refresh';
  static const String logout = '/auth/logout';
  static const String forgotPassword = '/auth/forgot-password';
  static const String resetPassword = '/auth/reset-password';

  // ── User endpoints ──
  static const String userProfile = '/users/me';
  static const String changePassword = '/users/me/password';
  static const String deleteAccount = '/users/me';
  static const String userPreferences = '/settings/preferences';

  // ── Business endpoints ──
  static const String businesses = '/businesses';
  static const String myBusinesses = '/businesses/mine';
  static const String search = '/search';

  /// GET business detail by slug: /businesses/{slug}
  static String businessBySlug(String slug) => '/businesses/$slug';

  /// PUT business hours: /businesses/{businessId}/hours
  static String businessHours(String businessId) =>
      '/businesses/$businessId/hours';

  /// POST add provider: /businesses/{businessId}/providers
  static String businessProviders(String businessId) =>
      '/businesses/$businessId/providers';

  /// POST resubmit rejected business: /businesses/{businessId}/resubmit
  static String businessResubmit(String businessId) =>
      '/businesses/$businessId/resubmit';

  // ── Service endpoints (per business) ──
  /// POST create service: /businesses/{businessId}/services
  static String businessServices(String businessId) =>
      '/businesses/$businessId/services';

  /// PUT / DELETE service: /businesses/{businessId}/services/{serviceId}
  static String businessService(String businessId, String serviceId) =>
      '/businesses/$businessId/services/$serviceId';

  // ── Category endpoints ──
  static const String categories = '/categories';

  // ── Appointment endpoints ──
  static const String appointments = '/appointments';
  static const String availableSlots = '/appointments/slots';

  // ── Provider endpoints ──
  static const String providers = '/providers';

  /// GET provider slots: /providers/{providerId}/slots
  static String providerSlots(String providerId) =>
      '/providers/$providerId/slots';

  // ── Review endpoints ──
  static const String reviews = '/reviews';

  // ── Notification endpoints ──
  static const String notifications = '/notifications';

  // ── Admin endpoints ──
  static const String adminBusinesses = '/admin/businesses';

  /// POST verify: /admin/businesses/{businessId}/verify
  static String adminVerify(String businessId) =>
      '/admin/businesses/$businessId/verify';

  /// POST reject: /admin/businesses/{businessId}/reject
  static String adminReject(String businessId) =>
      '/admin/businesses/$businessId/reject';

  /// POST toggle active: /admin/businesses/{businessId}/status
  static String adminToggleStatus(String businessId) =>
      '/admin/businesses/$businessId/status';

  // ── Document endpoints ──

  /// GET documents for a business: /documents/business/{businessId}
  static String businessDocuments(String businessId) =>
      '/documents/business/$businessId';

  /// GET download a document: /documents/{documentId}/download
  static String documentDownload(String documentId) =>
      '/documents/$documentId/download';

  // ── Health ──
  static const String health = '/health';
}
