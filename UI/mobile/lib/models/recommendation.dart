class Recommendation {
  final int ponudaId;
  final String naziv;
  final String opis;
  final String cijena;
  final double score;
  final String razlog;
  final List<String> matchingZanrovi;

  Recommendation({
    required this.ponudaId,
    required this.naziv,
    required this.opis,
    required this.cijena,
    required this.score,
    required this.razlog,
    required this.matchingZanrovi,
  });

  factory Recommendation.fromJson(Map<String, dynamic> json) {
    final matching = json['matchingZanrovi'];
    return Recommendation(
      ponudaId: json['ponudaId'] is int
          ? json['ponudaId'] as int
          : int.parse(json['ponudaId'].toString()),
      naziv: json['naziv']?.toString() ?? '',
      opis: json['opis']?.toString() ?? '',
      cijena: (json['cijena'] ?? 0).toString(),
      score: (json['score'] is num)
          ? (json['score'] as num).toDouble()
          : double.tryParse(json['score']?.toString() ?? '') ?? 0,
      razlog: json['razlog']?.toString() ?? '',
      matchingZanrovi: matching is List
          ? matching.map((e) => e.toString()).toList()
          : const [],
    );
  }
}
