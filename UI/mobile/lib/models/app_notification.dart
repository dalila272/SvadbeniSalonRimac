class AppNotification {
  final int id;
  final String naslov;
  final String tekst;
  final String? kind;
  final bool isRead;
  final DateTime createdAt;

  AppNotification({
    required this.id,
    required this.naslov,
    required this.tekst,
    required this.kind,
    required this.isRead,
    required this.createdAt,
  });

  AppNotification copyWith({bool? isRead}) {
    return AppNotification(
      id: id,
      naslov: naslov,
      tekst: tekst,
      kind: kind,
      isRead: isRead ?? this.isRead,
      createdAt: createdAt,
    );
  }

  factory AppNotification.fromJson(Map<String, dynamic> json) {
    return AppNotification(
      id: json['id'] is int ? json['id'] as int : int.parse(json['id'].toString()),
      naslov: json['naslov']?.toString() ?? '',
      tekst: json['tekst']?.toString() ?? '',
      kind: json['kind']?.toString(),
      isRead: json['isRead'] == true,
      createdAt: DateTime.tryParse(json['createdAt']?.toString() ?? '')?.toLocal() ??
          DateTime.now(),
    );
  }
}
