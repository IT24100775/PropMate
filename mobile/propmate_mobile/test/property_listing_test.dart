import 'package:flutter_test/flutter_test.dart';
import 'package:propmate_mobile/features/property-listings/models/property_listing.dart';

void main() {
  group('PropertyListing.fromJson', () {
    test('creates property listing from valid JSON', () {
      final json = {
        'id': 10,
        'ownerId': 5,
        'title': 'Modern Colombo Apartment',
        'description': 'Spacious apartment in a convenient city location.',
        'purpose': 'Rent',
        'propertyType': 'Apartment',
        'price': 150000.50,
        'address': '123 Galle Road',
        'city': 'Colombo',
        'bedrooms': 3,
        'bathrooms': 2,
        'status': 'Published',
        'imageUrls': [
          'https://example.com/image1.jpg',
          'https://example.com/image2.jpg',
        ],
      };

      final listing = PropertyListing.fromJson(json);

      expect(listing.id, 10);
      expect(listing.ownerId, 5);
      expect(listing.title, 'Modern Colombo Apartment');
      expect(listing.purpose, 'Rent');
      expect(listing.propertyType, 'Apartment');
      expect(listing.price, 150000.50);
      expect(listing.city, 'Colombo');
      expect(listing.bedrooms, 3);
      expect(listing.bathrooms, 2);
      expect(listing.status, 'Published');
      expect(listing.imageUrls.length, 2);
    });

    test('converts integer price to double', () {
      final listing = PropertyListing.fromJson({
        'price': 250000,
      });

      expect(listing.price, 250000.0);
      expect(listing.price, isA<double>());
    });

    test('uses safe defaults when optional JSON values are missing', () {
      final listing = PropertyListing.fromJson({});

      expect(listing.id, 0);
      expect(listing.ownerId, 0);
      expect(listing.title, '');
      expect(listing.description, '');
      expect(listing.price, 0.0);
      expect(listing.bedrooms, 0);
      expect(listing.bathrooms, 0);
      expect(listing.status, '');
      expect(listing.imageUrls, isEmpty);
    });
  });
}