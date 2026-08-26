import 'dart:convert';

class ApiErrorParser {
  static String? messageFromBody(String body) {
    if (body.isEmpty) return null;
    try {
      final data = jsonDecode(body);
      if (data is! Map) return null;
      final map = Map<String, dynamic>.from(data);

      final root = map['message']?.toString();
      if (root != null && root.trim().isNotEmpty) {
        return root.trim();
      }

      final errors = map['errors'];
      if (errors is Map) {
        for (final preferred in ['clientError', 'notFound', 'unauthorized']) {
          final value = errors[preferred];
          if (value is List && value.isNotEmpty) {
            return value.first.toString();
          }
          if (value is String && value.trim().isNotEmpty) {
            return value.trim();
          }
        }
        for (final entry in errors.entries) {
          final value = entry.value;
          if (value is List && value.isNotEmpty) {
            return value.first.toString();
          }
          if (value is String && value.trim().isNotEmpty) {
            return value.trim();
          }
        }
      }

      final title = map['title']?.toString();
      if (title != null && title.trim().isNotEmpty) {
        return title.trim();
      }
    } catch (_) {}
    return null;
  }
}
