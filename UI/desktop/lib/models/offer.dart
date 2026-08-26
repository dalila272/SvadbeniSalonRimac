class Offer {
  final int id;
  final String name;
  final String price;
  final String description;
  final bool isActive;

  Offer({
    required this.id,
    required this.name,
    required this.price,
    required this.description,
    required this.isActive,
  });

  factory Offer.fromJson(Map<String, dynamic> json) {
    return Offer(
      id: json['id'] is int ? json['id'] : int.parse(json['id'].toString()),
      name: json['naziv'] ?? '',
      price: (json['cijena'] ?? 0).toString(),
      description: json['opis'] ?? '',
      isActive: json['isActive'] ?? true,
    );
  }
}
