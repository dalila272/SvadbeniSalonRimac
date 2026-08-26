class Musician {
  final int id;
  final String name;
  final String description;
  final bool isActive;
  final List<int> genreIds;
  final List<String> genreNames;

  Musician({
    required this.id,
    required this.name,
    this.description = '',
    required this.isActive,
    this.genreIds = const [],
    this.genreNames = const [],
  });

  String get genresLabel =>
      genreNames.isEmpty ? '—' : genreNames.join(', ');

  factory Musician.fromJson(Map<String, dynamic> json) {
    final genres = json['zanrovi'] as List<dynamic>? ?? [];
    return Musician(
      id: json['id'] is int ? json['id'] : int.parse(json['id'].toString()),
      name: json['naziv']?.toString() ?? '',
      description: json['opis']?.toString() ?? '',
      isActive: json['isActive'] ?? true,
      genreIds: (json['zanrIds'] as List<dynamic>?)
              ?.map<int>((e) => e is int ? e : int.parse(e.toString()))
              .toList() ??
          genres
              .map<int>((g) => g['id'] is int
                  ? g['id'] as int
                  : int.parse(g['id'].toString()))
              .toList(),
      genreNames: genres
          .map((g) => g['naziv']?.toString() ?? '')
          .where((n) => n.isNotEmpty)
          .toList(),
    );
  }
}
