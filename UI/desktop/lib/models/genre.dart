class Genre {
  final int id;
  final String name;
  final bool isActive;

  Genre({
    required this.id,
    required this.name,
    required this.isActive,
  });

  factory Genre.fromJson(Map<String, dynamic> json) {
    return Genre(
      id: json['id'] is int ? json['id'] : int.parse(json['id'].toString()),
      name: json['naziv']?.toString() ?? '',
      isActive: json['isActive'] ?? true,
    );
  }
}
