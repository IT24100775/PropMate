import 'dart:convert';

import '../../../shared/services/api_client.dart';
import 'package:http/http.dart' as http;

class MaintenanceApi {
  static String get baseUrl => ApiClient.baseUrl;

  Future<List<Map<String, dynamic>>> getRequests(int tenantId) async {
    final response = await http.get(
      Uri.parse('$baseUrl/maintenance/tenant/$tenantId'),
    );
    _ensureSuccess(response);
    return _asMapList(jsonDecode(response.body));
  }

  Future<List<Map<String, dynamic>>> getNotifications(int tenantId) async {
    final response = await http.get(
      Uri.parse('$baseUrl/maintenance/tenant/$tenantId/notifications'),
    );
    _ensureSuccess(response);
    return _asMapList(jsonDecode(response.body));
  }

  Future<void> createRequest({
    required int propertyId,
    required int tenantId,
    required String description,
    required String category,
    required String priority,
  }) async {
    final response = await http.post(
      Uri.parse('$baseUrl/maintenance'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'propertyId': propertyId,
        'tenantId': tenantId,
        'description': description,
        'category': category,
        'priority': priority,
      }),
    );
    _ensureSuccess(response);
  }

  List<Map<String, dynamic>> _asMapList(dynamic value) {
    if (value is! List) return [];
    return value
        .whereType<Map>()
        .map((item) => Map<String, dynamic>.from(item))
        .toList();
  }

  void _ensureSuccess(http.Response response) {
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception('API request failed (${response.statusCode}).');
    }
  }
}