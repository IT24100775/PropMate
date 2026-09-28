import 'dart:convert';
import '../models/property.dart';
import 'api_client.dart';

class PropertyService {
  Future<PaginatedPropertyResult> getProperties({
    String? search,
    String? city,
    int? type,
    int? purpose,
    double? minPrice,
    double? maxPrice,
    int? bedrooms,
    int? bathrooms,
    String sort = "newest",
    int page = 1,
  }) async {
    try {
      String query = '?Page=$page&PageSize=10&Sort=$sort';
      if (search != null && search.isNotEmpty) query += '&Keyword=$search';
      if (city != null && city.isNotEmpty) query += '&City=$city';
      if (type != null) query += '&PropertyType=$type';
      if (purpose != null) query += '&Purpose=$purpose';
      if (minPrice != null) query += '&MinPrice=$minPrice';
      if (maxPrice != null) query += '&MaxPrice=$maxPrice';
      if (bedrooms != null) query += '&MinBedrooms=$bedrooms';
      if (bathrooms != null) query += '&MinBathrooms=$bathrooms';
      
      final response = await ApiClient.get('/discovery/properties$query');
      if (response.statusCode == 200) {
        return PaginatedPropertyResult.fromJson(jsonDecode(response.body));
      }
    } catch (e) {
      // ignore
    }
    return PaginatedPropertyResult(items: [], page: 1, totalCount: 0, totalPages: 0);
  }
}
