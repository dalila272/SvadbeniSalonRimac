import 'package:flutter/material.dart';
import 'colors.dart';

class AppTheme {
  static ThemeData lightTheme = ThemeData(
    primaryColor: AppColors.primaryColor,
    scaffoldBackgroundColor: AppColors.backgroundColor,
    appBarTheme: const AppBarTheme(
      backgroundColor: AppColors.primaryColor,
      foregroundColor: AppColors.whiteColor,
      iconTheme: IconThemeData(color: AppColors.whiteColor),
      actionsIconTheme: IconThemeData(color: AppColors.whiteColor),
      centerTitle: true,
      elevation: 0,
      titleTextStyle: TextStyle(
        color: AppColors.whiteColor,
        fontSize: 20,
      ),
    ),
    drawerTheme: const DrawerThemeData(
      backgroundColor: AppColors.whiteColor,
      surfaceTintColor: Colors.transparent,
    ),
    textTheme: const TextTheme(
      bodyLarge: TextStyle(color: AppColors.textColor, fontSize: 16),
      bodyMedium: TextStyle(color: AppColors.textColor, fontSize: 14),
      titleLarge: TextStyle(
        color: AppColors.textColor,
        fontSize: 22,
      ),
    ),
    listTileTheme: const ListTileThemeData(
      selectedColor: AppColors.whiteColor,
      selectedTileColor: AppColors.primaryColor,
      iconColor: AppColors.primaryColor,
      textColor: AppColors.textColor,
      titleTextStyle: TextStyle(
        color: AppColors.textColor,
        fontSize: 16,
      ),
    ),
    elevatedButtonTheme: ElevatedButtonThemeData(
      style: ElevatedButton.styleFrom(
        backgroundColor: AppColors.primaryColor,
        foregroundColor: AppColors.whiteColor,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(8),
        ),
      ),
    ),
    inputDecorationTheme: InputDecorationTheme(
      enabledBorder: OutlineInputBorder(
        borderSide: const BorderSide(color: AppColors.primaryColor),
        borderRadius: BorderRadius.circular(8),
      ),
      focusedBorder: OutlineInputBorder(
        borderSide: const BorderSide(color: AppColors.primaryColor, width: 2),
        borderRadius: BorderRadius.circular(8),
      ),
      border: OutlineInputBorder(
        borderSide: const BorderSide(color: AppColors.primaryColor),
        borderRadius: BorderRadius.circular(8),
      ),
    ),
    colorScheme: ColorScheme.fromSeed(
      seedColor: AppColors.primaryColor,
      brightness: Brightness.light,
      surface: AppColors.whiteColor,
      onSurface: AppColors.textColor,
    ),
  );
}
