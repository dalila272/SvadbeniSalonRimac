import 'dart:convert';

import 'package:http/http.dart' as http;
import 'package:svadbeni_salon_desktop/models/search_result.dart';
import 'package:svadbeni_salon_desktop/models/wedding.dart';
import 'package:svadbeni_salon_desktop/providers/base_provider.dart';
import 'package:svadbeni_salon_desktop/utils/api_config.dart';

class WeddingProvider extends BaseProvider<Wedding> {
  WeddingProvider() : super('Svadbe');

  @override
  Wedding fromJson(data) {
    return Wedding.fromJson(data as Map<String, dynamic>);
  }

  Future<SearchResult<Wedding>> getForMonth(DateTime month) {
    final firstDay = DateTime(month.year, month.month, 1);
    final lastDay = DateTime(month.year, month.month + 1, 0);
    return get(filter: {
      'datumOd': firstDay.toIso8601String(),
      'datumDo': lastDay.toIso8601String(),
      'pageSize': 100,
      'includeTotalCount': true,
    });
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

  Future<void> changeStatus(int id, int status, {String? razlog}) async {
    final url = '${_base()}Svadbe/$id/status';
    final body = <String, dynamic>{'status': status};
    if (razlog != null && razlog.trim().isNotEmpty) {
      body['razlog'] = razlog.trim();
    }
    final response = await sendAuthenticated(
      () => http.put(
        Uri.parse(url),
        headers: createHeaders(),
        body: jsonEncode(body),
      ),
    );
    validateResponse(response);
  }

  String _base() {
    return resolveApiBaseUrl();
  }
}
