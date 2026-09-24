import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/property.dart';

class PropertyService {
  static const String baseUrl = 'http://localhost:5235/api/properties';

  Future<List<Property>> getProperties({
    String? search,
    double? minPrice,
    double? maxPrice,
    int? bedrooms,
    int? bathrooms,
    String? location,
    bool? isAvailable,
  }) async {
    final queryParameters = <String, String>{};

    if (search != null && search.trim().isNotEmpty) {
      queryParameters['search'] = search.trim();
    }

    if (minPrice != null) {
      queryParameters['minPrice'] = minPrice.toString();
    }

    if (maxPrice != null) {
      queryParameters['maxPrice'] = maxPrice.toString();
    }

    if (bedrooms != null) {
      queryParameters['bedrooms'] = bedrooms.toString();
    }

    if (bathrooms != null) {
      queryParameters['bathrooms'] = bathrooms.toString();
    }

    if (location != null && location.trim().isNotEmpty) {
      queryParameters['location'] = location.trim();
    }

    if (isAvailable != null) {
      queryParameters['isAvailable'] = isAvailable.toString();
    }

    final uri = Uri.parse(baseUrl).replace(
      queryParameters: queryParameters.isEmpty ? null : queryParameters,
    );

    final response = await http.get(uri);

    if (response.statusCode == 200) {
      final List<dynamic> data = jsonDecode(response.body);

      return data.map((json) => Property.fromJson(json)).toList();
    } else {
      throw Exception('Failed to load properties');
    }
  }

  Future<Property> getPropertyById(int id) async {
    final response = await http.get(Uri.parse('$baseUrl/$id'));

    if (response.statusCode == 200) {
      return Property.fromJson(jsonDecode(response.body));
    } else {
      throw Exception('Failed to load property');
    }
  }
}
