import 'package:svadbeni_salon_desktop/providers/auth_provider.dart';
import 'package:svadbeni_salon_desktop/screens/dashboard_screen.dart';
import 'package:svadbeni_salon_desktop/screens/daily_meeting_list_screen.dart';
import 'package:svadbeni_salon_desktop/screens/decoration_list_screen.dart';
import 'package:svadbeni_salon_desktop/screens/musician_list_screen.dart';
import 'package:svadbeni_salon_desktop/screens/employee_list_screen.dart';
import 'package:svadbeni_salon_desktop/screens/genre_list_screen.dart';
import 'package:svadbeni_salon_desktop/screens/login_screen.dart';
import 'package:svadbeni_salon_desktop/screens/article_list_screen.dart';
import 'package:svadbeni_salon_desktop/screens/menu_list_screen.dart';
import 'package:svadbeni_salon_desktop/screens/offer_list_screen.dart';
import 'package:svadbeni_salon_desktop/screens/reports_screen.dart';
import 'package:svadbeni_salon_desktop/screens/salon_review_list_screen.dart';
import 'package:svadbeni_salon_desktop/screens/wedding_list_screen.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_desktop/constants/role_names.dart';

class MasterScreen extends StatefulWidget {
  const MasterScreen({super.key, required this.child, required this.title});
  final Widget child;
  final String title;

  @override
  State<MasterScreen> createState() => _MasterScreenState();
}

class _MasterScreenState extends State<MasterScreen> {
  final _scaffoldKey = GlobalKey<ScaffoldState>();
  bool _isDrawerOpen = false;

  void _navigate(Widget screen) {
    _scaffoldKey.currentState?.closeDrawer();
    Navigator.pushReplacement(
      context,
      MaterialPageRoute(builder: (_) => screen),
    );
  }

  void _handleBack() {
    if (_scaffoldKey.currentState?.isDrawerOpen ?? false) {
      _scaffoldKey.currentState!.closeDrawer();
      return;
    }
    if (Navigator.of(context).canPop()) {
      Navigator.of(context).pop();
    }
  }

  bool get _showBack =>
      _isDrawerOpen || Navigator.of(context).canPop();

  bool get _allowPop => _showBack;

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthProvider>();

    return PopScope(
      canPop: _allowPop,
      onPopInvokedWithResult: (didPop, result) {
        if (didPop) return;
        _handleBack();
      },
      child: Scaffold(
        key: _scaffoldKey,
        onDrawerChanged: (isOpen) {
          if (_isDrawerOpen != isOpen) {
            setState(() => _isDrawerOpen = isOpen);
          }
        },
        appBar: AppBar(
          title: Text(widget.title),
          centerTitle: true,
          backgroundColor: const Color(0xFF875252),
          foregroundColor: Colors.white,
          iconTheme: const IconThemeData(color: Colors.white),
          leading: _showBack
              ? BackButton(onPressed: _handleBack)
              : null,
          actions: [
            if (Navigator.of(context).canPop())
              IconButton(
                icon: const Icon(Icons.close),
                tooltip: 'Zatvori',
                onPressed: () => Navigator.of(context).pop(),
              ),
          ],
        ),
        drawer: Drawer(
          backgroundColor: Colors.white,
          child: ListView(
            padding: EdgeInsets.zero,
            children: [
              DrawerHeader(
                decoration: const BoxDecoration(color: Color(0xFF875252)),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  mainAxisAlignment: MainAxisAlignment.end,
                  children: [
                    const Text(
                      'Svadbeni Salon Rimac',
                      style: TextStyle(color: Colors.white, fontSize: 22),
                    ),
                    if (auth.role != null) ...[
                      const SizedBox(height: 8),
                      Text(
                        auth.isAdmin ? RoleNames.admin : RoleNames.zaposlenik,
                        style: const TextStyle(color: Colors.white70),
                      ),
                    ],
                  ],
                ),
              ),
              ListTile(
                leading: const Icon(Icons.dashboard_outlined),
                title: const Text('Početna'),
                onTap: () => _navigate(const DashboardScreen()),
              ),
              if (auth.isAdmin) ...[
                ListTile(
                  leading: const Icon(Icons.card_giftcard),
                  title: const Text('Ponude'),
                  onTap: () => _navigate(const OfferListScreen()),
                ),
                ListTile(
                  leading: const Icon(Icons.music_note),
                  title: const Text('Muzičari'),
                  onTap: () => _navigate(const MusicianListScreen()),
                ),
                ListTile(
                  leading: const Icon(Icons.library_music_outlined),
                  title: const Text('Žanrovi'),
                  onTap: () => _navigate(const GenreListScreen()),
                ),
                ListTile(
                  leading: const Icon(Icons.palette_outlined),
                  title: const Text('Dekoracije'),
                  onTap: () => _navigate(const DecorationListScreen()),
                ),
                ListTile(
                  leading: const Icon(Icons.badge_outlined),
                  title: const Text('Zaposlenici'),
                  onTap: () => _navigate(const EmployeeListScreen()),
                ),
              ],
              if (auth.isAdmin || auth.isEmployee) ...[
                ListTile(
                  leading: const Icon(Icons.restaurant_menu),
                  title: const Text('Meniji'),
                  onTap: () => _navigate(const MenuListScreen()),
                ),
                ListTile(
                  leading: const Icon(Icons.restaurant),
                  title: const Text('Artikli'),
                  onTap: () => _navigate(const ArticleListScreen()),
                ),
              ],
              ListTile(
                leading: const Icon(Icons.favorite),
                title: const Text('Svadbe'),
                onTap: () => _navigate(const WeddingListScreen()),
              ),
              ListTile(
                leading: const Icon(Icons.event),
                title: const Text('Dnevni sastanci'),
                onTap: () => _navigate(const DailyMeetingListScreen()),
              ),
              if (auth.isAdmin)
                ListTile(
                  leading: const Icon(Icons.star_rate),
                  title: const Text('Recenzije'),
                  onTap: () => _navigate(const SalonReviewListScreen()),
                ),
              if (auth.isAdmin || auth.isEmployee)
                ListTile(
                  leading: const Icon(Icons.summarize),
                  title: const Text('Izvještaji'),
                  onTap: () => _navigate(const ReportsScreen()),
                ),
              const Divider(),
              ListTile(
                leading: const Icon(Icons.logout),
                title: const Text('Odjava'),
                onTap: () {
                  showDialog(
                    context: context,
                    builder: (ctx) => AlertDialog(
                      title: const Text('Odjava'),
                      content: const Text('Da li ste sigurni?'),
                      actions: [
                        TextButton(
                          onPressed: () => Navigator.pop(ctx),
                          child: const Text('Ne'),
                        ),
                        TextButton(
                          onPressed: () {
                            Navigator.pop(ctx);
                            _scaffoldKey.currentState?.closeDrawer();
                            context.read<AuthProvider>().logout();
                            Navigator.pushAndRemoveUntil(
                              context,
                              MaterialPageRoute(
                                builder: (_) => const LoginScreen(),
                              ),
                              (route) => false,
                            );
                          },
                          child: const Text('Da'),
                        ),
                      ],
                    ),
                  );
                },
              ),
            ],
          ),
        ),
        body: widget.child,
      ),
    );
  }
}
