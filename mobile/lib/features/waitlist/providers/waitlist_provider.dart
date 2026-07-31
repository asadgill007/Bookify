import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/api_client.dart';
import '../../../core/constants/api_constants.dart';

/// Waitlist entry model matching the backend WaitlistEntryDto.
class WaitlistEntry {
  final String id;
  final String businessId;
  final String providerId;
  final String serviceId;
  final String appointmentDate; // yyyy-MM-dd
  final String? preferredStartTime;
  final String? preferredEndTime;
  final String? notes;
  final String status; // Waiting, Notified, Booked, Cancelled...
  final int priority;
  final int position;
  final String customerName;
  final String providerName;
  final String serviceName;
  final DateTime createdAt;

  const WaitlistEntry({
    required this.id,
    required this.businessId,
    required this.providerId,
    required this.serviceId,
    required this.appointmentDate,
    this.preferredStartTime,
    this.preferredEndTime,
    this.notes,
    required this.status,
    required this.priority,
    required this.position,
    required this.customerName,
    required this.providerName,
    required this.serviceName,
    required this.createdAt,
  });

  bool get isActive =>
      status.toLowerCase() == 'waiting' || status.toLowerCase() == 'notified';

  factory WaitlistEntry.fromJson(Map<String, dynamic> json) => WaitlistEntry(
        id: json['id'] as String? ?? '',
        businessId: json['businessId'] as String? ?? '',
        providerId: json['providerId'] as String? ?? '',
        serviceId: json['serviceId'] as String? ?? '',
        appointmentDate: json['appointmentDate'] as String? ?? '',
        preferredStartTime: json['preferredStartTime'] as String?,
        preferredEndTime: json['preferredEndTime'] as String?,
        notes: json['notes'] as String?,
        status: json['status'] as String? ?? 'Waiting',
        priority: json['priority'] as int? ?? 0,
        position: json['position'] as int? ?? 0,
        customerName: json['customerName'] as String? ?? '',
        providerName: json['providerName'] as String? ?? '',
        serviceName: json['serviceName'] as String? ?? '',
        createdAt: DateTime.tryParse(json['createdAt'] as String? ?? '') ??
            DateTime.now(),
      );
}

/// API methods for the waitlist.
class WaitlistApi {
  final ApiClient _api;

  WaitlistApi(this._api);

  /// Join the waitlist for a provider/service on a date.
  Future<({String entryId, int position})> join({
    required String businessId,
    required String providerId,
    required String serviceId,
    required String appointmentDate, // yyyy-MM-dd
    String? preferredStartTime,
    String? preferredEndTime,
    String? notes,
  }) async {
    final response = await _api.post(
      ApiConstants.waitlistJoin,
      data: {
        'businessId': businessId,
        'providerId': providerId,
        'serviceId': serviceId,
        'appointmentDate': appointmentDate,
        'preferredStartTime': ?preferredStartTime,
        'preferredEndTime': ?preferredEndTime,
        'notes': ?(notes != null && notes.isNotEmpty ? notes : null),
      },
    );
    final body = response.data;
    if (body is Map<String, dynamic>) {
      final data = (body['data'] ?? body) as Map<String, dynamic>;
      return (
        entryId: data['entryId'] as String? ?? '',
        position: data['position'] as int? ?? 0,
      );
    }
    return (entryId: '', position: 0);
  }

  /// List the current user's waitlist entries.
  Future<List<WaitlistEntry>> getMyWaitlist() async {
    final response = await _api.get(ApiConstants.myWaitlist);
    return _parseList(response.data);
  }

  /// Leave (cancel) a waitlist entry.
  Future<void> leave(String entryId) async {
    await _api.delete(ApiConstants.waitlistEntry(entryId));
  }

  List<WaitlistEntry> _parseList(dynamic data) {
    if (data is Map<String, dynamic>) {
      final inner = data['data'];
      if (inner is Map<String, dynamic>) {
        final items = inner['items'];
        if (items is List) {
          return items
              .map((e) => WaitlistEntry.fromJson(e as Map<String, dynamic>))
              .toList();
        }
      }
      if (inner is List) {
        return inner
            .map((e) => WaitlistEntry.fromJson(e as Map<String, dynamic>))
            .toList();
      }
    }
    if (data is List) {
      return data
          .map((e) => WaitlistEntry.fromJson(e as Map<String, dynamic>))
          .toList();
    }
    return [];
  }
}

final waitlistApiProvider = Provider<WaitlistApi>((ref) {
  return WaitlistApi(ref.watch(apiClientProvider));
});

/// The current user's waitlist entries.
final myWaitlistProvider = FutureProvider<List<WaitlistEntry>>((ref) async {
  final api = ref.watch(waitlistApiProvider);
  return api.getMyWaitlist();
});
