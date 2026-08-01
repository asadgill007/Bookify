import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/theme/app_theme.dart';
import '../providers/support_provider.dart';

/// Contact Support form — submits a support ticket.
class ContactSupportScreen extends ConsumerStatefulWidget {
  /// Optional prefill for the "Report a Problem" flow (linked to an appointment).
  final String? appointmentId;
  final String? businessName;

  const ContactSupportScreen({super.key, this.appointmentId, this.businessName});

  @override
  ConsumerState<ContactSupportScreen> createState() => _ContactSupportScreenState();
}

class _ContactSupportScreenState extends ConsumerState<ContactSupportScreen> {
  final _subjectController = TextEditingController();
  final _messageController = TextEditingController();
  final _emailController = TextEditingController();
  String _category = SupportCategories.general;
  bool _isSubmitting = false;
  String? _error;

  bool get _isReport => widget.appointmentId != null;

  @override
  void initState() {
    super.initState();
    if (_isReport) {
      _category = SupportCategories.problem;
      _subjectController.text = widget.businessName != null
          ? 'Issue with appointment at ${widget.businessName}'
          : 'Issue with my appointment';
    }
  }

  @override
  void dispose() {
    _subjectController.dispose();
    _messageController.dispose();
    _emailController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (_subjectController.text.trim().isEmpty || _messageController.text.trim().isEmpty) {
      setState(() => _error = 'Subject and message are required.');
      return;
    }
    setState(() {
      _isSubmitting = true;
      _error = null;
    });
    try {
      await ref.read(supportApiProvider).submit(
            category: _category,
            subject: _subjectController.text.trim(),
            message: _messageController.text.trim(),
            appointmentId: widget.appointmentId,
            contactEmail: _emailController.text.trim(),
          );
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
            content: Text("Thank you! Your message has been sent. We'll reply within 24 hours.")),
      );
      context.pop();
    } catch (_) {
      if (!mounted) return;
      setState(() => _error = 'Could not submit your message. Please try again.');
    } finally {
      if (mounted) setState(() => _isSubmitting = false);
    }
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
          title: Text(_isReport ? 'Report a Problem' : 'Contact Support'),
          backgroundColor: Colors.transparent,
        ),
        body: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            if (_isReport) ...[
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: AppTheme.warning.withValues(alpha: 0.12),
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(color: AppTheme.warning.withValues(alpha: 0.3)),
                ),
                child: Row(
                  children: [
                    Icon(Icons.report_problem_outlined, color: AppTheme.warning),
                    const SizedBox(width: 10),
                    Expanded(
                      child: Text(
                        'Tell us what went wrong with this appointment and we\'ll look into it.',
                        style: theme.textTheme.bodySmall,
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 16),
            ],
            GlassContainer(
              borderRadius: AppTheme.radiusLg,
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('Category', style: theme.textTheme.titleSmall?.copyWith(fontWeight: FontWeight.w600)),
                  const SizedBox(height: 10),
                  if (_isReport)
                    // Static label for the report flow — 'problem' is not part
                    // of the selectable categories and a dropdown with a value
                    // outside its items would assert.
                    Container(
                      width: double.infinity,
                      padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 14),
                      decoration: BoxDecoration(
                        color: isDark
                            ? Colors.white.withValues(alpha: 0.05)
                            : Colors.white.withValues(alpha: 0.5),
                        borderRadius: BorderRadius.circular(14),
                      ),
                      child: Row(
                        children: [
                          Icon(Icons.report_problem_outlined,
                              size: 18, color: AppTheme.warning),
                          const SizedBox(width: 8),
                          Text(SupportCategories.problem,
                              style: theme.textTheme.bodyMedium),
                        ],
                      ),
                    )
                  else
                    DropdownButtonFormField<String>(
                      initialValue: _category,
                      decoration: InputDecoration(
                        filled: true,
                        fillColor: isDark
                            ? Colors.white.withValues(alpha: 0.05)
                            : Colors.white.withValues(alpha: 0.5),
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(14),
                          borderSide: BorderSide.none,
                        ),
                      ),
                      items: SupportCategories.all
                          .map((c) =>
                              DropdownMenuItem(value: c, child: Text(c)))
                          .toList(),
                      onChanged: (v) {
                        if (v != null) setState(() => _category = v);
                      },
                    ),
                  const SizedBox(height: 16),
                  TextField(
                    controller: _subjectController,
                    enabled: !_isReport,
                    decoration: const InputDecoration(labelText: 'Subject'),
                  ),
                  const SizedBox(height: 16),
                  TextField(
                    controller: _messageController,
                    maxLines: 6,
                    decoration: const InputDecoration(
                      labelText: 'Message',
                      hintText: 'Tell us what happened...',
                      alignLabelWithHint: true,
                    ),
                  ),
                  const SizedBox(height: 16),
                  TextField(
                    controller: _emailController,
                    keyboardType: TextInputType.emailAddress,
                    decoration: const InputDecoration(labelText: 'Contact email (optional)'),
                  ),
                  if (_error != null) ...[
                    const SizedBox(height: 12),
                    Text(_error!,
                        style: TextStyle(color: colorScheme.error, fontSize: 13)),
                  ],
                  const SizedBox(height: 20),
                  SizedBox(
                    width: double.infinity,
                    height: 52,
                    child: FilledButton(
                      onPressed: _isSubmitting ? null : _submit,
                      child: _isSubmitting
                          ? const SizedBox(
                              height: 20,
                              width: 20,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            )
                          : Text(_isReport ? 'Submit Report' : 'Send Message'),
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 20),
            Center(
              child: Column(
                children: [
                  Text('Prefer email or phone?',
                      style: theme.textTheme.bodySmall?.copyWith(color: colorScheme.onSurfaceVariant)),
                  const SizedBox(height: 4),
                  Text('support@bookify.app  ·  +1 (555) 010-2030',
                      style: theme.textTheme.bodySmall?.copyWith(color: AppTheme.indigoLuxury)),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
