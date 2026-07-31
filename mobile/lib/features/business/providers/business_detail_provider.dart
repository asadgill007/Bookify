import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/api_client.dart';
import '../../../core/constants/api_constants.dart';

/// Full business detail fetched from GET /businesses/{slug}.
class BusinessDetail {
  final String id;
  final String name;
  final String slug;
  final String? description;
  final String? email;
  final String? phoneNumber;
  final String addressLine1;
  final String? addressLine2;
  final String city;
  final String? state;
  final String postalCode;
  final String country;
  final String? website;
  final bool isVerified;
  final String verificationStatus;
  final String? rejectionReason;
  final String bookingType;
  final String? cancellationPolicy;
  final String timeZone;
  final String currency;
  final double averageRating;
  final int totalReviews;
  final List<String> categories;
  final String? coverImageUrl;
  final String? logoUrl;
  final List<String> gallery;
  final List<ServiceItem> services;
  final List<ProviderItem> providers;
  final List<BusinessHoursItem> openingHours;

  const BusinessDetail({
    required this.id,
    required this.name,
    required this.slug,
    this.description,
    this.email,
    this.phoneNumber,
    required this.addressLine1,
    this.addressLine2,
    required this.city,
    this.state,
    required this.postalCode,
    required this.country,
    this.website,
    required this.isVerified,
    required this.verificationStatus,
    this.rejectionReason,
    required this.bookingType,
    this.cancellationPolicy,
    required this.timeZone,
    required this.currency,
    required this.averageRating,
    required this.totalReviews,
    required this.categories,
    this.coverImageUrl,
    this.logoUrl,
    required this.gallery,
    required this.services,
    required this.providers,
    required this.openingHours,
  });

  factory BusinessDetail.fromJson(Map<String, dynamic> json) {
    final address = json['address'] as Map<String, dynamic>? ?? {};
    return BusinessDetail(
      id: json['id'] as String? ?? '',
      name: json['name'] as String? ?? '',
      slug: json['slug'] as String? ?? '',
      description: json['description'] as String?,
      email: json['email'] as String?,
      phoneNumber: json['phoneNumber'] as String?,
      addressLine1: address['line1'] as String? ?? '',
      addressLine2: address['line2'] as String?,
      city: address['city'] as String? ?? '',
      state: address['state'] as String?,
      postalCode: address['postalCode'] as String? ?? '',
      country: address['country'] as String? ?? '',
      website: json['website'] as String?,
      isVerified: json['isVerified'] as bool? ?? false,
      verificationStatus: json['verificationStatus'] as String? ?? 'Pending',
      rejectionReason: json['rejectionReason'] as String?,
      bookingType: json['bookingType'] as String? ?? 'Instant',
      cancellationPolicy: json['cancellationPolicy'] as String?,
      timeZone: json['timeZone'] as String? ?? 'UTC',
      currency: json['currency'] as String? ?? 'USD',
      averageRating: (json['averageRating'] as num?)?.toDouble() ?? 0,
      totalReviews: json['totalReviews'] as int? ?? 0,
      categories: (json['categories'] as List<dynamic>?)
              ?.map((e) => e.toString())
              .toList() ??
          [],
      coverImageUrl: json['coverImageUrl'] as String?,
      logoUrl: json['logoUrl'] as String?,
      gallery: (json['gallery'] as List<dynamic>?)
              ?.map((e) => (e as Map<String, dynamic>)['url'] as String? ?? '')
              .where((u) => u.isNotEmpty)
              .toList() ??
          [],
      services: (json['services'] as List<dynamic>?)
              ?.map((e) => ServiceItem.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      providers: (json['providers'] as List<dynamic>?)
              ?.map((e) => ProviderItem.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      openingHours: (json['openingHours'] as List<dynamic>?)
              ?.map((e) => BusinessHoursItem.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}

class ServiceItem {
  final String id;
  final String name;
  final String? description;
  final int durationMinutes;
  final double price;
  final String currency;
  final String? category;

  const ServiceItem({
    required this.id,
    required this.name,
    this.description,
    required this.durationMinutes,
    required this.price,
    required this.currency,
    this.category,
  });

  factory ServiceItem.fromJson(Map<String, dynamic> json) => ServiceItem(
        id: json['id'] as String? ?? '',
        name: json['name'] as String? ?? '',
        description: json['description'] as String?,
        durationMinutes: json['durationMinutes'] as int? ?? 60,
        price: (json['price'] as num?)?.toDouble() ?? 0,
        currency: json['currency'] as String? ?? 'USD',
        category: json['category'] as String?,
      );
}

class ProviderItem {
  final String id;
  final String firstName;
  final String lastName;
  final String? title;
  final String? avatarUrl;

  const ProviderItem({
    required this.id,
    required this.firstName,
    required this.lastName,
    this.title,
    this.avatarUrl,
  });

  String get fullName => '$firstName $lastName'.trim();

  factory ProviderItem.fromJson(Map<String, dynamic> json) => ProviderItem(
        id: json['id'] as String? ?? '',
        firstName: json['firstName'] as String? ?? '',
        lastName: json['lastName'] as String? ?? '',
        title: json['title'] as String?,
        avatarUrl: json['avatarUrl'] as String?,
      );
}

class BusinessHoursItem {
  final String dayOfWeek;
  final String openTime;
  final String closeTime;
  final bool isClosed;

  const BusinessHoursItem({
    required this.dayOfWeek,
    required this.openTime,
    required this.closeTime,
    required this.isClosed,
  });

  factory BusinessHoursItem.fromJson(Map<String, dynamic> json) =>
      BusinessHoursItem(
        dayOfWeek: json['dayOfWeek'] as String? ?? '',
        openTime: json['openTime'] as String? ?? '09:00',
        closeTime: json['closeTime'] as String? ?? '17:00',
        isClosed: json['isClosed'] as bool? ?? false,
      );
}

/// Fetches business detail by slug.
final businessDetailProvider =
    FutureProvider.family<BusinessDetail, String>((ref, slug) async {
  final api = ref.watch(apiClientProvider);
  final response = await api.get(ApiConstants.businessBySlug(slug));
  final body = response.data as Map<String, dynamic>;
  final data = (body['data'] ?? body) as Map<String, dynamic>;
  return BusinessDetail.fromJson(data);
});

/// Time slot returned by GET /providers/{id}/slots.
class TimeSlot {
  final String startTime;
  final String endTime;
  final bool isAvailable;
  final String? reason;

  const TimeSlot({
    required this.startTime,
    required this.endTime,
    required this.isAvailable,
    this.reason,
  });

  factory TimeSlot.fromJson(Map<String, dynamic> json) => TimeSlot(
        startTime: json['startTime'] as String,
        endTime: json['endTime'] as String,
        isAvailable: json['isAvailable'] as bool? ?? true,
        reason: json['reason'] as String?,
      );
}

/// Params key for the slots provider (record-based for stable identity).
typedef SlotsParams = ({String providerId, String? serviceId, String date});

/// Fetches available slots for a provider on a date.
final providerSlotsProvider =
    FutureProvider.family<List<TimeSlot>, SlotsParams>((ref, params) async {
  final api = ref.watch(apiClientProvider);
  final response = await api.get(
    ApiConstants.providerSlots(params.providerId),
    queryParameters: {
      if (params.serviceId != null) 'serviceId': params.serviceId,
      'date': params.date,
    },
  );
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
      .map((e) => TimeSlot.fromJson(e as Map<String, dynamic>))
      .toList();
});
