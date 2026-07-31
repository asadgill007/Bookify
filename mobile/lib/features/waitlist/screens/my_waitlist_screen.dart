import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../../../core/theme/app_theme.dart';
import '../providers/waitlist_provider.dart';

/// My Waitlist screen: shows the customer's active waitlist entries.
class MyWaitlistScreen extends ConsumerWidget {
  const MyWaitlistScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final entriesAsync = ref.watch(myWaitlistProvider);

    return GradientBackground(
      child: Scaffold(
        backgroundColor: Colors.transparent,
        appBar: AppBar(
          title: const Text('My Waitlist'),
          backgroundColor: Colors.transparent,
        ),
        body: entriesAsync.when(
          loading: () => const Center(child: CircularProgressIndicator()),
          error: (err, _) => Center(
            child: Padding(
              padding: const EdgeInsets.all(32),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.error_outline, size: 48, color: colorScheme.error),
                  const SizedBox(height: 12),
                  Text('Could not load your waitlist',
                      style: theme.textTheme.titleMedium),
                  const SizedBox(height: 16),
                  FilledButton.tonal(
                    onPressed: () => ref.invalidate(myWaitlistProvider),
                    child: const Text('Retry'),
                  ),
                ],
              ),
            ),
          ),
          data: (entries) {
            if (entries.isEmpty) {
              return Center(
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(Icons.hourglass_empty_rounded,
                        size: 64,
                        color: colorScheme.onSurfaceVariant.withValues(alpha: 0.4)),
                    const SizedBox(height: 16),
                    Text('Nothing in your waitlist',
                        style: theme.textTheme.titleMedium
                            ?.copyWith(color: colorScheme.onSurfaceVariant)),
                    const SizedBox(height: 8),
                    Text(
                      'When a fully-booked slot opens up, join the waitlist '
                      'to be notified when it becomes available.',
                      textAlign: TextAlign.center,
                      style: theme.textTheme.bodyMedium?.copyWith(
                          color: colorScheme.onSurfaceVariant),
                    ),
                    const SizedBox(height: 20),
                    OutlinedButton.icon(
                      onPressed: () {
                        // Return to search so the user can find a business.
                        ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(
                              content: Text('Search for a business to book'),
                          ),
                        );
                      },
                      icon: const Icon(Icons.search),
                      label: const Text('Find a Business'),
                    ),
                  ],
                ),
              );
            }

            return RefreshIndicator(
              onRefresh: () => ref.refresh(myWaitlistProvider.future),
              child: ListView.separated(
                physics: const AlwaysScrollableScrollPhysics(),
                padding: const EdgeInsets.all(16),
                itemCount: entries.length,
                separatorBuilder: (_, _) => const SizedBox(height: 12),
                itemBuilder: (context, index) {
                  final entry = entries[index];
                  return _WaitlistCard(entry: entry, index: index);
                },
              ),
            );
          },
        ),
      ),
    );
  }
}

class _WaitlistCard extends ConsumerWidget {
  final WaitlistEntry entry;
  final int index;
  const _WaitlistCard({required this.entry, required this.index});

  Future<void> _leave(BuildContext context, WidgetRef ref) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Leave waitlist?'),
        content: const Text(
            'You will stop being notified about this slot becoming available.'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Cancel'),
          ),
          FilledButton(
            style: FilledButton.styleFrom(backgroundColor: Colors.red),
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('Leave'),
          ),
        ],
      ),
    );
    if (confirmed != true) return;
    try {
      await ref.read(waitlistApiProvider).leave(entry.id);
      if (!context.mounted) return;
      ref.invalidate(myWaitlistProvider);
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('You left the waitlist.')),
      );
    } catch (_) {
      if (!context.mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Could not leave the waitlist.')),
      );
    }
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final isActive = entry.isActive;

    return GlassContainer(
      borderRadius: AppTheme.radiusLg,
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 44,
                height: 44,
                decoration: BoxDecoration(
                  gradient: LinearGradient(
                    colors: [AppTheme.indigoLuxury, const Color(0xFF7C3AED)],
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                  ),
                  borderRadius: BorderRadius.circular(14),
                ),
                child: const Icon(Icons.hourglass_bottom,
                    color: Colors.white, size: 22),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(entry.serviceName,
                        style: theme.textTheme.titleSmall
                            ?.copyWith(fontWeight: FontWeight.w600)),
                    Text(entry.providerName,
                        style: theme.textTheme.bodySmall?.copyWith(
                            color: colorScheme.onSurfaceVariant)),
                  ],
                ),
              ),
              _StatusBadge(active: isActive, status: entry.status),
            ],
          ),
          const SizedBox(height: 12),
          Row(
            children: [
              Icon(Icons.event, size: 15, color: colorScheme.onSurfaceVariant),
              const SizedBox(width: 6),
              Text(_formatDate(entry.appointmentDate),
                  style: theme.textTheme.bodySmall),
              if (entry.preferredStartTime != null) ...[
                const SizedBox(width: 12),
                Icon(Icons.schedule, size: 15,
                    color: colorScheme.onSurfaceVariant),
                const SizedBox(width: 6),
                Text(_formatTime(entry.preferredStartTime!),
                    style: theme.textTheme.bodySmall),
              ],
            ],
          ),
          const SizedBox(height: 8),
          Row(
            children: [
              Icon(Icons.format_list_numbered,
                  size: 15, color: AppTheme.indigoLuxury),
              const SizedBox(width: 6),
              Text(
                entry.position > 0
                    ? 'Position ${entry.position} in queue'
                    : 'Waiting',
                style: theme.textTheme.bodySmall?.copyWith(
                    color: AppTheme.indigoLuxury, fontWeight: FontWeight.w600),
              ),
            ],
          ),
          const SizedBox(height: 14),
          SizedBox(
            width: double.infinity,
            height: 42,
            child: OutlinedButton.icon(
              onPressed: () => _leave(context, ref),
              icon: const Icon(Icons.logout, size: 16, color: Colors.red),
              label: const Text('Leave Waitlist',
                  style: TextStyle(color: Colors.red)),
              style: OutlinedButton.styleFrom(
                side: const BorderSide(color: Colors.red),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                ),
              ),
            ),
          ),
        ],
      ),
    ).animate().fadeIn(duration: 300.ms, delay: (index * 60).ms);
  }

  String _formatDate(String yyyymmdd) {
    final parts = yyyymmdd.split('-');
    if (parts.length != 3) return yyyymmdd;
    const months = [
      'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
      'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec',
    ];
    final m = int.tryParse(parts[1]);
    final d = int.tryParse(parts[2]);
    final y = int.tryParse(parts[0]);
    if (m == null || d == null || y == null) return yyyymmdd;
    return '$d ${months[m - 1]} $y';
  }

  String _formatTime(String t) {
    final parts = t.split(':');
    if (parts.isEmpty) return t;
    final h = int.tryParse(parts[0]);
    if (h == null) return t;
    final period = h >= 12 ? 'PM' : 'AM';
    final hr = h % 12 == 0 ? 12 : h % 12;
    return '$hr:${parts.length > 1 ? parts[1] : '00'} $period';
  }
}

class _StatusBadge extends StatelessWidget {
  final bool active;
  final String status;
  const _StatusBadge({required this.active, required this.status});

  @override
  Widget build(BuildContext context) {
    final color = active ? AppTheme.success : AppTheme.slate500;
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.12),
        borderRadius: BorderRadius.circular(AppTheme.radiusFull),
      ),
      child: Text(
        status,
        style: TextStyle(
            color: color, fontSize: 11, fontWeight: FontWeight.w600),
      ),
    );
  }
}
