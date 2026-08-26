class WeddingPayment {
  final int id;
  final int weddingId;
  final double amount;
  final DateTime paymentDate;

  WeddingPayment({
    required this.id,
    required this.weddingId,
    required this.amount,
    required this.paymentDate,
  });

  factory WeddingPayment.fromJson(Map<String, dynamic> json) {
    return WeddingPayment(
      id: json['id'] is int ? json['id'] : int.parse(json['id'].toString()),
      weddingId: json['svadbaId'] is int
          ? json['svadbaId']
          : int.parse(json['svadbaId'].toString()),
      amount: json['iznos'] is num
          ? (json['iznos'] as num).toDouble()
          : double.parse(json['iznos'].toString()),
      paymentDate: _parseDate(json['datumUplate']),
    );
  }

  static DateTime _parseDate(dynamic value) {
    if (value == null) return DateTime.now();
    final text = value.toString();
    if (text.length >= 10) {
      return DateTime.parse(text.substring(0, 10));
    }
    return DateTime.parse(text);
  }
}
