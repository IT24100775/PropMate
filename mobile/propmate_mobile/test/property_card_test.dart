import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:propmate_mobile/features/property-listings/models/property_listing.dart';
import 'package:propmate_mobile/features/property-listings/widgets/property_card.dart';

void main() {
  final property = PropertyListing(
    id: 1,
    ownerId: 10,
    title: 'Modern Colombo Apartment',
    description: 'A spacious apartment in Colombo.',
    purpose: 'Rent',
    propertyType: 'Apartment',
    price: 150000,
    address: '123 Galle Road',
    city: 'Colombo',
    bedrooms: 3,
    bathrooms: 2,
    status: 'Published',
    imageUrls: const [],
  );

  testWidgets('displays published property information correctly',
      (WidgetTester tester) async {
    await tester.pumpWidget(
      MaterialApp(
        home: Scaffold(
          body: PropertyCard(
            property: property,
            onTap: () {},
          ),
        ),
      ),
    );

    expect(find.text('Modern Colombo Apartment'), findsOneWidget);
    expect(find.text('Rent'), findsOneWidget);
    expect(find.text('Apartment'), findsOneWidget);
    expect(find.text('Colombo'), findsOneWidget);
    expect(find.text('LKR 150,000'), findsOneWidget);
    expect(find.text('3 Beds'), findsOneWidget);
    expect(find.text('2 Baths'), findsOneWidget);
    expect(find.text('View Details'), findsOneWidget);
  });

  testWidgets('calls onTap when property card is tapped',
      (WidgetTester tester) async {
    var tapped = false;

    await tester.pumpWidget(
      MaterialApp(
        home: Scaffold(
          body: PropertyCard(
            property: property,
            onTap: () {
              tapped = true;
            },
          ),
        ),
      ),
    );

    await tester.tap(find.text('Modern Colombo Apartment'));
    await tester.pump();

    expect(tapped, isTrue);
  });

  testWidgets('shows placeholder when property has no image',
      (WidgetTester tester) async {
    await tester.pumpWidget(
      MaterialApp(
        home: Scaffold(
          body: PropertyCard(
            property: property,
            onTap: () {},
          ),
        ),
      ),
    );

    expect(find.byIcon(Icons.home_work_outlined), findsOneWidget);
  });
}