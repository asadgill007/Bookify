import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../business/providers/businesses_provider.dart';

/// Search screen — wired to real business search API.
class SearchScreen extends ConsumerStatefulWidget {
  const SearchScreen({super.key});

  @override
  ConsumerState<SearchScreen> createState() => _SearchScreenState();
}

class _SearchScreenState extends ConsumerState<SearchScreen> {
  final _searchController = TextEditingController();
  String _currentQuery = '';

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  void _performSearch(String query) {
    setState(() => _currentQuery = query.trim());
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final searchAsync = ref.watch(searchResultsProvider(_currentQuery));

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
            onPressed: () {
              // TODO: Show filter options
            },
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
                        onTap: () => context.push('/business/${biz.id}'),
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
