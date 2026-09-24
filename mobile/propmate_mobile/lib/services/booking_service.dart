import 'dart:convert';

import 'package:http/http.dart' as http;

class BookingService {
  static const String baseUrl = 'http://localhost:5235/api/viewing-bookings';

  Future<void> bookViewing({
    required int viewingSlotId,
    required int userId,
  }) async {
    final response = await http.post(
      Uri.parse(baseUrl),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'viewingSlotId': viewingSlotId, 'userId': userId}),
    );

    if (response.statusCode == 201) {
      return;
    }

    if (response.statusCode == 409) {
      throw Exception('This viewing slot is already booked');
    }

    if (response.statusCode == 404) {
      throw Exception('Viewing slot not found');
    }

    throw Exception('Failed to book viewing');
  }
}
