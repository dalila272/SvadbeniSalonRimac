import 'dart:convert';

import 'package:http/http.dart' as http;
import 'package:svadbeni_salon_desktop/models/menu.dart';
import 'package:svadbeni_salon_desktop/providers/base_provider.dart';

class MenuProvider extends BaseProvider<Menu> {
  MenuProvider() : super('Meniji');

  @override
  Menu fromJson(data) {
    return Menu.fromJson(data as Map<String, dynamic>);
  }

  Future<MenuDetail> getDetail(int id) async {
    final url = '${BaseProvider.baseUrl}$endpoint/$id';
    final response = await sendAuthenticated(
      () => http.get(Uri.parse(url), headers: createHeaders()),
    );
    validateResponse(response);
    return MenuDetail.fromJson(
      jsonDecode(response.body) as Map<String, dynamic>,
    );
  }
}
