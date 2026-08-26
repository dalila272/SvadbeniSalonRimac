import 'dart:typed_data';

import 'package:http/http.dart' as http;
import 'package:svadbeni_salon_desktop/providers/auth_provider.dart';
import 'package:svadbeni_salon_desktop/utils/api_client_exception.dart';
import 'package:svadbeni_salon_desktop/utils/api_config.dart';

class ReportProvider {
  String _base() {
    return resolveApiBaseUrl();
  }

  Map<String, String> _headers() {
    final token = AuthProvider.accesstoken ?? '';
    return {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer $token',
    };
  }

  Future<Uint8List> downloadWeddingsReport({
    required DateTime dateFrom,
    required DateTime dateTo,
    int? status,
  }) {
    return _downloadPdf(
      'Izvjestaji/svadbe/pdf',
      {
        'datumOd': _dateParam(dateFrom),
        'datumDo': _dateParam(dateTo),
        if (status != null) 'status': status,
      },
    );
  }

  Future<Uint8List> downloadPaymentsReport({
    required DateTime dateFrom,
    required DateTime dateTo,
  }) {
    return _downloadPdf(
      'Izvjestaji/uplate/pdf',
      {
        'datumOd': _dateParam(dateFrom),
        'datumDo': _dateParam(dateTo),
      },
    );
  }

  String _dateParam(DateTime date) =>
      '${date.year}-${date.month.toString().padLeft(2, '0')}-${date.day.toString().padLeft(2, '0')}';

  Future<Uint8List> _downloadPdf(
    String path,
    Map<String, dynamic> query,
  ) async {
    final queryString = query.entries
        .map((e) => '${e.key}=${Uri.encodeComponent(e.value.toString())}')
        .join('&');
    final url = '${_base()}$path?$queryString';
    final response = await http.get(Uri.parse(url), headers: _headers());

    if (response.statusCode < 299) {
      return response.bodyBytes;
    }
    if (response.statusCode == 401) {
      throw ApiClientException(
        'Your session has expired. Please sign in again.',
      );
    }
    final parsed = ApiErrorParser.messageFromBody(response.body);
    throw ApiClientException(
      parsed ?? 'Izvještaj nije moguće preuzeti.',
    );
  }
}
