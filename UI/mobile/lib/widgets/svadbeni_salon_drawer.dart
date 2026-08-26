import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_rimac/providers/auth_provider.dart';
import 'package:svadbeni_salon_rimac/utils/routes.dart';

import '../constants/colors.dart';

class SvadbeniSalonDrawer extends StatefulWidget {
  const SvadbeniSalonDrawer({super.key});

  @override
  State<SvadbeniSalonDrawer> createState() => _SvadbeniSalonDrawerState();
}

class _SvadbeniSalonDrawerState extends State<SvadbeniSalonDrawer> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;
      final auth = context.read<AuthProvider>();
      if (auth.profileImageBase64 == null) {
        auth.refreshProfileCache();
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthProvider>();
    final displayName = auth.displayName;
    final avatar = auth.profileImageProvider;

    return Drawer(
      backgroundColor: AppColors.whiteColor,
      child: Column(
        children: [
          DrawerHeader(
            margin: EdgeInsets.zero,
            padding: const EdgeInsets.fromLTRB(16, 16, 16, 12),
            decoration: const BoxDecoration(color: AppColors.primaryColor),
            child: Align(
              alignment: Alignment.bottomLeft,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisAlignment: MainAxisAlignment.end,
                children: [
                  CircleAvatar(
                    radius: 32,
                    backgroundColor: Colors.white24,
                    backgroundImage: avatar,
                    child: avatar == null
                        ? const Icon(Icons.person, size: 36, color: Colors.white)
                        : null,
                  ),
                  const SizedBox(height: 12),
                  const Text(
                    'Svadbeni Salon Rimac',
                    style: TextStyle(
                      color: Colors.white,
                      fontSize: 18,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    displayName,
                    style: const TextStyle(color: Colors.white70, fontSize: 15),
                  ),
                ],
              ),
            ),
          ),
          Expanded(
            child: ListView(
              padding: const EdgeInsets.only(top: 8),
              children: [
                ListTile(
                  leading: const Icon(Icons.home_outlined, color: AppColors.primaryColor),
                  title: const Text(
                    'Početna',
                    style: TextStyle(color: AppColors.textColor),
                  ),
                  onTap: () => Navigator.pushNamed(context, AppRoutes.home),
                ),
                ListTile(
                  leading: const Icon(Icons.card_giftcard, color: AppColors.primaryColor),
                  title: const Text(
                    'Ponude',
                    style: TextStyle(color: AppColors.textColor),
                  ),
                  onTap: () => Navigator.pushNamed(context, AppRoutes.packages),
                ),
                ListTile(
                  leading: const Icon(Icons.history, color: AppColors.primaryColor),
                  title: const Text(
                    'Moje aktivnosti',
                    style: TextStyle(color: AppColors.textColor),
                  ),
                  onTap: () => Navigator.pushNamed(context, AppRoutes.dailyMeetings),
                ),
                ListTile(
                  leading: const Icon(Icons.favorite, color: AppColors.primaryColor),
                  title: const Text(
                    'Svadba',
                    style: TextStyle(color: AppColors.textColor),
                  ),
                  onTap: () => Navigator.pushNamed(context, AppRoutes.wedding),
                ),
                ListTile(
                  leading: const Icon(Icons.star_rate, color: AppColors.primaryColor),
                  title: const Text(
                    'Ocjene',
                    style: TextStyle(color: AppColors.textColor),
                  ),
                  onTap: () => Navigator.pushNamed(context, AppRoutes.review),
                ),
              ],
            ),
          ),
          const Divider(height: 1),
          ListTile(
            leading: const Icon(Icons.person_outline, color: AppColors.primaryColor),
            title: const Text(
              'Profil',
              style: TextStyle(color: AppColors.textColor),
            ),
            onTap: () => Navigator.pushNamed(context, AppRoutes.profile),
          ),
          ListTile(
            leading: const Icon(Icons.logout, color: AppColors.primaryColor),
            title: const Text(
              'Odjava',
              style: TextStyle(color: AppColors.textColor),
            ),
            onTap: () async {
              final confirm = await showDialog<bool>(
                context: context,
                builder: (context) => AlertDialog(
                  title: const Text('Potvrda odjave'),
                  content: const Text('Da li ste sigurni da se želite odjaviti?'),
                  actions: [
                    TextButton(
                      onPressed: () => Navigator.of(context).pop(false),
                      child: const Text('Ne'),
                    ),
                    TextButton(
                      onPressed: () => Navigator.of(context).pop(true),
                      child: const Text('Da'),
                    ),
                  ],
                ),
              );

              if (confirm == true && context.mounted) {
                context.read<AuthProvider>().logout();
                Navigator.pushReplacementNamed(context, AppRoutes.login);
              }
            },
          ),
          const SizedBox(height: 8),
        ],
      ),
    );
  }
}
