import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/favourite.dart';

class FavouriteService {
  static const String baseUrl = 'http://localhost:5235/api/favourites';

  Future<Favourite> addFavourite({
    required int propertyId,
    required int userId,
  }) async {
    final response = await http.post(
      Uri.parse(baseUrl),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'propertyId': propertyId, 'userId': userId}),
    );

    if (response.statusCode == 201) {
      return Favourite.fromJson(jsonDecode(response.body));
    }

    if (response.statusCode == 409) {
      throw Exception('Property is already in favourites');
    }

    if (response.statusCode == 404) {
      throw Exception('Property not found');
    }

    throw Exception('Failed to add favourite');
  }

  Future<void> removeFavourite(int favouriteId) async {
    final response = await http.delete(Uri.parse('$baseUrl/$favouriteId'));

    if (response.statusCode == 204) {
      return;
    }

    if (response.statusCode == 404) {
      throw Exception('Favourite not found');
    }

    throw Exception('Failed to remove favourite');
  }
}
