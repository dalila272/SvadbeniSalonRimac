class OfferDetail {
  final int id;
  final String name;
  final String price;
  final String description;
  final int menuId;
  final bool isActive;
  final List<int> musicianIds;
  final List<int> decorationIds;

  OfferDetail({
    required this.id,
    required this.name,
    required this.price,
    required this.description,
    required this.menuId,
    required this.isActive,
    this.musicianIds = const [],
    this.decorationIds = const [],
  });

  factory OfferDetail.fromJson(Map<String, dynamic> json) {
    return OfferDetail(
      id: json['id'] is int ? json['id'] : int.parse(json['id'].toString()),
      name: json['naziv'] ?? '',
      price: (json['cijena'] ?? 0).toString(),
      description: json['opis'] ?? '',
      menuId: json['meniId'] is int
          ? json['meniId']
          : int.parse(json['meniId'].toString()),
      isActive: json['isActive'] ?? true,
      musicianIds: (json['muzicari'] as List<dynamic>? ?? [])
          .map<int>((e) => e['id'] is int ? e['id'] as int : int.parse(e['id'].toString()))
          .toList(),
      decorationIds: (json['dekoracije'] as List<dynamic>? ?? [])
          .map<int>((e) => e['id'] is int ? e['id'] as int : int.parse(e['id'].toString()))
          .toList(),
    );
  }

  Map<String, dynamic> toRequestJson() {
    return {
      'naziv': name,
      'opis': description,
      'cijena': double.tryParse(price) ?? 0,
      'meniId': menuId,
      'isActive': isActive,
      'muzicarIds': musicianIds,
      'dekoracijaIds': decorationIds,
    };
  }
}
