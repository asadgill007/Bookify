import 'dart:io' show Platform;
import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:google_sign_in/google_sign_in.dart';

class GoogleSignInService {
  /// Android OAuth Client ID (registered in Google Cloud Console with
  /// package name `com.bookify.bookify` and debug SHA-1 fingerprint).
  /// This is public by design — Google OAuth client IDs for mobile apps
  /// are not secrets.
  static const String _androidClientId =
      '243414294987-p6385cttv2erie7nfpcc15m96bphuq7p.apps.googleusercontent.com';

  /// iOS OAuth Client ID — NOT YET CREATED.
  /// When you create an iOS OAuth Client in Google Cloud Console, paste the
  /// client ID here. Until then, Google Sign-In on iOS will degrade gracefully
  /// (the button shows but sign-in returns null).
  static const String _iosClientId = '';

  /// Optional OAuth client ID for web, passed via
  /// `--dart-define=GOOGLE_CLIENT_ID=...`.
  ///
  /// The `google_sign_in_web` plugin REQUIRES a client id on web: constructing
  /// `GoogleSignIn` without one throws an assertion. We therefore create the
  /// instance lazily inside a try/catch so the app never crashes on startup
  /// when Google OAuth is not configured for the build.
  static const String _webClientId = String.fromEnvironment('GOOGLE_CLIENT_ID');

  /// Resolves the correct client ID for the current platform.
  /// - Web: uses `--dart-define=GOOGLE_CLIENT_ID` (required for web)
  /// - Android: uses the hardcoded Android Client ID (registered in Google Cloud Console)
  /// - iOS: uses `_iosClientId` (empty until iOS client is created)
  static String get _clientId {
    if (kIsWeb) return _webClientId;
    if (Platform.isAndroid) return _androidClientId;
    if (Platform.isIOS) return _iosClientId;
    return _webClientId; // fallback
  }

  GoogleSignIn? _googleSignIn;
  bool _initFailed = false;
  GoogleSignInAccount? _currentUser;

  /// Whether a web OAuth client ID was compiled into this build via
  /// `--dart-define=GOOGLE_CLIENT_ID=...`. On web this is mandatory for
  /// Google Sign-In to work at all.
  bool get isConfigured => _clientId.isNotEmpty;

  /// Whether Google Sign-In is usable in this build (configured and the
  /// plugin constructed successfully).
  bool get isAvailable => !_initFailed && _googleSignIn != null && isConfigured;

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
