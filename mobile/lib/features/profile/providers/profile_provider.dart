import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/api_client.dart';
import '../../../core/constants/api_constants.dart';

/// User profile model matching the backend API.
class UserProfile {
  final String id;
  final String firstName;
  final String lastName;
  final String email;
  final String? phoneNumber;
  final String? avatarUrl;
  final int role;

  UserProfile({
    required this.id,
    required this.firstName,
    required this.lastName,
    required this.email,
    this.phoneNumber,
    this.avatarUrl,
    required this.role,
  });

  String get fullName => '$firstName $lastName';

  factory UserProfile.fromJson(Map<String, dynamic> json) {
    return UserProfile(
      id: json['id'] as String,
      firstName: json['firstName'] as String,
      lastName: json['lastName'] as String,
      email: json['email'] as String,
      phoneNumber: json['phoneNumber'] as String?,
      avatarUrl: json['avatarUrl'] as String?,
      role: json['role'] as int? ?? 0,
    );
  }
}

/// Provider that fetches the authenticated user's profile.
final profileProvider = FutureProvider<UserProfile>((ref) async {
  final api = ref.watch(apiClientProvider);
  final response = await api.get(ApiConstants.userProfile);
  final data = response.data;

  if (data is Map<String, dynamic> && data.containsKey('data')) {
    return UserProfile.fromJson(data['data'] as Map<String, dynamic>);
  }
  if (data is Map<String, dynamic>) {
    return UserProfile.fromJson(data);
  }
  throw StateError('Unexpected profile response format');
});
