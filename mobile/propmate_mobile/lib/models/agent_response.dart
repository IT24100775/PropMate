class AgentResponse {
  final Map<String, dynamic> interpretedCriteria;
  final List<String> plan;
  final List<AgentMatch> matches;
  final List<String> warnings;
  final double confidence;

  AgentResponse({
    required this.interpretedCriteria,
    required this.plan,
    required this.matches,
    required this.warnings,
    required this.confidence,
  });

  factory AgentResponse.fromJson(Map<String, dynamic> json) {
    var rawMatches = json['matches'] as List? ?? [];
    var mappedMatches = rawMatches.map((m) => AgentMatch.fromJson(m)).toList();
    var rawWarnings = json['warnings'] as List? ?? [];

    return AgentResponse(
      interpretedCriteria: json['interpretedCriteria'] ?? {},
      plan: (json['plan'] as List? ?? []).map((e) => e.toString()).toList(),
      matches: mappedMatches,
      warnings: rawWarnings.map((e) => e.toString()).toList(),
      confidence: (json['confidence'] ?? 0.0).toDouble(),
    );
  }
}

class AgentMatch {
  final int propertyListingId;
  final String reasons;
  final List<dynamic> availableViewingSlots;

  AgentMatch({
    required this.propertyListingId,
    required this.reasons,
    required this.availableViewingSlots,
  });

  factory AgentMatch.fromJson(Map<String, dynamic> json) {
    return AgentMatch(
      propertyListingId: json['propertyListingId'] ?? 0,
      reasons: json['reasons'] ?? '',
      availableViewingSlots: json['availableViewingSlots'] ?? [],
    );
  }
}
