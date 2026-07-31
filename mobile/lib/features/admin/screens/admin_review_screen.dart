import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../../../core/theme/app_theme.dart';
import '../providers/admin_provider.dart';

/// Admin review queue: pending businesses with approve/reject actions.
class AdminReviewScreen extends ConsumerStatefulWidget {
  const AdminReviewScreen({super.key});

  @override
  ConsumerState<AdminReviewScreen> createState() => _AdminReviewScreenState();
}

class _AdminReviewScreenState extends ConsumerState<AdminReviewScreen> {
  String _statusFilter = 'Pending';

  Future<void> _showRejectDialog(AdminBusiness biz) async {
    final reasonController = TextEditingController();
    final result = await showDialog<String>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Reject Business'),
        content: TextField(
          controller: reasonController,
          maxLines: 3,
          maxLength: 1000,
          decoration: const InputDecoration(
            labelText: 'Reason for rejection *',
            hintText: 'Tell the owner what needs to change',
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancel'),
          ),
          FilledButton(
            onPressed: reasonController.text.trim().isEmpty
                ? null
                : () => Navigator.pop(context, reasonController.text.trim()),
            child: const Text('Reject'),
          ),
        ],
      ),
    );

    if (result == null || !mounted) return;

    final notifier = ref.read(adminBusinessesProvider.notifier);
    final error = await notifier.reject(biz.id, result);
    if (!mounted) return;
    _showResult(error, success: 'Business rejected.');
  }

  Future<void> _approve(AdminBusiness biz) async {
    final notifier = ref.read(adminBusinessesProvider.notifier);
    final error = await notifier.verify(biz.id);
    if (!mounted) return;
    _showResult(error, success: 'Business approved and is now live.');
  }

  void _showDocuments(BuildContext context, AdminBusiness biz) {
    showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      builder: (sheetContext) => Padding(
        padding: const EdgeInsets.only(bottom: 24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const SizedBox(height: 16),
            Text(
              'Documents — ${biz.name}',
              style: Theme.of(sheetContext).textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.w700,
                  ),
            ),
            const SizedBox(height: 12),
            const Divider(),
            Flexible(
              child: Consumer(
                builder: (context, ref, _) {
                  final docsAsync =
                      ref.watch(businessDocumentsProvider(biz.id));
                  return docsAsync.when(
                    loading: () => const Padding(
                      padding: EdgeInsets.all(32),
                      child: CircularProgressIndicator(),
                    ),
                    error: (err, _) => Padding(
                      padding: const EdgeInsets.all(24),
                      child: Text('Could not load documents: $err'),
                    ),
                    data: (docs) {
                      if (docs.isEmpty) {
                        return const Padding(
                          padding: EdgeInsets.all(24),
                          child: Text('No verification documents submitted yet.'),
                        );
                      }
                      return ListView.separated(
                        shrinkWrap: true,
                        itemCount: docs.length,
                        separatorBuilder: (_, _) => const Divider(height: 1),
                        itemBuilder: (context, index) {
                          final doc = docs[index];
                          return ListTile(
                            leading: const Icon(Icons.description_outlined),
                            title: Text(doc.fileName,
                                style: const TextStyle(
                                    fontWeight: FontWeight.w600)),
                            subtitle: Text(
                              '${doc.documentType} · ${doc.uploadedByName}'
                              ' · ${doc.createdAt.day}/${doc.createdAt.month}/${doc.createdAt.year}',
                            ),
                          );
                        },
                      );
                    },
                  );
                },
              ),
            ),
          ],
        ),
      ),
    );
  }

  void _showResult(String? error, {required String success}) {
    if (!mounted) return;
    if (error != null) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(error)),
      );
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(success)),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final isDark = theme.brightness == Brightness.dark;
    final businessesAsync = ref.watch(adminBusinessesProvider);

    return GradientBackground(
      child: Scaffold(
        backgroundColor: Colors.transparent,
        appBar: AppBar(
          backgroundColor: Colors.transparent,
          title: const Text('Business Review'),
        ),
        body: Column(
          children: [
            // Status filter chips
            SizedBox(
              height: 48,
              child: ListView(
                scrollDirection: Axis.horizontal,
                padding: const EdgeInsets.symmetric(horizontal: 16),
                children: ['Pending', 'Approved', 'Rejected', 'All']
                    .map((status) => Padding(
                          padding: const EdgeInsets.only(right: 8),
                          child: FilterChip(
                            label: Text(status),
                            selected: _statusFilter == status,
                            onSelected: (_) {
                              setState(() => _statusFilter = status);
                              ref
                                  .read(adminBusinessesProvider.notifier)
                                  .setStatus(status);
                            },
                          ),
                        ))
                    .toList(),
              ),
            ),
            const SizedBox(height: 4),
            Expanded(
              child: RefreshIndicator(
                onRefresh: () => ref
                    .read(adminBusinessesProvider.notifier)
                    .refresh(),
                child: businessesAsync.when(
                  loading: () => const Center(
                      child: CircularProgressIndicator()),
                  error: (err, _) => Center(
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(Icons.error_outline,
                            size: 48, color: colorScheme.error),
                        const SizedBox(height: 16),
                        Text('Failed to load businesses',
                            style: theme.textTheme.titleMedium),
                        const SizedBox(height: 8),
                        Padding(
                          padding: const EdgeInsets.symmetric(horizontal: 32),
                          child: Text(err.toString(),
                              style: theme.textTheme.bodySmall?.copyWith(
                                  color: colorScheme.onSurfaceVariant),
                              textAlign: TextAlign.center),
                        ),
                        const SizedBox(height: 16),
                        FilledButton.tonal(
                          onPressed: () => ref
                              .read(adminBusinessesProvider.notifier)
                              .refresh(),
                          child: const Text('Retry'),
                        ),
                      ],
                    ),
                  ),
                  data: (businesses) {
                    if (businesses.isEmpty) {
                      return Center(
                        child: Column(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            Icon(Icons.task_alt,
                                size: 64,
                                color: colorScheme.onSurfaceVariant
                                    .withValues(alpha: 0.4)),
                            const SizedBox(height: 16),
                            Text(
                              'No ${_statusFilter.toLowerCase()} businesses',
                              style: theme.textTheme.titleMedium?.copyWith(
                                color: colorScheme.onSurfaceVariant,
                              ),
                            ),
                          ],
                        ),
                      );
                    }
                    return ListView.builder(
                      physics: const AlwaysScrollableScrollPhysics(),
                      padding: const EdgeInsets.all(16),
                      itemCount: businesses.length,
                      itemBuilder: (context, index) {
                        final biz = businesses[index];
                        return _buildCard(
                            context, theme, colorScheme, isDark, biz);
                      },
                    );
                  },
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildCard(
    BuildContext context,
    ThemeData theme,
    ColorScheme colorScheme,
    bool isDark,
    AdminBusiness biz,
  ) {
    final statusColor = switch (biz.verificationStatus.toLowerCase()) {
      'approved' => AppTheme.success,
      'rejected' => colorScheme.error,
      _ => AppTheme.warning,
    };

    return GlassContainer(
      borderRadius: AppTheme.radiusXl,
      padding: const EdgeInsets.all(18),
      margin: const EdgeInsets.only(bottom: 14),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                width: 48,
                height: 48,
                decoration: BoxDecoration(
                  color: AppTheme.indigoLuxury.withValues(alpha: 0.12),
                  borderRadius: BorderRadius.circular(14),
                ),
                child: Icon(Icons.storefront,
                    color: AppTheme.indigoLuxury, size: 24),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      biz.name,
                      style: theme.textTheme.titleSmall?.copyWith(
                        fontWeight: FontWeight.w700,
                        color: colorScheme.onSurface,
                      ),
                    ),
                    const SizedBox(height: 2),
                    Text(
                      biz.ownerName.isEmpty
                          ? '${biz.city}, ${biz.country}'
                          : '${biz.ownerName} · ${biz.city}, ${biz.country}',
                      style: theme.textTheme.bodySmall?.copyWith(
                        color: colorScheme.onSurfaceVariant,
                      ),
                    ),
                    const SizedBox(height: 6),
                    Container(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 10, vertical: 4),
                      decoration: BoxDecoration(
                        color: statusColor.withValues(alpha: 0.12),
                        borderRadius: BorderRadius.circular(999),
                      ),
                      child: Text(
                        biz.verificationStatus,
                        style: theme.textTheme.labelSmall?.copyWith(
                          color: statusColor,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
              Column(
                crossAxisAlignment: CrossAxisAlignment.end,
                children: [
                  Row(
                    children: [
                      const Icon(Icons.star_rounded,
                          color: Color(0xFFF59E0B), size: 16),
                      const SizedBox(width: 2),
                      Text(
                        biz.averageRating.toStringAsFixed(1),
                        style: theme.textTheme.labelMedium
                            ?.copyWith(fontWeight: FontWeight.w600),
                      ),
                    ],
                  ),
                  const SizedBox(height: 4),
                  Text(
                    '${biz.totalReviews} reviews',
                    style: theme.textTheme.labelSmall?.copyWith(
                      color: colorScheme.onSurfaceVariant,
                    ),
                  ),
                ],
              ),
            ],
          ),
          if (biz.isRejected && biz.rejectionReason != null) ...[
            const SizedBox(height: 12),
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: colorScheme.error.withValues(alpha: 0.07),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Text(
                'Reason: ${biz.rejectionReason}',
                style: theme.textTheme.bodySmall,
              ),
            ),
          ],
          const SizedBox(height: 14),
          Row(
            children: [
              Expanded(
                child: OutlinedButton.icon(
                  onPressed: () => _showDocuments(context, biz),
                  icon: const Icon(Icons.folder_open, size: 18),
                  label: const Text('Documents'),
                ),
              ),
            ],
          ),
          const SizedBox(height: 10),
          Row(
            children: [
              Expanded(
                child: OutlinedButton.icon(
                  onPressed: biz.isPending ? () => _showRejectDialog(biz) : null,
                  icon: const Icon(Icons.close, size: 18),
                  label: const Text('Reject'),
                  style: OutlinedButton.styleFrom(
                    foregroundColor: colorScheme.error,
                    side: BorderSide(color: colorScheme.error),
                  ),
                ),
              ),
              const SizedBox(width: 10),
              Expanded(
                child: FilledButton.icon(
                  onPressed: biz.isPending ? () => _approve(biz) : null,
                  icon: const Icon(Icons.check, size: 18),
                  label: const Text('Approve'),
                ),
              ),
            ],
          ),
        ],
      ),
    ).animate().fadeIn(duration: 300.ms).slideY(begin: 0.08);
  }
}
