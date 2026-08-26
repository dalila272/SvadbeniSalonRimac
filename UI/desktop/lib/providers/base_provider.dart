import 'dart:convert';

import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import 'package:svadbeni_salon_desktop/models/search_result.dart';
import 'package:svadbeni_salon_desktop/providers/auth_provider.dart';
import 'package:svadbeni_salon_desktop/utils/api_client_exception.dart';
import 'package:svadbeni_salon_desktop/utils/api_config.dart';

abstract class BaseProvider<T> with ChangeNotifier {
  static String? _baseUrl;

  static String? get baseUrl => _baseUrl;

  final String endpoint;

  BaseProvider(this.endpoint) {
    _baseUrl = resolveApiBaseUrl();
  }

  Future<SearchResult<T>> get({dynamic filter}) async {
    var url = "$_baseUrl$endpoint";
    if (filter != null) {
      var queryString = getQueryString(filter);
      url = "$url?$queryString";
    }

    var response = await _send(() => http.get(Uri.parse(url), headers: createHeaders()));
    validateResponse(response);

    var data = jsonDecode(response.body);
    var result = SearchResult<T>();
    result.totalCount = data['totalCount'];
    final items = data['items'];
    result.items = items == null
        ? []
        : List<T>.from(items.map((e) => fromJson(e)));
    return result;
  }

  Future<T> getById(int id) async {
    var url = "$_baseUrl$endpoint/$id";
    var response = await _send(() => http.get(Uri.parse(url), headers: createHeaders()));
    validateResponse(response);
    return fromJson(jsonDecode(response.body));
  }

  Future<T> insert(dynamic request) async {
    var url = "$_baseUrl$endpoint";
    var response = await _send(
      () => http.post(
        Uri.parse(url),
        headers: createHeaders(),
        body: jsonEncode(request),
      ),
    );
    validateResponse(response);
    return fromJson(jsonDecode(response.body));
  }

  Future<T> update(int id, [dynamic request]) async {
    var url = "$_baseUrl$endpoint/$id";
    var response = await _send(
      () => http.put(
        Uri.parse(url),
        headers: createHeaders(),
        body: jsonEncode(request),
      ),
    );
    validateResponse(response);
    return fromJson(jsonDecode(response.body));
  }

  Future remove(int id) async {
    var url = "$_baseUrl$endpoint/$id";
    var response = await _send(() => http.delete(Uri.parse(url), headers: createHeaders()));
    validateResponse(response);
  }

  Future<http.Response> _send(Future<http.Response> Function() request) async {
    var response = await request();
    if (response.statusCode != 401) return response;

    final refreshed = await AuthProvider.tryRefreshAccessToken();
    if (!refreshed) return response;
    return await request();
  }

  T fromJson(data) {
    throw UnimplementedError('Override fromJson in subclass');
  }

  void validateResponse(http.Response response) {
    if (response.statusCode < 299) {
      return;
    }
    if (response.statusCode == 401) {
      throw ApiClientException(
        'Sesija je istekla. Prijavite se ponovo.',
      );
    }

    final parsed = ApiErrorParser.messageFromBody(response.body);
    if (response.statusCode >= 500) {
      throw ApiClientException(
        parsed ?? 'Serverska greška. Pokušajte kasnije.',
      );
    }

    throw ApiClientException(
      parsed ?? 'Zahtjev nije mogao biti izvršen. Pokušajte ponovo.',
    );
  }

  Map<String, String> createHeaders() {
    final token = AuthProvider.accesstoken ?? '';
    return {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer $token',
    };
  }

  String getQueryString(
    Map params, {
    String prefix = '&',
    bool inRecursion = false,
  }) {
    String query = '';
    params.forEach((key, value) {
      if (inRecursion) {
        if (key is int) {
          key = '[$key]';
        } else if (value is List || value is Map) {
          key = '.$key';
        } else {
          key = '.$key';
        }
      }
      if (value is String || value is int || value is double || value is bool) {
        var encoded = value;
        if (value is String) {
          encoded = Uri.encodeComponent(value);
        }
        query += '$prefix$key=$encoded';
      } else if (value is DateTime) {
        query += '$prefix$key=${value.toIso8601String()}';
      } else if (value is List || value is Map) {
        if (value is List) value = value.asMap();
        value.forEach((k, v) {
          query += getQueryString(
            {k: v},
            prefix: '$prefix$key',
            inRecursion: true,
          );
        });
      }
    });
    return query;
  }
}
