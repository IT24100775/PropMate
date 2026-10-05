import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:propmate_mobile/main.dart';

void main() {
  testWidgets('PropMate application starts successfully',
      (WidgetTester tester) async {
    await tester.pumpWidget(const PropMateApp());

    expect(find.byType(MaterialApp), findsOneWidget);
    expect(find.byType(AuthGate), findsOneWidget);
  });
}