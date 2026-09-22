import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../models/property_listing.dart';
import '../../services/property_listing_service.dart';

class PropertyDetailsScreen extends StatefulWidget {
  final int propertyId;

  const PropertyDetailsScreen({
    super.key,
    required this.propertyId,
  });

  @override
  State<PropertyDetailsScreen> createState() =>
      _PropertyDetailsScreenState();
}

class _PropertyDetailsScreenState extends State<PropertyDetailsScreen> {
  final PropertyListingService _service = PropertyListingService();

  late Future<PropertyListing> _property;

  @override
  void initState() {
    super.initState();
    _property = _service.getPropertyById(widget.propertyId);
  }

  String _formatPrice(double price) {
    return 'LKR ${price.toStringAsFixed(0).replaceAllMapped(
      RegExp(r'(\d)(?=(\d{3})+(?!\d))'),
      (match) => '${match[1]},',
    )}';
  }

  void _retry() {
    setState(() {
      _property = _service.getPropertyById(widget.propertyId);
    });
  }

  @override
  Widget build(BuildContext context) {
    const gold = Color(0xFFCF9E3E);
    const charcoal = Color(0xFF181817);
    const cream = Color(0xFFF7F5F0);

    return Scaffold(
      backgroundColor: cream,
      appBar: AppBar(
        backgroundColor: charcoal,
        foregroundColor: Colors.white,
        elevation: 0,
        titleSpacing: 8,
        title: Row(
            children: [
            Image.asset(
                'assets/images/logo-full.png',
                height: 34,
                fit: BoxFit.contain,
            ),
            const SizedBox(width: 14),
            Container(
                width: 1,
                height: 24,
                color: Colors.white24,
            ),
            const SizedBox(width: 14),
            Text(
                'Property Details',
                style: GoogleFonts.dmSans(
                color: Colors.white,
                fontSize: 16,
                fontWeight: FontWeight.w600,
                ),
            ),
            ],
        ),
        ),
      body: FutureBuilder<PropertyListing>(
        future: _property,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(
              child: CircularProgressIndicator(
                color: gold,
              ),
            );
          }

          if (snapshot.hasError) {
            return Center(
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Icon(
                      Icons.error_outline,
                      size: 58,
                      color: Colors.grey,
                    ),
                    const SizedBox(height: 16),
                    Text(
                      'Unable to load property',
                      style: GoogleFonts.playfairDisplay(
                        color: charcoal,
                        fontSize: 22,
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
                    ElevatedButton(
                      onPressed: _retry,
                      child: const Text('Try Again'),
                    ),
                  ],
                ),
              ),
            );
          }

          final property = snapshot.data!;

          final hasImage = property.imageUrls.isNotEmpty &&
              property.imageUrls.first.trim().isNotEmpty;

          return SingleChildScrollView(
            child: Center(
              child: ConstrainedBox(
                constraints: const BoxConstraints(
                  maxWidth: 850,
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Property image
                    ClipRRect(
                      borderRadius: const BorderRadius.only(
                        bottomLeft: Radius.circular(20),
                        bottomRight: Radius.circular(20),
                      ),
                      child: SizedBox(
                        width: double.infinity,
                        height: 330,
                        child: hasImage
                            ? Image.network(
                                property.imageUrls.first,
                                fit: BoxFit.cover,
                                errorBuilder:
                                    (context, error, stackTrace) =>
                                        _imagePlaceholder(),
                              )
                            : _imagePlaceholder(),
                      ),
                    ),

                    Padding(
                      padding: const EdgeInsets.fromLTRB(
                        22,
                        24,
                        22,
                        36,
                      ),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          // Sale/Rent + Property Type
                          Wrap(
                            spacing: 8,
                            runSpacing: 8,
                            children: [
                              _badge(
                                property.purpose,
                                filled: true,
                              ),
                              _badge(
                                property.propertyType,
                                filled: false,
                              ),
                            ],
                          ),

                          const SizedBox(height: 18),

                          // Property title
                          Text(
                            property.title,
                            style: GoogleFonts.playfairDisplay(
                              color: charcoal,
                              fontSize: 30,
                              fontWeight: FontWeight.w700,
                              height: 1.2,
                            ),
                          ),

                          const SizedBox(height: 12),

                          // Location
                          Row(
                            crossAxisAlignment:
                                CrossAxisAlignment.start,
                            children: [
                              const Icon(
                                Icons.location_on_outlined,
                                size: 20,
                                color: gold,
                              ),
                              const SizedBox(width: 6),
                              Expanded(
                                child: Text(
                                  '${property.address}, ${property.city}',
                                  style: const TextStyle(
                                    color: Color(0xFF666666),
                                    fontSize: 15,
                                  ),
                                ),
                              ),
                            ],
                          ),

                          const SizedBox(height: 24),

                          // Price
                          Text(
                            _formatPrice(property.price),
                            style: const TextStyle(
                              color: gold,
                              fontSize: 28,
                              fontWeight: FontWeight.w800,
                            ),
                          ),

                          if (property.purpose.toLowerCase() ==
                              'rent')
                            const Padding(
                              padding: EdgeInsets.only(top: 3),
                              child: Text(
                                'per month',
                                style: TextStyle(
                                  color: Colors.grey,
                                  fontSize: 13,
                                ),
                              ),
                            ),

                          const SizedBox(height: 26),

                          // Property information card
                          Container(
                            width: double.infinity,
                            padding: const EdgeInsets.symmetric(
                              vertical: 20,
                              horizontal: 16,
                            ),
                            decoration: BoxDecoration(
                              color: Colors.white,
                              borderRadius:
                                  BorderRadius.circular(16),
                              border: Border.all(
                                color: const Color(0xFFE7E2D8),
                              ),
                              boxShadow: [
                                BoxShadow(
                                  color:
                                      Colors.black.withValues(
                                    alpha: 0.04,
                                  ),
                                  blurRadius: 12,
                                  offset: const Offset(0, 4),
                                ),
                              ],
                            ),
                            child: Row(
                              children: [
                                Expanded(
                                  child: _feature(
                                    Icons.bed_outlined,
                                    '${property.bedrooms}',
                                    'Bedrooms',
                                  ),
                                ),

                                Container(
                                  height: 48,
                                  width: 1,
                                  color: Colors.grey.shade300,
                                ),

                                Expanded(
                                  child: _feature(
                                    Icons.bathtub_outlined,
                                    '${property.bathrooms}',
                                    'Bathrooms',
                                  ),
                                ),

                                Container(
                                  height: 48,
                                  width: 1,
                                  color: Colors.grey.shade300,
                                ),

                                Expanded(
                                  child: _feature(
                                    Icons.home_work_outlined,
                                    property.propertyType,
                                    'Property Type',
                                  ),
                                ),
                              ],
                            ),
                          ),

                          const SizedBox(height: 32),

                          // About
                          Text(
                            'About this property',
                            style: GoogleFonts.playfairDisplay(
                              color: charcoal,
                              fontSize: 23,
                              fontWeight: FontWeight.w700,
                            ),
                          ),

                          const SizedBox(height: 8),

                          Container(
                            width: 45,
                            height: 3,
                            decoration: BoxDecoration(
                              color: gold,
                              borderRadius:
                                  BorderRadius.circular(10),
                            ),
                          ),

                          const SizedBox(height: 16),

                          Text(
                            property.description,
                            style: const TextStyle(
                              color: Color(0xFF555555),
                              fontSize: 15,
                              height: 1.7,
                            ),
                          ),

                          const SizedBox(height: 32),

                          // Verification message
                          Container(
                            width: double.infinity,
                            padding: const EdgeInsets.all(16),
                            decoration: BoxDecoration(
                              color: const Color(0xFFFFF9ED),
                              borderRadius:
                                  BorderRadius.circular(12),
                              border: Border.all(
                                color: gold.withValues(
                                  alpha: 0.35,
                                ),
                              ),
                            ),
                            child: const Row(
                              crossAxisAlignment:
                                  CrossAxisAlignment.start,
                              children: [
                                Icon(
                                  Icons.verified_outlined,
                                  color: gold,
                                  size: 22,
                                ),
                                SizedBox(width: 10),
                                Expanded(
                                  child: Text(
                                    'This property has completed the PropMate listing review process.',
                                    style: TextStyle(
                                      color: Color(0xFF625535),
                                      fontSize: 13,
                                      height: 1.4,
                                    ),
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ),
          );
        },
      ),
    );
  }

  Widget _badge(
    String text, {
    required bool filled,
  }) {
    const gold = Color(0xFFCF9E3E);

    return Container(
      padding: const EdgeInsets.symmetric(
        horizontal: 12,
        vertical: 6,
      ),
      decoration: BoxDecoration(
        color: filled ? gold : Colors.transparent,
        border: Border.all(
          color: gold,
        ),
        borderRadius: BorderRadius.circular(20),
      ),
      child: Text(
        text,
        style: TextStyle(
          color: filled ? Colors.white : gold,
          fontSize: 12,
          fontWeight: FontWeight.w700,
        ),
      ),
    );
  }

  Widget _feature(
    IconData icon,
    String value,
    String label,
  ) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(
          icon,
          size: 25,
          color: const Color(0xFFCF9E3E),
        ),
        const SizedBox(height: 7),
        Text(
          value,
          textAlign: TextAlign.center,
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
          style: const TextStyle(
            color: Color(0xFF181817),
            fontSize: 16,
            fontWeight: FontWeight.w700,
          ),
        ),
        const SizedBox(height: 3),
        Text(
          label,
          textAlign: TextAlign.center,
          style: const TextStyle(
            color: Colors.grey,
            fontSize: 11,
          ),
        ),
      ],
    );
  }

  Widget _imagePlaceholder() {
    return Container(
      color: const Color(0xFFE9E6DF),
      child: const Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(
              Icons.home_work_outlined,
              size: 60,
              color: Color(0xFF9B978F),
            ),
            SizedBox(height: 8),
            Text(
              'Property image unavailable',
              style: TextStyle(
                color: Color(0xFF8A867E),
                fontSize: 13,
              ),
            ),
          ],
        ),
      ),
    );
  }
}