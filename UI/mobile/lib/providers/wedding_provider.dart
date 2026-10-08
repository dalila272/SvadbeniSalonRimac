import 'dart:convert';

import 'package:http/http.dart' as http;
import 'package:svadbeni_salon_rimac/models/wedding.dart';
import 'package:svadbeni_salon_rimac/providers/base_provider.dart';
import 'package:svadbeni_salon_rimac/utils/api_config.dart';

class WeddingProvider extends BaseProvider<Wedding> {
  WeddingProvider() : super('Svadbe');

  @override
  Wedding fromJson(dynamic data) {
    return Wedding.fromJson(data as Map<String, dynamic>);
  }

  Future<List<DateTime>> getOccupiedDates() async {
    final url = '${_base()}Svadbe/zauzeti';
    final response = await sendAuthenticated(
      () => http.get(Uri.parse(url), headers: createHeaders()),
    );
    validateResponse(response);
    final data = jsonDecode(response.body) as List<dynamic>;
    return data
        .map((d) => DateTime.parse(d.toString()))
        .map((d) => DateTime(d.year, d.month, d.day))
        .toList();
  }

  Future<void> cancel(int id, {required String razlog}) async {
    final url = '${_base()}Svadbe/$id/status';
    final response = await sendAuthenticated(
      () => http.put(
        Uri.parse(url),
        headers: createHeaders(),
        body: jsonEncode({'status': 2, 'razlog': razlog}),
      ),
    );
    validateResponse(response);
  }

  String _base() {
    return resolveApiBaseUrl();
  }
}
