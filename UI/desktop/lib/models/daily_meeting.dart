class DailyMeeting {
  final int id;
  final int? userId;
  final String clientName;
  final String? contactName;
  final DateTime meetingDate;
  final int status;
  final String? note;

  DailyMeeting({
    required this.id,
    this.userId,
    this.clientName = '',
    this.contactName,
    required this.meetingDate,
    required this.status,
    this.note,
  });

  String get displayName {
    if (clientName.isNotEmpty) return clientName;
    if (contactName != null && contactName!.isNotEmpty) return contactName!;
    if (userId != null) return 'Korisnik #$userId';
    return 'Gost';
  }

  bool get isActive => status == 0 || status == 1;
  bool get canEdit => status == 0;

  String get statusLabel {
    switch (status) {
      case 0:
        return 'Na čekanju';
      case 1:
        return 'Potvrđen';
      case 2:
        return 'Otkazan';
      case 3:
        return 'Završen';
      default:
        return 'Nepoznato';
    }
  }

  factory DailyMeeting.fromJson(Map<String, dynamic> json) {
    return DailyMeeting(
      id: json['id'] is int ? json['id'] : int.parse(json['id'].toString()),
      userId: json['userId'] == null
          ? null
          : json['userId'] is int
              ? json['userId']
              : int.parse(json['userId'].toString()),
      clientName: json['korisnikIme']?.toString() ?? '',
      contactName: json['kontaktIme']?.toString(),
      meetingDate: DateTime.parse(json['datumSastanka'].toString()),
      status: json['status'] is int
          ? json['status']
          : int.parse(json['status'].toString()),
      note: json['napomena']?.toString(),
    );
  }
}
