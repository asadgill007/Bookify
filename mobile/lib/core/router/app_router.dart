import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../features/auth/screens/login_screen.dart';
import '../../features/auth/screens/register_screen.dart';
import '../../features/auth/screens/forgot_password_screen.dart';
import '../../features/auth/screens/otp_verification_screen.dart';
import '../../features/auth/providers/auth_provider.dart';
import '../../features/splash/screens/splash_screen.dart';
import '../../features/onboarding/screens/onboarding_screen.dart';
import '../../features/home/screens/home_screen.dart';
import '../../features/categories/screens/categories_screen.dart';
import '../../features/search/screens/search_screen.dart';
import '../../features/profile/screens/profile_screen.dart';
import '../../features/settings/screens/settings_screen.dart';
import '../../features/business/screens/business_detail_screen.dart';
import '../../features/business/providers/businesses_provider.dart';
import '../../features/appointments/screens/appointments_screen.dart';
import '../../features/appointments/screens/booking_screen.dart';
import '../../features/appointments/screens/checkout_screen.dart';
import '../../features/appointments/screens/confirmation_screen.dart';
import '../../features/notifications/screens/notifications_screen.dart';
import '../../features/recurring/screens/my_recurring_bookings_screen.dart';
import '../../features/waitlist/screens/my_waitlist_screen.dart';
import '../../features/provider/screens/provider_onboarding_screen.dart';
import '../../features/provider/screens/my_businesses_screen.dart';
import '../../features/admin/screens/admin_review_screen.dart';
import '../../features/help/screens/help_center_screen.dart';
import '../../features/about/screens/about_screen.dart';
import '../../features/privacy/screens/privacy_screen.dart';
import '../../features/terms/screens/terms_screen.dart';

/// Auth guard redirect logic.
final authGuardProvider = Provider.family<String?, String>((ref, path) {
  final authState = ref.watch(authProvider);
  if (authState.status != AuthStatus.authenticated) {
    return '/login';
  }
  return null;
});

/// GoRouter instance provider with auth guards.
final appRouterProvider = Provider<GoRouter>((ref) {
  final authState = ref.watch(authProvider);

  return GoRouter(
    initialLocation: '/splash',
    debugLogDiagnostics: true,
    redirect: (context, state) {
      final isAuthenticated = authState.status == AuthStatus.authenticated;
      final isAuthRoute = state.matchedLocation == '/login' ||
          state.matchedLocation == '/register' ||
          state.matchedLocation == '/forgot-password' ||
          state.matchedLocation == '/otp-verification' ||
          state.matchedLocation == '/splash' ||
          state.matchedLocation == '/onboarding';

      final protectedRoutes = [
        '/appointments',
        '/profile',
        '/settings',
        '/notifications',
        '/booking',
        '/checkout',
        '/confirmation',
        '/my-business',
        '/provider-onboarding',
        '/admin/review',
        '/my-waitlist',
        '/my-recurring',
      ];

      final isProtectedRoute = protectedRoutes.any(
        (route) => state.matchedLocation.startsWith(route),
      );

      // If not authenticated and trying to access protected route, redirect to login
      if (!isAuthenticated && isProtectedRoute) {
        return '/login';
      }

      // If authenticated and on auth routes, redirect to home
      if (isAuthenticated && isAuthRoute && state.matchedLocation != '/splash') {
        return '/';
      }

      return null;
    },
    routes: [
      GoRoute(
        path: '/splash',
        name: 'splash',
        builder: (context, state) => const SplashScreen(),
      ),
      GoRoute(
        path: '/onboarding',
        name: 'onboarding',
        builder: (context, state) => const OnboardingScreen(),
      ),
      GoRoute(
        path: '/',
        name: 'home',
        builder: (context, state) => const HomeScreen(),
      ),
      GoRoute(
        path: '/login',
        name: 'login',
        builder: (context, state) => const LoginScreen(),
      ),
      GoRoute(
        path: '/register',
        name: 'register',
        builder: (context, state) => const RegisterScreen(),
      ),
      GoRoute(
        path: '/forgot-password',
        name: 'forgot-password',
        builder: (context, state) => const ForgotPasswordScreen(),
      ),
      GoRoute(
        path: '/otp-verification',
        name: 'otp-verification',
        builder: (context, state) => OtpVerificationScreen(
          email: state.extra is String ? state.extra as String : null,
        ),
      ),
      GoRoute(
        path: '/categories',
        name: 'categories',
        builder: (context, state) => const CategoriesScreen(),
      ),
      GoRoute(
        path: '/search',
        name: 'search',
        builder: (context, state) => SearchScreen(
          intent: state.extra is SearchIntent ? state.extra as SearchIntent : null,
        ),
      ),
      GoRoute(
        path: '/provider-onboarding',
        name: 'provider-onboarding',
        builder: (context, state) => const ProviderOnboardingScreen(),
      ),
      GoRoute(
        path: '/my-business',
        name: 'my-business',
        builder: (context, state) => const MyBusinessesScreen(),
      ),
      GoRoute(
        path: '/admin/review',
        name: 'admin-review',
        builder: (context, state) => const AdminReviewScreen(),
      ),
      GoRoute(
        path: '/business/:slug',
        name: 'business-detail',
        builder: (context, state) => BusinessDetailScreen(
          businessSlug: state.pathParameters['slug'] ?? '',
        ),
      ),
      GoRoute(
        path: '/appointments',
        name: 'appointments',
        builder: (context, state) => const AppointmentsScreen(),
      ),
      GoRoute(
        path: '/booking/:businessSlug/:serviceId',
        name: 'booking',
        builder: (context, state) => BookingScreen(
          businessSlug: state.pathParameters['businessSlug'] ?? '',
          serviceId: state.pathParameters['serviceId'] ?? '',
        ),
      ),
      GoRoute(
        path: '/checkout',
        name: 'checkout',
        builder: (context, state) => const CheckoutScreen(),
      ),
      GoRoute(
        path: '/confirmation',
        name: 'confirmation',
        builder: (context, state) => const ConfirmationScreen(),
      ),
      GoRoute(
        path: '/profile',
        name: 'profile',
        builder: (context, state) => const ProfileScreen(),
      ),
      GoRoute(
        path: '/settings',
        name: 'settings',
        builder: (context, state) => const SettingsScreen(),
      ),
      GoRoute(
        path: '/notifications',
        name: 'notifications',
        builder: (context, state) => const NotificationsScreen(),
      ),
      GoRoute(
        path: '/my-waitlist',
        name: 'my-waitlist',
        builder: (context, state) => const MyWaitlistScreen(),
      ),
      GoRoute(
        path: '/my-recurring',
        name: 'my-recurring',
        builder: (context, state) => const MyRecurringBookingsScreen(),
      ),
      GoRoute(
        path: '/help',
        name: 'help',
        builder: (context, state) => const HelpCenterScreen(),
      ),
      GoRoute(
        path: '/about',
        name: 'about',
        builder: (context, state) => const AboutScreen(),
      ),
      GoRoute(
        path: '/privacy',
        name: 'privacy',
        builder: (context, state) => const PrivacyScreen(),
      ),
      GoRoute(
        path: '/terms',
        name: 'terms',
        builder: (context, state) => const TermsScreen(),
      ),
    ],
    errorBuilder: (context, state) => Scaffold(
      appBar: AppBar(title: const Text('Not Found')),
      body: Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.error_outline, size: 64, color: Colors.grey),
            const SizedBox(height: 16),
            Text('Page not found: ${state.uri}'),
            const SizedBox(height: 16),
            FilledButton(
              onPressed: () => context.go('/'),
              child: const Text('Go Home'),
            ),
          ],
        ),
      ),
    ),
  );
});