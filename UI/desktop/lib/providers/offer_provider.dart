import 'dart:convert';

import 'package:http/http.dart' as http;
import 'package:svadbeni_salon_desktop/models/offer.dart';
import 'package:svadbeni_salon_desktop/models/offer_detail.dart';
import 'package:svadbeni_salon_desktop/providers/base_provider.dart';

class OfferProvider extends BaseProvider<Offer> {
  OfferProvider() : super('Ponude');

  @override
  Offer fromJson(data) {
    return Offer.fromJson(data as Map<String, dynamic>);
  }

  Future<OfferDetail> getDetail(int id) async {
    final url = '${BaseProvider.baseUrl}$endpoint/$id';
    final response = await http.get(
      Uri.parse(url),
      headers: createHeaders(),
    );
    validateResponse(response);
    return OfferDetail.fromJson(
      jsonDecode(response.body) as Map<String, dynamic>,
    );
  }
}
