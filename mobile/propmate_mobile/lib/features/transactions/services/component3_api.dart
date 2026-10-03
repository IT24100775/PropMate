import 'dart:convert';

import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

import '../models/transaction_models.dart';
import '../../../shared/services/api_client.dart';

class Component3Api {
  static String get baseUrl => ApiClient.baseUrl;

  /// Uses the same JWT created by the Buyer/Renter mobile
  /// login and registration flow.
  Future<Map<String, String>> _headers() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('jwt_token');

    return {
      'Accept': 'application/json',
      'Content-Type': 'application/json',
      if (token != null && token.isNotEmpty)
        'Authorization': 'Bearer $token',
    };
  }

  Future<dynamic> _request(
    String method,
    String path, {
    Map<String, dynamic>? body,
  }) async {
    final uri = Uri.parse('$baseUrl$path');
    final headers = await _headers();

    late http.Response response;

    if (method == 'GET') {
      response = await http.get(
        uri,
        headers: headers,
      );
    } else {
      response = await http.post(
        uri,
        headers: headers,
        body: jsonEncode(body ?? {}),
      );
    }

    final text = response.body;

    dynamic data;

    if (text.isNotEmpty) {
      try {
        data = jsonDecode(text);
      } catch (_) {
        data = text;
      }
    }

    if (response.statusCode < 200 ||
        response.statusCode >= 300) {
      String message =
          response.reasonPhrase ?? 'Request failed.';

      if (data is Map<String, dynamic> &&
          data['message'] != null) {
        message = data['message'].toString();
      } else if (data is String && data.isNotEmpty) {
        message = data;
      }

      throw Exception(message);
    }

    return data;
  }

  Future<RentalApplication> createRentalApplication({
    required int propertyListingId,
    required String employment,
    required double monthlyIncome,
    required int occupants,
    required DateTime moveInDate,
    required int durationMonths,
    String? message,
  }) async {
    final data = await _request(
      'POST',
      '/rental-applications',
      body: {
        'propertyListingId': propertyListingId,
        'employment': employment,
        'monthlyIncome': monthlyIncome,
        'occupants': occupants,
        'preferredMoveInDate':
            moveInDate.toIso8601String().split('T').first,
        'durationMonths': durationMonths,
        'message': message,
      },
    );

    return RentalApplication.fromJson(data);
  }

  Future<PurchaseOffer> createPurchaseOffer({
    required int propertyListingId,
    required double offerAmount,
    String? conditions,
  }) async {
    final data = await _request(
      'POST',
      '/purchase-offers',
      body: {
        'propertyListingId': propertyListingId,
        'offerAmount': offerAmount,
        'conditions': conditions,
      },
    );

    return PurchaseOffer.fromJson(data);
  }

  Future<List<RentalApplication>>
      myRentalApplications() async {
    final data = await _request(
      'GET',
      '/rental-applications/mine',
    );

    return (data as List)
        .map((x) => RentalApplication.fromJson(x))
        .toList();
  }

  Future<List<PurchaseOffer>> myPurchaseOffers() async {
    final data = await _request(
      'GET',
      '/purchase-offers/mine',
    );

    return (data as List)
        .map((x) => PurchaseOffer.fromJson(x))
        .toList();
  }

  Future<List<RentalNegotiationOffer>> rentalOffers(
    int id,
  ) async {
    final data = await _request(
      'GET',
      '/rental-applications/$id/negotiation/offers',
    );

    return (data as List)
        .map((x) => RentalNegotiationOffer.fromJson(x))
        .toList();
  }

  Future<List<PurchaseNegotiationOffer>> purchaseOffers(
    int id,
  ) async {
    final data = await _request(
      'GET',
      '/purchase-offers/$id/negotiation/offers',
    );

    return (data as List)
        .map((x) => PurchaseNegotiationOffer.fromJson(x))
        .toList();
  }

  Future<List<NegotiationMessage>> rentalMessages(
    int id,
  ) async {
    final data = await _request(
      'GET',
      '/rental-applications/$id/negotiation/messages',
    );

    return (data as List)
        .map((x) => NegotiationMessage.fromJson(x))
        .toList();
  }

  Future<List<NegotiationMessage>> purchaseMessages(
    int id,
  ) async {
    final data = await _request(
      'GET',
      '/purchase-offers/$id/negotiation/messages',
    );

    return (data as List)
        .map((x) => NegotiationMessage.fromJson(x))
        .toList();
  }

  Future<void> sendRentalMessage(
    int id,
    String message,
  ) async {
    await _request(
      'POST',
      '/rental-applications/$id/negotiation/messages',
      body: {
        'message': message,
      },
    );
  }

  Future<void> sendPurchaseMessage(
    int id,
    String message,
  ) async {
    await _request(
      'POST',
      '/purchase-offers/$id/negotiation/messages',
      body: {
        'message': message,
      },
    );
  }

  Future<void> counterRental(
    int id, {
    required double monthlyRent,
    required DateTime moveInDate,
    required int durationMonths,
    String? conditions,
  }) async {
    await _request(
      'POST',
      '/rental-applications/$id/negotiation/counter',
      body: {
        'monthlyRent': monthlyRent,
        'moveInDate':
            moveInDate.toIso8601String().split('T').first,
        'durationMonths': durationMonths,
        'conditions': conditions,
      },
    );
  }

  Future<void> counterPurchase(
    int id, {
    required double offerAmount,
    String? conditions,
  }) async {
    await _request(
      'POST',
      '/purchase-offers/$id/negotiation/counter',
      body: {
        'offerAmount': offerAmount,
        'conditions': conditions,
      },
    );
  }

  Future<void> acceptRentalCounter(
    int id,
    int offerId,
  ) async {
    await _request(
      'POST',
      '/rental-applications/$id/negotiation/offers/$offerId/accept',
    );
  }

  Future<void> acceptPurchaseCounter(
    int id,
    int offerId,
  ) async {
    await _request(
      'POST',
      '/purchase-offers/$id/negotiation/offers/$offerId/accept',
    );
  }

  Future<RentalAgreement?> rentalAgreement(int id) async {
    try {
      final data = await _request(
        'GET',
        '/rental-applications/$id/agreement',
      );

      return RentalAgreement.fromJson(data);
    } catch (_) {
      return null;
    }
  }

  Future<PurchaseAgreement?> purchaseAgreement(
    int id,
  ) async {
    try {
      final data = await _request(
        'GET',
        '/purchase-offers/$id/agreement',
      );

      return PurchaseAgreement.fromJson(data);
    } catch (_) {
      return null;
    }
  }

  Future<void> confirmRentalAgreement(int id) async {
    await _request(
      'POST',
      '/rental-applications/$id/agreement/confirm',
    );
  }

  Future<void> confirmPurchaseAgreement(int id) async {
    await _request(
      'POST',
      '/purchase-offers/$id/agreement/confirm',
    );
  }
}