import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:go_router/go_router.dart';
import '../../../core/theme/app_theme.dart';

/// Premium Home / Discovery screen with glassmorphism design.
class HomeScreen extends ConsumerStatefulWidget {
  const HomeScreen({super.key});

  @override
  ConsumerState<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends ConsumerState<HomeScreen> {
  final _searchController = TextEditingController();
  int _selectedCategory = -1;

  final List<Map<String, dynamic>> _categories = [
    {'name': 'Haircut', 'icon': Icons.content_cut, 'color': const Color(0xFF6366F1)},
    {'name': 'Spa', 'icon': Icons.spa, 'color': const Color(0xFF8B5CF6)},
    {'name': 'Dental', 'icon': Icons.medical_services, 'color': const Color(0xFF06B6D4)},
    {'name': 'Fitness', 'icon': Icons.fitness_center, 'color': const Color(0xFF10B981)},
    {'name': 'Nails', 'icon': Icons.brush, 'color': const Color(0xFFF472B6)},
    {'name': 'Skincare', 'icon': Icons.face, 'color': const Color(0xFFF59E0B)},
    {'name': 'Cleaning', 'icon': Icons.cleaning_services, 'color': const Color(0xFF3B82F6)},
    {'name': 'Training', 'icon': Icons.sports_gymnastics, 'color': const Color(0xFFEF4444)},
  ];

  final List<Map<String, dynamic>> _featuredBusinesses = [
    {
      'name': 'Serenity Spa & Wellness',
      'category': 'Spa & Massage',
      'rating': 4.9,
      'reviews': 203,
      'image': 'https://images.unsplash.com/photo-1544161515-4ab6ce6db834?w=800',
      'location': 'San Francisco',
      'verified': true,
      'price': '₹₹₹',
    },
    {
      'name': 'Luxe Hair Studio',
      'category': 'Haircut & Barbershop',
      'rating': 4.8,
      'reviews': 127,
      'image': 'https://images.unsplash.com/photo-1560066984-138dadb4c035?w=800',
      'location': 'New York',
      'verified': true,
      'price': '₹₹₹',
    },
    {
      'name': 'Zen Yoga Studio',
      'category': 'Fitness & Yoga',
      'rating': 4.9,
      'reviews': 178,
      'image': 'https://images.unsplash.com/photo-1544367551-2e0f4b6f0e2f?w=800',
      'location': 'Seattle',
      'verified': true,
      'price': '₹₹',
    },
    {
      'name': 'Peak Fitness Center',
      'category': 'Fitness & Yoga',
      'rating': 4.7,
      'reviews': 156,
      'image': 'https://images.unsplash.com/photo-1571902943202-507ec2618e8f?w=800',
      'location': 'Chicago',
      'verified': true,
      'price': '₹₹',
    },
    {
      'name': 'Radiance Skin Clinic',
      'category': 'Skincare & Aesthetics',
      'rating': 4.7,
      'reviews': 134,
      'image': 'https://images.unsplash.com/photo-1570172619644-dfd03ed5d881?w=800',
      'location': 'London',
      'verified': true,
      'price': '₹₹₹₹',
    },
    {
      'name': 'Elite Barber Shop',
      'category': 'Haircut & Barbershop',
      'rating': 4.6,
      'reviews': 89,
      'image': 'https://images.unsplash.com/photo-1503951918675-f72ffbfa538a?w=800',
      'location': 'Los Angeles',
      'verified': true,
      'price': '₹₹',
    },
  ];

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final isDark = theme.brightness == Brightness.dark;

    return GradientBackground(
      child: Scaffold(
        backgroundColor: Colors.transparent,
        body: SafeArea(
          child: RefreshIndicator(
            onRefresh: () async => Future.delayed(const Duration(seconds: 1)),
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
                          suffixIcon: _searchController.text.isNotEmpty
                              ? IconButton(
                                  icon: const Icon(Icons.clear, size: 20),
                                  onPressed: () {
                                    _searchController.clear();
                                    setState(() {});
                                  },
                                )
                              : Container(
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
                        onChanged: (_) => setState(() {}),
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
                SliverToBoxAdapter(
                  child: SizedBox(
                    height: 110,
                    child: ListView.separated(
                      scrollDirection: Axis.horizontal,
                      padding: const EdgeInsets.symmetric(horizontal: 16),
                      itemCount: _categories.length,
                      separatorBuilder: (_, __) => const SizedBox(width: 12),
                      itemBuilder: (context, index) {
                        final cat = _categories[index];
                        final isSelected = _selectedCategory == index;
                        return GestureDetector(
                          onTap: () => setState(() => _selectedCategory = isSelected ? -1 : index),
                          child: AnimatedContainer(
                            duration: 200.ms,
                            width: 80,
                            decoration: BoxDecoration(
                              borderRadius: BorderRadius.circular(AppTheme.radiusLg),
                              gradient: isSelected
                                  ? LinearGradient(
                                      colors: [cat['color'] as Color, (cat['color'] as Color).withValues(alpha: 0.7)],
                                      begin: Alignment.topLeft,
                                      end: Alignment.bottomRight,
                                    )
                                  : null,
                              color: isSelected ? null : (isDark ? AppTheme.glassDark : AppTheme.glassLight),
                              border: Border.all(
                                color: isSelected
                                    ? Colors.transparent
                                    : (isDark ? AppTheme.glassStrokeDark : AppTheme.glassStrokeLight),
                              ),
                              boxShadow: isSelected ? AppTheme.indigoGlowShadow : AppTheme.softShadow,
                            ),
                            child: Column(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                Icon(
                                  cat['icon'] as IconData,
                                  color: isSelected ? Colors.white : (cat['color'] as Color),
                                  size: 28,
                                ),
                                const SizedBox(height: 6),
                                Text(
                                  cat['name'] as String,
                                  style: TextStyle(
                                    fontSize: 11,
                                    fontWeight: FontWeight.w600,
                                    color: isSelected ? Colors.white : colorScheme.onSurface,
                                  ),
                                  textAlign: TextAlign.center,
                                  maxLines: 1,
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
                          onPressed: () {},
                          child: const Text('See All'),
                        ),
                      ],
                    ),
                  ).animate().fadeIn(duration: 400.ms, delay: 200.ms),
                ),
                SliverToBoxAdapter(
                  child: SizedBox(
                    height: 320,
                    child: ListView.separated(
                      scrollDirection: Axis.horizontal,
                      padding: const EdgeInsets.symmetric(horizontal: 16),
                      itemCount: _featuredBusinesses.length,
                      separatorBuilder: (_, __) => const SizedBox(width: 16),
                      itemBuilder: (context, index) {
                        final biz = _featuredBusinesses[index];
                        return _buildBusinessCard(context, biz, index, isDark, colorScheme);
                      },
                    ),
                  ),
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
    Map<String, dynamic> biz,
    int index,
    bool isDark,
    ColorScheme colorScheme,
  ) {
    return GestureDetector(
      onTap: () => context.push('/business/${biz['name']?.toString().toLowerCase().replaceAll(' ', '-')}'),
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
                      image: DecorationImage(
                        image: NetworkImage(biz['image'] as String),
                        fit: BoxFit.cover,
                      ),
                    ),
                    child: Container(
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
                if (biz['verified'] == true)
                  Positioned(
                    top: 12,
                    left: 12,
                    child: Container(
                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                      decoration: BoxDecoration(
                        color: AppTheme.success.withValues(alpha: 0.9),
                        borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                      ),
                      child: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(Icons.verified, color: Colors.white, size: 12),
                          const SizedBox(width: 4),
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
                          '${biz['rating']}',
                          style: TextStyle(
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
                      biz['name'] as String,
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
                            biz['location'] as String,
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
                        Text(
                          biz['price'] as String,
                          style: TextStyle(
                            fontSize: 12,
                            fontWeight: FontWeight.w600,
                            color: AppTheme.indigoLuxury,
                          ),
                        ),
                        const Spacer(),
                        Icon(Icons.chat_bubble_outline, size: 12, color: colorScheme.onSurfaceVariant),
                        const SizedBox(width: 4),
                        Text(
                          '${biz['reviews']}',
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