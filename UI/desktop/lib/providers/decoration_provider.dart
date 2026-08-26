import 'package:svadbeni_salon_desktop/models/salon_decoration.dart';
import 'package:svadbeni_salon_desktop/providers/base_provider.dart';

class DecorationProvider extends BaseProvider<SalonDecoration> {
  DecorationProvider() : super('Dekoracije');

  @override
  SalonDecoration fromJson(data) {
    return SalonDecoration.fromJson(data as Map<String, dynamic>);
  }
}
