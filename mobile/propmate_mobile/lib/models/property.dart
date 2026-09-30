class Property {
  final int id;
  final String title;
  final String description;
  final double price;
  final String purpose;
  final int bedrooms;
  final int bathrooms;
  final String? address;
  final String? city;
  final double? latitude;
  final double? longitude;
  final List<String> images;

  Property({
    required this.id,
    required this.title,
    required this.description,
    required this.price,
    required this.purpose,
    required this.bedrooms,
    required this.bathrooms,
    this.address,
    this.city,
    this.latitude,
    this.longitude,
    required this.images,
  });

  factory Property.fromJson(Map<String, dynamic> json) {
    var rawImages = json['images'] as List? ?? [];
    List<String> imageUrls = rawImages.map((img) => img['imageUrl'].toString()).toList();
    if (imageUrls.isEmpty && json['primaryImageUrl'] != null) {
      imageUrls.add(json['primaryImageUrl']);
    }

    return Property(
      id: json['id'] ?? 0,
      title: json['title'] ?? '',
      description: json['description'] ?? '',
      price: (json['price'] ?? 0.0).toDouble(),
      purpose: json['purpose']?.toString() ?? '',
      bedrooms: json['bedrooms'] ?? 0,
      bathrooms: json['bathrooms'] ?? 0,
      address: json['address'],
      city: json['city'],
      latitude: json['latitude'] != null ? (json['latitude'] as num).toDouble() : null,
      longitude: json['longitude'] != null ? (json['longitude'] as num).toDouble() : null,
      images: imageUrls,
    );
  }
}

class PaginatedPropertyResult {
  final List<Property> items;
  final int page;
  final int totalCount;
  final int totalPages;

  PaginatedPropertyResult({required this.items, required this.page, required this.totalCount, required this.totalPages});

  factory PaginatedPropertyResult.fromJson(Map<String, dynamic> json) {
    var list = json['items'] as List? ?? [];
    return PaginatedPropertyResult(
      items: list.map((e) => Property.fromJson(e)).toList(),
      page: json['page'] ?? 1,
      totalCount: json['totalCount'] ?? 0,
      totalPages: json['totalPages'] ?? 0,
    );
  }
}