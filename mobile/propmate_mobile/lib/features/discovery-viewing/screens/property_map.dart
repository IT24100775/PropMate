import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';

import '../models/property.dart';

class PropertyMapPage extends StatelessWidget {
  final Property property;

  const PropertyMapPage({
    super.key,
    required this.property,
  });

  static const Color _background = Color(0xFFF6F4EF);
  static const Color _dark = Color(0xFF181817);
  static const Color _gold = Color(0xFFCFA03C);
  static const Color _mutedText = Color(0xFF777168);
  static const Color _border = Color(0xFFDEDAD1);

  @override
  Widget build(BuildContext context) {
    final latitude = property.latitude;
    final longitude = property.longitude;

    final hasLocation = latitude != null && longitude != null;

    return Scaffold(
      backgroundColor: _background,
      appBar: AppBar(
        backgroundColor: _dark,
        foregroundColor: Colors.white,
        elevation: 0,
        iconTheme: const IconThemeData(
          color: _gold,
        ),
        title: const Text(
          'Property Location',
          style: TextStyle(
            fontSize: 17,
            fontWeight: FontWeight.w600,
          ),
        ),
      ),
      body: !hasLocation
          ? const Center(
              child: Padding(
                padding: EdgeInsets.all(24),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(
                      Icons.location_off_outlined,
                      size: 55,
                      color: _gold,
                    ),
                    SizedBox(height: 16),
                    Text(
                      'Location unavailable',
                      style: TextStyle(
                        color: _dark,
                        fontSize: 20,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                    SizedBox(height: 8),
                    Text(
                      'This property does not have map coordinates.',
                      textAlign: TextAlign.center,
                      style: TextStyle(
                        color: _mutedText,
                        fontSize: 14,
                      ),
                    ),
                  ],
                ),
              ),
            )
          : Column(
              children: [
                Expanded(
                  child: FlutterMap(
                    options: MapOptions(
                      initialCenter: LatLng(latitude, longitude),
                      initialZoom: 15,
                    ),
                    children: [
                      TileLayer(
                        urlTemplate:
                            'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
                        userAgentPackageName: 'com.propmate.mobile',
                      ),
                      MarkerLayer(
                        markers: [
                          Marker(
                            point: LatLng(latitude, longitude),
                            width: 50,
                            height: 50,
                            child: const Icon(
                              Icons.location_on,
                              color: _gold,
                              size: 48,
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),

                Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(20),
                  decoration: const BoxDecoration(
                    color: _background,
                    border: Border(
                      top: BorderSide(
                        color: _border,
                      ),
                    ),
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        property.title,
                        style: const TextStyle(
                          color: _dark,
                          fontSize: 18,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                      const SizedBox(height: 7),
                      Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const Icon(
                            Icons.location_on_outlined,
                            color: _gold,
                            size: 18,
                          ),
                          const SizedBox(width: 7),
                          Expanded(
                            child: Text(
                              _locationText(),
                              style: const TextStyle(
                                color: _mutedText,
                                fontSize: 13,
                                height: 1.4,
                              ),
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              ],
            ),
    );
  }

  String _locationText() {
    final parts = <String>[];

    final address = property.address?.trim();
    final city = property.city?.trim();

    if (address != null && address.isNotEmpty) {
        parts.add(address);
    }

    if (city != null && city.isNotEmpty) {
        parts.add(city);
    }

    if (parts.isEmpty) {
        return 'Property location';
    }

    return parts.join(', ');
  }
}