import 'dart:convert';

import 'package:shared_preferences/shared_preferences.dart';

import '../../shared/services/api_client.dart';

class MobileAuthService {
  static const String _tokenKey = 'jwt_token';
  static const String _userIdKey = 'user_id';
  static const String _emailKey = 'user_email';
  static const String _roleKey = 'user_role';

  /// Login an existing Buyer/Renter account.
  static Future<void> login({
    required String email,
    required String password,
  }) async {
    final response = await ApiClient.post(
        '/Auth/login',
        body: {
            'email': email.trim(),
            'password': password,
        },
    );

    final Map<String, dynamic> body = _decodeBody(response.body);

    if (response.statusCode != 200) {
      throw Exception(
        _extractError(body, 'Unable to log in.'),
      );
    }

    await _saveAuthSession(body);
  }

  /// Register a new Buyer/Renter account.
  ///
  /// The ASP.NET backend automatically assigns
  /// UserRole.BuyerRenter to newly registered users.
  static Future<void> register({
    required String firstName,
    required String lastName,
    required String email,
    required String password,
  }) async {
    final response = await ApiClient.post(
      '/Auth/register',
      body: {
        'firstName': firstName.trim(),
        'lastName': lastName.trim(),
        'email': email.trim(),
        'password': password,
      },
    );

    final Map<String, dynamic> body = _decodeBody(response.body);

    if (response.statusCode != 200) {
      throw Exception(
        _extractError(body, 'Unable to create account.'),
      );
    }

    await _saveAuthSession(body);
  }

  /// Returns true when a JWT has been stored on this device/browser.
  static Future<bool> isLoggedIn() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString(_tokenKey);

    return token != null && token.isNotEmpty;
  }

  static Future<String?> getToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_tokenKey);
  }

  static Future<int?> getUserId() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getInt(_userIdKey);
  }

  static Future<String?> getEmail() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_emailKey);
  }

  static Future<String?> getRole() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_roleKey);
  }

  /// Clears the local Buyer/Renter authentication session.
  static Future<void> logout() async {
    final prefs = await SharedPreferences.getInstance();

    await prefs.remove(_tokenKey);
    await prefs.remove(_userIdKey);
    await prefs.remove(_emailKey);
    await prefs.remove(_roleKey);
  }

  static Future<void> _saveAuthSession(
    Map<String, dynamic> body,
  ) async {
    final token = body['token']?.toString();
    final email = body['email']?.toString();
    final role = body['role']?.toString();

    final rawUserId = body['userId'];
    final int? userId = rawUserId is int
        ? rawUserId
        : int.tryParse(rawUserId?.toString() ?? '');

    if (token == null ||
        token.isEmpty ||
        userId == null ||
        email == null ||
        email.isEmpty ||
        role == null ||
        role.isEmpty) {
      throw Exception(
        'The server returned an invalid authentication response.',
      );
    }

    if (role != 'BuyerRenter') {
      throw Exception(
        'This mobile application is for Buyer/Renter accounts.',
      );
    }

    final prefs = await SharedPreferences.getInstance();

    await prefs.setString(_tokenKey, token);
    await prefs.setInt(_userIdKey, userId);
    await prefs.setString(_emailKey, email);
    await prefs.setString(_roleKey, role);
  }

  static Map<String, dynamic> _decodeBody(String body) {
    if (body.trim().isEmpty) {
      return <String, dynamic>{};
    }

    try {
      final decoded = jsonDecode(body);

      if (decoded is Map<String, dynamic>) {
        return decoded;
      }

      return <String, dynamic>{};
    } catch (_) {
      return <String, dynamic>{};
    }
  }

  static String _extractError(
    Map<String, dynamic> body,
    String fallback,
  ) {
    final message = body['message'];

    if (message != null && message.toString().trim().isNotEmpty) {
      return message.toString();
    }

    return fallback;
  }
}