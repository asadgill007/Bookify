import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/theme/app_theme.dart';

/// FAQ category + items.
class FaqCategory {
  final String name;
  final IconData icon;
  final List<FaqItem> items;
  const FaqCategory({required this.name, required this.icon, required this.items});
}

class FaqItem {
  final String question;
  final String answer;
  const FaqItem({required this.question, required this.answer});
}

const _faqCategories = [
  FaqCategory(
    name: 'Bookings',
    icon: Icons.calendar_month_outlined,
    items: [
      FaqItem(
        question: 'How do I book an appointment?',
        answer:
            'Browse businesses, open one you like, pick a service and a staff member, choose a time slot, then confirm at checkout. You\'ll see the booking in the Appointments tab.',
      ),
      FaqItem(
        question: 'Can I book on behalf of someone else?',
        answer:
            'Yes — just add their name in the booking notes and use your own account. The appointment will appear in your Appointments tab.',
      ),
      FaqItem(
        question: 'Why is a time slot not available?',
        answer:
            'Slots fill up quickly. Try another day or provider, or join the waitlist from the booking screen — we\'ll notify you if a slot opens up.',
      ),
      FaqItem(
        question: 'What happens after I book?',
        answer:
            'You\'ll get a confirmation with a booking reference, and reminders before your appointment. You can view or manage it in the Appointments tab.',
      ),
    ],
  ),
  FaqCategory(
    name: 'Payments',
    icon: Icons.payments_outlined,
    items: [
      FaqItem(
        question: 'How do payments work?',
        answer:
            'You pay securely when you confirm your booking. Bookify processes the payment and the business receives it after your appointment.',
      ),
      FaqItem(
        question: 'Is my payment information secure?',
        answer:
            'Yes. All payments are processed by trusted payment providers — we never store your full card details.',
      ),
      FaqItem(
        question: 'How do refunds work?',
        answer:
            'Refunds follow each business\'s cancellation policy and are returned to your original payment method within 5–10 business days.',
      ),
    ],
  ),
  FaqCategory(
    name: 'Cancellations',
    icon: Icons.event_busy_outlined,
    items: [
      FaqItem(
        question: 'Can I cancel or reschedule?',
        answer:
            'Yes — open the appointment in the Appointments tab and choose Cancel or Reschedule. Most businesses allow free cancellation up to 24 hours before.',
      ),
      FaqItem(
        question: 'What if I\'m late?',
        answer:
            'Try to let the business know. Late arrivals may result in a shortened service depending on the business\'s policy.',
      ),
    ],
  ),
  FaqCategory(
    name: 'Account',
    icon: Icons.person_outline,
    items: [
      FaqItem(
        question: 'How do I reset my password?',
        answer:
            'On the login screen tap "Forgot Password?" and enter your email. You\'ll receive a reset link to create a new password.',
      ),
      FaqItem(
        question: 'How do I change my profile or preferences?',
        answer:
            'Go to Profile → Edit Profile for your details, and Settings for language, currency, and notifications.',
      ),
      FaqItem(
        question: 'Can I delete my account?',
        answer:
            'Yes — Settings → Account → Delete Account. This permanently removes your account and data.',
      ),
    ],
  ),
  FaqCategory(
    name: 'Providers',
    icon: Icons.storefront_outlined,
    items: [
      FaqItem(
        question: 'How do I list my business?',
        answer:
            'Create an account and choose "List your business", then complete the onboarding steps: category, hours, services, staff and photos.',
      ),
      FaqItem(
        question: 'When does my business go live?',
        answer:
            'The moment your listing is complete (name, description, category, address, contact, hours, a priced service, and a photo) it goes live automatically — no admin approval needed.',
      ),
      FaqItem(
        question: 'How do I manage my bookings?',
        answer:
            'Open My Business from your profile to see your listings. Use the provider tools to manage availability and appointments.',
      ),
    ],
  ),
];

/// Help Center — searchable, categorized FAQ + contact support entry.
class HelpCenterScreen extends ConsumerStatefulWidget {
  const HelpCenterScreen({super.key});

  @override
  ConsumerState<HelpCenterScreen> createState() => _HelpCenterScreenState();
}

class _HelpCenterScreenState extends ConsumerState<HelpCenterScreen> {
  String _query = '';
  bool _showAllCategories = false;

  List<FaqItem> get _filteredItems {
    final q = _query.trim().toLowerCase();
    if (q.isEmpty) return [];
    final results = <FaqItem>[];
    for (final cat in _faqCategories) {
      for (final item in cat.items) {
        if (item.question.toLowerCase().contains(q) ||
            item.answer.toLowerCase().contains(q)) {
          results.add(item);
        }
      }
    }
    return results;
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final searching = _query.trim().isNotEmpty;

    return GradientBackground(
      child: Scaffold(
        backgroundColor: Colors.transparent,
        appBar: AppBar(
          title: const Text('Help Center'),
          backgroundColor: Colors.transparent,
        ),
        body: Column(
          children: [
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 8, 16, 8),
              child: GlassContainer(
                borderRadius: AppTheme.radiusFull,
                padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 2),
                child: TextField(
                  onChanged: (v) => setState(() => _query = v),
                  decoration: InputDecoration(
                    hintText: 'Search for answers...',
                    prefixIcon: Icon(Icons.search, color: AppTheme.indigoLuxury),
                    suffixIcon: searching
                        ? IconButton(
                            icon: const Icon(Icons.clear, size: 18),
                            onPressed: () => setState(() => _query = ''),
                          )
                        : null,
                    border: InputBorder.none,
                    enabledBorder: InputBorder.none,
                    focusedBorder: InputBorder.none,
                    hintStyle: TextStyle(color: colorScheme.onSurfaceVariant),
                  ),
                ),
              ),
            ),
            Expanded(
              child: searching
                  ? _searchResults(theme, colorScheme)
                  : _categoryList(theme, colorScheme),
            ),
          ],
        ),
      ),
    );
  }

  Widget _searchResults(ThemeData theme, ColorScheme colorScheme) {
    final results = _filteredItems;
    if (results.isEmpty) {
      return Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.search_off,
                size: 56, color: colorScheme.onSurfaceVariant.withValues(alpha: 0.4)),
            const SizedBox(height: 12),
            Text('No answers found for "$_query"',
                style: theme.textTheme.bodyMedium
                    ?.copyWith(color: colorScheme.onSurfaceVariant)),
          ],
        ),
      );
    }
    return ListView.builder(
      padding: const EdgeInsets.all(16),
      itemCount: results.length,
      itemBuilder: (context, index) {
        final item = results[index];
        return Card(
          margin: const EdgeInsets.only(bottom: 8),
          child: ExpansionTile(
            title:
                Text(item.question, style: const TextStyle(fontWeight: FontWeight.w600)),
            children: [
              Padding(
                padding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
                child: Text(item.answer,
                    style: theme.textTheme.bodyMedium
                        ?.copyWith(color: colorScheme.onSurfaceVariant, height: 1.5)),
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _categoryList(ThemeData theme, ColorScheme colorScheme) {
    final visible = _showAllCategories ? _faqCategories : _faqCategories.take(3).toList();
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        for (final cat in visible) ...[
          Padding(
            padding: const EdgeInsets.only(left: 4, bottom: 8),
            child: Row(
              children: [
                Icon(cat.icon, size: 18, color: AppTheme.indigoLuxury),
                const SizedBox(width: 8),
                Text(cat.name,
                    style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700)),
              ],
            ),
          ),
          for (final item in cat.items)
            Card(
              margin: const EdgeInsets.only(bottom: 8),
              child: ExpansionTile(
                title: Text(item.question,
                    style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 14)),
                children: [
                  Padding(
                    padding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
                    child: Text(item.answer,
                        style: theme.textTheme.bodyMedium
                            ?.copyWith(color: colorScheme.onSurfaceVariant, height: 1.5)),
                  ),
                ],
              ),
            ),
          const SizedBox(height: 8),
        ],
        if (!_showAllCategories)
          Center(
            child: TextButton(
              onPressed: () => setState(() => _showAllCategories = true),
              child: const Text('View all categories'),
            ),
          ),
        const SizedBox(height: 16),
        GlassContainer(
          borderRadius: AppTheme.radiusLg,
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Icon(Icons.support_agent, color: AppTheme.indigoLuxury),
                  const SizedBox(width: 8),
                  Text('Still need help?',
                      style: theme.textTheme.titleSmall?.copyWith(fontWeight: FontWeight.w700)),
                ],
              ),
              const SizedBox(height: 8),
              Text(
                'Our support team usually replies within 24 hours.',
                style: theme.textTheme.bodySmall?.copyWith(color: colorScheme.onSurfaceVariant),
              ),
              const SizedBox(height: 12),
              SizedBox(
                width: double.infinity,
                child: FilledButton.icon(
                  onPressed: () => context.push('/support/contact'),
                  icon: const Icon(Icons.email_outlined, size: 18),
                  label: const Text('Contact Support'),
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}
