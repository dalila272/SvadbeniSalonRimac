import 'dart:typed_data';

import 'package:svadbeni_salon_desktop/utils/api_config.dart';
import 'package:svadbeni_salon_desktop/utils/authenticated_http.dart';

class ReportProvider {
  String _base() {
    return resolveApiBaseUrl();
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
    final response = await AuthenticatedHttp.get(Uri.parse(url));
    AuthenticatedHttp.validate(
      response,
      fallbackMessage: 'Izvještaj nije moguće preuzeti.',
    );
    return response.bodyBytes;
  }
}
