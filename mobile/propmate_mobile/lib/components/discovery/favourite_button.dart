import 'package:flutter/material.dart';

import '../../models/favourite.dart';
import '../../services/favourite_service.dart';

class FavouriteButton extends StatefulWidget {
  final int propertyId;
  final int userId;

  const FavouriteButton({
    super.key,
    required this.propertyId,
    required this.userId,
  });

  @override
  State<FavouriteButton> createState() => _FavouriteButtonState();
}

class _FavouriteButtonState extends State<FavouriteButton> {
  final FavouriteService _favouriteService = FavouriteService();

  Favourite? _favourite;
  bool _isLoading = false;

  Future<void> _toggleFavourite() async {
    if (_isLoading) return;

    setState(() {
      _isLoading = true;
    });

    try {
      if (_favourite == null) {
        final favourite = await _favouriteService.addFavourite(
          propertyId: widget.propertyId,
          userId: widget.userId,
        );

        if (!mounted) return;

        setState(() {
          _favourite = favourite;
          _isLoading = false;
        });

        ScaffoldMessenger.of(context)
            .showSnackBar(const SnackBar(content: Text('Added to favourites')));
      } else {
        await _favouriteService.removeFavourite(_favourite!.id);

        if (!mounted) return;

        setState(() {
          _favourite = null;
          _isLoading = false;
        });

        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Removed from favourites')),
        );
      }
    } catch (e) {
      if (!mounted) return;

      setState(() {
        _isLoading = false;
      });

      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text(e.toString())));
    }
  }

  @override
  Widget build(BuildContext context) {
    return IconButton(
      onPressed: _isLoading ? null : _toggleFavourite,
      icon: _isLoading
          ? const SizedBox(
              width: 20,
              height: 20,
              child: CircularProgressIndicator(strokeWidth: 2),
            )
          : Icon(_favourite == null ? Icons.favorite_border : Icons.favorite),
      tooltip: _favourite == null
          ? 'Add to favourites'
          : 'Remove from favourites',
    );
  }
}
