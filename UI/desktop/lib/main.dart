import 'package:svadbeni_salon_desktop/providers/article_provider.dart';
import 'package:svadbeni_salon_desktop/providers/auth_provider.dart';
import 'package:svadbeni_salon_desktop/providers/daily_meeting_provider.dart';
import 'package:svadbeni_salon_desktop/providers/decoration_provider.dart';
import 'package:svadbeni_salon_desktop/providers/genre_provider.dart';
import 'package:svadbeni_salon_desktop/providers/menu_provider.dart';
import 'package:svadbeni_salon_desktop/providers/musician_provider.dart';
import 'package:svadbeni_salon_desktop/providers/offer_provider.dart';
import 'package:svadbeni_salon_desktop/providers/salon_review_provider.dart';
import 'package:svadbeni_salon_desktop/providers/user_provider.dart';
import 'package:svadbeni_salon_desktop/providers/wedding_invoice_provider.dart';
import 'package:svadbeni_salon_desktop/providers/wedding_payment_provider.dart';
import 'package:svadbeni_salon_desktop/providers/wedding_provider.dart';
import 'package:svadbeni_salon_desktop/screens/login_screen.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

void main() {
  runApp(
    MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => AuthProvider()),
        ChangeNotifierProvider(create: (_) => OfferProvider()),
        ChangeNotifierProvider(create: (_) => MenuProvider()),
        ChangeNotifierProvider(create: (_) => ArticleProvider()),
        ChangeNotifierProvider(create: (_) => MusicianProvider()),
        ChangeNotifierProvider(create: (_) => DecorationProvider()),
        ChangeNotifierProvider(create: (_) => GenreProvider()),
        ChangeNotifierProvider(create: (_) => WeddingProvider()),
        ChangeNotifierProvider(create: (_) => WeddingPaymentProvider()),
        ChangeNotifierProvider(create: (_) => WeddingInvoiceProvider()),
        ChangeNotifierProvider(create: (_) => DailyMeetingProvider()),
        ChangeNotifierProvider(create: (_) => SalonReviewProvider()),
        ChangeNotifierProvider(create: (_) => UserProvider()),
      ],
      child: const MyApp(),
    ),
  );
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Svadbeni Salon Rimac',
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xFF875252)),
        useMaterial3: true,
      ),
      home: const LoginScreen(),
    );
  }
}
