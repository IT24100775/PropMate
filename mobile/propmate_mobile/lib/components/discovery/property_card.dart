import 'package:flutter/material.dart';

import '../../models/property.dart';
import 'favourite_button.dart';

class PropertyCard extends StatelessWidget {
  final Property property;
  final VoidCallback? onTap;

  const PropertyCard({super.key, required this.property, this.onTap});

  @override
  Widget build(BuildContext context) {
    return Card(
      clipBehavior: Clip.antiAlias,
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(12),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Expanded(
                    child: Text(
                      property.title,
                      style: const TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                  FavouriteButton(propertyId: property.id, userId: 1),
                ],
              ),
              const SizedBox(height: 8),
              Row(
                children: [
                  const Icon(Icons.location_on, size: 18),
                  const SizedBox(width: 4),
                  Text(property.location),
                ],
              ),
              const SizedBox(height: 8),
              Text(
                'Rs. ${property.price.toStringAsFixed(0)}',
                style: const TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 8),
              Row(
                children: [
                  Text('${property.bedrooms} Bedrooms'),
                  const SizedBox(width: 16),
                  Text('${property.bathrooms} Bathrooms'),
                ],
              ),
              const SizedBox(height: 8),
              Text(
                property.isAvailable ? 'Available' : 'Not Available',
                style: TextStyle(
                  fontWeight: FontWeight.bold,
                  color: property.isAvailable ? Colors.green : Colors.red,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
