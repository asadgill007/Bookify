import 'dart:ui' as ui;
import 'package:flutter/material.dart';

/// ═══════════════════════════════════════════════════════
/// Bookify Premium Design System
/// ───────────────────────────────────────────────────────
/// Inspired by Material Design 3 × Apple HIG × Glassmorphism
/// 
/// Light Mode:  Slate-50 backgrounds, glass-light surfaces
/// Dark Mode:   Slate-900 backgrounds, glass-dark surfaces
/// AMOLED:      Pure black base with indigo accents
/// ═══════════════════════════════════════════════════════
class AppTheme {
  AppTheme._();

  // ── Brand Colors ─────────────────────────────────────
  static const Color indigoLuxury = Color(0xFF4F46E5);
  static const Color indigoLight = Color(0xFF6366F1);
  static const Color indigoDark = Color(0xFF818CF8);

  static const Color slate50 = Color(0xFFF8FAFC);
  static const Color slate100 = Color(0xFFF1F5F9);
  static const Color slate200 = Color(0xFFE2E8F0);
  static const Color slate300 = Color(0xFFCBD5E1);
  static const Color slate400 = Color(0xFF94A3B8);
  static const Color slate500 = Color(0xFF64748B);
  static const Color slate600 = Color(0xFF475569);
  static const Color slate700 = Color(0xFF334155);
  static const Color slate800 = Color(0xFF1E293B);
  static const Color slate900 = Color(0xFF0F172A);

  static const Color amoledBlack = Color(0xFF000000);
  static const Color success = Color(0xFF10B981);
  static const Color warning = Color(0xFFF59E0B);
  static const Color error = Color(0xFFEF4444);

  // ── Glass Colors ─────────────────────────────────────
  static Color glassLight = Colors.white.withValues(alpha: 0.7);
  static Color glassDark = const Color(0xFF0F172A).withValues(alpha: 0.8);
  static Color glassStrokeLight = Colors.white.withValues(alpha: 0.5);
  static Color glassStrokeDark = indigoLuxury.withValues(alpha: 0.15);

  // ── Typography ───────────────────────────────────────
  static const String headlineFont = 'PlusJakartaSans';
  static const String bodyFont = 'Inter';

  // ── Spacing ──────────────────────────────────────────
  static const double unit = 8;
  static const double gutter = 24;
  static const double marginMobile = 16;
  static const double stackSm = 12;
  static const double stackMd = 24;
  static const double stackLg = 48;

  // ── Border Radius ────────────────────────────────────
  static double get radiusSm => 8;
  static double get radiusMd => 12;
  static double get radiusLg => 16;
  static double get radiusXl => 24;
  static double get radiusFull => 9999;

  // ── Shadows ──────────────────────────────────────────
  static List<BoxShadow> get softShadow => [
        BoxShadow(
          color: Colors.black.withValues(alpha: 0.06),
          blurRadius: 12,
          offset: const Offset(0, 4),
        ),
        BoxShadow(
          color: Colors.black.withValues(alpha: 0.03),
          blurRadius: 4,
          offset: const Offset(0, 1),
        ),
      ];

  static List<BoxShadow> get elevatedShadow => [
        BoxShadow(
          color: Colors.black.withValues(alpha: 0.08),
          blurRadius: 24,
          offset: const Offset(0, 12),
          spreadRadius: -4,
        ),
      ];

  static List<BoxShadow> get indigoGlowShadow => [
        BoxShadow(
          color: indigoLuxury.withValues(alpha: 0.15),
          blurRadius: 20,
          offset: const Offset(0, 0),
        ),
      ];

  static List<BoxShadow> get darkElevatedShadow => [
        BoxShadow(
          color: indigoLuxury.withValues(alpha: 0.12),
          blurRadius: 20,
          offset: const Offset(0, 8),
          spreadRadius: -4,
        ),
      ];

  // ═══════════════════════════════════════════════════════
  // LIGHT THEME
  // ═══════════════════════════════════════════════════════
  static ThemeData get lightTheme {
    final colorScheme = ColorScheme.light(
      primary: indigoLuxury,
      onPrimary: Colors.white,
      primaryContainer: indigoLight,
      onPrimaryContainer: Colors.white,
      secondary: const Color(0xFF7C3AED),
      onSecondary: Colors.white,
      secondaryContainer: const Color(0xFFA78BFA),
      tertiary: const Color(0xFF0891B2),
      onTertiary: Colors.white,
      error: error,
      onError: Colors.white,
      surface: slate50,
      onSurface: slate900,
      surfaceContainerHighest: slate200,
      onSurfaceVariant: slate500,
      outline: slate300,
      outlineVariant: slate200,
      inverseSurface: slate800,
      inversePrimary: indigoDark,
    );

    return ThemeData(
      useMaterial3: true,
      brightness: Brightness.light,
      colorScheme: colorScheme,
      fontFamily: bodyFont,

      // ── Text Theme ──
      textTheme: TextTheme(
        displayLarge: const TextStyle(
          fontFamily: headlineFont,
          fontSize: 56,
          fontWeight: FontWeight.w700,
          height: 1.14,
          letterSpacing: -0.02,
        ),
        displayMedium: const TextStyle(
          fontFamily: headlineFont,
          fontSize: 40,
          fontWeight: FontWeight.w700,
          height: 1.15,
        ),
        displaySmall: const TextStyle(
          fontFamily: headlineFont,
          fontSize: 32,
          fontWeight: FontWeight.w700,
          height: 1.25,
          letterSpacing: -0.01,
        ),
        headlineLarge: const TextStyle(
          fontFamily: headlineFont,
          fontSize: 28,
          fontWeight: FontWeight.w700,
          height: 1.29,
        ),
        headlineMedium: const TextStyle(
          fontFamily: headlineFont,
          fontSize: 24,
          fontWeight: FontWeight.w600,
          height: 1.33,
        ),
        headlineSmall: const TextStyle(
          fontFamily: headlineFont,
          fontSize: 20,
          fontWeight: FontWeight.w600,
          height: 1.4,
        ),
        titleLarge: const TextStyle(
          fontFamily: headlineFont,
          fontSize: 20,
          fontWeight: FontWeight.w600,
          height: 1.4,
        ),
        titleMedium: const TextStyle(
          fontFamily: headlineFont,
          fontSize: 16,
          fontWeight: FontWeight.w600,
          height: 1.5,
        ),
        titleSmall: const TextStyle(
          fontFamily: headlineFont,
          fontSize: 14,
          fontWeight: FontWeight.w600,
          height: 1.43,
          letterSpacing: 0.01,
        ),
        bodyLarge: const TextStyle(
          fontFamily: bodyFont,
          fontSize: 18,
          fontWeight: FontWeight.w400,
          height: 1.56,
        ),
        bodyMedium: const TextStyle(
          fontFamily: bodyFont,
          fontSize: 16,
          fontWeight: FontWeight.w400,
          height: 1.5,
        ),
        bodySmall: const TextStyle(
          fontFamily: bodyFont,
          fontSize: 14,
          fontWeight: FontWeight.w400,
          height: 1.43,
        ),
        labelLarge: const TextStyle(
          fontFamily: bodyFont,
          fontSize: 14,
          fontWeight: FontWeight.w600,
          height: 1.43,
          letterSpacing: 0.01,
        ),
        labelMedium: const TextStyle(
          fontFamily: bodyFont,
          fontSize: 12,
          fontWeight: FontWeight.w500,
          height: 1.33,
          letterSpacing: 0.04,
        ),
        labelSmall: const TextStyle(
          fontFamily: bodyFont,
          fontSize: 11,
          fontWeight: FontWeight.w500,
          height: 1.45,
          letterSpacing: 0.06,
        ),
      ),

      // ── AppBar ──
      appBarTheme: AppBarTheme(
        centerTitle: false,
        elevation: 0,
        scrolledUnderElevation: 0.5,
        backgroundColor: Colors.transparent,
        foregroundColor: colorScheme.onSurface,
        surfaceTintColor: Colors.transparent,
        titleTextStyle: const TextStyle(
          fontFamily: headlineFont,
          fontSize: 20,
          fontWeight: FontWeight.w600,
          color: slate900,
        ),
      ),

      // ── Card ──
      cardTheme: CardThemeData(
        elevation: 0,
        color: glassLight,
        shadowColor: Colors.transparent,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(radiusLg),
          side: BorderSide(color: glassStrokeLight, width: 1),
        ),
      ),

      // ── Buttons ──
      filledButtonTheme: FilledButtonThemeData(
        style: FilledButton.styleFrom(
          backgroundColor: indigoLuxury,
          foregroundColor: Colors.white,
          padding: const EdgeInsets.symmetric(horizontal: 28, vertical: 16),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(radiusFull),
          ),
          elevation: 0,
          textStyle: const TextStyle(
            fontFamily: bodyFont,
            fontSize: 16,
            fontWeight: FontWeight.w600,
            letterSpacing: 0.01,
          ),
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          foregroundColor: indigoLuxury,
          padding: const EdgeInsets.symmetric(horizontal: 28, vertical: 16),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(radiusFull),
            side: BorderSide(color: indigoLuxury.withValues(alpha: 0.3), width: 1.5),
          ),
          textStyle: const TextStyle(
            fontFamily: bodyFont,
            fontSize: 16,
            fontWeight: FontWeight.w600,
          ),
        ),
      ),
      textButtonTheme: TextButtonThemeData(
        style: TextButton.styleFrom(
          foregroundColor: indigoLuxury,
          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
          textStyle: const TextStyle(
            fontFamily: bodyFont,
            fontSize: 14,
            fontWeight: FontWeight.w600,
          ),
        ),
      ),

      // ── Input Fields ──
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: slate100,
        contentPadding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(radiusLg),
          borderSide: BorderSide(color: slate200, width: 1),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(radiusLg),
          borderSide: BorderSide(color: slate200, width: 1),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(radiusLg),
          borderSide: const BorderSide(color: indigoLuxury, width: 2),
        ),
        errorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(radiusLg),
          borderSide: const BorderSide(color: error, width: 1),
        ),
        focusedErrorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(radiusLg),
          borderSide: const BorderSide(color: error, width: 2),
        ),
        labelStyle: TextStyle(color: slate500, fontSize: 14),
        hintStyle: TextStyle(color: slate400, fontSize: 16),
      ),

      // ── Chips ──
      chipTheme: ChipThemeData(
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(radiusMd),
          side: BorderSide(color: slate200),
        ),
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
        labelStyle: const TextStyle(fontSize: 13, fontWeight: FontWeight.w500),
        selectedColor: indigoLuxury,
        secondarySelectedColor: indigoLight,
      ),

      // ── Bottom Navigation ──
      navigationBarTheme: NavigationBarThemeData(
        backgroundColor: glassLight,
        elevation: 0,
        indicatorColor: indigoLuxury.withValues(alpha: 0.12),
        iconTheme: WidgetStateProperty.resolveWith((states) {
          if (states.contains(WidgetState.selected)) {
            return const IconThemeData(color: indigoLuxury, size: 24);
          }
          return IconThemeData(color: slate500, size: 24);
        }),
        labelTextStyle: WidgetStateProperty.resolveWith((states) {
          if (states.contains(WidgetState.selected)) {
            return const TextStyle(
              fontFamily: bodyFont,
              fontSize: 12,
              fontWeight: FontWeight.w600,
              color: indigoLuxury,
            );
          }
          return TextStyle(
            fontFamily: bodyFont,
            fontSize: 12,
            fontWeight: FontWeight.w500,
            color: slate500,
          );
        }),
        labelBehavior: NavigationDestinationLabelBehavior.alwaysShow,
      ),

      // ── Bottom Sheet ──
      bottomSheetTheme: BottomSheetThemeData(
        backgroundColor: slate50,
        elevation: 0,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.vertical(top: Radius.circular(radiusXl)),
        ),
      ),

      // ── Divider ──
      dividerTheme: DividerThemeData(
        color: slate200.withValues(alpha: 0.5),
        thickness: 1,
        space: 1,
      ),
    );
  }

  // ═══════════════════════════════════════════════════════
  // DARK THEME
  // ═══════════════════════════════════════════════════════
  static ThemeData get darkTheme {
    final colorScheme = ColorScheme.dark(
      primary: indigoDark,
      onPrimary: slate900,
      primaryContainer: indigoLight,
      onPrimaryContainer: Colors.white,
      secondary: const Color(0xFFA78BFA),
      onSecondary: slate900,
      secondaryContainer: const Color(0xFF7C3AED),
      tertiary: const Color(0xFF22D3EE),
      onTertiary: slate900,
      error: const Color(0xFFFCA5A5),
      onError: slate900,
      surface: slate900,
      onSurface: const Color(0xFFF1F5F9),
      surfaceContainerHighest: slate800,
      onSurfaceVariant: const Color(0xFF94A3B8),
      outline: slate700,
      outlineVariant: slate700,
      inverseSurface: slate100,
      inversePrimary: indigoLuxury,
    );

    return ThemeData(
      useMaterial3: true,
      brightness: Brightness.dark,
      colorScheme: colorScheme,
      fontFamily: bodyFont,

      // ── Text Theme (same hierarchy, adjusted for dark) ──
      textTheme: TextTheme(
        displayLarge: const TextStyle(
          fontFamily: headlineFont,
          fontSize: 56,
          fontWeight: FontWeight.w700,
          height: 1.14,
          letterSpacing: -0.02,
          color: Colors.white,
        ),
        displayMedium: const TextStyle(
          fontFamily: headlineFont,
          fontSize: 40,
          fontWeight: FontWeight.w700,
          height: 1.15,
          color: Colors.white,
        ),
        displaySmall: const TextStyle(
          fontFamily: headlineFont,
          fontSize: 32,
          fontWeight: FontWeight.w700,
          height: 1.25,
          letterSpacing: -0.01,
          color: Colors.white,
        ),
        headlineLarge: const TextStyle(
          fontFamily: headlineFont,
          fontSize: 28,
          fontWeight: FontWeight.w700,
          height: 1.29,
          color: Colors.white,
        ),
        headlineMedium: const TextStyle(
          fontFamily: headlineFont,
          fontSize: 24,
          fontWeight: FontWeight.w600,
          height: 1.33,
          color: Colors.white,
        ),
        headlineSmall: const TextStyle(
          fontFamily: headlineFont,
          fontSize: 20,
          fontWeight: FontWeight.w600,
          height: 1.4,
          color: Colors.white,
        ),
        titleLarge: const TextStyle(
          fontFamily: headlineFont,
          fontSize: 20,
          fontWeight: FontWeight.w600,
          height: 1.4,
        ),
        titleMedium: const TextStyle(
          fontFamily: headlineFont,
          fontSize: 16,
          fontWeight: FontWeight.w600,
          height: 1.5,
        ),
        titleSmall: const TextStyle(
          fontFamily: headlineFont,
          fontSize: 14,
          fontWeight: FontWeight.w600,
          height: 1.43,
          letterSpacing: 0.01,
        ),
        bodyLarge: const TextStyle(
          fontFamily: bodyFont,
          fontSize: 18,
          fontWeight: FontWeight.w400,
          height: 1.56,
        ),
        bodyMedium: const TextStyle(
          fontFamily: bodyFont,
          fontSize: 16,
          fontWeight: FontWeight.w400,
          height: 1.5,
        ),
        bodySmall: const TextStyle(
          fontFamily: bodyFont,
          fontSize: 14,
          fontWeight: FontWeight.w400,
          height: 1.43,
        ),
        labelLarge: const TextStyle(
          fontFamily: bodyFont,
          fontSize: 14,
          fontWeight: FontWeight.w600,
          height: 1.43,
          letterSpacing: 0.01,
        ),
        labelMedium: const TextStyle(
          fontFamily: bodyFont,
          fontSize: 12,
          fontWeight: FontWeight.w500,
          height: 1.33,
          letterSpacing: 0.04,
        ),
        labelSmall: const TextStyle(
          fontFamily: bodyFont,
          fontSize: 11,
          fontWeight: FontWeight.w500,
          height: 1.45,
          letterSpacing: 0.06,
        ),
      ),

      // ── AppBar ──
      appBarTheme: AppBarTheme(
        centerTitle: false,
        elevation: 0,
        scrolledUnderElevation: 0.5,
        backgroundColor: Colors.transparent,
        foregroundColor: colorScheme.onSurface,
        surfaceTintColor: Colors.transparent,
        titleTextStyle: const TextStyle(
          fontFamily: headlineFont,
          fontSize: 20,
          fontWeight: FontWeight.w600,
          color: Colors.white,
        ),
      ),

      // ── Card (glassmorphic dark) ──
      cardTheme: CardThemeData(
        elevation: 0,
        color: glassDark,
        shadowColor: Colors.transparent,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(radiusLg),
          side: BorderSide(color: glassStrokeDark, width: 1),
        ),
      ),

      // ── Buttons ──
      filledButtonTheme: FilledButtonThemeData(
        style: FilledButton.styleFrom(
          backgroundColor: indigoLight,
          foregroundColor: Colors.white,
          padding: const EdgeInsets.symmetric(horizontal: 28, vertical: 16),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(radiusFull),
          ),
          elevation: 0,
          textStyle: const TextStyle(
            fontFamily: bodyFont,
            fontSize: 16,
            fontWeight: FontWeight.w600,
            letterSpacing: 0.01,
          ),
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          foregroundColor: indigoDark,
          padding: const EdgeInsets.symmetric(horizontal: 28, vertical: 16),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(radiusFull),
            side: BorderSide(color: indigoDark.withValues(alpha: 0.3), width: 1.5),
          ),
          textStyle: const TextStyle(
            fontFamily: bodyFont,
            fontSize: 16,
            fontWeight: FontWeight.w600,
          ),
        ),
      ),
      textButtonTheme: TextButtonThemeData(
        style: TextButton.styleFrom(
          foregroundColor: indigoDark,
          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
          textStyle: const TextStyle(
            fontFamily: bodyFont,
            fontSize: 14,
            fontWeight: FontWeight.w600,
          ),
        ),
      ),

      // ── Input Fields ──
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: slate800.withValues(alpha: 0.5),
        contentPadding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(radiusLg),
          borderSide: BorderSide(color: slate700, width: 1),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(radiusLg),
          borderSide: BorderSide(color: slate700, width: 1),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(radiusLg),
          borderSide: BorderSide(color: indigoDark, width: 2),
        ),
        errorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(radiusLg),
          borderSide: const BorderSide(color: error, width: 1),
        ),
        focusedErrorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(radiusLg),
          borderSide: const BorderSide(color: error, width: 2),
        ),
        labelStyle: TextStyle(color: slate400, fontSize: 14),
        hintStyle: TextStyle(color: slate500, fontSize: 16),
      ),

      // ── Chips ──
      chipTheme: ChipThemeData(
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(radiusMd),
          side: BorderSide(color: slate700),
        ),
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
        labelStyle: const TextStyle(fontSize: 13, fontWeight: FontWeight.w500),
        selectedColor: indigoLight,
        secondarySelectedColor: indigoLuxury,
      ),

      // ── Bottom Navigation ──
      navigationBarTheme: NavigationBarThemeData(
        backgroundColor: glassDark,
        elevation: 0,
        indicatorColor: indigoLight.withValues(alpha: 0.15),
        iconTheme: WidgetStateProperty.resolveWith((states) {
          if (states.contains(WidgetState.selected)) {
            return const IconThemeData(color: indigoDark, size: 24);
          }
          return const IconThemeData(color: slate400, size: 24);
        }),
        labelTextStyle: WidgetStateProperty.resolveWith((states) {
          if (states.contains(WidgetState.selected)) {
            return const TextStyle(
              fontFamily: bodyFont,
              fontSize: 12,
              fontWeight: FontWeight.w600,
              color: indigoDark,
            );
          }
          return const TextStyle(
            fontFamily: bodyFont,
            fontSize: 12,
            fontWeight: FontWeight.w500,
            color: slate400,
          );
        }),
        labelBehavior: NavigationDestinationLabelBehavior.alwaysShow,
      ),

      // ── Bottom Sheet ──
      bottomSheetTheme: BottomSheetThemeData(
        backgroundColor: slate900,
        elevation: 0,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.vertical(top: Radius.circular(radiusXl)),
        ),
      ),

      // ── Divider ──
      dividerTheme: DividerThemeData(
        color: slate700.withValues(alpha: 0.5),
        thickness: 1,
        space: 1,
      ),

      // ── SnackBar ──
      snackBarTheme: SnackBarThemeData(
        backgroundColor: slate800,
        contentTextStyle: const TextStyle(color: Colors.white, fontSize: 14),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(radiusMd),
        ),
        behavior: SnackBarBehavior.floating,
      ),
    );
  }
}

/// ═══════════════════════════════════════════════════════
/// Utility: Glassmorphism container with true backdrop blur
/// ═══════════════════════════════════════════════════════
class GlassContainer extends StatelessWidget {
  final Widget child;
  final double? width;
  final double? height;
  final EdgeInsetsGeometry? padding;
  final EdgeInsetsGeometry? margin;
  final double borderRadius;
  final List<BoxShadow>? shadows;
  final BorderSide? borderSide;
  final Alignment? alignment;
  final double blurSigma;

  const GlassContainer({
    super.key,
    required this.child,
    this.width,
    this.height,
    this.padding,
    this.margin,
    this.borderRadius = 16,
    this.shadows,
    this.borderSide,
    this.alignment,
    this.blurSigma = 20,
  });

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return ClipRRect(
      borderRadius: BorderRadius.circular(borderRadius),
      child: BackdropFilter(
        filter: ui.ImageFilter.blur(sigmaX: blurSigma, sigmaY: blurSigma),
        child: Container(
          width: width,
          height: height,
          margin: margin,
          padding: padding,
          alignment: alignment,
          decoration: BoxDecoration(
            color: isDark ? AppTheme.glassDark : AppTheme.glassLight,
            borderRadius: BorderRadius.circular(borderRadius),
            border: Border.all(
              color: borderSide?.color ??
                  (isDark ? AppTheme.glassStrokeDark : AppTheme.glassStrokeLight),
              width: borderSide?.width ?? 1,
            ),
            boxShadow: shadows ??
                (isDark ? AppTheme.darkElevatedShadow : AppTheme.softShadow),
          ),
          clipBehavior: Clip.antiAlias,
          child: child,
        ),
      ),
    );
  }
}

/// ═══════════════════════════════════════════════════════
/// Utility: Gradient accent background
/// ═══════════════════════════════════════════════════════
class GradientBackground extends StatelessWidget {
  final Widget child;
  final List<Color>? colors;
  final Alignment? begin;
  final Alignment? end;

  const GradientBackground({
    super.key,
    required this.child,
    this.colors,
    this.begin,
    this.end,
  });

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return Container(
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: begin ?? Alignment.topLeft,
          end: end ?? Alignment.bottomRight,
          colors: colors ??
              (isDark
                  ? [
                      const Color(0xFF0F172A),
                      const Color(0xFF1E1B4B),
                    ]
                  : [
                      const Color(0xFFF8FAFC),
                      const Color(0xFFEEF2FF),
                    ]),
        ),
      ),
      child: child,
    );
  }
}
