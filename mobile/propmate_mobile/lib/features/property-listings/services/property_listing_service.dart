import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/property_listing.dart';

class PropertyListingService {
  static const String baseUrl =
      'http://localhost:5235/api/PropertyListings';

  Future<List<PropertyListing>> getPublishedProperties() async {
    final response = await http.get(
      Uri.parse(baseUrl),
      headers: {
        'Accept': 'application/json',
      },
    );

    if (response.statusCode == 200) {
      final Map<String, dynamic> decoded =
          jsonDecode(response.body) as Map<String, dynamic>;

      final List<dynamic> items =
          decoded['items'] as List<dynamic>? ?? [];

      return items
          .map(
            (item) => PropertyListing.fromJson(
              item as Map<String, dynamic>,
            ),
          )
          .toList();
    }

    throw Exception(
      'Failed to load properties (${response.statusCode}).',
    );
  }

  Future<PropertyListing> getPropertyById(int id) async {
    final response = await http.get(
      Uri.parse('$baseUrl/$id'),
      headers: {
        'Accept': 'application/json',
      },
    );

    if (response.statusCode == 200) {
      return PropertyListing.fromJson(
        jsonDecode(response.body) as Map<String, dynamic>,
      );
    }

    if (response.statusCode == 404) {
      throw Exception('Property not found.');
    }

    throw Exception(
      'Failed to load property (${response.statusCode}).',
    );
  }
}