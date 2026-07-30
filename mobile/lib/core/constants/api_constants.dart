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
  static const String userPreferences = '/settings/preferences';

  // ── Business endpoints ──
  static const String businesses = '/businesses';
  static const String search = '/search';

  // ── Category endpoints ──
  static const String categories = '/categories';

  // ── Appointment endpoints ──
  static const String appointments = '/appointments';
  static const String availableSlots = '/appointments/slots';

  // ── Provider endpoints ──
  static const String providers = '/providers';

  // ── Review endpoints ──
  static const String reviews = '/reviews';

  // ── Notification endpoints ──
  static const String notifications = '/notifications';

  // ── Health ──
  static const String health = '/health';
}
