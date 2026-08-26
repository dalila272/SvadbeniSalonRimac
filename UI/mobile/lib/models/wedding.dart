class Wedding {
  final int id;
  final int offerId;
  final String offerName;
  final DateTime weddingDate;
  final String time;
  final int guestCount;
  final int installmentCount;
  final int status;
  final String? note;
  final double offerPrice;
  final double paidAmount;
  final double remainingAmount;
  final bool isFullyPaid;

  Wedding({
    required this.id,
    required this.offerId,
    required this.offerName,
    required this.weddingDate,
    required this.time,
    required this.guestCount,
    required this.installmentCount,
    required this.status,
    this.note,
    this.offerPrice = 0,
    this.paidAmount = 0,
    double? remainingAmount,
    bool? isFullyPaid,
  })  : remainingAmount = remainingAmount ?? (offerPrice - paidAmount),
        isFullyPaid = isFullyPaid ??
            (offerPrice > 0 && paidAmount >= offerPrice);

  bool get isActive => status == 0 || status == 1;

  String get statusLabel {
    switch (status) {
      case 0:
        return 'Na čekanju';
      case 1:
        return 'Potvrđena';
      case 2:
        return 'Otkazana';
      case 3:
        return 'Završena';
      default:
        return 'Nepoznato';
    }
  }

  String get paymentLabel {
    if (isFullyPaid) return 'Uplaćeno';
    if (paidAmount <= 0) return 'Nije uplaćeno';
    return 'Djelomično uplaćeno';
  }

  factory Wedding.fromJson(Map<String, dynamic> json) {
    return Wedding(
      id: json['id'] is int ? json['id'] : int.parse(json['id'].toString()),
      offerId: json['ponudaId'] is int
          ? json['ponudaId']
          : int.parse(json['ponudaId'].toString()),
      offerName: json['ponudaNaziv'] ?? '',
      weddingDate: DateTime.parse(json['datumSvadbe'].toString()),
      time: json['vrijeme']?.toString() ?? '00:00:00',
      guestCount: json['brojGostiju'] is int
          ? json['brojGostiju']
          : int.parse(json['brojGostiju'].toString()),
      installmentCount: json['brojRata'] is int
          ? json['brojRata']
          : int.parse(json['brojRata'].toString()),
      status: json['status'] is int
          ? json['status']
          : int.parse(json['status'].toString()),
      note: json['napomena']?.toString(),
      offerPrice: json['cijenaPonude'] is num
          ? (json['cijenaPonude'] as num).toDouble()
          : double.tryParse(json['cijenaPonude']?.toString() ?? '0') ?? 0,
      paidAmount: json['uplaceniIznos'] is num
          ? (json['uplaceniIznos'] as num).toDouble()
          : double.tryParse(json['uplaceniIznos']?.toString() ?? '0') ?? 0,
      remainingAmount: json['preostaliIznos'] is num
          ? (json['preostaliIznos'] as num).toDouble()
          : double.tryParse(json['preostaliIznos']?.toString() ?? ''),
      isFullyPaid: json['isFullyPaid'] is bool ? json['isFullyPaid'] as bool : null,
    );
  }
}
