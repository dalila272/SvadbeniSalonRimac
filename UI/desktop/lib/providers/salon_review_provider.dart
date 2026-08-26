import 'package:svadbeni_salon_desktop/models/salon_review.dart';
import 'package:svadbeni_salon_desktop/providers/base_provider.dart';

class SalonReviewProvider extends BaseProvider<SalonReview> {
  SalonReviewProvider() : super('Recenzije');

  @override
  SalonReview fromJson(data) {
    return SalonReview.fromJson(data as Map<String, dynamic>);
  }
}
