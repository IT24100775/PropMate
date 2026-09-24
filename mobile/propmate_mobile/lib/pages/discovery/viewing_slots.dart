import 'package:flutter/material.dart';

import '../../components/discovery/viewing_slot_card.dart';
import '../../models/viewing_slot.dart';
import '../../services/viewing_service.dart';
import '../../services/booking_service.dart';

class ViewingSlotsPage extends StatefulWidget {
  final int propertyId;
  final String propertyTitle;

  const ViewingSlotsPage({
    super.key,
    required this.propertyId,
    required this.propertyTitle,
  });

  @override
  State<ViewingSlotsPage> createState() => _ViewingSlotsPageState();
}

class _ViewingSlotsPageState extends State<ViewingSlotsPage> {
  final ViewingService _viewingService = ViewingService();
  final BookingService _bookingService = BookingService();

  late Future<List<ViewingSlot>> _slots;

  @override
  void initState() {
    super.initState();
    _loadSlots();
  }

  void _loadSlots() {
    _slots = _viewingService.getViewingSlots(widget.propertyId);
  }

  Future<void> _refreshSlots() async {
    setState(() {
      _loadSlots();
    });

    await _slots;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(widget.propertyTitle)),
      body: RefreshIndicator(
        onRefresh: _refreshSlots,
        child: FutureBuilder<List<ViewingSlot>>(
          future: _slots,
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const Center(child: CircularProgressIndicator());
            }

            if (snapshot.hasError) {
              return Center(
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const Text('Failed to load viewing slots'),
                    const SizedBox(height: 10),
                    ElevatedButton(
                      onPressed: () {
                        setState(() {
                          _loadSlots();
                        });
                      },
                      child: const Text('Retry'),
                    ),
                  ],
                ),
              );
            }

            final slots = snapshot.data ?? [];

            if (slots.isEmpty) {
              return const Center(child: Text('No viewing slots available'));
            }

            return ListView.builder(
              padding: const EdgeInsets.all(16),
              itemCount: slots.length,
              itemBuilder: (context, index) {
                return ViewingSlotCard(
                  slot: slots[index],
                  onBook: () => _bookSlot(slots[index].id),
                );
              },
            );
          },
        ),
      ),
    );
  }

  Future<void> _bookSlot(int slotId) async {
    try {
      await _bookingService.bookViewing(
        viewingSlotId: slotId,
        userId: 1,
      );

      if (!mounted) return;

      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Viewing booked successfully!'),
        ),
      );

      setState(() {
        _loadSlots();
      });
    } catch (e) {
      if (!mounted) return;

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.toString())),
      );
    }
  }
}
