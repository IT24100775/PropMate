import 'package:flutter/material.dart';
import 'create_request.dart';
import '../component3/services/component3_api.dart';
import '../services/maintenance_api.dart';
import '../services/property_listing_service.dart';

class MaintenanceHome extends StatefulWidget {
  const MaintenanceHome({
    super.key,
    required this.tenantId,
    this.initialPropertyId,
    this.initialPropertyTitle,
  });

  final int tenantId;
  final int? initialPropertyId;
  final String? initialPropertyTitle;

  @override
  State<MaintenanceHome> createState() => _MaintenanceHomeState();
}

class _MaintenanceHomeState extends State<MaintenanceHome> {
  final MaintenanceApi _api = MaintenanceApi();
  final Component3Api _transactions = Component3Api();
  final PropertyListingService _listingService = PropertyListingService();

  List<Map<String, dynamic>> requests = [];
  List<Map<String, dynamic>> notifications = [];
  List<RentalPropertyOption> activeRentals = [];
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _loadData();
  }

  Future<void> _loadData() async {
    try {
      final results = await Future.wait<List<Map<String, dynamic>>>([
        _api.getRequests(widget.tenantId),
        _api.getNotifications(widget.tenantId),
      ]);

      // Fetch rental applications & agreements
      final rentalApplications = await _transactions.myRentalApplications();
      final agreements = await Future.wait(
        rentalApplications.map((rental) => _transactions.rentalAgreement(rental.id)),
      );

      final optionsMap = <int, RentalPropertyOption>{};

      // 1. Add completed rental agreements
      for (var index = 0; index < rentalApplications.length; index++) {
        final rental = rentalApplications[index];
        final agreement = agreements[index];
        if (agreement?.status == 'Completed') {
          optionsMap[rental.propertyListingId] = RentalPropertyOption(
            propertyListingId: rental.propertyListingId,
            title: rental.propertyTitle.isNotEmpty ? rental.propertyTitle : 'Rented Property #${rental.propertyListingId}',
            subtitle: 'Active Lease',
          );
        }
      }

      // 2. Add other tenant rental applications
      for (final rental in rentalApplications) {
        if (!optionsMap.containsKey(rental.propertyListingId)) {
          optionsMap[rental.propertyListingId] = RentalPropertyOption(
            propertyListingId: rental.propertyListingId,
            title: rental.propertyTitle.isNotEmpty ? rental.propertyTitle : 'Rental #${rental.propertyListingId}',
            subtitle: 'Applied Rental',
          );
        }
      }

      // 3. Add published properties with purpose "Rent"
      try {
        final publishedListings = await _listingService.getPublishedProperties();
        for (final listing in publishedListings) {
          if (listing.purpose.toLowerCase() == 'rent' && !optionsMap.containsKey(listing.id)) {
            optionsMap[listing.id] = RentalPropertyOption(
              propertyListingId: listing.id,
              title: listing.title,
              subtitle: '${listing.city} - LKR ${listing.price.toInt()}/mo',
            );
          }
        }
      } catch (_) {
        // Fallback gracefully if listing service fails
      }

      // 4. Ensure initial property is present if passed
      if (widget.initialPropertyId != null && !optionsMap.containsKey(widget.initialPropertyId)) {
        optionsMap[widget.initialPropertyId!] = RentalPropertyOption(
          propertyListingId: widget.initialPropertyId!,
          title: widget.initialPropertyTitle ?? 'Property #${widget.initialPropertyId}',
          subtitle: 'Selected Property',
        );
      }

      if (!mounted) return;
      setState(() {
        requests = results[0];
        notifications = results[1];
        activeRentals = optionsMap.values.toList();
        _error = null;
        _loading = false;
      });
    } catch (error) {
      if (!mounted) return;
      setState(() {
        _error = 'Could not connect to PropMate.';
        _loading = false;
      });
    }
  }

  Color getPriorityColor(String priority) {
    switch (priority.toUpperCase()) {
      case 'HIGH':
        return Colors.red;
      case 'MEDIUM':
        return Colors.orange;
      case 'LOW':
        return Colors.green;
      default:
        return Colors.grey;
    }
  }

  Color getStatusColor(String status) {
    switch (status.toUpperCase()) {
      case 'PENDING':
        return Colors.orange;
      case 'ASSIGNED':
        return Colors.blue;
      case 'SCHEDULED':
        return Colors.indigo;
      case 'IN_PROGRESS':
        return Colors.deepPurple;
      case 'RESOLVED':
        return Colors.green;
      default:
        return Colors.grey;
    }
  }

  @override
  Widget build(BuildContext context) {
    const charcoal = Color(0xFF181817);
    const gold = Color(0xFFCF9E3E);

    final pendingCount =
        requests.where((r) => r['status']?.toString().toUpperCase() == 'PENDING').length;

    final inProgressCount =
        requests.where((r) => r['status']?.toString().toUpperCase() == 'IN_PROGRESS').length;

    return Scaffold(
      backgroundColor: const Color(0xfff5f7fb),
      appBar: AppBar(
        title: Text(
          widget.initialPropertyTitle != null
              ? 'Maintenance: ${widget.initialPropertyTitle}'
              : 'Maintenance Hub',
          style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18),
        ),
        backgroundColor: Colors.white,
        foregroundColor: charcoal,
        elevation: 0,
        actions: [
          IconButton(
            tooltip: 'Notifications',
            onPressed: _showNotifications,
            icon: Badge(
              isLabelVisible: notifications.isNotEmpty,
              label: Text(notifications.length.toString()),
              child: const Icon(Icons.notifications_outlined),
            ),
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: () async {
          await _loadData();
        },
        child: ListView(
          padding: const EdgeInsets.all(20),
          children: [
            if (_error != null)
              Container(
                margin: const EdgeInsets.only(bottom: 16),
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: Colors.red.shade50,
                  borderRadius: BorderRadius.circular(10),
                  border: Border.all(color: Colors.red.shade200),
                ),
                child: Row(
                  children: [
                    const Icon(Icons.error_outline, color: Colors.red),
                    const SizedBox(width: 8),
                    Expanded(
                      child: Text(
                        _error!,
                        style: const TextStyle(color: Colors.red, fontWeight: FontWeight.w500),
                      ),
                    ),
                  ],
                ),
              ),

            if (_loading)
              const Padding(
                padding: EdgeInsets.all(32),
                child: Center(child: CircularProgressIndicator(color: gold)),
              ),

            // Header Banner
            Row(
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text(
                        'Maintenance',
                        style: TextStyle(
                          fontSize: 26,
                          fontWeight: FontWeight.bold,
                          color: charcoal,
                        ),
                      ),
                      const SizedBox(height: 4),
                      Text(
                        'Request repairs and track progress',
                        style: TextStyle(
                          fontSize: 14,
                          color: Colors.grey.shade600,
                        ),
                      ),
                    ],
                  ),
                ),
                if (widget.initialPropertyTitle != null)
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
                    decoration: BoxDecoration(
                      color: gold.withOpacity(0.12),
                      borderRadius: BorderRadius.circular(20),
                      border: Border.all(color: gold),
                    ),
                    child: const Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(Icons.home_outlined, size: 14, color: gold),
                        SizedBox(width: 4),
                        Text(
                          'Rent House',
                          style: TextStyle(fontSize: 12, fontWeight: FontWeight.bold, color: gold),
                        ),
                      ],
                    ),
                  ),
              ],
            ),

            const SizedBox(height: 20),

            // Statistics
            Row(
              children: [
                Expanded(
                  child: _buildStatCard(
                    title: 'Total',
                    value: requests.length.toString(),
                    icon: Icons.build_circle_outlined,
                    color: Colors.indigo,
                  ),
                ),
                const SizedBox(width: 10),
                Expanded(
                  child: _buildStatCard(
                    title: 'Pending',
                    value: pendingCount.toString(),
                    icon: Icons.pending_actions,
                    color: Colors.orange,
                  ),
                ),
                const SizedBox(width: 10),
                Expanded(
                  child: _buildStatCard(
                    title: 'In Progress',
                    value: inProgressCount.toString(),
                    icon: Icons.engineering_outlined,
                    color: Colors.deepPurple,
                  ),
                ),
              ],
            ),

            const SizedBox(height: 24),

            // CREATE MAINTENANCE REQUEST BUTTON - ALWAYS ACTIVE
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: () async {
                  final created = await Navigator.push<bool>(
                    context,
                    MaterialPageRoute(
                      builder: (context) => CreateRequestScreen(
                        tenantId: widget.tenantId,
                        activeRentals: activeRentals,
                        initialPropertyId: widget.initialPropertyId,
                      ),
                    ),
                  );
                  if (created == true) await _loadData();
                },
                icon: const Icon(Icons.add_circle_outline, size: 20),
                label: const Text(
                  'Create Maintenance Request',
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                style: ElevatedButton.styleFrom(
                  backgroundColor: gold,
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(vertical: 16),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                  elevation: 2,
                ),
              ),
            ),

            const SizedBox(height: 28),

            // Section title
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                const Text(
                  'My Maintenance Requests',
                  style: TextStyle(
                    fontSize: 18,
                    fontWeight: FontWeight.bold,
                    color: charcoal,
                  ),
                ),
                Text(
                  '${requests.length} found',
                  style: TextStyle(fontSize: 13, color: Colors.grey.shade600),
                ),
              ],
            ),

            const SizedBox(height: 12),

            if (requests.isEmpty && !_loading)
              Container(
                padding: const EdgeInsets.all(32),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(16),
                  border: Border.all(color: Colors.black12),
                ),
                child: Column(
                  children: [
                    const Icon(
                      Icons.assignment_outlined,
                      size: 54,
                      color: Colors.grey,
                    ),
                    const SizedBox(height: 12),
                    const Text(
                      'No maintenance requests yet',
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                        color: charcoal,
                      ),
                    ),
                    const SizedBox(height: 6),
                    Text(
                      'Tap the button above to report a problem in your rented house.',
                      textAlign: TextAlign.center,
                      style: TextStyle(fontSize: 13, color: Colors.grey.shade600),
                    ),
                  ],
                ),
              )
            else
              ...requests.map(
                (request) => _buildRequestCard(request),
              ),
          ],
        ),
      ),
    );
  }

  Widget _buildStatCard({
    required String title,
    required String value,
    required IconData icon,
    required Color color,
  }) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(14),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.04),
            blurRadius: 8,
            offset: const Offset(0, 3),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icon, color: color, size: 24),
          const SizedBox(height: 8),
          Text(
            value,
            style: const TextStyle(
              fontSize: 22,
              fontWeight: FontWeight.bold,
            ),
          ),
          Text(
            title,
            style: TextStyle(
              color: Colors.grey.shade600,
              fontSize: 12,
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _showNotifications() async {
    try {
      final latestNotifications = await _api.getNotifications(widget.tenantId);
      if (!mounted) return;
      setState(() => notifications = latestNotifications);
    } catch (_) {
      // Keep existing notifications on network fail
    }
    if (!mounted) return;

    await showModalBottomSheet<void>(
      context: context,
      showDragHandle: true,
      backgroundColor: Colors.white,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (context) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 10, 20, 14),
              child: Row(
                children: [
                  const Icon(Icons.notifications_active_outlined, color: Color(0xFFCF9E3E)),
                  const SizedBox(width: 8),
                  const Text(
                    'Maintenance Notifications',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                  const Spacer(),
                  Text(
                    '${notifications.length}',
                    style: const TextStyle(
                      fontWeight: FontWeight.bold,
                      color: Color(0xFFCF9E3E),
                    ),
                  ),
                ],
              ),
            ),
            const Divider(height: 1),
            if (notifications.isEmpty)
              const Padding(
                padding: EdgeInsets.all(40),
                child: Center(
                  child: Column(
                    children: [
                      Icon(Icons.notifications_none, size: 48, color: Colors.grey),
                      SizedBox(height: 12),
                      Text('No new notifications', style: TextStyle(color: Colors.grey)),
                    ],
                  ),
                ),
              )
            else
              Flexible(
                child: ListView.separated(
                  shrinkWrap: true,
                  itemCount: notifications.length,
                  separatorBuilder: (context, index) => const Divider(height: 1),
                  itemBuilder: (context, index) {
                    final n = notifications[index];
                    return ListTile(
                      leading: const CircleAvatar(
                        backgroundColor: Color(0xFFFFF7E6),
                        child: Icon(Icons.build_circle_outlined, color: Color(0xFFCF9E3E)),
                      ),
                      title: Text(
                        n['title']?.toString() ?? 'Notification',
                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                      ),
                      subtitle: Text(
                        n['message']?.toString() ?? '',
                        style: const TextStyle(fontSize: 13),
                      ),
                      trailing: Text(
                        n['createdAt'] != null
                            ? DateTime.tryParse(n['createdAt'].toString())?.toLocal().toString().substring(0, 10) ?? ''
                            : '',
                        style: const TextStyle(fontSize: 11, color: Colors.grey),
                      ),
                    );
                  },
                ),
              ),
          ],
        ),
      ),
    );
  }

  Widget _buildRequestCard(Map<String, dynamic> request) {
    final priority = request['priority']?.toString() ?? 'MEDIUM';
    final status = request['status']?.toString() ?? 'PENDING';
    final imageUrl = request['imageUrl']?.toString();

    return Container(
      margin: const EdgeInsets.only(bottom: 14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.04),
            blurRadius: 10,
            offset: const Offset(0, 3),
          ),
        ],
      ),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Row(
                  children: [
                    Text(
                      'Request #${request['id']}',
                      style: TextStyle(
                        fontWeight: FontWeight.bold,
                        color: Colors.grey.shade700,
                        fontSize: 13,
                      ),
                    ),
                    if (request['propertyId'] != null) ...[
                      const SizedBox(width: 8),
                      Text(
                        '• House #${request['propertyId']}',
                        style: const TextStyle(fontSize: 12, color: Colors.grey),
                      ),
                    ],
                  ],
                ),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    color: getStatusColor(status).withOpacity(0.12),
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: Text(
                    status,
                    style: TextStyle(
                      color: getStatusColor(status),
                      fontSize: 11,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
              ],
            ),

            const SizedBox(height: 10),

            Text(
              request['description'] ?? '',
              style: const TextStyle(
                fontSize: 15,
                fontWeight: FontWeight.w600,
              ),
            ),

            if (imageUrl != null && imageUrl.isNotEmpty) ...[
              const SizedBox(height: 10),
              ClipRRect(
                borderRadius: BorderRadius.circular(10),
                child: Image.network(
                  imageUrl,
                  height: 120,
                  width: double.infinity,
                  fit: BoxFit.cover,
                  errorBuilder: (context, error, stackTrace) => const SizedBox.shrink(),
                ),
              ),
            ],

            const SizedBox(height: 12),

            Row(
              children: [
                Icon(
                  Icons.category_outlined,
                  size: 16,
                  color: Colors.grey.shade600,
                ),
                const SizedBox(width: 6),
                Text(
                  request['category'] ?? 'General',
                  style: TextStyle(
                    color: Colors.grey.shade700,
                    fontSize: 13,
                  ),
                ),
                const Spacer(),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                  decoration: BoxDecoration(
                    color: getPriorityColor(priority).withOpacity(0.12),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Icon(
                        Icons.flag,
                        size: 12,
                        color: getPriorityColor(priority),
                      ),
                      const SizedBox(width: 4),
                      Text(
                        priority,
                        style: TextStyle(
                          color: getPriorityColor(priority),
                          fontSize: 11,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}