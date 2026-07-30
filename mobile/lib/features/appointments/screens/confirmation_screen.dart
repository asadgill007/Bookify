import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:flutter_animate/flutter_animate.dart';

class ConfirmationScreen extends ConsumerWidget {
  const ConfirmationScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;

    return Scaffold(
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Spacer(),
              Container(
                width: 100,
                height: 100,
                decoration: BoxDecoration(
                  color: Colors.green.withValues(alpha: 0.1),
                  shape: BoxShape.circle,
                ),
                child: const Icon(Icons.check_circle_rounded, size: 56, color: Colors.green),
              ).animate().scale(duration: 600.ms, curve: Curves.elasticOut),
              const SizedBox(height: 24),
              Text(
                'Booking Confirmed!',
                style: theme.textTheme.headlineMedium?.copyWith(fontWeight: FontWeight.bold),
                textAlign: TextAlign.center,
              ).animate().fadeIn(duration: 400.ms, delay: 300.ms),
              const SizedBox(height: 8),
              Text(
                'Your appointment has been booked successfully.',
                style: theme.textTheme.bodyLarge?.copyWith(color: colorScheme.onSurfaceVariant),
                textAlign: TextAlign.center,
              ).animate().fadeIn(duration: 400.ms, delay: 500.ms),
              const SizedBox(height: 32),
              Card(
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    children: [
                      _buildInfoRow(context, Icons.calendar_today, 'Tomorrow, 10:00 AM'),
                      const Divider(),
                      _buildInfoRow(context, Icons.access_time, '60 minutes'),
                      const Divider(),
                      _buildInfoRow(context, Icons.confirmation_number, 'BOK-${DateTime.now().millisecondsSinceEpoch.toString().substring(5, 11)}'),
                    ],
                  ),
                ),
              ).animate().fadeIn(duration: 400.ms, delay: 700.ms),
              const SizedBox(height: 24),
              // QR Code placeholder
              Container(
                width: 120,
                height: 120,
                decoration: BoxDecoration(
                  color: colorScheme.surfaceContainerHighest,
                  borderRadius: BorderRadius.circular(16),
                ),
                child: Icon(Icons.qr_code_rounded, size: 80, color: colorScheme.primary),
              ).animate().fadeIn(duration: 400.ms, delay: 900.ms),
              const Spacer(),
              FilledButton(
                onPressed: () => context.go('/'),
                style: FilledButton.styleFrom(minimumSize: const Size(double.infinity, 52)),
                child: const Text('Back to Home', style: TextStyle(fontSize: 16)),
              ),
              const SizedBox(height: 8),
              TextButton(
                onPressed: () => context.go('/appointments'),
                child: const Text('View My Appointments'),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildInfoRow(BuildContext context, IconData icon, String text) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Row(
        children: [
          Icon(icon, size: 20, color: Theme.of(context).colorScheme.primary),
          const SizedBox(width: 12),
          Text(text),
        ],
      ),
    );
  }
}