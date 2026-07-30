import 'package:google_sign_in/google_sign_in.dart';
import 'google_sign_in_service.dart';

class SocialAuthService {
  final GoogleSignInService _googleSignInService = GoogleSignInService();

  GoogleSignInAccount? get currentUser => _googleSignInService.currentUser;
  bool get isSignedIn => _googleSignInService.isSignedIn;

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