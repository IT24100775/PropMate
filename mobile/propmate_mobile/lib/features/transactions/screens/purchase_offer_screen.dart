import 'package:flutter/material.dart';

import '../services/component3_api.dart';
import 'my_applications_offers_screen.dart';

class PurchaseOfferScreen extends StatefulWidget {
  final int propertyId;

  const PurchaseOfferScreen({
    super.key,
    required this.propertyId,
  });

  @override
  State<PurchaseOfferScreen> createState() =>
      _PurchaseOfferScreenState();
}

class _PurchaseOfferScreenState
    extends State<PurchaseOfferScreen> {
  final Component3Api api = Component3Api();

  final TextEditingController amount =
      TextEditingController();

  final TextEditingController conditions =
      TextEditingController();

  bool saving = false;

  Future<void> submit() async {
    final value = double.tryParse(
      amount.text.trim(),
    );

    if (value == null || value <= 0) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text(
            'Enter a valid offer amount.',
          ),
        ),
      );
      return;
    }

    setState(() {
      saving = true;
    });

    try {
      await api.createPurchaseOffer(
        propertyListingId: widget.propertyId,
        offerAmount: value,
        conditions: conditions.text.trim().isEmpty
            ? null
            : conditions.text.trim(),
      );

      if (!mounted) return;

      Navigator.pushReplacement(
        context,
        MaterialPageRoute(
          builder: (_) =>
              const MyApplicationsOffersScreen(),
        ),
      );
    } catch (e) {
      if (!mounted) return;

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            e.toString(),
          ),
        ),
      );
    } finally {
      if (mounted) {
        setState(() {
          saving = false;
        });
      }
    }
  }

  @override
  void dispose() {
    amount.dispose();
    conditions.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text(
          'Make Purchase Offer',
        ),
      ),
      body: ListView(
        padding: const EdgeInsets.all(20),
        children: [
          TextField(
            controller: amount,
            keyboardType: const TextInputType.numberWithOptions(
              decimal: true,
            ),
            decoration: const InputDecoration(
              labelText: 'Offer amount',
              border: OutlineInputBorder(),
            ),
          ),

          const SizedBox(height: 14),

          TextField(
            controller: conditions,
            maxLines: 5,
            decoration: const InputDecoration(
              labelText: 'Conditions',
              border: OutlineInputBorder(),
            ),
          ),

          const SizedBox(height: 18),

          ElevatedButton(
            onPressed: saving ? null : submit,
            child: Text(
              saving
                  ? 'Submitting...'
                  : 'Submit Offer',
            ),
          ),
        ],
      ),
    );
  }
}