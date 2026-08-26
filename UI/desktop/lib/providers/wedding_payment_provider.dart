import 'dart:convert';

import 'package:http/http.dart' as http;
import 'package:svadbeni_salon_desktop/models/wedding_payment.dart';
import 'package:svadbeni_salon_desktop/providers/base_provider.dart';
import 'package:svadbeni_salon_desktop/utils/api_config.dart';

class WeddingPaymentProvider extends BaseProvider<WeddingPayment> {
  WeddingPaymentProvider() : super('Rate');

  @override
  WeddingPayment fromJson(data) {
    return WeddingPayment.fromJson(data as Map<String, dynamic>);
  }

  Future<void> addPayment(Map<String, dynamic> request) async {
    final url = '${_base()}Rate';
    final response = await http.post(
      Uri.parse(url),
      headers: createHeaders(),
      body: jsonEncode(request),
    );
    validateResponse(response);
  }

  String _base() {
    return resolveApiBaseUrl();
  }
}
