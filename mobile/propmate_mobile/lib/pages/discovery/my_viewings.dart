import 'package:flutter/material.dart';
import '../../models/viewing_booking.dart';
import '../../services/booking_service.dart';

class MyViewingsPage extends StatefulWidget {
  const MyViewingsPage({super.key});

  @override
  State<MyViewingsPage> createState() => _State();
}

class _State extends State<MyViewingsPage> {
  final BookingService _svc = BookingService();

  List<ViewingBooking> _bookings = [];
  bool _loading = true;

  static const Color _background = Color(0xFFF6F4EF);
  static const Color _cardBackground = Color(0xFFFBFAF7);
  static const Color _dark = Color(0xFF181817);
  static const Color _gold = Color(0xFFCFA03C);
  static const Color _mutedText = Color(0xFF777168);
  static const Color _border = Color(0xFFDEDAD1);

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    if (mounted) {
      setState(() => _loading = true);
    }

    final bookings = await _svc.getMyBookings();

    if (mounted) {
      setState(() {
        _bookings = bookings;
        _loading = false;
      });
    }
  }

  Future<void> _cancel(int id) async {
    await _svc.cancelBooking(id);
    await _load();
  }

  String _formatDateTime(DateTime dateTime) {
    final local = dateTime.toLocal();

    final months = [
      'Jan',
      'Feb',
      'Mar',
      'Apr',
      'May',
      'Jun',
      'Jul',
      'Aug',
      'Sep',
      'Oct',
      'Nov',
      'Dec',
    ];

    final hour = local.hour == 0
        ? 12
        : local.hour > 12
            ? local.hour - 12
            : local.hour;

    final minute = local.minute.toString().padLeft(2, '0');
    final period = local.hour >= 12 ? 'PM' : 'AM';

    return '${local.day} ${months[local.month - 1]} ${local.year}  •  '
        '$hour:$minute $period';
  }

  Color _statusBackground(int status) {
    switch (status) {
      case 0:
        return const Color(0xFFF3E8CB);
      case 1:
        return const Color(0xFFDDE9DE);
      default:
        return const Color(0xFFF2DFDC);
    }
  }

  Color _statusForeground(int status) {
    switch (status) {
      case 0:
        return const Color(0xFF8A681D);
      case 1:
        return const Color(0xFF416747);
      default:
        return const Color(0xFF8D4038);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: _background,

      appBar: AppBar(
        backgroundColor: _dark,
        foregroundColor: Colors.white,
        elevation: 0,
        centerTitle: false,
        iconTheme: const IconThemeData(
          color: _gold,
        ),
        title: const Text(
          'My Viewings',
          style: TextStyle(
            fontSize: 18,
            fontWeight: FontWeight.w600,
          ),
        ),
      ),

      body: _loading
          ? const Center(
              child: CircularProgressIndicator(
                color: _gold,
                strokeWidth: 2.5,
              ),
            )
          : _bookings.isEmpty
              ? _buildEmptyState()
              : RefreshIndicator(
                  color: _gold,
                  onRefresh: _load,
                  child: ListView.builder(
                    padding: const EdgeInsets.fromLTRB(
                      20,
                      24,
                      20,
                      30,
                    ),
                    itemCount: _bookings.length,
                    itemBuilder: (context, index) {
                      return _buildBookingCard(_bookings[index]);
                    },
                  ),
                ),
    );
  }

  Widget _buildEmptyState() {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Container(
              width: 72,
              height: 72,
              decoration: BoxDecoration(
                color: _cardBackground,
                border: Border.all(
                  color: _border,
                ),
              ),
              child: const Icon(
                Icons.calendar_month_outlined,
                color: _gold,
                size: 30,
              ),
            ),

            const SizedBox(height: 22),

            const Text(
              'No viewings found',
              style: TextStyle(
                color: _dark,
                fontSize: 22,
                fontWeight: FontWeight.w600,
              ),
            ),

            const SizedBox(height: 8),

            const Text(
              'Your scheduled property viewings will appear here.',
              textAlign: TextAlign.center,
              style: TextStyle(
                color: _mutedText,
                fontSize: 14,
                height: 1.5,
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildBookingCard(ViewingBooking booking) {
    final isBooked = booking.status == 0;

    return Container(
      margin: const EdgeInsets.only(bottom: 14),
      decoration: BoxDecoration(
        color: _cardBackground,
        border: Border.all(
          color: _border,
        ),
      ),
      child: Padding(
        padding: const EdgeInsets.all(18),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // TOP ROW
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Container(
                  width: 48,
                  height: 48,
                  decoration: BoxDecoration(
                    color: const Color(0xFFF0ECE3),
                    border: Border.all(
                      color: _border,
                    ),
                  ),
                  child: const Icon(
                    Icons.home_work_outlined,
                    color: _gold,
                    size: 23,
                  ),
                ),

                const SizedBox(width: 14),

                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text(
                        'PROPERTY VIEWING',
                        style: TextStyle(
                          color: _gold,
                          fontSize: 9,
                          fontWeight: FontWeight.w700,
                          letterSpacing: 1.3,
                        ),
                      ),

                      const SizedBox(height: 5),

                      Text(
                        booking.propertyTitle,
                        style: const TextStyle(
                          color: _dark,
                          fontSize: 17,
                          fontWeight: FontWeight.w600,
                          height: 1.25,
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),

            const SizedBox(height: 18),

            Container(
              width: double.infinity,
              padding: const EdgeInsets.symmetric(
                horizontal: 14,
                vertical: 13,
              ),
              decoration: BoxDecoration(
                color: Colors.white,
                border: Border.all(
                  color: _border,
                ),
              ),
              child: Row(
                children: [
                  const Icon(
                    Icons.schedule_outlined,
                    color: _gold,
                    size: 19,
                  ),

                  const SizedBox(width: 10),

                  Expanded(
                    child: Text(
                      _formatDateTime(booking.startTime),
                      style: const TextStyle(
                        color: _dark,
                        fontSize: 13,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                  ),
                ],
              ),
            ),

            const SizedBox(height: 14),

            // STATUS + CANCEL
            Row(
              children: [
                Container(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 11,
                    vertical: 6,
                  ),
                  decoration: BoxDecoration(
                    color: _statusBackground(booking.status),
                    border: Border.all(
                      color: _statusForeground(
                        booking.status,
                      ).withValues(alpha: 0.25),
                    ),
                  ),
                  child: Text(
                    booking.statusText.toUpperCase(),
                    style: TextStyle(
                      color: _statusForeground(booking.status),
                      fontSize: 10,
                      fontWeight: FontWeight.w700,
                      letterSpacing: 0.8,
                    ),
                  ),
                ),

                const Spacer(),

                if (isBooked)
                  TextButton.icon(
                    onPressed: () => _showCancelConfirmation(
                      booking,
                    ),
                    style: TextButton.styleFrom(
                      foregroundColor: const Color(0xFF8D4038),
                      padding: const EdgeInsets.symmetric(
                        horizontal: 10,
                        vertical: 8,
                      ),
                    ),
                    icon: const Icon(
                      Icons.close,
                      size: 17,
                    ),
                    label: const Text(
                      'CANCEL',
                      style: TextStyle(
                        fontSize: 10,
                        fontWeight: FontWeight.w700,
                        letterSpacing: 0.8,
                      ),
                    ),
                  ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _showCancelConfirmation(
    ViewingBooking booking,
  ) async {
    final shouldCancel = await showDialog<bool>(
      context: context,
      builder: (dialogContext) {
        return AlertDialog(
          backgroundColor: _cardBackground,
          shape: const RoundedRectangleBorder(
            borderRadius: BorderRadius.zero,
          ),
          title: const Text(
            'Cancel viewing?',
            style: TextStyle(
              color: _dark,
              fontWeight: FontWeight.w600,
            ),
          ),
          content: Text(
            'Are you sure you want to cancel your viewing for '
            '${booking.propertyTitle}?',
            style: const TextStyle(
              color: _mutedText,
              height: 1.5,
            ),
          ),
          actions: [
            TextButton(
              onPressed: () {
                Navigator.pop(dialogContext, false);
              },
              child: const Text(
                'KEEP VIEWING',
                style: TextStyle(
                  color: _dark,
                  fontSize: 11,
                  fontWeight: FontWeight.w700,
                ),
              ),
            ),

            TextButton(
              onPressed: () {
                Navigator.pop(dialogContext, true);
              },
              child: const Text(
                'CANCEL VIEWING',
                style: TextStyle(
                  color: Color(0xFF8D4038),
                  fontSize: 11,
                  fontWeight: FontWeight.w700,
                ),
              ),
            ),
          ],
        );
      },
    );

    if (shouldCancel == true) {
      await _cancel(booking.id);
    }
  }
}