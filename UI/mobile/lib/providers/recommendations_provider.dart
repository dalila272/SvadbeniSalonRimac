import 'dart:convert';

import 'package:svadbeni_salon_rimac/models/recommendation.dart';
import 'package:svadbeni_salon_rimac/providers/base_provider.dart';
import 'package:svadbeni_salon_rimac/utils/api_config.dart';
import 'package:svadbeni_salon_rimac/utils/authenticated_http.dart';

class GenreOption {
  final int id;
  final String name;

  GenreOption({required this.id, required this.name});

  factory GenreOption.fromJson(Map<String, dynamic> json) {
    return GenreOption(
      id: json['id'] is int ? json['id'] as int : int.parse(json['id'].toString()),
      name: json['naziv']?.toString() ?? '',
    );
  }
}

class RecommendationsProvider extends BaseProvider<Recommendation> {
  RecommendationsProvider() : super('Preporuke');

  String get _accessBase => resolveAccessBaseUrl();

  @override
  Recommendation fromJson(dynamic data) {
    return Recommendation.fromJson(data as Map<String, dynamic>);
  }

  Future<List<Recommendation>> fetchRecommendations({int limit = 5}) async {
    final url = '${resolveApiBaseUrl()}Preporuke?limit=$limit';
    final response = await AuthenticatedHttp.get(Uri.parse(url));
    AuthenticatedHttp.validate(response);
    final data = jsonDecode(response.body);
    if (data is! List) return [];
    return data
        .map((e) => Recommendation.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<List<GenreOption>> fetchGenres() async {
    final url = '${resolveApiBaseUrl()}Zanrovi?Page=1&PageSize=100';
    final response = await AuthenticatedHttp.get(Uri.parse(url));
    AuthenticatedHttp.validate(response);
    final data = jsonDecode(response.body);
    final items = data is Map ? data['items'] : data;
    if (items is! List) return [];
    return items
        .map((e) => GenreOption.fromJson(e as Map<String, dynamic>))
        .where((g) => g.name.isNotEmpty)
        .toList();
  }

  Future<List<int>> fetchMyInterestIds() async {
    final url = '$_accessBase/Interests';
    final response = await AuthenticatedHttp.get(Uri.parse(url));
    AuthenticatedHttp.validate(response);
    final data = jsonDecode(response.body) as Map<String, dynamic>;
    final ids = data['zanrIds'];
    if (ids is! List) return [];
    return ids
        .map((e) => e is int ? e : int.tryParse(e.toString()) ?? 0)
        .where((id) => id > 0)
        .toList();
  }

  Future<void> saveInterests(List<int> zanrIds) async {
    final url = '$_accessBase/Interests';
    final response = await AuthenticatedHttp.put(
      Uri.parse(url),
      body: jsonEncode({'zanrIds': zanrIds}),
    );
    AuthenticatedHttp.validate(response);
  }
}
