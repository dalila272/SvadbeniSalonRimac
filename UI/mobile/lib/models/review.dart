class Review {
  final int id;
  final int offerId;
  final String offerName;
  final int weddingId;
  final int rating;
  final String? comment;
  final DateTime createdAt;

  Review({
    required this.id,
    required this.offerId,
    required this.offerName,
    required this.weddingId,
    required this.rating,
    this.comment,
    required this.createdAt,
  });

  factory Review.fromJson(Map<String, dynamic> json) {
    return Review(
      id: json['id'] is int ? json['id'] : int.parse(json['id'].toString()),
      offerId: json['ponudaId'] is int
          ? json['ponudaId']
          : int.parse(json['ponudaId'].toString()),
      offerName: json['ponudaNaziv'] ?? '',
      weddingId: json['svadbaId'] is int
          ? json['svadbaId']
          : int.parse(json['svadbaId'].toString()),
      rating: json['ocjena'] is int
          ? json['ocjena']
          : int.parse(json['ocjena'].toString()),
      comment: json['komentar']?.toString(),
      createdAt: DateTime.parse(json['createdAt'].toString()),
    );
  }
}
