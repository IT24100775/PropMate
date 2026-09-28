import 'package:flutter/material.dart';
import '../../models/viewing_slot.dart';
import '../../services/booking_service.dart';

class ViewingSlotsPage extends StatefulWidget {
  final int propertyListingId;
  const ViewingSlotsPage({super.key, required this.propertyListingId});
  @override State<ViewingSlotsPage> createState() => _State();
}

class _State extends State<ViewingSlotsPage> {
  final BookingService _svc = BookingService();
  List<ViewingSlot> _slots = [];

  @override void initState() { super.initState(); _load(); }

  void _load() async {
    final s = await _svc.getAvailableSlots(widget.propertyListingId);
    if(mounted) setState(() => _slots = s);
  }

  void _book(int id) async {
    await _svc.bookViewing(id);
    if(mounted) Navigator.pop(context); // Go back after booking
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Available Slots')),
      body: _slots.isEmpty ? const Center(child: Text('No available slots.')) : ListView.builder(
        padding: const EdgeInsets.all(8),
        itemCount: _slots.length,
        itemBuilder: (c, i) {
          final s = _slots[i];
          return Card(
            child: ListTile(
              leading: const Icon(Icons.schedule, color: Colors.green),
              title: Text('${s.startTime}', style: const TextStyle(fontWeight: FontWeight.bold)),
              trailing: ElevatedButton(
                style: ElevatedButton.styleFrom(backgroundColor: Theme.of(context).colorScheme.primary),
                onPressed: () => _book(s.id), 
                child: const Text('Book')
              ),
            ),
          );
        },
      ),
    );
  }
}
