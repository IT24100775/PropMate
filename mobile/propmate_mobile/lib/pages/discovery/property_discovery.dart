import 'dart:async';

import 'package:flutter/material.dart';

import '../../models/property.dart';
import '../../services/property_service.dart';
import '../../components/discovery/property_card.dart';
import 'property_details.dart';

class PropertyDiscoveryPage extends StatefulWidget {
  const PropertyDiscoveryPage({super.key});

  @override
  State<PropertyDiscoveryPage> createState() => _PropertyDiscoveryPageState();
}

class _PropertyDiscoveryPageState extends State<PropertyDiscoveryPage> {
  final PropertyService _propertyService = PropertyService();

  final TextEditingController _searchController = TextEditingController();

  final TextEditingController _minPriceController = TextEditingController();

  final TextEditingController _maxPriceController = TextEditingController();

  int? _selectedBedrooms;
  int? _selectedBathrooms;

  late Future<List<Property>> _properties;

  Timer? _searchTimer;

  @override
  void initState() {
    super.initState();
    _loadProperties();
  }

  void _loadProperties() {
    final minPrice = double.tryParse(_minPriceController.text.trim());

    final maxPrice = double.tryParse(_maxPriceController.text.trim());

    _properties = _propertyService.getProperties(
      search: _searchController.text,
      minPrice: minPrice,
      maxPrice: maxPrice,
      bedrooms: _selectedBedrooms,
      bathrooms: _selectedBathrooms,
    );
  }

  void _searchProperties(String value) {
    _searchTimer?.cancel();

    _searchTimer = Timer(const Duration(milliseconds: 500), () {
      setState(() {
        _loadProperties();
      });
    });
  }

  void _applyFilters() {
    setState(() {
      _loadProperties();
    });
  }

  void _clearFilters() {
    _searchTimer?.cancel();

    _searchController.clear();
    _minPriceController.clear();
    _maxPriceController.clear();

    setState(() {
      _selectedBedrooms = null;
      _selectedBathrooms = null;
      _loadProperties();
    });
  }

  Future<void> _refreshProperties() async {
    setState(() {
      _loadProperties();
    });

    await _properties;
  }

  @override
  void dispose() {
    _searchTimer?.cancel();
    _searchController.dispose();
    _minPriceController.dispose();
    _maxPriceController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Property Discovery')),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              children: [
                // Search
                TextField(
                  controller: _searchController,
                  onChanged: _searchProperties,
                  decoration: InputDecoration(
                    hintText: 'Search properties...',
                    prefixIcon: const Icon(Icons.search),
                    suffixIcon: _searchController.text.isNotEmpty
                        ? IconButton(
                            icon: const Icon(Icons.clear),
                            onPressed: () {
                              _searchController.clear();

                              setState(() {
                                _loadProperties();
                              });
                            },
                          )
                        : null,
                    border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12),
                    ),
                  ),
                ),

                const SizedBox(height: 12),

                // Price filters
                Row(
                  children: [
                    Expanded(
                      child: TextField(
                        controller: _minPriceController,
                        keyboardType: TextInputType.number,
                        decoration: InputDecoration(
                          labelText: 'Min Price',
                          hintText: '30000000',
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(12),
                          ),
                        ),
                      ),
                    ),

                    const SizedBox(width: 12),

                    Expanded(
                      child: TextField(
                        controller: _maxPriceController,
                        keyboardType: TextInputType.number,
                        decoration: InputDecoration(
                          labelText: 'Max Price',
                          hintText: '50000000',
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(12),
                          ),
                        ),
                      ),
                    ),
                  ],
                ),

                const SizedBox(height: 12),

                // Bedrooms and bathrooms
                Row(
                  children: [
                    Expanded(
                      child: DropdownButtonFormField<int?>(
                        initialValue: _selectedBedrooms,
                        decoration: InputDecoration(
                          labelText: 'Bedrooms',
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(12),
                          ),
                        ),
                        items: [
                          const DropdownMenuItem<int?>(
                            value: null,
                            child: Text('Any'),
                          ),
                          ...List.generate(
                            5,
                            (index) => DropdownMenuItem<int?>(
                              value: index + 1,
                              child: Text('${index + 1}+'),
                            ),
                          ),
                        ],
                        onChanged: (value) {
                          setState(() {
                            _selectedBedrooms = value;
                          });
                        },
                      ),
                    ),

                    const SizedBox(width: 12),

                    Expanded(
                      child: DropdownButtonFormField<int?>(
                        initialValue: _selectedBathrooms,
                        decoration: InputDecoration(
                          labelText: 'Bathrooms',
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(12),
                          ),
                        ),
                        items: [
                          const DropdownMenuItem<int?>(
                            value: null,
                            child: Text('Any'),
                          ),
                          ...List.generate(
                            5,
                            (index) => DropdownMenuItem<int?>(
                              value: index + 1,
                              child: Text('${index + 1}+'),
                            ),
                          ),
                        ],
                        onChanged: (value) {
                          setState(() {
                            _selectedBathrooms = value;
                          });
                        },
                      ),
                    ),
                  ],
                ),

                const SizedBox(height: 12),

                // Buttons
                Row(
                  children: [
                    Expanded(
                      child: ElevatedButton.icon(
                        onPressed: _applyFilters,
                        icon: const Icon(Icons.filter_list),
                        label: const Text('Apply Filters'),
                      ),
                    ),

                    const SizedBox(width: 12),

                    OutlinedButton(
                      onPressed: _clearFilters,
                      child: const Text('Clear'),
                    ),
                  ],
                ),
              ],
            ),
          ),

          // Property list
          Expanded(
            child: RefreshIndicator(
              onRefresh: _refreshProperties,
              child: FutureBuilder<List<Property>>(
                future: _properties,
                builder: (context, snapshot) {
                  if (snapshot.connectionState == ConnectionState.waiting) {
                    return const Center(child: CircularProgressIndicator());
                  }

                  if (snapshot.hasError) {
                    return Center(
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          const Text('Failed to load properties'),
                          const SizedBox(height: 10),
                          ElevatedButton(
                            onPressed: () {
                              setState(() {
                                _loadProperties();
                              });
                            },
                            child: const Text('Retry'),
                          ),
                        ],
                      ),
                    );
                  }

                  final properties = snapshot.data ?? [];

                  if (properties.isEmpty) {
                    return const Center(child: Text('No properties found'));
                  }

                  return ListView.builder(
                    padding: const EdgeInsets.all(16),
                    itemCount: properties.length,
                    itemBuilder: (context, index) {
                      return PropertyCard(
                        property: properties[index],
                        onTap: () {
                          Navigator.push(
                            context,
                            MaterialPageRoute(
                              builder: (context) => PropertyDetailsPage(
                                property: properties[index],
                              ),
                            ),
                          );
                        },
                      );
                    },
                  );
                },
              ),
            ),
          ),
        ],
      ),
    );
  }
}
