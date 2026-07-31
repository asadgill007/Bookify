import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../../../core/theme/app_theme.dart';
import '../providers/my_businesses_provider.dart';

/// Provider dashboard: shows the current user's businesses and their
/// verification status. Doubles as the "pending verification" screen.
class MyBusinessesScreen extends ConsumerStatefulWidget {
  const MyBusinessesScreen({super.key});

  @override
  ConsumerState<MyBusinessesScreen> createState() => _MyBusinessesScreenState();
}

class _MyBusinessesScreenState extends ConsumerState<MyBusinessesScreen> {
  String? _resubmitError;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final isDark = theme.brightness == Brightness.dark;
    final businessesAsync = ref.watch(myBusinessesProvider);

    return GradientBackground(
      child: Scaffold(
        backgroundColor: Colors.transparent,
        appBar: AppBar(
          backgroundColor: Colors.transparent,
          title: const Text('My Business'),
          actions: [
            IconButton(
              icon: const Icon(Icons.add_business_outlined),
              tooltip: 'List a new business',
              onPressed: () => context.push('/onboarding'),
            ),
          ],
        ),
        body: RefreshIndicator(
          onRefresh: () async => ref.invalidate(myBusinessesProvider),
          child: businessesAsync.when(
            loading: () => const Center(child: CircularProgressIndicator()),
            error: (err, _) => Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.error_outline, size: 48,
                      color: colorScheme.error),
                  const SizedBox(height: 16),
                  Text('Failed to load your businesses',
                      style: theme.textTheme.titleMedium),
                  const SizedBox(height: 8),
                  Text(err.toString(),
                      style: theme.textTheme.bodySmall?.copyWith(
                          color: colorScheme.onSurfaceVariant)),
                  const SizedBox(height: 16),
                  FilledButton.tonal(
                    onPressed: () => ref.invalidate(myBusinessesProvider),
                    child: const Text('Retry'),
                  ),
                ],
              ),
            ),
            data: (businesses) {
              if (businesses.isEmpty) {
                return _emptyState(theme, colorScheme, isDark);
              }
              return ListView.builder(
                physics: const AlwaysScrollableScrollPhysics(),
                padding: const EdgeInsets.all(16),
                itemCount: businesses.length,
                itemBuilder: (context, index) {
                  final biz = businesses[index];
                  return _buildBusinessCard(
                      context, theme, colorScheme, isDark, biz);
                },
              );
            },
          ),
        ),
      ),
    );
  }

  Widget _emptyState(ThemeData theme, ColorScheme colorScheme, bool isDark) {
    return ListView(
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.all(24),
      children: [
        const SizedBox(height: 48),
        Icon(Icons.storefront_outlined,
            size: 80,
            color: colorScheme.onSurfaceVariant.withValues(alpha: 0.4)),
        const SizedBox(height: 16),
        Text(
          'You have no businesses yet',
          style: theme.textTheme.titleLarge?.copyWith(
            fontWeight: FontWeight.w700,
            color: colorScheme.onSurface,
          ),
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: 8),
        Text(
          'List your business and start receiving bookings in minutes.',
          style: theme.textTheme.bodyMedium?.copyWith(
            color: colorScheme.onSurfaceVariant,
          ),
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: 24),
        SizedBox(
          width: double.infinity,
          height: 52,
          child: FilledButton.icon(
            onPressed: () => context.push('/onboarding'),
            icon: const Icon(Icons.add_business_outlined),
            label: const Text('List Your Business'),
          ),
        ),
      ],
    );
  }

  Widget _buildBusinessCard(
    BuildContext context,
    ThemeData theme,
    ColorScheme colorScheme,
    bool isDark,
    MyBusiness biz,
  ) {
    return GlassContainer(
      borderRadius: AppTheme.radiusXl,
      padding: const EdgeInsets.all(20),
      margin: const EdgeInsets.only(bottom: 16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                width: 56,
                height: 56,
                decoration: BoxDecoration(
                  gradient: const LinearGradient(
                    colors: [AppTheme.indigoLuxury, Color(0xFF7C3AED)],
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                  ),
                  borderRadius: BorderRadius.circular(16),
                ),
                child: biz.coverImageUrl != null
                    ? ClipRRect(
                        borderRadius: BorderRadius.circular(16),
                        child: Image.network(
                          biz.coverImageUrl!,
                          fit: BoxFit.cover,
                          errorBuilder: (_, _, _) =>
                              const Icon(Icons.storefront,
                                  color: Colors.white, size: 28),
                        ),
                      )
                    : const Icon(Icons.storefront,
                        color: Colors.white, size: 28),
              ),
              const SizedBox(width: 14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      biz.name,
                      style: theme.textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.w700,
                        color: colorScheme.onSurface,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      '${biz.city}, ${biz.country} · '
                      '${biz.totalServices} services · '
                      '${biz.totalProviders} staff',
                      style: theme.textTheme.bodySmall?.copyWith(
                        color: colorScheme.onSurfaceVariant,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 14),
          _buildStatusBanner(theme, colorScheme, biz),
          if (biz.isRejected && biz.rejectionReason != null) ...[
            const SizedBox(height: 12),
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: colorScheme.error.withValues(alpha: 0.08),
                borderRadius: BorderRadius.circular(12),
                border: Border.all(
                    color: colorScheme.error.withValues(alpha: 0.2)),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('Reason',
                      style: theme.textTheme.labelMedium?.copyWith(
                          color: colorScheme.error,
                          fontWeight: FontWeight.w700)),
                  const SizedBox(height: 4),
                  Text(
                    biz.rejectionReason!,
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: colorScheme.onSurface,
                    ),
                  ),
                ],
              ),
            ),
          ],
          const SizedBox(height: 14),
          Row(
            children: [
              if (biz.isRejected)
                Expanded(
                  child: FilledButton.icon(
                    onPressed: _resubmitError != null
                        ? null
                        : () => _resubmit(biz),
                    icon: const Icon(Icons.replay_outlined, size: 18),
                    label: const Text('Resubmit for Review'),
                  ),
                )
              else if (biz.isApproved)
                Expanded(
                  child: FilledButton.icon(
                    onPressed: () =>
                        context.push('/business/${biz.slug}'),
                    icon: const Icon(Icons.open_in_new, size: 18),
                    label: const Text('View Listing'),
                  ),
                )
              else
                Expanded(
                  child: OutlinedButton(
                    onPressed: null,
                    child: const Text('Awaiting review'),
                  ),
                ),
            ],
          ),
          if (_resubmitError != null) ...[
            const SizedBox(height: 8),
            Text(
              _resubmitError!,
              style: theme.textTheme.bodySmall?.copyWith(
                  color: colorScheme.error),
            ),
          ],
        ],
      ),
    ).animate().fadeIn(duration: 400.ms).slideY(begin: 0.1);
  }

  Widget _buildStatusBanner(
      ThemeData theme, ColorScheme colorScheme, MyBusiness biz) {    final (icon, color) = switch (biz.verificationStatus.toLowerCase()) {
      'approved' => (Icons.verified_rounded, AppTheme.success),
      'rejected' => (Icons.cancel_rounded, colorScheme.error),
      _ => (Icons.hourglass_top_rounded, AppTheme.warning),
    };
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: color.withValues(alpha: 0.25)),
      ),
      child: Row(
        children: [
          Icon(icon, color: color, size: 20),
          const SizedBox(width: 10),
          Expanded(
            child: Text(
              biz.isPending
                  ? 'Your listing is under review. It will appear in customer search once an admin approves it.'
                  : biz.isApproved
                      ? 'Your business is live and visible to customers.'
                      : 'Your listing was rejected by our review team.',
              style: theme.textTheme.bodySmall?.copyWith(
                color: colorScheme.onSurface,
              ),
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _resubmit(MyBusiness biz) async {
    setState(() => _resubmitError = null);
    try {
      await ref.read(resubmitProvider(biz.id).future);
      ref.invalidate(myBusinessesProvider);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Business resubmitted for review.'),
          ),
        );
      }
    } catch (e) {
      if (mounted) {
        setState(() => _resubmitError = 'Resubmit failed: $e');
      }
    }
  }
}
