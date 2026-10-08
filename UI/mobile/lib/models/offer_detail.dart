class OfferMenu {
  final int id;
  final String name;
  final String description;
  final String price;

  OfferMenu({
    required this.id,
    required this.name,
    required this.description,
    required this.price,
  });

  factory OfferMenu.fromJson(Map<String, dynamic> json) {
    return OfferMenu(
      id: json['id'] is int ? json['id'] : int.parse(json['id'].toString()),
      name: json['naziv'] ?? '',
      description: json['opis'] ?? '',
      price: (json['cijena'] ?? 0).toString(),
    );
  }
}

class OfferMusician {
  final int id;
  final String name;
  final String description;
  final List<int> zanrIds;

  OfferMusician({
    required this.id,
    required this.name,
    required this.description,
    this.zanrIds = const [],
  });

  factory OfferMusician.fromJson(Map<String, dynamic> json) {
    final rawZanrovi = json['zanrIds'] as List<dynamic>? ?? [];
    return OfferMusician(
      id: json['id'] is int ? json['id'] : int.parse(json['id'].toString()),
      name: json['naziv'] ?? '',
      description: json['opis'] ?? '',
      zanrIds: rawZanrovi
          .map((e) => e is int ? e : int.tryParse(e.toString()) ?? 0)
          .where((id) => id > 0)
          .toList(),
    );
  }
}

class OfferDecoration {
  final int id;
  final String name;
  final String description;
  final String price;

  OfferDecoration({
    required this.id,
    required this.name,
    required this.description,
    required this.price,
  });

  factory OfferDecoration.fromJson(Map<String, dynamic> json) {
    return OfferDecoration(
      id: json['id'] is int ? json['id'] : int.parse(json['id'].toString()),
      name: json['naziv'] ?? '',
      description: json['opis'] ?? '',
      price: (json['cijena'] ?? 0).toString(),
    );
  }
}

class OfferDetail {
  final int id;
  final String title;
  final String price;
  final String description;
  final OfferMenu? menu;
  final List<OfferMusician> musicians;
  final List<OfferDecoration> decorations;

  OfferDetail({
    required this.id,
    required this.title,
    required this.price,
    required this.description,
    this.menu,
    this.musicians = const [],
    this.decorations = const [],
  });

  factory OfferDetail.fromJson(Map<String, dynamic> json) {
    return OfferDetail(
      id: json['id'] is int ? json['id'] : int.parse(json['id'].toString()),
      title: json['naziv'] ?? '',
      price: (json['cijena'] ?? 0).toString(),
      description: json['opis'] ?? '',
      menu: json['meni'] != null
          ? OfferMenu.fromJson(json['meni'] as Map<String, dynamic>)
          : null,
      musicians: (json['muzicari'] as List<dynamic>? ?? [])
          .map((e) => OfferMusician.fromJson(e as Map<String, dynamic>))
          .toList(),
      decorations: (json['dekoracije'] as List<dynamic>? ?? [])
          .map((e) => OfferDecoration.fromJson(e as Map<String, dynamic>))
          .toList(),
    );
  }
}
