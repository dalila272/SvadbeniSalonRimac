import 'package:flutter/material.dart';
import 'package:svadbeni_salon_rimac/pages/notifications_screen.dart';
import 'package:svadbeni_salon_rimac/pages/interests_screen.dart';
import 'package:svadbeni_salon_rimac/pages/daily_meeting_screen.dart';
import 'package:svadbeni_salon_rimac/pages/forgot_password_screen.dart';
import 'package:svadbeni_salon_rimac/pages/home_screen.dart';
import 'package:svadbeni_salon_rimac/pages/login_screen.dart';
import 'package:svadbeni_salon_rimac/pages/package_details_screen.dart';
import 'package:svadbeni_salon_rimac/pages/packages_screen.dart';
import 'package:svadbeni_salon_rimac/pages/profile_screen.dart';
import 'package:svadbeni_salon_rimac/pages/registration_screen.dart';
import 'package:svadbeni_salon_rimac/pages/reset_password_screen.dart';
import 'package:svadbeni_salon_rimac/pages/review_screen.dart';
import 'package:svadbeni_salon_rimac/pages/wedding_screen.dart';

class AppRoutes {
  static const String home = '/home';
  static const String login = '/login';
  static const String registration = '/registration';
  static const String packages = '/packages';
  static const String packageDetails = '/packageDetails';
  static const String dailyMeetings = '/dailyMeetings';
  static const String review = '/review';
  static const String wedding = '/wedding';
  static const String forgotPassword = '/forgot-password';
  static const String resetPassword = '/reset-password';
  static const String profile = '/profile';
  static const String interests = '/interests';
  static const String notifications = '/notifications';

  static Route<dynamic> generateRoute(RouteSettings settings) {
    switch (settings.name) {
      case login:
        return MaterialPageRoute(builder: (_) => const LoginScreen());
      case registration:
        return MaterialPageRoute(builder: (_) => const RegistrationScreen());
      case interests:
        final fromRegistration = settings.arguments == true;
        return MaterialPageRoute(
          builder: (_) => InterestsScreen(fromRegistration: fromRegistration),
        );
      case notifications:
        return MaterialPageRoute(builder: (_) => const NotificationsScreen());
      case forgotPassword:
        return MaterialPageRoute(builder: (_) => const ForgotPasswordScreen());
      case resetPassword:
        final emailOrUsername = settings.arguments as String? ?? '';
        return MaterialPageRoute(
          builder: (_) => ResetPasswordScreen(emailOrUsername: emailOrUsername),
        );
      case home:
        return MaterialPageRoute(builder: (_) => const HomeScreen());
      case packages:
        return MaterialPageRoute(builder: (_) => const PackagesScreen());
      case dailyMeetings:
        return MaterialPageRoute(builder: (_) => const DailyMeetingScreen());
      case wedding:
        return MaterialPageRoute(builder: (_) => const WeddingScreen());
      case packageDetails:
        final int offerId = settings.arguments as int;
        return MaterialPageRoute(
          builder: (_) => PackageDetailsScreen(offerId: offerId),
        );
      case review:
        return MaterialPageRoute(builder: (_) => const ReviewScreen());
      case profile:
        return MaterialPageRoute(builder: (_) => const ProfileScreen());

      default:
        return MaterialPageRoute(
          builder: (_) => Scaffold(
            body: Center(child: Text('Nema rute za: ${settings.name}')),
          ),
        );
    }
  }
}
