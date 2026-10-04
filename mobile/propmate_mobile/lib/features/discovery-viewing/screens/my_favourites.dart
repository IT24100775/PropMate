import 'package:flutter/material.dart';
import '../models/property.dart';
import '../services/favourite_service.dart';
import 'property_details.dart';

class MyFavouritesPage extends StatefulWidget {
  const MyFavouritesPage({super.key});

  @override
  State<MyFavouritesPage> createState() => _State();
}

class _State extends State<MyFavouritesPage> {
  final FavouriteService _svc = FavouriteService();

  List<Property> _favourites = [];
  bool _loading = true;

  static const Color _background = Color(0xFFF6F4EF);
  static const Color _cardBackground = Color(0xFFFBFAF7);
  static const Color _dark = Color(0xFF181817);
  static const Color _gold = Color(0xFFCFA03C);
  static const Color _mutedText = Color(0xFF777168);
  static const Color _border = Color(0xFFDEDAD1);
  static const Color _danger = Color(0xFF8D4038);

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    if (mounted) {
      setState(() => _loading = true);
    }

    final res = await _svc.getFavourites(page: 1);

    if (mounted) {
      setState(() {
        _favourites = res.items;
        _loading = false;
      });
    }
  }

  Future<void> _remove(int id) async {
    await _svc.removeFavourite(id);
    await _load();
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

        title: const Text(
          'My Favourites',
          style: TextStyle(
            fontSize: 18,
            fontWeight: FontWeight.w600,
          ),
        ),

        iconTheme: const IconThemeData(
          color: _gold,
        ),
      ),

      body: _loading
          ? const Center(
              child: CircularProgressIndicator(
                color: _gold,
                strokeWidth: 2.5,
              ),
            )
          : _favourites.isEmpty
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

                    itemCount: _favourites.length,

                    itemBuilder: (context, index) {
                      final property = _favourites[index];

                      return _buildPropertyCard(property);
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
                Icons.favorite_border_rounded,
                size: 30,
                color: _gold,
              ),
            ),

            const SizedBox(height: 22),

            const Text(
              'No favourites yet',
              style: TextStyle(
                color: _dark,
                fontSize: 22,
                fontWeight: FontWeight.w600,
              ),
            ),

            const SizedBox(height: 8),

            const Text(
              'Properties you save will appear here.',
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

  Widget _buildPropertyCard(Property property) {
    return Container(
      margin: const EdgeInsets.only(
        bottom: 14,
      ),

      decoration: BoxDecoration(
        color: _cardBackground,
        border: Border.all(
          color: _border,
        ),
      ),

      child: InkWell(
        onTap: () async {
          await Navigator.push(
            context,
            MaterialPageRoute(
              builder: (_) => PropertyDetailsPage(
                property: property,
                initialFavorite: true,
              ),
            ),
          );

          _load();
        },

        child: Padding(
          padding: const EdgeInsets.all(16),

          child: Row(
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              // -----------------------------------------
              // PROPERTY ICON
              // -----------------------------------------

              Container(
                width: 64,
                height: 64,
                decoration: BoxDecoration(
                  color: const Color(0xFFF0ECE3),
                  border: Border.all(
                    color: _border,
                  ),
                ),
                child: property.images.isNotEmpty
                    ? Image.network(
                        property.images.first,
                        width: 64,
                        height: 64,
                        fit: BoxFit.cover,
                        errorBuilder: (context, error, stackTrace) {
                          return const Icon(
                            Icons.home_work_outlined,
                            color: _gold,
                            size: 27,
                          );
                        },
                      )
                    : const Icon(
                        Icons.home_work_outlined,
                        color: _gold,
                        size: 27,
                      ),
              ),

              const SizedBox(width: 16),

              // -----------------------------------------
              // PROPERTY INFORMATION
              // -----------------------------------------

              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'SAVED PROPERTY',
                      style: TextStyle(
                        color: _gold,
                        fontSize: 9,
                        fontWeight: FontWeight.w700,
                        letterSpacing: 1.3,
                      ),
                    ),

                    const SizedBox(height: 5),

                    Text(
                      property.title,
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,

                      style: const TextStyle(
                        color: _dark,
                        fontSize: 16,
                        fontWeight: FontWeight.w600,
                        height: 1.25,
                      ),
                    ),

                    const SizedBox(height: 7),

                    Text(
                      'LKR ${property.price.toStringAsFixed(0)}',
                      style: const TextStyle(
                        color: _mutedText,
                        fontSize: 13,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                  ],
                ),
              ),

              const SizedBox(width: 8),

              // -----------------------------------------
              // REMOVE FAVOURITE
              // -----------------------------------------

              IconButton(
                tooltip: 'Remove from favourites',

                onPressed: () => _remove(property.id),

                style: IconButton.styleFrom(
                  foregroundColor: _danger,
                  backgroundColor: const Color(0xFFF2DFDC),

                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.zero,
                    side: const BorderSide(
                      color: Color(0xFFE4C7C2),
                    ),
                  ),
                ),

                icon: const Icon(
                  Icons.favorite_rounded,
                  size: 20,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}