import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:bookify/app.dart';

void main() {
  testWidgets('Bookify app renders', (WidgetTester tester) async {
    await tester.pumpWidget(
      const ProviderScope(
        child: BookifyApp(),
      ),
    );
    expect(find.byType(BookifyApp), findsOneWidget);

    // Advance the fake clock past the splash delay so no timers are left
    // pending when the test ends.
    await tester.pump(const Duration(seconds: 3));
    await tester.pumpAndSettle();
  });
}
