import 'package:flutter/material.dart';

class CreateRequestScreen extends StatelessWidget {
  const CreateRequestScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Create Maintenance Request'),
      ),
      body: const Center(
        child: Text(
          'Create Request Form',
          style: TextStyle(
            fontSize: 20,
            fontWeight: FontWeight.bold,
          ),
        ),
      ),
    );
  }
}