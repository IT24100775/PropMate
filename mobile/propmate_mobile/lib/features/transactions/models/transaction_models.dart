class RentalApplication {
  final int id;
  final int propertyListingId;
  final String propertyTitle;
  final int tenantId;
  final String tenantName;
  final String employment;
  final double monthlyIncome;
  final int occupants;
  final DateTime preferredMoveInDate;
  final int durationMonths;
  final String? message;
  final String status;
  final String negotiationStatus;

  RentalApplication({
    required this.id,
    required this.propertyListingId,
    required this.propertyTitle,
    required this.tenantId,
    required this.tenantName,
    required this.employment,
    required this.monthlyIncome,
    required this.occupants,
    required this.preferredMoveInDate,
    required this.durationMonths,
    this.message,
    required this.status,
    required this.negotiationStatus,
  });

  factory RentalApplication.fromJson(Map<String, dynamic> j) => RentalApplication(
    id: j['id'] ?? 0,
    propertyListingId: j['propertyListingId'] ?? 0,
    propertyTitle: j['propertyTitle'] ?? '',
    tenantId: j['tenantId'] ?? 0,
    tenantName: j['tenantName'] ?? '',
    employment: j['employment'] ?? '',
    monthlyIncome: (j['monthlyIncome'] as num?)?.toDouble() ?? 0,
    occupants: j['occupants'] ?? 0,
    preferredMoveInDate: DateTime.parse(j['preferredMoveInDate'].toString()),
    durationMonths: j['durationMonths'] ?? 0,
    message: j['message'],
    status: j['status']?.toString() ?? '',
    negotiationStatus: j['negotiationStatus']?.toString() ?? '',
  );
}

class PurchaseOffer {
  final int id;
  final int propertyListingId;
  final String propertyTitle;
  final int buyerId;
  final String buyerName;
  final double offerAmount;
  final String? conditions;
  final String status;
  final String negotiationStatus;

  PurchaseOffer({
    required this.id,
    required this.propertyListingId,
    required this.propertyTitle,
    required this.buyerId,
    required this.buyerName,
    required this.offerAmount,
    this.conditions,
    required this.status,
    required this.negotiationStatus,
  });

  factory PurchaseOffer.fromJson(Map<String, dynamic> j) => PurchaseOffer(
    id: j['id'] ?? 0,
    propertyListingId: j['propertyListingId'] ?? 0,
    propertyTitle: j['propertyTitle'] ?? '',
    buyerId: j['buyerId'] ?? 0,
    buyerName: j['buyerName'] ?? '',
    offerAmount: (j['offerAmount'] as num?)?.toDouble() ?? 0,
    conditions: j['conditions'],
    status: j['status']?.toString() ?? '',
    negotiationStatus: j['negotiationStatus']?.toString() ?? '',
  );
}

class RentalNegotiationOffer {
  final int id;
  final int proposedByUserId;
  final double monthlyRent;
  final DateTime moveInDate;
  final int durationMonths;
  final String? conditions;
  final String status;

  RentalNegotiationOffer({
    required this.id,
    required this.proposedByUserId,
    required this.monthlyRent,
    required this.moveInDate,
    required this.durationMonths,
    this.conditions,
    required this.status,
  });

  factory RentalNegotiationOffer.fromJson(Map<String, dynamic> j) => RentalNegotiationOffer(
    id: j['id'] ?? 0,
    proposedByUserId: j['proposedByUserId'] ?? 0,
    monthlyRent: (j['monthlyRent'] as num?)?.toDouble() ?? 0,
    moveInDate: DateTime.parse(j['moveInDate'].toString()),
    durationMonths: j['durationMonths'] ?? 0,
    conditions: j['conditions'],
    status: j['status']?.toString() ?? '',
  );
}

class PurchaseNegotiationOffer {
  final int id;
  final int proposedByUserId;
  final double offerAmount;
  final String? conditions;
  final String status;

  PurchaseNegotiationOffer({
    required this.id,
    required this.proposedByUserId,
    required this.offerAmount,
    this.conditions,
    required this.status,
  });

  factory PurchaseNegotiationOffer.fromJson(Map<String, dynamic> j) => PurchaseNegotiationOffer(
    id: j['id'] ?? 0,
    proposedByUserId: j['proposedByUserId'] ?? 0,
    offerAmount: (j['offerAmount'] as num?)?.toDouble() ?? 0,
    conditions: j['conditions'],
    status: j['status']?.toString() ?? '',
  );
}

class NegotiationMessage {
  final int id;
  final int senderUserId;
  final String senderName;
  final String message;
  final DateTime createdAt;

  NegotiationMessage({required this.id, required this.senderUserId, required this.senderName, required this.message, required this.createdAt});

  factory NegotiationMessage.fromJson(Map<String, dynamic> j) => NegotiationMessage(
    id: j['id'] ?? 0,
    senderUserId: j['senderUserId'] ?? 0,
    senderName: j['senderName'] ?? '',
    message: j['message'] ?? '',
    createdAt: DateTime.parse(j['createdAt'].toString()),
  );
}

class RentalAgreement {
  final int id;
  final double finalMonthlyRent;
  final DateTime moveInDate;
  final int durationMonths;
  final String terms;
  final String tenantObligation;
  final String ownerObligation;
  final String penaltyTerms;
  final bool buyerConfirmed;
  final bool sellerConfirmed;
  final String status;

  RentalAgreement({required this.id, required this.finalMonthlyRent, required this.moveInDate, required this.durationMonths, required this.terms, required this.tenantObligation, required this.ownerObligation, required this.penaltyTerms, required this.buyerConfirmed, required this.sellerConfirmed, required this.status});

  factory RentalAgreement.fromJson(Map<String, dynamic> j) => RentalAgreement(
    id: j['id'] ?? 0,
    finalMonthlyRent: (j['finalMonthlyRent'] as num?)?.toDouble() ?? 0,
    moveInDate: DateTime.parse(j['moveInDate'].toString()),
    durationMonths: j['durationMonths'] ?? 0,
    terms: j['terms'] ?? '',
    tenantObligation: j['tenantObligation'] ?? '',
    ownerObligation: j['ownerObligation'] ?? '',
    penaltyTerms: j['penaltyTerms'] ?? '',
    buyerConfirmed: j['buyerConfirmed'] == true,
    sellerConfirmed: j['sellerConfirmed'] == true,
    status: j['status']?.toString() ?? '',
  );
}

class PurchaseAgreement {
  final int id;
  final double finalPurchasePrice;
  final String conditions;
  final String buyerObligation;
  final String sellerObligation;
  final String penaltyTerms;
  final bool buyerConfirmed;
  final bool sellerConfirmed;
  final String status;

  PurchaseAgreement({required this.id, required this.finalPurchasePrice, required this.conditions, required this.buyerObligation, required this.sellerObligation, required this.penaltyTerms, required this.buyerConfirmed, required this.sellerConfirmed, required this.status});

  factory PurchaseAgreement.fromJson(Map<String, dynamic> j) => PurchaseAgreement(
    id: j['id'] ?? 0,
    finalPurchasePrice: (j['finalPurchasePrice'] as num?)?.toDouble() ?? 0,
    conditions: j['conditions'] ?? '',
    buyerObligation: j['buyerObligation'] ?? '',
    sellerObligation: j['sellerObligation'] ?? '',
    penaltyTerms: j['penaltyTerms'] ?? '',
    buyerConfirmed: j['buyerConfirmed'] == true,
    sellerConfirmed: j['sellerConfirmed'] == true,
    status: j['status']?.toString() ?? '',
  );
}
