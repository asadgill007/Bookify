import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:go_router/go_router.dart';
import '../../../core/theme/app_theme.dart';
import '../../../l10n/generated/app_localizations.dart';
import '../providers/favorites_provider.dart';

/// My Favorites screen: shows businesses the customer has hearted.
class FavoritesScreen extends ConsumerWidget {
  const FavoritesScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final l10n = AppLocalizations.of(context);
    final favoritesAsync = ref.watch(favoritesProvider);

    return GradientBackground(
      child: Scaffold(
        backgroundColor: Colors.transparent,
        appBar: AppBar(
          title: Text(l10n.favoritesTitle),
          backgroundColor: Colors.transparent,
        ),
        body: RefreshIndicator(
          onRefresh: () async => ref.invalidate(favoritesProvider),
          child: favoritesAsync.when(
            loading: () => const Center(child: CircularProgressIndicator()),
            error: (err, _) => Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.error_outline, size: 48, color: colorScheme.error),
                  const SizedBox(height: 16),
                  Text(l10n.commonError, style: theme.textTheme.titleMedium),
                  const SizedBox(height: 8),
                  Text(err.toString(),
                      style: theme.textTheme.bodySmall?.copyWith(
                          color: colorScheme.onSurfaceVariant)),
                  const SizedBox(height: 16),
                  FilledButton.tonal(
                    onPressed: () => ref.invalidate(favoritesProvider),
                    child: Text(l10n.commonRetry),
                  ),
                ],
              ),
            ),
            data: (favorites) {
              if (favorites.isEmpty) {
                return _emptyState(context, theme, colorScheme, l10n);
              }
              return ListView.separated(
                physics: const AlwaysScrollableScrollPhysics(),
                padding: const EdgeInsets.all(16),
                itemCount: favorites.length,
                separatorBuilder: (_, _) => const SizedBox(height: 12),
                itemBuilder: (context, index) {
                  final fav = favorites[index];
                  return _FavoriteCard(favorite: fav, index: index);
                },
              );
            },
          ),
        ),
      ),
    );
  }

  Widget _emptyState(
      BuildContext context, ThemeData theme, ColorScheme colorScheme, AppLocalizations l10n) {
    return ListView(
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.all(24),
      children: [
        const SizedBox(height: 64),
        Icon(Icons.favorite_border,
            size: 80, color: colorScheme.onSurfaceVariant.withValues(alpha: 0.4)),
        const SizedBox(height: 16),
        Text(l10n.favoritesEmpty,
            style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w700),
            textAlign: TextAlign.center),
        const SizedBox(height: 8),
        Text(l10n.favoritesEmptySubtitle,
            style: theme.textTheme.bodyMedium?.copyWith(color: colorScheme.onSurfaceVariant),
            textAlign: TextAlign.center),
        const SizedBox(height: 24),
        SizedBox(
          width: double.infinity,
          height: 52,
          child: FilledButton.icon(
            onPressed: () => context.push('/search'),
            icon: const Icon(Icons.search),
            label: Text(l10n.navSearch),
          ),
        ),
      ],
    );
  }
}

class _FavoriteCard extends ConsumerWidget {
  final FavoriteBusiness favorite;
  final int index;
  const _FavoriteCard({required this.favorite, required this.index});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final isDark = theme.brightness == Brightness.dark;

    return GlassContainer(
      borderRadius: AppTheme.radiusLg,
      padding: const EdgeInsets.all(12),
      child: InkWell(
        borderRadius: BorderRadius.circular(AppTheme.radiusLg),
        onTap: () => context.push('/business/${favorite.slug}'),
        child: Row(
          children: [
            ClipRRect(
              borderRadius: BorderRadius.circular(12),
              child: Container(
                width: 64,
                height: 64,
                decoration: BoxDecoration(
                  color: isDark ? AppTheme.slate800 : AppTheme.slate200,
                ),
                child: favorite.coverImageUrl != null
                    ? Image.network(
                        favorite.coverImageUrl!,
                        fit: BoxFit.cover,
                        errorBuilder: (_, _, _) =>
                            Icon(Icons.storefront, color: colorScheme.onSurfaceVariant),
                      )
                    : Icon(Icons.storefront, color: colorScheme.onSurfaceVariant),
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Expanded(
                        child: Text(favorite.name,
                            style: theme.textTheme.titleSmall
                                ?.copyWith(fontWeight: FontWeight.w600),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis),
                      ),
                      IconButton(
                        icon: const Icon(Icons.favorite, color: Colors.red, size: 20),
                        visualDensity: VisualDensity.compact,
                        onPressed: () async {
                          await ref.read(favoritesActionsProvider).remove(favorite.id);
                          if (context.mounted) {
                            ScaffoldMessenger.of(context).showSnackBar(
                              const SnackBar(content: Text('Removed from favorites')),
                            );
                          }
                        },
                      ),
                    ],
                  ),
                  Text('${favorite.city}, ${favorite.country}',
                      style: theme.textTheme.bodySmall
                          ?.copyWith(color: colorScheme.onSurfaceVariant)),
                  const SizedBox(height: 4),
                  Row(
                    children: [
                      Icon(Icons.star_rounded, size: 16, color: Colors.amber),
                      const SizedBox(width: 4),
                      Text(favorite.averageRating.toStringAsFixed(1),
                          style: theme.textTheme.bodySmall),
                      const SizedBox(width: 8),
                      Text('(${favorite.totalReviews})',
                          style: theme.textTheme.bodySmall
                              ?.copyWith(color: colorScheme.onSurfaceVariant)),
                    ],
                  ),
                ],
              ),
            ),
            const Icon(Icons.chevron_right, color: Colors.grey),
          ],
        ),
      ),
    ).animate().fadeIn(duration: 300.ms, delay: (index * 60).ms);
  }
}
