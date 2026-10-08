import 'dart:convert';

import 'package:http/http.dart' as http;
import 'package:svadbeni_salon_rimac/models/daily_meeting.dart';
import 'package:svadbeni_salon_rimac/providers/base_provider.dart';
import 'package:svadbeni_salon_rimac/utils/api_config.dart';

class DailyMeetingProvider extends BaseProvider<DailyMeeting> {
  DailyMeetingProvider() : super('DnevniSastanci');

  @override
  DailyMeeting fromJson(dynamic data) {
    return DailyMeeting.fromJson(data as Map<String, dynamic>);
  }

  Future<List<DateTime>> getBusySlots() async {
    final url = '${_baseUrl()}DnevniSastanci/zauzeti';
    final response = await sendAuthenticated(
      () => http.get(Uri.parse(url), headers: createHeaders()),
    );
    validateResponse(response);

    final data = jsonDecode(response.body) as List<dynamic>;
    return data.map((e) => DateTime.parse(e.toString())).toList();
  }

  Future<void> cancel(int id, {required String razlog}) async {
    final url = '${_baseUrl()}DnevniSastanci/$id/status';
    final response = await sendAuthenticated(
      () => http.put(
        Uri.parse(url),
        headers: createHeaders(),
        body: jsonEncode({'status': 2, 'razlog': razlog}),
      ),
    );
    validateResponse(response);
  }

  String _baseUrl() {
    return resolveApiBaseUrl();
  }
}
