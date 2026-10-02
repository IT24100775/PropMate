import 'dart:convert';
import '../models/viewing_booking.dart';
import '../models/viewing_slot.dart';
import '../../../shared/services/api_client.dart';

class BookingService {
  Future<List<ViewingSlot>> getAvailableSlots(int propertyListingId) async {
    final response = await ApiClient.get('/viewing-slots/property/$propertyListingId/available');
    if (response.statusCode == 200) {
      final List data = jsonDecode(response.body);
      return data.map((x) => ViewingSlot.fromJson(x)).toList();
    }
    return [];
  }

  Future<bool> bookViewing(int viewingSlotId) async {
    final response = await ApiClient.post('/viewing-bookings', body: {'viewingSlotId': viewingSlotId});
    return response.statusCode == 200 || response.statusCode == 201;
  }

  Future<List<ViewingBooking>> getMyBookings() async {
    final response = await ApiClient.get('/viewing-bookings/my');
    if (response.statusCode == 200) {
      final List data = jsonDecode(response.body);
      return data.map((x) => ViewingBooking.fromJson(x)).toList();
    }
    return [];
  }

  Future<bool> cancelBooking(int bookingId) async {
    final response = await ApiClient.post('/viewing-bookings/$bookingId/cancel');
    return response.statusCode == 200;
  }
}
