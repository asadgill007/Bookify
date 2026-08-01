import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../auth/providers/auth_provider.dart';
import '../../appointments/screens/appointments_screen.dart';
import '../../../core/constants/api_constants.dart';
import '../../../core/network/api_client.dart';

class ProfileScreen extends ConsumerWidget {
  const ProfileScreen({super.key});

  Future<void> _showEditProfileDialog(BuildContext context, WidgetRef ref) async {
    final authState = ref.read(authProvider);
    final firstName = TextEditingController(text: authState.email?.split('@').first ?? '');
    final lastName = TextEditingController();
    final phone = TextEditingController();
    var isSubmitting = false;
    String? error;

    await showDialog<void>(
      context: context,
      builder: (ctx) => StatefulBuilder(
        builder: (ctx, setState) {
          Future<void> submit() async {
            if (firstName.text.trim().isEmpty || lastName.text.trim().isEmpty) {
              setState(() => error = 'First and last name are required.');
              return;
            }
            setState(() {
              isSubmitting = true;
              error = null;
            });
            try {
              final api = ref.read(apiClientProvider);
              final response = await api.put(
                ApiConstants.userProfile,
                data: {
                  'firstName': firstName.text.trim(),
                  'lastName': lastName.text.trim(),
                  'phoneNumber': phone.text.trim().isEmpty ? null : phone.text.trim(),
                },
              );
              if (!ctx.mounted) return;
              if (response.statusCode == 200) {
                Navigator.pop(ctx);
                if (context.mounted) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text('Profile updated successfully')),
                  );
                }
              } else {
                setState(() => error = 'Could not update profile. Please try again.');
              }
            } catch (_) {
              setState(() => error = 'Network error. Please try again.');
            } finally {
              if (ctx.mounted) setState(() => isSubmitting = false);
            }
          }

          return AlertDialog(
            title: const Text('Edit Profile'),
            content: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                TextField(
                  controller: firstName,
                  decoration: const InputDecoration(labelText: 'First Name'),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: lastName,
                  decoration: const InputDecoration(labelText: 'Last Name'),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: phone,
                  keyboardType: TextInputType.phone,
                  decoration: const InputDecoration(labelText: 'Phone Number (optional)'),
                ),
                if (error != null) ...[
                  const SizedBox(height: 12),
                  Text(error!, style: TextStyle(color: Theme.of(ctx).colorScheme.error)),
                ],
              ],
            ),
            actions: [
              TextButton(
                onPressed: () => Navigator.pop(ctx),
                child: const Text('Cancel'),
              ),
              FilledButton(
                onPressed: isSubmitting ? null : submit,
                child: isSubmitting
                    ? const SizedBox(height: 18, width: 18, child: CircularProgressIndicator(strokeWidth: 2))
                    : const Text('Save'),
              ),
            ],
          );
        },
      ),
    );
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final authState = ref.watch(authProvider);
    final appointmentsAsync = ref.watch(appointmentsProvider);
    final bookingsCount = appointmentsAsync.valueOrNull?.length ?? 0;

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
                    // Stats (bookings count is real data from the API)
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                      children: [
                        _buildStat(theme, '$bookingsCount', 'Bookings'),
                      ],
                    ),
                    const SizedBox(height: 32),
                    // Menu Items
                    Card(
                      child: Column(
                        children: [
                          _buildMenuItem(Icons.person_outline, 'Edit Profile',
                              () => _showEditProfileDialog(context, ref)),
                          const Divider(height: 1),
                          _buildMenuItem(Icons.calendar_month_outlined, 'My Appointments', () => context.push('/appointments')),
                          const Divider(height: 1),
                          _buildMenuItem(Icons.hourglass_bottom, 'My Waitlist',
                              () => context.push('/my-waitlist')),
                          const Divider(height: 1),
                          _buildMenuItem(Icons.repeat, 'Recurring Bookings',
                              () => context.push('/my-recurring')),
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
                          _buildMenuItem(Icons.favorite_outline, 'Favorites',
                              () => context.push('/favorites')),
                          const Divider(height: 1),
                          _buildMenuItem(Icons.notifications_outlined, 'Notifications', () => context.push('/notifications')),
                        ],
                      ),
                    ),
                    const SizedBox(height: 16),
                    Card(
                      child: Column(
                        children: [
                          _buildMenuItem(Icons.support_agent_outlined, 'Contact Support',
                              () => context.push('/contact-support')),
                          const Divider(height: 1),
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