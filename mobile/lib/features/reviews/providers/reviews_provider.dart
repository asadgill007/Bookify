import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/api_client.dart';
import '../../../core/constants/api_constants.dart';

/// Review model matching the backend ReviewDetailDto.
class Review {
  final String id;
  final String customerName;
  final String? customerAvatarUrl;
  final int rating;
  final String? comment;
  final bool isVerifiedPurchase;
  final String? providerReply;
  final DateTime? repliedAt;
  final String? providerName;
  final String? businessName;
  final DateTime createdAt;

  const Review({
    required this.id,
    required this.customerName,
    this.customerAvatarUrl,
    required this.rating,
    this.comment,
    required this.isVerifiedPurchase,
    this.providerReply,
    this.repliedAt,
    this.providerName,
    this.businessName,
    required this.createdAt,
  });

  factory Review.fromJson(Map<String, dynamic> json) => Review(
        id: json['id'] as String? ?? '',
        customerName: json['customerName'] as String? ?? 'Customer',
        customerAvatarUrl: json['customerAvatarUrl'] as String?,
        rating: json['rating'] as int? ?? 5,
        comment: json['comment'] as String?,
        isVerifiedPurchase: json['isVerifiedPurchase'] as bool? ?? false,
        providerReply: json['providerReply'] as String?,
        repliedAt: json['repliedAt'] != null
            ? DateTime.tryParse(json['repliedAt'] as String)
            : null,
        providerName: json['providerName'] as String?,
        businessName: json['businessName'] as String?,
        createdAt: DateTime.tryParse(json['createdAt'] as String? ?? '') ??
            DateTime.now(),
      );
}

/// Paginated review results (items + total count for "load more" UI).
class ReviewPage {
  final List<Review> items;
  final int page;
  final int totalCount;
  final int totalPages;

  const ReviewPage({
    required this.items,
    required this.page,
    required this.totalCount,
    required this.totalPages,
  });

  bool get hasNextPage => page < totalPages;
}

/// API methods for reviews.
class ReviewsApi {
  final ApiClient _api;

  ReviewsApi(this._api);

  /// List reviews for a business (paginated response -> data.items).
  Future<ReviewPage> getBusinessReviews(String businessId, {int page = 1, int pageSize = 20}) async {
    final response = await _api.get(
      ApiConstants.businessReviews(businessId),
      queryParameters: {'page': page, 'pageSize': pageSize},
    );
    final body = response.data;
    var totalCount = 0;
    var totalPages = 1;
    List<Review> items;
    if (body is Map<String, dynamic>) {
      final inner = body['data'];
      if (inner is Map<String, dynamic>) {
        totalCount = inner['totalCount'] as int? ?? 0;
        totalPages = inner['totalPages'] as int? ?? 1;
        items = _parseListFrom(inner['items']);
      } else {
        items = _parseListFrom(inner);
      }
    } else {
      items = _parseListFrom(body);
    }
    return ReviewPage(
      items: items,
      page: page,
      totalCount: totalCount,
      totalPages: totalPages,
    );
  }

  List<Review> _parseListFrom(dynamic items) {
    if (items is List) {
      return items
          .map((e) => Review.fromJson(e as Map<String, dynamic>))
          .toList();
    }
    return [];
  }

  /// Create a review for a completed appointment.
  Future<void> createReview({
    required String appointmentId,
    required int rating,
    String? comment,
  }) async {
    await _api.post(
      ApiConstants.reviewByAppointment(appointmentId),
      data: {'rating': rating, 'comment': comment},
    );
  }

  /// Update the current user's review.
  Future<void> updateReview({
    required String reviewId,
    required int rating,
    String? comment,
  }) async {
    await _api.put(
      ApiConstants.reviewById(reviewId),
      data: {'rating': rating, 'comment': comment},
    );
  }

  /// Delete the current user's review.
  Future<void> deleteReview(String reviewId) async {
    await _api.delete(ApiConstants.reviewById(reviewId));
  }

  /// Vote a review as helpful (or not).
  Future<void> voteReview(String reviewId, bool isHelpful) async {
    await _api.post(
      ApiConstants.reviewVote(reviewId),
      data: {'isHelpful': isHelpful},
    );
  }

  /// Report a review.
  Future<void> reportReview(
    String reviewId, {
    required String reason,
    String? description,
  }) async {
    await _api.post(
      ApiConstants.reviewReport(reviewId),
      data: {'reason': reason, 'description': description},
    );
  }


}

final reviewsApiProvider = Provider<ReviewsApi>((ref) {
  return ReviewsApi(ref.watch(apiClientProvider));
});

/// Reviews page for a business, keyed by businessId.
final businessReviewsProvider =
    FutureProvider.family<ReviewPage, String>((ref, businessId) async {
  final api = ref.watch(reviewsApiProvider);
  return api.getBusinessReviews(businessId);
});

/// "Full name" of the signed-in user (from /users/me), used to detect the
/// customer's own reviews so we can show Edit / Delete actions.
final currentUserNameProvider = FutureProvider<String?>((ref) async {
  try {
    final api = ref.watch(apiClientProvider);
    final response = await api.get(ApiConstants.userProfile);
    final body = response.data;
    if (body is Map<String, dynamic>) {
      final data = (body['data'] ?? body);
      if (data is Map<String, dynamic>) {
        final first = data['firstName'] as String? ?? '';
        final last = data['lastName'] as String? ?? '';
        final name = '$first $last'.trim();
        return name.isEmpty ? null : name;
      }
    }
    return null;
  } catch (_) {
    return null;
  }
});
