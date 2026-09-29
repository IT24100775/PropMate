import 'package:flutter/material.dart';
import '../../models/property.dart';
import '../../services/favourite_service.dart';
import 'viewing_slots.dart';

class PropertyDetailsPage extends StatefulWidget {
  final Property property;
  final bool initialFavorite;

  const PropertyDetailsPage({
    super.key,
    required this.property,
    this.initialFavorite = false,
  });

  @override
  State<PropertyDetailsPage> createState() => _State();
}

class _State extends State<PropertyDetailsPage> {
  final FavouriteService _fav = FavouriteService();

  late bool _isFav;
  bool _updatingFavourite = false;

  static const Color _background = Color(0xFFF6F4EF);
  static const Color _cardBackground = Color(0xFFFBFAF7);
  static const Color _dark = Color(0xFF181817);
  static const Color _gold = Color(0xFFCFA03C);
  static const Color _mutedText = Color(0xFF777168);
  static const Color _border = Color(0xFFDEDAD1);

  @override
  void initState() {
    super.initState();
    _isFav = widget.initialFavorite;
  }

  Future<void> _toggleFavourite() async {
    if (_updatingFavourite) return;

    setState(() => _updatingFavourite = true);

    bool ok;

    if (_isFav) {
      ok = await _fav.removeFavourite(widget.property.id);
    } else {
      ok = await _fav.addFavourite(widget.property.id);
    }

    if (!mounted) return;

    setState(() {
      _updatingFavourite = false;

      if (ok) {
        _isFav = !_isFav;
      }
    });

    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(
          ok
              ? (_isFav
                  ? 'Added to Favourites'
                  : 'Removed from Favourites')
              : 'Failed to update favourite.',
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final property = widget.property;

    return Scaffold(
      backgroundColor: _background,

      appBar: AppBar(
        backgroundColor: _dark,
        foregroundColor: Colors.white,
        elevation: 0,
        iconTheme: const IconThemeData(
          color: _gold,
        ),
        title: Text(
          property.title,
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
          style: const TextStyle(
            fontSize: 17,
            fontWeight: FontWeight.w600,
          ),
        ),
      ),

      body: SingleChildScrollView(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // --------------------------------------------------
            // PROPERTY HERO
            // --------------------------------------------------

            Container(
              height: 235,
              color: const Color(0xFFE9E5DC),
              child: Stack(
                children: [
                  const Center(
                    child: Icon(
                      Icons.apartment_rounded,
                      size: 72,
                      color: Color(0xFFB9B2A6),
                    ),
                  ),

                  Positioned(
                    top: 18,
                    right: 18,
                    child: Container(
                      decoration: BoxDecoration(
                        color: _cardBackground,
                        border: Border.all(
                          color: _border,
                        ),
                      ),
                      child: IconButton(
                        tooltip: _isFav
                            ? 'Remove from favourites'
                            : 'Save to favourites',
                        onPressed:
                            _updatingFavourite ? null : _toggleFavourite,
                        icon: _updatingFavourite
                            ? const SizedBox(
                                width: 20,
                                height: 20,
                                child: CircularProgressIndicator(
                                  strokeWidth: 2,
                                  color: _gold,
                                ),
                              )
                            : Icon(
                                _isFav
                                    ? Icons.favorite_rounded
                                    : Icons.favorite_border_rounded,
                                color: _isFav
                                    ? const Color(0xFF8D4038)
                                    : _dark,
                              ),
                      ),
                    ),
                  ),
                ],
              ),
            ),

            // --------------------------------------------------
            // PROPERTY CONTENT
            // --------------------------------------------------

            Padding(
              padding: const EdgeInsets.fromLTRB(
                20,
                26,
                20,
                36,
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'PROPERTY DETAILS',
                    style: TextStyle(
                      color: _gold,
                      fontSize: 10,
                      fontWeight: FontWeight.w700,
                      letterSpacing: 1.7,
                    ),
                  ),

                  const SizedBox(height: 9),

                  Text(
                    property.title,
                    style: const TextStyle(
                      color: _dark,
                      fontSize: 27,
                      fontWeight: FontWeight.w600,
                      height: 1.15,
                    ),
                  ),

                  const SizedBox(height: 10),

                  Text(
                    'LKR ${property.price.toStringAsFixed(0)}',
                    style: const TextStyle(
                      color: _gold,
                      fontSize: 21,
                      fontWeight: FontWeight.w700,
                    ),
                  ),

                  const SizedBox(height: 24),

                  // --------------------------------------------------
                  // PROPERTY FACTS
                  // --------------------------------------------------

                  Container(
                    decoration: BoxDecoration(
                      color: _cardBackground,
                      border: Border.all(
                        color: _border,
                      ),
                    ),
                    child: Row(
                      children: [
                        Expanded(
                          child: _buildFact(
                            Icons.bed_outlined,
                            '${property.bedrooms}',
                            'BEDROOMS',
                          ),
                        ),

                        Container(
                          width: 1,
                          height: 72,
                          color: _border,
                        ),

                        Expanded(
                          child: _buildFact(
                            Icons.bathtub_outlined,
                            '${property.bathrooms}',
                            'BATHROOMS',
                          ),
                        ),

                        Container(
                          width: 1,
                          height: 72,
                          color: _border,
                        ),

                        Expanded(
                          child: _buildFact(
                            Icons.location_on_outlined,
                            '',
                            'LOCATION',
                          ),
                        ),
                      ],
                    ),
                  ),

                  const SizedBox(height: 32),

                  // --------------------------------------------------
                  // DESCRIPTION
                  // --------------------------------------------------

                  const Text(
                    'ABOUT THIS PROPERTY',
                    style: TextStyle(
                      color: _gold,
                      fontSize: 10,
                      fontWeight: FontWeight.w700,
                      letterSpacing: 1.5,
                    ),
                  ),

                  const SizedBox(height: 10),

                  const Text(
                    'Description',
                    style: TextStyle(
                      color: _dark,
                      fontSize: 21,
                      fontWeight: FontWeight.w600,
                    ),
                  ),

                  const SizedBox(height: 10),

                  Text(
                    property.description,
                    style: const TextStyle(
                      color: _mutedText,
                      fontSize: 14,
                      height: 1.65,
                    ),
                  ),

                  const SizedBox(height: 34),

                  // --------------------------------------------------
                  // BOOK VIEWING
                  // --------------------------------------------------

                  SizedBox(
                    width: double.infinity,
                    height: 54,
                    child: ElevatedButton(
                      onPressed: () {
                        Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (_) => ViewingSlotsPage(
                              propertyListingId: property.id,
                            ),
                          ),
                        );
                      },
                      style: ElevatedButton.styleFrom(
                        backgroundColor: _dark,
                        foregroundColor: Colors.white,
                        elevation: 0,
                        shape: const RoundedRectangleBorder(
                          borderRadius: BorderRadius.zero,
                        ),
                      ),
                      child: const Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(
                            Icons.calendar_month_outlined,
                            color: _gold,
                            size: 19,
                          ),
                          SizedBox(width: 10),
                          Text(
                            'BOOK A VIEWING',
                            style: TextStyle(
                              fontSize: 11,
                              fontWeight: FontWeight.w700,
                              letterSpacing: 1.3,
                            ),
                          ),
                          SizedBox(width: 10),
                          Icon(
                            Icons.arrow_forward,
                            color: _gold,
                            size: 17,
                          ),
                        ],
                      ),
                    ),
                  ),

                  const SizedBox(height: 12),

                  // --------------------------------------------------
                  // FAVOURITE
                  // --------------------------------------------------

                  SizedBox(
                    width: double.infinity,
                    height: 52,
                    child: OutlinedButton(
                      onPressed:
                          _updatingFavourite ? null : _toggleFavourite,
                      style: OutlinedButton.styleFrom(
                        foregroundColor: _dark,
                        backgroundColor: _cardBackground,
                        side: const BorderSide(
                          color: _border,
                        ),
                        elevation: 0,
                        shape: const RoundedRectangleBorder(
                          borderRadius: BorderRadius.zero,
                        ),
                      ),
                      child: _updatingFavourite
                          ? const SizedBox(
                              width: 20,
                              height: 20,
                              child: CircularProgressIndicator(
                                strokeWidth: 2,
                                color: _gold,
                              ),
                            )
                          : Row(
                              mainAxisAlignment:
                                  MainAxisAlignment.center,
                              children: [
                                Icon(
                                  _isFav
                                      ? Icons.favorite_rounded
                                      : Icons.favorite_border_rounded,
                                  color: _isFav
                                      ? const Color(0xFF8D4038)
                                      : _gold,
                                  size: 19,
                                ),
                                const SizedBox(width: 10),
                                Text(
                                  _isFav
                                      ? 'REMOVE FROM FAVOURITES'
                                      : 'SAVE TO FAVOURITES',
                                  style: const TextStyle(
                                    fontSize: 11,
                                    fontWeight: FontWeight.w700,
                                    letterSpacing: 1,
                                  ),
                                ),
                              ],
                            ),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildFact(
    IconData icon,
    String value,
    String label,
  ) {
    return Padding(
      padding: const EdgeInsets.symmetric(
        vertical: 16,
        horizontal: 6,
      ),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(
            icon,
            color: _gold,
            size: 21,
          ),

          const SizedBox(height: 7),

          if (value.isNotEmpty)
            Text(
              value,
              style: const TextStyle(
                color: _dark,
                fontSize: 15,
                fontWeight: FontWeight.w700,
              ),
            ),

          if (value.isNotEmpty)
            const SizedBox(height: 2),

          Text(
            label,
            textAlign: TextAlign.center,
            style: const TextStyle(
              color: _mutedText,
              fontSize: 9,
              fontWeight: FontWeight.w600,
              letterSpacing: 0.7,
            ),
          ),
        ],
      ),
    );
  }
}