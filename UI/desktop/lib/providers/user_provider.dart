import 'dart:convert';

import 'package:http/http.dart' as http;
import 'package:svadbeni_salon_desktop/providers/base_provider.dart';

import '../models/user.dart';

class UserProvider extends BaseProvider<User> {
  UserProvider() : super("Users");

  @override
  User fromJson(data) {
    return User.fromJson(data as Map<String, dynamic>);
  }

  Future<void> setPassword(
    int userId,
    String newPassword,
    String confirmNewPassword,
  ) async {
    final uri = Uri.parse('${BaseProvider.baseUrl}$endpoint/$userId/SetPassword');
    final response = await http.put(
      uri,
      headers: createHeaders(),
      body: jsonEncode({
        'newPassword': newPassword,
        'confirmNewPassword': confirmNewPassword,
      }),
    );
    validateResponse(response);
  }
}
