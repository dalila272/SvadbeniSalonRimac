class Article {
  final int id;
  final String name;
  final int tip;
  final String price;
  final bool isActive;

  Article({
    required this.id,
    required this.name,
    required this.tip,
    required this.price,
    required this.isActive,
  });

  String get tipLabel => tip == 1 ? 'Hrana' : 'Piće';

  factory Article.fromJson(Map<String, dynamic> json) {
    return Article(
      id: json['id'] is int ? json['id'] : int.parse(json['id'].toString()),
      name: json['naziv'] ?? '',
      tip: json['tip'] is int ? json['tip'] : int.parse(json['tip'].toString()),
      price: (json['cijena'] ?? 0).toString(),
      isActive: json['isActive'] ?? true,
    );
  }
}

class ArticleItem {
  final int id;
  final String name;
  final int tip;

  ArticleItem({
    required this.id,
    required this.name,
    required this.tip,
  });

  factory ArticleItem.fromJson(Map<String, dynamic> json) {
    return ArticleItem(
      id: json['id'] is int ? json['id'] : int.parse(json['id'].toString()),
      name: json['naziv'] ?? '',
      tip: json['tip'] is int ? json['tip'] : int.parse(json['tip'].toString()),
    );
  }

  factory ArticleItem.fromArticle(Article article) {
    return ArticleItem(id: article.id, name: article.name, tip: article.tip);
  }
}
