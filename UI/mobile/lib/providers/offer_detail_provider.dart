import 'package:svadbeni_salon_rimac/models/offer_detail.dart';
import 'package:svadbeni_salon_rimac/providers/base_provider.dart';

class OfferDetailProvider extends BaseProvider<OfferDetail> {
  OfferDetailProvider() : super('Ponude');

  @override
  OfferDetail fromJson(dynamic data) {
    return OfferDetail.fromJson(data as Map<String, dynamic>);
  }
}
