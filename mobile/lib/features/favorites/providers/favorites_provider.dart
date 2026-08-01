import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/constants/api_constants.dart';
import '../../../core/network/api_client.dart';

/// Favorite business summary (mirrors GET /favorites).
class FavoriteBusiness {
  final String id;
  final String name;
  final String slug;
  final String? category;
  final double averageRating;
  final int totalReviews;
  final String? coverImageUrl;
  final String city;
  final String country;

  const FavoriteBusiness({
    required this.id,
    required this.name,
    required this.slug,
    this.category,
    this.averageRating = 0,
    this.totalReviews = 0,
    this.coverImageUrl,
    required this.city,
    required this.country,
  });

  factory FavoriteBusiness.fromJson(Map<String, dynamic> json) =>
      FavoriteBusiness(
        id: json['id'] as String? ?? '',
        name: json['name'] as String? ?? '',
        slug: json['slug'] as String? ?? '',
        category: json['category'] as String?,
        averageRating: (json['averageRating'] as num?)?.toDouble() ?? 0,
        totalReviews: json['totalReviews'] as int? ?? 0,
        coverImageUrl: json['coverImageUrl'] as String?,
        city: json['city'] as String? ?? '',
        country: json['country'] as String? ?? '',
      );
}

/// List of the user's favorite businesses.
final favoritesProvider = FutureProvider<List<FavoriteBusiness>>((ref) async {
  final api = ref.watch(apiClientProvider);
  final response = await api.get(ApiConstants.favorites);
  final body = response.data;
  List<dynamic> rawList;
  if (body is Map<String, dynamic> && body.containsKey('data')) {
    rawList = body['data'] as List<dynamic>;
  } else if (body is List) {
    rawList = body;
  } else {
    return [];
  }
  return rawList
      .map((e) => FavoriteBusiness.fromJson(e as Map<String, dynamic>))
      .toList();
});

/// Ids of businesses the user has favorited (for heart state).
final favoriteIdsProvider = FutureProvider<Set<String>>((ref) async {
  final api = ref.watch(apiClientProvider);
  final response = await api.get(ApiConstants.favoriteIds);
  final body = response.data;
  List<dynamic> rawList;
  if (body is Map<String, dynamic> && body.containsKey('data')) {
    rawList = body['data'] as List<dynamic>;
  } else if (body is List) {
    rawList = body;
  } else {
    return {};
  }
  return rawList.map((e) => e.toString()).toSet();
});

/// Toggles a favorite for the given business id. Returns the new state.
class FavoritesActions {
  final Ref _ref;
  FavoritesActions(this._ref);

  Future<bool> toggle(String businessId) async {
    final api = _ref.read(apiClientProvider);
    final current = _ref.read(favoriteIdsProvider).valueOrNull ?? <String>{};
    final isFav = current.contains(businessId);

    if (isFav) {
      await api.delete(ApiConstants.favoriteByBusiness(businessId));
    } else {
      await api.post(ApiConstants.favoriteByBusiness(businessId));
    }

    _ref.invalidate(favoriteIdsProvider);
    _ref.invalidate(favoritesProvider);
    return !isFav;
  }

  Future<void> remove(String businessId) async {
    final api = _ref.read(apiClientProvider);
    await api.delete(ApiConstants.favoriteByBusiness(businessId));
    _ref.invalidate(favoriteIdsProvider);
    _ref.invalidate(favoritesProvider);
  }
}

final favoritesActionsProvider = Provider<FavoritesActions>((ref) {
  return FavoritesActions(ref);
});

/// Heart state for a business card — watches the favorites ids.
final isFavoriteProvider = Provider.family<bool, String>((ref, businessId) {
  final ids = ref.watch(favoriteIdsProvider).valueOrNull ?? const <String>{};
  return ids.contains(businessId);
});
