class ViewingSlot {
  final int id;
  final int propertyListingId;
  final DateTime startTime;
  final DateTime endTime;
  final bool isAvailable;

  ViewingSlot({
    required this.id,
    required this.propertyListingId,
    required this.startTime,
    required this.endTime,
    required this.isAvailable,
  });

  factory ViewingSlot.fromJson(Map<String, dynamic> json) {
    return ViewingSlot(
      id: json['id'] ?? 0,
      propertyListingId: json['propertyListingId'] ?? 0,
      startTime: DateTime.parse(json['startTime']).toLocal(),
      endTime: DateTime.parse(json['endTime']).toLocal(),
      isAvailable: json['isAvailable'] ?? false,
    );
  }
}