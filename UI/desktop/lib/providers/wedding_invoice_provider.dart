import 'dart:convert';
import 'dart:typed_data';

import 'package:http/http.dart' as http;
import 'package:svadbeni_salon_desktop/models/wedding_invoice.dart';
import 'package:svadbeni_salon_desktop/providers/base_provider.dart';
import 'package:svadbeni_salon_desktop/utils/api_config.dart';

class WeddingInvoiceProvider extends BaseProvider<WeddingInvoice> {
  WeddingInvoiceProvider() : super('Racuni');

  @override
  WeddingInvoice fromJson(data) {
    return WeddingInvoice.fromJson(data as Map<String, dynamic>);
  }

  Future<WeddingInvoice> createForWedding(int weddingId) async {
    final url = '${_base()}Racuni';
    final response = await sendAuthenticated(
      () => http.post(
        Uri.parse(url),
        headers: createHeaders(),
        body: jsonEncode({'svadbaId': weddingId}),
      ),
    );
    validateResponse(response);
    return fromJson(jsonDecode(response.body));
  }

  Future<Uint8List> downloadPdf(int invoiceId) async {
    final url = '${_base()}Racuni/$invoiceId/pdf';
    final response = await sendAuthenticated(
      () => http.get(Uri.parse(url), headers: createHeaders()),
    );
    validateResponse(response);
    return response.bodyBytes;
  }

  String _base() {
    return resolveApiBaseUrl();
  }
}
