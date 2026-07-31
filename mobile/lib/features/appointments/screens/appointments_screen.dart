import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/api_client.dart';
import '../../../core/constants/api_constants.dart';
import '../../../core/theme/app_theme.dart';
import '../../reviews/screens/review_form_screen.dart';

/// Appointment model from the backend API.
class Appointment {
  final String id;
  final String bookingReference;
  final String businessName;
  final DateTime startTime;
  final DateTime endTime;
  final String status;
  final double totalAmount;
  final String currency;

  Appointment({
    required this.id,
    required this.bookingReference,
    required this.businessName,
    required this.startTime,
    required this.endTime,
    required this.status,
    required this.totalAmount,
    required this.currency,
  });

  factory Appointment.fromJson(Map<String, dynamic> json) {
    return Appointment(
      id: json['id'] as String,
      bookingReference: json['bookingReference'] as String,
      businessName: json['businessName'] as String? ?? 'Unknown',
      startTime: DateTime.parse(json['startTime'] as String),
      endTime: DateTime.parse(json['endTime'] as String),
      status: json['status'] as String? ?? 'Pending',
      totalAmount: (json['totalAmount'] as num?)?.toDouble() ?? 0,
      currency: json['currency'] as String? ?? 'USD',
    );
  }
}

/// Provider that fetches user's appointments.
final appointmentsProvider = FutureProvider<List<Appointment>>((ref) async {
  final api = ref.watch(apiClientProvider);
  final response = await api.get(ApiConstants.appointments);
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
      .map((e) => Appointment.fromJson(e as Map<String, dynamic>))
      .toList();
});

/// My Appointments screen.
class AppointmentsScreen extends ConsumerWidget {
  const AppointmentsScreen({super.key});

  bool _isCompleted(String status) {
    final s = status.toLowerCase();
    return s == 'completed' || s == 'noshow';
  }

  void _openReviewFlow(BuildContext context, Appointment apt) {
    // Fetch business name for the review header via a lightweight call is
    // unnecessary: the appointment already carries businessName.
    Navigator.of(context).push<bool>(
      MaterialPageRoute(
        builder: (_) => ReviewFormScreen(
          appointmentId: apt.id,
          businessName: apt.businessName,
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final appointmentsAsync = ref.watch(appointmentsProvider);

    return Scaffold(
      appBar: AppBar(title: const Text('My Appointments')),
      body: appointmentsAsync.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (err, _) => Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(Icons.error_outline, size: 48, color: colorScheme.error),
              const SizedBox(height: 16),
              Text('Failed to load appointments',
                  style: theme.textTheme.titleMedium),
              const SizedBox(height: 16),
              FilledButton.tonal(
                onPressed: () => ref.invalidate(appointmentsProvider),
                child: const Text('Retry'),
              ),
            ],
          ),
        ),
        data: (appointments) {
          if (appointments.isEmpty) {
            return Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.calendar_month_outlined, size: 64,
                      color: colorScheme.onSurfaceVariant.withValues(alpha: 0.4)),
                  const SizedBox(height: 16),
                  Text('No appointments yet',
                      style: theme.textTheme.titleMedium?.copyWith(
                          color: colorScheme.onSurfaceVariant)),
                  const SizedBox(height: 8),
                  Text('Book your first appointment to get started',
                      style: theme.textTheme.bodyMedium?.copyWith(
                          color: colorScheme.onSurfaceVariant)),
                ],
              ),
            );
          }

          return ListView.builder(
            padding: const EdgeInsets.all(16),
            itemCount: appointments.length,
            itemBuilder: (context, index) {
              final apt = appointments[index];
              final canReview = _isCompleted(apt.status);
              return Card(
                margin: const EdgeInsets.only(bottom: 12),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    ListTile(
                      contentPadding: const EdgeInsets.all(16),
                      title: Text(apt.businessName,
                          style: theme.textTheme.titleSmall?.copyWith(fontWeight: FontWeight.w600)),
                      subtitle: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const SizedBox(height: 4),
                          Text('Ref: ${apt.bookingReference}'),
                          Text('${apt.startTime.day}/${apt.startTime.month}/${apt.startTime.year}'),
                          Text('${apt.totalAmount.toStringAsFixed(2)} ${apt.currency}'),
                        ],
                      ),
                      trailing: Chip(
                        label: Text(apt.status,
                            style: theme.textTheme.labelSmall?.copyWith(
                                color: apt.status == 'Confirmed' ? Colors.green : colorScheme.onSurface)),
                        backgroundColor: apt.status == 'Confirmed'
                            ? Colors.green.shade50
                            : colorScheme.surfaceContainerHighest,
                      ),
                      isThreeLine: true,
                    ),
                    if (canReview)
                      Padding(
                        padding: const EdgeInsets.fromLTRB(16, 0, 16, 12),
                        child: SizedBox(
                          width: double.infinity,
                          height: 40,
                          child: DecoratedBox(
                            decoration: BoxDecoration(
                              gradient: LinearGradient(
                                colors: [
                                  AppTheme.indigoLuxury,
                                  const Color(0xFF7C3AED),
                                ],
                                begin: Alignment.topLeft,
                                end: Alignment.bottomRight,
                              ),
                              borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                            ),
                            child: MaterialButton(
                              onPressed: () => _openReviewFlow(context, apt),
                              shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.circular(AppTheme.radiusFull)),
                              child: const Text('Write a Review',
                                  style: TextStyle(
                                      color: Colors.white,
                                      fontSize: 13,
                                      fontWeight: FontWeight.w600)),
                            ),
                          ),
                        ),
                      ),
                  ],
                ),
              );
            },
          );
        },
      ),
    );
  }
}
