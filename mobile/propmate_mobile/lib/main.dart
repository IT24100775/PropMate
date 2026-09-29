import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import 'screens/property_listing/published_properties_screen.dart';

import 'dart:convert';
import 'package:http/http.dart' as http;
import 'component3/services/component3_api.dart';

Future<void> setupDevAuth() async {
  try {
    final response = await http.post(
      Uri.parse('http://localhost:5235/api/auth/login'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'email': 'buyer@propmate.com', 'password': 'Password123!'}),
    );
    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      Component3Api.devToken = data['token'];
    }
  } catch (e) {
    debugPrint('Dev auth setup failed: $e');
  }
}

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await setupDevAuth();
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
      title: 'PropMate',
      debugShowCheckedModeBanner: false,

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
            padding: const EdgeInsets.symmetric(
              horizontal: 22,
              vertical: 14,
            ),
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

      home: const PublishedPropertiesScreen(),
    );
  }
}