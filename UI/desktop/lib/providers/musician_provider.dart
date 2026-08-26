import 'package:svadbeni_salon_desktop/models/musician.dart';
import 'package:svadbeni_salon_desktop/providers/base_provider.dart';

class MusicianProvider extends BaseProvider<Musician> {
  MusicianProvider() : super('Muzicari');

  @override
  Musician fromJson(data) {
    return Musician.fromJson(data as Map<String, dynamic>);
  }
}
