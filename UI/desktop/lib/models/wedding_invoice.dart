class WeddingInvoice {
  final int id;
  final int weddingId;
  final String invoiceNumber;
  final DateTime issueDate;
  final double totalAmount;
  final double paidAmount;
  final String clientName;
  final String offerName;
  final DateTime weddingDate;
  final bool isFullyPaid;

  WeddingInvoice({
    required this.id,
    required this.weddingId,
    required this.invoiceNumber,
    required this.issueDate,
    required this.totalAmount,
    required this.paidAmount,
    required this.clientName,
    required this.offerName,
    required this.weddingDate,
    bool? isFullyPaid,
  }) : isFullyPaid = isFullyPaid ?? (totalAmount - paidAmount <= 0);

  double get remainingAmount => totalAmount - paidAmount;

  factory WeddingInvoice.fromJson(Map<String, dynamic> json) {
    return WeddingInvoice(
      id: json['id'] is int ? json['id'] : int.parse(json['id'].toString()),
      weddingId: json['svadbaId'] is int
          ? json['svadbaId']
          : int.parse(json['svadbaId'].toString()),
      invoiceNumber: json['brojRacuna']?.toString() ?? '',
      issueDate: DateTime.parse(json['datumIzdavanja'].toString()),
      totalAmount: json['ukupanIznos'] is num
          ? (json['ukupanIznos'] as num).toDouble()
          : double.parse(json['ukupanIznos'].toString()),
      paidAmount: json['uplaceniIznos'] is num
          ? (json['uplaceniIznos'] as num).toDouble()
          : double.parse(json['uplaceniIznos'].toString()),
      clientName: json['korisnikIme']?.toString() ?? '',
      offerName: json['ponudaNaziv']?.toString() ?? '',
      weddingDate: DateTime.parse(json['datumSvadbe'].toString()),
      isFullyPaid: json['isFullyPaid'] is bool ? json['isFullyPaid'] as bool : null,
    );
  }
}
