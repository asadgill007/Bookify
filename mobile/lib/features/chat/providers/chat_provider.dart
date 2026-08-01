import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/constants/api_constants.dart';
import '../../../core/network/api_client.dart';

/// A single chat bubble (from the backend history or the live session).
class ChatBubble {
  final String id;
  final String role; // "user" | "assistant"
  final String content;
  final DateTime createdAt;

  const ChatBubble({
    required this.id,
    required this.role,
    required this.content,
    required this.createdAt,
  });

  bool get isUser => role == 'user';

  factory ChatBubble.local(String role, String content) => ChatBubble(
        id: DateTime.now().microsecondsSinceEpoch.toString(),
        role: role,
        content: content,
        createdAt: DateTime.now(),
      );

  factory ChatBubble.fromJson(Map<String, dynamic> json) => ChatBubble(
        id: json['id'] as String? ?? '',
        role: json['role'] as String? ?? 'user',
        content: json['content'] as String? ?? '',
        createdAt:
            DateTime.tryParse(json['createdAt'] as String? ?? '') ?? DateTime.now(),
      );
}

/// Loads the user's persisted chat history (newest first, reversed to oldest).
final chatHistoryProvider = FutureProvider<List<ChatBubble>>((ref) async {
  final api = ref.watch(apiClientProvider);
  final response = await api.get(ApiConstants.chatHistory, queryParameters: {'limit': 100});
  final body = response.data;
  List<dynamic> rawList;
  if (body is Map<String, dynamic> && body.containsKey('data')) {
    rawList = body['data'] as List<dynamic>;
  } else if (body is List) {
    rawList = body;
  } else {
    return [];
  }
  final bubbles = rawList
      .map((e) => ChatBubble.fromJson(e as Map<String, dynamic>))
      .toList();
  return bubbles.reversed.toList();
});

/// Sends a message and returns the assistant reply.
class ChatApi {
  final ApiClient _api;
  ChatApi(this._api);

  Future<String> send(String message) async {
    final response = await _api.post(
      ApiConstants.chatMessages,
      data: {'message': message},
    );
    final body = response.data as Map<String, dynamic>;
    final data = (body['data'] ?? body) as Map<String, dynamic>;
    return data['reply'] as String? ?? '';
  }
}

final chatApiProvider = Provider<ChatApi>((ref) => ChatApi(ref.watch(apiClientProvider)));
