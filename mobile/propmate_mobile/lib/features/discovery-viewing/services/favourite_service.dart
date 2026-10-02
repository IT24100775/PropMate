import 'dart:convert';
import '../models/property.dart';
import '../../../services/api_client.dart';

class FavouriteService {
  Future<PaginatedPropertyResult> getFavourites({int page = 1}) async {
    final response = await ApiClient.get('/favourites?page=$page&_t=${DateTime.now().millisecondsSinceEpoch}');
    if (response.statusCode == 200) {
      return PaginatedPropertyResult.fromJson(jsonDecode(response.body));
    }
    return PaginatedPropertyResult(items: [], page: 1, totalCount: 0, totalPages: 0);
  }

  Future<bool> addFavourite(int propertyListingId) async {
    final response = await ApiClient.post('/favourites/$propertyListingId');
    return response.statusCode == 200 || response.statusCode == 201 || response.statusCode == 409;
  }

  Future<bool> removeFavourite(int propertyListingId) async {
    final response = await ApiClient.delete('/favourites/$propertyListingId');
    return response.statusCode == 204 || response.statusCode == 200 || response.statusCode == 404;
  }
}
