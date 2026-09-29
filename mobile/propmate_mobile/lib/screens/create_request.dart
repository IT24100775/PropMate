import 'package:flutter/material.dart';
import '../services/maintenance_api.dart';

class RentalPropertyOption {
  final int propertyListingId;
  final String title;
  final String? subtitle;

  const RentalPropertyOption({
    required this.propertyListingId,
    required this.title,
    this.subtitle,
  });
}

class CreateRequestScreen extends StatefulWidget {
  const CreateRequestScreen({
    super.key,
    required this.tenantId,
    required this.activeRentals,
    this.initialPropertyId,
  });

  final int tenantId;
  final List<RentalPropertyOption> activeRentals;
  final int? initialPropertyId;

  @override
  State<CreateRequestScreen> createState() => _CreateRequestScreenState();
}

class _CreateRequestScreenState extends State<CreateRequestScreen> {
  final _formKey = GlobalKey<FormState>();
  final _descriptionController = TextEditingController();
  final _imageUrlController = TextEditingController();
  final _phoneController = TextEditingController();
  final _notesController = TextEditingController();

  final _api = MaintenanceApi();

  int? _propertyListingId;
  String _category = 'Plumbing';
  String _priority = 'MEDIUM';
  DateTime? _selectedDate;
  TimeOfDay? _selectedTime;
  String? _attachedImageUrl;
  bool _submitting = false;

  final List<String> _categories = [
    'Plumbing',
    'Electrical',
    'Appliance',
    'HVAC / AC',
    'Carpentry',
    'Painting',
    'Roofing',
    'General',
  ];

  final List<Map<String, String>> _samplePhotoPresets = [
    {
      'label': 'Pipe Leak',
      'url': 'https://images.unsplash.com/photo-1585704032915-c3400ca199e7?w=600&auto=format&fit=crop',
    },
    {
      'label': 'AC Issue',
      'url': 'https://images.unsplash.com/photo-1621905251189-08b45d6a269e?w=600&auto=format&fit=crop',
    },
    {
      'label': 'Electrical',
      'url': 'https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=600&auto=format&fit=crop',
    },
    {
      'label': 'Wall Damage',
      'url': 'https://images.unsplash.com/photo-1590381105924-c72589b9ef3f?w=600&auto=format&fit=crop',
    },
  ];

  @override
  void initState() {
    super.initState();
    if (widget.initialPropertyId != null &&
        widget.activeRentals.any((r) => r.propertyListingId == widget.initialPropertyId)) {
      _propertyListingId = widget.initialPropertyId;
    } else {
      _propertyListingId = widget.activeRentals.firstOrNull?.propertyListingId;
    }
    // Default preferred date to tomorrow
    _selectedDate = DateTime.now().add(const Duration(days: 1));
    _selectedTime = const TimeOfDay(hour: 10, minute: 0);
  }

  @override
  void dispose() {
    _descriptionController.dispose();
    _imageUrlController.dispose();
    _phoneController.dispose();
    _notesController.dispose();
    super.dispose();
  }

  Future<void> _pickDate() async {
    final now = DateTime.now();
    final picked = await showDatePicker(
      context: context,
      initialDate: _selectedDate ?? now.add(const Duration(days: 1)),
      firstDate: now,
      lastDate: now.add(const Duration(days: 90)),
      builder: (context, child) {
        return Theme(
          data: Theme.of(context).copyWith(
            colorScheme: const ColorScheme.light(
              primary: Color(0xFFCF9E3E),
              onPrimary: Colors.white,
              onSurface: Color(0xFF181817),
            ),
          ),
          child: child!,
        );
      },
    );

    if (picked != null) {
      setState(() => _selectedDate = picked);
    }
  }

  Future<void> _pickTime() async {
    final picked = await showTimePicker(
      context: context,
      initialTime: _selectedTime ?? const TimeOfDay(hour: 10, minute: 0),
      builder: (context, child) {
        return Theme(
          data: Theme.of(context).copyWith(
            colorScheme: const ColorScheme.light(
              primary: Color(0xFFCF9E3E),
              onPrimary: Colors.white,
              onSurface: Color(0xFF181817),
            ),
          ),
          child: child!,
        );
      },
    );

    if (picked != null) {
      setState(() => _selectedTime = picked);
    }
  }

  String _formatDate(DateTime date) {
    return '${date.year}-${date.month.toString().padLeft(2, '0')}-${date.day.toString().padLeft(2, '0')}';
  }

  String _formatTime(TimeOfDay time) {
    final hour = time.hourOfPeriod == 0 ? 12 : time.hourOfPeriod;
    final minute = time.minute.toString().padLeft(2, '0');
    final period = time.period == DayPeriod.am ? 'AM' : 'PM';
    return '$hour:$minute $period';
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    if (_propertyListingId == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please select your rented house.')),
      );
      return;
    }

    setState(() => _submitting = true);

    try {
      final preferredDateStr = _selectedDate != null ? _formatDate(_selectedDate!) : null;
      final preferredTimeStr = _selectedTime != null ? _formatTime(_selectedTime!) : null;

      final imageUrl = _attachedImageUrl?.trim().isNotEmpty == true
          ? _attachedImageUrl!.trim()
          : (_imageUrlController.text.trim().isNotEmpty
              ? _imageUrlController.text.trim()
              : null);

      var fullDescription = _descriptionController.text.trim();
      if (_notesController.text.trim().isNotEmpty) {
        fullDescription = '$fullDescription\nAccess Notes: ${_notesController.text.trim()}';
      }

      await _api.createRequest(
        propertyId: _propertyListingId!,
        description: fullDescription,
        category: _category,
        priority: _priority,
        imageUrl: imageUrl,
        preferredDate: preferredDateStr,
        preferredTime: preferredTimeStr,
        contactPhone: _phoneController.text.trim().isNotEmpty
            ? _phoneController.text.trim()
            : null,
      );

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          backgroundColor: Colors.green,
          content: Text('Maintenance request submitted successfully!'),
        ),
      );
      Navigator.pop(context, true);
    } catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          backgroundColor: Colors.red,
          content: Text('Failed to submit request: $error'),
        ),
      );
    } finally {
      if (mounted) setState(() => _submitting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    const gold = Color(0xFFCF9E3E);
    const charcoal = Color(0xFF181817);

    return Scaffold(
      backgroundColor: const Color(0xFFF7F5F0),
      appBar: AppBar(
        title: const Text('New Maintenance Request'),
        backgroundColor: charcoal,
        foregroundColor: Colors.white,
        elevation: 0,
      ),
      body: SafeArea(
        child: Form(
          key: _formKey,
          child: ListView(
            padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 24),
            children: [
              // Header Card
              Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(14),
                  border: Border.all(color: Colors.black12),
                ),
                child: Row(
                  children: [
                    Container(
                      padding: const EdgeInsets.all(10),
                      decoration: BoxDecoration(
                        color: gold.withOpacity(0.12),
                        borderRadius: BorderRadius.circular(10),
                      ),
                      child: const Icon(Icons.build_outlined, color: gold, size: 26),
                    ),
                    const SizedBox(width: 14),
                    const Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            'Report a Property Issue',
                            style: TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.bold,
                              color: charcoal,
                            ),
                          ),
                          SizedBox(height: 2),
                          Text(
                            'Select your rented house and describe the repair needed.',
                            style: TextStyle(fontSize: 12, color: Colors.grey),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),

              const SizedBox(height: 20),

              // 1. SELECT RENTED HOUSE
              _buildSectionTitle('1. Rented House', Icons.home_work_outlined),
              const SizedBox(height: 8),
              if (widget.activeRentals.isEmpty)
                Container(
                  padding: const EdgeInsets.all(14),
                  decoration: BoxDecoration(
                    color: Colors.amber.shade50,
                    borderRadius: BorderRadius.circular(10),
                    border: Border.all(color: Colors.amber.shade300),
                  ),
                  child: const Row(
                    children: [
                      Icon(Icons.warning_amber_rounded, color: Colors.orange),
                      SizedBox(width: 10),
                      Expanded(
                        child: Text(
                          'No rented properties found. Please ensure you have an active rental.',
                          style: TextStyle(fontSize: 13, color: Colors.black87),
                        ),
                      ),
                    ],
                  ),
                )
              else
                DropdownButtonFormField<int>(
                  initialValue: _propertyListingId,
                  decoration: InputDecoration(
                    labelText: 'Select Rented House',
                    prefixIcon: const Icon(Icons.location_city_outlined, color: gold),
                    filled: true,
                    fillColor: Colors.white,
                    border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12),
                    ),
                  ),
                  items: widget.activeRentals
                      .map(
                        (rental) => DropdownMenuItem(
                          value: rental.propertyListingId,
                          child: Text(
                            rental.subtitle != null && rental.subtitle!.isNotEmpty
                                ? '${rental.title} (${rental.subtitle})'
                                : rental.title,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ),
                      )
                      .toList(),
                  onChanged: (value) => setState(() => _propertyListingId = value),
                  validator: (value) =>
                      value == null ? 'Please select your rented house.' : null,
                ),

              const SizedBox(height: 20),

              // 2. CATEGORY & PRIORITY
              _buildSectionTitle('2. Category & Urgency', Icons.category_outlined),
              const SizedBox(height: 8),
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Expanded(
                    flex: 5,
                    child: DropdownButtonFormField<String>(
                      initialValue: _category,
                      decoration: InputDecoration(
                        labelText: 'Category',
                        filled: true,
                        fillColor: Colors.white,
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                      ),
                      items: _categories
                          .map((item) => DropdownMenuItem(value: item, child: Text(item)))
                          .toList(),
                      onChanged: (value) => setState(() => _category = value!),
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    flex: 4,
                    child: DropdownButtonFormField<String>(
                      initialValue: _priority,
                      decoration: InputDecoration(
                        labelText: 'Priority',
                        filled: true,
                        fillColor: Colors.white,
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                      ),
                      items: const [
                        DropdownMenuItem(
                          value: 'LOW',
                          child: Text('🟢 Low', style: TextStyle(color: Colors.green)),
                        ),
                        DropdownMenuItem(
                          value: 'MEDIUM',
                          child: Text('🟠 Medium', style: TextStyle(color: Colors.orange)),
                        ),
                        DropdownMenuItem(
                          value: 'HIGH',
                          child: Text('🔴 High', style: TextStyle(color: Colors.red)),
                        ),
                      ],
                      onChanged: (value) => setState(() => _priority = value!),
                    ),
                  ),
                ],
              ),

              const SizedBox(height: 20),

              // 3. DESCRIPTION
              _buildSectionTitle('3. What Needs Fixing?', Icons.description_outlined),
              const SizedBox(height: 8),
              TextFormField(
                controller: _descriptionController,
                maxLines: 4,
                decoration: InputDecoration(
                  labelText: 'Problem Description',
                  hintText: 'e.g. Water is leaking under the kitchen sink pipe whenever the tap is running...',
                  filled: true,
                  fillColor: Colors.white,
                  alignLabelWithHint: true,
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
                validator: (value) => value == null || value.trim().isEmpty
                    ? 'Please describe the issue.'
                    : null,
              ),

              const SizedBox(height: 20),

              // 4. PREFERRED DATE & TIME
              _buildSectionTitle('4. Preferred Schedule', Icons.schedule_outlined),
              const SizedBox(height: 8),
              Row(
                children: [
                  Expanded(
                    child: OutlinedButton.icon(
                      onPressed: _pickDate,
                      icon: const Icon(Icons.calendar_month_outlined, color: gold, size: 20),
                      label: Text(
                        _selectedDate != null
                            ? _formatDate(_selectedDate!)
                            : 'Select Date',
                        style: const TextStyle(color: charcoal, fontWeight: FontWeight.w600),
                      ),
                      style: OutlinedButton.styleFrom(
                        backgroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(vertical: 14),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: OutlinedButton.icon(
                      onPressed: _pickTime,
                      icon: const Icon(Icons.access_time_outlined, color: gold, size: 20),
                      label: Text(
                        _selectedTime != null
                            ? _formatTime(_selectedTime!)
                            : 'Select Time',
                        style: const TextStyle(color: charcoal, fontWeight: FontWeight.w600),
                      ),
                      style: OutlinedButton.styleFrom(
                        backgroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(vertical: 14),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                      ),
                    ),
                  ),
                ],
              ),

              const SizedBox(height: 20),

              // 5. SUBMIT PHOTOS
              _buildSectionTitle('5. Submit Photos (Optional)', Icons.photo_camera_outlined),
              const SizedBox(height: 8),

              // Image URL input field
              TextFormField(
                controller: _imageUrlController,
                decoration: InputDecoration(
                  labelText: 'Photo Link / Image URL',
                  hintText: 'https://example.com/photo.jpg',
                  prefixIcon: const Icon(Icons.link, color: gold),
                  suffixIcon: _imageUrlController.text.isNotEmpty
                      ? IconButton(
                          icon: const Icon(Icons.clear),
                          onPressed: () {
                            _imageUrlController.clear();
                            setState(() => _attachedImageUrl = null);
                          },
                        )
                      : null,
                  filled: true,
                  fillColor: Colors.white,
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
                onChanged: (val) {
                  setState(() => _attachedImageUrl = val.trim());
                },
              ),

              const SizedBox(height: 10),

              // Preset sample photos for easy 1-tap testing
              const Text(
                'Quick sample photos (tap to attach):',
                style: TextStyle(fontSize: 12, color: Colors.grey),
              ),
              const SizedBox(height: 6),
              Wrap(
                spacing: 8,
                runSpacing: 6,
                children: _samplePhotoPresets.map((preset) {
                  final isSelected = _attachedImageUrl == preset['url'];
                  return ChoiceChip(
                    label: Text(preset['label']!),
                    selected: isSelected,
                    selectedColor: gold.withOpacity(0.2),
                    onSelected: (selected) {
                      setState(() {
                        if (selected) {
                          _attachedImageUrl = preset['url'];
                          _imageUrlController.text = preset['url']!;
                        } else {
                          _attachedImageUrl = null;
                          _imageUrlController.clear();
                        }
                      });
                    },
                  );
                }).toList(),
              ),

              // Image Preview
              if (_attachedImageUrl != null && _attachedImageUrl!.isNotEmpty) ...[
                const SizedBox(height: 12),
                Stack(
                  children: [
                    ClipRRect(
                      borderRadius: BorderRadius.circular(12),
                      child: Image.network(
                        _attachedImageUrl!,
                        height: 160,
                        width: double.infinity,
                        fit: BoxFit.cover,
                        errorBuilder: (context, error, stackTrace) => Container(
                          height: 100,
                          color: Colors.grey.shade200,
                          alignment: Alignment.center,
                          child: const Text('Could not load image preview from URL.'),
                        ),
                      ),
                    ),
                    Positioned(
                      top: 8,
                      right: 8,
                      child: CircleAvatar(
                        backgroundColor: Colors.black54,
                        radius: 16,
                        child: IconButton(
                          padding: EdgeInsets.zero,
                          icon: const Icon(Icons.close, color: Colors.white, size: 18),
                          onPressed: () {
                            setState(() {
                              _attachedImageUrl = null;
                              _imageUrlController.clear();
                            });
                          },
                        ),
                      ),
                    ),
                  ],
                ),
              ],

              const SizedBox(height: 20),

              // 6. CONTACT & ACCESS NOTES
              _buildSectionTitle('6. Contact & Access Notes (Optional)', Icons.contact_phone_outlined),
              const SizedBox(height: 8),
              TextFormField(
                controller: _phoneController,
                keyboardType: TextInputType.phone,
                decoration: InputDecoration(
                  labelText: 'Contact Phone Number',
                  hintText: '+94 77 123 4567',
                  prefixIcon: const Icon(Icons.phone_outlined, color: gold),
                  filled: true,
                  fillColor: Colors.white,
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
              ),

              const SizedBox(height: 12),

              TextFormField(
                controller: _notesController,
                maxLines: 2,
                decoration: InputDecoration(
                  labelText: 'Access Instructions',
                  hintText: 'e.g. Ring front bell twice, someone is available between 10am - 4pm',
                  prefixIcon: const Icon(Icons.info_outline, color: gold),
                  filled: true,
                  fillColor: Colors.white,
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
              ),

              const SizedBox(height: 32),

              // SUBMIT BUTTON
              SizedBox(
                width: double.infinity,
                child: ElevatedButton.icon(
                  onPressed: _submitting ? null : _submit,
                  icon: _submitting
                      ? const SizedBox(
                          height: 20,
                          width: 20,
                          child: CircularProgressIndicator(
                            strokeWidth: 2,
                            color: Colors.white,
                          ),
                        )
                      : const Icon(Icons.send_rounded),
                  label: Text(
                    _submitting ? 'Submitting...' : 'Submit Maintenance Request',
                    style: const TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: gold,
                    foregroundColor: Colors.white,
                    padding: const EdgeInsets.symmetric(vertical: 16),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildSectionTitle(String title, IconData icon) {
    const charcoal = Color(0xFF181817);
    const gold = Color(0xFFCF9E3E);

    return Row(
      children: [
        Icon(icon, size: 18, color: gold),
        const SizedBox(width: 8),
        Text(
          title,
          style: const TextStyle(
            fontSize: 15,
            fontWeight: FontWeight.bold,
            color: charcoal,
          ),
        ),
      ],
    );
  }
}