import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class PrivacyScreen extends ConsumerWidget {
  const PrivacyScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(title: const Text('Privacy Policy')),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Privacy Policy', style: theme.textTheme.headlineMedium?.copyWith(fontWeight: FontWeight.bold)),
            const SizedBox(height: 16),
            Text(
              'Last updated: July 2026',
              style: theme.textTheme.bodyMedium?.copyWith(color: theme.colorScheme.onSurfaceVariant),
            ),
            const SizedBox(height: 24),
            Text('Information We Collect', style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            Text(
              'We collect information you provide directly, such as your name, email address, phone number, and payment information when you create an account or make a booking.',
              style: theme.textTheme.bodyLarge,
            ),
            const SizedBox(height: 24),
            Text('How We Use Your Information', style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            Text(
              'We use your information to process bookings, send reminders, improve our services, and communicate with you about your account.',
              style: theme.textTheme.bodyLarge,
            ),
            const SizedBox(height: 24),
            Text('Data Security', style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            Text(
              'We implement appropriate security measures to protect your personal information. Your data is encrypted in transit and at rest.',
              style: theme.textTheme.bodyLarge,
            ),
            const SizedBox(height: 24),
            Text('Contact Us', style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            Text(
              'If you have questions about this policy, contact us at privacy@bookify.app',
              style: theme.textTheme.bodyLarge,
            ),
          ],
        ),
      ),
    );
  }
}