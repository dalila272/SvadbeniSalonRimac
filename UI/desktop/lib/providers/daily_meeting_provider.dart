import 'dart:convert';

import 'package:http/http.dart' as http;
import 'package:svadbeni_salon_desktop/models/daily_meeting.dart';
import 'package:svadbeni_salon_desktop/models/search_result.dart';
import 'package:svadbeni_salon_desktop/providers/base_provider.dart';
import 'package:svadbeni_salon_desktop/utils/api_config.dart';

class DailyMeetingProvider extends BaseProvider<DailyMeeting> {
  DailyMeetingProvider() : super('DnevniSastanci');

  @override
  DailyMeeting fromJson(data) {
    return DailyMeeting.fromJson(data as Map<String, dynamic>);
  }

  Future<SearchResult<DailyMeeting>> getForMonth(DateTime month) {
    final firstDay = DateTime(month.year, month.month, 1);
    final lastDay = DateTime(month.year, month.month + 1, 0, 23, 59, 59);
    return get(filter: {
      'datumOd': firstDay.toIso8601String(),
      'datumDo': lastDay.toIso8601String(),
      'pageSize': 100,
    });
  }

  Future<List<DateTime>> getBusySlots() async {
    final url = '${_base()}DnevniSastanci/zauzeti';
    final response = await http.get(Uri.parse(url), headers: createHeaders());
    validateResponse(response);
    final data = jsonDecode(response.body) as List<dynamic>;
    return data.map((e) => DateTime.parse(e.toString())).toList();
  }

  Future<void> changeStatus(int id, int status, {String? razlog}) async {
    final url = '${_base()}DnevniSastanci/$id/status';
    final body = <String, dynamic>{'status': status};
    if (razlog != null && razlog.trim().isNotEmpty) {
      body['razlog'] = razlog.trim();
    }
    final response = await http.put(
      Uri.parse(url),
      headers: createHeaders(),
      body: jsonEncode(body),
    );
    validateResponse(response);
  }

  String _base() {
    return resolveApiBaseUrl();
  }
}
