import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../../../core/theme/app_theme.dart';

/// Premium Business Detail screen with glassmorphism design.
class BusinessDetailScreen extends ConsumerStatefulWidget {
  final String businessSlug;
  const BusinessDetailScreen({super.key, required this.businessSlug});

  @override
  ConsumerState<BusinessDetailScreen> createState() => _BusinessDetailScreenState();
}

class _BusinessDetailScreenState extends ConsumerState<BusinessDetailScreen> {
  bool _isFavorite = false;

  // ── Mock data matching seed data ──
  final _business = {
    'name': 'Serenity Spa & Wellness',
    'category': 'Spa & Massage',
    'rating': 4.9,
    'reviews': 203,
    'location': '789 Pine Road, San Francisco',
    'coverImage': 'https://images.unsplash.com/photo-1544161515-4ab6ce6db834?w=800',
    'description': 'Award-winning spa offering massages, facials, and holistic body treatments. Escape the city hustle and rejuvenate your body and mind in our serene sanctuary.',
    'verified': true,
    'priceRange': '₹₹₹',
  };

  final List<String> _galleryImages = [
    'https://images.unsplash.com/photo-1544161515-4ab6ce6db834?w=800',
    'https://images.unsplash.com/photo-1540555700478-4be289fbec6d?w=800',
    'https://images.unsplash.com/photo-1560750588-73207b1ef5b8?w=800',
    'https://images.unsplash.com/photo-1600334089648-b0d9d3028eb2?w=800',
  ];

  final List<Map<String, dynamic>> _services = [
    {'name': 'Swedish Massage (60 min)', 'duration': '60 min', 'price': 95.0},
    {'name': 'Deep Tissue Massage (60 min)', 'duration': '60 min', 'price': 110.0},
    {'name': 'Hot Stone Massage (75 min)', 'duration': '75 min', 'price': 130.0},
    {'name': 'Classic Facial', 'duration': '50 min', 'price': 85.0},
    {'name': 'Body Scrub & Wrap', 'duration': '60 min', 'price': 100.0},
  ];

  final List<Map<String, dynamic>> _providers = [
    {'name': 'Aisha Patel', 'specialty': 'Lead Massage Therapist', 'rating': 4.9, 'avatar': 'https://i.pravatar.cc/150?u=aisha'},
    {'name': 'Grace Park', 'specialty': 'Esthetician', 'rating': 4.8, 'avatar': 'https://i.pravatar.cc/150?u=grace'},
    {'name': 'Daniel Foster', 'specialty': 'Spa Therapist', 'rating': 4.7, 'avatar': 'https://i.pravatar.cc/150?u=daniel'},
  ];

  final List<Map<String, dynamic>> _reviews = [
    {'name': 'Emma T.', 'rating': 5, 'comment': 'Best Swedish massage I\'ve ever had. Aisha is incredible!', 'date': '2 weeks ago'},
    {'name': 'Liam S.', 'rating': 5, 'comment': 'The facial was amazing. My skin has never felt smoother.', 'date': '3 weeks ago'},
    {'name': 'Olivia M.', 'rating': 4, 'comment': 'Deep tissue was intense but in a good way. Great value.', 'date': '1 month ago'},
    {'name': 'Noah K.', 'rating': 5, 'comment': 'Body scrub and wrap was heavenly. So relaxing.', 'date': '2 months ago'},
  ];

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final isDark = theme.brightness == Brightness.dark;

    return GradientBackground(
      child: Scaffold(
        backgroundColor: Colors.transparent,
        body: Column(
          children: [
            // ── Scrollable Content ──
            Expanded(
              child: CustomScrollView(
                physics: const BouncingScrollPhysics(),
                slivers: [
                  // ── Header with Cover Image ──
                  SliverAppBar(
                    expandedHeight: 280,
                    pinned: true,
                    backgroundColor: Colors.transparent,
                    surfaceTintColor: Colors.transparent,
                    flexibleSpace: FlexibleSpaceBar(
                      background: Stack(
                        fit: StackFit.expand,
                        children: [
                          // Cover image
                          Image.network(
                            _business['coverImage'] as String,
                            fit: BoxFit.cover,
                            errorBuilder: (_, __, ___) => Container(color: isDark ? AppTheme.slate800 : AppTheme.slate200),
                          ),
                          // Gradient overlay
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
                          // Decorative circles
                          Positioned(
                            top: -40, right: -40,
                            child: Container(
                              width: 160, height: 160,
                              decoration: BoxDecoration(
                                shape: BoxShape.circle,
                                color: AppTheme.indigoLuxury.withValues(alpha: 0.15),
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
                      Padding(
                        padding: const EdgeInsets.only(right: 12, top: 8, bottom: 8),
                        child: GlassContainer(
                          borderRadius: AppTheme.radiusFull,
                          width: 44, height: 44,
                          padding: EdgeInsets.zero,
                          child: const IconButton(
                            icon: Icon(Icons.share_outlined, color: Colors.white, size: 22),
                            onPressed: null,
                          ),
                        ),
                      ),
                    ],
                  ).animate().fadeIn(duration: 400.ms),

                  // ── Business Info Card ──
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
                                    _business['name'] as String,
                                    style: theme.textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.w700),
                                  ),
                                ),
                                if (_business['verified'] == true)
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
                                        Text('Verified', style: TextStyle(color: Colors.white, fontSize: 11, fontWeight: FontWeight.w600)),
                                      ],
                                    ),
                                  ),
                              ],
                            ),
                            const SizedBox(height: 8),
                            Row(
                              children: [
                                const Icon(Icons.star, color: Color(0xFFF59E0B), size: 18),
                                const SizedBox(width: 4),
                                Text('${_business['rating']}', style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700)),
                                const SizedBox(width: 4),
                                Text('(${_business['reviews']} reviews)', style: theme.textTheme.bodySmall?.copyWith(color: colorScheme.onSurfaceVariant)),
                                const Spacer(),
                                Text(_business['priceRange'] as String, style: TextStyle(color: AppTheme.indigoLuxury, fontWeight: FontWeight.w600)),
                              ],
                            ),
                            const SizedBox(height: 8),
                            Row(
                              children: [
                                Icon(Icons.location_on_outlined, size: 16, color: colorScheme.onSurfaceVariant),
                                const SizedBox(width: 4),
                                Expanded(
                                  child: Text(
                                    _business['location'] as String,
                                    style: theme.textTheme.bodySmall?.copyWith(color: colorScheme.onSurfaceVariant),
                                  ),
                                ),
                              ],
                            ),
                          ],
                        ),
                      ),
                    ).animate().fadeIn(duration: 500.ms, delay: 200.ms).slideY(begin: 0.2, curve: Curves.easeOutCubic),
                  ),

                  // ── About Section ──
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                      child: GlassContainer(
                        borderRadius: AppTheme.radiusLg,
                        padding: const EdgeInsets.all(16),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text('About', style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700)),
                            const SizedBox(height: 8),
                            Text(
                              _business['description'] as String,
                              style: theme.textTheme.bodyMedium?.copyWith(color: colorScheme.onSurfaceVariant, height: 1.6),
                            ),
                          ],
                        ),
                      ),
                    ).animate().fadeIn(duration: 400.ms, delay: 300.ms),
                  ),

                  // ── Gallery Section ──
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
                      child: Text('Gallery', style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700)),
                    ).animate().fadeIn(duration: 400.ms, delay: 350.ms),
                  ),
                  SliverToBoxAdapter(
                    child: SizedBox(
                      height: 120,
                      child: ListView.separated(
                        scrollDirection: Axis.horizontal,
                        padding: const EdgeInsets.symmetric(horizontal: 16),
                        itemCount: _galleryImages.length,
                        separatorBuilder: (_, __) => const SizedBox(width: 12),
                        itemBuilder: (context, index) {
                          return GlassContainer(
                            width: 160,
                            borderRadius: AppTheme.radiusLg,
                            padding: EdgeInsets.zero,
                            child: Image.network(
                              _galleryImages[index],
                              fit: BoxFit.cover,
                              errorBuilder: (_, __, ___) => Container(color: isDark ? AppTheme.slate800 : AppTheme.slate200),
                            ),
                          ).animate().fadeIn(duration: 400.ms, delay: (400 + index * 80).ms);
                        },
                      ),
                    ),
                  ),

                  // ── Services Section ──
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(16, 24, 16, 8),
                      child: Text('Services', style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700)),
                    ).animate().fadeIn(duration: 400.ms, delay: 450.ms),
                  ),
                  SliverList(
                    delegate: SliverChildBuilderDelegate(
                      (context, index) {
                        final service = _services[index];
                        return Padding(
                          padding: EdgeInsets.only(
                            left: 16, right: 16,
                            bottom: 12,
                          ),
                          child: GlassContainer(
                            borderRadius: AppTheme.radiusLg,
                            padding: const EdgeInsets.all(16),
                            child: Row(
                              children: [
                                Expanded(
                                  child: Column(
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: [
                                      Text(
                                        service['name'] as String,
                                        style: theme.textTheme.titleSmall?.copyWith(fontWeight: FontWeight.w600),
                                      ),
                                      const SizedBox(height: 4),
                                      Text(
                                        service['duration'] as String,
                                        style: theme.textTheme.bodySmall?.copyWith(color: colorScheme.onSurfaceVariant),
                                      ),
                                    ],
                                  ),
                                ),
                                Text(
                                  '\$${service['price']}',
                                  style: theme.textTheme.titleMedium?.copyWith(
                                    fontWeight: FontWeight.w700,
                                    color: AppTheme.indigoLuxury,
                                  ),
                                ),
                                const SizedBox(width: 12),
                                Container(
                                  padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                                  decoration: BoxDecoration(
                                    gradient: LinearGradient(
                                      colors: [AppTheme.indigoLuxury, const Color(0xFF7C3AED)],
                                      begin: Alignment.topLeft, end: Alignment.bottomRight,
                                    ),
                                    borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                                  ),
                                  child: const Text('Book', style: TextStyle(color: Colors.white, fontSize: 12, fontWeight: FontWeight.w600)),
                                ),
                              ],
                            ),
                          ),
                        ).animate().fadeIn(duration: 400.ms, delay: (500 + index * 60).ms);
                      },
                      childCount: _services.length,
                    ),
                  ),

                  // ── Providers Section ──
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
                      child: Text('Our Team', style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700)),
                    ).animate().fadeIn(duration: 400.ms, delay: 550.ms),
                  ),
                  SliverToBoxAdapter(
                    child: SizedBox(
                      height: 180,
                      child: ListView.separated(
                        scrollDirection: Axis.horizontal,
                        padding: const EdgeInsets.symmetric(horizontal: 16),
                        itemCount: _providers.length,
                        separatorBuilder: (_, __) => const SizedBox(width: 12),
                        itemBuilder: (context, index) {
                          final prov = _providers[index];
                          return GlassContainer(
                            width: 140,
                            borderRadius: AppTheme.radiusLg,
                            padding: const EdgeInsets.all(16),
                            child: Column(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                CircleAvatar(
                                  radius: 30,
                                  backgroundImage: NetworkImage(prov['avatar'] as String),
                                  backgroundColor: isDark ? AppTheme.slate700 : AppTheme.slate200,
                                ),
                                const SizedBox(height: 10),
                                Text(
                                  prov['name'] as String,
                                  style: theme.textTheme.titleSmall?.copyWith(fontWeight: FontWeight.w600),
                                  textAlign: TextAlign.center,
                                  maxLines: 1,
                                  overflow: TextOverflow.ellipsis,
                                ),
                                const SizedBox(height: 4),
                                Text(
                                  prov['specialty'] as String,
                                  style: theme.textTheme.bodySmall?.copyWith(color: colorScheme.onSurfaceVariant, fontSize: 11),
                                  textAlign: TextAlign.center,
                                  maxLines: 2,
                                  overflow: TextOverflow.ellipsis,
                                ),
                                const SizedBox(height: 6),
                                Row(
                                  mainAxisAlignment: MainAxisAlignment.center,
                                  children: [
                                    const Icon(Icons.star, color: Color(0xFFF59E0B), size: 14),
                                    const SizedBox(width: 4),
                                    Text('${prov['rating']}', style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w600)),
                                  ],
                                ),
                              ],
                            ),
                          ).animate().fadeIn(duration: 400.ms, delay: (600 + index * 80).ms);
                        },
                      ),
                    ),
                  ),

                  // ── Reviews Section ──
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(16, 24, 16, 8),
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text('Reviews', style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700)),
                          TextButton(
                            onPressed: () {},
                            child: const Text('See All'),
                          ),
                        ],
                      ),
                    ).animate().fadeIn(duration: 400.ms, delay: 650.ms),
                  ),
                  SliverList(
                    delegate: SliverChildBuilderDelegate(
                      (context, index) {
                        final review = _reviews[index];
                        return Padding(
                          padding: EdgeInsets.only(
                            left: 16, right: 16,
                            bottom: 12,
                          ),
                          child: GlassContainer(
                            borderRadius: AppTheme.radiusLg,
                            padding: const EdgeInsets.all(16),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Row(
                                  children: [
                                    CircleAvatar(
                                      radius: 18,
                                      backgroundColor: AppTheme.indigoLuxury.withValues(alpha: 0.2),
                                      child: Text(
                                        (review['name'] as String)[0],
                                        style: TextStyle(color: AppTheme.indigoLuxury, fontWeight: FontWeight.w600),
                                      ),
                                    ),
                                    const SizedBox(width: 12),
                                    Expanded(
                                      child: Column(
                                        crossAxisAlignment: CrossAxisAlignment.start,
                                        children: [
                                          Text(review['name'] as String, style: theme.textTheme.titleSmall?.copyWith(fontWeight: FontWeight.w600)),
                                          Text(review['date'] as String, style: theme.textTheme.bodySmall?.copyWith(color: colorScheme.onSurfaceVariant, fontSize: 11)),
                                        ],
                                      ),
                                    ),
                                    Row(
                                      children: List.generate(5, (i) => Icon(
                                        i < (review['rating'] as int) ? Icons.star : Icons.star_border,
                                        color: const Color(0xFFF59E0B),
                                        size: 16,
                                      )),
                                    ),
                                  ],
                                ),
                                const SizedBox(height: 10),
                                Text(
                                  review['comment'] as String,
                                  style: theme.textTheme.bodyMedium?.copyWith(color: colorScheme.onSurfaceVariant, height: 1.5),
                                ),
                              ],
                            ),
                          ),
                        ).animate().fadeIn(duration: 400.ms, delay: (700 + index * 80).ms);
                      },
                      childCount: _reviews.length,
                    ),
                  ),

                  // ── Bottom padding ──
                  const SliverToBoxAdapter(child: SizedBox(height: 100)),
                ],
              ),
            ),

            // ── Sticky Book Now Button ──
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
                      onPressed: () => context.push('/booking/${widget.businessSlug}'),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(AppTheme.radiusFull)),
                      child: const Text(
                        'Book Now',
                        style: TextStyle(color: Colors.white, fontSize: 16, fontWeight: FontWeight.w600),
                      ),
                    ),
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}