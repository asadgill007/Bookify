import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/theme/app_theme.dart';
import '../../../core/services/social_auth_service.dart';
import '../providers/auth_provider.dart';

/// Reusable "Sign in with Google" button. Uses the google_sign_in SDK to get
/// an ID token, then authenticates against the backend (which creates or
/// links the account). Google's web OAuth requires a client id; pass it via
/// --dart-define=GOOGLE_CLIENT_ID when running in a browser.
class GoogleSignInButton extends ConsumerStatefulWidget {
  /// Account type used ONLY when the Google account has no Bookify account yet.
  final String accountType;

  const GoogleSignInButton({super.key, this.accountType = AccountType.customer});

  @override
  ConsumerState<GoogleSignInButton> createState() => _GoogleSignInButtonState();
}

class _GoogleSignInButtonState extends ConsumerState<GoogleSignInButton> {
  final _socialAuth = SocialAuthService();
  bool _loading = false;

  Future<void> _handleGoogleSignIn() async {
    if (_loading) return;
    setState(() => _loading = true);

    // On web a client ID is compiled in via --dart-define=GOOGLE_CLIENT_ID.
    // When it is absent the plugin cannot start a flow, so report the real
    // cause instead of a misleading "cancelled" message.
    if (!_socialAuth.isGoogleConfigured) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text(
            'Google sign-in is not configured for this build. '
            'Add a web client ID via --dart-define=GOOGLE_CLIENT_ID.',
          ),
        ),
      );
      setState(() => _loading = false);
      return;
    }

    try {
      final account = await _socialAuth.signInWithGoogle();
      if (account == null) {
        if (!mounted) return;
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Google sign-in was cancelled.')),
        );
        return;
      }

      final idToken = await _socialAuth.getGoogleIdToken();
      if (idToken == null) {
        if (!mounted) return;
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
              content: Text('Could not get a Google ID token. Please try again.')),
        );
        return;
      }

      final ok = await ref
          .read(authProvider.notifier)
          .googleSignIn(idToken: idToken, accountType: widget.accountType);

      if (!mounted) return;
      if (ok) {
        final role = ref.read(authProvider).role;
        final isBusiness = role == 'BusinessOwner' || role == 'Provider';
        context.go(isBusiness ? '/provider-onboarding' : '/');
      }
    } catch (_) {
      if (!mounted) return;
      // On web without a configured OAuth client id google_sign_in throws.
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text(
            'Google sign-in is not configured for this build. '
            'Add a web client ID via --dart-define=GOOGLE_CLIENT_ID.',
          ),
        ),
      );
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isDark = theme.brightness == Brightness.dark;

    return SizedBox(
      width: double.infinity,
      height: 50,
      child: OutlinedButton.icon(
        onPressed: _loading ? null : _handleGoogleSignIn,
        icon: _loading
            ? const SizedBox(
                height: 18,
                width: 18,
                child: CircularProgressIndicator(strokeWidth: 2),
              )
            : const Icon(Icons.g_mobiledata, size: 28, color: Color(0xFF4285F4)),
        label: Text(
          _loading ? 'Signing in...' : 'Sign in with Google',
          style: const TextStyle(fontWeight: FontWeight.w600),
        ),
        style: OutlinedButton.styleFrom(
          foregroundColor: isDark ? Colors.white : AppTheme.slate900,
          side: BorderSide(
            color: isDark ? AppTheme.glassStrokeDark : AppTheme.slate300,
          ),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(AppTheme.radiusFull),
          ),
        ),
      ),
    );
  }
}
