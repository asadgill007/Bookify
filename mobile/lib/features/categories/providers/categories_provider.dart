import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/api_client.dart';
import '../../../core/constants/api_constants.dart';

/// Category data model matching the backend API response.
class Category {
  final String id;
  final String name;
  final String slug;
  final String? iconName;
  final int displayOrder;
  final List<SubCategory> subCategories;

  Category({
    required this.id,
    required this.name,
    required this.slug,
    this.iconName,
    required this.displayOrder,
    required this.subCategories,
  });

  factory Category.fromJson(Map<String, dynamic> json) {
    return Category(
      id: json['id'] as String,
      name: json['name'] as String,
      slug: json['slug'] as String,
      iconName: json['iconName'] as String?,
      displayOrder: json['displayOrder'] as int? ?? 0,
      subCategories: (json['subCategories'] as List<dynamic>?)
              ?.map((e) => SubCategory.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}

class SubCategory {
  final String id;
  final String name;
  final String slug;

  SubCategory({required this.id, required this.name, required this.slug});

  factory SubCategory.fromJson(Map<String, dynamic> json) {
    return SubCategory(
      id: json['id'] as String,
      name: json['name'] as String,
      slug: json['slug'] as String,
    );
  }
}

/// Riverpod provider that fetches categories from the API.
final categoriesProvider = FutureProvider<List<Category>>((ref) async {
  final api = ref.watch(apiClientProvider);
  final response = await api.get(ApiConstants.categories);
  final data = response.data;

  // Handle wrapped ApiResponse format: { data: [...], isSuccess: true, ... }
  List<dynamic> rawList;
  if (data is Map<String, dynamic> && data.containsKey('data')) {
    rawList = data['data'] as List<dynamic>;
  } else if (data is List) {
    rawList = data;
  } else {
    throw StateError('Unexpected API response format: ${data.runtimeType}');
  }

  return rawList
      .map((e) => Category.fromJson(e as Map<String, dynamic>))
      .toList();
});
