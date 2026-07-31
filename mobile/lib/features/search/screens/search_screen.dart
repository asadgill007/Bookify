import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../business/providers/businesses_provider.dart';
import '../../categories/providers/categories_provider.dart';

/// Search screen — wired to real business search API.
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

  @override
  void initState() {
    super.initState();
    final intent = widget.intent;
    final query = intent?.query ?? '';
    _categorySlug = intent?.categorySlug;
    _categoryQuery = _categorySlug != null ? query : '';
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

  /// Shows a filter bottom sheet: pick a category to narrow the search.
  Future<void> _showFilterSheet() async {
    final categoriesAsync = ref.read(categoriesProvider);
    final categories = categoriesAsync.valueOrNull ?? const [];
    final selected = await showModalBottomSheet<String?>(
      context: context,
      builder: (sheetContext) => SafeArea(
        child: ListView(
          shrinkWrap: true,
          padding: const EdgeInsets.all(16),
          children: [
            Text('Filter by category',
                style: Theme.of(sheetContext).textTheme.titleMedium),
            const SizedBox(height: 8),
            if (categories.isEmpty)
              const Padding(
                padding: EdgeInsets.all(16),
                child: Text('No categories available'),
              )
            else
              ...categories.map(
                (cat) => ListTile(
                  leading: Icon(
                    Icons.category_outlined,
                    color: cat.slug == _categorySlug
                        ? Theme.of(sheetContext).colorScheme.primary
                        : null,
                  ),
                  title: Text(cat.name),
                  trailing: cat.slug == _categorySlug
                      ? Icon(Icons.check,
                          color: Theme.of(sheetContext).colorScheme.primary)
                      : null,
                  onTap: () => Navigator.pop(sheetContext, cat.slug),
                ),
              ),
          ],
        ),
      ),
    );

    if (selected == null) return;
    setState(() {
      _categorySlug = selected;
      // Clear the text query so results come from the category filter.
      _currentQuery = '';
      _categoryQuery = '';
    });
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

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final searchAsync = ref.watch(searchResultsProvider(
      SearchIntent(query: _currentQuery, categorySlug: _categorySlug),
    ));

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
            icon: const Icon(Icons.tune),
            onPressed: _showFilterSheet,
          ),
        ],
      ),
      body: _currentQuery.isEmpty
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
                    return Card(
                      margin: const EdgeInsets.only(bottom: 12),
                      child: ListTile(
                        contentPadding: const EdgeInsets.all(12),
                        leading: CircleAvatar(
                          backgroundColor: colorScheme.primaryContainer,
                          child: Icon(Icons.store, color: colorScheme.onPrimaryContainer),
                        ),
                        title: Text(biz.name,
                            style: theme.textTheme.titleSmall?.copyWith(fontWeight: FontWeight.w600)),
                        subtitle: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(biz.city),
                            const SizedBox(height: 4),
                            Row(
                              children: [
                                Icon(Icons.star_rounded, size: 16, color: Colors.amber),
                                const SizedBox(width: 4),
                                Text(biz.averageRating.toStringAsFixed(1)),
                                const SizedBox(width: 8),
                                Text('(${biz.totalReviews})',
                                    style: theme.textTheme.bodySmall?.copyWith(
                                        color: colorScheme.onSurfaceVariant)),
                              ],
                            ),
                          ],
                        ),
                        trailing: const Icon(Icons.chevron_right),
                        onTap: () => context.push('/business/${biz.slug}'),
                      ),
                    );
                  },
                );
              },
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
          Text('Try a different search term',
              style: theme.textTheme.bodyMedium?.copyWith(
                  color: colorScheme.onSurfaceVariant)),
        ],
      ),
    );
  }
}
