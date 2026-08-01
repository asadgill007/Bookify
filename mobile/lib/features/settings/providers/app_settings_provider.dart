import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../../core/constants/api_constants.dart';
import '../../../core/network/api_client.dart';

/// Persisted app settings (language, currency, theme). The selected
/// language/currency are also pushed to the backend user preferences so they
/// survive across devices, and mirrored in shared_preferences for instant
/// restore on app start.
class AppSettings {
  final Locale locale;
  final String currency;
  final ThemeMode themeMode;

  const AppSettings({
    this.locale = const Locale('en'),
    this.currency = 'USD',
    this.themeMode = ThemeMode.system,
  });

  AppSettings copyWith({
    Locale? locale,
    String? currency,
    ThemeMode? themeMode,
  }) {
    return AppSettings(
      locale: locale ?? this.locale,
      currency: currency ?? this.currency,
      themeMode: themeMode ?? this.themeMode,
    );
  }
}

/// Supported currencies with display symbols.
class CurrencyInfo {
  final String code;
  final String name;
  final String symbol;
  final double rate;

  const CurrencyInfo({
    required this.code,
    required this.name,
    required this.symbol,
    required this.rate,
  });

  factory CurrencyInfo.fromJson(Map<String, dynamic> json) => CurrencyInfo(
        code: json['code'] as String? ?? '',
        name: json['name'] as String? ?? '',
        symbol: json['symbol'] as String? ?? '',
        rate: (json['rate'] as num?)?.toDouble() ?? 1.0,
      );
}

/// App settings notifier — persists locale/currency to shared_preferences and
/// syncs the choice to the backend when the user is signed in.
class AppSettingsNotifier extends StateNotifier<AppSettings> {
  final Ref _ref;

  AppSettingsNotifier(this._ref) : super(const AppSettings()) {
    _restore();
  }

  Future<void> _restore() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final lang = prefs.getString('app_locale') ?? 'en';
      final currency = prefs.getString('app_currency') ?? 'USD';
      final theme = prefs.getString('app_theme');
      state = AppSettings(
        locale: Locale(lang),
        currency: currency,
        themeMode: theme == 'dark'
            ? ThemeMode.dark
            : theme == 'light'
                ? ThemeMode.light
                : ThemeMode.system,
      );
    } catch (_) {
      // keep defaults
    }
  }

  Future<void> setLocale(Locale locale) async {
    state = state.copyWith(locale: locale);
    try {
      final prefs = await SharedPreferences.getInstance();
      await prefs.setString('app_locale', locale.languageCode);
      _syncToBackend();
    } catch (_) {}
  }

  Future<void> setCurrency(String code) async {
    state = state.copyWith(currency: code);
    try {
      final prefs = await SharedPreferences.getInstance();
      await prefs.setString('app_currency', code);
      _syncToBackend();
    } catch (_) {}
  }

  Future<void> setThemeMode(ThemeMode mode) async {
    state = state.copyWith(themeMode: mode);
    try {
      final prefs = await SharedPreferences.getInstance();
      await prefs.setString('app_theme', mode.name);
    } catch (_) {}
  }

  Future<void> _syncToBackend() async {
    try {
      final api = _ref.read(apiClientProvider);
      await api.put(ApiConstants.userPreferences, data: {
        'language': state.locale.languageCode,
        'currency': state.currency,
        'isDarkMode': state.themeMode == ThemeMode.dark,
        'notificationsEnabled': true,
      });
    } catch (_) {
      // offline — local preference still applies
    }
  }
}

final appSettingsProvider = StateNotifierProvider<AppSettingsNotifier, AppSettings>(
  (ref) => AppSettingsNotifier(ref),
);

/// Currencies + rates from the backend (GET /currencies).
final currenciesProvider = FutureProvider<List<CurrencyInfo>>((ref) async {
  final api = ref.watch(apiClientProvider);
  final response = await api.get('/currencies');
  final body = response.data;
  Map<String, dynamic> data;
  if (body is Map<String, dynamic> && body.containsKey('data')) {
    data = body['data'] as Map<String, dynamic>;
  } else if (body is Map<String, dynamic>) {
    data = body;
  } else {
    return fallbackCurrencies;
  }

  final rates = data['rates'] as List<dynamic>? ?? [];
  return rates
      .map((e) => CurrencyInfo.fromJson(e as Map<String, dynamic>))
      .toList();
});

/// Fallback table so the currency selector still works when the backend is
/// offline (matches the backend's static snapshot).
final List<CurrencyInfo> fallbackCurrencies = const [
  CurrencyInfo(code: 'USD', name: 'US Dollar', symbol: r'$', rate: 1.0),
  CurrencyInfo(code: 'EUR', name: 'Euro', symbol: '€', rate: 0.92),
  CurrencyInfo(code: 'GBP', name: 'British Pound', symbol: '£', rate: 0.78),
  CurrencyInfo(code: 'PKR', name: 'Pakistani Rupee', symbol: '₨', rate: 278.5),
  CurrencyInfo(code: 'AED', name: 'UAE Dirham', symbol: 'د.إ', rate: 3.67),
  CurrencyInfo(code: 'INR', name: 'Indian Rupee', symbol: '₹', rate: 83.4),
  CurrencyInfo(code: 'CAD', name: 'Canadian Dollar', symbol: 'C\$', rate: 1.37),
  CurrencyInfo(code: 'SAR', name: 'Saudi Riyal', symbol: '﷼', rate: 3.75),
];

/// Formats a price from a business's base currency into the user's selected
/// display currency using the fetched rates. Falls back to the source price
/// when rates aren't loaded yet.
String formatConvertedPrice(
  double amount,
  String fromCurrency,
  String displayCurrency,
  List<CurrencyInfo> rates,
) {
  if (fromCurrency == displayCurrency || rates.isEmpty) {
    return _formatWithSymbol(amount, fromCurrency);
  }

  final from = rates.where((c) => c.code == fromCurrency).firstOrNull;
  final to = rates.where((c) => c.code == displayCurrency).firstOrNull;
  if (from == null || to == null || from.rate <= 0 || to.rate <= 0) {
    return _formatWithSymbol(amount, fromCurrency);
  }

  // Convert via USD pivot (rates are per-1-USD).
  final usd = amount / from.rate;
  final converted = usd * to.rate;
  return _formatWithSymbol(converted, displayCurrency);
}

String _formatWithSymbol(double amount, String currency) {
  final symbol = _symbolFor(currency);
  final value = amount.toStringAsFixed(2);
  return currency == 'PKR' || currency == 'INR' || currency == 'AED' || currency == 'SAR'
      ? '$value $currency'
      : '$symbol$value';
}

String _symbolFor(String currency) {
  switch (currency) {
    case 'USD':
      return r'$';
    case 'EUR':
      return '€';
    case 'GBP':
      return '£';
    case 'PKR':
      return '₨';
    case 'AED':
      return 'د.إ';
    case 'INR':
      return '₹';
    case 'CAD':
      return r'C$';
    case 'SAR':
      return '﷼';
    default:
      return '$currency ';
  }
}
