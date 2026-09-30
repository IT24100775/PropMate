import 'package:flutter/material.dart';
import '../../services/mobile_auth_service.dart';
import '../../screens/auth/login_screen.dart';
import '../../services/property_service.dart';
import '../../services/agent_service.dart';
import '../../models/property.dart';
import '../../models/agent_response.dart';
import 'property_details.dart';
import 'my_favourites.dart';
import 'my_viewings.dart';
import '../../screens/maintenance_home.dart';
import '../../services/favourite_service.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';
import 'package:geolocator/geolocator.dart';

class PropertyDiscoveryPage extends StatefulWidget {
  const PropertyDiscoveryPage({Key? key}) : super(key: key);
  @override
  _PropertyDiscoveryPageState createState() => _PropertyDiscoveryPageState();
}

class _PropertyDiscoveryPageState extends State<PropertyDiscoveryPage> {
  final PropertyService _service = PropertyService();
  final AgentService _agentService = AgentService();
  final FavouriteService _favService = FavouriteService();

  static const Color _background = Color(0xFFF6F4EF);
  static const Color _cardBackground = Color(0xFFFBFAF7);
  static const Color _dark = Color(0xFF181817);
  static const Color _gold = Color(0xFFCFA03C);
  static const Color _mutedText = Color(0xFF777168);
  static const Color _border = Color(0xFFDEDAD1);
  static const Color _danger = Color(0xFF8D4038);
  static const Color _success = Color(0xFF416747);
  
  List<Property> _properties = [];
  bool _loading = false;
  bool _loadingMore = false;
  int _page = 1;
  int _totalPages = 1;
  
  final TextEditingController _aiController = TextEditingController();
  bool _aiLoading = false;
  AgentResponse? _agentResponse;

  bool _isMapView = false;
  LatLng? _currentLocation;
  final MapController _mapController = MapController();

  final ScrollController _scrollController = ScrollController();
  
  // Filters & Search
  final TextEditingController _searchController = TextEditingController();
  final TextEditingController _cityController = TextEditingController();
  final TextEditingController _minPriceController = TextEditingController();
  final TextEditingController _maxPriceController = TextEditingController();
  final TextEditingController _bedroomsController = TextEditingController();
  final TextEditingController _bathroomsController = TextEditingController();
  int? _purpose;
  int? _propertyType;
  double? _minPrice;
  double? _maxPrice;
  int? _bedrooms;
  int? _bathrooms;
  String _sort = 'newest';
  
  // Favorites Cache
  final Set<int> _favoriteIds = {};
  int _favLoadCounter = 0;

  @override
  void initState() {
    super.initState();
    _scrollController.addListener(_onScroll);
    _loadFavorites();
    _loadProperties(refresh: true);
    _determinePosition();
  }
  
  Future<void> _determinePosition() async {
    bool serviceEnabled;
    LocationPermission permission;

    serviceEnabled = await Geolocator.isLocationServiceEnabled();
    if (!serviceEnabled) return;

    permission = await Geolocator.checkPermission();
    if (permission == LocationPermission.denied) {
      permission = await Geolocator.requestPermission();
      if (permission == LocationPermission.denied) return;
    }
    
    if (permission == LocationPermission.deniedForever) return;

    try {
      final pos = await Geolocator.getCurrentPosition();
      setState(() {
        _currentLocation = LatLng(pos.latitude, pos.longitude);
      });
    } catch (_) {}
  }
  
  @override
  void dispose() {
    _scrollController.dispose();
    _searchController.dispose();
    _cityController.dispose();
    _minPriceController.dispose();
    _maxPriceController.dispose();
    _bedroomsController.dispose();
    _bathroomsController.dispose();
    _aiController.dispose();
    super.dispose();
  }

  void _onScroll() {
    if (_scrollController.position.pixels >= _scrollController.position.maxScrollExtent - 200 && !_loading && !_loadingMore && _page < _totalPages) {
      _loadProperties(refresh: false);
    }
  }

  Future<void> _loadFavorites() async {
    final int currentLoad = ++_favLoadCounter;
    try {
      final Set<int> newFavs = {};
      // Load first 100 favorites basically to cache state
      for (int i = 1; i <= 5; i++) {
        if (currentLoad != _favLoadCounter) return;
        final res = await _favService.getFavourites(page: i);
        if (currentLoad != _favLoadCounter) return;
        newFavs.addAll(res.items.map((e) => e.id));
        if (res.page >= res.totalPages) break;
      }
      if (mounted && currentLoad == _favLoadCounter) {
        setState(() {
          _favoriteIds.clear();
          _favoriteIds.addAll(newFavs);
        });
      }
    } catch (_) {}
  }

  Future<void> _toggleFavorite(int currentId) async {
    _favLoadCounter++; // Abort any in-flight bulk reloads to protect our optimistic state
    final isFav = _favoriteIds.contains(currentId);
    if (mounted) {
      setState(() {
         if (isFav) _favoriteIds.remove(currentId);
         else _favoriteIds.add(currentId);
      });
    }
    try {
       bool success;
       if (isFav) {
          success = await _favService.removeFavourite(currentId);
       } else {
          success = await _favService.addFavourite(currentId);
       }
       if (!success && mounted) {
          // Revert on failure
          setState(() {
             if (isFav) _favoriteIds.add(currentId);
             else _favoriteIds.remove(currentId);
          });
       }
    } catch (_) {
       // Revert
       if (mounted) {
          setState(() {
             if (isFav) _favoriteIds.add(currentId);
             else _favoriteIds.remove(currentId);
          });
       }
    }
  }

  Future<void> _loadProperties({bool refresh = true}) async {
    if (refresh) {
      setState(() { _loading = true; _agentResponse = null; _page = 1; _properties.clear(); });
    } else {
      setState(() { _loadingMore = true; _page++; });
    }
    
    try {
      final res = await _service.getProperties(
        search: _searchController.text.trim(),
        city: _cityController.text.trim(),
        type: _propertyType,
        purpose: _purpose,
        minPrice: _minPrice,
        maxPrice: _maxPrice,
        bedrooms: _bedrooms,
        bathrooms: _bathrooms,
        sort: _sort,
        page: _page
      );
      if (mounted) {
        setState(() {
          if (refresh) {
            _properties = res.items;
          } else {
            _properties.addAll(res.items);
          }
          _totalPages = res.totalPages;
        });
      }
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
    } finally {
      if (mounted) setState(() { _loading = false; _loadingMore = false; });
    }
  }

  Future<void> _askAi() async {
    final q = _aiController.text.trim();
    if (q.isEmpty) return;
    
    setState(() { _aiLoading = true; _agentResponse = null; });
    
    try {
      final res = await _agentService.queryAgent(q);
      if (mounted) {
        setState(() {
          _agentResponse = res;
        });
      }
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
    } finally {
      if (mounted) setState(() { _aiLoading = false; });
    }
  }

  void _showFilterSheet() {
    // Sync controllers to the actual applied state so Cancel discards stale unapplied inputs
    _minPriceController.text = _minPrice != null ? _minPrice!.toStringAsFixed(0) : '';
    _maxPriceController.text = _maxPrice != null ? _maxPrice!.toStringAsFixed(0) : '';
    _bedroomsController.text = _bedrooms != null ? _bedrooms!.toString() : '';
    _bathroomsController.text = _bathrooms != null ? _bathrooms!.toString() : '';

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: _background,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.zero,),
      builder: (_) {
         return StatefulBuilder(
           builder: (ctx, setSheetState) {
             return Padding(
               padding: EdgeInsets.only(bottom: MediaQuery.of(ctx).viewInsets.bottom, left: 16, right: 16, top: 16),
               child: SingleChildScrollView(
                 child: Column(
                   mainAxisSize: MainAxisSize.min,
                   crossAxisAlignment: CrossAxisAlignment.stretch,
                   children: [
                     const Text(
                      'REFINE YOUR SEARCH',
                      style: TextStyle(
                        color: _gold,
                        fontSize: 10,
                        fontWeight: FontWeight.w700,
                        letterSpacing: 1.6,
                      ),
                    ),

                    const SizedBox(height: 7),

                    const Text(
                      'Filters & Sorting',
                      style: TextStyle(
                        color: _dark,
                        fontSize: 24,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                     const SizedBox(height: 16),
                     
                     // Sort
                     DropdownButtonFormField<String>(
                       value: _sort,
                       decoration: const InputDecoration(labelText: 'Sort By'),
                       items: const [
                         DropdownMenuItem(value: 'newest', child: Text('Newest')),
                         DropdownMenuItem(value: 'price-low-to-high', child: Text('Price: Low to High')),
                         DropdownMenuItem(value: 'price-high-to-low', child: Text('Price: High to Low')),
                       ],
                       onChanged: (val) => setSheetState(() => _sort = val!),
                     ),
                     const SizedBox(height: 12),
                     
                     // City
                     TextField(
                       controller: _cityController,
                       decoration: const InputDecoration(labelText: 'City/Location'),
                     ),
                     const SizedBox(height: 12),
                     
                     // Purpose
                     DropdownButtonFormField<int?>(
                       value: _purpose,
                       decoration: const InputDecoration(labelText: 'Purpose'),
                       items: const [
                         DropdownMenuItem(value: null, child: Text('All')),
                         DropdownMenuItem(value: 0, child: Text('Sale')),
                         DropdownMenuItem(value: 1, child: Text('Rent')),
                       ],
                       onChanged: (val) => setSheetState(() => _purpose = val),
                     ),
                     const SizedBox(height: 12),
                     
                     // Property Type
                     DropdownButtonFormField<int?>(
                       value: _propertyType,
                       decoration: const InputDecoration(labelText: 'Property Type'),
                       items: const [
                         DropdownMenuItem(value: null, child: Text('All')),
                         DropdownMenuItem(value: 0, child: Text('House')),
                         DropdownMenuItem(value: 1, child: Text('Apartment')),
                         DropdownMenuItem(value: 2, child: Text('Condo')),
                       ],
                       onChanged: (val) => setSheetState(() => _propertyType = val),
                     ),
                     const SizedBox(height: 12),

                     // Price
                     Row(
                       children: [
                         Expanded(
                           child: TextField(
                             controller: _minPriceController,
                             keyboardType: TextInputType.number,
                             decoration: const InputDecoration(labelText: 'Min Price (LKR)'),
                           )
                         ),
                         const SizedBox(width: 12),
                         Expanded(
                           child: TextField(
                             controller: _maxPriceController,
                             keyboardType: TextInputType.number,
                             decoration: const InputDecoration(labelText: 'Max Price (LKR)'),
                           )
                         )
                       ],
                     ),
                     const SizedBox(height: 12),

                     // Beds/Baths
                     Row(
                       children: [
                         Expanded(
                           child: TextField(
                             controller: _bedroomsController,
                             keyboardType: TextInputType.number,
                             decoration: const InputDecoration(labelText: 'Min Beds'),
                           )
                         ),
                         const SizedBox(width: 12),
                         Expanded(
                           child: TextField(
                             controller: _bathroomsController,
                             keyboardType: TextInputType.number,
                             decoration: const InputDecoration(labelText: 'Min Baths'),
                           )
                         )
                       ],
                     ),
                     
                     const SizedBox(height: 24),
                     Row(
                       children: [
                         Expanded(
                           child: OutlinedButton(
                             onPressed: () {
                               // 1. Clear text visually
                               _cityController.clear();
                               _minPriceController.clear();
                               _maxPriceController.clear();
                               _bedroomsController.clear();
                               _bathroomsController.clear();
                               
                               // 2. Clear modal dropdown UI state
                               setSheetState(() {
                                 _purpose = null;
                                 _propertyType = null;
                                 _sort = 'newest';
                               });

                               // 3. Apply to parent filter state
                               setState(() {
                                 _purpose = null;
                                 _propertyType = null;
                                 _minPrice = null;
                                 _maxPrice = null;
                                 _bedrooms = null;
                                 _bathrooms = null;
                                 _sort = 'newest';
                               });
                               
                               // 4. Trigger reload immediately
                               _loadProperties(refresh: true);
                             },
                             style: OutlinedButton.styleFrom(
                              foregroundColor: _dark,
                              side: const BorderSide(color: _border),
                              shape: const RoundedRectangleBorder(
                                borderRadius: BorderRadius.zero,
                              ),
                              minimumSize: const Size.fromHeight(50),
                            ),
                             child: const Text('Clear'),
                           ),
                         ),
                         const SizedBox(width: 12),
                         Expanded(
                           child: ElevatedButton(
                             onPressed: () {
                               setState(() {
                                 _minPrice = double.tryParse(_minPriceController.text);
                                 _maxPrice = double.tryParse(_maxPriceController.text);
                                 _bedrooms = int.tryParse(_bedroomsController.text);
                                 _bathrooms = int.tryParse(_bathroomsController.text);
                               });
                               Navigator.pop(ctx);
                               _loadProperties(refresh: true);
                             },
                             style: ElevatedButton.styleFrom(
                              backgroundColor: _dark,
                              foregroundColor: Colors.white,
                              elevation: 0,
                              shape: const RoundedRectangleBorder(
                                borderRadius: BorderRadius.zero,
                              ),
                              minimumSize: const Size.fromHeight(50),
                            ),
                             child: const Text('Apply'),
                           ),
                         ),
                       ],
                     ),
                     const SizedBox(height: 32),
                   ],
                 )
               )
             );
           }
         );
      }
    );
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
        'PropMate',
        style: TextStyle(
          fontSize: 18,
          fontWeight: FontWeight.w700,
          letterSpacing: 1.2,
        ),
      ),

      actions: [
        IconButton(
          tooltip: _isMapView ? 'List view' : 'Map view',
          icon: Icon(
            _isMapView
                ? Icons.view_list_outlined
                : Icons.map_outlined,
            color: _gold,
          ),
          onPressed: () {
            setState(() {
              _isMapView = !_isMapView;
            });
          },
        ),

        IconButton(
          tooltip: 'Refresh',
          icon: const Icon(
            Icons.refresh,
            color: Colors.white,
          ),
          onPressed: () {
            _loadFavorites();
            _loadProperties(refresh: true);
          },
        ),

        const SizedBox(width: 6),
      ],
    ),

    // ----------------------------------------------------
    // DRAWER
    // ----------------------------------------------------

    drawer: Drawer(
      backgroundColor: _background,
      child: Column(
        children: [
          Container(
            width: double.infinity,
            padding: const EdgeInsets.fromLTRB(
              24,
              58,
              24,
              30,
            ),
            color: _dark,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Image.asset(
                  'assets/images/logo-icon.png',
                  width: 46,
                  height: 46,
                  fit: BoxFit.contain,
                ),

                SizedBox(height: 14),

                Text(
                  'PROPMATE',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 19,
                    fontWeight: FontWeight.w700,
                    letterSpacing: 2.5,
                  ),
                ),

                SizedBox(height: 6),

                Text(
                  'PROPERTY DISCOVERY',
                  style: TextStyle(
                    color: _gold,
                    fontSize: 9,
                    fontWeight: FontWeight.w700,
                    letterSpacing: 1.5,
                  ),
                ),
              ],
            ),
          ),

          Expanded(
            child: ListView(
              padding: const EdgeInsets.symmetric(
                vertical: 18,
              ),
              children: [
                const Padding(
                  padding: EdgeInsets.fromLTRB(
                    24,
                    0,
                    24,
                    10,
                  ),
                  child: Text(
                    'EXPLORE',
                    style: TextStyle(
                      color: _mutedText,
                      fontSize: 9,
                      fontWeight: FontWeight.w700,
                      letterSpacing: 1.5,
                    ),
                  ),
                ),

                ListTile(
                  leading: const Icon(
                    Icons.search,
                    color: _gold,
                  ),
                  title: const Text(
                    'Discover',
                    style: TextStyle(
                      color: _dark,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  onTap: () => Navigator.pop(context),
                ),

                ListTile(
                  leading: const Icon(
                    Icons.favorite_border_rounded,
                    color: _gold,
                  ),
                  title: const Text(
                    'My Favourites',
                    style: TextStyle(
                      color: _dark,
                    ),
                  ),
                  onTap: () async {
                    Navigator.pop(context);

                    await Navigator.push(
                      context,
                      MaterialPageRoute(
                        builder: (_) =>
                            const MyFavouritesPage(),
                      ),
                    );

                    _loadFavorites();
                  },
                ),

                ListTile(
                  leading: const Icon(
                    Icons.calendar_month_outlined,
                    color: _gold,
                  ),
                  title: const Text(
                    'My Viewings',
                    style: TextStyle(
                      color: _dark,
                    ),
                  ),
                  onTap: () {
                    Navigator.pop(context);

                    Navigator.push(
                      context,
                      MaterialPageRoute(
                        builder: (_) =>
                            const MyViewingsPage(),
                      ),
                    );
                  },
                                ),

                                  ListTile(
                  leading: const Icon(
                    Icons.build_outlined,
                    color: _gold,
                  ),
                  title: const Text(
                    'Maintenance',
                    style: TextStyle(
                      color: _dark,
                    ),
                  ),
                  onTap: () {
                    Navigator.pop(context);

                    Navigator.push(
                      context,
                      MaterialPageRoute(
                        builder: (_) => const MaintenanceHome(),
                      ),
                    );
                  },
                ),              

                const Divider(
                  height: 28,
                  indent: 24,
                  endIndent: 24,
                  color: _border,
                ),

                ListTile(
                  leading: const Icon(
                    Icons.logout_rounded,
                    color: _danger,
                  ),
                  title: const Text(
                    'Sign Out',
                    style: TextStyle(
                      color: _danger,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  onTap: () async {
                    Navigator.pop(context);

                    await MobileAuthService.logout();

                    if (!mounted) {
                      return;
                    }

                    Navigator.of(context).pushAndRemoveUntil(
                      MaterialPageRoute(
                        builder: (_) => const LoginScreen(),
                      ),
                      (route) => false,
                    );
                  },
                ),
              ],
            ),
          ),

          const SizedBox(height: 12),
        ],
      ),
    ),

    body: Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        // ------------------------------------------------
        // PAGE INTRO
        // ------------------------------------------------

        Container(
          padding: const EdgeInsets.fromLTRB(
            20,
            24,
            20,
            18,
          ),
          child: const Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'PROPERTY DISCOVERY',
                style: TextStyle(
                  color: _gold,
                  fontSize: 10,
                  fontWeight: FontWeight.w700,
                  letterSpacing: 1.6,
                ),
              ),

              SizedBox(height: 7),

              Text(
                'Find your next property',
                style: TextStyle(
                  color: _dark,
                  fontSize: 26,
                  fontWeight: FontWeight.w600,
                  height: 1.15,
                ),
              ),

              SizedBox(height: 6),

              Text(
                'Search published properties or let the discovery agent help you.',
                style: TextStyle(
                  color: _mutedText,
                  fontSize: 13,
                  height: 1.45,
                ),
              ),
            ],
          ),
        ),

        // ------------------------------------------------
        // STANDARD SEARCH
        // ------------------------------------------------

        Padding(
          padding: const EdgeInsets.symmetric(
            horizontal: 20,
          ),
          child: Row(
            children: [
              Expanded(
                child: TextField(
                  controller: _searchController,

                  decoration: InputDecoration(
                    hintText: 'Search properties...',

                    hintStyle: const TextStyle(
                      color: Color(0xFF9B968D),
                      fontSize: 13,
                    ),

                    prefixIcon: const Icon(
                      Icons.search,
                      color: _mutedText,
                      size: 20,
                    ),

                    suffixIcon: IconButton(
                      tooltip: 'Search',
                      icon: const Icon(
                        Icons.arrow_forward,
                        color: _gold,
                        size: 20,
                      ),
                      onPressed: () =>
                          _loadProperties(refresh: true),
                    ),

                    filled: true,
                    fillColor: Colors.white,

                    contentPadding:
                        const EdgeInsets.symmetric(
                      horizontal: 15,
                      vertical: 15,
                    ),

                    enabledBorder:
                        const OutlineInputBorder(
                      borderRadius: BorderRadius.zero,
                      borderSide: BorderSide(
                        color: _border,
                      ),
                    ),

                    focusedBorder:
                        const OutlineInputBorder(
                      borderRadius: BorderRadius.zero,
                      borderSide: BorderSide(
                        color: _gold,
                        width: 1.5,
                      ),
                    ),
                  ),

                  onSubmitted: (_) =>
                      _loadProperties(refresh: true),
                ),
              ),

              const SizedBox(width: 10),

              SizedBox(
                width: 50,
                height: 50,
                child: IconButton(
                  tooltip: 'Filters & sorting',
                  onPressed: _showFilterSheet,

                  style: IconButton.styleFrom(
                    backgroundColor: _dark,
                    foregroundColor: _gold,
                    shape:
                        const RoundedRectangleBorder(
                      borderRadius: BorderRadius.zero,
                    ),
                  ),

                  icon: const Icon(
                    Icons.tune,
                    size: 20,
                  ),
                ),
              ),
            ],
          ),
        ),

        const SizedBox(height: 12),

        // ------------------------------------------------
        // AI DISCOVERY
        // ------------------------------------------------

        Container(
          margin: const EdgeInsets.symmetric(
            horizontal: 20,
          ),

          padding: const EdgeInsets.all(14),

          decoration: BoxDecoration(
            color: _cardBackground,
            border: Border.all(
              color: _border,
            ),
          ),

          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Row(
                children: [
                  Icon(
                    Icons.auto_awesome,
                    color: _gold,
                    size: 17,
                  ),

                  SizedBox(width: 7),

                  Text(
                    'AI PROPERTY DISCOVERY',
                    style: TextStyle(
                      color: _gold,
                      fontSize: 9,
                      fontWeight: FontWeight.w700,
                      letterSpacing: 1.3,
                    ),
                  ),
                ],
              ),

              const SizedBox(height: 10),

              Row(
                children: [
                  Expanded(
                    child: TextField(
                      controller: _aiController,

                      decoration: const InputDecoration(
                        hintText:
                            'e.g. A house in Colombo under 40M',

                        hintStyle: TextStyle(
                          color: Color(0xFF9B968D),
                          fontSize: 12,
                        ),

                        filled: true,
                        fillColor: Colors.white,

                        contentPadding:
                            EdgeInsets.symmetric(
                          horizontal: 13,
                          vertical: 13,
                        ),

                        enabledBorder:
                            OutlineInputBorder(
                          borderRadius:
                              BorderRadius.zero,
                          borderSide: BorderSide(
                            color: _border,
                          ),
                        ),

                        focusedBorder:
                            OutlineInputBorder(
                          borderRadius:
                              BorderRadius.zero,
                          borderSide: BorderSide(
                            color: _gold,
                            width: 1.5,
                          ),
                        ),
                      ),

                      onSubmitted: (_) {
                        if (!_aiLoading) {
                          _askAi();
                        }
                      },
                    ),
                  ),

                  const SizedBox(width: 8),

                  SizedBox(
                    height: 47,
                    child: ElevatedButton(
                      onPressed:
                          _aiLoading ? null : _askAi,

                      style: ElevatedButton.styleFrom(
                        backgroundColor: _dark,
                        foregroundColor: Colors.white,
                        elevation: 0,

                        shape:
                            const RoundedRectangleBorder(
                          borderRadius:
                              BorderRadius.zero,
                        ),

                        padding:
                            const EdgeInsets.symmetric(
                          horizontal: 15,
                        ),
                      ),

                      child: _aiLoading
                          ? const SizedBox(
                              width: 18,
                              height: 18,
                              child:
                                  CircularProgressIndicator(
                                strokeWidth: 2,
                                color: _gold,
                              ),
                            )
                          : const Row(
                              children: [
                                Icon(
                                  Icons.auto_awesome,
                                  color: _gold,
                                  size: 15,
                                ),
                                SizedBox(width: 6),
                                Text(
                                  'ASK',
                                  style: TextStyle(
                                    fontSize: 10,
                                    fontWeight:
                                        FontWeight.w700,
                                    letterSpacing: 1,
                                  ),
                                ),
                              ],
                            ),
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),

        // ------------------------------------------------
        // AI ANALYSIS
        // ------------------------------------------------

        if (_agentResponse != null)
          Container(
            margin: const EdgeInsets.fromLTRB(
              20,
              12,
              20,
              0,
            ),

            padding: const EdgeInsets.all(14),

            decoration: BoxDecoration(
              color: const Color(0xFFF0ECE3),
              border: Border.all(
                color: _border,
              ),
            ),

            child: Column(
              crossAxisAlignment:
                  CrossAxisAlignment.stretch,
              children: [
                Row(
                  children: [
                    const Icon(
                      Icons.psychology_alt_outlined,
                      color: _gold,
                      size: 19,
                    ),

                    const SizedBox(width: 8),

                    const Expanded(
                      child: Text(
                        'DISCOVERY AGENT ANALYSIS',
                        style: TextStyle(
                          color: _dark,
                          fontSize: 10,
                          fontWeight: FontWeight.w700,
                          letterSpacing: 1,
                        ),
                      ),
                    ),

                    Container(
                      padding:
                          const EdgeInsets.symmetric(
                        horizontal: 8,
                        vertical: 4,
                      ),

                      decoration: BoxDecoration(
                        color: const Color(0xFFDDE9DE),
                        border: Border.all(
                          color:
                              const Color(0xFFB9CFBC),
                        ),
                      ),

                      child: Text(
                        '${(_agentResponse!.confidence * 100).toStringAsFixed(0)}% MATCH',
                        style: const TextStyle(
                          color: _success,
                          fontSize: 9,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                    ),
                  ],
                ),

                if (_agentResponse!.warnings.isNotEmpty) ...[
                  const SizedBox(height: 12),

                  Container(
                    padding: const EdgeInsets.all(10),
                    decoration: BoxDecoration(
                      color: const Color(0xFFF2DFDC),
                      border: Border.all(
                        color: const Color(0xFFE4C7C2),
                      ),
                    ),
                    child: Text(
                      'Note: ${_agentResponse!.warnings.join(", ")}',
                      style: const TextStyle(
                        color: _danger,
                        fontSize: 12,
                        height: 1.4,
                      ),
                    ),
                  ),
                ],

                const SizedBox(height: 10),

                Text(
                  'Plan: ${_agentResponse!.plan.join(" → ")}',
                  style: const TextStyle(
                    color: _mutedText,
                    fontSize: 11,
                    height: 1.4,
                  ),
                ),
              ],
            ),
          ),

        const SizedBox(height: 12),

        // ------------------------------------------------
        // RESULTS
        // ------------------------------------------------

        Expanded(
          child: _loading
              ? const Center(
                  child: CircularProgressIndicator(
                    color: _gold,
                    strokeWidth: 2.5,
                  ),
                )
              : _agentResponse != null
                  ? _buildAgentResults()
                  : (_isMapView
                      ? _buildMapResults()
                      : _buildStandardResults()),
        ),
      ],
    ),
  );
}

  Widget _buildMapResults() {
    final markers = _properties.where((p) => p.latitude != null && p.longitude != null).map((p) {
      return Marker(
        point: LatLng(p.latitude!, p.longitude!),
        width: 40,
        height: 40,
        child: GestureDetector(
          onTap: () async {
            final isFav = _favoriteIds.contains(p.id);
            await Navigator.push(context, MaterialPageRoute(builder: (_) => PropertyDetailsPage(property: p, initialFavorite: isFav)));
            _loadFavorites();
          },
          child: const Icon(Icons.location_on, color: _gold, size: 40),
        ),
      );
    }).toList();

    LatLng initialCenter = _currentLocation ?? const LatLng(7.8731, 80.7718); // Default Sri Lanka
    if (_currentLocation == null && markers.isNotEmpty) {
      initialCenter = markers.first.point; // Center on available property if location is denied
    }

    return Stack(
      children: [
        FlutterMap(
          mapController: _mapController,
          options: MapOptions(
            initialCenter: initialCenter,
            initialZoom: 10,
          ),
          children: [
            TileLayer(
              urlTemplate: 'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
              userAgentPackageName: 'com.propmate.mobile',
            ),
            MarkerLayer(markers: markers),
            if (_currentLocation != null)
              MarkerLayer(markers: [
                Marker(
                  point: _currentLocation!,
                  width: 20,
                  height: 20,
                  child: Container(
                    decoration: BoxDecoration(
  color: _dark,
  shape: BoxShape.circle,
  border: Border.all(
    color: Colors.white,
    width: 3,
  ),
  boxShadow: const [
    BoxShadow(
      blurRadius: 5,
      color: Color(0x33000000),
    ),
  ],
),
                  ),
                )
              ]),
          ],
        ),
        if (_currentLocation != null)
          Positioned(
            bottom: 16,
            right: 16,
            child: FloatingActionButton(
              mini: true,
              backgroundColor: _dark,
foregroundColor: _gold,
elevation: 2,
child: const Icon(
  Icons.my_location,
  color: _gold,
),
              onPressed: () {
                _mapController.move(_currentLocation!, 12);
              },
            ),
          ),
      ]
    );
  }

  Widget _buildStandardResults() {
  if (_properties.isEmpty) {
    return _buildEmptyResults(
      Icons.home_work_outlined,
      'No properties found',
      'Try changing your search or filters.',
    );
  }

  return ListView.builder(
    controller: _scrollController,

    padding: const EdgeInsets.fromLTRB(
      20,
      4,
      20,
      30,
    ),

    itemCount:
        _properties.length + (_loadingMore ? 1 : 0),

    itemBuilder: (context, index) {
      if (index == _properties.length) {
        return const Center(
          child: Padding(
            padding: EdgeInsets.all(20),
            child: CircularProgressIndicator(
              color: _gold,
              strokeWidth: 2,
            ),
          ),
        );
      }

      final property = _properties[index];
      final isFav =
          _favoriteIds.contains(property.id);

      final location =
          (property.city?.trim().isNotEmpty ?? false)
              ? property.city!
              : (property.address?.trim().isNotEmpty ??
                      false)
                  ? property.address!
                  : 'Location not specified';

      return Container(
        margin: const EdgeInsets.only(bottom: 16),

        decoration: BoxDecoration(
          color: _cardBackground,
          border: Border.all(
            color: _border,
          ),
        ),

        child: InkWell(
          onTap: () async {
            final currentFav =
                _favoriteIds.contains(property.id);

            await Navigator.push(
              context,
              MaterialPageRoute(
                builder: (_) => PropertyDetailsPage(
                  property: property,
                  initialFavorite: currentFav,
                ),
              ),
            );

            _loadFavorites();
          },

          child: Column(
            crossAxisAlignment:
                CrossAxisAlignment.start,
            children: [
              Stack(
                children: [
                  Container(
                    height: 145,
                    width: double.infinity,
                    color: const Color(0xFFE9E5DC),
                    child: const Icon(
                      Icons.apartment_rounded,
                      size: 54,
                      color: Color(0xFFB9B2A6),
                    ),
                  ),

                  Positioned(
                    left: 12,
                    bottom: 12,
                    child: Container(
                      padding:
                          const EdgeInsets.symmetric(
                        horizontal: 9,
                        vertical: 5,
                      ),
                      color: _dark,
                      child: const Text(
                        'PUBLISHED',
                        style: TextStyle(
                          color: _gold,
                          fontSize: 8,
                          fontWeight: FontWeight.w700,
                          letterSpacing: 1.2,
                        ),
                      ),
                    ),
                  ),

                  Positioned(
                    top: 10,
                    right: 10,
                    child: Container(
                      decoration: BoxDecoration(
                        color: _cardBackground,
                        border: Border.all(
                          color: _border,
                        ),
                      ),
                      child: IconButton(
                        tooltip: isFav
                            ? 'Remove from favourites'
                            : 'Save to favourites',
                        onPressed: () =>
                            _toggleFavorite(property.id),
                        icon: Icon(
                          isFav
                              ? Icons.favorite_rounded
                              : Icons
                                  .favorite_border_rounded,
                          color: isFav
                              ? _danger
                              : _dark,
                          size: 21,
                        ),
                      ),
                    ),
                  ),
                ],
              ),

              Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment:
                      CrossAxisAlignment.start,
                  children: [
                    Text(
                      property.title,
                      maxLines: 2,
                      overflow:
                          TextOverflow.ellipsis,
                      style: const TextStyle(
                        color: _dark,
                        fontSize: 18,
                        fontWeight: FontWeight.w600,
                        height: 1.25,
                      ),
                    ),

                    const SizedBox(height: 7),

                    Text(
                      'LKR ${property.price.toStringAsFixed(0)}',
                      style: const TextStyle(
                        color: _gold,
                        fontSize: 17,
                        fontWeight: FontWeight.w700,
                      ),
                    ),

                    const SizedBox(height: 10),

                    Row(
                      children: [
                        const Icon(
                          Icons.location_on_outlined,
                          color: _mutedText,
                          size: 16,
                        ),

                        const SizedBox(width: 5),

                        Expanded(
                          child: Text(
                            location,
                            maxLines: 1,
                            overflow:
                                TextOverflow.ellipsis,
                            style: const TextStyle(
                              color: _mutedText,
                              fontSize: 12,
                            ),
                          ),
                        ),
                      ],
                    ),

                    const SizedBox(height: 14),

                    Container(
                      padding:
                          const EdgeInsets.symmetric(
                        vertical: 10,
                      ),

                      decoration:
                          const BoxDecoration(
                        border: Border(
                          top: BorderSide(
                            color: _border,
                          ),
                        ),
                      ),

                      child: Row(
                        children: [
                          const Icon(
                            Icons.bed_outlined,
                            color: _gold,
                            size: 18,
                          ),

                          const SizedBox(width: 5),

                          Text(
                            '${property.bedrooms} Beds',
                            style: const TextStyle(
                              color: _mutedText,
                              fontSize: 12,
                              fontWeight:
                                  FontWeight.w500,
                            ),
                          ),

                          const SizedBox(width: 20),

                          const Icon(
                            Icons.bathtub_outlined,
                            color: _gold,
                            size: 17,
                          ),

                          const SizedBox(width: 5),

                          Text(
                            '${property.bathrooms} Baths',
                            style: const TextStyle(
                              color: _mutedText,
                              fontSize: 12,
                              fontWeight:
                                  FontWeight.w500,
                            ),
                          ),

                          const Spacer(),

                          const Icon(
                            Icons.arrow_forward,
                            color: _gold,
                            size: 17,
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
      );
    },
  );
}

  Widget _buildAgentResults() {
  final matches = _agentResponse!.matches;

  if (matches.isEmpty) {
    return _buildEmptyResults(
      Icons.auto_awesome,
      'No AI matches found',
      'Try describing the property you want in a different way.',
    );
  }

  return ListView.builder(
    padding: const EdgeInsets.fromLTRB(
      20,
      4,
      20,
      30,
    ),
    itemCount: matches.length,
    itemBuilder: (context, i) {
      final match = matches[i];

      // Find the real property returned by the normal discovery API.
      final pIndex = _properties.indexWhere(
        (p) => p.id == match.propertyListingId,
      );

      final titleText = pIndex >= 0
          ? _properties[pIndex].title
          : 'Property ID: ${match.propertyListingId}';

      // Preserve your existing fallback behaviour.
      final pObj = pIndex >= 0
          ? _properties[pIndex]
          : Property.fromJson({
              'id': match.propertyListingId,
              'title': 'Property ID ${match.propertyListingId}',
              'description': '',
              'price': 0.0,
              'bedrooms': 0,
              'bathrooms': 0,
              'address': '',
              'city': '',
              'location': '',
              'status': 0,
              'ownerId': 0,
              'images': []
            });

      final isFav = _favoriteIds.contains(pObj.id);

      final location =
          (pObj.city?.trim().isNotEmpty ?? false)
              ? pObj.city!
              : (pObj.address?.trim().isNotEmpty ?? false)
                  ? pObj.address!
                  : 'Location not specified';

      return Container(
        margin: const EdgeInsets.only(bottom: 16),
        decoration: BoxDecoration(
          color: _cardBackground,
          border: Border.all(
            color: _border,
          ),
        ),
        child: InkWell(
          onTap: () async {
            final currentFav =
                _favoriteIds.contains(pObj.id);

            await Navigator.push(
              context,
              MaterialPageRoute(
                builder: (_) => PropertyDetailsPage(
                  property: pObj,
                  initialFavorite: currentFav,
                ),
              ),
            );

            _loadFavorites();
          },
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              // ---------------------------------------------
              // PROPERTY IMAGE PLACEHOLDER
              // ---------------------------------------------

              Stack(
                children: [
                  Container(
                    height: 135,
                    width: double.infinity,
                    color: const Color(0xFFE9E5DC),
                    child: const Icon(
                      Icons.apartment_rounded,
                      size: 52,
                      color: Color(0xFFB9B2A6),
                    ),
                  ),

                  // AI MATCH LABEL
                  Positioned(
                    left: 12,
                    bottom: 12,
                    child: Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 9,
                        vertical: 5,
                      ),
                      color: _dark,
                      child: const Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(
                            Icons.auto_awesome,
                            color: _gold,
                            size: 11,
                          ),
                          SizedBox(width: 5),
                          Text(
                            'AI MATCH',
                            style: TextStyle(
                              color: _gold,
                              fontSize: 8,
                              fontWeight: FontWeight.w700,
                              letterSpacing: 1.2,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),

                  // FAVOURITE BUTTON
                  Positioned(
                    top: 10,
                    right: 10,
                    child: Container(
                      decoration: BoxDecoration(
                        color: _cardBackground,
                        border: Border.all(
                          color: _border,
                        ),
                      ),
                      child: IconButton(
                        tooltip: isFav
                            ? 'Remove from favourites'
                            : 'Save to favourites',
                        onPressed: () =>
                            _toggleFavorite(pObj.id),
                        icon: Icon(
                          isFav
                              ? Icons.favorite_rounded
                              : Icons.favorite_border_rounded,
                          color:
                              isFav ? _danger : _dark,
                          size: 21,
                        ),
                      ),
                    ),
                  ),
                ],
              ),

              // ---------------------------------------------
              // PROPERTY INFORMATION
              // ---------------------------------------------

              Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment:
                      CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'RECOMMENDED PROPERTY',
                      style: TextStyle(
                        color: _gold,
                        fontSize: 9,
                        fontWeight: FontWeight.w700,
                        letterSpacing: 1.3,
                      ),
                    ),

                    const SizedBox(height: 7),

                    Text(
                      titleText,
                      style: const TextStyle(
                        color: _dark,
                        fontSize: 18,
                        fontWeight: FontWeight.w600,
                        height: 1.25,
                      ),
                    ),

                    // Only show these real-property fields when
                    // we actually found the property object.
                    if (pIndex >= 0) ...[
                      const SizedBox(height: 7),

                      Text(
                        'LKR ${pObj.price.toStringAsFixed(0)}',
                        style: const TextStyle(
                          color: _gold,
                          fontSize: 17,
                          fontWeight: FontWeight.w700,
                        ),
                      ),

                      const SizedBox(height: 9),

                      Row(
                        children: [
                          const Icon(
                            Icons.location_on_outlined,
                            color: _mutedText,
                            size: 15,
                          ),

                          const SizedBox(width: 5),

                          Expanded(
                            child: Text(
                              location,
                              maxLines: 1,
                              overflow:
                                  TextOverflow.ellipsis,
                              style: const TextStyle(
                                color: _mutedText,
                                fontSize: 12,
                              ),
                            ),
                          ),
                        ],
                      ),

                      const SizedBox(height: 10),

                      Row(
                        children: [
                          const Icon(
                            Icons.bed_outlined,
                            color: _gold,
                            size: 17,
                          ),

                          const SizedBox(width: 5),

                          Text(
                            '${pObj.bedrooms} Beds',
                            style: const TextStyle(
                              color: _mutedText,
                              fontSize: 12,
                            ),
                          ),

                          const SizedBox(width: 18),

                          const Icon(
                            Icons.bathtub_outlined,
                            color: _gold,
                            size: 16,
                          ),

                          const SizedBox(width: 5),

                          Text(
                            '${pObj.bathrooms} Baths',
                            style: const TextStyle(
                              color: _mutedText,
                              fontSize: 12,
                            ),
                          ),
                        ],
                      ),
                    ],

                    const SizedBox(height: 16),

                    // -----------------------------------------
                    // WHY THE AGENT CHOSE IT
                    // -----------------------------------------

                    Container(
                      width: double.infinity,
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: const Color(0xFFF0ECE3),
                        border: Border.all(
                          color: _border,
                        ),
                      ),
                      child: Column(
                        crossAxisAlignment:
                            CrossAxisAlignment.start,
                        children: [
                          const Row(
                            children: [
                              Icon(
                                Icons.psychology_alt_outlined,
                                color: _gold,
                                size: 17,
                              ),

                              SizedBox(width: 7),

                              Text(
                                'WHY THIS MATCHES',
                                style: TextStyle(
                                  color: _dark,
                                  fontSize: 9,
                                  fontWeight:
                                      FontWeight.w700,
                                  letterSpacing: 1.1,
                                ),
                              ),
                            ],
                          ),

                          const SizedBox(height: 8),

                          Text(
                            match.reasons,
                            style: const TextStyle(
                              color: _mutedText,
                              fontSize: 12,
                              height: 1.5,
                            ),
                          ),
                        ],
                      ),
                    ),

                    // -----------------------------------------
                    // VIEWING AVAILABILITY
                    // -----------------------------------------

                    if (match
                        .availableViewingSlots.isNotEmpty) ...[
                      const SizedBox(height: 12),

                      Container(
                        width: double.infinity,
                        padding:
                            const EdgeInsets.symmetric(
                          horizontal: 11,
                          vertical: 10,
                        ),
                        decoration: BoxDecoration(
                          color: const Color(0xFFE8EEE8),
                          border: Border.all(
                            color:
                                const Color(0xFFC9D8CA),
                          ),
                        ),
                        child: Row(
                          children: [
                            const Icon(
                              Icons.event_available_outlined,
                              size: 17,
                              color: _success,
                            ),

                            const SizedBox(width: 7),

                            Text(
                              '${match.availableViewingSlots.length} viewing ${match.availableViewingSlots.length == 1 ? 'slot' : 'slots'} available',
                              style: const TextStyle(
                                color: _success,
                                fontSize: 11,
                                fontWeight: FontWeight.w600,
                              ),
                            ),

                            const Spacer(),

                            const Icon(
                              Icons.arrow_forward,
                              color: _success,
                              size: 15,
                            ),
                          ],
                        ),
                      ),
                    ],

                    const SizedBox(height: 14),

                    // -----------------------------------------
                    // VIEW DETAILS
                    // -----------------------------------------

                    Container(
                      padding: const EdgeInsets.only(
                        top: 12,
                      ),
                      decoration: const BoxDecoration(
                        border: Border(
                          top: BorderSide(
                            color: _border,
                          ),
                        ),
                      ),
                      child: const Row(
                        children: [
                          Text(
                            'VIEW PROPERTY',
                            style: TextStyle(
                              color: _dark,
                              fontSize: 9,
                              fontWeight: FontWeight.w700,
                              letterSpacing: 1.1,
                            ),
                          ),

                          Spacer(),

                          Icon(
                            Icons.arrow_forward,
                            color: _gold,
                            size: 17,
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
      );
    },
  );
}

  Widget _buildEmptyResults(
  IconData icon,
  String title,
  String message,
) {
  return Center(
    child: Padding(
      padding: const EdgeInsets.all(32),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Container(
            width: 70,
            height: 70,
            decoration: BoxDecoration(
              color: _cardBackground,
              border: Border.all(
                color: _border,
              ),
            ),
            child: Icon(
              icon,
              color: _gold,
              size: 29,
            ),
          ),

          const SizedBox(height: 20),

          Text(
            title,
            textAlign: TextAlign.center,
            style: const TextStyle(
              color: _dark,
              fontSize: 20,
              fontWeight: FontWeight.w600,
            ),
          ),

          const SizedBox(height: 7),

          Text(
            message,
            textAlign: TextAlign.center,
            style: const TextStyle(
              color: _mutedText,
              fontSize: 13,
              height: 1.5,
            ),
          ),
        ],
      ),
    ),
  );
}
}
