import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class NotificationsScreen extends ConsumerWidget {
  const NotificationsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Notifications'),
        actions: [
          TextButton(
            onPressed: () {},
            child: const Text('Mark All Read'),
          ),
        ],
      ),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          _buildNotification(
            theme,
            Icons.calendar_today,
            'Appointment Reminder',
            'Your appointment with Dr. Smith is tomorrow at 10:00 AM',
            DateTime.now().subtract(const Duration(hours: 2)),
            isUnread: true,
          ),
          _buildNotification(
            theme,
            Icons.check_circle,
            'Booking Confirmed',
            'Your haircut appointment has been confirmed',
            DateTime.now().subtract(const Duration(days: 1)),
            isUnread: true,
          ),
          _buildNotification(
            theme,
            Icons.star_rounded,
            'Review Request',
            'How was your experience? Leave a review for Salon Luxe',
            DateTime.now().subtract(const Duration(days: 3)),
          ),
          _buildNotification(
            theme,
            Icons.cancel_outlined,
            'Appointment Cancelled',
            'Your appointment for Friday has been cancelled',
            DateTime.now().subtract(const Duration(days: 5)),
          ),
        ],
      ),
    );
  }

  Widget _buildNotification(
    ThemeData theme,
    IconData icon,
    String title,
    String body,
    DateTime time, {
    bool isUnread = false,
  }) {
    return Card(
      color: isUnread ? theme.colorScheme.primaryContainer.withValues(alpha: 0.3) : null,
      child: ListTile(
        leading: CircleAvatar(
          backgroundColor: theme.colorScheme.primaryContainer,
          child: Icon(icon, color: theme.colorScheme.primary),
        ),
        title: Text(title, style: TextStyle(fontWeight: isUnread ? FontWeight.bold : FontWeight.normal)),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const SizedBox(height: 4),
            Text(body, maxLines: 2, overflow: TextOverflow.ellipsis),
            const SizedBox(height: 4),
            Text(
              _formatTime(time),
              style: theme.textTheme.labelSmall?.copyWith(color: theme.colorScheme.onSurfaceVariant),
            ),
          ],
        ),
        trailing: isUnread
            ? Container(
                width: 8,
                height: 8,
                decoration: const BoxDecoration(
                  color: Colors.blue,
                  shape: BoxShape.circle,
                ),
              )
            : null,
        onTap: () {},
      ),
    );
  }

  String _formatTime(DateTime time) {
    final diff = DateTime.now().difference(time);
    if (diff.inMinutes < 60) return '${diff.inMinutes}m ago';
    if (diff.inHours < 24) return '${diff.inHours}h ago';
    return '${diff.inDays}d ago';
  }
}