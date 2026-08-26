class Menu {
  final int id;
  final String name;
  final String description;
  final String price;
  final bool isActive;

  Menu({
    required this.id,
    required this.name,
    required this.description,
    required this.price,
    required this.isActive,
  });

  factory Menu.fromJson(Map<String, dynamic> json) {
    return Menu(
      id: json['id'] is int ? json['id'] : int.parse(json['id'].toString()),
      name: json['naziv'] ?? '',
      description: json['opis'] ?? '',
      price: (json['cijena'] ?? 0).toString(),
      isActive: json['isActive'] ?? true,
    );
  }
}

class MenuDetail extends Menu {
  final List<int> hranaIds;
  final List<int> piceIds;

  MenuDetail({
    required super.id,
    required super.name,
    required super.description,
    required super.price,
    required super.isActive,
    required this.hranaIds,
    required this.piceIds,
  });

  factory MenuDetail.fromJson(Map<String, dynamic> json) {
    final artikli = json['artikli'] as List<dynamic>? ?? [];
    final hranaIds = <int>[];
    final piceIds = <int>[];

    for (final item in artikli) {
      final map = item as Map<String, dynamic>;
      final id = map['id'] is int ? map['id'] : int.parse(map['id'].toString());
      final tip = map['tip'] is int ? map['tip'] : int.parse(map['tip'].toString());
      if (tip == 1) {
        hranaIds.add(id);
      } else if (tip == 2) {
        piceIds.add(id);
      }
    }

    return MenuDetail(
      id: json['id'] is int ? json['id'] : int.parse(json['id'].toString()),
      name: json['naziv'] ?? '',
      description: json['opis'] ?? '',
      price: (json['cijena'] ?? 0).toString(),
      isActive: json['isActive'] ?? true,
      hranaIds: hranaIds,
      piceIds: piceIds,
    );
  }
}
