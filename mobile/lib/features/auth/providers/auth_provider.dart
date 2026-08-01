import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/api_client.dart';
import '../../../core/constants/api_constants.dart';

/// Auth state.
enum AuthStatus { unknown, authenticating, authenticated, unauthenticated }

/// Account types supported at registration.
class AccountType {
  static const String customer = 'customer';
  static const String provider = 'provider';
  static const String businessOwner = 'businessOwner';
}

class AuthState {
  final AuthStatus status;
  final String? userId;
  final String? email;
  final String? role;
  final String? error;

  const AuthState({
    this.status = AuthStatus.unauthenticated,
    this.userId,
    this.email,
    this.role,
    this.error,
  });

  AuthState copyWith({
    AuthStatus? status,
    String? userId,
    String? email,
    String? role,
    String? error,
  }) {
    return AuthState(
      status: status ?? this.status,
      userId: userId ?? this.userId,
      email: email ?? this.email,
      role: role ?? this.role,
      error: error,
    );
  }
}

/// Auth notifier with login, register, logout, and auto-check on init.
class AuthNotifier extends StateNotifier<AuthState> {
  final Ref _ref;

  AuthNotifier(this._ref) : super(const AuthState()) {
    // Auto-check auth status on initialization
    _checkAuthStatus();
  }

  Future<void> _checkAuthStatus() async {
    try {
      final api = _ref.read(apiClientProvider);
      final token = await api.getAccessToken();
      if (token != null) {
        state = state.copyWith(status: AuthStatus.authenticated);
      } else {
        state = const AuthState(status: AuthStatus.unauthenticated);
      }
    } catch (_) {
      state = const AuthState(status: AuthStatus.unauthenticated);
    }
  }

  Future<void> login(String email, String password) async {
    state = state.copyWith(status: AuthStatus.authenticating, error: null);
    try {
      final api = _ref.read(apiClientProvider);
      final response = await api.post(
        ApiConstants.login,
        data: {'email': email, 'password': password},
      );

      // API wraps responses in { "data": { ... } } envelope
      final body = response.data as Map<String, dynamic>;
      final data = (body['data'] ?? body) as Map<String, dynamic>;

      final accessToken = data['accessToken'] as String?;
      final refreshToken = data['refreshToken'] as String?;

      if (accessToken == null) {
        throw Exception('No access token in response');
      }

      await api.saveTokens(accessToken, refreshToken ?? '');

      state = state.copyWith(
        status: AuthStatus.authenticated,
        email: email,
        userId: data['userId'] as String?,
        role: data['role'] as String?,
        error: null,
      );
    } catch (e) {
      state = state.copyWith(
        status: AuthStatus.unauthenticated,
        error: 'Login failed. Check your credentials.',
      );
    }
  }

  /// Registers a new account. [accountType] is one of
  /// [AccountType.customer], [AccountType.provider] or
  /// [AccountType.businessOwner]. Returns true on success.
  Future<bool> register({
    required String firstName,
    required String lastName,
    required String email,
    required String password,
    required String confirmPassword,
    String accountType = AccountType.customer,
  }) async {
    state = state.copyWith(status: AuthStatus.authenticating, error: null);
    try {
      final api = _ref.read(apiClientProvider);
      final response = await api.post(
        ApiConstants.register,
        data: {
          'firstName': firstName,
          'lastName': lastName,
          'email': email,
          'password': password,
          'confirmPassword': confirmPassword,
          'accountType': accountType,
        },
      );

      // API wraps responses in { "data": { ... } } envelope
      final body = response.data as Map<String, dynamic>;
      final data = (body['data'] ?? body) as Map<String, dynamic>;

      // Registration response includes access token directly
      final accessToken = data['accessToken'] as String?;
      final refreshToken = data['refreshToken'] as String?;

      if (accessToken != null) {
        await api.saveTokens(accessToken, refreshToken ?? '');
      } else {
        // If no token in register response, fall back to auto-login
        await login(email, password);
        return state.status == AuthStatus.authenticated;
      }

      state = state.copyWith(
        status: AuthStatus.authenticated,
        email: email,
        userId: data['userId'] as String?,
        role: data['role'] as String?,
        error: null,
      );
      return true;
    } catch (e) {
      state = state.copyWith(
        status: AuthStatus.unauthenticated,
        error: 'Registration failed. Please try again.',
      );
      return false;
    }
  }

  /// Signs in with a Google ID token via the backend. Creates a Customer
  /// account on first sign-in, or links/signs in an existing user.
  /// Returns true on success. [accountType] only applies to brand-new accounts.
  Future<bool> googleSignIn({
    required String idToken,
    String accountType = AccountType.customer,
  }) async {
    state = state.copyWith(status: AuthStatus.authenticating, error: null);
    try {
      final api = _ref.read(apiClientProvider);
      final response = await api.post(
        ApiConstants.googleLogin,
        data: {'idToken': idToken, 'accountType': accountType},
      );

      final body = response.data as Map<String, dynamic>;
      final data = (body['data'] ?? body) as Map<String, dynamic>;

      final accessToken = data['accessToken'] as String?;
      final refreshToken = data['refreshToken'] as String?;
      if (accessToken == null) {
        throw Exception('No access token in response');
      }

      await api.saveTokens(accessToken, refreshToken ?? '');
      state = state.copyWith(
        status: AuthStatus.authenticated,
        email: data['email'] as String?,
        userId: data['userId'] as String?,
        role: data['role'] as String?,
        error: null,
      );
      return true;
    } catch (e) {
      state = state.copyWith(
        status: AuthStatus.unauthenticated,
        error: 'Google sign-in failed. Please try again.',
      );
      return false;
    }
  }

  Future<void> logout() async {
    final api = _ref.read(apiClientProvider);
    await api.clearTokens();
    state = const AuthState(status: AuthStatus.unauthenticated);
  }
}

final authProvider = StateNotifierProvider<AuthNotifier, AuthState>((ref) {
  return AuthNotifier(ref);
});
