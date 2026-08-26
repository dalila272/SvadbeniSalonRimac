import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:svadbeni_salon_desktop/constants/role_names.dart';
import 'package:svadbeni_salon_desktop/utils/api_client_exception.dart';
import 'package:svadbeni_salon_desktop/utils/api_config.dart';

class AuthProvider extends ChangeNotifier {
  bool _isAuthenticated = false;
  static String? _accesstoken;
  static String? _refreshtoken;
  static AuthProvider? _instance;
  String? _role;

  static String? get accesstoken => _accesstoken;
  String? get refreshtoken => _refreshtoken;
  String? get role => _role;

  bool get isAdmin => _role == RoleNames.admin;
  bool get isEmployee => _role == RoleNames.zaposlenik;
  bool get canAccessDesktop => isAdmin || isEmployee;

  String _baseUrl = "";

  AuthProvider() {
    _baseUrl = resolveAccessBaseUrl();
    _instance = this;
  }

  bool get isAuthenticated => _isAuthenticated;

  Future login(String username, String password) async {
    var url = "$_baseUrl/Login";
    var uri = Uri.parse(url);
    var headers = createHeaders();
    var body = jsonEncode({
      "username": username,
      "password": password
    });

    http.Response response = await http.post(uri, headers: headers, body: body);
    _validateLoginResponse(response);

    var data = jsonDecode(response.body);
    _accesstoken = data['accesstoken'];
    _refreshtoken = data['refreshtoken'];
    _role = data['role'] as String?;
    _isAuthenticated = true;
    notifyListeners();
  }

  void _validateLoginResponse(http.Response response) {
    if (response.statusCode < 299) {
      return;
    }
    if (response.statusCode == 401) {
      throw ApiClientException('Pogrešno korisničko ime ili lozinka.');
    }
    final parsed = ApiErrorParser.messageFromBody(response.body);
    throw ApiClientException(
      parsed ?? 'Prijava nije uspjela. Pokušajte ponovo.',
    );
  }

  static Future<bool> tryRefreshAccessToken() async {
    final instance = _instance;
    final refresh = _refreshtoken;
    if (instance == null || refresh == null || refresh.isEmpty) {
      return false;
    }

    try {
      final response = await http.post(
        Uri.parse("${instance._baseUrl}/LoginWithRefreshToken"),
        headers: {"Content-Type": "application/json"},
        body: jsonEncode({"refreshToken": refresh}),
      );
      if (response.statusCode >= 299) {
        instance._clearSessionLocal();
        return false;
      }
      final data = jsonDecode(response.body);
      final token = data['accesstoken'] as String?;
      if (token == null || token.isEmpty) {
        instance._clearSessionLocal();
        return false;
      }
      _accesstoken = token;
      _refreshtoken = data['refreshtoken'] as String? ?? refresh;
      instance._role = data['role'] as String? ?? instance._role;
      instance._isAuthenticated = true;
      instance.notifyListeners();
      return true;
    } catch (_) {
      instance._clearSessionLocal();
      return false;
    }
  }

  void _clearSessionLocal() {
    _isAuthenticated = false;
    _accesstoken = null;
    _refreshtoken = null;
    _role = null;
    notifyListeners();
  }

  Future<void> logout() async {
    final token = _accesstoken;
    final refresh = _refreshtoken;
    try {
      if (token != null && token.isNotEmpty) {
        await http.post(
          Uri.parse("$_baseUrl/Logout"),
          headers: {
            "Content-Type": "application/json",
            "Authorization": "Bearer $token",
          },
          body: jsonEncode({"refreshToken": refresh}),
        );
      }
    } catch (_) {
      // Lokalni logout i ako server nije dostupan.
    }

    _clearSessionLocal();
  }

  Map<String, String> createHeaders() {
    return {
      "Content-Type": "application/json",
    };
  }
}
