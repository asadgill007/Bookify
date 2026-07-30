import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

/// Business detail screen using slug (not ID) for API compatibility.
class BusinessDetailScreen extends ConsumerWidget {
  final String businessSlug;

  const BusinessDetailScreen({
    super.key,
    required this.businessSlug,
  });

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;

    return Scaffold(
      body: CustomScrollView(
        slivers: [
          // Cover Image & AppBar
          SliverAppBar(
            expandedHeight: 250,
            pinned: true,
            flexibleSpace: FlexibleSpaceBar(
              background: Container(
                decoration: BoxDecoration(
                  gradient: LinearGradient(
                    begin: Alignment.topCenter,
                    end: Alignment.bottomCenter,
                    colors: [
                      colorScheme.primary.withValues(alpha: 0.8),
                      colorScheme.surface,
                    ],
                  ),
                ),
                child: Center(
                  child: Icon(Icons.store_rounded, size: 80, color: Colors.white.withValues(alpha: 0.8)),
                ),
              ),
            ),
            actions: [
              IconButton(
                icon: const Icon(Icons.favorite_outline),
                onPressed: () {},
              ),
              IconButton(
                icon: const Icon(Icons.share_outlined),
                onPressed: () {},
              ),
            ],
          ),
          // Content
          SliverToBoxAdapter(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Business Name & Rating
                  Text(
                    businessSlug.replaceAll('-', ' ').toUpperCase(),
                    style: theme.textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 8),
                  Row(
                    children: [
                      const Icon(Icons.star_rounded, size: 20, color: Colors.amber),
                      const SizedBox(width: 4),
                      Text('4.5', style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold)),
                      const SizedBox(width: 4),
                      Text('(128 reviews)', style: theme.textTheme.bodyMedium?.copyWith(color: colorScheme.onSurfaceVariant)),
                      const Spacer(),
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                        decoration: BoxDecoration(
                          color: Colors.green.withValues(alpha: 0.1),
                          borderRadius: BorderRadius.circular(20),
                        ),
                        child: const Text('Open', style: TextStyle(color: Colors.green, fontSize: 12)),
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  Row(
                    children: [
                      Icon(Icons.location_on_outlined, size: 16, color: colorScheme.onSurfaceVariant),
                      const SizedBox(width: 4),
                      Text('123 Main Street, New York', style: theme.textTheme.bodyMedium?.copyWith(color: colorScheme.onSurfaceVariant)),
                    ],
                  ),
                  const SizedBox(height: 24),

                  // About
                  Text('About', style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold)),
                  const SizedBox(height: 8),
                  Text(
                    'Premium service provider offering top-quality experiences. Book your appointment today.',
                    style: theme.textTheme.bodyLarge?.copyWith(color: colorScheme.onSurfaceVariant, height: 1.5),
                  ),
                  const SizedBox(height: 24),

                  // Services
                  Text('Services', style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold)),
                  const SizedBox(height: 12),
                  _buildServiceCard(theme, colorScheme, 'Haircut & Styling', '\$45', '60 min', () {
                    context.push('/booking/$businessSlug/service1');
                  }),
                  const SizedBox(height: 8),
                  _buildServiceCard(theme, colorScheme, 'Beard Trim & Shape', '\$25', '30 min', () {
                    context.push('/booking/$businessSlug/service2');
                  }),
                  const SizedBox(height: 8),
                  _buildServiceCard(theme, colorScheme, 'Premium Hair Package', '\$80', '90 min', () {
                    context.push('/booking/$businessSlug/service3');
                  }),
                  const SizedBox(height: 24),

                  // Business Hours
                  Text('Business Hours', style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold)),
                  const SizedBox(height: 12),
                  Card(
                    child: Padding(
                      padding: const EdgeInsets.all(16),
                      child: Column(
                        children: [
                          _buildHourRow('Monday - Friday', '9:00 AM - 8:00 PM'),
                          const Divider(),
                          _buildHourRow('Saturday', '10:00 AM - 6:00 PM'),
                          const Divider(),
                          _buildHourRow('Sunday', 'Closed'),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: 24),

                  // Location
                  Text('Location', style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold)),
                  const SizedBox(height: 12),
                  Card(
                    child: Container(
                      height: 150,
                      decoration: BoxDecoration(
                        color: colorScheme.surfaceContainerHighest,
                        borderRadius: BorderRadius.circular(16),
                      ),
                      child: Center(
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Icon(Icons.map_outlined, size: 40, color: colorScheme.primary),
                            const SizedBox(height: 8),
                            Text('Map View - Future Integration', style: theme.textTheme.bodyMedium?.copyWith(color: colorScheme.onSurfaceVariant)),
                          ],
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(height: 100),
                ],
              ),
            ),
          ),
        ],
      ),
      // Floating Book Button
      bottomNavigationBar: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: colorScheme.surface,
          border: Border(top: BorderSide(color: colorScheme.outlineVariant)),
        ),
        child: SafeArea(
          child: FilledButton(
            onPressed: () => context.push('/booking/$businessSlug/service1'),
            style: FilledButton.styleFrom(minimumSize: const Size(double.infinity, 52)),
            child: const Text('Book Appointment', style: TextStyle(fontSize: 16)),
          ),
        ),
      ),
    );
  }

  Widget _buildServiceCard(ThemeData theme, ColorScheme colorScheme, String name, String price, String duration, VoidCallback onTap) {
    return Card(
      child: ListTile(
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        title: Text(name, style: const TextStyle(fontWeight: FontWeight.w600)),
        subtitle: Text('$duration • $price'),
        trailing: FilledButton.tonal(
          onPressed: onTap,
          child: const Text('Book'),
        ),
      ),
    );
  }

  Widget _buildHourRow(String days, String hours) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(days),
          Text(hours, style: const TextStyle(fontWeight: FontWeight.w600)),
        ],
      ),
    );
  }
}