import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/api_client.dart';
import '../../../core/constants/api_constants.dart';

/// One item of the auto-verification checklist (mirrors the backend DTO).
class BusinessChecklistItem {
  final String key;
  final String label;
  final bool isComplete;

  const BusinessChecklistItem({
    required this.key,
    required this.label,
    required this.isComplete,
  });

  factory BusinessChecklistItem.fromJson(Map<String, dynamic> json) =>
      BusinessChecklistItem(
        key: json['key'] as String? ?? '',
        label: json['label'] as String? ?? '',
        isComplete: json['isComplete'] as bool? ?? false,
      );
}

/// My business summary from GET /businesses/mine.
class MyBusiness {
  final String id;
  final String name;
  final String slug;
  final String? description;
  final String city;
  final String country;
  final bool isVerified;
  final String verificationStatus; // Pending | Approved | Rejected
  final String? rejectionReason;
  final int totalServices;
  final int totalProviders;
  final String? coverImageUrl;

  /// Auto-verification checklist state (provider dashboard).
  final bool isChecklistComplete;
  final List<BusinessChecklistItem> checklist;

  const MyBusiness({
    required this.id,
    required this.name,
    required this.slug,
    this.description,
    required this.city,
    required this.country,
    required this.isVerified,
    required this.verificationStatus,
    this.rejectionReason,
    required this.totalServices,
    required this.totalProviders,
    this.coverImageUrl,
    this.isChecklistComplete = false,
    this.checklist = const [],
  });

  bool get isPending => verificationStatus.toLowerCase() == 'pending';
  bool get isApproved => verificationStatus.toLowerCase() == 'approved';
  bool get isRejected => verificationStatus.toLowerCase() == 'rejected';

  factory MyBusiness.fromJson(Map<String, dynamic> json) {
    return MyBusiness(
      id: json['id'] as String,
      name: json['name'] as String? ?? '',
      slug: json['slug'] as String? ?? '',
      description: json['description'] as String?,
      city: json['city'] as String? ?? '',
      country: json['country'] as String? ?? '',
      isVerified: json['isVerified'] as bool? ?? false,
      verificationStatus: json['verificationStatus'] as String? ?? 'Pending',
      rejectionReason: json['rejectionReason'] as String?,
      totalServices: json['totalServices'] as int? ?? 0,
      totalProviders: json['totalProviders'] as int? ?? 0,
      coverImageUrl: json['coverImageUrl'] as String?,
      isChecklistComplete: json['isChecklistComplete'] as bool? ?? false,
      checklist: (json['checklist'] as List<dynamic>? ?? [])
          .map((e) => BusinessChecklistItem.fromJson(e as Map<String, dynamic>))
          .toList(),
    );
  }
}

/// Fetches the current user's businesses.
final myBusinessesProvider = FutureProvider<List<MyBusiness>>((ref) async {
  final api = ref.watch(apiClientProvider);
  final response = await api.get(ApiConstants.myBusinesses);
  final data = response.data;

  List<dynamic> rawList;
  if (data is Map<String, dynamic> && data.containsKey('data')) {
    rawList = data['data'] as List<dynamic>;
  } else if (data is List) {
    rawList = data;
  } else {
    return [];
  }

  return rawList
      .map((e) => MyBusiness.fromJson(e as Map<String, dynamic>))
      .toList();
});

/// Resubmits a rejected business for review.
final resubmitProvider =
    FutureProvider.family<void, String>((ref, businessId) async {
  final api = ref.watch(apiClientProvider);
  await api.post(ApiConstants.businessResubmit(businessId));
});
