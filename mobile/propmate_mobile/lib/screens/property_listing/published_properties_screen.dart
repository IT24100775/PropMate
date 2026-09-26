import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../models/property_listing.dart';
import '../../services/property_listing_service.dart';
import '../../widgets/property_card.dart';
import 'property_details_screen.dart';

class PublishedPropertiesScreen extends StatefulWidget {
  const PublishedPropertiesScreen({super.key});

  @override
  State<PublishedPropertiesScreen> createState() =>
      _PublishedPropertiesScreenState();
}

class _PublishedPropertiesScreenState
    extends State<PublishedPropertiesScreen> {
  final PropertyListingService _service = PropertyListingService();

  late Future<List<PropertyListing>> _properties;

  @override
  void initState() {
    super.initState();
    _loadProperties();
  }

  void _loadProperties() {
    _properties = _service.getPublishedProperties();
  }

  Future<void> _refreshProperties() async {
    setState(() {
      _loadProperties();
    });

    await _properties;
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