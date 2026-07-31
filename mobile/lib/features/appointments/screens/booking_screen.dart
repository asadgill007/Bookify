import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../../../core/theme/app_theme.dart';
import '../../business/providers/business_detail_provider.dart';

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
                                    return const SliverToBoxAdapter(
                                      child: Padding(
                                        padding: EdgeInsets.all(24),
                                        child: Center(
                                          child: Text(
                                              'No available slots on this day.'),
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
