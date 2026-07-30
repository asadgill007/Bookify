import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/api_client.dart';
import '../../../core/constants/api_constants.dart';

/// Auth state.
enum AuthStatus { unknown, authenticating, authenticated, unauthenticated }

class AuthState {
  final AuthStatus status;
  final String? userId;
  final String? email;
  final String? error;

  const AuthState({
    this.status = AuthStatus.unauthenticated,
    this.userId,
    this.email,
    this.error,
  });

  AuthState copyWith({
    AuthStatus? status,
    String? userId,
    String? email,
    String? error,
  }) {
    return AuthState(
      status: status ?? this.status,
      userId: userId ?? this.userId,
      email: email ?? this.email,
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

      final data = response.data as Map<String, dynamic>;
      final accessToken = data['accessToken'] as String;
      final refreshToken = data['refreshToken'] as String;

      await api.saveTokens(accessToken, refreshToken);

      state = state.copyWith(
        status: AuthStatus.authenticated,
        email: email,
        userId: data['userId'] as String?,
        error: null,
      );
    } catch (e) {
      state = state.copyWith(
        status: AuthStatus.unauthenticated,
        error: 'Login failed. Check your credentials.',
      );
    }
  }

  Future<void> register({
    required String firstName,
    required String lastName,
    required String email,
    required String password,
    required String confirmPassword,
  }) async {
    state = state.copyWith(status: AuthStatus.authenticating, error: null);
    try {
      final api = _ref.read(apiClientProvider);
      await api.post(
        ApiConstants.register,
        data: {
          'firstName': firstName,
          'lastName': lastName,
          'email': email,
          'password': password,
          'confirmPassword': confirmPassword,
        },
      );

      // Auto-login after successful registration
      await login(email, password);
    } catch (e) {
      state = state.copyWith(
        status: AuthStatus.unauthenticated,
        error: 'Registration failed. Please try again.',
      );
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
