import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/constants/api_constants.dart';
import '../../../core/network/api_client.dart';

/// Notification model matching the backend NotificationDto.
class AppNotification {
  final String id;
  final String type;
  final String title;
  final String body;
  final bool isRead;
  final DateTime createdAt;

  const AppNotification({
    required this.id,
    required this.type,
    required this.title,
    required this.body,
    required this.isRead,
    required this.createdAt,
  });

  factory AppNotification.fromJson(Map<String, dynamic> json) {
    return AppNotification(
      id: json['id'] as String? ?? '',
      type: json['type'] as String? ?? 'System',
      title: json['title'] as String? ?? '',
      body: json['body'] as String? ?? '',
      isRead: json['isRead'] as bool? ?? false,
      createdAt:
          DateTime.tryParse(json['createdAt'] as String? ?? '') ?? DateTime.now(),
    );
  }
}

/// Fetches the authenticated user's notifications from the backend.
final notificationsProvider = FutureProvider<List<AppNotification>>((ref) async {
  final api = ref.watch(apiClientProvider);
  final response = await api.get(ApiConstants.notifications);

  final data = response.data;
  List<dynamic> rawList;
  if (data is Map<String, dynamic> && data.containsKey('data')) {
    rawList = data['data'] as List<dynamic>;
  } else if (data is List) {
    rawList = data;
  } else {
    return [];
  }

  return rawList
      .map((e) => AppNotification.fromJson(e as Map<String, dynamic>))
      .toList();
});

class NotificationsScreen extends ConsumerWidget {
  const NotificationsScreen({super.key});

  Future<void> _markAllRead(BuildContext context, WidgetRef ref) async {
    try {
      final api = ref.read(apiClientProvider);
      await api.put('${ApiConstants.notifications}/read-all');
      ref.invalidate(notificationsProvider);
    } catch (_) {
      if (!context.mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Could not update notifications')),
      );
    }
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final notificationsAsync = ref.watch(notificationsProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Notifications'),
        actions: [
          TextButton(
            onPressed: () => _markAllRead(context, ref),
            child: const Text('Mark All Read'),
          ),
        ],
      ),
      body: notificationsAsync.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (err, _) => Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(Icons.error_outline, size: 48, color: colorScheme.error),
              const SizedBox(height: 16),
              Text('Failed to load notifications',
                  style: theme.textTheme.titleMedium),
              const SizedBox(height: 16),
              FilledButton.tonal(
                onPressed: () => ref.invalidate(notificationsProvider),
                child: const Text('Retry'),
              ),
            ],
          ),
        ),
        data: (notifications) {
          if (notifications.isEmpty) {
            return Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.notifications_none, size: 64,
                      color: colorScheme.onSurfaceVariant.withValues(alpha: 0.4)),
                  const SizedBox(height: 16),
                  Text('No notifications yet',
                      style: theme.textTheme.titleMedium?.copyWith(
                          color: colorScheme.onSurfaceVariant)),
                  const SizedBox(height: 8),
                  Text('We\'ll let you know when something happens',
                      style: theme.textTheme.bodyMedium?.copyWith(
                          color: colorScheme.onSurfaceVariant)),
                ],
              ),
            );
          }

          return ListView.builder(
            padding: const EdgeInsets.all(16),
            itemCount: notifications.length,
            itemBuilder: (context, index) {
              final notification = notifications[index];
              return _buildNotification(theme, notification);
            },
          );
        },
      ),
    );
  }

  Widget _buildNotification(ThemeData theme, AppNotification notification) {
    final isUnread = !notification.isRead;
    return Card(
      color: isUnread ? theme.colorScheme.primaryContainer.withValues(alpha: 0.3) : null,
      child: ListTile(
        leading: CircleAvatar(
          backgroundColor: theme.colorScheme.primaryContainer,
          child: Icon(
            _typeIcon(notification.type),
            color: theme.colorScheme.primary,
          ),
        ),
        title: Text(notification.title,
            style:
                TextStyle(fontWeight: isUnread ? FontWeight.bold : FontWeight.normal)),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const SizedBox(height: 4),
            Text(notification.body, maxLines: 2, overflow: TextOverflow.ellipsis),
            const SizedBox(height: 4),
            Text(
              _formatTime(notification.createdAt),
              style: theme.textTheme.labelSmall?.copyWith(
                  color: theme.colorScheme.onSurfaceVariant),
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
      ),
    );
  }

  IconData _typeIcon(String type) {
    switch (type) {
      case 'BookingConfirmed':
      case 'AppointmentConfirmed':
        return Icons.check_circle;
      case 'AppointmentReminder':
        return Icons.calendar_today;
      case 'AppointmentCancelled':
        return Icons.cancel_outlined;
      case 'Review':
        return Icons.star_rounded;
      case 'Payment':
        return Icons.payments_outlined;
      case 'System':
      default:
        return Icons.notifications_outlined;
    }
  }

  String _formatTime(DateTime time) {
    final diff = DateTime.now().difference(time);
    if (diff.inMinutes < 1) return 'Just now';
    if (diff.inMinutes < 60) return '${diff.inMinutes}m ago';
    if (diff.inHours < 24) return '${diff.inHours}h ago';
    return '${diff.inDays}d ago';
  }
}