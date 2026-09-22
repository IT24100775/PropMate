class PropertyListing {
  final int id;
  final int ownerId;
  final String title;
  final String description;
  final String purpose;
  final String propertyType;
  final double price;
  final String address;
  final String city;
  final int bedrooms;
  final int bathrooms;
  final String status;
  final List<String> imageUrls;

  PropertyListing({
    required this.id,
    required this.ownerId,
    required this.title,
    required this.description,
    required this.purpose,
    required this.propertyType,
    required this.price,
    required this.address,
    required this.city,
    required this.bedrooms,
    required this.bathrooms,
    required this.status,
    required this.imageUrls,
  });

  factory PropertyListing.fromJson(Map<String, dynamic> json) {
    return PropertyListing(
      id: json['id'] ?? 0,
      ownerId: json['ownerId'] ?? 0,
      title: json['title'] ?? '',
      description: json['description'] ?? '',
      purpose: json['purpose'] ?? '',
      propertyType: json['propertyType'] ?? '',
      price: (json['price'] as num?)?.toDouble() ?? 0.0,
      address: json['address'] ?? '',
      city: json['city'] ?? '',
      bedrooms: json['bedrooms'] ?? 0,
      bathrooms: json['bathrooms'] ?? 0,
      status: json['status'] ?? '',
      imageUrls: (json['imageUrls'] as List<dynamic>?)
              ?.map((image) => image.toString())
              .toList() ??
          [],
    );
  }
}