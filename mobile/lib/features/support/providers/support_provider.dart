import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/constants/api_constants.dart';
import '../../../core/network/api_client.dart';

/// Support ticket categories shown in the contact/report forms.
class SupportCategories {
  static const general = 'General';
  static const booking = 'Booking issue';
  static const payment = 'Payment';
  static const cancellation = 'Cancellation';
  static const account = 'Account';
  static const provider = 'Provider question';
  static const problem = 'Report a problem';

  static const all = [general, booking, payment, cancellation, account, provider];
}

/// A support ticket submitted by the user.
class SupportTicket {
  final String id;
  final String category;
  final String subject;
  final String message;
  final String status;
  final DateTime createdAt;

  const SupportTicket({
    required this.id,
    required this.category,
    required this.subject,
    required this.message,
    required this.status,
    required this.createdAt,
  });

  factory SupportTicket.fromJson(Map<String, dynamic> json) => SupportTicket(
        id: json['id'] as String? ?? '',
        category: json['category'] as String? ?? '',
        subject: json['subject'] as String? ?? '',
        message: json['message'] as String? ?? '',
        status: json['status'] as String? ?? 'Open',
        createdAt: DateTime.tryParse(json['createdAt'] as String? ?? '') ?? DateTime.now(),
      );
}

/// The current user's support tickets.
final myTicketsProvider = FutureProvider<List<SupportTicket>>((ref) async {
  final api = ref.watch(apiClientProvider);
  final response = await api.get(ApiConstants.supportTickets);
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
      .map((e) => SupportTicket.fromJson(e as Map<String, dynamic>))
      .toList();
});

/// Submits a support ticket (contact support or report a problem).
class SupportApi {
  final ApiClient _api;
  SupportApi(this._api);

  Future<void> submit({
    required String category,
    required String subject,
    required String message,
    String? appointmentId,
    String? contactEmail,
  }) async {
    await _api.post(
      ApiConstants.supportTickets,
      data: {
        'category': category,
        'subject': subject,
        'message': message,
        'appointmentId': ?appointmentId,
        if (contactEmail != null && contactEmail.isNotEmpty) 'contactEmail': contactEmail,
      },
    );
  }
}

final supportApiProvider = Provider<SupportApi>((ref) => SupportApi(ref.watch(apiClientProvider)));
