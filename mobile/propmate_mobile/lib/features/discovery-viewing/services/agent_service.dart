import 'dart:convert';
import '../../../services/api_client.dart';
import '../models/agent_response.dart';

class AgentService {
  Future<AgentResponse> queryAgent(String query) async {
    final response = await ApiClient.post('/agent/discover', body: {'query': query});
    if (response.statusCode == 200) {
      return AgentResponse.fromJson(jsonDecode(response.body));
    } else {
      throw Exception('Failed to communicate with Agent API');
    }
  }
}
