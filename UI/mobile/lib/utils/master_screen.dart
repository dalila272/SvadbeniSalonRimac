import 'package:flutter/material.dart';
import 'package:svadbeni_salon_rimac/constants/colors.dart';
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
  int currentIndex = 0;

  void _onItemTapped(int index) {
    setState(() {
      currentIndex = index;
    });
    if (currentIndex == 0) {
      Navigator.pushNamed(context, AppRoutes.packages);
    } else if (currentIndex == 1) {
      Navigator.pushNamed(context, AppRoutes.dailyMeetings);
    } else if (currentIndex == 2) {
      Navigator.pushNamed(context, AppRoutes.wedding);
    } else if (currentIndex == 3) {
      Navigator.pushNamed(context, AppRoutes.review);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Svadbeni salon'),
        backgroundColor: AppColors.primaryColor,
        foregroundColor: Colors.white,
        iconTheme: const IconThemeData(color: Colors.white),
      ),
      drawer: const SvadbeniSalonDrawer(),
      body: SafeArea(
        child: widget.child!,
      ),
    );
  }
}
