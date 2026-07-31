import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/api_client.dart';
import '../../../core/constants/api_constants.dart';

/// Business model matching the backend API search result.
class Business {
  final String id;
  final String name;
  final String slug;
  final String? description;
  final String? category;
  final String city;
  final String country;
  final double averageRating;
  final int totalReviews;
  final String? coverImageUrl;
  final bool isVerified;

  Business({
    required this.id,
    required this.name,
    required this.slug,
    this.description,
    this.category,
    required this.city,
    required this.country,
    this.averageRating = 0.0,
    this.totalReviews = 0,
    this.coverImageUrl,
    this.isVerified = true,
  });

  factory Business.fromJson(Map<String, dynamic> json) {
    return Business(
      id: json['id'] as String? ?? '',
      name: json['name'] as String? ?? '',
      slug: json['slug'] as String? ?? '',
      description: json['description'] as String?,
      category: json['category'] as String?,
      city: json['city'] as String? ?? '',
      country: json['country'] as String? ?? '',
      averageRating: (json['averageRating'] as num?)?.toDouble() ?? 0.0,
      totalReviews: json['totalReviews'] as int? ?? 0,
      coverImageUrl: json['coverImageUrl'] as String?,
      isVerified: json['isVerified'] as bool? ?? true,
    );
  }
}

/// Provider that fetches businesses from the backend.
final businessesProvider = FutureProvider<List<Business>>((ref) async {
  final api = ref.watch(apiClientProvider);
  final response = await api.get(ApiConstants.businesses, queryParameters: {
    'page': 1,
    'pageSize': 20,
    'sortBy': 'averageRating',
    'sortDirection': 'desc',
  });

  final data = response.data;
  if (data is Map<String, dynamic> && data.containsKey('data')) {
    final items = data['data'] as List<dynamic>;
    return items.map((e) => Business.fromJson(e as Map<String, dynamic>)).toList();
  }
  if (data is List) {
    return data.map((e) => Business.fromJson(e as Map<String, dynamic>)).toList();
  }
  return [];
});

/// Search intent carried through navigation: a text query and/or a category slug.
class SearchIntent {
  final String query;
  final String? categorySlug;

  const SearchIntent({this.query = '', this.categorySlug});
}

/// Provider that searches businesses by query string and/or category slug.
final searchResultsProvider =
    FutureProvider.family<List<Business>, SearchIntent>((ref, intent) async {
  if (intent.query.isEmpty && intent.categorySlug == null) return [];

  final api = ref.watch(apiClientProvider);
  final response = await api.get(ApiConstants.businesses, queryParameters: {
    'page': 1,
    'pageSize': 20,
    if (intent.query.isNotEmpty) 'search': intent.query,
    if (intent.categorySlug != null) 'category': intent.categorySlug,
  });

  final data = response.data;
  if (data is Map<String, dynamic> && data.containsKey('data')) {
    final items = data['data'] as List<dynamic>;
    return items.map((e) => Business.fromJson(e as Map<String, dynamic>)).toList();
  }
  if (data is List) {
    return data.map((e) => Business.fromJson(e as Map<String, dynamic>)).toList();
  }
  return [];
});
