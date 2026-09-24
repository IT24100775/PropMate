class Property {
  final int id;
  final String title;
  final String description;
  final double price;
  final int bedrooms;
  final int bathrooms;
  final String location;
  final double latitude;
  final double longitude;
  final bool isAvailable;

  Property({
    required this.id,
    required this.title,
    required this.description,
    required this.price,
    required this.bedrooms,
    required this.bathrooms,
    required this.location,
    required this.latitude,
    required this.longitude,
    required this.isAvailable,
  });

  factory Property.fromJson(Map<String, dynamic> json) {
    return Property(
      id: json['id'],
      title: json['title'],
      description: json['description'],
      price: (json['price'] as num).toDouble(),
      bedrooms: json['bedrooms'],
      bathrooms: json['bathrooms'],
      location: json['location'],
      latitude: (json['latitude'] as num).toDouble(),
      longitude: (json['longitude'] as num).toDouble(),
      isAvailable: json['isAvailable'],
    );
  }
}
