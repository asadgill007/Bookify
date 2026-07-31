import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../../../core/theme/app_theme.dart';

/// Premium Booking screen with glassmorphism calendar/time-slot selection.
class BookingScreen extends ConsumerStatefulWidget {
  final String businessId;
  final String serviceId;

  const BookingScreen({
    super.key,
    required this.businessId,
    required this.serviceId,
  });

  @override
  ConsumerState<BookingScreen> createState() => _BookingScreenState();
}

class _BookingScreenState extends ConsumerState<BookingScreen> {
  DateTime _selectedDate = DateTime.now().add(const Duration(days: 1));
  String? _selectedTime;
  int _selectedProvider = 0;
  int _selectedService = 0;
  final _notesController = TextEditingController();

  final List<DateTime> _dates = List.generate(
    14,
    (i) => DateTime.now().add(Duration(days: i + 1)),
  );

  final List<String> _timeSlots = [
    '09:00', '09:30', '10:00', '10:30', '11:00', '11:30',
    '12:00', '12:30', '13:00', '13:30', '14:00', '14:30',
    '15:00', '15:30', '16:00', '16:30', '17:00',
  ];

  final List<String> _bookedSlots = ['09:00', '11:00', '14:00', '16:30'];

  final List<Map<String, dynamic>> _providers = [
    {'name': 'Sophia C.', 'specialty': 'Senior Stylist', 'avatar': 'https://i.pravatar.cc/150?u=sophia'},
    {'name': 'Leila A.', 'specialty': 'Color Specialist', 'avatar': 'https://i.pravatar.cc/150?u=leila'},
    {'name': 'Thomas A.', 'specialty': 'Junior Stylist', 'avatar': 'https://i.pravatar.cc/150?u=thomas'},
  ];

  final List<Map<String, dynamic>> _services = [
    {'name': "Women's Haircut & Style", 'duration': '45 min', 'price': 65.0},
    {'name': "Men's Haircut", 'duration': '30 min', 'price': 35.0},
    {'name': 'Balayage', 'duration': '120 min', 'price': 180.0},
    {'name': 'Blowout & Style', 'duration': '45 min', 'price': 55.0},
  ];

  @override
  void dispose() {
    _notesController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final isDark = theme.brightness == Brightness.dark;

    return GradientBackground(
      child: Scaffold(
        backgroundColor: Colors.transparent,
        appBar: AppBar(
          title: const Text('Book Appointment'),
          backgroundColor: Colors.transparent,
        ),
        body: Column(
          children: [
            Expanded(
              child: CustomScrollView(
                physics: const BouncingScrollPhysics(),
                slivers: [
                  // ── Service Selection ──
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(16, 8, 16, 8),
                      child: Text('Select Service', style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700)),
                    ).animate().fadeIn(duration: 400.ms),
                  ),
                  SliverList(
                    delegate: SliverChildBuilderDelegate(
                      (context, index) {
                        final service = _services[index];
                        final isSelected = _selectedService == index;
                        return Padding(
                          padding: const EdgeInsets.only(left: 16, right: 16, bottom: 10),
                          child: GestureDetector(
                            onTap: () => setState(() => _selectedService = index),
                            child: GlassContainer(
                              borderRadius: AppTheme.radiusLg,
                              padding: const EdgeInsets.all(14),
                              borderSide: isSelected ? BorderSide(color: AppTheme.indigoLuxury, width: 2) : null,
                              child: Row(
                                children: [
                                  Radio<int>(
                                    value: index,
                                    groupValue: _selectedService,
                                    onChanged: (v) => setState(() => _selectedService = v!),
                                    activeColor: AppTheme.indigoLuxury,
                                  ),
                                  Expanded(
                                    child: Column(
                                      crossAxisAlignment: CrossAxisAlignment.start,
                                      children: [
                                        Text(service['name'] as String, style: theme.textTheme.titleSmall?.copyWith(fontWeight: FontWeight.w600)),
                                        Text(service['duration'] as String, style: theme.textTheme.bodySmall?.copyWith(color: colorScheme.onSurfaceVariant)),
                                      ],
                                    ),
                                  ),
                                  Text('\$${service['price']}', style: TextStyle(fontSize: 18, fontWeight: FontWeight.w700, color: AppTheme.indigoLuxury)),
                                ],
                              ),
                            ),
                          ),
                        ).animate().fadeIn(duration: 300.ms, delay: (50 + index * 50).ms);
                      },
                      childCount: _services.length,
                    ),
                  ),

                  // ── Provider Selection ──
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
                      child: Text('Select Provider', style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700)),
                    ).animate().fadeIn(duration: 400.ms, delay: 200.ms),
                  ),
                  SliverToBoxAdapter(
                    child: SizedBox(
                      height: 100,
                      child: ListView.separated(
                        scrollDirection: Axis.horizontal,
                        padding: const EdgeInsets.symmetric(horizontal: 16),
                        itemCount: _providers.length,
                        separatorBuilder: (_, __) => const SizedBox(width: 12),
                        itemBuilder: (context, index) {
                          final prov = _providers[index];
                          final isSelected = _selectedProvider == index;
                          return GestureDetector(
                            onTap: () => setState(() => _selectedProvider = index),
                            child: GlassContainer(
                              width: 100,
                              borderRadius: AppTheme.radiusLg,
                              padding: const EdgeInsets.all(12),
                              borderSide: isSelected ? BorderSide(color: AppTheme.indigoLuxury, width: 2) : null,
                              child: Column(
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: [
                                  CircleAvatar(
                                    radius: 20,
                                    backgroundImage: NetworkImage(prov['avatar'] as String),
                                    backgroundColor: isDark ? AppTheme.slate700 : AppTheme.slate200,
                                  ),
                                  const SizedBox(height: 6),
                                  Text(prov['name'] as String, style: TextStyle(fontSize: 11, fontWeight: FontWeight.w600, color: colorScheme.onSurface), maxLines: 1, overflow: TextOverflow.ellipsis),
                                ],
                              ),
                            ),
                          ).animate().fadeIn(duration: 300.ms, delay: (250 + index * 60).ms);
                        },
                      ),
                    ),
                  ),

                  // ── Date Selection ──
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
                      child: Text('Select Date', style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700)),
                    ).animate().fadeIn(duration: 400.ms, delay: 300.ms),
                  ),
                  SliverToBoxAdapter(
                    child: SizedBox(
                      height: 80,
                      child: ListView.separated(
                        scrollDirection: Axis.horizontal,
                        padding: const EdgeInsets.symmetric(horizontal: 16),
                        itemCount: _dates.length,
                        separatorBuilder: (_, __) => const SizedBox(width: 8),
                        itemBuilder: (context, index) {
                          final date = _dates[index];
                          final isSelected = date.day == _selectedDate.day && date.month == _selectedDate.month;
                          final dayName = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'][date.weekday - 1];

                          return GestureDetector(
                            onTap: () => setState(() => _selectedDate = date),
                            child: AnimatedContainer(
                              duration: 200.ms,
                              width: 60,
                              decoration: BoxDecoration(
                                borderRadius: BorderRadius.circular(AppTheme.radiusLg),
                                gradient: isSelected
                                    ? LinearGradient(colors: [AppTheme.indigoLuxury, const Color(0xFF7C3AED)], begin: Alignment.topLeft, end: Alignment.bottomRight)
                                    : null,
                                color: isSelected ? null : (isDark ? AppTheme.glassDark : AppTheme.glassLight),
                                border: Border.all(
                                  color: isSelected ? Colors.transparent : (isDark ? AppTheme.glassStrokeDark : AppTheme.glassStrokeLight),
                                ),
                                boxShadow: isSelected ? AppTheme.indigoGlowShadow : null,
                              ),
                              child: Column(
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: [
                                  Text(dayName, style: TextStyle(fontSize: 11, fontWeight: FontWeight.w500, color: isSelected ? Colors.white : colorScheme.onSurfaceVariant)),
                                  const SizedBox(height: 4),
                                  Text('${date.day}', style: TextStyle(fontSize: 18, fontWeight: FontWeight.w700, color: isSelected ? Colors.white : colorScheme.onSurface)),
                                  Text('${date.month}/${date.year.toString().substring(2)}', style: TextStyle(fontSize: 9, color: isSelected ? Colors.white70 : colorScheme.onSurfaceVariant)),
                                ],
                              ),
                            ),
                          ).animate().fadeIn(duration: 300.ms, delay: (350 + index * 30).ms);
                        },
                      ),
                    ),
                  ),

                  // ── Time Slot Selection ──
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
                      child: Text('Select Time', style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700)),
                    ).animate().fadeIn(duration: 400.ms, delay: 400.ms),
                  ),
                  SliverPadding(
                    padding: const EdgeInsets.symmetric(horizontal: 16),
                    sliver: SliverGrid(
                      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                        crossAxisCount: 4,
                        mainAxisSpacing: 10,
                        crossAxisSpacing: 10,
                        childAspectRatio: 1.6,
                      ),
                      delegate: SliverChildBuilderDelegate(
                        (context, index) {
                          final time = _timeSlots[index];
                          final isBooked = _bookedSlots.contains(time);
                          final isSelected = _selectedTime == time;

                          return GestureDetector(
                            onTap: isBooked ? null : () => setState(() => _selectedTime = time),
                            child: AnimatedContainer(
                              duration: 200.ms,
                              decoration: BoxDecoration(
                                borderRadius: BorderRadius.circular(AppTheme.radiusMd),
                                gradient: isSelected
                                    ? LinearGradient(colors: [AppTheme.indigoLuxury, const Color(0xFF7C3AED)], begin: Alignment.topLeft, end: Alignment.bottomRight)
                                    : null,
                                color: isBooked
                                    ? (isDark ? AppTheme.slate800 : AppTheme.slate200)
                                    : (isSelected ? null : (isDark ? AppTheme.glassDark : AppTheme.glassLight)),
                                border: Border.all(
                                  color: isBooked
                                      ? (isDark ? AppTheme.slate700 : AppTheme.slate300)
                                      : (isSelected ? Colors.transparent : (isDark ? AppTheme.glassStrokeDark : AppTheme.glassStrokeLight)),
                                ),
                              ),
                              child: Center(
                                child: Text(
                                  time,
                                  style: TextStyle(
                                    fontSize: 13,
                                    fontWeight: isSelected ? FontWeight.w700 : FontWeight.w500,
                                    color: isBooked
                                        ? colorScheme.onSurfaceVariant.withValues(alpha: 0.4)
                                        : isSelected
                                            ? Colors.white
                                            : colorScheme.onSurface,
                                    decoration: isBooked ? TextDecoration.lineThrough : null,
                                  ),
                                ),
                              ),
                            ),
                          ).animate().fadeIn(duration: 300.ms, delay: (450 + (index % 8) * 40).ms);
                        },
                        childCount: _timeSlots.length,
                      ),
                    ),
                  ),

                  // ── Notes ──
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
                      child: Text('Notes (Optional)', style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700)),
                    ).animate().fadeIn(duration: 400.ms, delay: 500.ms),
                  ),
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 16),
                      child: GlassContainer(
                        borderRadius: AppTheme.radiusLg,
                        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
                        child: TextField(
                          controller: _notesController,
                          maxLines: 3,
                          decoration: InputDecoration(
                            hintText: 'Any special requests...',
                            hintStyle: TextStyle(color: colorScheme.onSurfaceVariant),
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

            // ── Sticky Continue Button ──
            Container(
              padding: const EdgeInsets.fromLTRB(16, 12, 16, 24),
              decoration: BoxDecoration(
                gradient: LinearGradient(
                  begin: Alignment.topCenter,
                  end: Alignment.bottomCenter,
                  colors: [
                    isDark ? AppTheme.slate900.withValues(alpha: 0) : AppTheme.slate50.withValues(alpha: 0),
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
                        colors: [AppTheme.indigoLuxury, const Color(0xFF7C3AED)],
                        begin: Alignment.topLeft, end: Alignment.bottomRight,
                      ),
                      borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                      boxShadow: AppTheme.indigoGlowShadow,
                    ),
                    child: MaterialButton(
                      onPressed: _selectedTime == null ? null : () => context.push('/checkout'),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(AppTheme.radiusFull)),
                      child: const Text(
                        'Continue to Payment',
                        style: TextStyle(color: Colors.white, fontSize: 16, fontWeight: FontWeight.w600),
                      ),
                    ),
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}