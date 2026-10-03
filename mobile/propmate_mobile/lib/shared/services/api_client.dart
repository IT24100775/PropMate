import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

class ApiClient {
  static String get baseUrl {
    const deployedUrl = String.fromEnvironment('API_BASE_URL');

    if (deployedUrl.isNotEmpty) {
      return deployedUrl;
    }

    if (kIsWeb) {
      return 'http://localhost:5235/api';
    } else {
      return 'http://10.0.2.2:5235/api';
    }
  }

  static Future<Map<String, String>> _getTokenHeaders() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('jwt_token');
    if (token != null) {
      return {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      };
    }
    return {'Content-Type': 'application/json'};
  }

  static Future<http.Response> get(String url) async {
    return await http.get(Uri.parse('$baseUrl$url'), headers: await _getTokenHeaders());
  }

  static Future<http.Response> post(String url, {Map<String, dynamic>? body}) async {
    return await http.post(
      Uri.parse('$baseUrl$url'),
      headers: await _getTokenHeaders(),
      body: body != null ? jsonEncode(body) : null,
    );
  }

  static Future<http.Response> delete(String url) async {
    return await http.delete(Uri.parse('$baseUrl$url'), headers: await _getTokenHeaders());
  }
}
