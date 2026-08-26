class DailyMeeting {
  final int id;
  final DateTime meetingDate;
  final int status;
  final String? note;

  DailyMeeting({
    required this.id,
    required this.meetingDate,
    required this.status,
    this.note,
  });

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
      meetingDate: DateTime.parse(json['datumSastanka'].toString()),
      status: json['status'] is int
          ? json['status']
          : int.parse(json['status'].toString()),
      note: json['napomena']?.toString(),
    );
  }
}
