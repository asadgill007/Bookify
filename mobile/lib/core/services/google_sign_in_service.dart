import 'package:google_sign_in/google_sign_in.dart';

class GoogleSignInService {
  final GoogleSignIn _googleSignIn = GoogleSignIn(
    scopes: [
      'email',
      'profile',
    ],
  );

  GoogleSignInAccount? _currentUser;

  /// Get current signed-in user
  GoogleSignInAccount? get currentUser => _currentUser;

  /// Check if user is already signed in
  bool get isSignedIn => _currentUser != null;

  /// Sign in with Google
  Future<GoogleSignInAccount?> signIn() async {
    try {
      _currentUser = await _googleSignIn.signIn();
      return _currentUser;
    } catch (e) {
      return null;
    }
  }

  /// Sign out
  Future<void> signOut() async {
    try {
      await _googleSignIn.signOut();
      _currentUser = null;
    } catch (e) {
      // Ignore errors
    }
  }

  /// Disconnect (revoke access)
  Future<void> disconnect() async {
    try {
      await _googleSignIn.disconnect();
      _currentUser = null;
    } catch (e) {
      // Ignore errors
    }
  }

  /// Get authentication token
  Future<String?> getAuthToken() async {
    try {
      final account = _googleSignIn.currentUser;
      if (account == null) return null;

      final auth = await account.authentication;
      return auth.accessToken;
    } catch (e) {
      return null;
    }
  }

  /// Get ID token (for backend authentication)
  Future<String?> getIdToken() async {
    try {
      final account = _googleSignIn.currentUser;
      if (account == null) return null;

      final auth = await account.authentication;
      return auth.idToken;
    } catch (e) {
      return null;
    }
  }

  /// Initialize and check for existing sign-in
  Future<bool> initialize() async {
    try {
      _currentUser = _googleSignIn.currentUser;
      return _currentUser != null;
    } catch (e) {
      return false;
    }
  }
}