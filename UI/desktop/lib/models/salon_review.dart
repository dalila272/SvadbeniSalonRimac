class SalonReview {
  final int id;
  final int userId;
  final String clientName;
  final String offerName;
  final int? weddingId;
  final DateTime? weddingDate;
  final int rating;
  final String? comment;
  final DateTime createdAt;

  SalonReview({
    required this.id,
    required this.userId,
    this.clientName = '',
    required this.offerName,
    this.weddingId,
    this.weddingDate,
    required this.rating,
    this.comment,
    required this.createdAt,
  });

  String get displayClientName =>
      clientName.isNotEmpty ? clientName : 'Korisnik #$userId';

  factory SalonReview.fromJson(Map<String, dynamic> json) {
    return SalonReview(
      id: json['id'] is int ? json['id'] : int.parse(json['id'].toString()),
      userId: json['userId'] is int
          ? json['userId']
          : int.parse(json['userId'].toString()),
      clientName: json['korisnikIme']?.toString() ?? '',
      offerName: json['ponudaNaziv']?.toString() ?? '',
      weddingId: json['svadbaId'] == null
          ? null
          : (json['svadbaId'] is int
              ? json['svadbaId'] as int
              : int.parse(json['svadbaId'].toString())),
      weddingDate: json['svadbaDatum'] != null
          ? DateTime.parse(json['svadbaDatum'].toString())
          : null,
      rating: json['ocjena'] is int
          ? json['ocjena']
          : int.parse(json['ocjena'].toString()),
      comment: json['komentar']?.toString(),
      createdAt: DateTime.parse(json['createdAt'].toString()),
    );
  }
}
