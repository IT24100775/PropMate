import 'package:flutter/material.dart';

import '../../models/viewing_slot.dart';

class ViewingSlotCard extends StatelessWidget {
  final ViewingSlot slot;
  final VoidCallback? onBook;

  const ViewingSlotCard({super.key, required this.slot, this.onBook});

  String _formatDateTime(DateTime dateTime) {
    return '${dateTime.day.toString().padLeft(2, '0')}/'
        '${dateTime.month.toString().padLeft(2, '0')}/'
        '${dateTime.year} '
        '${dateTime.hour.toString().padLeft(2, '0')}:'
        '${dateTime.minute.toString().padLeft(2, '0')}';
  }

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Viewing Slot',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),

            const SizedBox(height: 10),

            Text('Start: ${_formatDateTime(slot.startTime)}'),

            const SizedBox(height: 4),

            Text('End: ${_formatDateTime(slot.endTime)}'),

            const SizedBox(height: 10),

            Text(
              slot.isAvailable ? 'Available' : 'Booked',
              style: TextStyle(
                fontWeight: FontWeight.bold,
                color: slot.isAvailable ? Colors.green : Colors.red,
              ),
            ),

            if (slot.isAvailable) ...[
              const SizedBox(height: 12),
              SizedBox(
                width: double.infinity,
                child: ElevatedButton.icon(
                  onPressed: onBook,
                  icon: const Icon(Icons.calendar_month),
                  label: const Text('Book Viewing'),
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
