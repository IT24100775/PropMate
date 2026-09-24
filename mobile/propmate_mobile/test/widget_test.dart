import 'package:flutter_test/flutter_test.dart';
import 'package:propmate_mobile/main.dart';

void main() {
  testWidgets('Property discovery page loads', (WidgetTester tester) async {
    await tester.pumpWidget(const PropMateApp());

    expect(find.text('Property Discovery'), findsOneWidget);
  });
}
