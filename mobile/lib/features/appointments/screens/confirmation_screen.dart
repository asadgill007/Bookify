import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../../../core/theme/app_theme.dart';
import 'checkout_screen.dart';

/// Premium Booking Confirmation screen with digital ticket / QR code style.
class ConfirmationScreen extends ConsumerWidget {
  const ConfirmationScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    // Booking result is passed via the router as extra.
    final result = GoRouterState.of(context).extra as AppointmentResult?;
    final bookingRef = result?.bookingReference ?? 'BK-PENDING';
    final serviceName = result?.serviceName ?? 'Appointment';
    final amount = result?.totalAmount ?? 0;
    final currency = result?.currency ?? 'USD';
    final startTime = result?.startTime;

    final weekdays = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
    final months = [
      'January', 'February', 'March', 'April', 'May', 'June',
      'July', 'August', 'September', 'October', 'November', 'December',
    ];
    final dateLabel = startTime != null
        ? '${weekdays[startTime.weekday - 1]}, '
            '${months[startTime.month - 1]} ${startTime.day}, ${startTime.year}'
        : '';
    final timeLabel = startTime != null
        ? '${startTime.hour.toString().padLeft(2, '0')}:'
            '${startTime.minute.toString().padLeft(2, '0')}'
        : '';

    return GradientBackground(
      child: Scaffold(
        backgroundColor: Colors.transparent,
        body: SafeArea(
          child: CustomScrollView(
            physics: const BouncingScrollPhysics(),
            slivers: [
              const SliverToBoxAdapter(child: SizedBox(height: 40)),

              // ── Success Animation ──
              SliverToBoxAdapter(
                child: Center(
                  child: Container(
                    width: 100,
                    height: 100,
                    decoration: BoxDecoration(
                      shape: BoxShape.circle,
                      gradient: LinearGradient(
                        colors: [AppTheme.success, const Color(0xFF34D399)],
                        begin: Alignment.topLeft,
                        end: Alignment.bottomRight,
                      ),
                      boxShadow: [
                        BoxShadow(
                          color: AppTheme.success.withValues(alpha: 0.3),
                          blurRadius: 30,
                          spreadRadius: 5,
                        ),
                      ],
                    ),
                    child: const Icon(Icons.check, color: Colors.white, size: 48),
                  ),
                ).animate().scale(
                  duration: 600.ms,
                  begin: const Offset(0, 0),
                  curve: Curves.elasticOut,
                ).then().shake(),
              ),
              const SliverToBoxAdapter(child: SizedBox(height: 24)),

              // ── Success Text ──
              SliverToBoxAdapter(
                child: Center(
                  child: Text(
                    'Booking Confirmed!',
                    style: theme.textTheme.headlineSmall?.copyWith(
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                ).animate().fadeIn(duration: 400.ms, delay: 400.ms).slideY(begin: 0.2),
              ),
              const SliverToBoxAdapter(child: SizedBox(height: 8)),
              SliverToBoxAdapter(
                child: Center(
                  child: Text(
                    'Your appointment has been confirmed',
                    style: theme.textTheme.bodyMedium?.copyWith(
                      color: colorScheme.onSurfaceVariant,
                    ),
                  ),
                ).animate().fadeIn(duration: 400.ms, delay: 500.ms),
              ),
              const SliverToBoxAdapter(child: SizedBox(height: 32)),

              // ── Digital Ticket ──
              SliverToBoxAdapter(
                child: Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 24),
                  child: GlassContainer(
                    borderRadius: AppTheme.radiusXl,
                    padding: EdgeInsets.zero,
                    child: Column(
                      children: [
                        // ── Ticket Header ──
                        Container(
                          width: double.infinity,
                          padding: const EdgeInsets.all(20),
                          decoration: BoxDecoration(
                            gradient: LinearGradient(
                              colors: [AppTheme.indigoLuxury, const Color(0xFF7C3AED)],
                              begin: Alignment.topLeft,
                              end: Alignment.bottomRight,
                            ),
                            borderRadius: BorderRadius.vertical(
                              top: Radius.circular(AppTheme.radiusXl),
                            ),
                          ),
                          child: Column(
                            children: [
                              // ── QR Code Placeholder ──
                              Container(
                                width: 120,
                                height: 120,
                                padding: const EdgeInsets.all(16),
                                decoration: BoxDecoration(
                                  color: Colors.white,
                                  borderRadius: BorderRadius.circular(AppTheme.radiusLg),
                                ),
                                child: Center(
                                  child: Icon(
                                    Icons.qr_code_rounded,
                                    size: 80,
                                    color: AppTheme.indigoLuxury.withValues(alpha: 0.8),
                                  ),
                                ),
                              ),
                              const SizedBox(height: 16),
                              Text(
                                bookingRef,
                                style: const TextStyle(
                                  color: Colors.white,
                                  fontSize: 18,
                                  fontWeight: FontWeight.w700,
                                  letterSpacing: 2,
                                ),
                              ),
                              const SizedBox(height: 4),
                              Text(
                                'Booking Reference',
                                style: TextStyle(
                                  color: Colors.white.withValues(alpha: 0.7),
                                  fontSize: 12,
                                ),
                              ),
                            ],
                          ),
                        ),

                        // ── Ticket Details ──
                        Padding(
                          padding: const EdgeInsets.all(20),
                          child: Column(
                            children: [
                              _buildTicketRow(Icons.content_cut, 'Service', serviceName, theme, colorScheme),
                              const SizedBox(height: 16),
                              _buildTicketRow(Icons.person, 'Provider', 'Your provider', theme, colorScheme),
                              const SizedBox(height: 16),
                              _buildTicketRow(Icons.calendar_today, 'Date', dateLabel, theme, colorScheme),
                              const SizedBox(height: 16),
                              _buildTicketRow(Icons.access_time, 'Time', timeLabel, theme, colorScheme),
                              const SizedBox(height: 16),
                              _buildTicketRow(Icons.attach_money, 'Amount Paid', '${currency == 'USD' ? '\$' : ''}${amount.toStringAsFixed(2)}', theme, colorScheme, isPrice: true),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                ).animate().fadeIn(duration: 500.ms, delay: 600.ms).slideY(begin: 0.3, curve: Curves.easeOutCubic),
              ),

              const SliverToBoxAdapter(child: SizedBox(height: 32)),

              // ── Action Buttons ──
              SliverToBoxAdapter(
                child: Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 24),
                  child: Row(
                    children: [
                      Expanded(
                        child: GlassContainer(
                          borderRadius: AppTheme.radiusFull,
                          padding: EdgeInsets.zero,
                          child: OutlinedButton.icon(
                            onPressed: () {},
                            icon: const Icon(Icons.calendar_month_outlined, size: 20),
                            label: const Text('Add to Calendar'),
                            style: OutlinedButton.styleFrom(
                              foregroundColor: AppTheme.indigoLuxury,
                              padding: const EdgeInsets.symmetric(vertical: 14),
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                                side: BorderSide(color: AppTheme.indigoLuxury.withValues(alpha: 0.3), width: 1.5),
                              ),
                            ),
                          ),
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: GlassContainer(
                          borderRadius: AppTheme.radiusFull,
                          padding: EdgeInsets.zero,
                          child: OutlinedButton.icon(
                            onPressed: () {},
                            icon: const Icon(Icons.share_outlined, size: 20),
                            label: const Text('Share'),
                            style: OutlinedButton.styleFrom(
                              foregroundColor: AppTheme.indigoLuxury,
                              padding: const EdgeInsets.symmetric(vertical: 14),
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                                side: BorderSide(color: AppTheme.indigoLuxury.withValues(alpha: 0.3), width: 1.5),
                              ),
                            ),
                          ),
                        ),
                      ),
                    ],
                  ),
                ).animate().fadeIn(duration: 400.ms, delay: 800.ms),
              ),

              const SliverToBoxAdapter(child: SizedBox(height: 24)),

              // ── Back to Home Button ──
              SliverToBoxAdapter(
                child: Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 24),
                  child: SizedBox(
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
                        onPressed: () => context.go('/'),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                        ),
                        child: const Text(
                          'Back to Home',
                          style: TextStyle(
                            color: Colors.white,
                            fontSize: 16,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ),
                    ),
                  ),
                ).animate().fadeIn(duration: 400.ms, delay: 900.ms),
              ),

              const SliverToBoxAdapter(child: SizedBox(height: 40)),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildTicketRow(
    IconData icon,
    String label,
    String value,
    ThemeData theme,
    ColorScheme colorScheme, {
    bool isPrice = false,
  }) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Container(
          width: 36,
          height: 36,
          decoration: BoxDecoration(
            color: AppTheme.indigoLuxury.withValues(alpha: 0.1),
            borderRadius: BorderRadius.circular(AppTheme.radiusSm),
          ),
          child: Icon(icon, size: 18, color: AppTheme.indigoLuxury),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                label,
                style: theme.textTheme.bodySmall?.copyWith(
                  color: colorScheme.onSurfaceVariant,
                  fontSize: 11,
                ),
              ),
              const SizedBox(height: 2),
              Text(
                value,
                style: theme.textTheme.bodyMedium?.copyWith(
                  fontWeight: isPrice ? FontWeight.w700 : FontWeight.w600,
                  color: isPrice ? AppTheme.indigoLuxury : colorScheme.onSurface,
                  fontSize: isPrice ? 16 : 14,
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}