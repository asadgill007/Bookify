import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../../../core/theme/app_theme.dart';

/// Premium Checkout screen with glassmorphism payment selection.
class CheckoutScreen extends ConsumerStatefulWidget {
  const CheckoutScreen({super.key});

  @override
  ConsumerState<CheckoutScreen> createState() => _CheckoutScreenState();
}

class _CheckoutScreenState extends ConsumerState<CheckoutScreen> {
  String _selectedPayment = 'credit_card';
  final _couponController = TextEditingController();
  bool _isProcessing = false;

  final List<Map<String, dynamic>> _paymentMethods = [
    {'id': 'credit_card', 'name': 'Credit Card', 'icon': Icons.credit_card, 'color': const Color(0xFF6366F1)},
    {'id': 'paypal', 'name': 'PayPal', 'icon': Icons.payments, 'color': const Color(0xFF0070BA)},
    {'id': 'apple_pay', 'name': 'Apple Pay', 'icon': Icons.phone_iphone, 'color': const Color(0xFF000000)},
    {'id': 'google_pay', 'name': 'Google Pay', 'icon': Icons.account_balance_wallet, 'color': const Color(0xFF4285F4)},
  ];

  final _orderDetails = {
    'service': "Women's Haircut & Style",
    'provider': 'Sophia Chen',
    'date': 'Friday, Aug 7',
    'time': '10:30 AM',
    'location': 'Luxe Hair Studio, New York',
    'subtotal': 65.00,
    'serviceFee': 5.00,
    'tax': 8.45,
  };

  @override
  void dispose() {
    _couponController.dispose();
    super.dispose();
  }

  double get _totalPrice => (_orderDetails['subtotal'] as double) + (_orderDetails['serviceFee'] as double) + (_orderDetails['tax'] as double);

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final isDark = theme.brightness == Brightness.dark;

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
                  // ── Order Summary ──
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(16, 8, 16, 8),
                      child: Text('Order Summary', style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700)),
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
                            _buildOrderRow('Service', _orderDetails['service'] as String, theme, colorScheme),
                            const SizedBox(height: 10),
                            _buildOrderRow('Provider', _orderDetails['provider'] as String, theme, colorScheme),
                            const SizedBox(height: 10),
                            _buildOrderRow('Date', _orderDetails['date'] as String, theme, colorScheme),
                            const SizedBox(height: 10),
                            _buildOrderRow('Time', _orderDetails['time'] as String, theme, colorScheme),
                            const SizedBox(height: 10),
                            _buildOrderRow('Location', _orderDetails['location'] as String, theme, colorScheme),
                            const Divider(height: 24, color: AppTheme.slate200),
                            _buildPriceRow('Subtotal', '\$${(_orderDetails['subtotal'] as double).toStringAsFixed(2)}', theme, colorScheme, isBold: false),
                            const SizedBox(height: 6),
                            _buildPriceRow('Service Fee', '\$${(_orderDetails['serviceFee'] as double).toStringAsFixed(2)}', theme, colorScheme, isBold: false),
                            const SizedBox(height: 6),
                            _buildPriceRow('Tax', '\$${(_orderDetails['tax'] as double).toStringAsFixed(2)}', theme, colorScheme, isBold: false),
                            const SizedBox(height: 6),
                            _buildPriceRow('Total', '\$${_totalPrice.toStringAsFixed(2)}', theme, colorScheme, isBold: true),
                          ],
                        ),
                      ),
                    ).animate().fadeIn(duration: 400.ms, delay: 100.ms),
                  ),

                  // ── Coupon Input ──
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
                      child: Text('Promo Code', style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700)),
                    ).animate().fadeIn(duration: 400.ms, delay: 200.ms),
                  ),
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 16),
                      child: GlassContainer(
                        borderRadius: AppTheme.radiusFull,
                        padding: const EdgeInsets.symmetric(horizontal: 4, vertical: 4),
                        child: Row(
                          children: [
                            Expanded(
                              child: TextField(
                                controller: _couponController,
                                decoration: InputDecoration(
                                  hintText: 'Enter coupon code',
                                  hintStyle: TextStyle(color: colorScheme.onSurfaceVariant, fontSize: 14),
                                  prefixIcon: Icon(Icons.confirmation_number_outlined, color: AppTheme.indigoLuxury, size: 20),
                                  border: InputBorder.none,
                                  enabledBorder: InputBorder.none,
                                  focusedBorder: InputBorder.none,
                                  contentPadding: const EdgeInsets.symmetric(vertical: 14),
                                ),
                                style: theme.textTheme.bodyMedium,
                              ),
                            ),
                            Container(
                              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                              decoration: BoxDecoration(
                                gradient: LinearGradient(colors: [AppTheme.indigoLuxury, const Color(0xFF7C3AED)], begin: Alignment.topLeft, end: Alignment.bottomRight),
                                borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                              ),
                              child: const Text('Apply', style: TextStyle(color: Colors.white, fontWeight: FontWeight.w600, fontSize: 14)),
                            ),
                          ],
                        ),
                      ),
                    ).animate().fadeIn(duration: 400.ms, delay: 250.ms),
                  ),

                  // ── Payment Method ──
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(16, 24, 16, 8),
                      child: Text('Payment Method', style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700)),
                    ).animate().fadeIn(duration: 400.ms, delay: 300.ms),
                  ),
                  SliverList(
                    delegate: SliverChildBuilderDelegate(
                      (context, index) {
                        final method = _paymentMethods[index];
                        final isSelected = _selectedPayment == method['id'];
                        return Padding(
                          padding: const EdgeInsets.only(left: 16, right: 16, bottom: 10),
                          child: GestureDetector(
                            onTap: () => setState(() => _selectedPayment = method['id'] as String),
                            child: GlassContainer(
                              borderRadius: AppTheme.radiusLg,
                              padding: const EdgeInsets.all(14),
                              borderSide: isSelected ? BorderSide(color: AppTheme.indigoLuxury, width: 2) : null,
                              child: Row(
                                children: [
                                  Container(
                                    width: 48, height: 48,
                                    decoration: BoxDecoration(
                                      color: (method['color'] as Color).withValues(alpha: 0.1),
                                      borderRadius: BorderRadius.circular(AppTheme.radiusMd),
                                    ),
                                    child: Icon(method['icon'] as IconData, color: method['color'] as Color, size: 24),
                                  ),
                                  const SizedBox(width: 14),
                                  Expanded(
                                    child: Text(method['name'] as String, style: theme.textTheme.titleSmall?.copyWith(fontWeight: FontWeight.w600)),
                                  ),
                                  Container(
                                    width: 22, height: 22,
                                    decoration: BoxDecoration(
                                      shape: BoxShape.circle,
                                      border: Border.all(
                                        color: isSelected ? AppTheme.indigoLuxury : colorScheme.onSurfaceVariant,
                                        width: 2,
                                      ),
                                      color: isSelected ? AppTheme.indigoLuxury : Colors.transparent,
                                    ),
                                    child: isSelected ? const Icon(Icons.check, color: Colors.white, size: 14) : null,
                                  ),
                                ],
                              ),
                            ),
                          ),
                        ).animate().fadeIn(duration: 300.ms, delay: (350 + index * 60).ms);
                      },
                      childCount: _paymentMethods.length,
                    ),
                  ),

                  const SliverToBoxAdapter(child: SizedBox(height: 100)),
                ],
              ),
            ),

            // ── Sticky Confirm Button ──
            Container(
              padding: const EdgeInsets.fromLTRB(16, 12, 16, 24),
              decoration: BoxDecoration(
                gradient: LinearGradient(
                  begin: Alignment.topCenter,
                  end: Alignment.bottomCenter,
                  colors: [
                    isDark ? AppTheme.slate900.withValues(alpha: 0) : AppTheme.slate50.withValues(alpha: 0),
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
                        colors: [AppTheme.indigoLuxury, const Color(0xFF7C3AED)],
                        begin: Alignment.topLeft, end: Alignment.bottomRight,
                      ),
                      borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                      boxShadow: AppTheme.indigoGlowShadow,
                    ),
                    child: MaterialButton(
                      onPressed: _isProcessing ? null : () async {
                        setState(() => _isProcessing = true);
                        await Future.delayed(const Duration(seconds: 1));
                        if (!mounted) return;
                        context.push('/confirmation');
                      },
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(AppTheme.radiusFull)),
                      child: _isProcessing
                          ? const SizedBox(width: 24, height: 24, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                          : Text(
                              'Confirm Booking — \$${_totalPrice.toStringAsFixed(2)}',
                              style: const TextStyle(color: Colors.white, fontSize: 16, fontWeight: FontWeight.w600),
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

  Widget _buildOrderRow(String label, String value, ThemeData theme, ColorScheme colorScheme) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(
          width: 72,
          child: Text(label, style: theme.textTheme.bodySmall?.copyWith(color: colorScheme.onSurfaceVariant, fontWeight: FontWeight.w500)),
        ),
        Expanded(
          child: Text(value, style: theme.textTheme.bodyMedium?.copyWith(fontWeight: FontWeight.w500)),
        ),
      ],
    );
  }

  Widget _buildPriceRow(String label, String value, ThemeData theme, ColorScheme colorScheme, {bool isBold = false}) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(label, style: TextStyle(
          fontSize: isBold ? 16 : 14,
          fontWeight: isBold ? FontWeight.w700 : FontWeight.w400,
          color: isBold ? colorScheme.onSurface : colorScheme.onSurfaceVariant,
        )),
        Text(value, style: TextStyle(
          fontSize: isBold ? 18 : 14,
          fontWeight: isBold ? FontWeight.w700 : FontWeight.w600,
          color: isBold ? AppTheme.indigoLuxury : colorScheme.onSurface,
        )),
      ],
    );
  }
}