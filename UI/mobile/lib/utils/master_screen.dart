import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_rimac/constants/colors.dart';
import 'package:svadbeni_salon_rimac/providers/notifications_provider.dart';
import 'package:svadbeni_salon_rimac/utils/routes.dart';
import 'package:svadbeni_salon_rimac/widgets/svadbeni_salon_drawer.dart';

class MasterScreenWidget extends StatefulWidget {
  final Widget? child;
  String? title;
  String? title_widget;

  MasterScreenWidget({this.child, this.title, this.title_widget, Key? key})
      : super(key: key);

  @override
  State<MasterScreenWidget> createState() => _MasterScreenWidget();
}

class _MasterScreenWidget extends State<MasterScreenWidget> {
  @override
  Widget build(BuildContext context) {
    final unread = context.watch<NotificationsProvider>().unreadCount;

    return Scaffold(
      appBar: AppBar(
        title: Text(widget.title ?? 'Svadbeni salon'),
        backgroundColor: AppColors.primaryColor,
        foregroundColor: Colors.white,
        iconTheme: const IconThemeData(color: Colors.white),
        actions: [
          IconButton(
            tooltip: 'Notifikacije',
            onPressed: () =>
                Navigator.pushNamed(context, AppRoutes.notifications),
            icon: Badge(
              isLabelVisible: unread > 0,
              label: Text(unread > 99 ? '99+' : '$unread'),
              child: const Icon(Icons.notifications_outlined),
            ),
          ),
        ],
      ),
      drawer: const SvadbeniSalonDrawer(),
      body: SafeArea(
        child: widget.child!,
      ),
    );
  }
}
