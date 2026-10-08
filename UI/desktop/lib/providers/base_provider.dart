import 'dart:convert';

import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import 'package:svadbeni_salon_desktop/models/search_result.dart';
import 'package:svadbeni_salon_desktop/utils/api_config.dart';
import 'package:svadbeni_salon_desktop/utils/authenticated_http.dart';

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

    var response = await AuthenticatedHttp.get(Uri.parse(url));
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
    var response = await AuthenticatedHttp.get(Uri.parse(url));
    validateResponse(response);
    return fromJson(jsonDecode(response.body));
  }

  Future<T> insert(dynamic request) async {
    var url = "$_baseUrl$endpoint";
    var response = await AuthenticatedHttp.post(
      Uri.parse(url),
      body: jsonEncode(request),
    );
    validateResponse(response);
    return fromJson(jsonDecode(response.body));
  }

  Future<T> update(int id, [dynamic request]) async {
    var url = "$_baseUrl$endpoint/$id";
    var response = await AuthenticatedHttp.put(
      Uri.parse(url),
      body: jsonEncode(request),
    );
    validateResponse(response);
    return fromJson(jsonDecode(response.body));
  }

  Future remove(int id) async {
    var url = "$_baseUrl$endpoint/$id";
    var response = await AuthenticatedHttp.delete(Uri.parse(url));
    validateResponse(response);
  }

  /// Authenticated request with refresh-on-401 (for custom provider methods).
  Future<http.Response> sendAuthenticated(
    Future<http.Response> Function() request,
  ) =>
      AuthenticatedHttp.send(request);

  T fromJson(data) {
    throw UnimplementedError('Override fromJson in subclass');
  }

  void validateResponse(http.Response response) {
    AuthenticatedHttp.validate(response);
  }

  Map<String, String> createHeaders() => AuthenticatedHttp.headers();

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
