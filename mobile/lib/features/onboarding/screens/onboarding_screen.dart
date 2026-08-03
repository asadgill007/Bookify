import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:smooth_page_indicator/smooth_page_indicator.dart';
import '../../../core/theme/app_theme.dart';

/// Premium Animated Onboarding screen with glassmorphism design.
class OnboardingScreen extends ConsumerStatefulWidget {
  const OnboardingScreen({super.key});

  @override
  ConsumerState<OnboardingScreen> createState() => _OnboardingScreenState();
}

class _OnboardingScreenState extends ConsumerState<OnboardingScreen> {
  final _pageController = PageController();
  int _currentPage = 0;

  final List<_OnboardingPage> _pages = [
    _OnboardingPage(
      icon: Icons.spa,
      title: 'Discover Premium\nServices',
      description: 'Browse hundreds of top-rated services from verified professionals. Find exactly what you need.',
      color: const Color(0xFF6366F1),
      gradientColors: [const Color(0xFF6366F1), const Color(0xFF8B5CF6)],
    ),
    _OnboardingPage(
      icon: Icons.calendar_month,
      title: 'Book Instantly,\nAnytime',
      description: 'Schedule appointments in seconds. View real-time availability and book your preferred time slot.',
      color: const Color(0xFF8B5CF6),
      gradientColors: [const Color(0xFF8B5CF6), const Color(0xFFA78BFA)],
    ),
    _OnboardingPage(
      icon: Icons.payments,
      title: 'Secure Payments\n& Rewards',
      description: 'Pay securely with multiple payment methods. Earn loyalty points and redeem exclusive rewards.',
      color: const Color(0xFF06B6D4),
      gradientColors: [const Color(0xFF06B6D4), const Color(0xFF22D3EE)],
    ),
    _OnboardingPage(
      icon: Icons.qr_code,
      title: 'Digital Tickets\n& QR Access',
      description: 'No more paper tickets. Access your bookings with a simple QR scan. Manage everything from your phone.',
      color: const Color(0xFF10B981),
      gradientColors: [const Color(0xFF10B981), const Color(0xFF34D399)],
    ),
  ];

  @override
  void dispose() {
    _pageController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isDark = theme.brightness == Brightness.dark;

    return GradientBackground(
      child: Scaffold(
        backgroundColor: Colors.transparent,
        body: SafeArea(
          child: Column(
            children: [
              // ── Skip Button ──
              Padding(
                padding: const EdgeInsets.fromLTRB(16, 8, 16, 0),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.end,
                  children: [
                    TextButton(
                      onPressed: () => context.go('/'),
                      child: Text(
                        'Skip',
                        style: TextStyle(
                          color: isDark ? AppTheme.slate300 : AppTheme.slate500,
                          fontWeight: FontWeight.w500,
                        ),
                      ),
                    ).animate().fadeIn(duration: 400.ms),
                  ],
                ),
              ),

              // ── Page View ──
              Expanded(
                child: PageView.builder(
                  controller: _pageController,
                  onPageChanged: (index) => setState(() => _currentPage = index),
                  itemCount: _pages.length,
                  itemBuilder: (context, index) {
                    final page = _pages[index];
                    return Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 32),
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          // ── Animated Icon Container ──
                          AnimatedContainer(
                            duration: 400.ms,
                            width: 180,
                            height: 180,
                            decoration: BoxDecoration(
                              shape: BoxShape.circle,
                              gradient: LinearGradient(
                                colors: page.gradientColors,
                                begin: Alignment.topLeft,
                                end: Alignment.bottomRight,
                              ),
                              boxShadow: [
                                BoxShadow(
                                  color: page.color.withValues(alpha: 0.3),
                                  blurRadius: 40,
                                  spreadRadius: 5,
                                ),
                              ],
                            ),
                            child: Icon(
                              page.icon,
                              size: 72,
                              color: Colors.white,
                            ),
                          ).animate().fadeIn(
                            duration: 600.ms,
                            delay: 200.ms,
                          ).scale(
                            begin: const Offset(0.6, 0.6),
                            curve: Curves.elasticOut,
                          ),

                          const SizedBox(height: 48),

                          // ── Title ──
                          Text(
                            page.title,
                            textAlign: TextAlign.center,
                            style: theme.textTheme.headlineMedium?.copyWith(
                              fontWeight: FontWeight.w700,
                              height: 1.3,
                            ),
                          ).animate().fadeIn(
                            duration: 500.ms,
                            delay: 400.ms,
                          ).slideY(begin: 0.3, curve: Curves.easeOutCubic),

                          const SizedBox(height: 16),

                          // ── Description ──
                          Text(
                            page.description,
                            textAlign: TextAlign.center,
                            style: theme.textTheme.bodyLarge?.copyWith(
                              color: isDark ? AppTheme.slate400 : AppTheme.slate500,
                              height: 1.6,
                            ),
                          ).animate().fadeIn(
                            duration: 500.ms,
                            delay: 600.ms,
                          ).slideY(begin: 0.2, curve: Curves.easeOutCubic),
                        ],
                      ),
                    );
                  },
                ),
              ),

              // ── Page Indicator & Buttons ──
              Padding(
                padding: const EdgeInsets.fromLTRB(24, 0, 24, 24),
                child: Column(
                  children: [
                    // ── Page Indicator ──
                    SmoothPageIndicator(
                      controller: _pageController,
                      count: _pages.length,
                      effect: ExpandingDotsEffect(
                        dotHeight: 8,
                        dotWidth: 8,
                        activeDotColor: AppTheme.indigoLuxury,
                        dotColor: isDark ? AppTheme.slate700 : AppTheme.slate300,
                        expansionFactor: 3,
                        spacing: 8,
                      ),
                    ).animate().fadeIn(duration: 400.ms, delay: 300.ms),

                    const SizedBox(height: 32),

                    // ── Get Started / Next Button ──
                    SizedBox(
                      width: double.infinity,
                      height: 56,
                      child: DecoratedBox(
                        decoration: BoxDecoration(
                          gradient: LinearGradient(
                            colors: [AppTheme.indigoLuxury, const Color(0xFF7C3AED)],
                            begin: Alignment.topLeft,
                            end: Alignment.bottomRight,
                          ),
                          borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                          boxShadow: AppTheme.indigoGlowShadow,
                        ),
                        child: MaterialButton(
                          onPressed: () {
                            if (_currentPage < _pages.length - 1) {
                              _pageController.nextPage(
                                duration: 400.ms,
                                curve: Curves.easeInOutCubic,
                              );
                            } else {
                              context.go('/');
                            }
                          },
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                          ),
                          child: Text(
                            _currentPage < _pages.length - 1 ? 'Next' : 'Get Started',
                            style: const TextStyle(
                              color: Colors.white,
                              fontSize: 16,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ),
                      ),
                    ).animate().fadeIn(duration: 400.ms, delay: 500.ms),

                    const SizedBox(height: 16),

                    // ── Already have an account? ──
                    if (_currentPage == _pages.length - 1)
                      Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Text(
                            'Already have an account? ',
                            style: TextStyle(
                              color: isDark ? AppTheme.slate400 : AppTheme.slate500,
                              fontSize: 14,
                            ),
                          ),
                          TextButton(
                            onPressed: () => context.push('/login'),
                            child: const Text(
                              'Sign In',
                              style: TextStyle(
                                fontWeight: FontWeight.w600,
                                color: AppTheme.indigoLuxury,
                              ),
                            ),
                          ),
                        ],
                      ).animate().fadeIn(duration: 400.ms, delay: 600.ms),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _OnboardingPage {
  final IconData icon;
  final String title;
  final String description;
  final Color color;
  final List<Color> gradientColors;

  const _OnboardingPage({
    required this.icon,
    required this.title,
    required this.description,
    required this.color,
    required this.gradientColors,
  });
}