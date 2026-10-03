import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/viewing_slot.dart';
import '../../../shared/services/api_client.dart';

class ViewingService {
  static String get baseUrl => ApiClient.baseUrl;

  Future<List<ViewingSlot>> getViewingSlots(int propertyId) async {
    final response = await http.get(
      Uri.parse('$baseUrl/properties/$propertyId/viewing-slots'),
    );

    if (response.statusCode == 200) {
      final List<dynamic> data = jsonDecode(response.body);

      return data.map((json) => ViewingSlot.fromJson(json)).toList();
    } else {
      throw Exception('Failed to load viewing slots');
    }
  }
}
