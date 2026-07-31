import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../../../core/theme/app_theme.dart';
import '../../business/providers/business_detail_provider.dart';
import '../../recurring/providers/recurring_bookings_provider.dart';
import '../../waitlist/providers/waitlist_provider.dart';

/// Recurrence options chosen on the booking screen (null = one-off booking).
class RecurrenceChoice {
  final int type; // RecurrenceTypeValue: 1=weekly, 2=monthly
  final int interval;
  final int? maxOccurrences;
  final DateTime? endDate;

  const RecurrenceChoice({
    required this.type,
    this.interval = 1,
    this.maxOccurrences,
    this.endDate,
  });

  bool get isMonthly => type == RecurrenceTypeValue.monthly;
}

/// Booking draft passed from booking screen to checkout.
class BookingDraft {
  final String businessId;
  final String businessName;
  final String serviceId;
  final String serviceName;
  final int durationMinutes;
  final double price;
  final String currency;
  final String providerId;
  final String providerName;
  final DateTime startTime;
  final DateTime endTime;
  final RecurrenceChoice? recurrence;

  const BookingDraft({
    required this.businessId,
    required this.businessName,
    required this.serviceId,
    required this.serviceName,
    required this.durationMinutes,
    required this.price,
    required this.currency,
    required this.providerId,
    required this.providerName,
    required this.startTime,
    required this.endTime,
    this.recurrence,
  });
}

/// Premium Booking screen, wired to the real services/providers/slots APIs.
class BookingScreen extends ConsumerStatefulWidget {
  final String businessSlug;
  final String serviceId;

  const BookingScreen({
    super.key,
    required this.businessSlug,
    required this.serviceId,
  });

  @override
  ConsumerState<BookingScreen> createState() => _BookingScreenState();
}

class _BookingScreenState extends ConsumerState<BookingScreen> {
  DateTime _selectedDate = DateTime.now().add(const Duration(days: 1));
  String? _selectedProviderId;
  String? _selectedSlot;
  String _selectedServiceId = '';
  final _notesController = TextEditingController();

  // Recurring booking state
  bool _isRecurring = false;
  int _recurrenceType = RecurrenceTypeValue.weekly;
  int _recurrenceInterval = 1;
  int _maxOccurrences = 6;
  DateTime? _seriesEndDate;
  bool _useEndDate = false;

  // Waitlist state
  bool _joiningWaitlist = false;
  String? _waitlistError;
  TimeOfDay? _preferredTime;
  String? _waitlistResult;

  @override
  void initState() {
    super.initState();
    _selectedServiceId = widget.serviceId;
  }

  @override
  void dispose() {
    _notesController.dispose();
    super.dispose();
  }

  List<DateTime> get _dates =>
      List.generate(14, (i) => DateTime.now().add(Duration(days: i + 1)));

  String _formatDateParam(DateTime d) =>
      '${d.year.toString().padLeft(4, '0')}-'
      '${d.month.toString().padLeft(2, '0')}-'
      '${d.day.toString().padLeft(2, '0')}';

  Future<void> _joinWaitlist(
      BusinessDetail business, String serviceId, String providerId) async {
    if (_joiningWaitlist) return;
    setState(() {
      _joiningWaitlist = true;
      _waitlistError = null;
      _waitlistResult = null;
    });
    try {
      final api = ref.read(waitlistApiProvider);
      final preferred = _preferredTime;
      final result = await api.join(
        businessId: business.id,
        providerId: providerId,
        serviceId: serviceId,
        appointmentDate: _formatDateParam(_selectedDate),
        preferredStartTime: preferred != null
            ? '${preferred.hour.toString().padLeft(2, '0')}:'
                '${preferred.minute.toString().padLeft(2, '0')}'
            : null,
        preferredEndTime: preferred != null
            ? '${(preferred.hour + 1).toString().padLeft(2, '0')}:'
                '${preferred.minute.toString().padLeft(2, '0')}'
            : null,
        notes: _notesController.text.trim().isEmpty
            ? null
            : _notesController.text.trim(),
      );
      if (!mounted) return;
      setState(() {
        _joiningWaitlist = false;
        _waitlistResult =
            'You are #${result.position} in the waitlist. We will notify you '
            'if this slot becomes available.';
      });
    } catch (_) {
      if (!mounted) return;
      setState(() {
        _joiningWaitlist = false;
        _waitlistError = 'Could not join the waitlist. Please try again.';
      });
    }
  }

  Future<void> _pickPreferredTime() async {
    final picked = await showTimePicker(
      context: context,
      initialTime: _preferredTime ?? const TimeOfDay(hour: 9, minute: 0),
    );
    if (picked != null) {
      setState(() => _preferredTime = picked);
    }
  }

  void _goToCheckout(BusinessDetail business, String serviceId, String providerId) {
    if (business.services.isEmpty || business.providers.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
            content: Text('This business has no bookable services or staff.')),
      );
      return;
    }
    if (_selectedSlot == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please pick a time slot.')),
      );
      return;
    }
    final service = business.services.firstWhere(
      (s) => s.id == serviceId,
      orElse: () => business.services.first,
    );
    final provider = business.providers.firstWhere(
      (p) => p.id == providerId,
      orElse: () => business.providers.first,
    );
    final start = DateTime.parse(_selectedSlot!);
    final draft = BookingDraft(
      businessId: business.id,
      businessName: business.name,
      serviceId: service.id,
      serviceName: service.name,
      durationMinutes: service.durationMinutes,
      price: service.price,
      currency: business.currency,
      providerId: providerId,
      providerName: provider.fullName,
      startTime: start,
      endTime: start.add(Duration(minutes: service.durationMinutes)),
      recurrence: _isRecurring
          ? RecurrenceChoice(
              type: _recurrenceType,
              interval: _recurrenceInterval,
              maxOccurrences: _useEndDate ? null : _maxOccurrences,
              endDate: _useEndDate ? _seriesEndDate : null,
            )
          : null,
    );
    context.push('/checkout', extra: draft);
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final isDark = theme.brightness == Brightness.dark;
    final detailAsync = ref.watch(businessDetailProvider(widget.businessSlug));

    return GradientBackground(
      child: Scaffold(
        backgroundColor: Colors.transparent,
        appBar: AppBar(
          title: const Text('Book Appointment'),
          backgroundColor: Colors.transparent,
        ),
        body: detailAsync.when(
          loading: () => const Center(child: CircularProgressIndicator()),
          error: (err, _) => Center(
            child: Padding(
              padding: const EdgeInsets.all(32),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.error_outline, size: 48, color: colorScheme.error),
                  const SizedBox(height: 12),
                  Text('Could not load booking details',
                      style: theme.textTheme.titleMedium),
                  const SizedBox(height: 8),
                  Text(err.toString(),
                      style: theme.textTheme.bodySmall?.copyWith(
                          color: colorScheme.onSurfaceVariant),
                      textAlign: TextAlign.center),
                ],
              ),
            ),
          ),
          data: (business) {
            // Compute effective selections WITHOUT mutating state during build.
            final effectiveServiceId = business.services.any(
              (s) => s.id == _selectedServiceId,
            )
                ? _selectedServiceId
                : (business.services.isNotEmpty
                    ? business.services.first.id
                    : '');
            final effectiveProviderId = _selectedProviderId ??
                (business.providers.isNotEmpty
                    ? business.providers.first.id
                    : null);

            final selectedProvider = business.providers.isNotEmpty
                ? business.providers.firstWhere(
                    (p) => p.id == effectiveProviderId,
                    orElse: () => business.providers.first,
                  )
                : null;

            // Slots for the selected provider + date
            final slotsAsync =
                selectedProvider != null && effectiveServiceId.isNotEmpty
                    ? ref.watch(providerSlotsProvider((
                        providerId: selectedProvider.id,
                        serviceId: effectiveServiceId,
                        date: _formatDateParam(_selectedDate),
                      )))
                    : null;

            return Column(
              children: [
                Expanded(
                  child: CustomScrollView(
                    physics: const BouncingScrollPhysics(),
                    slivers: [
                      // Service selection
                      if (business.services.isNotEmpty) ...[
                        SliverToBoxAdapter(
                          child: Padding(
                            padding: const EdgeInsets.fromLTRB(16, 8, 16, 8),
                            child: Text('Select Service',
                                style: theme.textTheme.titleMedium?.copyWith(
                                    fontWeight: FontWeight.w700)),
                          ).animate().fadeIn(duration: 400.ms),
                        ),
                        SliverList(
                          delegate: SliverChildBuilderDelegate(
                            (context, index) {
                              final service = business.services[index];
                              final isSelected = service.id == effectiveServiceId;
                              return Padding(
                                padding: const EdgeInsets.only(
                                    left: 16, right: 16, bottom: 10),
                                child: GestureDetector(
                                  onTap: () => setState(() {
                                    _selectedServiceId = service.id;
                                    _selectedSlot = null;
                                  }),
                                  child: GlassContainer(
                                    borderRadius: AppTheme.radiusLg,
                                    padding: const EdgeInsets.all(14),
                                    borderSide: isSelected
                                        ? BorderSide(
                                            color: AppTheme.indigoLuxury, width: 2)
                                        : null,
                                    child: Row(
                                      children: [
                                        _radioDot(isSelected, colorScheme),
                                        const SizedBox(width: 10),
                                        Expanded(
                                          child: Column(
                                            crossAxisAlignment:
                                                CrossAxisAlignment.start,
                                            children: [
                                              Text(service.name,
                                                  style: theme.textTheme.titleSmall
                                                      ?.copyWith(
                                                          fontWeight:
                                                              FontWeight.w600)),
                                              Text(
                                                '${service.durationMinutes} min',
                                                style: theme.textTheme.bodySmall
                                                    ?.copyWith(
                                                        color: colorScheme
                                                            .onSurfaceVariant),
                                              ),
                                            ],
                                          ),
                                        ),
                                        Text(
                                          '${business.currency == 'USD' ? '\$' : ''}'
                                          '${service.price.toStringAsFixed(2)}',
                                          style: TextStyle(
                                            fontSize: 18,
                                            fontWeight: FontWeight.w700,
                                            color: AppTheme.indigoLuxury,
                                          ),
                                        ),
                                      ],
                                    ),
                                  ),
                                ),
                              ).animate().fadeIn(
                                  duration: 300.ms, delay: (50 + index * 50).ms);
                            },
                            childCount: business.services.length,
                          ),
                        ),
                      ],

                      // Provider selection
                      if (business.providers.isNotEmpty) ...[
                        SliverToBoxAdapter(
                          child: Padding(
                            padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
                            child: Text('Select Provider',
                                style: theme.textTheme.titleMedium?.copyWith(
                                    fontWeight: FontWeight.w700)),
                          ).animate().fadeIn(duration: 400.ms, delay: 200.ms),
                        ),
                        SliverToBoxAdapter(
                          child: SizedBox(
                            height: 100,
                            child: ListView.separated(
                              scrollDirection: Axis.horizontal,
                              padding: const EdgeInsets.symmetric(horizontal: 16),
                              itemCount: business.providers.length,
                              separatorBuilder: (_, _) => const SizedBox(width: 12),
                              itemBuilder: (context, index) {
                                final prov = business.providers[index];
                                final isSelected = prov.id == effectiveProviderId;
                                return GestureDetector(
                                  onTap: () => setState(() {
                                    _selectedProviderId = prov.id;
                                    _selectedSlot = null;
                                  }),
                                  child: GlassContainer(
                                    width: 100,
                                    borderRadius: AppTheme.radiusLg,
                                    padding: const EdgeInsets.all(12),
                                    borderSide: isSelected
                                        ? BorderSide(
                                            color: AppTheme.indigoLuxury, width: 2)
                                        : null,
                                    child: Column(
                                      mainAxisAlignment: MainAxisAlignment.center,
                                      children: [
                                        CircleAvatar(
                                          radius: 20,
                                          backgroundImage: prov.avatarUrl != null
                                              ? NetworkImage(prov.avatarUrl!)
                                              : null,
                                          backgroundColor: isDark
                                              ? AppTheme.slate700
                                              : AppTheme.slate200,
                                          child: prov.avatarUrl == null
                                              ? const Icon(Icons.person, size: 18)
                                              : null,
                                        ),
                                        const SizedBox(height: 6),
                                        Text(
                                          prov.fullName,
                                          style: TextStyle(
                                            fontSize: 11,
                                            fontWeight: FontWeight.w600,
                                            color: colorScheme.onSurface,
                                          ),
                                          maxLines: 1,
                                          overflow: TextOverflow.ellipsis,
                                        ),
                                      ],
                                    ),
                                  ),
                                ).animate().fadeIn(
                                    duration: 300.ms,
                                    delay: (250 + index * 60).ms);
                              },
                            ),
                          ),
                        ),
                      ],

                      // Date selection
                      SliverToBoxAdapter(
                        child: Padding(
                          padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
                          child: Text('Select Date',
                              style: theme.textTheme.titleMedium?.copyWith(
                                  fontWeight: FontWeight.w700)),
                        ).animate().fadeIn(duration: 400.ms, delay: 300.ms),
                      ),
                      SliverToBoxAdapter(
                        child: SizedBox(
                          height: 80,
                          child: ListView.separated(
                            scrollDirection: Axis.horizontal,
                            padding: const EdgeInsets.symmetric(horizontal: 16),
                            itemCount: _dates.length,
                            separatorBuilder: (_, _) => const SizedBox(width: 8),
                            itemBuilder: (context, index) {
                              final date = _dates[index];
                              final isSelected = date.day == _selectedDate.day &&
                                  date.month == _selectedDate.month;
                              final dayName = [
                                'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'
                              ][date.weekday - 1];

                              return GestureDetector(
                                onTap: () => setState(() {
                                  _selectedDate = date;
                                  _selectedSlot = null;
                                }),
                                child: AnimatedContainer(
                                  duration: 200.ms,
                                  width: 60,
                                  decoration: BoxDecoration(
                                    borderRadius:
                                        BorderRadius.circular(AppTheme.radiusLg),
                                    gradient: isSelected
                                        ? LinearGradient(
                                            colors: [
                                              AppTheme.indigoLuxury,
                                              const Color(0xFF7C3AED),
                                            ],
                                            begin: Alignment.topLeft,
                                            end: Alignment.bottomRight,
                                          )
                                        : null,
                                    color: isSelected
                                        ? null
                                        : (isDark
                                            ? AppTheme.glassDark
                                            : AppTheme.glassLight),
                                    border: Border.all(
                                      color: isSelected
                                          ? Colors.transparent
                                          : (isDark
                                              ? AppTheme.glassStrokeDark
                                              : AppTheme.glassStrokeLight),
                                    ),
                                    boxShadow:
                                        isSelected ? AppTheme.indigoGlowShadow : null,
                                  ),
                                  child: Column(
                                    mainAxisAlignment: MainAxisAlignment.center,
                                    children: [
                                      Text(dayName,
                                          style: TextStyle(
                                            fontSize: 11,
                                            fontWeight: FontWeight.w500,
                                            color: isSelected
                                                ? Colors.white
                                                : colorScheme.onSurfaceVariant,
                                          )),
                                      const SizedBox(height: 4),
                                      Text('${date.day}',
                                          style: TextStyle(
                                            fontSize: 18,
                                            fontWeight: FontWeight.w700,
                                            color: isSelected
                                                ? Colors.white
                                                : colorScheme.onSurface,
                                          )),
                                      Text(
                                        '${date.month}/${date.year.toString().substring(2)}',
                                        style: TextStyle(
                                          fontSize: 9,
                                          color: isSelected
                                              ? Colors.white70
                                              : colorScheme.onSurfaceVariant,
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                              ).animate().fadeIn(
                                  duration: 300.ms,
                                  delay: (350 + index * 30).ms);
                            },
                          ),
                        ),
                      ),

                      // Time slot selection
                      SliverToBoxAdapter(
                        child: Padding(
                          padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
                          child: Text('Select Time',
                              style: theme.textTheme.titleMedium?.copyWith(
                                  fontWeight: FontWeight.w700)),
                        ).animate().fadeIn(duration: 400.ms, delay: 400.ms),
                      ),
                      SliverPadding(
                        padding: const EdgeInsets.symmetric(horizontal: 16),
                        sliver: slotsAsync == null
                            ? const SliverToBoxAdapter(
                                child: Padding(
                                  padding: EdgeInsets.all(24),
                                  child: Center(
                                      child: Text('No providers available.')),
                                ),
                              )
                            : slotsAsync.when(
                                loading: () => const SliverToBoxAdapter(
                                  child: Padding(
                                    padding: EdgeInsets.all(24),
                                    child: Center(
                                        child: CircularProgressIndicator()),
                                  ),
                                ),
                                error: (err, _) => SliverToBoxAdapter(
                                  child: Padding(
                                    padding: const EdgeInsets.all(24),
                                    child: Center(
                                        child: Text('Could not load slots: $err')),
                                  ),
                                ),
                                data: (slots) {
                                  final available = slots
                                      .where((s) => s.isAvailable)
                                      .toList();
                                  if (available.isEmpty) {
                                    // No slots: offer the waitlist.
                                    return SliverToBoxAdapter(
                                      child: Padding(
                                        padding: const EdgeInsets.symmetric(
                                            horizontal: 16, vertical: 8),
                                        child: GlassContainer(
                                          borderRadius: AppTheme.radiusLg,
                                          padding: const EdgeInsets.all(16),
                                          child: Column(
                                            crossAxisAlignment:
                                                CrossAxisAlignment.start,
                                            children: [
                                              Row(
                                                children: [
                                                  Icon(Icons.hourglass_top,
                                                      color:
                                                          AppTheme.indigoLuxury,
                                                      size: 20),
                                                  const SizedBox(width: 8),
                                                  Text('Fully booked? Join the waitlist',
                                                      style: theme.textTheme
                                                          .titleSmall
                                                          ?.copyWith(
                                                              fontWeight:
                                                                  FontWeight
                                                                      .w600)),
                                                ],
                                              ),
                                              const SizedBox(height: 6),
                                              Text(
                                                'No slots are available on this '
                                                'day. Join the waitlist and we\'ll '
                                                'notify you if one opens up.',
                                                style: theme.textTheme.bodySmall
                                                    ?.copyWith(
                                                        color: colorScheme
                                                            .onSurfaceVariant),
                                              ),
                                              const SizedBox(height: 12),
                                              // Preferred time picker
                                              InkWell(
                                                onTap: _pickPreferredTime,
                                                borderRadius:
                                                    BorderRadius.circular(12),
                                                child: Container(
                                                  padding: const EdgeInsets.all(12),
                                                  decoration: BoxDecoration(
                                                    color: AppTheme.indigoLuxury
                                                        .withValues(alpha: 0.08),
                                                    borderRadius: BorderRadius
                                                        .circular(12),
                                                  ),
                                                  child: Row(
                                                    children: [
                                                      const Icon(
                                                          Icons.schedule,
                                                          size: 18,
                                                          color: AppTheme
                                                              .indigoLuxury),
                                                      const SizedBox(width: 8),
                                                      Text(
                                                        _preferredTime != null
                                                            ? 'Preferred time: '
                                                                '${_formatTimeOfDay(_preferredTime!)}'
                                                            : 'Choose a preferred time (optional)',
                                                        style: theme.textTheme
                                                            .bodySmall
                                                            ?.copyWith(
                                                                color: _preferredTime !=
                                                                        null
                                                                    ? colorScheme
                                                                        .onSurface
                                                                    : colorScheme
                                                                        .onSurfaceVariant),
                                                      ),
                                                    ],
                                                  ),
                                                ),
                                              ),
                                              if (_waitlistError != null) ...[
                                                const SizedBox(height: 10),
                                                Text(
                                                  _waitlistError!,
                                                  style: theme.textTheme.bodySmall
                                                      ?.copyWith(
                                                          color: colorScheme
                                                              .error),
                                                ),
                                              ],
                                              if (_waitlistResult != null) ...[
                                                const SizedBox(height: 10),
                                                Container(
                                                  padding: const EdgeInsets.all(10),
                                                  decoration: BoxDecoration(
                                                    color: AppTheme.success
                                                        .withValues(alpha: 0.12),
                                                    borderRadius:
                                                        BorderRadius.circular(10),
                                                  ),
                                                  child: Row(
                                                    children: [
                                                      const Icon(
                                                          Icons.check_circle,
                                                          color:
                                                              AppTheme.success,
                                                          size: 18),
                                                      const SizedBox(width: 8),
                                                      Expanded(
                                                        child: Text(
                                                          _waitlistResult!,
                                                          style: theme
                                                              .textTheme
                                                              .bodySmall
                                                              ?.copyWith(
                                                                  color: AppTheme
                                                                      .success),
                                                        ),
                                                      ),
                                                    ],
                                                  ),
                                                ),
                                              ],
                                              const SizedBox(height: 12),
                                              SizedBox(
                                                width: double.infinity,
                                                height: 44,
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
                                                    borderRadius:
                                                        BorderRadius.circular(
                                                            AppTheme
                                                                .radiusFull),
                                                  ),
                                                  child: MaterialButton(
                                                    onPressed:
                                                        _joiningWaitlist ||
                                                                _waitlistResult !=
                                                                    null
                                                            ? null
                                                            : () => _joinWaitlist(
                                                                  business,
                                                                  effectiveServiceId,
                                                                  effectiveProviderId!,
                                                                ),
                                                    shape: RoundedRectangleBorder(
                                                        borderRadius:
                                                            BorderRadius.circular(
                                                                AppTheme
                                                                    .radiusFull)),
                                                    child: _joiningWaitlist
                                                        ? const SizedBox(
                                                            width: 20,
                                                            height: 20,
                                                            child:
                                                                CircularProgressIndicator(
                                                                    color: Colors
                                                                        .white,
                                                                    strokeWidth:
                                                                        2),
                                                          )
                                                        : const Text(
                                                            'Join Waitlist',
                                                            style: TextStyle(
                                                                color: Colors
                                                                    .white,
                                                                fontSize: 14,
                                                                fontWeight:
                                                                    FontWeight
                                                                        .w600),
                                                          ),
                                                  ),
                                                ),
                                              ),
                                            ],
                                          ),
                                        ),
                                      ),
                                    );
                                  }
                                  return SliverGrid(
                                    gridDelegate:
                                        const SliverGridDelegateWithFixedCrossAxisCount(
                                      crossAxisCount: 4,
                                      mainAxisSpacing: 10,
                                      crossAxisSpacing: 10,
                                      childAspectRatio: 1.6,
                                    ),
                                    delegate: SliverChildBuilderDelegate(
                                      (context, index) {
                                        final slot = available[index];
                                        final time = _formatTime(
                                            DateTime.parse(slot.startTime));
                                        final isSelected =
                                            _selectedSlot == slot.startTime;
                                        return GestureDetector(
                                          onTap: () => setState(
                                              () => _selectedSlot = slot.startTime),
                                          child: AnimatedContainer(
                                            duration: 200.ms,
                                            decoration: BoxDecoration(
                                              borderRadius: BorderRadius.circular(
                                                  AppTheme.radiusMd),
                                              gradient: isSelected
                                                  ? LinearGradient(
                                                      colors: [
                                                        AppTheme.indigoLuxury,
                                                        const Color(0xFF7C3AED),
                                                      ],
                                                      begin: Alignment.topLeft,
                                                      end: Alignment.bottomRight,
                                                    )
                                                  : null,
                                              color: isSelected
                                                  ? null
                                                  : (isDark
                                                      ? AppTheme.glassDark
                                                      : AppTheme.glassLight),
                                              border: Border.all(
                                                color: isSelected
                                                    ? Colors.transparent
                                                    : (isDark
                                                        ? AppTheme.glassStrokeDark
                                                        : AppTheme
                                                            .glassStrokeLight),
                                              ),
                                            ),
                                            child: Center(
                                              child: Text(
                                                time,
                                                style: TextStyle(
                                                  fontSize: 13,
                                                  fontWeight: isSelected
                                                      ? FontWeight.w700
                                                      : FontWeight.w500,
                                                  color: isSelected
                                                      ? Colors.white
                                                      : colorScheme.onSurface,
                                                ),
                                              ),
                                            ),
                                          ),
                                        ).animate().fadeIn(
                                            duration: 300.ms,
                                            delay: (450 + (index % 8) * 40).ms);
                                      },
                                      childCount: available.length,
                                    ),
                                  );
                                },
                              ),
                      ),

                      // Make this recurring
                      SliverToBoxAdapter(
                        child: Padding(
                          padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
                          child: Row(
                            children: [
                              Expanded(
                                child: Text('Make this recurring',
                                    style: theme.textTheme.titleMedium
                                        ?.copyWith(
                                            fontWeight: FontWeight.w700)),
                              ),
                              Switch(
                                value: _isRecurring,
                                activeTrackColor: AppTheme.indigoLuxury,
                                onChanged: (v) => setState(() => _isRecurring = v),
                              ),
                            ],
                          ),
                        ).animate().fadeIn(duration: 400.ms, delay: 500.ms),
                      ),
                      if (_isRecurring) ...[
                        // Recurrence pattern
                        SliverToBoxAdapter(
                          child: Padding(
                            padding: const EdgeInsets.symmetric(horizontal: 16),
                            child: GlassContainer(
                              borderRadius: AppTheme.radiusLg,
                              padding: const EdgeInsets.all(16),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text('Repeats',
                                      style: theme.textTheme.titleSmall
                                          ?.copyWith(
                                              fontWeight: FontWeight.w600)),
                                  const SizedBox(height: 10),
                                  Row(
                                    children: [
                                      _patternChip(
                                        label: 'Weekly',
                                        selected:
                                            _recurrenceType ==
                                                    RecurrenceTypeValue.weekly &&
                                                _recurrenceInterval == 1,
                                        onTap: () => setState(() {
                                          _recurrenceType =
                                              RecurrenceTypeValue.weekly;
                                          _recurrenceInterval = 1;
                                        }),
                                      ),
                                      const SizedBox(width: 8),
                                      _patternChip(
                                        label: 'Bi-weekly',
                                        selected:
                                            _recurrenceType ==
                                                RecurrenceTypeValue.weekly &&
                                                _recurrenceInterval == 2,
                                        onTap: () => setState(() {
                                          _recurrenceType =
                                              RecurrenceTypeValue.weekly;
                                          _recurrenceInterval = 2;
                                        }),
                                      ),
                                      const SizedBox(width: 8),
                                      _patternChip(
                                        label: 'Monthly',
                                        selected:
                                            _recurrenceType ==
                                                RecurrenceTypeValue.monthly,
                                        onTap: () => setState(() {
                                          _recurrenceType =
                                              RecurrenceTypeValue.monthly;
                                          _recurrenceInterval = 1;
                                        }),
                                      ),
                                    ],
                                  ),
                                  const SizedBox(height: 14),
                                  Text('Ends',
                                      style: theme.textTheme.titleSmall
                                          ?.copyWith(
                                              fontWeight: FontWeight.w600)),
                                  const SizedBox(height: 8),
                                  Row(
                                    children: [
                                      Expanded(
                                        child: _endOptionChip(
                                          label: 'After $_maxOccurrences times',
                                          selected: !_useEndDate,
                                          onTap: () => setState(
                                              () => _useEndDate = false),
                                        ),
                                      ),
                                      const SizedBox(width: 8),
                                      Expanded(
                                        child: _endOptionChip(
                                          label: _seriesEndDate != null
                                              ? 'On date'
                                              : 'End date',
                                          selected: _useEndDate,
                                          onTap: _seriesEndDate == null
                                              ? _pickSeriesEndDate
                                              : () => setState(
                                                  () => _useEndDate = true),
                                        ),
                                      ),
                                    ],
                                  ),
                                  if (!_useEndDate) ...[
                                    const SizedBox(height: 8),
                                    Row(
                                      children: [
                                        _stepButton(
                                          icon: Icons.remove,
                                          onTap: () => setState(() {
                                            if (_maxOccurrences > 2) {
                                              _maxOccurrences -= 1;
                                            }
                                          }),
                                        ),
                                        const SizedBox(width: 12),
                                        Text('$_maxOccurrences',
                                            style: theme.textTheme.titleMedium
                                                ?.copyWith(
                                                    fontWeight:
                                                        FontWeight.w700)),
                                        const SizedBox(width: 12),
                                        _stepButton(
                                          icon: Icons.add,
                                          onTap: () => setState(() {
                                            if (_maxOccurrences < 52) {
                                              _maxOccurrences += 1;
                                            }
                                          }),
                                        ),
                                      ],
                                    ),
                                  ] else if (_seriesEndDate != null) ...[
                                    const SizedBox(height: 8),
                                    Text(
                                      'Series ends ${_formatSeriesDate(_seriesEndDate!)}',
                                      style: theme.textTheme.bodySmall
                                          ?.copyWith(
                                              color: AppTheme.indigoLuxury,
                                              fontWeight: FontWeight.w600),
                                    ),
                                  ],
                                ],
                              ),
                            ),
                          ),
                        ),
                      ],

                      // Notes
                      SliverToBoxAdapter(
                        child: Padding(
                          padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
                          child: Text('Notes (Optional)',
                              style: theme.textTheme.titleMedium?.copyWith(
                                  fontWeight: FontWeight.w700)),
                        ).animate().fadeIn(duration: 400.ms, delay: 500.ms),
                      ),
                      SliverToBoxAdapter(
                        child: Padding(
                          padding: const EdgeInsets.symmetric(horizontal: 16),
                          child: GlassContainer(
                            borderRadius: AppTheme.radiusLg,
                            padding: const EdgeInsets.symmetric(
                                horizontal: 16, vertical: 4),
                            child: TextField(
                              controller: _notesController,
                              maxLines: 3,
                              decoration: InputDecoration(
                                hintText: 'Any special requests...',
                                hintStyle:
                                    TextStyle(color: colorScheme.onSurfaceVariant),
                                border: InputBorder.none,
                                enabledBorder: InputBorder.none,
                                focusedBorder: InputBorder.none,
                              ),
                              style: theme.textTheme.bodyMedium,
                            ),
                          ),
                        ).animate().fadeIn(duration: 400.ms, delay: 550.ms),
                      ),

                      const SliverToBoxAdapter(child: SizedBox(height: 100)),
                    ],
                  ),
                ),

                // Sticky Continue button
                Container(
                  padding: const EdgeInsets.fromLTRB(16, 12, 16, 24),
                  decoration: BoxDecoration(
                    gradient: LinearGradient(
                      begin: Alignment.topCenter,
                      end: Alignment.bottomCenter,
                      colors: [
                        isDark
                            ? AppTheme.slate900.withValues(alpha: 0)
                            : AppTheme.slate50.withValues(alpha: 0),
                        isDark ? AppTheme.slate900 : AppTheme.slate50,
                      ],
                    ),
                  ),
                  child: SafeArea(
                    top: false,
                    child: SizedBox(
                      width: double.infinity,
                      height: 56,
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
                          boxShadow: AppTheme.indigoGlowShadow,
                        ),
                        child: MaterialButton(
                          onPressed: _selectedSlot == null ||
                                  effectiveProviderId == null
                              ? null
                              : () => _goToCheckout(
                                    business,
                                    effectiveServiceId,
                                    effectiveProviderId,
                                  ),
                          shape: RoundedRectangleBorder(
                              borderRadius:
                                  BorderRadius.circular(AppTheme.radiusFull)),
                          child: const Text(
                            'Continue to Checkout',
                            style: TextStyle(
                                color: Colors.white,
                                fontSize: 16,
                                fontWeight: FontWeight.w600),
                          ),
                        ),
                      ),
                    ),
                  ),
                ),
              ],
            );
          },
        ),
      ),
    );
  }

  Future<void> _pickSeriesEndDate() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: _selectedDate.add(const Duration(days: 30)),
      firstDate: _selectedDate,
      lastDate: DateTime.now().add(const Duration(days: 365)),
    );
    if (picked != null) {
      setState(() {
        _seriesEndDate = picked;
        _useEndDate = true;
      });
    }
  }

  String _formatSeriesDate(DateTime d) {
    const months = [
      'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
      'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec',
    ];
    return '${d.day} ${months[d.month - 1]} ${d.year}';
  }

  String _formatTimeOfDay(TimeOfDay t) {
    final period = t.hour >= 12 ? 'PM' : 'AM';
    final hr = t.hour % 12 == 0 ? 12 : t.hour % 12;
    return '$hr:${t.minute.toString().padLeft(2, '0')} $period';
  }

  Widget _patternChip({
    required String label,
    required bool selected,
    required VoidCallback onTap,
  }) {
    return GestureDetector(
      onTap: onTap,
      child: AnimatedContainer(
        duration: 200.ms,
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 7),
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(AppTheme.radiusFull),
          gradient: selected
              ? LinearGradient(
                  colors: [AppTheme.indigoLuxury, const Color(0xFF7C3AED)],
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                )
              : null,
          color: selected ? null : AppTheme.indigoLuxury.withValues(alpha: 0.1),
        ),
        child: Text(
          label,
          style: TextStyle(
            color: selected ? Colors.white : AppTheme.indigoLuxury,
            fontSize: 12,
            fontWeight: FontWeight.w600,
          ),
        ),
      ),
    );
  }

  Widget _endOptionChip({
    required String label,
    required bool selected,
    required VoidCallback onTap,
  }) {
    return GestureDetector(
      onTap: onTap,
      child: AnimatedContainer(
        duration: 200.ms,
        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(AppTheme.radiusMd),
          border: Border.all(
            color: selected ? AppTheme.indigoLuxury : AppTheme.glassStrokeDark,
            width: selected ? 2 : 1,
          ),
          color: selected
              ? AppTheme.indigoLuxury.withValues(alpha: 0.1)
              : Colors.transparent,
        ),
        child: Text(
          label,
          textAlign: TextAlign.center,
          style: TextStyle(
            color: selected ? AppTheme.indigoLuxury : null,
            fontSize: 12,
            fontWeight: selected ? FontWeight.w600 : FontWeight.w500,
          ),
        ),
      ),
    );
  }

  Widget _stepButton({required IconData icon, required VoidCallback onTap}) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: 34,
        height: 34,
        decoration: BoxDecoration(
          color: AppTheme.indigoLuxury.withValues(alpha: 0.1),
          borderRadius: BorderRadius.circular(10),
        ),
        child: Icon(icon, size: 18, color: AppTheme.indigoLuxury),
      ),
    );
  }

  String _formatTime(DateTime dt) {
    final h = dt.hour.toString().padLeft(2, '0');
    final m = dt.minute.toString().padLeft(2, '0');
    return '$h:$m';
  }

  Widget _radioDot(bool isSelected, ColorScheme colorScheme) {
    return Container(
      width: 22,
      height: 22,
      decoration: BoxDecoration(
        shape: BoxShape.circle,
        border: Border.all(
          color: isSelected ? AppTheme.indigoLuxury : colorScheme.onSurfaceVariant,
          width: 2,
        ),
        color: isSelected ? AppTheme.indigoLuxury : Colors.transparent,
      ),
      child: isSelected
          ? const Icon(Icons.check, color: Colors.white, size: 14)
          : null,
    );
  }
}
