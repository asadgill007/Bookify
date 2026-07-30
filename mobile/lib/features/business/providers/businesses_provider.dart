import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/api_client.dart';
import '../../../core/constants/api_constants.dart';

/// Business model matching the backend API.
class Business {
  final String id;
  final String name;
  final String slug;
  final String? description;
  final String city;
  final String country;
  final double averageRating;
  final int totalReviews;
  final String? coverImageUrl;
  final double priceRange;

  Business({
    required this.id,
    required this.name,
    required this.slug,
    this.description,
    required this.city,
    required this.country,
    this.averageRating = 0.0,
    this.totalReviews = 0,
    this.coverImageUrl,
    this.priceRange = 0.0,
  });

  factory Business.fromJson(Map<String, dynamic> json) {
    return Business(
      id: json['id'] as String? ?? '',
      name: json['name'] as String? ?? '',
      slug: json['slug'] as String? ?? '',
      description: json['description'] as String?,
      city: json['city'] as String? ?? '',
      country: json['country'] as String? ?? '',
      averageRating: (json['averageRating'] as num?)?.toDouble() ?? 0.0,
      totalReviews: json['totalReviews'] as int? ?? 0,
      coverImageUrl: json['coverImageUrl'] as String?,
      priceRange: (json['priceRange'] as num?)?.toDouble() ?? 0.0,
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

/// Provider that searches businesses by query string.
final searchResultsProvider = FutureProvider.family<List<Business>, String>((ref, query) async {
  if (query.isEmpty) return [];
  
  final api = ref.watch(apiClientProvider);
  final response = await api.get(ApiConstants.businesses, queryParameters: {
    'page': 1,
    'pageSize': 20,
    'search': query,
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
