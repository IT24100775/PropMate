import 'package:flutter/material.dart';
import '../component3/models/transaction_models.dart';
import '../services/maintenance_api.dart';

class CreateRequestScreen extends StatefulWidget {
  const CreateRequestScreen({
    super.key,
    required this.tenantId,
    required this.activeRentals,
  });

  final int tenantId;
  final List<RentalApplication> activeRentals;

  @override
  State<CreateRequestScreen> createState() => _CreateRequestScreenState();
}

class _CreateRequestScreenState extends State<CreateRequestScreen> {
  final _formKey = GlobalKey<FormState>();
  final _descriptionController = TextEditingController();
  final _api = MaintenanceApi();
  String _category = 'General';
  String _priority = 'MEDIUM';
  int? _propertyListingId;
  bool _submitting = false;

  @override
  void initState() {
    super.initState();
    _propertyListingId = widget.activeRentals.firstOrNull?.propertyListingId;
  }

  @override
  void dispose() {
    _descriptionController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _submitting = true);
    try {
      await _api.createRequest(
        propertyId: _propertyListingId!,
        description: _descriptionController.text.trim(),
        category: _category,
        priority: _priority,
      );
      if (!mounted) return;
      Navigator.pop(context, true);
    } catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Could not send request: $error')),
      );
    } finally {
      if (mounted) setState(() => _submitting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('New maintenance request'),
      ),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(20),
          children: [
            DropdownButtonFormField<int>(
              initialValue: _propertyListingId,
              decoration: const InputDecoration(
                labelText: 'Rented property',
                border: OutlineInputBorder(),
              ),
              items: widget.activeRentals
                  .map(
                    (rental) => DropdownMenuItem(
                      value: rental.propertyListingId,
                      child: Text(rental.propertyTitle),
                    ),
                  )
                  .toList(),
              onChanged: (value) => setState(() => _propertyListingId = value),
              validator: (value) => value == null ? 'Select your rented property.' : null,
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _descriptionController,
              maxLines: 5,
              decoration: const InputDecoration(
                labelText: 'What needs fixing?',
                hintText: 'Describe the problem clearly',
                border: OutlineInputBorder(),
              ),
              validator: (value) => value == null || value.trim().isEmpty
                  ? 'Please describe the problem.'
                  : null,
            ),
            const SizedBox(height: 16),
            DropdownButtonFormField<String>(
              initialValue: _category,
              decoration: const InputDecoration(
                labelText: 'Category',
                border: OutlineInputBorder(),
              ),
              items: ['General', 'Plumbing', 'Electrical', 'Appliance']
                  .map((item) => DropdownMenuItem(value: item, child: Text(item)))
                  .toList(),
              onChanged: (value) => setState(() => _category = value!),
            ),
            const SizedBox(height: 16),
            DropdownButtonFormField<String>(
              initialValue: _priority,
              decoration: const InputDecoration(
                labelText: 'Priority',
                border: OutlineInputBorder(),
              ),
              items: ['LOW', 'MEDIUM', 'HIGH']
                  .map((item) => DropdownMenuItem(value: item, child: Text(item)))
                  .toList(),
              onChanged: (value) => setState(() => _priority = value!),
            ),
            const SizedBox(height: 24),
            FilledButton.icon(
              onPressed: _submitting ? null : _submit,
              icon: _submitting
                  ? const SizedBox(
                      height: 18,
                      width: 18,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Icon(Icons.send),
              label: Text(_submitting ? 'Sending...' : 'Send request'),
              style: FilledButton.styleFrom(
                padding: const EdgeInsets.symmetric(vertical: 16),
              ),
            ),
          ],
        ),
      ),
    );
  }
}