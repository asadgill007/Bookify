import 'package:bookify/core/constants/api_constants.dart';
import 'package:bookify/core/network/api_client.dart';
import 'package:bookify/features/auth/providers/auth_provider.dart';
import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:flutter_test/flutter_test.dart';

/// Fake [ApiClient] that stubs the Google auth endpoint and token storage so
/// the [AuthNotifier] can be tested without network or platform channels.
class FakeApiClient extends ApiClient {
  FakeApiClient() : super(const FlutterSecureStorage());

  bool failWithServerError = false;
  bool failWithNetworkError = false;
  String? savedAccessToken;
  String? savedRefreshToken;

  @override
  Future<String?> getAccessToken() async => null;

  @override
  Future<void> saveTokens(String accessToken, String refreshToken) async {
    savedAccessToken = accessToken;
    savedRefreshToken = refreshToken;
  }

  @override
  Future<Response<T>> post<T>(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
    CancelToken? cancelToken,
  }) async {
    if (path == ApiConstants.googleLogin) {
      if (failWithServerError) {
        throw DioException(
          requestOptions: RequestOptions(path: path),
          response: Response<Map<String, dynamic>>(
            requestOptions: RequestOptions(path: path),
            statusCode: 400,
            data: {
              'success': false,
              'message':
                  "Google sign-in failed: the account's email is not verified.",
            },
          ),
          type: DioExceptionType.badResponse,
        );
      }
      if (failWithNetworkError) {
        throw DioException(
          requestOptions: RequestOptions(path: path),
          type: DioExceptionType.connectionError,
        );
      }
      return Response<Map<String, dynamic>>(
        requestOptions: RequestOptions(path: path),
        statusCode: 200,
        data: {
          'data': {
            'userId': 'user-1',
            'email': 'john@gmail.com',
            'role': 'Customer',
            'accessToken': 'access-token',
            'refreshToken': 'refresh-token',
          },
        },
      ) as Response<T>;
    }
    throw UnimplementedError('Unexpected path: $path');
  }
}

void main() {
  late FakeApiClient api;
  late ProviderContainer container;

  setUp(() {
    api = FakeApiClient();
    container = ProviderContainer(
      overrides: [apiClientProvider.overrideWithValue(api)],
    );
    addTearDown(container.dispose);
  });

  test('googleSignIn success stores tokens and sets authenticated state',
      () async {
    final notifier = container.read(authProvider.notifier);

    final ok = await notifier.googleSignIn(idToken: 'valid-id-token');

    expect(ok, isTrue);
    final state = container.read(authProvider);
    expect(state.status, AuthStatus.authenticated);
    expect(state.email, 'john@gmail.com');
    expect(state.role, 'Customer');
    expect(state.userId, 'user-1');
    expect(state.error, isNull);
    expect(api.savedAccessToken, 'access-token');
    expect(api.savedRefreshToken, 'refresh-token');
  });

  test('googleSignIn backend error surfaces the server message', () async {
    api.failWithServerError = true;
    final notifier = container.read(authProvider.notifier);

    final ok = await notifier.googleSignIn(idToken: 'unverified-email');

    expect(ok, isFalse);
    final state = container.read(authProvider);
    expect(state.status, AuthStatus.unauthenticated);
    expect(state.error, contains('email is not verified'));
  });

  test('googleSignIn network failure shows generic error message', () async {
    api.failWithNetworkError = true;
    final notifier = container.read(authProvider.notifier);

    final ok = await notifier.googleSignIn(idToken: 'bad-token');

    expect(ok, isFalse);
    final state = container.read(authProvider);
    expect(state.status, AuthStatus.unauthenticated);
    expect(state.error, 'Google sign-in failed. Please try again.');
  });
}
