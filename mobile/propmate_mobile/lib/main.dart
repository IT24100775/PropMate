import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import 'screens/maintenance_home.dart';
import 'screens/property_listing/published_properties_screen.dart';

void main() {
  runApp(const PropMateApp());
}

class PropMateApp extends StatelessWidget {
  const PropMateApp({super.key});

  @override
  Widget build(BuildContext context) {
    const gold = Color(0xFFCF9E3E);
    const charcoal = Color(0xFF181817);
    const cream = Color(0xFFF7F5F0);

    return MaterialApp(
      debugShowCheckedModeBanner: false,
      title: 'PropMate',
      theme: ThemeData(
        useMaterial3: true,
        scaffoldBackgroundColor: cream,
        colorScheme: ColorScheme.fromSeed(
          seedColor: gold,
          primary: gold,
          surface: cream,
        ),
        textTheme: GoogleFonts.dmSansTextTheme(),
        appBarTheme: AppBarTheme(
          backgroundColor: charcoal,
          foregroundColor: Colors.white,
          elevation: 0,
          centerTitle: false,
          titleTextStyle: GoogleFonts.playfairDisplay(
            color: Colors.white,
            fontSize: 24,
            fontWeight: FontWeight.w700,
          ),
        ),
        elevatedButtonTheme: ElevatedButtonThemeData(
          style: ElevatedButton.styleFrom(
            backgroundColor: gold,
            foregroundColor: Colors.white,
            padding: const EdgeInsets.symmetric(horizontal: 22, vertical: 14),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(10),
            ),
          ),
        ),
        cardTheme: CardThemeData(
          color: Colors.white,
          elevation: 1,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(16),
          ),
        ),
      ),
      home: const TenantHome(tenantId: 1),
    );
  }
}

class TenantHome extends StatefulWidget {
  const TenantHome({super.key, required this.tenantId});

  final int tenantId;

  @override
  State<TenantHome> createState() => _TenantHomeState();
}

class _TenantHomeState extends State<TenantHome> {
  int _selectedIndex = 0;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: IndexedStack(
        index: _selectedIndex,
        children: [
          PublishedPropertiesScreen(),
          MaintenanceHome(
            tenantId: widget.tenantId,
          ),
        ],
      ),
      bottomNavigationBar: NavigationBar(
        selectedIndex: _selectedIndex,
        onDestinationSelected: (index) => setState(() => _selectedIndex = index),
        destinations: const [
          NavigationDestination(
            icon: Icon(Icons.home_work_outlined),
            label: 'Properties',
          ),
          NavigationDestination(
            icon: Icon(Icons.build_outlined),
            label: 'Maintenance',
          ),
        ],
      ),
    );
  }
}
