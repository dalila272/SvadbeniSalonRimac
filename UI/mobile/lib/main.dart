import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_rimac/constants/theme.dart';
import 'package:svadbeni_salon_rimac/providers/auth_provider.dart';
import 'package:svadbeni_salon_rimac/utils/routes.dart';

void main() {
  runApp(
    ChangeNotifierProvider(
      create: (_) => AuthProvider(),
      child: const MyApp(),
    ),
  );
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      initialRoute: AppRoutes.login,
      onGenerateRoute: AppRoutes.generateRoute,
      theme: AppTheme.lightTheme,
    );
  }
}
