import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../../../core/theme/app_theme.dart';
import '../providers/recurring_bookings_provider.dart';

/// My Recurring Bookings screen: lists active recurring series with cancel.
class MyRecurringBookingsScreen extends ConsumerWidget {
  const MyRecurringBookingsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final recurringAsync = ref.watch(myRecurringBookingsProvider);

    return GradientBackground(
      child: Scaffold(
        backgroundColor: Colors.transparent,
        appBar: AppBar(
          title: const Text('Recurring Bookings'),
          backgroundColor: Colors.transparent,
        ),
        body: recurringAsync.when(
          loading: () => const Center(child: CircularProgressIndicator()),
          error: (err, _) => Center(
            child: Padding(
              padding: const EdgeInsets.all(32),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.error_outline, size: 48, color: colorScheme.error),
                  const SizedBox(height: 12),
                  Text('Could not load recurring bookings',
                      style: theme.textTheme.titleMedium),
                  const SizedBox(height: 16),
                  FilledButton.tonal(
                    onPressed: () =>
                        ref.invalidate(myRecurringBookingsProvider),
                    child: const Text('Retry'),
                  ),
                ],
              ),
            ),
          ),
          data: (series) {
            if (series.isEmpty) {
              return Center(
                child: Padding(
                  padding: const EdgeInsets.all(32),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Icon(Icons.repeat_rounded,
                          size: 64,
                          color: colorScheme.onSurfaceVariant
                              .withValues(alpha: 0.4)),
                      const SizedBox(height: 16),
                      Text('No recurring bookings',
                          style: theme.textTheme.titleMedium
                              ?.copyWith(color: colorScheme.onSurfaceVariant)),
                      const SizedBox(height: 8),
                      Text(
                        'When booking an appointment, turn on '
                        '"Make this recurring" to schedule a series of '
                        'appointments automatically.',
                        textAlign: TextAlign.center,
                        style: theme.textTheme.bodyMedium?.copyWith(
                            color: colorScheme.onSurfaceVariant),
                      ),
                    ],
                  ),
                ),
              );
            }

            return RefreshIndicator(
              onRefresh: () => ref.refresh(myRecurringBookingsProvider.future),
              child: ListView.separated(
                physics: const AlwaysScrollableScrollPhysics(),
                padding: const EdgeInsets.all(16),
                itemCount: series.length,
                separatorBuilder: (_, _) => const SizedBox(height: 12),
                itemBuilder: (context, index) {
                  final item = series[index];
                  return _RecurringCard(item: item, index: index);
                },
              ),
            );
          },
        ),
      ),
    );
  }
}

class _RecurringCard extends ConsumerWidget {
  final RecurringBooking item;
  final int index;
  const _RecurringCard({required this.item, required this.index});

  Future<void> _cancel(BuildContext context, WidgetRef ref) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Cancel this series?'),
        content: Text(
            'This will cancel ${item.occurrencesCreated} scheduled '
            'appointments in this recurring series.'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Keep'),
          ),
          FilledButton(
            style: FilledButton.styleFrom(backgroundColor: Colors.red),
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('Cancel Series'),
          ),
        ],
      ),
    );
    if (confirmed != true) return;
    try {
      await ref.read(recurringBookingsApiProvider).cancelSeries(item.id);
      if (!context.mounted) return;
      ref.invalidate(myRecurringBookingsProvider);
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Recurring series cancelled.')),
      );
    } catch (_) {
      if (!context.mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Could not cancel the series.')),
      );
    }
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;

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
                child: const Icon(Icons.repeat, color: Colors.white, size: 22),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(item.serviceName,
                        style: theme.textTheme.titleSmall
                            ?.copyWith(fontWeight: FontWeight.w600)),
                    Text(item.businessName,
                        style: theme.textTheme.bodySmall?.copyWith(
                            color: colorScheme.onSurfaceVariant)),
                  ],
                ),
              ),
              Container(
                padding:
                    const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                decoration: BoxDecoration(
                  color: (item.isActive ? AppTheme.success : AppTheme.slate500)
                      .withValues(alpha: 0.12),
                  borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                ),
                child: Text(
                  item.isActive ? 'Active' : 'Cancelled',
                  style: TextStyle(
                    color:
                        item.isActive ? AppTheme.success : AppTheme.slate500,
                    fontSize: 11,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          // Pattern chip
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
            decoration: BoxDecoration(
              color: AppTheme.indigoLuxury.withValues(alpha: 0.1),
              borderRadius: BorderRadius.circular(AppTheme.radiusFull),
            ),
            child: Text(
              item.recurrenceLabel,
              style: const TextStyle(
                  color: AppTheme.indigoLuxury,
                  fontSize: 12,
                  fontWeight: FontWeight.w600),
            ),
          ),
          const SizedBox(height: 10),
          Row(
            children: [
              Icon(Icons.calendar_month, size: 15,
                  color: colorScheme.onSurfaceVariant),
              const SizedBox(width: 6),
              Text(
                'Starts ${_formatDate(item.seriesStartDate)}'
                '${_formatEnd(item)}',
                style: theme.textTheme.bodySmall,
              ),
            ],
          ),
          const SizedBox(height: 6),
          Row(
            children: [
              Icon(Icons.schedule, size: 15, color: colorScheme.onSurfaceVariant),
              const SizedBox(width: 6),
              Text(
                '${_formatTime(item.startTime)} – ${_formatTime(item.endTime)}'
                ' · ${item.occurrencesCreated} scheduled',
                style: theme.textTheme.bodySmall,
              ),
            ],
          ),
          if (item.notes != null && item.notes!.isNotEmpty) ...[
            const SizedBox(height: 8),
            Text(
              item.notes!,
              style: theme.textTheme.bodySmall
                  ?.copyWith(color: colorScheme.onSurfaceVariant),
              maxLines: 2,
              overflow: TextOverflow.ellipsis,
            ),
          ],
          if (item.isActive) ...[
            const SizedBox(height: 14),
            SizedBox(
              width: double.infinity,
              height: 42,
              child: OutlinedButton.icon(
                onPressed: () => _cancel(context, ref),
                icon: const Icon(Icons.cancel_outlined,
                    size: 16, color: Colors.red),
                label: const Text('Cancel Series',
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
        ],
      ),
    ).animate().fadeIn(duration: 300.ms, delay: (index * 60).ms);
  }

  String _formatEnd(RecurringBooking item) {
    if (item.seriesEndDate != null) {
      return ' · ends ${_formatDate(item.seriesEndDate!)}';
    }
    if (item.maxOccurrences != null && item.maxOccurrences! > 0) {
      return ' · up to ${item.maxOccurrences} times';
    }
    return '';
  }

  String _formatDate(DateTime d) {
    const months = [
      'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
      'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec',
    ];
    return '${d.day} ${months[d.month - 1]} ${d.year}';
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
