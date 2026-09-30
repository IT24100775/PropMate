import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import 'pages/discovery/property_discovery.dart';
import 'screens/auth/login_screen.dart';
import 'services/mobile_auth_service.dart';

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

      home: const AuthGate(),
    );
  }
}

class AuthGate extends StatefulWidget {
  const AuthGate({super.key});

  @override
  State<AuthGate> createState() => _AuthGateState();
}

class _AuthGateState extends State<AuthGate> {
  late Future<bool> _authCheck;

  @override
  void initState() {
    super.initState();
    _authCheck = MobileAuthService.isLoggedIn();
  }

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<bool>(
      future: _authCheck,
      builder: (context, snapshot) {
        if (snapshot.connectionState != ConnectionState.done) {
          return const Scaffold(
            body: Center(
              child: CircularProgressIndicator(),
            ),
          );
        }

        if (snapshot.data == true) {
          return const PropertyDiscoveryPage();
        }

        return const LoginScreen();
      },
    );
  }
}