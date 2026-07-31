import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../auth/providers/auth_provider.dart';

class ProfileScreen extends ConsumerWidget {
  const ProfileScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final authState = ref.watch(authProvider);

    return Scaffold(
      body: SafeArea(
        child: CustomScrollView(
          slivers: [
            SliverAppBar(
              title: const Text('Profile'),
              actions: [
                IconButton(
                  icon: const Icon(Icons.settings_outlined),
                  onPressed: () => context.push('/settings'),
                ),
              ],
            ),
            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Column(
                  children: [
                    // Avatar
                    CircleAvatar(
                      radius: 48,
                      backgroundColor: colorScheme.primaryContainer,
                      child: Icon(Icons.person_rounded, size: 48, color: colorScheme.primary),
                    ),
                    const SizedBox(height: 16),
                    Text(
                      authState.email?.split('@').first ?? 'User',
                      style: theme.textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.bold),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      authState.email ?? '',
                      style: theme.textTheme.bodyMedium?.copyWith(color: colorScheme.onSurfaceVariant),
                    ),
                    const SizedBox(height: 24),
                    // Stats
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                      children: [
                        _buildStat(theme, '12', 'Bookings'),
                        _buildStat(theme, '4', 'Reviews'),
                        _buildStat(theme, '3', 'Favorites'),
                      ],
                    ),
                    const SizedBox(height: 32),
                    // Menu Items
                    Card(
                      child: Column(
                        children: [
                          _buildMenuItem(Icons.person_outline, 'Edit Profile', () => {}),
                          const Divider(height: 1),
                          _buildMenuItem(Icons.calendar_month_outlined, 'My Appointments', () => context.push('/appointments')),
                          const Divider(height: 1),
                          if (authState.role == 'BusinessOwner' ||
                              authState.role == 'Provider')
                            _buildMenuItem(Icons.storefront_outlined, 'My Business',
                                () => context.push('/my-business')),
                          if (authState.role == 'BusinessOwner' ||
                              authState.role == 'Provider')
                            const Divider(height: 1),
                          if (authState.role == 'Admin')
                            _buildMenuItem(Icons.admin_panel_settings_outlined, 'Review Businesses',
                                () => context.push('/admin/review')),
                          if (authState.role == 'Admin')
                            const Divider(height: 1),
                          _buildMenuItem(Icons.favorite_outline, 'Favorites', () => {}),
                          const Divider(height: 1),
                          _buildMenuItem(Icons.notifications_outlined, 'Notifications', () => context.push('/notifications')),
                        ],
                      ),
                    ),
                    const SizedBox(height: 16),
                    Card(
                      child: Column(
                        children: [
                          _buildMenuItem(Icons.help_outline, 'Help Center', () => context.push('/help')),
                          const Divider(height: 1),
                          _buildMenuItem(Icons.info_outline, 'About', () => context.push('/about')),
                        ],
                      ),
                    ),
                    const SizedBox(height: 24),
                    // Logout
                    SizedBox(
                      width: double.infinity,
                      child: OutlinedButton.icon(
                        onPressed: () async {
                          await ref.read(authProvider.notifier).logout();
                          if (context.mounted) context.go('/login');
                        },
                        icon: const Icon(Icons.logout, color: Colors.red),
                        label: const Text('Sign Out', style: TextStyle(color: Colors.red)),
                        style: OutlinedButton.styleFrom(
                          side: const BorderSide(color: Colors.red),
                          minimumSize: const Size(double.infinity, 48),
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
      bottomNavigationBar: NavigationBar(
        selectedIndex: 3,
        onDestinationSelected: (index) {
          switch (index) {
            case 0: context.go('/');
            case 1: context.push('/search');
            case 2: context.push('/appointments');
            case 3: context.go('/profile');
          }
        },
        destinations: const [
          NavigationDestination(icon: Icon(Icons.home_outlined), selectedIcon: Icon(Icons.home_rounded), label: 'Home'),
          NavigationDestination(icon: Icon(Icons.search_outlined), selectedIcon: Icon(Icons.search_rounded), label: 'Search'),
          NavigationDestination(icon: Icon(Icons.calendar_month_outlined), selectedIcon: Icon(Icons.calendar_month_rounded), label: 'Appointments'),
          NavigationDestination(icon: Icon(Icons.person_outline), selectedIcon: Icon(Icons.person_rounded), label: 'Profile'),
        ],
      ),
    );
  }

  Widget _buildStat(ThemeData theme, String value, String label) {
    return Column(
      children: [
        Text(value, style: theme.textTheme.headlineMedium?.copyWith(fontWeight: FontWeight.bold)),
        Text(label, style: theme.textTheme.bodySmall?.copyWith(color: theme.colorScheme.onSurfaceVariant)),
      ],
    );
  }

  Widget _buildMenuItem(IconData icon, String title, VoidCallback onTap) {
    return ListTile(
      leading: Icon(icon),
      title: Text(title),
      trailing: const Icon(Icons.chevron_right),
      onTap: onTap,
    );
  }
}