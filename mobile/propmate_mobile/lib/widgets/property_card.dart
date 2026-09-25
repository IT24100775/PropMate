import 'package:flutter/material.dart';

import '../models/property_listing.dart';

class PropertyCard extends StatelessWidget {
  final PropertyListing property;
  final VoidCallback onTap;

  const PropertyCard({
    super.key,
    required this.property,
    required this.onTap,
  });

  String _formatPrice(double price) {
    return 'LKR ${price.toStringAsFixed(0).replaceAllMapped(
      RegExp(r'(\d)(?=(\d{3})+(?!\d))'),
      (match) => '${match[1]},',
    )}';
  }

  @override
  Widget build(BuildContext context) {
    const gold = Color(0xFFCF9E3E);
    const charcoal = Color(0xFF181817);

    final hasImage =
        property.imageUrls.isNotEmpty && property.imageUrls.first.isNotEmpty;

    return Card(
      margin: const EdgeInsets.only(bottom: 18),
      elevation: 1.5,
      clipBehavior: Clip.antiAlias,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16),
      ),
      child: InkWell(
        onTap: onTap,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            SizedBox(
              height: 210,
              width: double.infinity,
              child: hasImage
                  ? Image.network(
                      property.imageUrls.first,
                      fit: BoxFit.cover,
                      errorBuilder: (context, error, stackTrace) =>
                        const _ImagePlaceholder(),
                    )
                  : const _ImagePlaceholder(),
            ),

            Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      _Badge(text: property.purpose),
                      const SizedBox(width: 8),
                      _Badge(
                        text: property.propertyType,
                        outlined: true,
                      ),
                    ],
                  ),

                  const SizedBox(height: 12),

                  Text(
                    property.title,
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      color: charcoal,
                      fontSize: 19,
                      fontWeight: FontWeight.w700,
                    ),
                  ),

                  const SizedBox(height: 6),

                  Row(
                    children: [
                      const Icon(
                        Icons.location_on_outlined,
                        size: 17,
                        color: Colors.grey,
                      ),
                      const SizedBox(width: 4),
                      Expanded(
                        child: Text(
                          property.city,
                          style: const TextStyle(
                            color: Colors.grey,
                          ),
                        ),
                      ),
                    ],
                  ),

                  const SizedBox(height: 14),

                  Text(
                    _formatPrice(property.price),
                    style: const TextStyle(
                      color: gold,
                      fontSize: 20,
                      fontWeight: FontWeight.w800,
                    ),
                  ),

                  const SizedBox(height: 14),

                  Row(
                    children: [
                      const Icon(
                        Icons.bed_outlined,
                        size: 19,
                      ),
                      const SizedBox(width: 5),
                      Text('${property.bedrooms} Beds'),

                      const SizedBox(width: 20),

                      const Icon(
                        Icons.bathtub_outlined,
                        size: 18,
                      ),
                      const SizedBox(width: 5),
                      Text('${property.bathrooms} Baths'),
                    ],
                  ),
                        const SizedBox(height: 16),

                        const Divider(
                        height: 1,
                        ),

                        const SizedBox(height: 12),

                        const Row(
                        mainAxisAlignment: MainAxisAlignment.end,
                        children: [
                            Text(
                            'View Details',
                            style: TextStyle(
                                color: gold,
                                fontSize: 13,
                                fontWeight: FontWeight.w700,
                            ),
                            ),
                            SizedBox(width: 4),
                            Icon(
                            Icons.arrow_forward,
                            size: 16,
                            color: gold,
                            ),
                        ],
                        ),

                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _Badge extends StatelessWidget {
  final String text;
  final bool outlined;

  const _Badge({
    required this.text,
    this.outlined = false,
  });

  @override
  Widget build(BuildContext context) {
    const gold = Color(0xFFCF9E3E);

    return Container(
      padding: const EdgeInsets.symmetric(
        horizontal: 10,
        vertical: 5,
      ),
      decoration: BoxDecoration(
        color: outlined ? Colors.transparent : gold,
        border: Border.all(color: gold),
        borderRadius: BorderRadius.circular(20),
      ),
      child: Text(
        text,
        style: TextStyle(
          color: outlined ? gold : Colors.white,
          fontSize: 12,
          fontWeight: FontWeight.w600,
        ),
      ),
    );
  }
}

class _ImagePlaceholder extends StatelessWidget {
  const _ImagePlaceholder();

  @override
  Widget build(BuildContext context) {
    return Container(
      color: const Color(0xFFE9E6DF),
      child: const Center(
        child: Icon(
          Icons.home_work_outlined,
          size: 52,
          color: Colors.grey,
        ),
      ),
    );
  }
}