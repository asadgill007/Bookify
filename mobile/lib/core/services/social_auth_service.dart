import 'package:google_sign_in/google_sign_in.dart';
import 'google_sign_in_service.dart';

class SocialAuthService {
  final GoogleSignInService _googleSignInService = GoogleSignInService();

  GoogleSignInAccount? get currentUser => _googleSignInService.currentUser;
  bool get isSignedIn => _googleSignInService.isSignedIn;

  /// Whether a web OAuth client ID was compiled into this build. On web,
  /// Google Sign-In cannot start without it.
  bool get isGoogleConfigured => _googleSignInService.isConfigured;

  /// Whether Google Sign-In can actually start a flow in this build.
  bool get isGoogleAvailable => _googleSignInService.isAvailable;

  Future<GoogleSignInAccount?> signInWithGoogle() async {
    return await _googleSignInService.signIn();
  }

  Future<void> signOut() async {
    await _googleSignInService.signOut();
  }

  Future<String?> getGoogleIdToken() async {
    return await _googleSignInService.getIdToken();
  }

  Future<String?> getGoogleAccessToken() async {
    return await _googleSignInService.getAuthToken();
  }

  Future<bool> initialize() async {
    return await _googleSignInService.initialize();
  }
}