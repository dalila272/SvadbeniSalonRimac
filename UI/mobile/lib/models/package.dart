class Package {
  final int id;
  final String title;
  final String price;
  final String description;

  Package({
    required this.id,
    required this.title,
    required this.price,
    required this.description,
  });

  factory Package.fromJson(Map<String, dynamic> json) {
    return Package(
      id: json['id'] is int ? json['id'] : int.parse(json['id'].toString()),
      title: json['naziv'] ?? json['title'] ?? '',
      price: (json['cijena'] ?? json['price']).toString(),
      description: json['opis'] ?? json['description'] ?? '',
    );
  }
}
