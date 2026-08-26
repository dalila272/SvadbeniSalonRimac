import 'package:svadbeni_salon_desktop/models/genre.dart';
import 'package:svadbeni_salon_desktop/providers/base_provider.dart';

class GenreProvider extends BaseProvider<Genre> {
  GenreProvider() : super('Zanrovi');

  @override
  Genre fromJson(data) {
    return Genre.fromJson(data as Map<String, dynamic>);
  }
}
