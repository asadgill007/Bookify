import 'package:google_sign_in/google_sign_in.dart';

class GoogleSignInService {
  /// Optional OAuth client ID for web, passed via
  /// `--dart-define=GOOGLE_CLIENT_ID=...`.
  ///
  /// The `google_sign_in_web` plugin REQUIRES a client id on web: constructing
  /// `GoogleSignIn` without one throws an assertion. We therefore create the
  /// instance lazily inside a try/catch so the app never crashes on startup
  /// when Google OAuth is not configured for the build.
  static const String _clientId = String.fromEnvironment('GOOGLE_CLIENT_ID');

  GoogleSignIn? _googleSignIn;
  bool _initFailed = false;
  GoogleSignInAccount? _currentUser;

  /// Lazily built (and guarded) GoogleSignIn instance. Construction on web
  /// throws when no client id is configured, so it is wrapped and the failure
  /// is cached to avoid retrying on every call.
  GoogleSignIn? get _instance {
    if (_initFailed) return null;
    if (_googleSignIn == null) {
      try {
        _googleSignIn = GoogleSignIn(
          clientId: _clientId.isEmpty ? null : _clientId,
          scopes: [
            'email',
            'profile',
          ],
        );
      } catch (_) {
        // google_sign_in_web throws when no client id is configured. Cache the
        // failure so every method below degrades to "not signed in" instead
        // of throwing.
        _initFailed = true;
      }
    }
    return _googleSignIn;
  }

  /// Get current signed-in user
  GoogleSignInAccount? get currentUser => _currentUser;

  /// Check if user is already signed in
  bool get isSignedIn => _currentUser != null;

  /// Sign in with Google
  Future<GoogleSignInAccount?> signIn() async {
    final instance = _instance;
    if (instance == null) return null;
    try {
      _currentUser = await instance.signIn();
      return _currentUser;
    } catch (e) {
      return null;
    }
  }

  /// Sign out
  Future<void> signOut() async {
    final instance = _instance;
    if (instance == null) return;
    try {
      await instance.signOut();
      _currentUser = null;
    } catch (e) {
      // Ignore errors
    }
  }

  /// Disconnect (revoke access)
  Future<void> disconnect() async {
    final instance = _instance;
    if (instance == null) return;
    try {
      await instance.disconnect();
      _currentUser = null;
    } catch (e) {
      // Ignore errors
    }
  }

  /// Get authentication token
  Future<String?> getAuthToken() async {
    final instance = _instance;
    if (instance == null) return null;
    try {
      final account = instance.currentUser;
      if (account == null) return null;

      final auth = await account.authentication;
      return auth.accessToken;
    } catch (e) {
      return null;
    }
  }

  /// Get ID token (for backend authentication)
  Future<String?> getIdToken() async {
    final instance = _instance;
    if (instance == null) return null;
    try {
      final account = instance.currentUser;
      if (account == null) return null;

      final auth = await account.authentication;
      return auth.idToken;
    } catch (e) {
      return null;
    }
  }

  /// Initialize and check for existing sign-in
  Future<bool> initialize() async {
    final instance = _instance;
    if (instance == null) return false;
    try {
      _currentUser = instance.currentUser;
      return _currentUser != null;
    } catch (e) {
      return false;
    }
  }
}
