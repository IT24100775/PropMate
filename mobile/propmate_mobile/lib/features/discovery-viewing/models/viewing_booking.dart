class ViewingBooking {
  final int id;
  final int status;
  final DateTime bookedAt;
  final int viewingSlotId;
  final DateTime startTime;
  final DateTime endTime;
  final int propertyListingId;
  final String propertyTitle;
  final String propertyCity;
  final String? primaryImageUrl;

  ViewingBooking({
    required this.id,
    required this.status,
    required this.bookedAt,
    required this.viewingSlotId,
    required this.startTime,
    required this.endTime,
    required this.propertyListingId,
    required this.propertyTitle,
    required this.propertyCity,
    this.primaryImageUrl,
  });

  factory ViewingBooking.fromJson(Map<String, dynamic> json) {
    int parsedStatus = 0;
    if (json['status'] is int) {
      parsedStatus = json['status'];
    } else if (json['status'] is String) {
      final s = json['status'].toString().toLowerCase();
      if (s == 'completed') parsedStatus = 1;
      else if (s == 'cancelled') parsedStatus = 2;
    }

    return ViewingBooking(
      id: json['id'] ?? 0,
      status: parsedStatus,
      bookedAt: DateTime.parse(json['bookedAt']).toLocal(),
      viewingSlotId: json['viewingSlotId'] ?? 0,
      startTime: DateTime.parse(json['startTime']).toLocal(),
      endTime: DateTime.parse(json['endTime']).toLocal(),
      propertyListingId: json['propertyListingId'] ?? 0,
      propertyTitle: json['propertyTitle'] ?? '',
      propertyCity: json['propertyCity'] ?? '',
      primaryImageUrl: json['primaryImageUrl'],
    );
  }
  
  String get statusText {
    if (status == 0) return 'Booked';
    if (status == 1) return 'Completed';
    if (status == 2) return 'Cancelled';
    return 'Unknown';
  }
}