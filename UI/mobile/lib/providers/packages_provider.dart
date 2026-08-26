import 'package:svadbeni_salon_rimac/models/package.dart';
import 'package:svadbeni_salon_rimac/providers/base_provider.dart';

class PackagesProvider extends BaseProvider<Package> {
  PackagesProvider() : super('Ponude');

  @override
  Package fromJson(dynamic data) {
    return Package.fromJson(data as Map<String, dynamic>);
  }
}
