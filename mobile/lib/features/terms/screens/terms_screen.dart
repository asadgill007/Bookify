import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class TermsScreen extends ConsumerWidget {
  const TermsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(title: const Text('Terms of Service')),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Terms of Service', style: theme.textTheme.headlineMedium?.copyWith(fontWeight: FontWeight.bold)),
            const SizedBox(height: 16),
            Text('Last updated: July 2026', style: theme.textTheme.bodyMedium?.copyWith(color: theme.colorScheme.onSurfaceVariant)),
            const SizedBox(height: 24),
            Text('1. Acceptance of Terms', style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            Text('By using Bookify, you agree to these terms. If you do not agree, do not use the service.', style: theme.textTheme.bodyLarge),
            const SizedBox(height: 24),
            Text('2. Booking and Cancellation', style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            Text('You agree to provide accurate information when booking. Cancellation policies vary by business.', style: theme.textTheme.bodyLarge),
            const SizedBox(height: 24),
            Text('3. User Conduct', style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            Text('You agree not to misuse the platform, harass others, or engage in fraudulent activity.', style: theme.textTheme.bodyLarge),
            const SizedBox(height: 24),
            Text('4. Limitation of Liability', style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            Text('Bookify is a platform connecting customers with service providers. We are not responsible for the quality of services provided.', style: theme.textTheme.bodyLarge),
          ],
        ),
      ),
    );
  }
}