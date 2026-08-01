import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../../../core/network/api_client.dart';
import '../../../core/constants/api_constants.dart';
import '../../../core/theme/app_theme.dart';
import '../../settings/providers/app_settings_provider.dart';
import '../../recurring/providers/recurring_bookings_provider.dart';
import 'booking_screen.dart';

/// Appointment result returned by the backend.
class AppointmentResult {
  final String id;
  final String bookingReference;
  final String status;
  final DateTime startTime;
  final DateTime endTime;
  final double totalAmount;
  final String currency;
  final String serviceName;

  const AppointmentResult({
    required this.id,
    required this.bookingReference,
    required this.status,
    required this.startTime,
    required this.endTime,
    required this.totalAmount,
    required this.currency,
    required this.serviceName,
  });

  factory AppointmentResult.fromJson(Map<String, dynamic> json) =>
      AppointmentResult(
        id: json['id'] as String? ?? '',
        bookingReference: json['bookingReference'] as String? ?? '',
        status: json['status'] as String? ?? 'Pending',
        startTime:
            DateTime.tryParse(json['startTime'] as String? ?? '') ?? DateTime.now(),
        endTime: DateTime.tryParse(json['endTime'] as String? ?? '') ?? DateTime.now(),
        totalAmount: (json['totalAmount'] as num?)?.toDouble() ?? 0,
        currency: json['currency'] as String? ?? 'USD',
        serviceName: json['serviceName'] as String? ?? '',
      );
}

/// Premium Checkout screen that creates a real appointment.
class CheckoutScreen extends ConsumerStatefulWidget {
  const CheckoutScreen({super.key});

  @override
  ConsumerState<CheckoutScreen> createState() => _CheckoutScreenState();
}

class _CheckoutScreenState extends ConsumerState<CheckoutScreen> {
  bool _isProcessing = false;
  String? _error;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final isDark = theme.brightness == Brightness.dark;
    final draft = GoRouterState.of(context).extra as BookingDraft?;

    // Currency conversion: display the total in the user's selected currency.
    final settings = ref.watch(appSettingsProvider);
    final currenciesAsync = ref.watch(currenciesProvider);
    final rates = currenciesAsync.valueOrNull ?? fallbackCurrencies;
    final convertedTotal = formatConvertedPrice(
      draft?.price ?? 0,
      draft?.currency ?? 'USD',
      settings.currency,
      rates,
    );

    if (draft == null) {
      return Scaffold(
        appBar: AppBar(title: const Text('Checkout')),
        body: const Center(child: Text('No booking details found.')),
      );
    }

    return GradientBackground(
      child: Scaffold(
        backgroundColor: Colors.transparent,
        appBar: AppBar(
          title: const Text('Checkout'),
          backgroundColor: Colors.transparent,
        ),
        body: Column(
          children: [
            Expanded(
              child: CustomScrollView(
                physics: const BouncingScrollPhysics(),
                slivers: [
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(16, 8, 16, 8),
                      child: Text('Order Summary',
                          style: theme.textTheme.titleMedium?.copyWith(
                              fontWeight: FontWeight.w700)),
                    ).animate().fadeIn(duration: 400.ms),
                  ),
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 16),
                      child: GlassContainer(
                        borderRadius: AppTheme.radiusLg,
                        padding: const EdgeInsets.all(16),
                        child: Column(
                          children: [
                            _buildOrderRow(
                                'Service', draft.serviceName, theme, colorScheme),
                            const SizedBox(height: 10),
                            _buildOrderRow(
                                'Provider', draft.providerName, theme, colorScheme),
                            const SizedBox(height: 10),
                            _buildOrderRow('Business', draft.businessName,
                                theme, colorScheme),
                            const SizedBox(height: 10),
                            _buildOrderRow(
                                'Date',
                                '${draft.startTime.day}/${draft.startTime.month}/${draft.startTime.year}',
                                theme,
                                colorScheme),
                            const SizedBox(height: 10),
                            _buildOrderRow(
                                'Time',
                                '${_formatTime(draft.startTime)} – ${_formatTime(draft.endTime)}',
                                theme,
                                colorScheme),
                            const Divider(height: 24, color: AppTheme.slate200),
                            _buildPriceRow(
                                'Total',
                                convertedTotal,
                                theme,
                                colorScheme,
                                isBold: true),
                          ],
                        ),
                      ),
                    ),
                  ).animate().fadeIn(duration: 400.ms, delay: 100.ms),

                  if (_error != null)
                    SliverToBoxAdapter(
                      child: Padding(
                        padding: const EdgeInsets.fromLTRB(16, 16, 16, 0),
                        child: Container(
                          padding: const EdgeInsets.all(12),
                          decoration: BoxDecoration(
                            color: Colors.red.withValues(alpha: 0.1),
                            borderRadius: BorderRadius.circular(12),
                            border: Border.all(
                                color: Colors.red.withValues(alpha: 0.2)),
                          ),
                          child: Row(
                            children: [
                              Icon(Icons.error_outline,
                                  color: Colors.red.shade400, size: 20),
                              const SizedBox(width: 8),
                              Expanded(
                                child: Text(
                                  _error!,
                                  style: theme.textTheme.bodySmall
                                      ?.copyWith(color: Colors.red.shade400),
                                ),
                              ),
                            ],
                          ),
                        ),
                      ),
                    ),
                  const SliverToBoxAdapter(child: SizedBox(height: 100)),
                ],
              ),
            ),

            // Sticky Confirm button
            Container(
              padding: const EdgeInsets.fromLTRB(16, 12, 16, 24),
              decoration: BoxDecoration(
                gradient: LinearGradient(
                  begin: Alignment.topCenter,
                  end: Alignment.bottomCenter,
                  colors: [
                    isDark
                        ? AppTheme.slate900.withValues(alpha: 0)
                        : AppTheme.slate50.withValues(alpha: 0),
                    isDark ? AppTheme.slate900 : AppTheme.slate50,
                  ],
                ),
              ),
              child: SafeArea(
                top: false,
                child: SizedBox(
                  width: double.infinity,
                  height: 56,
                  child: DecoratedBox(
                    decoration: BoxDecoration(
                      gradient: LinearGradient(
                        colors: [
                          AppTheme.indigoLuxury,
                          const Color(0xFF7C3AED),
                        ],
                        begin: Alignment.topLeft,
                        end: Alignment.bottomRight,
                      ),
                      borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                      boxShadow: AppTheme.indigoGlowShadow,
                    ),
                    child: MaterialButton(
                      onPressed: _isProcessing ? null : () => _confirm(draft),
                      shape: RoundedRectangleBorder(
                          borderRadius:
                              BorderRadius.circular(AppTheme.radiusFull)),
                      child: _isProcessing
                          ? const SizedBox(
                              width: 24,
                              height: 24,
                              child: CircularProgressIndicator(
                                  color: Colors.white, strokeWidth: 2),
                            )
                          : Text(
                              'Confirm Booking — $convertedTotal',
                              style: const TextStyle(
                                  color: Colors.white,
                                  fontSize: 16,
                                  fontWeight: FontWeight.w600),
                            ),
                    ),
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _confirm(BookingDraft draft) async {
    setState(() {
      _isProcessing = true;
      _error = null;
    });
    try {
      final api = ref.read(apiClientProvider);

      // Recurring booking: create the series, not a single appointment.
      if (draft.recurrence != null) {
        final rec = draft.recurrence!;
        final recurringApi = ref.read(recurringBookingsApiProvider);
        final seriesStart = draft.startTime;
        await recurringApi.create(
          providerId: draft.providerId,
          serviceId: draft.serviceId,
          businessId: draft.businessId,
          recurrenceType: rec.type,
          startTime: _timeOnly(draft.startTime),
          endTime: _timeOnly(draft.endTime),
          seriesStartDate: seriesStart,
          maxOccurrences: rec.maxOccurrences,
          seriesEndDate: rec.endDate,
          interval: rec.interval,
          notes: null,
        );

        final result = AppointmentResult(
          id: '',
          bookingReference: 'RECURRING',
          status: 'Confirmed',
          startTime: draft.startTime,
          endTime: draft.endTime,
          totalAmount: draft.price * (rec.maxOccurrences ?? 1),
          currency: draft.currency,
          serviceName: draft.serviceName,
        );
        if (!mounted) return;
        context.pushReplacement('/confirmation', extra: result);
        return;
      }

      final response = await api.post(
        ApiConstants.appointments,
        data: {
          'providerId': draft.providerId,
          'serviceId': draft.serviceId,
          'businessId': draft.businessId,
          'startTime': draft.startTime.toIso8601String(),
          'endTime': draft.endTime.toIso8601String(),
        },
      );

      final body = response.data as Map<String, dynamic>;
      final data = (body['data'] ?? body) as Map<String, dynamic>;
      final result = AppointmentResult.fromJson(data);

      if (!mounted) return;
      context.pushReplacement('/confirmation', extra: result);
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _isProcessing = false;
        _error = 'Booking failed. Please try another time slot. $e';
      });
    }
  }

  String _timeOnly(DateTime dt) {
    return '${dt.hour.toString().padLeft(2, '0')}:'
        '${dt.minute.toString().padLeft(2, '0')}:00';
  }

  String _formatTime(DateTime dt) {
    final h = dt.hour.toString().padLeft(2, '0');
    final m = dt.minute.toString().padLeft(2, '0');
    return '$h:$m';
  }

  Widget _buildOrderRow(
      String label, String value, ThemeData theme, ColorScheme colorScheme) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(
          width: 72,
          child: Text(label,
              style: theme.textTheme.bodySmall?.copyWith(
                  color: colorScheme.onSurfaceVariant,
                  fontWeight: FontWeight.w500)),
        ),
        Expanded(
          child: Text(value,
              style: theme.textTheme.bodyMedium?.copyWith(
                  fontWeight: FontWeight.w500)),
        ),
      ],
    );
  }

  Widget _buildPriceRow(String label, String value, ThemeData theme,
      ColorScheme colorScheme,
      {bool isBold = false}) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(label,
            style: TextStyle(
              fontSize: isBold ? 16 : 14,
              fontWeight: isBold ? FontWeight.w700 : FontWeight.w400,
              color:
                  isBold ? colorScheme.onSurface : colorScheme.onSurfaceVariant,
            )),
        Text(value,
            style: TextStyle(
              fontSize: isBold ? 18 : 14,
              fontWeight: isBold ? FontWeight.w700 : FontWeight.w600,
              color: isBold ? AppTheme.indigoLuxury : colorScheme.onSurface,
            )),
      ],
    );
  }
}
