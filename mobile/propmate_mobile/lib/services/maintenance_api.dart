import 'dart:convert';

import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import '../component3/services/auth_token_provider.dart';

class MaintenanceApi {
  Map<String, String> get _headers => {
        if (TenantSession.instance.accessToken case final token?)
          'Authorization': 'Bearer $token',
      };

  static final String baseUrl = Uri(
    scheme: 'http',
    host: defaultTargetPlatform == TargetPlatform.android
        ? '10.0.2.2'
        : 'localhost',
    port: 5235,
    path: '/api',
  ).toString();

  Future<List<Map<String, dynamic>>> getRequests(int tenantId) async {
    final response = await http.get(
      Uri.parse('$baseUrl/maintenance/tenant/$tenantId'),
      headers: _headers,
    );
    _ensureSuccess(response);
    return _asMapList(jsonDecode(response.body));
  }

  Future<List<Map<String, dynamic>>> getNotifications(int tenantId) async {
    final response = await http.get(
      Uri.parse('$baseUrl/maintenance/tenant/$tenantId/notifications'),
      headers: _headers,
    );
    _ensureSuccess(response);
    return _asMapList(jsonDecode(response.body));
  }

  Future<void> createRequest({
    required int propertyId,
    required String description,
    required String category,
    required String priority,
  }) async {
    final response = await http.post(
      Uri.parse('$baseUrl/maintenance/tenant'),
      headers: {'Content-Type': 'application/json', ..._headers},
      body: jsonEncode({
        'propertyListingId': propertyId,
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