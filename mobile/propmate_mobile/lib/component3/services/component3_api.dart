import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import '../models/transaction_models.dart';
import 'auth_token_provider.dart';

class Component3Api {
  static final String baseUrl =
      'http://${defaultTargetPlatform == TargetPlatform.android ? '10.0.2.2' : 'localhost'}:5235/api';
  final AuthTokenProvider authTokenProvider;

  Component3Api({AuthTokenProvider? authTokenProvider})
      : authTokenProvider = authTokenProvider ?? TenantSession.instance;

  Map<String, String> _headers() {
    final token = authTokenProvider.accessToken;
    return {
      'Accept': 'application/json',
      'Content-Type': 'application/json',
      if (token != null && token.isNotEmpty) 'Authorization': 'Bearer $token',
    };
  }

  Future<dynamic> _request(String method, String path, {Map<String, dynamic>? body}) async {
    final uri = Uri.parse('$baseUrl$path');
    late http.Response response;
    final headers = _headers();
    if (method == 'GET') response = await http.get(uri, headers: headers);
    else response = await http.post(uri, headers: headers, body: jsonEncode(body ?? {}));
    final text = response.body;
    dynamic data;
    if (text.isNotEmpty) { try { data = jsonDecode(text); } catch (_) { data = text; } }
    if (response.statusCode < 200 || response.statusCode >= 300) {
      final message = data is Map ? data['message'] ?? data['title'] : data;
      throw Exception(
        message?.toString() ?? 'Request failed (${response.statusCode}).',
      );
    }
    return data;
  }

  Future<RentalApplication> createRentalApplication({required int propertyListingId, required String employment, required double monthlyIncome, required int occupants, required DateTime moveInDate, required int durationMonths, String? message}) async => RentalApplication.fromJson(await _request('POST', '/rental-applications', body: {
    'propertyListingId': propertyListingId, 'employment': employment, 'monthlyIncome': monthlyIncome, 'occupants': occupants,
    'preferredMoveInDate': moveInDate.toIso8601String().split('T').first, 'durationMonths': durationMonths, 'message': message,
  }));

  Future<PurchaseOffer> createPurchaseOffer({required int propertyListingId, required double offerAmount, String? conditions}) async => PurchaseOffer.fromJson(await _request('POST', '/purchase-offers', body: {
    'propertyListingId': propertyListingId, 'offerAmount': offerAmount, 'conditions': conditions,
  }));

  Future<List<RentalApplication>> myRentalApplications() async => (await _request('GET', '/rental-applications/mine') as List).map((x) => RentalApplication.fromJson(x)).toList();
  Future<List<PurchaseOffer>> myPurchaseOffers() async => (await _request('GET', '/purchase-offers/mine') as List).map((x) => PurchaseOffer.fromJson(x)).toList();

  Future<List<RentalNegotiationOffer>> rentalOffers(int id) async => (await _request('GET', '/rental-applications/$id/negotiation/offers') as List).map((x) => RentalNegotiationOffer.fromJson(x)).toList();
  Future<List<PurchaseNegotiationOffer>> purchaseOffers(int id) async => (await _request('GET', '/purchase-offers/$id/negotiation/offers') as List).map((x) => PurchaseNegotiationOffer.fromJson(x)).toList();
  Future<List<NegotiationMessage>> rentalMessages(int id) async => (await _request('GET', '/rental-applications/$id/negotiation/messages') as List).map((x) => NegotiationMessage.fromJson(x)).toList();
  Future<List<NegotiationMessage>> purchaseMessages(int id) async => (await _request('GET', '/purchase-offers/$id/negotiation/messages') as List).map((x) => NegotiationMessage.fromJson(x)).toList();

  Future<void> sendRentalMessage(int id, String message) async { await _request('POST', '/rental-applications/$id/negotiation/messages', body: {'message': message}); }
  Future<void> sendPurchaseMessage(int id, String message) async { await _request('POST', '/purchase-offers/$id/negotiation/messages', body: {'message': message}); }

  Future<void> counterRental(int id, {required double monthlyRent, required DateTime moveInDate, required int durationMonths, String? conditions}) async { await _request('POST', '/rental-applications/$id/negotiation/counter', body: {'monthlyRent': monthlyRent, 'moveInDate': moveInDate.toIso8601String().split('T').first, 'durationMonths': durationMonths, 'conditions': conditions}); }
  Future<void> counterPurchase(int id, {required double offerAmount, String? conditions}) async { await _request('POST', '/purchase-offers/$id/negotiation/counter', body: {'offerAmount': offerAmount, 'conditions': conditions}); }
  Future<void> acceptRentalCounter(int id, int offerId) async { await _request('POST', '/rental-applications/$id/negotiation/offers/$offerId/accept'); }
  Future<void> acceptPurchaseCounter(int id, int offerId) async { await _request('POST', '/purchase-offers/$id/negotiation/offers/$offerId/accept'); }

  Future<RentalAgreement?> rentalAgreement(int id) async { try { return RentalAgreement.fromJson(await _request('GET', '/rental-applications/$id/agreement')); } catch (_) { return null; } }
  Future<PurchaseAgreement?> purchaseAgreement(int id) async { try { return PurchaseAgreement.fromJson(await _request('GET', '/purchase-offers/$id/agreement')); } catch (_) { return null; } }
  Future<void> confirmRentalAgreement(int id) async { await _request('POST', '/rental-applications/$id/agreement/confirm'); }
  Future<void> confirmPurchaseAgreement(int id) async { await _request('POST', '/purchase-offers/$id/agreement/confirm'); }
}
