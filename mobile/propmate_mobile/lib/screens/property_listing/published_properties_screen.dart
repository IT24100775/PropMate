import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../models/property_listing.dart';
import '../../services/maintenance_api.dart';
import '../../services/property_listing_service.dart';
import '../../widgets/property_card.dart';
import '../maintenance_home.dart';
import 'property_details_screen.dart';

class PublishedPropertiesScreen extends StatefulWidget {
  final int tenantId;

  const PublishedPropertiesScreen({
    super.key,
    this.tenantId = 1,
  });

  @override
  State<PublishedPropertiesScreen> createState() =>
      _PublishedPropertiesScreenState();
}

class _PublishedPropertiesScreenState
    extends State<PublishedPropertiesScreen> {
  final PropertyListingService _service = PropertyListingService();
  final MaintenanceApi _maintenanceApi = MaintenanceApi();

  late Future<List<PropertyListing>> _properties;
  List<Map<String, dynamic>> _notifications = [];

  @override
  void initState() {
    super.initState();
    _loadProperties();
    _loadNotifications();
  }

  void _loadProperties() {
    _properties = _service.getPublishedProperties();
  }

  Future<void> _loadNotifications() async {
    try {
      final notifs = await _maintenanceApi.getNotifications(widget.tenantId);
      if (mounted) {
        setState(() => _notifications = notifs);
      }
    } catch (_) {
      // Graceful fallback
    }
  }

  Future<void> _refreshProperties() async {
    setState(() {
      _loadProperties();
    });
    await Future.wait([
      _properties,
      _loadNotifications(),
    ]);
  }

  void _openProperty(PropertyListing property) {
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (_) => PropertyDetailsScreen(
          propertyId: property.id,
        ),
      ),
    );
  }

  void _openMaintenance(PropertyListing property) {
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (_) => MaintenanceHome(
          tenantId: widget.tenantId,
          initialPropertyId: property.id,
          initialPropertyTitle: property.title,
        ),
      ),
    );
  }

  Future<void> _showNotifications() async {
    await _loadNotifications();
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
                    '${_notifications.length}',
                    style: const TextStyle(
                      fontWeight: FontWeight.bold,
                      color: Color(0xFFCF9E3E),
                    ),
                  ),
                ],
              ),
            ),
            const Divider(height: 1),
            if (_notifications.isEmpty)
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
                  itemCount: _notifications.length,
                  separatorBuilder: (context, index) => const Divider(height: 1),
                  itemBuilder: (context, index) {
                    final n = _notifications[index];
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

  @override
  Widget build(BuildContext context) {
    const charcoal = Color(0xFF181817);
    const gold = Color(0xFFCF9E3E);

    return Scaffold(
      // PropMate navigation bar
      appBar: AppBar(
        backgroundColor: charcoal,
        foregroundColor: Colors.white,
        elevation: 0,
        titleSpacing: 16,
        title: Image.asset(
          'assets/images/logo-white.png',
          height: 52,
          fit: BoxFit.contain,
        ),
        actions: [
          IconButton(
            tooltip: 'Notifications',
            onPressed: _showNotifications,
            icon: Badge(
              isLabelVisible: _notifications.isNotEmpty,
              label: Text(_notifications.length.toString()),
              child: const Icon(Icons.notifications_outlined, color: Colors.white),
            ),
          ),
          const SizedBox(width: 8),
        ],
      ),

      body: SafeArea(
        child: RefreshIndicator(
          onRefresh: _refreshProperties,
          color: gold,
          child: FutureBuilder<List<PropertyListing>>(
            future: _properties,
            builder: (context, snapshot) {
              // Loading
              if (snapshot.connectionState == ConnectionState.waiting) {
                return const Center(
                  child: CircularProgressIndicator(
                    color: gold,
                  ),
                );
              }

              // Error
              if (snapshot.hasError) {
                return ListView(
                  physics: const AlwaysScrollableScrollPhysics(),
                  padding: const EdgeInsets.all(24),
                  children: [
                    const SizedBox(height: 100),
                    const Icon(
                      Icons.cloud_off_outlined,
                      size: 60,
                      color: Colors.grey,
                    ),
                    const SizedBox(height: 18),
                    const Text(
                      'Unable to load properties',
                      textAlign: TextAlign.center,
                      style: TextStyle(
                        color: charcoal,
                        fontSize: 20,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      '${snapshot.error}',
                      textAlign: TextAlign.center,
                      style: const TextStyle(
                        color: Colors.grey,
                      ),
                    ),
                    const SizedBox(height: 20),
                    Center(
                      child: ElevatedButton(
                        onPressed: () {
                          setState(() {
                            _loadProperties();
                          });
                        },
                        child: const Text('Try Again'),
                      ),
                    ),
                  ],
                );
              }

              final properties = snapshot.data ?? [];

              // Empty
              if (properties.isEmpty) {
                return ListView(
                  physics: const AlwaysScrollableScrollPhysics(),
                  padding: const EdgeInsets.all(24),
                  children: const [
                    SizedBox(height: 120),
                    Icon(
                      Icons.home_work_outlined,
                      size: 65,
                      color: Colors.grey,
                    ),
                    SizedBox(height: 18),
                    Text(
                      'No properties available',
                      textAlign: TextAlign.center,
                      style: TextStyle(
                        color: charcoal,
                        fontSize: 20,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                    SizedBox(height: 8),
                    Text(
                      'Published properties will appear here.',
                      textAlign: TextAlign.center,
                      style: TextStyle(
                        color: Colors.grey,
                      ),
                    ),
                  ],
                );
              }

              // Published properties
              return ListView(
                physics: const AlwaysScrollableScrollPhysics(),
                padding: const EdgeInsets.fromLTRB(
                  18,
                  22,
                  18,
                  30,
                ),
                children: [
                  Text(
                    'Find Your Next Property',
                    style: GoogleFonts.playfairDisplay(
                      color: charcoal,
                      fontSize: 30,
                      fontWeight: FontWeight.w700,
                    ),
                  ),

                  const SizedBox(height: 6),

                  const Text(
                    'Discover verified properties available for sale and rent.',
                    style: TextStyle(
                      color: Colors.grey,
                      fontSize: 14,
                    ),
                  ),

                  const SizedBox(height: 7),

                  // Property count
                  Text(
                    '${properties.length} published '
                    '${properties.length == 1 ? 'property' : 'properties'}',
                    style: const TextStyle(
                      color: gold,
                      fontSize: 13,
                      fontWeight: FontWeight.w600,
                    ),
                  ),

                  const SizedBox(height: 22),

                  // Property cards
                  ...properties.map(
                    (property) => PropertyCard(
                      property: property,
                      onTap: () => _openProperty(property),
                      onMaintenanceTap: () => _openMaintenance(property),
                    ),
                  ),
                ],
              );
            },
          ),
        ),
      ),
    );
  }
}