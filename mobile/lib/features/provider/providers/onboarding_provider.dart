import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/api_client.dart';
import '../../../core/constants/api_constants.dart';

/// Result of creating a business.
class CreatedBusiness {
  final String id;
  final String slug;

  const CreatedBusiness({required this.id, required this.slug});

  factory CreatedBusiness.fromJson(Map<String, dynamic> json) =>
      CreatedBusiness(id: json['id'] as String, slug: json['slug'] as String? ?? '');
}

/// Day hours entry for onboarding.
class DayHoursInput {
  final int dayOfWeek; // 0=Sunday .. 6=Saturday (System.DayOfWeek)
  final String openTime;
  final String closeTime;
  final bool isClosed;

  const DayHoursInput({
    required this.dayOfWeek,
    required this.openTime,
    required this.closeTime,
    required this.isClosed,
  });

  Map<String, dynamic> toJson() => {
        'dayOfWeek': dayOfWeek,
        'openTime': openTime,
        'closeTime': closeTime,
        'isClosed': isClosed,
      };
}

/// Service draft during onboarding.
class ServiceDraft {
  final String name;
  final String? description;
  final int durationMinutes;
  final double priceAmount;
  final String currency;
  final String? category;

  const ServiceDraft({
    required this.name,
    this.description,
    required this.durationMinutes,
    required this.priceAmount,
    this.currency = 'USD',
    this.category,
  });

  Map<String, dynamic> toJson() => {
        'name': name,
        'description': description,
        'durationMinutes': durationMinutes,
        'priceAmount': priceAmount,
        'currency': currency,
        'category': category,
      };
}

/// Provider/staff draft during onboarding.
class ProviderDraft {
  final String firstName;
  final String lastName;
  final String email;
  final String? title;
  final String? bio;
  final String? avatarUrl;

  const ProviderDraft({
    required this.firstName,
    required this.lastName,
    required this.email,
    this.title,
    this.bio,
    this.avatarUrl,
  });

  Map<String, dynamic> toJson() => {
        'firstName': firstName,
        'lastName': lastName,
        'email': email,
        'title': title,
        'bio': bio,
        'avatarUrl': avatarUrl,
        'displayOrder': 1,
      };
}

/// Performs the multi-step onboarding API calls.
class OnboardingApi {
  final ApiClient _api;

  OnboardingApi(this._api);

  /// Step 1: create the business listing (returns pending state on backend).
  Future<CreatedBusiness> createBusiness({
    required String name,
    required String description,
    required String email,
    required String phoneNumber,
    required String website,
    required String addressLine1,
    String? addressLine2,
    required String city,
    String? state,
    required String postalCode,
    required String country,
    required String timeZone,
    required String currency,
    String? cancellationPolicy,
    String? coverImageUrl,
    required List<String> categoryIds,
  }) async {
    final response = await _api.post(
      ApiConstants.businesses,
      data: {
        'name': name,
        'description': description,
        'email': email,
        'phoneNumber': phoneNumber,
        'website': website,
        'addressLine1': addressLine1,
        'addressLine2': addressLine2,
        'city': city,
        'state': state,
        'postalCode': postalCode,
        'country': country,
        'timeZone': timeZone,
        'currency': currency,
        'cancellationPolicy': cancellationPolicy,
        'coverImageUrl': coverImageUrl,
        'categoryIds': categoryIds,
      },
    );

    final body = response.data as Map<String, dynamic>;
    final data = (body['data'] ?? body) as Map<String, dynamic>;
    return CreatedBusiness.fromJson(data);
  }

  /// Step 2: set weekly opening hours.
  Future<void> setBusinessHours(String businessId, List<DayHoursInput> hours) async {
    await _api.put(
      ApiConstants.businessHours(businessId),
      data: {'hours': hours.map((h) => h.toJson()).toList()},
    );
  }

  /// Step 3: add a service.
  Future<String> addService(String businessId, ServiceDraft draft) async {
    final response = await _api.post(
      ApiConstants.businessServices(businessId),
      data: draft.toJson(),
    );
    final body = response.data as Map<String, dynamic>;
    final data = (body['data'] ?? body) as Map<String, dynamic>;
    return data['serviceId'] as String? ?? '';
  }

  /// Step 4: add a provider/staff member.
  Future<String> addProvider(String businessId, ProviderDraft draft) async {
    final response = await _api.post(
      ApiConstants.businessProviders(businessId),
      data: draft.toJson(),
    );
    final body = response.data as Map<String, dynamic>;
    final data = (body['data'] ?? body) as Map<String, dynamic>;
    return data['providerId'] as String? ?? '';
  }

  /// Full onboarding in one call — returns the created business id + slug.
  Future<CreatedBusiness> runOnboarding({
    required String name,
    required String description,
    required String email,
    required String phoneNumber,
    required String website,
    required String addressLine1,
    String? addressLine2,
    required String city,
    String? state,
    required String postalCode,
    required String country,
    required String timeZone,
    required String currency,
    String? cancellationPolicy,
    String? coverImageUrl,
    required List<String> categoryIds,
    required List<DayHoursInput> hours,
    required List<ServiceDraft> services,
    required List<ProviderDraft> providers,
  }) async {
    // 1. Create business
    final business = await createBusiness(
      name: name,
      description: description,
      email: email,
      phoneNumber: phoneNumber,
      website: website,
      addressLine1: addressLine1,
      addressLine2: addressLine2,
      city: city,
      state: state,
      postalCode: postalCode,
      country: country,
      timeZone: timeZone,
      currency: currency,
      cancellationPolicy: cancellationPolicy,
      coverImageUrl: coverImageUrl,
      categoryIds: categoryIds,
    );

    // 2. Set hours
    if (hours.isNotEmpty) {
      await setBusinessHours(business.id, hours);
    }

    // 3. Add services
    for (final service in services) {
      await addService(business.id, service);
    }

    // 4. Add providers
    for (final provider in providers) {
      await addProvider(business.id, provider);
    }

    return business;
  }
}

final onboardingApiProvider = Provider<OnboardingApi>((ref) {
  return OnboardingApi(ref.watch(apiClientProvider));
});
