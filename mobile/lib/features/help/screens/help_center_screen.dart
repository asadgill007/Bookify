import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class HelpCenterScreen extends ConsumerWidget {
  const HelpCenterScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(title: const Text('Help Center')),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          _buildFaqItem(theme, 'How do I book an appointment?', 'Browse businesses, select a service, choose your preferred time slot, and confirm your booking.'),
          _buildFaqItem(theme, 'Can I cancel or reschedule?', 'Yes, you can cancel or reschedule appointments from the Appointments screen up to 24 hours before the scheduled time.'),
          _buildFaqItem(theme, 'How do I reset my password?', 'Go to the login screen and tap "Forgot Password" to receive a reset link via email.'),
          _buildFaqItem(theme, 'Is my payment information secure?', 'Yes, all payments are processed securely. We do not store your payment details.'),
          _buildFaqItem(theme, 'How do I contact support?', 'You can reach us at support@bookify.app or through the app.'),
        ],
      ),
    );
  }

  Widget _buildFaqItem(ThemeData theme, String question, String answer) {
    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      child: ExpansionTile(
        title: Text(question, style: const TextStyle(fontWeight: FontWeight.w600)),
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
            child: Text(answer, style: theme.textTheme.bodyMedium?.copyWith(color: theme.colorScheme.onSurfaceVariant)),
          ),
        ],
      ),
    );
  }
}