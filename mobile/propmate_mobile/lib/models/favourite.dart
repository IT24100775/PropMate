class Favourite {
  final int id;
  final int propertyId;
  final int userId;

  Favourite({required this.id, required this.propertyId, required this.userId});

  factory Favourite.fromJson(Map<String, dynamic> json) {
    return Favourite(
      id: json['id'],
      propertyId: json['propertyId'],
      userId: json['userId'],
    );
  }
}
