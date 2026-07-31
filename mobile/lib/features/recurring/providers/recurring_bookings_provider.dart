import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/api_client.dart';
import '../../../core/constants/api_constants.dart';

/// Recurrence patterns matching backend RecurrenceType enum.
class RecurrenceTypeValue {
  static const int daily = 0;
  static const int weekly = 1;
  static const int monthly = 2;
  static const int custom = 3;
}

/// Recurring booking model matching backend RecurringBookingDto.
class RecurringBooking {
  final String id;
  final String recurrenceType; // "Weekly", "Monthly", ...
  final int interval;
  final String startTime; // "HH:mm:ss"
  final String endTime;
  final DateTime seriesStartDate;
  final DateTime? seriesEndDate;
  final int? maxOccurrences;
  final int occurrencesCreated;
  final bool isActive;
  final String providerName;
  final String serviceName;
  final String businessName;
  final String? notes;
  final DateTime createdAt;

  const RecurringBooking({
    required this.id,
    required this.recurrenceType,
    required this.interval,
    required this.startTime,
    required this.endTime,
    required this.seriesStartDate,
    this.seriesEndDate,
    this.maxOccurrences,
    required this.occurrencesCreated,
    required this.isActive,
    required this.providerName,
    required this.serviceName,
    required this.businessName,
    this.notes,
    required this.createdAt,
  });

  String get recurrenceLabel {
    final type = recurrenceType.toLowerCase();
    final every = interval > 1 ? 'every $interval ' : '';
    return switch (type) {
      'daily' => 'Daily$every',
      'weekly' => every.isNotEmpty ? 'Every $interval weeks' : 'Weekly',
      'monthly' => every.isNotEmpty ? 'Every $interval months' : 'Monthly',
      _ => recurrenceType,
    };
  }

  factory RecurringBooking.fromJson(Map<String, dynamic> json) =>
      RecurringBooking(
        id: json['id'] as String? ?? '',
        recurrenceType: json['recurrenceType'] as String? ?? '',
        interval: json['interval'] as int? ?? 1,
        startTime: json['startTime'] as String? ?? '09:00:00',
        endTime: json['endTime'] as String? ?? '10:00:00',
        seriesStartDate:
            DateTime.tryParse(json['seriesStartDate'] as String? ?? '') ??
                DateTime.now(),
        seriesEndDate: json['seriesEndDate'] != null
            ? DateTime.tryParse(json['seriesEndDate'] as String)
            : null,
        maxOccurrences: json['maxOccurrences'] as int?,
        occurrencesCreated: json['occurrencesCreated'] as int? ?? 0,
        isActive: json['isActive'] as bool? ?? false,
        providerName: json['providerName'] as String? ?? '',
        serviceName: json['serviceName'] as String? ?? '',
        businessName: json['businessName'] as String? ?? '',
        notes: json['notes'] as String?,
        createdAt: DateTime.tryParse(json['createdAt'] as String? ?? '') ??
            DateTime.now(),
      );
}

/// API methods for recurring bookings.
class RecurringBookingsApi {
  final ApiClient _api;

  RecurringBookingsApi(this._api);

  /// Create a recurring booking series.
  Future<void> create({
    required String providerId,
    required String serviceId,
    required String businessId,
    required int recurrenceType, // RecurrenceTypeValue.*
    required String startTime, // "HH:mm"
    required String endTime,
    required DateTime seriesStartDate,
    DateTime? seriesEndDate,
    int? maxOccurrences,
    int interval = 1,
    int? dayOfMonth,
    List<int>? daysOfWeek, // DayOfWeek ints 0-6 (Sunday=0)
    String? notes,
  }) async {
    await _api.post(
      ApiConstants.recurringBookings,
      data: {
        'providerId': providerId,
        'serviceId': serviceId,
        'businessId': businessId,
        'recurrenceType': recurrenceType,
        'startTime': startTime,
        'endTime': endTime,
        'seriesStartDate':
            seriesStartDate.toIso8601String().split('T').first,
        'seriesEndDate': ?(seriesEndDate?.toIso8601String().split('T').first),
        'maxOccurrences': ?maxOccurrences,
        'interval': interval,
        'dayOfMonth': ?dayOfMonth,
        'daysOfWeek': daysOfWeek ?? <int>[],
        'notes': ?(notes != null && notes.isNotEmpty ? notes : null),
      },
    );
  }

  /// List the current user's recurring booking series.
  Future<List<RecurringBooking>> getMyRecurringBookings() async {
    final response = await _api.get(
      ApiConstants.recurringBookings,
      queryParameters: {'role': 'customer'},
    );
    return _parseList(response.data);
  }

  /// Cancel an entire recurring series.
  Future<void> cancelSeries(String recurringBookingId) async {
    await _api.put(ApiConstants.recurringCancel(recurringBookingId));
  }

  /// Skip the next upcoming occurrence.
  Future<void> skipNext(String recurringBookingId) async {
    await _api.put(ApiConstants.recurringSkipNext(recurringBookingId));
  }

  List<RecurringBooking> _parseList(dynamic data) {
    if (data is Map<String, dynamic>) {
      final inner = data['data'];
      if (inner is Map<String, dynamic>) {
        final items = inner['items'];
        if (items is List) {
          return items
              .map((e) => RecurringBooking.fromJson(e as Map<String, dynamic>))
              .toList();
        }
      }
      if (inner is List) {
        return inner
            .map((e) => RecurringBooking.fromJson(e as Map<String, dynamic>))
            .toList();
      }
    }
    if (data is List) {
      return data
          .map((e) => RecurringBooking.fromJson(e as Map<String, dynamic>))
          .toList();
    }
    return [];
  }
}

final recurringBookingsApiProvider = Provider<RecurringBookingsApi>((ref) {
  return RecurringBookingsApi(ref.watch(apiClientProvider));
});

/// The current user's recurring booking series.
final myRecurringBookingsProvider =
    FutureProvider<List<RecurringBooking>>((ref) async {
  final api = ref.watch(recurringBookingsApiProvider);
  return api.getMyRecurringBookings();
});
