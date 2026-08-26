class CatalogItem {
  final int id;
  final String name;

  CatalogItem({required this.id, required this.name});

  factory CatalogItem.fromJson(Map<String, dynamic> json) {
    return CatalogItem(
      id: json['id'] is int ? json['id'] : int.parse(json['id'].toString()),
      name: json['naziv'] ?? '',
    );
  }
}
