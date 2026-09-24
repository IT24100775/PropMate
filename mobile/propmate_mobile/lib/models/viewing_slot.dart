class ViewingSlot {
  final int id;
  final int propertyId;
  final DateTime startTime;
  final DateTime endTime;
  final bool isAvailable;

  ViewingSlot({
    required this.id,
    required this.propertyId,
    required this.startTime,
    required this.endTime,
    required this.isAvailable,
  });

  factory ViewingSlot.fromJson(Map<String, dynamic> json) {
    return ViewingSlot(
      id: json['id'],
      propertyId: json['propertyId'],
      startTime: DateTime.parse(json['startTime']),
      endTime: DateTime.parse(json['endTime']),
      isAvailable: json['isAvailable'],
    );
  }
}
