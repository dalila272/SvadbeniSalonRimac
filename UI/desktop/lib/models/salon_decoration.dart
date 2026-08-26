class SalonDecoration {
  final int id;
  final String name;
  final String description;
  final String price;
  final bool isActive;

  SalonDecoration({
    required this.id,
    required this.name,
    this.description = '',
    required this.price,
    required this.isActive,
  });

  factory SalonDecoration.fromJson(Map<String, dynamic> json) {
    return SalonDecoration(
      id: json['id'] is int ? json['id'] : int.parse(json['id'].toString()),
      name: json['naziv']?.toString() ?? '',
      description: json['opis']?.toString() ?? '',
      price: (json['cijena'] ?? 0).toString(),
      isActive: json['isActive'] ?? true,
    );
  }
}
