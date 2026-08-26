import 'package:svadbeni_salon_desktop/models/article_item.dart';
import 'package:svadbeni_salon_desktop/providers/base_provider.dart';

class ArticleProvider extends BaseProvider<Article> {
  ArticleProvider() : super('Artikli');

  @override
  Article fromJson(data) {
    return Article.fromJson(data as Map<String, dynamic>);
  }
}
