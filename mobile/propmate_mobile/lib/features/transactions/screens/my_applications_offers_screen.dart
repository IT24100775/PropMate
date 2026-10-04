import 'package:flutter/material.dart';

import '../models/transaction_models.dart';
import '../services/component3_api.dart';
import 'negotiation_screen.dart';

class MyApplicationsOffersScreen extends StatefulWidget {
  const MyApplicationsOffersScreen({super.key});

  @override
  State<MyApplicationsOffersScreen> createState() =>
      _MyApplicationsOffersScreenState();
}

class _MyApplicationsOffersScreenState
    extends State<MyApplicationsOffersScreen> {
  final Component3Api _api = Component3Api();

  bool _loading = true;
  String? _error;

  List<RentalApplication> _rentalApplications = [];
  List<PurchaseOffer> _purchaseOffers = [];

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });

    try {
      final results = await Future.wait([
        _api.myRentalApplications(),
        _api.myPurchaseOffers(),
      ]);

      if (!mounted) return;

      setState(() {
        _rentalApplications =
            results[0] as List<RentalApplication>;
        _purchaseOffers =
            results[1] as List<PurchaseOffer>;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;

      setState(() {
        _error = e.toString();
        _loading = false;
      });
    }
  }

  bool _canOpenTransaction(
    String status,
    String negotiationStatus,
  ) {
    final value = status.toLowerCase();
    final negotiation = negotiationStatus.toLowerCase();

    return value.contains('accepted') ||
        value.contains('innegotiation') ||
        value.contains('agreementgenerated') ||
        value.contains('completed') ||
        negotiation.contains('open') ||
        negotiation.contains('closedaccepted');
  }

  void _openRental(RentalApplication application) {
    if (!_canOpenTransaction(
      application.status,
      application.negotiationStatus,
    )) {
      return;
    }

    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (_) => Component3NegotiationScreen(
          rental: true,
          transactionId: application.id,
          title: application.propertyTitle.isNotEmpty
              ? application.propertyTitle
              : 'Rental Application #${application.id}',
        ),
      ),
    ).then((_) => _load());
  }

  void _openPurchase(PurchaseOffer offer) {
    if (!_canOpenTransaction(
      offer.status,
      offer.negotiationStatus,
    )) {
      return;
    }

    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (_) => Component3NegotiationScreen(
          rental: false,
          transactionId: offer.id,
          title: offer.propertyTitle.isNotEmpty
              ? offer.propertyTitle
              : 'Purchase Offer #${offer.id}',
        ),
      ),
    ).then((_) => _load());
  }

  @override
  Widget build(BuildContext context) {
    return DefaultTabController(
      length: 2,
      child: Scaffold(
        appBar: AppBar(
          title: const Text('My Applications & Offers'),
          bottom: const TabBar(
            tabs: [
              Tab(text: 'Rentals'),
              Tab(text: 'Purchases'),
            ],
          ),
        ),
        body: _loading
            ? const Center(
                child: CircularProgressIndicator(),
              )
            : _error != null
                ? _buildError()
                : TabBarView(
                    children: [
                      RefreshIndicator(
                        onRefresh: _load,
                        child: _buildRentals(),
                      ),
                      RefreshIndicator(
                        onRefresh: _load,
                        child: _buildPurchases(),
                      ),
                    ],
                  ),
      ),
    );
  }

  Widget _buildError() {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(
              Icons.error_outline,
              size: 42,
            ),
            const SizedBox(height: 12),
            Text(
                'Unable to load your applications and offers. Please try again.',
                textAlign: TextAlign.center,
            ),
            const SizedBox(height: 12),
            ElevatedButton(
              onPressed: _load,
              child: const Text('Try Again'),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildRentals() {
    if (_rentalApplications.isEmpty) {
      return _emptyList(
        'No rental applications yet.',
      );
    }

    return ListView.builder(
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.all(16),
      itemCount: _rentalApplications.length,
      itemBuilder: (context, index) {
        final application = _rentalApplications[index];

        final canOpen = _canOpenTransaction(
          application.status,
          application.negotiationStatus,
        );

        return Card(
          margin: const EdgeInsets.only(bottom: 12),
          child: ListTile(
            contentPadding: const EdgeInsets.all(16),
            leading: const CircleAvatar(
              child: Icon(Icons.home_outlined),
            ),
            title: Text(
              application.propertyTitle.isNotEmpty
                  ? application.propertyTitle
                  : 'Rental Application #${application.id}',
            ),
            subtitle: Padding(
              padding: const EdgeInsets.only(top: 8),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('Status: ${application.status}'),
                  const SizedBox(height: 4),
                  Text(
                    'Negotiation: ${application.negotiationStatus}',
                  ),
                  const SizedBox(height: 4),
                  Text(
                    'Move-in: '
                    '${application.preferredMoveInDate.toLocal().toString().split(' ').first}',
                  ),
                ],
              ),
            ),
            trailing: canOpen
                ? const Icon(Icons.chevron_right)
                : null,
            onTap: canOpen
                ? () => _openRental(application)
                : null,
          ),
        );
      },
    );
  }

  Widget _buildPurchases() {
    if (_purchaseOffers.isEmpty) {
      return _emptyList(
        'No purchase offers yet.',
      );
    }

    return ListView.builder(
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.all(16),
      itemCount: _purchaseOffers.length,
      itemBuilder: (context, index) {
        final offer = _purchaseOffers[index];

        final canOpen = _canOpenTransaction(
          offer.status,
          offer.negotiationStatus,
        );

        return Card(
          margin: const EdgeInsets.only(bottom: 12),
          child: ListTile(
            contentPadding: const EdgeInsets.all(16),
            leading: const CircleAvatar(
              child: Icon(Icons.sell_outlined),
            ),
            title: Text(
              offer.propertyTitle.isNotEmpty
                  ? offer.propertyTitle
                  : 'Purchase Offer #${offer.id}',
            ),
            subtitle: Padding(
              padding: const EdgeInsets.only(top: 8),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('Status: ${offer.status}'),
                  const SizedBox(height: 4),
                  Text(
                    'Negotiation: ${offer.negotiationStatus}',
                  ),
                  const SizedBox(height: 4),
                  Text(
                    'Offer: LKR ${offer.offerAmount.toStringAsFixed(0)}',
                  ),
                ],
              ),
            ),
            trailing: canOpen
                ? const Icon(Icons.chevron_right)
                : null,
            onTap: canOpen
                ? () => _openPurchase(offer)
                : null,
          ),
        );
      },
    );
  }

  Widget _emptyList(String message) {
    return ListView(
      physics: const AlwaysScrollableScrollPhysics(),
      children: [
        SizedBox(
          height: 350,
          child: Center(
            child: Text(
              message,
              style: const TextStyle(
                color: Colors.grey,
              ),
            ),
          ),
        ),
      ],
    );
  }
}