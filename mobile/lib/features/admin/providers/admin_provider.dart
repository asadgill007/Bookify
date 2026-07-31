import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/api_client.dart';
import '../../../core/constants/api_constants.dart';

/// Admin view of a business listing.
class AdminBusiness {
  final String id;
  final String name;
  final String slug;
  final String ownerName;
  final String city;
  final String country;
  final bool isVerified;
  final String verificationStatus;
  final String? rejectionReason;
  final DateTime? reviewedAt;
  final bool isActive;
  final double averageRating;
  final int totalReviews;
  final DateTime createdAt;

  const AdminBusiness({
    required this.id,
    required this.name,
    required this.slug,
    this.ownerName = '',
    required this.city,
    required this.country,
    required this.isVerified,
    required this.verificationStatus,
    this.rejectionReason,
    this.reviewedAt,
    required this.isActive,
    required this.averageRating,
    required this.totalReviews,
    required this.createdAt,
  });

  bool get isPending => verificationStatus.toLowerCase() == 'pending';
  bool get isApproved => verificationStatus.toLowerCase() == 'approved';
  bool get isRejected => verificationStatus.toLowerCase() == 'rejected';

  factory AdminBusiness.fromJson(Map<String, dynamic> json) {
    return AdminBusiness(
      id: json['id'] as String,
      name: json['name'] as String? ?? '',
      slug: json['slug'] as String? ?? '',
      ownerName: json['ownerName'] as String? ?? '',
      city: json['city'] as String? ?? '',
      country: json['country'] as String? ?? '',
      isVerified: json['isVerified'] as bool? ?? false,
      verificationStatus: json['verificationStatus'] as String? ?? 'Pending',
      rejectionReason: json['rejectionReason'] as String?,
      reviewedAt: json['reviewedAt'] != null
          ? DateTime.tryParse(json['reviewedAt'] as String)
          : null,
      isActive: json['isActive'] as bool? ?? true,
      averageRating: (json['averageRating'] as num?)?.toDouble() ?? 0,
      totalReviews: json['totalReviews'] as int? ?? 0,
      createdAt:
          DateTime.tryParse(json['createdAt'] as String? ?? '') ?? DateTime.now(),
    );
  }
}

/// Fetches admin business list, optionally filtered by verification status.
class AdminBusinessesNotifier
    extends AsyncNotifier<List<AdminBusiness>> {
  String _status = 'Pending';

  Future<List<AdminBusiness>> _fetch(String status) async {
    final api = ref.read(apiClientProvider);
    final response = await api.get(ApiConstants.adminBusinesses, queryParameters: {
      if (status != 'All') 'status': status,
      'page': 1,
      'pageSize': 50,
    });
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
        .map((e) => AdminBusiness.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  @override
  Future<List<AdminBusiness>> build() => _fetch(_status);

  Future<void> setStatus(String status) async {
    _status = status;
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() => _fetch(_status));
  }

  Future<void> refresh() async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() => _fetch(_status));
  }

  /// Approve a pending business. Returns error string or null on success.
  Future<String?> verify(String businessId) async {
    try {
      final api = ref.read(apiClientProvider);
      await api.post(ApiConstants.adminVerify(businessId));
      await refresh();
      return null;
    } catch (e) {
      return 'Verify failed: $e';
    }
  }

  /// Reject a pending business with a reason.
  Future<String?> reject(String businessId, String reason) async {
    try {
      final api = ref.read(apiClientProvider);
      await api.post(ApiConstants.adminReject(businessId), data: {
        'reason': reason,
      });
      await refresh();
      return null;
    } catch (e) {
      return 'Reject failed: $e';
    }
  }
}

final adminBusinessesProvider =
    AsyncNotifierProvider<AdminBusinessesNotifier, List<AdminBusiness>>(
        AdminBusinessesNotifier.new);

/// A verification document attached to a business.
class BusinessDocument {
  final String id;
  final String documentType;
  final String fileName;
  final String uploadedByName;
  final DateTime createdAt;

  const BusinessDocument({
    required this.id,
    required this.documentType,
    required this.fileName,
    required this.uploadedByName,
    required this.createdAt,
  });

  factory BusinessDocument.fromJson(Map<String, dynamic> json) {
    return BusinessDocument(
      id: json['id'] as String,
      documentType: json['documentType'] as String? ?? 'Unknown',
      fileName: json['fileName'] as String? ?? '',
      uploadedByName: json['uploadedByName'] as String? ?? '',
      createdAt: DateTime.tryParse(json['createdAt'] as String? ?? '') ??
          DateTime.now(),
    );
  }
}

/// Fetches verification documents submitted for a business.
final businessDocumentsProvider =
    FutureProvider.family<List<BusinessDocument>, String>((ref, businessId) async {
  final api = ref.watch(apiClientProvider);
  final response = await api.get(ApiConstants.businessDocuments(businessId));
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
      .map((e) => BusinessDocument.fromJson(e as Map<String, dynamic>))
      .toList();
});
