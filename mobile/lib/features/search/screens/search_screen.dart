import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../business/providers/businesses_provider.dart';
import '../../categories/providers/categories_provider.dart';
import '../../favorites/providers/favorites_provider.dart';
import '../../../core/theme/app_theme.dart';

/// Search screen — wired to real business search API with advanced filters
/// (price range, minimum rating, distance radius, category).
class SearchScreen extends ConsumerStatefulWidget {
  /// Optional initial search intent passed via navigation extra.
  final SearchIntent? intent;

  const SearchScreen({super.key, this.intent});

  @override
  ConsumerState<SearchScreen> createState() => _SearchScreenState();
}

class _SearchScreenState extends ConsumerState<SearchScreen> {
  final _searchController = TextEditingController();
  String _currentQuery = '';
  String? _categorySlug;
  String _categoryQuery = ''; // text tied to the active category filter

  // Advanced filters
  RangeValues _priceRange = const RangeValues(0, 500);
  double _ratingMin = 0;
  double _radiusKm = 0;
  bool _filtersApplied = false;

  @override
  void initState() {
    super.initState();
    final intent = widget.intent;
    final query = intent?.query ?? '';
    _categorySlug = intent?.categorySlug;
    _categoryQuery = _categorySlug != null ? query : '';
    if (intent?.priceMax != null) {
      _priceRange = RangeValues(intent!.priceMin ?? 0, intent.priceMax!);
    }
    _ratingMin = intent?.ratingMin ?? 0;
    _radiusKm = intent?.radiusKm ?? 0;
    _filtersApplied = intent?.hasFilters ?? false;
    if (query.isNotEmpty) {
      _searchController.text = query;
      _currentQuery = query;
    }
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  /// Builds the current intent from the on-screen state.
  SearchIntent _currentIntent() => SearchIntent(
        query: _currentQuery,
        categorySlug: _categorySlug,
        priceMin: _filtersApplied && _priceRange.start > 0
            ? _priceRange.start
            : null,
        priceMax: _filtersApplied && _priceRange.end < 500
            ? _priceRange.end
            : null,
        ratingMin: _filtersApplied && _ratingMin > 0 ? _ratingMin : null,
        radiusKm: _filtersApplied && _radiusKm > 0 ? _radiusKm : null,
      );

  /// Shows the advanced filter sheet: price range, min rating, radius,
  /// and category selection.
  Future<void> _showFilterSheet() async {
    final categoriesAsync = ref.read(categoriesProvider);
    final categories = categoriesAsync.valueOrNull ?? const [];

    var draftPrice = _priceRange;
    var draftRating = _ratingMin;
    var draftRadius = _radiusKm;
    var draftCategory = _categorySlug;

    await showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (sheetContext) {
        return StatefulBuilder(
          builder: (sheetContext, setSheetState) {
            final theme = Theme.of(sheetContext);
            return Container(
              decoration: BoxDecoration(
                color: theme.colorScheme.surface,
                borderRadius:
                    const BorderRadius.vertical(top: Radius.circular(28)),
              ),
              padding: EdgeInsets.only(
                left: 20,
                right: 20,
                top: 16,
                bottom: MediaQuery.of(sheetContext).viewInsets.bottom + 16,
              ),
              child: SingleChildScrollView(
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Text('Filters',
                            style: theme.textTheme.titleLarge
                                ?.copyWith(fontWeight: FontWeight.w700)),
                        const Spacer(),
                        TextButton(
                          onPressed: () => setSheetState(() {
                            draftPrice = const RangeValues(0, 500);
                            draftRating = 0;
                            draftRadius = 0;
                            draftCategory = null;
                          }),
                          child: const Text('Reset'),
                        ),
                      ],
                    ),
                    const SizedBox(height: 8),

                    // Category
                    Text('Category',
                        style: theme.textTheme.titleSmall
                            ?.copyWith(fontWeight: FontWeight.w600)),
                    const SizedBox(height: 8),
                    Wrap(
                      spacing: 8,
                      runSpacing: 8,
                      children: [
                        ChoiceChip(
                          label: const Text('All'),
                          selected: draftCategory == null,
                          onSelected: (_) =>
                              setSheetState(() => draftCategory = null),
                        ),
                        ...categories.map(
                          (cat) => ChoiceChip(
                            label: Text(cat.name),
                            selected: draftCategory == cat.slug,
                            onSelected: (_) => setSheetState(
                                () => draftCategory = cat.slug),
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 20),

                    // Price range
                    Row(
                      children: [
                        Text('Price Range',
                            style: theme.textTheme.titleSmall
                                ?.copyWith(fontWeight: FontWeight.w600)),
                        const Spacer(),
                        Text(
                          '\$${draftPrice.start.toStringAsFixed(0)} – '
                          '\$${draftPrice.end == 500 ? '500+' : draftPrice.end.toStringAsFixed(0)}',
                          style: theme.textTheme.bodySmall?.copyWith(
                              color: theme.colorScheme.primary,
                              fontWeight: FontWeight.w600),
                        ),
                      ],
                    ),
                    RangeSlider(
                      values: draftPrice,
                      min: 0,
                      max: 500,
                      divisions: 50,
                      labels: RangeLabels(
                        '\$${draftPrice.start.toStringAsFixed(0)}',
                        '\$${draftPrice.end.toStringAsFixed(0)}',
                      ),
                      onChanged: (v) =>
                          setSheetState(() => draftPrice = v),
                    ),
                    const SizedBox(height: 8),

                    // Minimum rating
                    Row(
                      children: [
                        Text('Minimum Rating',
                            style: theme.textTheme.titleSmall
                                ?.copyWith(fontWeight: FontWeight.w600)),
                        const Spacer(),
                        Text(
                          draftRating == 0 ? 'Any' : '${draftRating.toStringAsFixed(0)}+ ★',
                          style: theme.textTheme.bodySmall?.copyWith(
                              color: theme.colorScheme.primary,
                              fontWeight: FontWeight.w600),
                        ),
                      ],
                    ),
                    Slider(
                      value: draftRating,
                      min: 0,
                      max: 5,
                      divisions: 5,
                      label: draftRating.toStringAsFixed(0),
                      onChanged: (v) => setSheetState(() => draftRating = v),
                    ),
                    const SizedBox(height: 8),

                    // Distance radius
                    Row(
                      children: [
                        Text('Distance Radius',
                            style: theme.textTheme.titleSmall
                                ?.copyWith(fontWeight: FontWeight.w600)),
                        const Spacer(),
                        Text(
                          draftRadius == 0
                              ? 'Anywhere'
                              : 'Within ${draftRadius.toStringAsFixed(0)} km',
                          style: theme.textTheme.bodySmall?.copyWith(
                              color: theme.colorScheme.primary,
                              fontWeight: FontWeight.w600),
                        ),
                      ],
                    ),
                    Slider(
                      value: draftRadius,
                      min: 0,
                      max: 100,
                      divisions: 20,
                      label: '${draftRadius.toStringAsFixed(0)} km',
                      onChanged: (v) => setSheetState(() => draftRadius = v),
                    ),
                    const SizedBox(height: 24),

                    SizedBox(
                      width: double.infinity,
                      height: 52,
                      child: FilledButton.icon(
                        onPressed: () {
                          setState(() {
                            _priceRange = draftPrice;
                            _ratingMin = draftRating;
                            _radiusKm = draftRadius;
                            _categorySlug = draftCategory;
                            _filtersApplied = true;
                          });
                          Navigator.pop(sheetContext);
                        },
                        icon: const Icon(Icons.filter_alt),
                        label: const Text('Apply Filters'),
                      ),
                    ),
                  ],
                ),
              ),
            );
          },
        );
      },
    );
  }

  void _performSearch(String query) {
    setState(() {
      _currentQuery = query.trim();
      // Keep the category filter when re-submitting the exact text that opened it
      // (e.g. tapping a category chip then pressing Enter); a genuinely new query
      // drops the filter and does a text search instead.
      if (_categorySlug != null && _currentQuery != _categoryQuery) {
        _categorySlug = null;
      }
    });
  }

  void _clearAllFilters() {
    setState(() {
      _categorySlug = null;
      _priceRange = const RangeValues(0, 500);
      _ratingMin = 0;
      _radiusKm = 0;
      _filtersApplied = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final intent = _currentIntent();
    final searchAsync = ref.watch(searchResultsProvider(intent));
    final canSearch = _currentQuery.isNotEmpty || intent.hasFilters;

    return Scaffold(
      appBar: AppBar(
        title: TextField(
          controller: _searchController,
          autofocus: true,
          decoration: InputDecoration(
            hintText: 'Search businesses...',
            border: InputBorder.none,
            filled: false,
          ),
          textInputAction: TextInputAction.search,
          onSubmitted: _performSearch,
        ),
        actions: [
          IconButton(
            icon: Badge(
              isLabelVisible: intent.hasFilters,
              label: const Text(''),
              child: const Icon(Icons.tune),
            ),
            onPressed: _showFilterSheet,
          ),
        ],
      ),
      body: Column(
        children: [
          if (intent.hasFilters)
            _buildActiveFilterChips(theme, colorScheme),
          Expanded(
            child: !canSearch
                ? _emptyState(theme, colorScheme)
                : searchAsync.when(
                    loading: () => const Center(child: CircularProgressIndicator()),
                    error: (err, _) => _errorState(theme, colorScheme, err),
                    data: (businesses) {
                      if (businesses.isEmpty) {
                        return _noResultsState(theme, colorScheme);
                      }
                      return ListView.builder(
                        padding: const EdgeInsets.all(16),
                        itemCount: businesses.length,
                        itemBuilder: (context, index) {
                          final biz = businesses[index];
                          return _SearchResultCard(business: biz);
                        },
                      );
                    },
                  ),
          ),
        ],
      ),
    );
  }

  Widget _buildActiveFilterChips(ThemeData theme, ColorScheme colorScheme) {
    final chips = <Widget>[];
    if (_categorySlug != null) {
      chips.add(_filterChip('Category', () => setState(() => _categorySlug = null)));
    }
    if (_filtersApplied && (_priceRange.start > 0 || _priceRange.end < 500)) {
      chips.add(_filterChip(
        '\$${_priceRange.start.toStringAsFixed(0)}–\$${_priceRange.end == 500 ? '500+' : _priceRange.end.toStringAsFixed(0)}',
        () => setState(() => _priceRange = const RangeValues(0, 500)),
      ));
    }
    if (_ratingMin > 0) {
      chips.add(_filterChip('${_ratingMin.toStringAsFixed(0)}+★',
          () => setState(() => _ratingMin = 0)));
    }
    if (_radiusKm > 0) {
      chips.add(_filterChip('${_radiusKm.toStringAsFixed(0)} km',
          () => setState(() => _radiusKm = 0)));
    }
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.fromLTRB(16, 4, 8, 4),
      child: Row(
        children: [
          Expanded(
            child: SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              child: Row(children: chips),
            ),
          ),
          IconButton(
            icon: const Icon(Icons.close, size: 18),
            tooltip: 'Clear all filters',
            onPressed: _clearAllFilters,
          ),
        ],
      ),
    );
  }

  Widget _filterChip(String label, VoidCallback onDeleted) {
    return Padding(
      padding: const EdgeInsets.only(right: 8),
      child: Chip(
        label: Text(label),
        deleteIcon: const Icon(Icons.close, size: 14),
        onDeleted: onDeleted,
        visualDensity: VisualDensity.compact,
        backgroundColor: AppTheme.indigoLuxury.withValues(alpha: 0.1),
        side: BorderSide(color: AppTheme.indigoLuxury.withValues(alpha: 0.3)),
        labelStyle: const TextStyle(fontSize: 12),
      ),
    );
  }

  Widget _emptyState(ThemeData theme, ColorScheme colorScheme) {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(Icons.search, size: 64,
              color: colorScheme.onSurfaceVariant.withValues(alpha: 0.4)),
          const SizedBox(height: 16),
          Text('Search for doctors, salons, spas...',
              style: theme.textTheme.bodyLarge?.copyWith(
                  color: colorScheme.onSurfaceVariant)),
          const SizedBox(height: 8),
          Text('Try searching by name, category, or location',
              style: theme.textTheme.bodySmall?.copyWith(
                  color: colorScheme.onSurfaceVariant)),
        ],
      ),
    );
  }

  Widget _errorState(ThemeData theme, ColorScheme colorScheme, Object error) {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(Icons.error_outline, size: 48, color: colorScheme.error),
          const SizedBox(height: 16),
          Text('Search failed', style: theme.textTheme.titleMedium),
          const SizedBox(height: 8),
          Text(error.toString(), style: theme.textTheme.bodySmall),
          const SizedBox(height: 16),
          FilledButton.tonal(
            onPressed: () => _performSearch(_currentQuery),
            child: const Text('Retry'),
          ),
        ],
      ),
    );
  }

  Widget _noResultsState(ThemeData theme, ColorScheme colorScheme) {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(Icons.search_off, size: 64,
              color: colorScheme.onSurfaceVariant.withValues(alpha: 0.4)),
          const SizedBox(height: 16),
          Text('No results found for "$_currentQuery"',
              style: theme.textTheme.titleMedium?.copyWith(
                  color: colorScheme.onSurfaceVariant)),
          const SizedBox(height: 8),
          Text('Try a different search term or clear your filters',
              style: theme.textTheme.bodyMedium?.copyWith(
                  color: colorScheme.onSurfaceVariant)),
        ],
      ),
    );
  }
}

/// A single search result card with a favorite heart, rating, price hint
/// and distance (when available).
class _SearchResultCard extends ConsumerWidget {
  final Business business;
  const _SearchResultCard({required this.business});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final isFavorite = ref.watch(isFavoriteProvider(business.id));

    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      clipBehavior: Clip.antiAlias,
      child: InkWell(
        onTap: () => context.push('/business/${business.slug}'),
        child: Padding(
          padding: const EdgeInsets.all(12),
          child: Row(
            children: [
              ClipRRect(
                borderRadius: BorderRadius.circular(12),
                child: Container(
                  width: 64,
                  height: 64,
                  color: colorScheme.primaryContainer.withValues(alpha: 0.4),
                  child: business.coverImageUrl != null
                      ? Image.network(
                          business.coverImageUrl!,
                          fit: BoxFit.cover,
                          errorBuilder: (_, _, _) => Icon(Icons.store,
                              color: colorScheme.onSurfaceVariant),
                        )
                      : Icon(Icons.store, color: colorScheme.onSurfaceVariant),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(business.name,
                        style: theme.textTheme.titleSmall
                            ?.copyWith(fontWeight: FontWeight.w600),
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis),
                    const SizedBox(height: 4),
                    Text(business.city,
                        style: theme.textTheme.bodySmall?.copyWith(
                            color: colorScheme.onSurfaceVariant)),
                    const SizedBox(height: 6),
                    Row(
                      children: [
                        Icon(Icons.star_rounded, size: 16, color: Colors.amber),
                        const SizedBox(width: 4),
                        Text(business.averageRating.toStringAsFixed(1)),
                        const SizedBox(width: 8),
                        Text('(${business.totalReviews})',
                            style: theme.textTheme.bodySmall?.copyWith(
                                color: colorScheme.onSurfaceVariant)),
                        if (business.category != null) ...[
                          const SizedBox(width: 8),
                          Flexible(
                            child: Text('· ${business.category}',
                                style: theme.textTheme.bodySmall?.copyWith(
                                    color: AppTheme.indigoLuxury),
                                maxLines: 1,
                                overflow: TextOverflow.ellipsis),
                          ),
                        ],
                      ],
                    ),
                  ],
                ),
              ),
              IconButton(
                icon: Icon(
                  isFavorite ? Icons.favorite : Icons.favorite_border,
                  color: isFavorite ? Colors.red : colorScheme.onSurfaceVariant,
                  size: 22,
                ),
                onPressed: () async {
                  try {
                    await ref
                        .read(favoritesActionsProvider)
                        .toggle(business.id);
                  } catch (_) {
                    if (!context.mounted) return;
                    // Unauthenticated or offline: nudge the user to sign in.
                    ScaffoldMessenger.of(context).showSnackBar(
                      const SnackBar(
                        content: Text(
                            'Sign in to save favorites (or try again later).'),
                      ),
                    );
                  }
                },
              ),
            ],
          ),
        ),
      ),
    );
  }
}
