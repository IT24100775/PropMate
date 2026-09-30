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
    String sort = 'newest',
    int page = 1,
  }) async {
    try {
      final queryParameters = <String, String>{
        'Page': page.toString(),
        'PageSize': '10',
      };

      // Search
      if (search != null && search.trim().isNotEmpty) {
        queryParameters['Search'] = search.trim();
      }

      // City
      if (city != null && city.trim().isNotEmpty) {
        queryParameters['City'] = city.trim();
      }

      // Backend enums:
      // ListingPurpose: Sale = 0, Rent = 1
      if (purpose != null) {
        queryParameters['Purpose'] = purpose.toString();
      }

      // Backend PropertyType enum values are sent as integers
      if (type != null) {
        queryParameters['PropertyType'] = type.toString();
      }

      // Price
      if (minPrice != null) {
        queryParameters['MinPrice'] = minPrice.toString();
      }

      if (maxPrice != null) {
        queryParameters['MaxPrice'] = maxPrice.toString();
      }

      // Translate the existing C2 sort choices to the C1 backend API.
      switch (sort) {
        case 'price_low':
          queryParameters['SortBy'] = 'price';
          queryParameters['SortOrder'] = 'asc';
          break;

        case 'price_high':
          queryParameters['SortBy'] = 'price';
          queryParameters['SortOrder'] = 'desc';
          break;

        case 'oldest':
          queryParameters['SortBy'] = 'createdAt';
          queryParameters['SortOrder'] = 'asc';
          break;

        case 'newest':
        default:
          queryParameters['SortBy'] = 'createdAt';
          queryParameters['SortOrder'] = 'desc';
          break;
      }

      final query = Uri(queryParameters: queryParameters).query;

      final response = await ApiClient.get(
        '/PropertyListings?$query',
      );

      if (response.statusCode == 200) {
        final decoded =
            jsonDecode(response.body) as Map<String, dynamic>;

        var result = PaginatedPropertyResult.fromJson(decoded);

        // The current backend search endpoint does not yet expose
        // bedroom/bathroom query parameters. Preserve the existing
        // C2 filters locally for now.
        var filteredItems = result.items;

        if (bedrooms != null) {
          filteredItems = filteredItems
              .where((property) => property.bedrooms >= bedrooms)
              .toList();
        }

        if (bathrooms != null) {
          filteredItems = filteredItems
              .where((property) => property.bathrooms >= bathrooms)
              .toList();
        }

        return PaginatedPropertyResult(
          items: filteredItems,
          page: result.page,
          totalCount: bedrooms != null || bathrooms != null
              ? filteredItems.length
              : result.totalCount,
          totalPages: bedrooms != null || bathrooms != null
              ? (filteredItems.isEmpty ? 0 : 1)
              : result.totalPages,
        );
      }

      throw Exception(
        'Failed to load properties (${response.statusCode}).',
      );
    } catch (e) {
      rethrow;
    }
  }
}