import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/constants/api_constants.dart';
import '../../../core/network/api_client.dart';
import '../../auth/providers/auth_provider.dart';
import '../providers/app_settings_provider.dart';

class SettingsScreen extends ConsumerStatefulWidget {
  const SettingsScreen({super.key});

  @override
  ConsumerState<SettingsScreen> createState() => _SettingsScreenState();
}

class _SettingsScreenState extends ConsumerState<SettingsScreen> {
  bool _pushNotifications = true;
  bool _emailNotifications = false;
  bool _isDeleting = false;

  Future<void> _showChangePasswordDialog() async {
    final currentPassword = TextEditingController();
    final newPassword = TextEditingController();
    final confirmPassword = TextEditingController();
    String? error;
    var isSubmitting = false;

    await showDialog<void>(
      context: context,
      builder: (ctx) => StatefulBuilder(
        builder: (ctx, setState) {
          Future<void> submit() async {
            if (newPassword.text.length < 8) {
              setState(() => error = 'New password must be at least 8 characters.');
              return;
            }
            if (newPassword.text != confirmPassword.text) {
              setState(() => error = 'Passwords do not match.');
              return;
            }
            setState(() {
              isSubmitting = true;
              error = null;
            });
            try {
              final api = ref.read(apiClientProvider);
              final response = await api.put(
                ApiConstants.changePassword,
                data: {
                  'currentPassword': currentPassword.text,
                  'newPassword': newPassword.text,
                },
              );
              if (!ctx.mounted) return;
              if (response.statusCode == 200) {
                Navigator.pop(ctx);
                if (!mounted) return;
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Password changed successfully')),
                );
              } else {
                setState(() => error = 'Current password is incorrect.');
              }
            } catch (_) {
              setState(() => error = 'Network error. Please try again.');
            } finally {
              if (ctx.mounted) setState(() => isSubmitting = false);
            }
          }

          return AlertDialog(
            title: const Text('Change Password'),
            content: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                TextField(
                  controller: currentPassword,
                  obscureText: true,
                  decoration: const InputDecoration(labelText: 'Current Password'),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: newPassword,
                  obscureText: true,
                  decoration: const InputDecoration(labelText: 'New Password'),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: confirmPassword,
                  obscureText: true,
                  decoration: const InputDecoration(labelText: 'Confirm New Password'),
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
                    : const Text('Update Password'),
              ),
            ],
          );
        },
      ),
    );
  }

  Future<void> _showLanguageDialog() async {
    final settings = ref.read(appSettingsProvider);
    await showDialog<void>(
      context: context,
      builder: (ctx) => SimpleDialog(
        title: const Text('Select Language'),
        children: [
          SimpleDialogOption(
            onPressed: () async {
              await ref.read(appSettingsProvider.notifier).setLocale(const Locale('en'));
              if (ctx.mounted) Navigator.pop(ctx);
            },
            child: Row(
              children: [
                const Icon(Icons.language, size: 20),
                const SizedBox(width: 12),
                Text('English',
                    style: TextStyle(
                      fontWeight: settings.locale.languageCode == 'en'
                          ? FontWeight.bold
                          : FontWeight.normal,
                      color: settings.locale.languageCode == 'en'
                          ? Theme.of(ctx).colorScheme.primary
                          : null,
                    )),
                if (settings.locale.languageCode == 'en')
                  const Icon(Icons.check, size: 18),
              ],
            ),
          ),
          SimpleDialogOption(
            onPressed: () async {
              await ref.read(appSettingsProvider.notifier).setLocale(const Locale('ur'));
              if (ctx.mounted) Navigator.pop(ctx);
            },
            child: Row(
              children: [
                const Icon(Icons.language, size: 20),
                const SizedBox(width: 12),
                Text('اردو (Urdu)',
                    style: TextStyle(
                      fontWeight: settings.locale.languageCode == 'ur'
                          ? FontWeight.bold
                          : FontWeight.normal,
                      color: settings.locale.languageCode == 'ur'
                          ? Theme.of(ctx).colorScheme.primary
                          : null,
                    )),
                if (settings.locale.languageCode == 'ur')
                  const Icon(Icons.check, size: 18),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _showCurrencyDialog() async {
    final settings = ref.read(appSettingsProvider);
    final currenciesAsync = ref.read(currenciesProvider);
    final currencies = currenciesAsync.valueOrNull ?? fallbackCurrencies;

    await showDialog<void>(
      context: context,
      builder: (ctx) => SimpleDialog(
        title: const Text('Select Currency'),
        children: currencies.map((c) {
          return SimpleDialogOption(
            onPressed: () async {
              await ref
                  .read(appSettingsProvider.notifier)
                  .setCurrency(c.code);
              if (ctx.mounted) Navigator.pop(ctx);
            },
            child: Row(
              children: [
                SizedBox(
                  width: 40,
                  child: Text(c.symbol,
                      style: const TextStyle(fontSize: 16)),
                ),
                Expanded(child: Text(c.name)),
                if (settings.currency == c.code)
                  const Icon(Icons.check, size: 18,
                      color: Colors.green),
              ],
            ),
          );
        }).toList(),
      ),
    );
  }

  Future<void> _confirmDeleteAccount() async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Delete Account'),
        content: const Text('This permanently deletes your account and all data. This cannot be undone.'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Cancel'),
          ),
          FilledButton(
            style: FilledButton.styleFrom(backgroundColor: Colors.red),
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('Delete'),
          ),
        ],
      ),
    );

    if (confirmed != true) return;

    setState(() => _isDeleting = true);
    try {
      final api = ref.read(apiClientProvider);
      await api.delete(ApiConstants.deleteAccount);
      if (!mounted) return;
      await ref.read(authProvider.notifier).logout();
      if (mounted) context.go('/login');
    } catch (_) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Could not delete account. Please try again.')),
      );
    } finally {
      if (mounted) setState(() => _isDeleting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final settings = ref.watch(appSettingsProvider);
    final themeMode = settings.themeMode;
    final languageLabel =
        settings.locale.languageCode == 'ur' ? 'اردو (Urdu)' : 'English';

    return Scaffold(
      appBar: AppBar(title: const Text('Settings')),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          // Appearance
          Text('Appearance', style: theme.textTheme.titleSmall?.copyWith(color: colorScheme.primary)),
          const SizedBox(height: 8),
          Card(
            child: Column(
              children: [
                ListTile(
                  leading: const Icon(Icons.dark_mode_outlined),
                  title: const Text('Dark Mode'),
                  subtitle: Text(themeMode == ThemeMode.system ? 'System' : themeMode == ThemeMode.dark ? 'Dark' : 'Light'),
                  trailing: SegmentedButton<ThemeMode>(
                    segments: const [
                      ButtonSegment(value: ThemeMode.system, icon: Icon(Icons.brightness_auto)),
                      ButtonSegment(value: ThemeMode.light, icon: Icon(Icons.light_mode)),
                      ButtonSegment(value: ThemeMode.dark, icon: Icon(Icons.dark_mode)),
                    ],
                    selected: {themeMode},
                    onSelectionChanged: (selected) => ref
                        .read(appSettingsProvider.notifier)
                        .setThemeMode(selected.first),
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 24),

          // Language
          Text('Language', style: theme.textTheme.titleSmall?.copyWith(color: colorScheme.primary)),
          const SizedBox(height: 8),
          Card(
            child: Column(
              children: [
                ListTile(
                  leading: const Icon(Icons.language_outlined),
                  title: const Text('Language'),
                  subtitle: Text(languageLabel),
                  trailing: const Icon(Icons.chevron_right),
                  onTap: _showLanguageDialog,
                ),
                const Divider(height: 1),
                ListTile(
                  leading: const Icon(Icons.currency_exchange_outlined),
                  title: const Text('Currency'),
                  subtitle: Text(settings.currency),
                  trailing: const Icon(Icons.chevron_right),
                  onTap: _showCurrencyDialog,
                ),
              ],
            ),
          ),
          const SizedBox(height: 24),

          // Notifications
          Text('Notifications', style: theme.textTheme.titleSmall?.copyWith(color: colorScheme.primary)),
          const SizedBox(height: 8),
          Card(
            child: Column(
              children: [
                SwitchListTile(
                  title: const Text('Push Notifications'),
                  subtitle: const Text('Receive booking updates'),
                  value: _pushNotifications,
                  onChanged: (value) => setState(() => _pushNotifications = value),
                ),
                const Divider(height: 1),
                SwitchListTile(
                  title: const Text('Email Notifications'),
                  subtitle: const Text('Receive promotional emails'),
                  value: _emailNotifications,
                  onChanged: (value) => setState(() => _emailNotifications = value),
                ),
              ],
            ),
          ),
          const SizedBox(height: 24),

          // Account
          Text('Account', style: theme.textTheme.titleSmall?.copyWith(color: colorScheme.primary)),
          const SizedBox(height: 8),
          Card(
            child: Column(
              children: [
                ListTile(
                  leading: const Icon(Icons.person_outline),
                  title: const Text('Edit Profile'),
                  trailing: const Icon(Icons.chevron_right),
                  onTap: () => context.push('/profile'),
                ),
                const Divider(height: 1),
                ListTile(
                  leading: const Icon(Icons.lock_outline),
                  title: const Text('Change Password'),
                  trailing: const Icon(Icons.chevron_right),
                  onTap: _showChangePasswordDialog,
                ),
                const Divider(height: 1),
                ListTile(
                  leading: const Icon(Icons.delete_outline, color: Colors.red),
                  title: Text(
                    _isDeleting ? 'Deleting account...' : 'Delete Account',
                    style: const TextStyle(color: Colors.red),
                  ),
                  trailing: _isDeleting
                      ? const SizedBox(
                          height: 18,
                          width: 18,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        )
                      : null,
                  onTap: _isDeleting ? null : _confirmDeleteAccount,
                ),
              ],
            ),
          ),
          const SizedBox(height: 24),

          // Support
          Text('Support', style: theme.textTheme.titleSmall?.copyWith(color: colorScheme.primary)),
          const SizedBox(height: 8),
          Card(
            child: Column(
              children: [
                ListTile(
                  leading: const Icon(Icons.help_outline),
                  title: const Text('Help Center'),
                  trailing: const Icon(Icons.chevron_right),
                  onTap: () => context.push('/help'),
                ),
                const Divider(height: 1),
                ListTile(
                  leading: const Icon(Icons.info_outline),
                  title: const Text('About'),
                  trailing: const Icon(Icons.chevron_right),
                  onTap: () => context.push('/about'),
                ),
                const Divider(height: 1),
                ListTile(
                  leading: const Icon(Icons.privacy_tip_outlined),
                  title: const Text('Privacy Policy'),
                  trailing: const Icon(Icons.chevron_right),
                  onTap: () => context.push('/privacy'),
                ),
                const Divider(height: 1),
                ListTile(
                  leading: const Icon(Icons.description_outlined),
                  title: const Text('Terms of Service'),
                  trailing: const Icon(Icons.chevron_right),
                  onTap: () => context.push('/terms'),
                ),
              ],
            ),
          ),
          const SizedBox(height: 32),
        ],
      ),
    );
  }
}