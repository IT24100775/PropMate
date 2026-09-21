import 'package:flutter/material.dart';
import 'screens/maintenance_home.dart';

void main() {
  runApp(const PropMateApp());
}

class PropMateApp extends StatelessWidget {
  const PropMateApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      debugShowCheckedModeBanner: false,
      title: 'PropMate',
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(
          seedColor: Colors.indigo,
        ),
        useMaterial3: true,
      ),
      home: const MaintenanceHome(),
    );
  }
}