class ApiClientException implements Exception {
  final String message;
  ApiClientException(this.message);

  @override
  String toString() => message;
}
