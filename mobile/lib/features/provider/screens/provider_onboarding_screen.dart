import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../../../core/theme/app_theme.dart';
import '../../categories/providers/categories_provider.dart';
import '../providers/onboarding_provider.dart';

/// Multi-step provider onboarding wizard.
/// Steps: Business info → Category & Address → Hours → Services → Staff.
class ProviderOnboardingScreen extends ConsumerStatefulWidget {
  const ProviderOnboardingScreen({super.key});

  @override
  ConsumerState<ProviderOnboardingScreen> createState() =>
      _ProviderOnboardingScreenState();
}

class _ProviderOnboardingScreenState
    extends ConsumerState<ProviderOnboardingScreen> {
  final _pageController = PageController();
  int _currentStep = 0;
  bool _submitting = false;

  // Step 1 — Business info
  final _nameController = TextEditingController();
  final _descriptionController = TextEditingController();
  final _emailController = TextEditingController();
  final _phoneController = TextEditingController();
  final _websiteController = TextEditingController();
  final _cancellationController = TextEditingController();
  final _coverImageController = TextEditingController();

  // Step 2 — Category & address
  final Set<String> _selectedCategoryIds = {};
  final _addressLine1Controller = TextEditingController();
  final _addressLine2Controller = TextEditingController();
  final _cityController = TextEditingController();
  final _stateController = TextEditingController();
  final _postalController = TextEditingController();
  final _countryController = TextEditingController(text: 'US');

  // Step 3 — Hours
  final List<DayHoursInput> _hours = List.generate(7, (i) {
    final day = (i + 1) % 7; // Mon..Sun -> System.DayOfWeek 1..6,0
    return DayHoursInput(
      dayOfWeek: day,
      openTime: '09:00',
      closeTime: '17:00',
      isClosed: false,
    );
  });

  // Step 4 — Services
  final List<ServiceDraft> _services = [];
  final _serviceNameController = TextEditingController();
  final _serviceDescController = TextEditingController();
  final _serviceDurationController = TextEditingController();
  final _servicePriceController = TextEditingController();

  // Step 5 — Staff
  final List<ProviderDraft> _providers = [];
  final _staffFirstController = TextEditingController();
  final _staffLastController = TextEditingController();
  final _staffEmailController = TextEditingController();
  final _staffTitleController = TextEditingController();
  final _staffBioController = TextEditingController();

  static const _stepTitles = [
    'Business Info',
    'Category & Location',
    'Opening Hours',
    'Your Services',
    'Your Team',
  ];

  static const _stepIcons = [
    Icons.storefront_outlined,
    Icons.category_outlined,
    Icons.schedule_outlined,
    Icons.content_cut_outlined,
    Icons.group_outlined,
  ];

  @override
  void dispose() {
    _pageController.dispose();
    _nameController.dispose();
    _descriptionController.dispose();
    _emailController.dispose();
    _phoneController.dispose();
    _websiteController.dispose();
    _cancellationController.dispose();
    _coverImageController.dispose();
    _addressLine1Controller.dispose();
    _addressLine2Controller.dispose();
    _cityController.dispose();
    _stateController.dispose();
    _postalController.dispose();
    _countryController.dispose();
    _serviceNameController.dispose();
    _serviceDescController.dispose();
    _serviceDurationController.dispose();
    _servicePriceController.dispose();
    _staffFirstController.dispose();
    _staffLastController.dispose();
    _staffEmailController.dispose();
    _staffTitleController.dispose();
    _staffBioController.dispose();
    super.dispose();
  }

  bool get _canProceed {
    switch (_currentStep) {
      case 0:
        return _nameController.text.trim().isNotEmpty &&
            _emailController.text.trim().contains('@');
      case 1:
        return _selectedCategoryIds.isNotEmpty &&
            _addressLine1Controller.text.trim().isNotEmpty &&
            _cityController.text.trim().isNotEmpty &&
            _postalController.text.trim().isNotEmpty;
      case 2:
        return true; // defaults are valid
      case 3:
        return _services.isNotEmpty;
      case 4:
        return _providers.isNotEmpty;
      default:
        return false;
    }
  }

  Future<void> _next() async {
    if (!_canProceed) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please complete this step first.')),
      );
      return;
    }
    if (_currentStep == _stepTitles.length - 1) {
      await _submit();
      return;
    }
    setState(() => _currentStep++);
    _pageController.animateToPage(
      _currentStep,
      duration: 350.ms,
      curve: Curves.easeOutCubic,
    );
  }

  void _back() {
    if (_currentStep == 0) {
      context.pop();
      return;
    }
    setState(() => _currentStep--);
    _pageController.animateToPage(
      _currentStep,
      duration: 300.ms,
      curve: Curves.easeOutCubic,
    );
  }

  Future<void> _submit() async {
    setState(() => _submitting = true);
    try {
      final onboarding = ref.read(onboardingApiProvider);
      await onboarding.runOnboarding(
        name: _nameController.text.trim(),
        description: _descriptionController.text.trim(),
        email: _emailController.text.trim(),
        phoneNumber: _phoneController.text.trim(),
        website: _websiteController.text.trim(),
        addressLine1: _addressLine1Controller.text.trim(),
        addressLine2: _addressLine2Controller.text.trim().isEmpty
            ? null
            : _addressLine2Controller.text.trim(),
        city: _cityController.text.trim(),
        state: _stateController.text.trim().isEmpty
            ? null
            : _stateController.text.trim(),
        postalCode: _postalController.text.trim(),
        country: _countryController.text.trim(),
        timeZone: 'UTC',
        currency: 'USD',
        cancellationPolicy: _cancellationController.text.trim().isEmpty
            ? null
            : _cancellationController.text.trim(),
        coverImageUrl: _coverImageController.text.trim().isEmpty
            ? null
            : _coverImageController.text.trim(),
        categoryIds: _selectedCategoryIds.toList(),
        hours: _hours,
        services: _services,
        providers: _providers,
      );

      if (!mounted) return;
      context.go('/my-business');
    } catch (e) {
      if (!mounted) return;
      setState(() => _submitting = false);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Submission failed: $e')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isDark = theme.brightness == Brightness.dark;
    final colorScheme = theme.colorScheme;

    return GradientBackground(
      child: Scaffold(
        backgroundColor: Colors.transparent,
        appBar: AppBar(
          backgroundColor: Colors.transparent,
          title: const Text('List Your Business'),
          leading: IconButton(
            icon: const Icon(Icons.close_rounded),
            onPressed: () => context.go('/'),
          ),
        ),
        body: Column(
          children: [
            // Step progress indicator
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 8, 20, 8),
              child: Row(
                children: List.generate(_stepTitles.length, (i) {
                  final isActive = i <= _currentStep;
                  return Expanded(
                    child: Container(
                      height: 6,
                      margin: const EdgeInsets.symmetric(horizontal: 2),
                      decoration: BoxDecoration(
                        borderRadius: BorderRadius.circular(3),
                        color: isActive
                            ? AppTheme.indigoLuxury
                            : (isDark
                                ? AppTheme.slate700
                                : AppTheme.slate200),
                      ),
                    ),
                  );
                }),
              ),
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 4, 20, 8),
              child: Row(
                children: [
                  Container(
                    width: 36,
                    height: 36,
                    decoration: BoxDecoration(
                      gradient: const LinearGradient(
                        colors: [AppTheme.indigoLuxury, Color(0xFF7C3AED)],
                      ),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Icon(_stepIcons[_currentStep],
                        color: Colors.white, size: 20),
                  ),
                  const SizedBox(width: 12),
                  Text(
                    'Step ${_currentStep + 1} of ${_stepTitles.length} — '
                    '${_stepTitles[_currentStep]}',
                    style: theme.textTheme.titleSmall?.copyWith(
                      fontWeight: FontWeight.w700,
                      color: colorScheme.onSurface,
                    ),
                  ),
                ],
              ),
            ),
            Expanded(
              child: PageView(
                controller: _pageController,
                physics: const NeverScrollableScrollPhysics(),
                onPageChanged: (i) => setState(() => _currentStep = i),
                children: [
                  _buildBusinessInfoStep(theme, isDark),
                  _buildCategoryAddressStep(theme, isDark),
                  _buildHoursStep(theme, isDark),
                  _buildServicesStep(theme, isDark),
                  _buildStaffStep(theme, isDark),
                ],
              ),
            ),
            // Bottom actions
            SafeArea(
              top: false,
              child: Padding(
                padding: const EdgeInsets.fromLTRB(20, 8, 20, 16),
                child: Row(
                  children: [
                    if (_currentStep > 0)
                      Expanded(
                        child: OutlinedButton(
                          onPressed: _submitting ? null : _back,
                          child: const Text('Back'),
                        ),
                      ),
                    if (_currentStep > 0) const SizedBox(width: 12),
                    Expanded(
                      flex: 2,
                      child: FilledButton(
                        onPressed: _submitting ? null : _next,
                        child: _submitting
                            ? const SizedBox(
                                height: 20,
                                width: 20,
                                child: CircularProgressIndicator(
                                  strokeWidth: 2,
                                  color: Colors.white,
                                ),
                              )
                            : Text(
                                _currentStep == _stepTitles.length - 1
                                    ? 'Submit for Review'
                                    : 'Continue',
                              ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  // ── Step 1: Business info ────────────────────────────
  Widget _buildBusinessInfoStep(ThemeData theme, bool isDark) {
    return SingleChildScrollView(
      padding: const EdgeInsets.fromLTRB(20, 8, 20, 20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _stepHeading(theme, 'Tell us about your business'),
          const SizedBox(height: 16),
          _field(
            controller: _nameController,
            label: 'Business Name *',
            icon: Icons.storefront_outlined,
            onChanged: (_) => setState(() {}),
          ),
          const SizedBox(height: 12),
          _field(
            controller: _descriptionController,
            label: 'Description',
            icon: Icons.notes_outlined,
            maxLines: 3,
          ),
          const SizedBox(height: 12),
          _field(
            controller: _emailController,
            label: 'Contact Email *',
            icon: Icons.email_outlined,
            keyboardType: TextInputType.emailAddress,
            onChanged: (_) => setState(() {}),
          ),
          const SizedBox(height: 12),
          _field(
            controller: _phoneController,
            label: 'Phone Number',
            icon: Icons.phone_outlined,
            keyboardType: TextInputType.phone,
          ),
          const SizedBox(height: 12),
          _field(
            controller: _websiteController,
            label: 'Website',
            icon: Icons.language_outlined,
          ),
          const SizedBox(height: 12),
          _field(
            controller: _coverImageController,
            label: 'Cover image URL (optional)',
            icon: Icons.image_outlined,
          ),
          const SizedBox(height: 12),
          _field(
            controller: _cancellationController,
            label: 'Cancellation Policy',
            icon: Icons.event_busy_outlined,
            maxLines: 2,
          ),
        ],
      ),
    );
  }

  // ── Step 2: Category & address ───────────────────────
  Widget _buildCategoryAddressStep(ThemeData theme, bool isDark) {
    final colorScheme = theme.colorScheme;
    final categoriesAsync = ref.watch(categoriesProvider);
    return SingleChildScrollView(
      padding: const EdgeInsets.fromLTRB(20, 8, 20, 20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _stepHeading(theme, 'Choose a category & location'),
          const SizedBox(height: 12),
          Text(
            'Select one or more categories that fit your business.',
            style: theme.textTheme.bodySmall?.copyWith(
              color: colorScheme.onSurfaceVariant,
            ),
          ),
          const SizedBox(height: 12),
          categoriesAsync.when(
            loading: () => const Padding(
              padding: EdgeInsets.all(24),
              child: Center(child: CircularProgressIndicator()),
            ),
            error: (err, _) => Padding(
              padding: const EdgeInsets.all(12),
              child: Text('Could not load categories: $err'),
            ),
            data: (categories) => Wrap(
              spacing: 8,
              runSpacing: 8,
              children: categories.map((cat) {
                final isSelected = _selectedCategoryIds.contains(cat.id);
                return FilterChip(
                  label: Text(cat.name),
                  selected: isSelected,
                  onSelected: (sel) => setState(() {
                    if (sel) {
                      _selectedCategoryIds.add(cat.id);
                    } else {
                      _selectedCategoryIds.remove(cat.id);
                    }
                  }),
                );
              }).toList(),
            ),
          ),
          const SizedBox(height: 20),
          _field(
            controller: _addressLine1Controller,
            label: 'Address Line 1 *',
            icon: Icons.home_outlined,
            onChanged: (_) => setState(() {}),
          ),
          const SizedBox(height: 12),
          _field(
            controller: _addressLine2Controller,
            label: 'Address Line 2 (optional)',
            icon: Icons.home_work_outlined,
          ),
          const SizedBox(height: 12),
          Row(
            children: [
              Expanded(
                child: _field(
                  controller: _cityController,
                  label: 'City *',
                  icon: Icons.location_city_outlined,
                  onChanged: (_) => setState(() {}),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: _field(
                  controller: _stateController,
                  label: 'State',
                  icon: Icons.map_outlined,
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Row(
            children: [
              Expanded(
                child: _field(
                  controller: _postalController,
                  label: 'Postal Code *',
                  icon: Icons.pin_drop_outlined,
                  onChanged: (_) => setState(() {}),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: _field(
                  controller: _countryController,
                  label: 'Country *',
                  icon: Icons.public_outlined,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  // ── Step 3: Hours ────────────────────────────────────
  Widget _buildHoursStep(ThemeData theme, bool isDark) {
    final colorScheme = theme.colorScheme;
    final dayNames = [
      'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday',
      'Saturday', 'Sunday',
    ];
    return ListView.builder(
      padding: const EdgeInsets.fromLTRB(20, 8, 20, 20),
      itemCount: 7,
      itemBuilder: (context, index) {
        final hours = _hours[index];
        return Card(
          margin: const EdgeInsets.only(bottom: 10),
          child: Padding(
            padding: const EdgeInsets.all(14),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Expanded(
                      child: Text(
                        dayNames[index],
                        style: theme.textTheme.titleSmall
                            ?.copyWith(fontWeight: FontWeight.w700),
                      ),
                    ),
                    Switch(
                      value: !hours.isClosed,
                      onChanged: (v) => setState(() {
                        _hours[index] = DayHoursInput(
                          dayOfWeek: hours.dayOfWeek,
                          openTime: hours.openTime,
                          closeTime: hours.closeTime,
                          isClosed: !v,
                        );
                      }),
                    ),
                    Text(
                      hours.isClosed ? 'Closed' : 'Open',
                      style: theme.textTheme.bodySmall?.copyWith(
                        color: hours.isClosed
                            ? colorScheme.error
                            : AppTheme.success,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ],
                ),
                if (!hours.isClosed) ...[
                  const SizedBox(height: 8),
                  Row(
                    children: [
                      Expanded(
                        child: _timePicker(
                          label: 'Opens',
                          time: hours.openTime,
                          onPick: (t) => setState(() {
                            _hours[index] = DayHoursInput(
                              dayOfWeek: hours.dayOfWeek,
                              openTime: t,
                              closeTime: hours.closeTime,
                              isClosed: false,
                            );
                          }),
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: _timePicker(
                          label: 'Closes',
                          time: hours.closeTime,
                          onPick: (t) => setState(() {
                            _hours[index] = DayHoursInput(
                              dayOfWeek: hours.dayOfWeek,
                              openTime: hours.openTime,
                              closeTime: t,
                              isClosed: false,
                            );
                          }),
                        ),
                      ),
                    ],
                  ),
                ],
              ],
            ),
          ),
        );
      },
    );
  }

  Widget _timePicker({
    required String label,
    required String time,
    required ValueChanged<String> onPick,
  }) {
    return InkWell(
      onTap: () async {
        final parts = time.split(':');
        final picked = await showTimePicker(
          context: context,
          initialTime: TimeOfDay(
            hour: int.parse(parts[0]),
            minute: int.parse(parts[1]),
          ),
        );
        if (picked != null) {
          onPick('${picked.hour.toString().padLeft(2, '0')}:'
              '${picked.minute.toString().padLeft(2, '0')}');
        }
      },
      child: InputDecorator(
        decoration: InputDecoration(
          labelText: label,
          prefixIcon: const Icon(Icons.access_time, size: 20),
        ),
        child: Text(time, style: const TextStyle(fontWeight: FontWeight.w600)),
      ),
    );
  }

  // ── Step 4: Services ─────────────────────────────────
  Widget _buildServicesStep(ThemeData theme, bool isDark) {
    final colorScheme = theme.colorScheme;
    return Column(
      children: [
        Expanded(
          child: ListView(
            padding: const EdgeInsets.fromLTRB(20, 8, 20, 8),
            children: [
              _stepHeading(theme, 'Add the services you offer'),
              const SizedBox(height: 8),
              Text(
                'At least one service is required. ${_services.length} added.',
                style: theme.textTheme.bodySmall?.copyWith(
                  color: colorScheme.onSurfaceVariant,
                ),
              ),
              const SizedBox(height: 12),
              if (_services.isEmpty)
                Padding(
                  padding: const EdgeInsets.all(32),
                  child: Center(
                    child: Text(
                      'Tap "Add Service" to create your first one.',
                      style: theme.textTheme.bodyMedium?.copyWith(
                        color: colorScheme.onSurfaceVariant,
                      ),
                    ),
                  ),
                )
              else
                ..._services.asMap().entries.map((entry) {
                  final s = entry.value;
                  return Card(
                    margin: const EdgeInsets.only(bottom: 10),
                    child: ListTile(
                      leading: Container(
                        width: 42,
                        height: 42,
                        decoration: BoxDecoration(
                          color: AppTheme.indigoLuxury.withValues(alpha: 0.12),
                          borderRadius: BorderRadius.circular(12),
                        ),
                        child: Icon(Icons.content_cut,
                            color: AppTheme.indigoLuxury, size: 20),
                      ),
                      title: Text(s.name,
                          style: theme.textTheme.titleSmall
                              ?.copyWith(fontWeight: FontWeight.w700)),
                      subtitle: Text(
                        '${s.durationMinutes} min · \$${s.priceAmount.toStringAsFixed(2)}',
                        style: theme.textTheme.bodySmall,
                      ),
                      trailing: IconButton(
                        icon: const Icon(Icons.delete_outline,
                            color: Colors.red),
                        onPressed: () => setState(() => _services.removeAt(entry.key)),
                      ),
                    ),
                  );
                }),
              const SizedBox(height: 12),
              // Add service form
              GlassContainer(
                borderRadius: AppTheme.radiusLg,
                padding: const EdgeInsets.all(16),
                child: Column(
                  children: [
                    _field(
                      controller: _serviceNameController,
                      label: 'Service name (e.g. Swedish Massage 60min)',
                      icon: Icons.content_cut_outlined,
                    ),
                    const SizedBox(height: 10),
                    _field(
                      controller: _serviceDescController,
                      label: 'Description (optional)',
                      icon: Icons.notes_outlined,
                      maxLines: 2,
                    ),
                    const SizedBox(height: 10),
                    Row(
                      children: [
                        Expanded(
                          child: _field(
                            controller: _serviceDurationController,
                            label: 'Duration (min)',
                            icon: Icons.timer_outlined,
                            keyboardType: TextInputType.number,
                          ),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: _field(
                            controller: _servicePriceController,
                            label: 'Price (USD)',
                            icon: Icons.attach_money_outlined,
                            keyboardType: TextInputType.number,
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 12),
                    SizedBox(
                      width: double.infinity,
                      child: FilledButton.tonal(
                        onPressed: _addService,
                        child: const Text('Add Service'),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }

  void _addService() {
    final name = _serviceNameController.text.trim();
    final dur = int.tryParse(_serviceDurationController.text.trim());
    final price = double.tryParse(_servicePriceController.text.trim());
    if (name.isEmpty || dur == null || dur <= 0 || price == null || price < 0) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text(
              'Please fill name, a positive duration and a valid price.'),
        ),
      );
      return;
    }
    setState(() {
      _services.add(ServiceDraft(
        name: name,
        description: _serviceDescController.text.trim().isEmpty
            ? null
            : _serviceDescController.text.trim(),
        durationMinutes: dur,
        priceAmount: price,
      ));
      _serviceNameController.clear();
      _serviceDescController.clear();
      _serviceDurationController.clear();
      _servicePriceController.clear();
    });
  }

  // ── Step 5: Staff ────────────────────────────────────
  Widget _buildStaffStep(ThemeData theme, bool isDark) {
    final colorScheme = theme.colorScheme;
    return Column(
      children: [
        Expanded(
          child: ListView(
            padding: const EdgeInsets.fromLTRB(20, 8, 20, 8),
            children: [
              _stepHeading(theme, 'Add your team members'),
              const SizedBox(height: 8),
              Text(
                'Staff get their own provider account. '
                '${_providers.length} added.',
                style: theme.textTheme.bodySmall?.copyWith(
                  color: colorScheme.onSurfaceVariant,
                ),
              ),
              const SizedBox(height: 12),
              if (_providers.isEmpty)
                Padding(
                  padding: const EdgeInsets.all(32),
                  child: Center(
                    child: Text(
                      'Add at least one provider so customers can book with someone.',
                      style: theme.textTheme.bodyMedium?.copyWith(
                        color: colorScheme.onSurfaceVariant,
                      ),
                      textAlign: TextAlign.center,
                    ),
                  ),
                )
              else
                ..._providers.asMap().entries.map((entry) {
                  final p = entry.value;
                  return Card(
                    margin: const EdgeInsets.only(bottom: 10),
                    child: ListTile(
                      leading: const CircleAvatar(
                        child: Icon(Icons.person_outline),
                      ),
                      title: Text('${p.firstName} ${p.lastName}',
                          style: theme.textTheme.titleSmall
                              ?.copyWith(fontWeight: FontWeight.w700)),
                      subtitle: Text(
                        '${p.title ?? 'Provider'} · ${p.email}',
                        style: theme.textTheme.bodySmall,
                      ),
                      trailing: IconButton(
                        icon: const Icon(Icons.delete_outline,
                            color: Colors.red),
                        onPressed: () =>
                            setState(() => _providers.removeAt(entry.key)),
                      ),
                    ),
                  );
                }),
              const SizedBox(height: 12),
              // Add provider form
              GlassContainer(
                borderRadius: AppTheme.radiusLg,
                padding: const EdgeInsets.all(16),
                child: Column(
                  children: [
                    Row(
                      children: [
                        Expanded(
                          child: _field(
                            controller: _staffFirstController,
                            label: 'First Name *',
                            icon: Icons.person_outline,
                          ),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: _field(
                            controller: _staffLastController,
                            label: 'Last Name *',
                            icon: Icons.person_outline,
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 10),
                    _field(
                      controller: _staffEmailController,
                      label: 'Work Email *',
                      icon: Icons.email_outlined,
                      keyboardType: TextInputType.emailAddress,
                    ),
                    const SizedBox(height: 10),
                    _field(
                      controller: _staffTitleController,
                      label: 'Role / Title (e.g. Senior Stylist)',
                      icon: Icons.work_outline,
                    ),
                    const SizedBox(height: 10),
                    _field(
                      controller: _staffBioController,
                      label: 'Short bio (optional)',
                      icon: Icons.notes_outlined,
                      maxLines: 2,
                    ),
                    const SizedBox(height: 12),
                    SizedBox(
                      width: double.infinity,
                      child: FilledButton.tonal(
                        onPressed: _addProvider,
                        child: const Text('Add Team Member'),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }

  void _addProvider() {
    final first = _staffFirstController.text.trim();
    final last = _staffLastController.text.trim();
    final email = _staffEmailController.text.trim();
    if (first.isEmpty || last.isEmpty || !email.contains('@')) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Please fill first/last name and a valid email.'),
        ),
      );
      return;
    }
    setState(() {
      _providers.add(ProviderDraft(
        firstName: first,
        lastName: last,
        email: email,
        title: _staffTitleController.text.trim().isEmpty
            ? null
            : _staffTitleController.text.trim(),
        bio: _staffBioController.text.trim().isEmpty
            ? null
            : _staffBioController.text.trim(),
      ));
      _staffFirstController.clear();
      _staffLastController.clear();
      _staffEmailController.clear();
      _staffTitleController.clear();
      _staffBioController.clear();
    });
  }

  // ── Shared helpers ───────────────────────────────────
  Widget _stepHeading(ThemeData theme, String title) {
    final colorScheme = theme.colorScheme;
    return Text(
      title,
      style: theme.textTheme.titleLarge?.copyWith(
        fontWeight: FontWeight.w700,
        color: colorScheme.onSurface,
      ),
    );
  }

  Widget _field({
    required TextEditingController controller,
    required String label,
    required IconData icon,
    int maxLines = 1,
    TextInputType? keyboardType,
    ValueChanged<String>? onChanged,
  }) {
    return TextFormField(
      controller: controller,
      maxLines: maxLines,
      keyboardType: keyboardType,
      onChanged: onChanged,
      decoration: InputDecoration(
        labelText: label,
        prefixIcon: Icon(icon, size: 20),
        alignLabelWithHint: maxLines > 1,
      ),
    );
  }
}
