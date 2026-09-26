import 'package:flutter/material.dart';

import '../models/transaction_models.dart';
import '../services/component3_api.dart';
import '../widgets/ai_assistant_panel.dart';

class Component3NegotiationScreen extends StatefulWidget {
  final bool rental;
  final int transactionId;
  final String title;

  const Component3NegotiationScreen({
    super.key,
    required this.rental,
    required this.transactionId,
    required this.title,
  });

  @override
  State<Component3NegotiationScreen> createState() =>
      _Component3NegotiationScreenState();
}

class _Component3NegotiationScreenState
    extends State<Component3NegotiationScreen> {
  final api = Component3Api();

  final message = TextEditingController();
  final amount = TextEditingController();
  final rent = TextEditingController();
  final duration = TextEditingController(text: '12');
  final conditions = TextEditingController();

  DateTime? moveIn;

  bool loading = true;

  List<dynamic> offers = [];
  List<NegotiationMessage> messages = [];
  dynamic agreement;

  @override
  void initState() {
    super.initState();
    load();
  }

  Future<void> load() async {
    try {
      final o = widget.rental
          ? await api.rentalOffers(widget.transactionId)
          : await api.purchaseOffers(widget.transactionId);

      final m = widget.rental
          ? await api.rentalMessages(widget.transactionId)
          : await api.purchaseMessages(widget.transactionId);

      final a = widget.rental
          ? await api.rentalAgreement(widget.transactionId)
          : await api.purchaseAgreement(widget.transactionId);

      if (mounted) {
        setState(() {
          offers = o;
          messages = m;
          agreement = a;
          loading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          loading = false;
        });

        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(e.toString())),
        );
      }
    }
  }

  Future<void> sendMessage() async {
    if (message.text.trim().isEmpty) {
      return;
    }

    try {
      if (widget.rental) {
        await api.sendRentalMessage(
          widget.transactionId,
          message.text.trim(),
        );
      } else {
        await api.sendPurchaseMessage(
          widget.transactionId,
          message.text.trim(),
        );
      }

      message.clear();
      await load();
    } catch (e) {
      _err(e);
    }
  }

  Future<void> counter() async {
    try {
      if (widget.rental) {
        final r = double.tryParse(rent.text.trim());
        final d = int.tryParse(duration.text.trim());

        if (r == null || d == null || moveIn == null) {
          throw Exception(
            'Complete rent, move-in date and duration.',
          );
        }

        await api.counterRental(
          widget.transactionId,
          monthlyRent: r,
          moveInDate: moveIn!,
          durationMonths: d,
          conditions: conditions.text.trim().isEmpty
              ? null
              : conditions.text.trim(),
        );
      } else {
        final a = double.tryParse(amount.text.trim());

        if (a == null) {
          throw Exception('Enter a valid counter amount.');
        }

        await api.counterPurchase(
          widget.transactionId,
          offerAmount: a,
          conditions: conditions.text.trim().isEmpty
              ? null
              : conditions.text.trim(),
        );
      }

      await load();
    } catch (e) {
      _err(e);
    }
  }

  Future<void> accept(dynamic offer) async {
    try {
      if (widget.rental) {
        await api.acceptRentalCounter(
          widget.transactionId,
          offer.id,
        );
      } else {
        await api.acceptPurchaseCounter(
          widget.transactionId,
          offer.id,
        );
      }

      await load();
    } catch (e) {
      _err(e);
    }
  }

  Future<void> confirm() async {
    try {
      if (widget.rental) {
        await api.confirmRentalAgreement(widget.transactionId);
      } else {
        await api.confirmPurchaseAgreement(widget.transactionId);
      }

      await load();
    } catch (e) {
      _err(e);
    }
  }

  void _err(Object e) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(e.toString())),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(widget.title),
      ),
      body: loading
          ? const Center(
              child: CircularProgressIndicator(),
            )
          : ListView(
              padding: const EdgeInsets.all(16),
              children: [
                Component3AiAssistantPanel(
                  transactionType: widget.rental
                      ? 'Rental negotiation'
                      : 'Purchase negotiation',
                  transactionId: widget.transactionId,
                ),

                if (agreement != null) _agreement(),

                const SizedBox(height: 12),

                Text(
                  'Counter-offer history',
                  style: Theme.of(context).textTheme.titleLarge,
                ),

                ...offers.map(
                  (o) => Card(
                    child: ListTile(
                      title: Text(
                        widget.rental
                            ? 'LKR ${o.monthlyRent.toStringAsFixed(0)} / month'
                            : 'LKR ${o.offerAmount.toStringAsFixed(0)}',
                      ),
                      subtitle: Text(
                        '${o.status}\n${o.conditions ?? ''}',
                      ),
                      isThreeLine: true,
                      trailing: o.status
                              .toString()
                              .toLowerCase()
                              .contains('pending')
                          ? TextButton(
                              onPressed: () => accept(o),
                              child: const Text('Accept'),
                            )
                          : null,
                    ),
                  ),
                ),

                const Divider(),

                Text(
                  'Send counter-offer',
                  style: Theme.of(context).textTheme.titleLarge,
                ),

                if (widget.rental) ...[
                  _field(
                    rent,
                    'Monthly rent',
                  ),
                  _field(
                    duration,
                    'Duration months',
                  ),
                  ListTile(
                    contentPadding: EdgeInsets.zero,
                    title: Text(
                      moveIn == null
                          ? 'Choose move-in date'
                          : 'Move-in: ${moveIn!.toIso8601String().split('T').first}',
                    ),
                    trailing: const Icon(
                      Icons.calendar_today,
                    ),
                    onTap: () async {
                      final d = await showDatePicker(
                        context: context,
                        firstDate: DateTime.now(),
                        lastDate: DateTime.now().add(
                          const Duration(days: 3650),
                        ),
                        initialDate: DateTime.now().add(
                          const Duration(days: 7),
                        ),
                      );

                      if (d != null) {
                        setState(() {
                          moveIn = d;
                        });
                      }
                    },
                  ),
                ] else
                  _field(
                    amount,
                    'Offer amount',
                  ),

                _field(
                  conditions,
                  'Conditions',
                  maxLines: 3,
                ),

                ElevatedButton(
                  onPressed: counter,
                  child: const Text(
                    'Send counter-offer',
                  ),
                ),

                const SizedBox(height: 18),

                Text(
                  'Messages',
                  style: Theme.of(context).textTheme.titleLarge,
                ),

                ...messages.map(
                  (m) => ListTile(
                    title: Text(m.senderName),
                    subtitle: Text(m.message),
                    trailing: Text(
                      m.createdAt
                          .toLocal()
                          .toString()
                          .substring(0, 16),
                    ),
                  ),
                ),

                Row(
                  children: [
                    Expanded(
                      child: TextField(
                        controller: message,
                        decoration: const InputDecoration(
                          hintText: 'Write a message',
                        ),
                      ),
                    ),
                    IconButton(
                      onPressed: sendMessage,
                      icon: const Icon(Icons.send),
                    ),
                  ],
                ),
              ],
            ),
    );
  }

  Widget _field(
    TextEditingController controller,
    String label, {
    int maxLines = 1,
  }) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 10),
      child: TextField(
        controller: controller,
        maxLines: maxLines,
        keyboardType: TextInputType.number,
        decoration: InputDecoration(
          labelText: label,
          border: const OutlineInputBorder(),
        ),
      ),
    );
  }

  Widget _agreement() {
    final a = agreement;

    final bool buyer = a.buyerConfirmed;
    final bool seller = a.sellerConfirmed;

    return Card(
      color: Colors.white,
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Agreement',
              style: Theme.of(context).textTheme.titleLarge,
            ),

            const SizedBox(height: 8),

            Text(
              widget.rental
                  ? 'Final monthly rent: LKR ${a.finalMonthlyRent.toStringAsFixed(0)}'
                  : 'Final purchase price: LKR ${a.finalPurchasePrice.toStringAsFixed(0)}',
            ),

            const SizedBox(height: 8),

            Text(
              widget.rental
                  ? a.tenantObligation
                  : a.buyerObligation,
            ),

            const SizedBox(height: 8),

            Text(
              widget.rental
                  ? a.ownerObligation
                  : a.sellerObligation,
            ),

            const SizedBox(height: 8),

            Text(
              a.penaltyTerms,
            ),

            const SizedBox(height: 12),

            Text(
              'Buyer/Tenant confirmed: $buyer',
            ),

            Text(
              'Seller/Owner confirmed: $seller',
            ),

            const SizedBox(height: 12),

            if (!buyer || !seller)
              ElevatedButton(
                onPressed: confirm,
                child: const Text(
                  'Confirm Agreement',
                ),
              ),
          ],
        ),
      ),
    );
  }
}