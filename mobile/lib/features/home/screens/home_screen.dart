import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:go_router/go_router.dart';
import '../../../core/theme/app_theme.dart';
import '../../categories/providers/categories_provider.dart';
import '../../business/providers/businesses_provider.dart';
import '../../auth/providers/auth_provider.dart';

/// Premium Home / Discovery screen with glassmorphism design,
/// wired to the real categories and businesses APIs.
class HomeScreen extends ConsumerStatefulWidget {
  const HomeScreen({super.key});

  @override
  ConsumerState<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends ConsumerState<HomeScreen> {
  final _searchController = TextEditingController();

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  IconData _categoryIcon(String? iconName) {
    switch (iconName?.toLowerCase()) {
      case 'content_cut':
      case 'salon':
      case 'beauty':
        return Icons.content_cut;
      case 'spa':
        return Icons.spa;
      case 'medical_services':
      case 'doctor':
        return Icons.medical_services;
      case 'fitness_center':
      case 'gym':
        return Icons.fitness_center;
      case 'brush':
      case 'nail':
        return Icons.brush;
      case 'face':
      case 'skincare':
        return Icons.face;
      case 'cleaning_services':
        return Icons.cleaning_services;
      case 'sports_gymnastics':
        return Icons.sports_gymnastics;
      case 'restaurant':
      case 'dining':
        return Icons.restaurant;
      case 'hotel':
        return Icons.hotel;
      default:
        return Icons.category;
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final isDark = theme.brightness == Brightness.dark;
    final categoriesAsync = ref.watch(categoriesProvider);
    final businessesAsync = ref.watch(businessesProvider);
    final authState = ref.watch(authProvider);
    final isBusiness = authState.role == 'BusinessOwner' ||
        authState.role == 'Provider';
    final isAdmin = authState.role == 'Admin';

    return GradientBackground(
      child: Scaffold(
        backgroundColor: Colors.transparent,
        floatingActionButton: FloatingActionButton.extended(
          heroTag: 'chatFab',
          onPressed: () => context.push('/chat'),
          backgroundColor: AppTheme.indigoLuxury,
          foregroundColor: Colors.white,
          icon: const Icon(Icons.auto_awesome_rounded),
          label: const Text('AI Assistant'),
        ),
        body: SafeArea(
          child: RefreshIndicator(
            onRefresh: () async {
              ref.invalidate(categoriesProvider);
              ref.invalidate(businessesProvider);
            },
            child: CustomScrollView(
              physics: const BouncingScrollPhysics(),
              slivers: [
                SliverAppBar(
                  floating: true,
                  backgroundColor: Colors.transparent,
                  surfaceTintColor: Colors.transparent,
                  title: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'Discover',
                        style: theme.textTheme.headlineMedium?.copyWith(
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                      Text(
                        'Find your perfect service',
                        style: theme.textTheme.bodySmall?.copyWith(
                          color: colorScheme.onSurfaceVariant,
                        ),
                      ),
                    ],
                  ),
                  actions: [
                    if (isBusiness)
                      IconButton(
                        icon: const Icon(Icons.storefront_outlined),
                        tooltip: 'My Business',
                        onPressed: () => context.push('/my-business'),
                      ),
                    if (isAdmin)
                      IconButton(
                        icon: const Icon(Icons.admin_panel_settings_outlined),
                        tooltip: 'Review Businesses',
                        onPressed: () => context.push('/admin/review'),
                      ),
                    IconButton(
                      icon: const Icon(Icons.notifications_outlined),
                      onPressed: () => context.push('/notifications'),
                    ),
                    IconButton(
                      icon: const Icon(Icons.settings_outlined),
                      onPressed: () => context.push('/settings'),
                    ),
                  ],
                ),

                SliverToBoxAdapter(
                  child: Padding(
                    padding: const EdgeInsets.fromLTRB(16, 8, 16, 16),
                    child: GlassContainer(
                      borderRadius: AppTheme.radiusFull,
                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                      child: TextField(
                        controller: _searchController,
                        decoration: InputDecoration(
                          hintText: 'Search for services...',
                          hintStyle: TextStyle(color: colorScheme.onSurfaceVariant),
                          prefixIcon: Icon(Icons.search, color: AppTheme.indigoLuxury),
                          suffixIcon: Container(
                            margin: const EdgeInsets.all(6),
                            decoration: BoxDecoration(
                              gradient: LinearGradient(
                                colors: [AppTheme.indigoLuxury, const Color(0xFF7C3AED)],
                                begin: Alignment.topLeft,
                                end: Alignment.bottomRight,
                              ),
                              borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                            ),
                            child: const Icon(Icons.auto_awesome, color: Colors.white, size: 18),
                          ),
                          border: InputBorder.none,
                          enabledBorder: InputBorder.none,
                          focusedBorder: InputBorder.none,
                        ),
                        style: theme.textTheme.bodyLarge,
                        textInputAction: TextInputAction.search,
                        onSubmitted: (value) {
                          if (value.trim().isNotEmpty) {
                            context.push(
                              '/search',
                              extra: SearchIntent(query: value.trim()),
                            );
                          }
                        },
                      ),
                    ),
                  ).animate().fadeIn(duration: 400.ms).slideY(begin: 0.2, curve: Curves.easeOutCubic),
                ),

                SliverToBoxAdapter(
                  child: Padding(
                    padding: const EdgeInsets.only(left: 16, bottom: 8),
                    child: Text(
                      'Categories',
                      style: theme.textTheme.titleLarge?.copyWith(
                        fontWeight: FontWeight.w700,
                        color: colorScheme.onSurface,
                      ),
                    ),
                  ).animate().fadeIn(duration: 400.ms, delay: 100.ms),
                ),
                categoriesAsync.when(
                  loading: () => const SliverToBoxAdapter(
                    child: SizedBox(
                      height: 110,
                      child: Center(child: CircularProgressIndicator()),
                    ),
                  ),
                  error: (err, _) => const SliverToBoxAdapter(child: SizedBox(height: 60)),
                  data: (categories) => SliverToBoxAdapter(
                    child: SizedBox(
                      height: 110,
                      child: ListView.separated(
                        scrollDirection: Axis.horizontal,
                        padding: const EdgeInsets.symmetric(horizontal: 16),
                        itemCount: categories.length,
                        separatorBuilder: (_, _) => const SizedBox(width: 12),
                        itemBuilder: (context, index) {
                          final cat = categories[index];
                          final color = index % 2 == 0
                              ? AppTheme.indigoLuxury
                              : const Color(0xFF7C3AED);
                          return GestureDetector(
                            onTap: () => context.push(
                              '/search',
                              extra: SearchIntent(
                                query: cat.name,
                                categorySlug: cat.slug,
                              ),
                            ),
                            child: GlassContainer(
                              width: 80,
                              borderRadius: AppTheme.radiusLg,
                              child: Column(
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: [
                                  Icon(
                                    _categoryIcon(cat.iconName),
                                    color: color,
                                    size: 28,
                                  ),
                                  const SizedBox(height: 6),
                                  Text(
                                    cat.name,
                                    style: TextStyle(
                                      fontSize: 11,
                                      fontWeight: FontWeight.w600,
                                      color: colorScheme.onSurface,
                                    ),
                                    textAlign: TextAlign.center,
                                    maxLines: 2,
                                    overflow: TextOverflow.ellipsis,
                                  ),
                                ],
                              ),
                            ),
                          ).animate().fadeIn(
                            duration: 400.ms,
                            delay: (150 + index * 50).ms,
                          );
                        },
                      ),
                    ),
                  ),
                ),

                SliverToBoxAdapter(
                  child: Padding(
                    padding: const EdgeInsets.fromLTRB(16, 24, 16, 8),
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(
                          'Featured Businesses',
                          style: theme.textTheme.titleLarge?.copyWith(
                            fontWeight: FontWeight.w700,
                            color: colorScheme.onSurface,
                          ),
                        ),
                        TextButton(
                          onPressed: () => context.push('/search'),
                          child: const Text('See All'),
                        ),
                      ],
                    ),
                  ).animate().fadeIn(duration: 400.ms, delay: 200.ms),
                ),
                businessesAsync.when(
                  loading: () => const SliverToBoxAdapter(
                    child: SizedBox(
                      height: 320,
                      child: Center(child: CircularProgressIndicator()),
                    ),
                  ),
                  error: (err, _) => SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.all(32),
                      child: Column(
                        children: [
                          Icon(Icons.error_outline, color: colorScheme.error, size: 48),
                          const SizedBox(height: 12),
                          Text(
                            'Could not load businesses. Is the backend running?',
                            textAlign: TextAlign.center,
                            style: theme.textTheme.bodyMedium?.copyWith(
                              color: colorScheme.onSurfaceVariant,
                            ),
                          ),
                          const SizedBox(height: 8),
                          Text(
                            err.toString(),
                            textAlign: TextAlign.center,
                            style: theme.textTheme.bodySmall?.copyWith(
                              color: colorScheme.onSurfaceVariant,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                  data: (businesses) {
                    if (businesses.isEmpty) {
                      return const SliverToBoxAdapter(child: SizedBox(height: 80));
                    }
                    return SliverToBoxAdapter(
                      child: SizedBox(
                        height: 320,
                        child: ListView.separated(
                          scrollDirection: Axis.horizontal,
                          padding: const EdgeInsets.symmetric(horizontal: 16),
                          itemCount: businesses.length,
                          separatorBuilder: (_, _) => const SizedBox(width: 16),
                          itemBuilder: (context, index) {
                            final biz = businesses[index];
                            return _buildBusinessCard(
                              context, biz, index, isDark, colorScheme);
                          },
                        ),
                      ),
                    );
                  },
                ),

                const SliverToBoxAdapter(child: SizedBox(height: 100)),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildBusinessCard(
    BuildContext context,
    Business biz,
    int index,
    bool isDark,
    ColorScheme colorScheme,
  ) {
    return GestureDetector(
      onTap: () => context.push('/business/${biz.slug}'),
      child: GlassContainer(
        width: 280,
        borderRadius: AppTheme.radiusXl,
        padding: EdgeInsets.zero,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Stack(
              children: [
                ClipRRect(
                  borderRadius: BorderRadius.vertical(top: Radius.circular(AppTheme.radiusXl)),
                  child: Container(
                    height: 160,
                    decoration: BoxDecoration(
                      image: biz.coverImageUrl != null
                          ? DecorationImage(
                              image: NetworkImage(biz.coverImageUrl!),
                              fit: BoxFit.cover,
                              onError: (_, _) {},
                            )
                          : null,
                      color: isDark ? AppTheme.slate800 : AppTheme.slate200,
                    ),
                    child: biz.coverImageUrl == null
                        ? Icon(Icons.storefront, size: 48, color: colorScheme.onSurfaceVariant)
                        : Container(
                            decoration: BoxDecoration(
                              gradient: LinearGradient(
                                begin: Alignment.topCenter,
                                end: Alignment.bottomCenter,
                                colors: [
                                  Colors.transparent,
                                  Colors.black.withValues(alpha: 0.6),
                                ],
                              ),
                            ),
                          ),
                  ),
                ),
                if (biz.isVerified)
                  Positioned(
                    top: 12,
                    left: 12,
                    child: Container(
                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                      decoration: BoxDecoration(
                        color: AppTheme.success.withValues(alpha: 0.9),
                        borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                      ),
                      child: const Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(Icons.verified, color: Colors.white, size: 12),
                          SizedBox(width: 4),
                          Text(
                            'Verified',
                            style: TextStyle(
                              color: Colors.white,
                              fontSize: 10,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                Positioned(
                  top: 12,
                  right: 12,
                  child: Container(
                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                    decoration: BoxDecoration(
                      color: Colors.black.withValues(alpha: 0.7),
                      borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(Icons.star, color: const Color(0xFFF59E0B), size: 14),
                        const SizedBox(width: 4),
                        Text(
                          biz.averageRating.toStringAsFixed(1),
                          style: const TextStyle(
                            color: Colors.white,
                            fontSize: 12,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
            Expanded(
              child: Padding(
                padding: const EdgeInsets.all(14),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      biz.name,
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.w700,
                        color: colorScheme.onSurface,
                      ),
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                    const SizedBox(height: 4),
                    Row(
                      children: [
                        Icon(Icons.location_on_outlined, size: 14, color: colorScheme.onSurfaceVariant),
                        const SizedBox(width: 4),
                        Expanded(
                          child: Text(
                            '${biz.city}, ${biz.country}',
                            style: TextStyle(
                              fontSize: 12,
                              color: colorScheme.onSurfaceVariant,
                            ),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 8),
                    Row(
                      children: [
                        if (biz.category != null) ...[
                          Text(
                            biz.category!,
                            style: TextStyle(
                              fontSize: 12,
                              fontWeight: FontWeight.w600,
                              color: AppTheme.indigoLuxury,
                            ),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ],
                        const Spacer(),
                        Icon(Icons.chat_bubble_outline, size: 12, color: colorScheme.onSurfaceVariant),
                        const SizedBox(width: 4),
                        Text(
                          '${biz.totalReviews}',
                          style: TextStyle(
                            fontSize: 11,
                            color: colorScheme.onSurfaceVariant,
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    ).animate().fadeIn(
      duration: 500.ms,
      delay: (250 + index * 80).ms,
    ).slideX(begin: 0.1, curve: Curves.easeOutCubic);
  }
}
