import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:bookify/app.dart';
import 'package:bookify/features/settings/providers/app_settings_provider.dart';
import 'package:bookify/l10n/generated/app_localizations.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  test('Locale switch persists to SharedPreferences and survives restart', () async {
    SharedPreferences.setMockInitialValues({});
    final prefs = await SharedPreferences.getInstance();

    final container = ProviderContainer();
    addTearDown(container.dispose);

    // Provider creation is lazy: reading it here instantiates the notifier,
    // which kicks off an async _restore(). Settle the event queue so the
    // restore finishes before we mutate state (otherwise it can race the
    // switch below and overwrite it).
    container.read(appSettingsProvider);
    await pumpEventQueue();

    // Defaults to English.
    expect(container.read(appSettingsProvider).locale.languageCode, 'en');

    // Switch to Urdu — mirrors tapping the Urdu option in Settings.
    await container.read(appSettingsProvider.notifier).setLocale(const Locale('ur'));

    expect(container.read(appSettingsProvider).locale.languageCode, 'ur');
    expect(prefs.getString('app_locale'), 'ur',
        reason: 'must persist to SharedPreferences');

    // "Restart": a fresh container restores the persisted locale.
    final container2 = ProviderContainer();
    addTearDown(container2.dispose);
    container2.read(appSettingsProvider);
    await pumpEventQueue();
    expect(container2.read(appSettingsProvider).locale.languageCode, 'ur',
        reason: 'restored from SharedPreferences after restart');
  });

  testWidgets('BookifyApp switches MaterialApp locale and Urdu applies RTL', (tester) async {
    SharedPreferences.setMockInitialValues({});

    await tester.pumpWidget(
      const ProviderScope(child: BookifyApp()),
    );
    await tester.pump(const Duration(seconds: 3));
    await tester.pumpAndSettle();

    // Use the context of a rendered Text widget, which sits BELOW the
    // Localizations scope (the root BookifyApp element is above it).
    AppLocalizations l10n =
        AppLocalizations.of(tester.element(find.byType(Text).first));
    expect(l10n.localeName, 'en');

    // Switch to Urdu via the settings notifier (same path the UI uses).
    final container =
        ProviderScope.containerOf(tester.element(find.byType(Text).first));
    await container.read(appSettingsProvider.notifier).setLocale(const Locale('ur'));
    await tester.pumpAndSettle();

    l10n = AppLocalizations.of(tester.element(find.byType(Text).first));
    expect(l10n.localeName, 'ur',
        reason: 'localizations delegate resolves Urdu after switch');

    // RTL: Urdu is a right-to-left script, so the resolved text direction
    // must flip to RTL across the widget tree.
    final direction =
        Directionality.of(tester.element(find.byType(Text).first));
    expect(direction, TextDirection.rtl,
        reason: 'Urdu locale must resolve to RTL text direction');
  });
}
