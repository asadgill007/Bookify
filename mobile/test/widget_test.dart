import 'package:flutter_test/flutter_test.dart';
import 'package:bookify/app.dart';

void main() {
  testWidgets('Bookify app renders', (WidgetTester tester) async {
    await tester.pumpWidget(const BookifyApp());
    expect(find.byType(BookifyApp), findsOneWidget);
  });
}
