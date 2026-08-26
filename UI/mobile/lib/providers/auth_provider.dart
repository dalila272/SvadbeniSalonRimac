import 'dart:async';
import 'dart:convert';
import 'dart:io';

import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:jwt_decoder/jwt_decoder.dart';
import 'package:svadbeni_salon_rimac/constants/role_names.dart';
import 'package:svadbeni_salon_rimac/models/user_profile.dart';
import 'package:svadbeni_salon_rimac/utils/api_config.dart';
import 'package:svadbeni_salon_rimac/utils/util.dart';

class AuthProvider extends ChangeNotifier {
  bool _isAuthenticated = false;
  static String? _accessToken;
  static String? _refreshToken;
  static AuthProvider? _instance;
  String? _role;
  static Map<String, dynamic>? _accessTokenDecoded;
  String? _profileImageBase64;
  String? _cachedFirstName;
  String? _cachedLastName;
  MemoryImage? _profileImageProvider;

  static String? get accessToken => _accessToken;
  String? get refreshToken => _refreshToken;
  String? get role => _role;
  static Map<String, dynamic>? get accessTokenDecoded => _accessTokenDecoded;
  String? get profileImageBase64 => _profileImageBase64;
  MemoryImage? get profileImageProvider => _profileImageProvider;

  bool get isClient => _role == RoleNames.customer;
  bool get canAccessMobile => isClient;

  late final String _baseUrl;

  AuthProvider() {
    _baseUrl = resolveAccessBaseUrl();
    _instance = this;
  }

  bool get isAuthenticated => _isAuthenticated;

  String get displayName {
    final cached = '${_cachedFirstName ?? ''} ${_cachedLastName ?? ''}'.trim();
    if (cached.isNotEmpty) return cached;
    final first = _readClaim('FirstName') ?? '';
    final last = _readClaim('LastName') ?? '';
    final full = '$first $last'.trim();
    if (full.isNotEmpty) return full;
    return 'korisniče';
  }

  void _applyProfileCache(UserProfile profile) {
    _profileImageBase64 = profile.profileImageBase64;
    _cachedFirstName = profile.firstName;
    _cachedLastName = profile.lastName;
    _profileImageProvider = _decodeAvatar(profile.profileImageBase64);
  }

  static MemoryImage? _decodeAvatar(String? base64) {
    if (base64 == null || base64.isEmpty) return null;
    try {
      final cleaned = base64.contains(',') ? base64.split(',').last : base64;
      final bytes = dataFromBase64String(cleaned);
      return MemoryImage(bytes);
    } catch (_) {
      return null;
    }
  }

  Future<void> refreshProfileCache() async {
    if (!_isAuthenticated || _accessToken == null) return;
    try {
      final profile = await getProfile();
      _applyProfileCache(profile);
      notifyListeners();
    } catch (_) {
    }
  }

  static Future<bool> tryRefreshAccessToken() async {
    final instance = _instance;
    final refresh = _refreshToken;
    if (instance == null || refresh == null || refresh.isEmpty) {
      return false;
    }

    try {
      final uri = Uri.parse('${instance._baseUrl}/LoginWithRefreshToken');
      final response = await http
          .post(
            uri,
            headers: {'Content-Type': 'application/json'},
            body: jsonEncode({'refreshToken': refresh}),
          )
          .timeout(const Duration(seconds: 8));

      if (response.statusCode >= 299) {
        await instance._clearSessionLocal();
        return false;
      }

      final data = jsonDecode(response.body) as Map<String, dynamic>;
      final token = data['accesstoken'] as String?;
      if (token == null || token.isEmpty) {
        await instance._clearSessionLocal();
        return false;
      }

      _accessToken = token;
      _refreshToken = data['refreshtoken'] as String? ?? refresh;
      _accessTokenDecoded = JwtDecoder.decode(token);
      instance._role = data['role'] as String? ?? instance._role;
      instance._isAuthenticated = true;
      instance.notifyListeners();
      return true;
    } catch (_) {
      await instance._clearSessionLocal();
      return false;
    }
  }

  Future<void> _clearSessionLocal() async {
    _isAuthenticated = false;
    _accessToken = null;
    _refreshToken = null;
    _role = null;
    _accessTokenDecoded = null;
    _profileImageBase64 = null;
    _profileImageProvider = null;
    _cachedFirstName = null;
    _cachedLastName = null;
    notifyListeners();
  }

  String? _readClaim(String name) {
    final decoded = _accessTokenDecoded;
    if (decoded == null) return null;

    for (final entry in decoded.entries) {
      final key = entry.key.toLowerCase();
      if (key == name.toLowerCase() || key.endsWith('/${name.toLowerCase()}')) {
        final value = entry.value?.toString().trim();
        if (value != null && value.isNotEmpty) return value;
      }
    }
    return null;
  }

  Future<void> register({
    required String firstName,
    required String lastName,
    required String email,
    required String username,
    required String password,
  }) async {
    final uri = Uri.parse('$_baseUrl/Register');

    try {
      final response = await http
          .post(
            uri,
            headers: {'Content-Type': 'application/json'},
            body: jsonEncode({
              'firstName': firstName,
              'lastName': lastName,
              'email': email,
              'username': username,
              'password': password,
            }),
          )
          .timeout(const Duration(seconds: 15));

      if (response.statusCode >= 299) {
        final message = _tryParseError(response.body);
        throw Exception(message ?? 'Registracija nije uspjela (${response.statusCode}).');
      }
    } on TimeoutException {
      throw Exception('API nije dostupan (timeout). Provjeri da li API radi.');
    } on SocketException {
      throw Exception('Nema konekcije s API-jem ($_baseUrl).');
    } on FormatException {
      throw Exception('Neočekivan odgovor sa servera.');
    }
  }

  Future<String> forgotPassword(String emailOrUsername) async {
    final uri = Uri.parse('$_baseUrl/ForgotPassword');
    try {
      final response = await http
          .post(
            uri,
            headers: {'Content-Type': 'application/json'},
            body: jsonEncode({'emailOrUsername': emailOrUsername}),
          )
          .timeout(const Duration(seconds: 15));

      if (response.statusCode >= 299) {
        final message = _tryParseError(response.body);
        throw Exception(message ?? 'Zahtjev nije uspio (${response.statusCode}).');
      }

      try {
        final data = jsonDecode(response.body);
        if (data is Map && data['message'] != null) {
          return data['message'].toString();
        }
      } catch (_) {}
      return 'Ako nalog postoji, poslali smo kod za reset lozinke na email.';
    } on TimeoutException {
      throw Exception('API nije dostupan (timeout).');
    } on SocketException {
      throw Exception('Nema konekcije s API-jem ($_baseUrl).');
    }
  }

  Future<String> resetPassword({
    required String emailOrUsername,
    required String code,
    required String newPassword,
  }) async {
    final uri = Uri.parse('$_baseUrl/ResetPassword');
    try {
      final response = await http
          .post(
            uri,
            headers: {'Content-Type': 'application/json'},
            body: jsonEncode({
              'emailOrUsername': emailOrUsername,
              'code': code,
              'newPassword': newPassword,
            }),
          )
          .timeout(const Duration(seconds: 15));

      if (response.statusCode >= 299) {
        final message = _tryParseError(response.body);
        throw Exception(message ?? 'Reset nije uspio (${response.statusCode}).');
      }

      try {
        final data = jsonDecode(response.body);
        if (data is Map && data['message'] != null) {
          return data['message'].toString();
        }
      } catch (_) {}
      return 'Lozinka je uspješno promijenjena.';
    } on TimeoutException {
      throw Exception('API nije dostupan (timeout).');
    } on SocketException {
      throw Exception('Nema konekcije s API-jem ($_baseUrl).');
    }
  }

  Future<UserProfile> getProfile() async {
    final uri = Uri.parse('$_baseUrl/Profile');
    final response = await http
        .get(uri, headers: _authHeaders())
        .timeout(const Duration(seconds: 15));

    if (response.statusCode >= 299) {
      final message = _tryParseError(response.body);
      throw Exception(message ?? 'Učitavanje profila nije uspjelo.');
    }

    final profile =
        UserProfile.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
    _applyProfileCache(profile);
    notifyListeners();
    return profile;
  }

  Future<UserProfile> updateProfile(Map<String, dynamic> body) async {
    final uri = Uri.parse('$_baseUrl/Profile');
    final response = await http
        .put(
          uri,
          headers: _authHeaders(),
          body: jsonEncode(body),
        )
        .timeout(const Duration(seconds: 30));

    if (response.statusCode >= 299) {
      final message = _tryParseError(response.body);
      throw Exception(message ?? 'Spremanje profila nije uspjelo.');
    }

    final updated =
        UserProfile.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
    _applyProfileCache(updated);
    notifyListeners();
    return updated;
  }

  Future<String> changePassword({
    required String currentPassword,
    required String newPassword,
    required String confirmNewPassword,
  }) async {
    final uri = Uri.parse('$_baseUrl/ChangePassword');
    final response = await http
        .put(
          uri,
          headers: _authHeaders(),
          body: jsonEncode({
            'password': currentPassword,
            'newPassword': newPassword,
            'confirmNewPassword': confirmNewPassword,
          }),
        )
        .timeout(const Duration(seconds: 15));

    if (response.statusCode >= 299) {
      final message = _tryParseError(response.body);
      throw Exception(message ?? 'Promjena lozinke nije uspjela.');
    }

    try {
      final data = jsonDecode(response.body);
      if (data is Map && data['message'] != null) {
        return data['message'].toString();
      }
    } catch (_) {}
    return 'Lozinka je uspješno promijenjena.';
  }

  Map<String, String> _authHeaders() {
    return {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ${_accessToken ?? ''}',
    };
  }

  Future<void> login(String username, String password) async {
    final uri = Uri.parse('$_baseUrl/Login');

    try {
      final response = await http
          .post(
            uri,
            headers: {'Content-Type': 'application/json'},
            body: jsonEncode({'username': username, 'password': password}),
          )
          .timeout(const Duration(seconds: 15));

      _validateResponse(response);
      final data = jsonDecode(response.body) as Map<String, dynamic>;

      final token = data['accesstoken'] as String?;
      if (token == null || token.isEmpty) {
        throw Exception('API nije vratio access token.');
      }

      _accessToken = token;
      _refreshToken = data['refreshtoken'] as String?;
      _accessTokenDecoded = JwtDecoder.decode(token);
      _role = data['role'] as String? ?? _readClaim('Role');
      _isAuthenticated = true;
      notifyListeners();
      unawaited(refreshProfileCache());
    } on TimeoutException {
      throw Exception(
        'API nije dostupan (timeout). Provjeri da li API radi na portu 5121 i da koristiš ispravan URL za simulator.',
      );
    } on SocketException {
      throw Exception(
        'Nema konekcije s API-jem ($_baseUrl). Provjeri: 1) API radi (dotnet run), 2) iOS → 127.0.0.1, Android emulator → 10.0.2.2, fizički telefon → IP Mac-a na WiFi.',
      );
    } on FormatException {
      throw Exception('Neočekivan odgovor sa servera.');
    }
  }

  Future<void> logout() async {
    final token = _accessToken;
    final refresh = _refreshToken;
    try {
      if (token != null && token.isNotEmpty) {
        final uri = Uri.parse('$_baseUrl/Logout');
        await http
            .post(
              uri,
              headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer $token',
              },
              body: jsonEncode({'refreshToken': refresh}),
            )
            .timeout(const Duration(seconds: 8));
      }
    } catch (_) {
      // Lokalni logout i ako server nije dostupan.
    }

    await _clearSessionLocal();
  }

  void _validateResponse(http.Response response) {
    if (response.statusCode < 299) return;
    if (response.statusCode == 401) {
      throw Exception('Neispravno korisničko ime ili lozinka.');
    }
    final message = _tryParseError(response.body);
    throw Exception(message ?? 'Prijava nije uspjela (${response.statusCode}).');
  }

  String? _tryParseError(String body) {
    try {
      final data = jsonDecode(body);
      if (data is Map) {
        if (data['message'] != null) return data['message'].toString();
        if (data['errors'] != null) {
          final errors = data['errors'] as Map<String, dynamic>;
          for (final value in errors.values) {
            if (value is List && value.isNotEmpty) return value.first.toString();
          }
        }
      }
    } catch (_) {}
    return null;
  }
}
