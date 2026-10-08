import 'package:http/http.dart' as http;
import 'package:svadbeni_salon_desktop/providers/auth_provider.dart';
import 'package:svadbeni_salon_desktop/utils/api_client_exception.dart';

/// Shared authenticated HTTP helper: Bearer headers + refresh-on-401 retry.
class AuthenticatedHttp {
  AuthenticatedHttp._();

  static Map<String, String> headers({bool json = true}) {
    final token = AuthProvider.accesstoken ?? '';
    return {
      if (json) 'Content-Type': 'application/json',
      'Authorization': 'Bearer $token',
    };
  }

  static Future<http.Response> send(
    Future<http.Response> Function() request,
  ) async {
    var response = await request();
    if (response.statusCode != 401) return response;

    final refreshed = await AuthProvider.tryRefreshAccessToken();
    if (!refreshed) return response;
    return await request();
  }

  static void validate(http.Response response, {String? fallbackMessage}) {
    if (response.statusCode < 299) return;
    if (response.statusCode == 401) {
      throw ApiClientException('Sesija je istekla. Prijavite se ponovo.');
    }

    final parsed = ApiErrorParser.messageFromBody(response.body);
    if (response.statusCode >= 500) {
      throw ApiClientException(
        parsed ?? 'Serverska greška. Pokušajte kasnije.',
      );
    }

    throw ApiClientException(
      parsed ?? fallbackMessage ?? 'Zahtjev nije mogao biti izvršen. Pokušajte ponovo.',
    );
  }

  static Future<http.Response> get(Uri uri) =>
      send(() => http.get(uri, headers: headers()));

  static Future<http.Response> post(Uri uri, {Object? body}) => send(
        () => http.post(uri, headers: headers(), body: body),
      );

  static Future<http.Response> put(Uri uri, {Object? body}) => send(
        () => http.put(uri, headers: headers(), body: body),
      );

  static Future<http.Response> delete(Uri uri) =>
      send(() => http.delete(uri, headers: headers()));
}
