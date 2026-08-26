import 'dart:convert';

class ApiClientException implements Exception {
  ApiClientException(this.message);

  final String message;

  @override
  String toString() => message;
}

class ApiErrorParser {
  ApiErrorParser._();

  static String? messageFromBody(String body) {
    final trimmed = body.trim();
    if (trimmed.isEmpty) {
      return null;
    }
    try {
      final decoded = jsonDecode(trimmed);
      if (decoded is! Map) {
        return null;
      }
      final map = Map<String, dynamic>.from(decoded);

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
    } catch (_) {
      return null;
    }
    return null;
  }
}
