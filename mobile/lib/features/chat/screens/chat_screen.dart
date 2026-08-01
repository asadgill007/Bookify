import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../../../core/theme/app_theme.dart';
import '../providers/chat_provider.dart';

/// Bookify Assistant chat screen — persisted history + rule-based backend.
class ChatScreen extends ConsumerStatefulWidget {
  const ChatScreen({super.key});

  @override
  ConsumerState<ChatScreen> createState() => _ChatScreenState();
}

class _ChatScreenState extends ConsumerState<ChatScreen> {
  final _controller = TextEditingController();
  final _scrollController = ScrollController();
  final List<ChatBubble> _liveBubbles = [];
  bool _isSending = false;
  bool _initialLoading = true;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _loadHistory());
  }

  @override
  void dispose() {
    _controller.dispose();
    _scrollController.dispose();
    super.dispose();
  }

  Future<void> _loadHistory() async {
    final history = await ref.read(chatHistoryProvider.future);
    if (!mounted) return;
    setState(() {
      _liveBubbles
        ..clear()
        ..addAll(history);
      _initialLoading = false;
    });
    _scrollToBottom();
  }

  void _scrollToBottom() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (_scrollController.hasClients) {
        _scrollController.animateTo(
          _scrollController.position.maxScrollExtent,
          duration: const Duration(milliseconds: 300),
          curve: Curves.easeOut,
        );
      }
    });
  }

  Future<void> _send(String text) async {
    final message = text.trim();
    if (message.isEmpty || _isSending) return;

    setState(() {
      _isSending = true;
      _liveBubbles.add(ChatBubble.local('user', message));
    });
    _controller.clear();
    _scrollToBottom();

    try {
      final reply = await ref.read(chatApiProvider).send(message);
      if (!mounted) return;
      setState(() {
        _liveBubbles.add(ChatBubble.local('assistant', reply));
        _isSending = false;
      });
    } catch (_) {
      if (!mounted) return;
      setState(() {
        _liveBubbles.add(ChatBubble.local(
            'assistant', "Sorry, I couldn't reach the assistant right now. Please try again."));
        _isSending = false;
      });
    }
    _scrollToBottom();
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
          title: Row(
            children: [
              Container(
                padding: const EdgeInsets.all(6),
                decoration: BoxDecoration(
                  gradient: LinearGradient(
                    colors: [AppTheme.indigoLuxury, const Color(0xFF7C3AED)],
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                  ),
                  borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                ),
                child: const Icon(Icons.auto_awesome, color: Colors.white, size: 18),
              ),
              const SizedBox(width: 10),
              const Text('Bookify Assistant'),
            ],
          ),
          backgroundColor: Colors.transparent,
        ),
        body: Column(
          children: [
            Expanded(
              child: _initialLoading
                  ? const Center(child: CircularProgressIndicator())
                  : _liveBubbles.isEmpty
                      ? _emptyState(theme, colorScheme)
                      : ListView.builder(
                          controller: _scrollController,
                          padding: const EdgeInsets.all(16),
                          itemCount: _liveBubbles.length,
                          itemBuilder: (context, index) {
                            final bubble = _liveBubbles[index];
                            return _bubble(bubble, theme, colorScheme, isDark);
                          },
                        ),
            ),
            if (_liveBubbles.isEmpty) _suggestionChips(theme, colorScheme),
            _inputBar(theme, colorScheme),
          ],
        ),
      ),
    );
  }

  Widget _emptyState(ThemeData theme, ColorScheme colorScheme) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.auto_awesome, size: 56, color: AppTheme.indigoLuxury),
            const SizedBox(height: 16),
            Text('Hi! I\'m Bookify\'s assistant.',
                style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700)),
            const SizedBox(height: 8),
            Text(
              'I can help you find businesses, check your booking status, '
              'or answer questions about bookings, payments and your account.',
              textAlign: TextAlign.center,
              style: theme.textTheme.bodyMedium?.copyWith(color: colorScheme.onSurfaceVariant),
            ),
          ],
        ),
      ),
    );
  }

  Widget _suggestionChips(ThemeData theme, ColorScheme colorScheme) {
    const suggestions = [
      'Find a salon near me',
      'Check my booking status',
      'How do I cancel a booking?',
      'How do I reset my password?',
    ];
    return Padding(
      padding: const EdgeInsets.fromLTRB(16, 0, 16, 8),
      child: Wrap(
        spacing: 8,
        runSpacing: 8,
        children: suggestions.map((s) {
          return ActionChip(
            label: Text(s, style: const TextStyle(fontSize: 12)),
            onPressed: () => _send(s),
          );
        }).toList(),
      ),
    );
  }

  Widget _bubble(ChatBubble bubble, ThemeData theme, ColorScheme colorScheme, bool isDark) {
    final isUser = bubble.isUser;
    return Align(
      alignment: isUser ? Alignment.centerRight : Alignment.centerLeft,
      child: Container(
        margin: const EdgeInsets.only(bottom: 10),
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
        constraints: BoxConstraints(
          maxWidth: MediaQuery.of(context).size.width * 0.78,
        ),
        decoration: BoxDecoration(
          color: isUser
              ? AppTheme.indigoLuxury
              : (isDark ? AppTheme.glassDark : Colors.white.withValues(alpha: 0.9)),
          borderRadius: BorderRadius.only(
            topLeft: const Radius.circular(16),
            topRight: const Radius.circular(16),
            bottomLeft: Radius.circular(isUser ? 16 : 4),
            bottomRight: Radius.circular(isUser ? 4 : 16),
          ),
        ),
        child: Text(
          bubble.content,
          style: theme.textTheme.bodyMedium?.copyWith(
            color: isUser ? Colors.white : colorScheme.onSurface,
            height: 1.4,
          ),
        ),
      ),
    ).animate().fadeIn(duration: 250.ms);
  }

  Widget _inputBar(ThemeData theme, ColorScheme colorScheme) {
    return SafeArea(
      top: false,
      child: Container(
        padding: const EdgeInsets.fromLTRB(12, 8, 12, 12),
        decoration: BoxDecoration(
          color: theme.brightness == Brightness.dark
              ? AppTheme.slate900.withValues(alpha: 0.9)
              : Colors.white.withValues(alpha: 0.9),
        ),
        child: Row(
          children: [
            Expanded(
              child: GlassContainer(
                borderRadius: AppTheme.radiusFull,
                padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 2),
                child: TextField(
                  controller: _controller,
                  minLines: 1,
                  maxLines: 4,
                  decoration: InputDecoration(
                    hintText: 'Ask me anything...',
                    border: InputBorder.none,
                    enabledBorder: InputBorder.none,
                    focusedBorder: InputBorder.none,
                    hintStyle: TextStyle(color: colorScheme.onSurfaceVariant),
                  ),
                  textInputAction: TextInputAction.send,
                  onSubmitted: _send,
                ),
              ),
            ),
            const SizedBox(width: 8),
            GestureDetector(
              onTap: () => _send(_controller.text),
              child: Container(
                width: 46,
                height: 46,
                decoration: BoxDecoration(
                  gradient: LinearGradient(
                    colors: [AppTheme.indigoLuxury, const Color(0xFF7C3AED)],
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                  ),
                  shape: BoxShape.circle,
                ),
                child: _isSending
                    ? const Padding(
                        padding: EdgeInsets.all(13),
                        child: CircularProgressIndicator(
                            strokeWidth: 2, color: Colors.white),
                      )
                    : const Icon(Icons.send_rounded, color: Colors.white, size: 20),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
