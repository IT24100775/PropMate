import 'package:flutter/material.dart';

class Component3AiAssistantPanel extends StatefulWidget {
  final String transactionType;
  final int transactionId;
  const Component3AiAssistantPanel({super.key, required this.transactionType, required this.transactionId});
  @override State<Component3AiAssistantPanel> createState() => _Component3AiAssistantPanelState();
}

class _Component3AiAssistantPanelState extends State<Component3AiAssistantPanel> {
  bool open = false;
  @override Widget build(BuildContext context) {
    if (!open) return Align(alignment: Alignment.centerRight, child: TextButton.icon(onPressed: () => setState(() => open = true), icon: const Icon(Icons.auto_awesome), label: const Text('AI Assistant')));
    return Card(child: Padding(padding: const EdgeInsets.all(16), child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
      Row(children: [const Icon(Icons.auto_awesome), const SizedBox(width: 8), const Expanded(child: Text('AI negotiation assistant', style: TextStyle(fontWeight: FontWeight.w700))), IconButton(onPressed: () => setState(() => open = false), icon: const Icon(Icons.close))]),
      const SizedBox(height: 8),
      Text('Context: ${widget.transactionType} #${widget.transactionId}'),
      const SizedBox(height: 8),
      const Text('The assistant can help analyze negotiation context and suggest next steps. High-impact transaction actions require explicit human approval.'),
    ])));
  }
}
