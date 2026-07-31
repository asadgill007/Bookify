import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../../../core/theme/app_theme.dart';
import '../providers/business_detail_provider.dart';

/// Premium Business Detail screen, wired to the real API by slug.
class BusinessDetailScreen extends ConsumerStatefulWidget {
  final String businessSlug;
  const BusinessDetailScreen({super.key, required this.businessSlug});

  @override
  ConsumerState<BusinessDetailScreen> createState() =>
      _BusinessDetailScreenState();
}

class _BusinessDetailScreenState extends ConsumerState<BusinessDetailScreen> {
  bool _isFavorite = false;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final isDark = theme.brightness == Brightness.dark;
    final detailAsync = ref.watch(businessDetailProvider(widget.businessSlug));

    return GradientBackground(
      child: Scaffold(
        backgroundColor: Colors.transparent,
        body: detailAsync.when(
          loading: () => Scaffold(
            backgroundColor: Colors.transparent,
            appBar: AppBar(backgroundColor: Colors.transparent),
            body: const Center(child: CircularProgressIndicator()),
          ),
          error: (err, _) => Scaffold(
            backgroundColor: Colors.transparent,
            appBar: AppBar(backgroundColor: Colors.transparent),
            body: Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.error_outline, size: 48, color: colorScheme.error),
                  const SizedBox(height: 16),
                  Text('Could not load this business',
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
                    onPressed: () =>
                        ref.invalidate(businessDetailProvider(widget.businessSlug)),
                    child: const Text('Retry'),
                  ),
                ],
              ),
            ),
          ),
          data: (business) => _buildDetail(context, theme, colorScheme, isDark, business),
        ),
      ),
    );
  }

  Widget _buildDetail(
    BuildContext context,
    ThemeData theme,
    ColorScheme colorScheme,
    bool isDark,
    BusinessDetail business,
  ) {
    final coverUrl = business.coverImageUrl ??
        (business.gallery.isNotEmpty ? business.gallery.first : null);
    final gallery = business.gallery.isNotEmpty
        ? business.gallery
        : (coverUrl != null ? [coverUrl] : <String>[]);

    return Column(
      children: [
        Expanded(
          child: CustomScrollView(
            physics: const BouncingScrollPhysics(),
            slivers: [
              // Header with cover image
              SliverAppBar(
                expandedHeight: 280,
                pinned: true,
                backgroundColor: Colors.transparent,
                surfaceTintColor: Colors.transparent,
                flexibleSpace: FlexibleSpaceBar(
                  background: Stack(
                    fit: StackFit.expand,
                    children: [
                      if (coverUrl != null)
                        Image.network(
                          coverUrl,
                          fit: BoxFit.cover,
                          errorBuilder: (_, _, _) => Container(
                            color: isDark ? AppTheme.slate800 : AppTheme.slate200,
                            child: Icon(Icons.storefront, size: 64,
                                color: colorScheme.onSurfaceVariant),
                          ),
                        )
                      else
                        Container(
                          color: isDark ? AppTheme.slate800 : AppTheme.slate200,
                          child: Icon(Icons.storefront, size: 64,
                              color: colorScheme.onSurfaceVariant),
                        ),
                      DecoratedBox(
                        decoration: BoxDecoration(
                          gradient: LinearGradient(
                            begin: Alignment.topCenter,
                            end: Alignment.bottomCenter,
                            colors: [
                              Colors.transparent,
                              Colors.black.withValues(alpha: 0.7),
                            ],
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
                leading: Padding(
                  padding: const EdgeInsets.all(8),
                  child: GlassContainer(
                    borderRadius: AppTheme.radiusFull,
                    width: 44, height: 44,
                    padding: EdgeInsets.zero,
                    child: IconButton(
                      icon: const Icon(Icons.arrow_back, color: Colors.white, size: 22),
                      onPressed: () => context.pop(),
                    ),
                  ),
                ),
                actions: [
                  Padding(
                    padding: const EdgeInsets.all(8),
                    child: GlassContainer(
                      borderRadius: AppTheme.radiusFull,
                      width: 44, height: 44,
                      padding: EdgeInsets.zero,
                      child: IconButton(
                        icon: Icon(
                          _isFavorite ? Icons.favorite : Icons.favorite_border,
                          color: _isFavorite ? Colors.red : Colors.white,
                          size: 22,
                        ),
                        onPressed: () => setState(() => _isFavorite = !_isFavorite),
                      ),
                    ),
                  ),
                ],
              ).animate().fadeIn(duration: 400.ms),

              // Business info card
              SliverToBoxAdapter(
                child: Padding(
                  padding: const EdgeInsets.fromLTRB(16, -40, 16, 16),
                  child: GlassContainer(
                    borderRadius: AppTheme.radiusXl,
                    padding: const EdgeInsets.all(20),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            Expanded(
                              child: Text(
                                business.name,
                                style: theme.textTheme.headlineSmall?.copyWith(
                                    fontWeight: FontWeight.w700),
                              ),
                            ),
                            if (business.isVerified)
                              Container(
                                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                                decoration: BoxDecoration(
                                  color: AppTheme.success.withValues(alpha: 0.9),
                                  borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                                ),
                                child: const Row(
                                  mainAxisSize: MainAxisSize.min,
                                  children: [
                                    Icon(Icons.verified, color: Colors.white, size: 14),
                                    SizedBox(width: 4),
                                    Text('Verified', style: TextStyle(
                                        color: Colors.white, fontSize: 11,
                                        fontWeight: FontWeight.w600)),
                                  ],
                                ),
                              ),
                          ],
                        ),
                        if (business.categories.isNotEmpty) ...[
                          const SizedBox(height: 6),
                          Wrap(
                            spacing: 6,
                            runSpacing: 6,
                            children: business.categories.map((c) => Container(
                              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                              decoration: BoxDecoration(
                                color: AppTheme.indigoLuxury.withValues(alpha: 0.1),
                                borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                              ),
                              child: Text(c, style: TextStyle(
                                  fontSize: 11, fontWeight: FontWeight.w600,
                                  color: AppTheme.indigoLuxury)),
                            )).toList(),
                          ),
                        ],
                        const SizedBox(height: 8),
                        Row(
                          children: [
                            const Icon(Icons.star, color: Color(0xFFF59E0B), size: 18),
                            const SizedBox(width: 4),
                            Text(business.averageRating.toStringAsFixed(1),
                                style: theme.textTheme.titleMedium?.copyWith(
                                    fontWeight: FontWeight.w700)),
                            const SizedBox(width: 4),
                            Text('(${business.totalReviews} reviews)',
                                style: theme.textTheme.bodySmall?.copyWith(
                                    color: colorScheme.onSurfaceVariant)),
                          ],
                        ),
                        const SizedBox(height: 8),
                        Row(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Icon(Icons.location_on_outlined, size: 16,
                                color: colorScheme.onSurfaceVariant),
                            const SizedBox(width: 4),
                            Expanded(
                              child: Text(
                                [
                                  business.addressLine1,
                                  business.city,
                                  business.state,
                                  business.postalCode,
                                  business.country,
                                ].where((s) => s != null && s.isNotEmpty).join(', '),
                                style: theme.textTheme.bodySmall?.copyWith(
                                    color: colorScheme.onSurfaceVariant),
                              ),
                            ),
                          ],
                        ),
                        if (business.website != null) ...[
                          const SizedBox(height: 6),
                          Row(
                            children: [
                              Icon(Icons.language, size: 16,
                                  color: colorScheme.onSurfaceVariant),
                              const SizedBox(width: 4),
                              Expanded(
                                child: Text(business.website!,
                                    style: theme.textTheme.bodySmall?.copyWith(
                                        color: AppTheme.indigoLuxury)),
                              ),
                            ],
                          ),
                        ],
                      ],
                    ),
                  ),
                ),
              ).animate().fadeIn(duration: 500.ms, delay: 200.ms).slideY(
                  begin: 0.2, curve: Curves.easeOutCubic),

              // About
              if (business.description != null && business.description!.isNotEmpty)
                SliverToBoxAdapter(
                  child: Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                    child: GlassContainer(
                      borderRadius: AppTheme.radiusLg,
                      padding: const EdgeInsets.all(16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text('About',
                              style: theme.textTheme.titleMedium
                                  ?.copyWith(fontWeight: FontWeight.w700)),
                          const SizedBox(height: 8),
                          Text(business.description!,
                              style: theme.textTheme.bodyMedium?.copyWith(
                                  color: colorScheme.onSurfaceVariant, height: 1.6)),
                        ],
                      ),
                    ),
                  ).animate().fadeIn(duration: 400.ms, delay: 300.ms),
                ),

              // Gallery
              if (gallery.isNotEmpty) ...[
                SliverToBoxAdapter(
                  child: Padding(
                    padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
                    child: Text('Gallery',
                        style: theme.textTheme.titleMedium
                            ?.copyWith(fontWeight: FontWeight.w700)),
                  ).animate().fadeIn(duration: 400.ms, delay: 350.ms),
                ),
                SliverToBoxAdapter(
                  child: SizedBox(
                    height: 120,
                    child: ListView.separated(
                      scrollDirection: Axis.horizontal,
                      padding: const EdgeInsets.symmetric(horizontal: 16),
                      itemCount: gallery.length,
                      separatorBuilder: (_, _) => const SizedBox(width: 12),
                      itemBuilder: (context, index) {
                        return GlassContainer(
                          width: 160,
                          borderRadius: AppTheme.radiusLg,
                          padding: EdgeInsets.zero,
                          child: Image.network(
                            gallery[index],
                            fit: BoxFit.cover,
                            errorBuilder: (_, _, _) => Container(
                              color: isDark ? AppTheme.slate800 : AppTheme.slate200,
                            ),
                          ),
                        ).animate().fadeIn(
                            duration: 400.ms, delay: (400 + index * 80).ms);
                      },
                    ),
                  ),
                ),
              ],

              // Services
              SliverToBoxAdapter(
                child: Padding(
                  padding: const EdgeInsets.fromLTRB(16, 24, 16, 8),
                  child: Text('Services',
                      style: theme.textTheme.titleMedium
                          ?.copyWith(fontWeight: FontWeight.w700)),
                ).animate().fadeIn(duration: 400.ms, delay: 450.ms),
              ),
              SliverList(
                delegate: SliverChildBuilderDelegate(
                  (context, index) {
                    final service = business.services[index];
                    return Padding(
                      padding: const EdgeInsets.only(left: 16, right: 16, bottom: 12),
                      child: GlassContainer(
                        borderRadius: AppTheme.radiusLg,
                        padding: const EdgeInsets.all(16),
                        child: Row(
                          children: [
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(service.name,
                                      style: theme.textTheme.titleSmall?.copyWith(
                                          fontWeight: FontWeight.w600)),
                                  const SizedBox(height: 4),
                                  Text(
                                    '${service.durationMinutes} min',
                                    style: theme.textTheme.bodySmall?.copyWith(
                                        color: colorScheme.onSurfaceVariant),
                                  ),
                                ],
                              ),
                            ),
                            Text(
                              '${business.currency == 'USD' ? '\$' : ''}'
                              '${service.price.toStringAsFixed(2)}',
                              style: theme.textTheme.titleMedium?.copyWith(
                                fontWeight: FontWeight.w700,
                                color: AppTheme.indigoLuxury,
                              ),
                            ),
                            const SizedBox(width: 12),
                            GestureDetector(
                              onTap: business.providers.isEmpty
                                  ? null
                                  : () => context.push(
                                        '/booking/${business.slug}/${service.id}',
                                      ),
                              child: Container(
                                padding: const EdgeInsets.symmetric(
                                    horizontal: 12, vertical: 6),
                                decoration: BoxDecoration(
                                  gradient: LinearGradient(
                                    colors: [AppTheme.indigoLuxury, const Color(0xFF7C3AED)],
                                    begin: Alignment.topLeft, end: Alignment.bottomRight,
                                  ),
                                  borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                                ),
                                child: Text(
                                  business.providers.isEmpty ? 'No staff' : 'Book',
                                  style: const TextStyle(
                                      color: Colors.white, fontSize: 12,
                                      fontWeight: FontWeight.w600),
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                    ).animate().fadeIn(duration: 400.ms, delay: (500 + index * 60).ms);
                  },
                  childCount: business.services.length,
                ),
              ),

              // Providers
              if (business.providers.isNotEmpty) ...[
                SliverToBoxAdapter(
                  child: Padding(
                    padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
                    child: Text('Our Team',
                        style: theme.textTheme.titleMedium
                            ?.copyWith(fontWeight: FontWeight.w700)),
                  ).animate().fadeIn(duration: 400.ms, delay: 550.ms),
                ),
                SliverToBoxAdapter(
                  child: SizedBox(
                    height: 180,
                    child: ListView.separated(
                      scrollDirection: Axis.horizontal,
                      padding: const EdgeInsets.symmetric(horizontal: 16),
                      itemCount: business.providers.length,
                      separatorBuilder: (_, _) => const SizedBox(width: 12),
                      itemBuilder: (context, index) {
                        final prov = business.providers[index];
                        return GlassContainer(
                          width: 140,
                          borderRadius: AppTheme.radiusLg,
                          padding: const EdgeInsets.all(16),
                          child: Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              CircleAvatar(
                                radius: 30,
                                backgroundImage: prov.avatarUrl != null
                                    ? NetworkImage(prov.avatarUrl!)
                                    : null,
                                backgroundColor: isDark
                                    ? AppTheme.slate700
                                    : AppTheme.slate200,
                                child: prov.avatarUrl == null
                                    ? Icon(Icons.person, color: colorScheme.onSurfaceVariant)
                                    : null,
                              ),
                              const SizedBox(height: 10),
                              Text(prov.fullName,
                                  style: theme.textTheme.titleSmall?.copyWith(
                                      fontWeight: FontWeight.w600),
                                  textAlign: TextAlign.center,
                                  maxLines: 1,
                                  overflow: TextOverflow.ellipsis),
                              const SizedBox(height: 4),
                              Text(prov.title ?? 'Provider',
                                  style: theme.textTheme.bodySmall?.copyWith(
                                      color: colorScheme.onSurfaceVariant, fontSize: 11),
                                  textAlign: TextAlign.center,
                                  maxLines: 2,
                                  overflow: TextOverflow.ellipsis),
                            ],
                          ),
                        ).animate().fadeIn(duration: 400.ms, delay: (600 + index * 80).ms);
                      },
                    ),
                  ),
                ),
              ],

              // Opening hours
              if (business.openingHours.isNotEmpty) ...[
                SliverToBoxAdapter(
                  child: Padding(
                    padding: const EdgeInsets.fromLTRB(16, 24, 16, 8),
                    child: Text('Opening Hours',
                        style: theme.textTheme.titleMedium
                            ?.copyWith(fontWeight: FontWeight.w700)),
                  ).animate().fadeIn(duration: 400.ms, delay: 650.ms),
                ),
                SliverToBoxAdapter(
                  child: Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 16),
                    child: GlassContainer(
                      borderRadius: AppTheme.radiusLg,
                      padding: const EdgeInsets.all(16),
                      child: Column(
                        children: business.openingHours.map((h) {
                          return Padding(
                            padding: const EdgeInsets.symmetric(vertical: 4),
                            child: Row(
                              children: [
                                SizedBox(
                                  width: 110,
                                  child: Text(h.dayOfWeek,
                                      style: theme.textTheme.bodySmall?.copyWith(
                                          fontWeight: FontWeight.w600)),
                                ),
                                Expanded(
                                  child: Text(
                                    h.isClosed
                                        ? 'Closed'
                                        : '${h.openTime} – ${h.closeTime}',
                                    style: theme.textTheme.bodySmall?.copyWith(
                                      color: h.isClosed
                                          ? colorScheme.onSurfaceVariant
                                          : colorScheme.onSurface,
                                    ),
                                  ),
                                ),
                              ],
                            ),
                          );
                        }).toList(),
                      ),
                    ),
                  ).animate().fadeIn(duration: 400.ms, delay: 700.ms),
                ),
              ],

              const SliverToBoxAdapter(child: SizedBox(height: 100)),
            ],
          ),
        ),

        // Sticky Book Now button
        if (business.services.isNotEmpty && business.providers.isNotEmpty)
          Container(
            padding: const EdgeInsets.fromLTRB(16, 12, 16, 24),
            decoration: BoxDecoration(
              gradient: LinearGradient(
                begin: Alignment.topCenter,
                end: Alignment.bottomCenter,
                colors: [
                  isDark ? AppTheme.slate900.withValues(alpha: 0) : AppTheme.slate50.withValues(alpha: 0),
                  isDark ? AppTheme.slate900 : AppTheme.slate50,
                ],
              ),
            ),
            child: SafeArea(
              top: false,
              child: SizedBox(
                width: double.infinity,
                height: 56,
                child: DecoratedBox(
                  decoration: BoxDecoration(
                    gradient: LinearGradient(
                      colors: [AppTheme.indigoLuxury, const Color(0xFF7C3AED)],
                      begin: Alignment.topLeft, end: Alignment.bottomRight,
                    ),
                    borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                    boxShadow: AppTheme.indigoGlowShadow,
                  ),
                  child: MaterialButton(
                    onPressed: () => context.push(
                      '/booking/${business.slug}/${business.services.first.id}',
                    ),
                    shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(AppTheme.radiusFull)),
                    child: const Text(
                      'Book Now',
                      style: TextStyle(
                          color: Colors.white, fontSize: 16, fontWeight: FontWeight.w600),
                    ),
                  ),
                ),
              ),
            ),
          ),
      ],
    );
  }
}
