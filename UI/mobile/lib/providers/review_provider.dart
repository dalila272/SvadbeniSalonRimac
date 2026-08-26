import 'package:svadbeni_salon_rimac/models/review.dart';
import 'package:svadbeni_salon_rimac/providers/base_provider.dart';

class ReviewProvider extends BaseProvider<Review> {
  ReviewProvider() : super('Recenzije');

  @override
  Review fromJson(dynamic data) {
    return Review.fromJson(data as Map<String, dynamic>);
  }
}
