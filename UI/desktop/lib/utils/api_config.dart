String resolveApiBaseUrl() {
  const fromEnv = String.fromEnvironment('API_BASE_URL');
  var base = fromEnv.trim();
  if (base.isEmpty) {
    base = 'http://localhost:5121';
  }
  if (!base.endsWith('/')) {
    base = '$base/';
  }
  return base;
}

String resolveAccessBaseUrl() {
  final root = resolveApiBaseUrl().replaceAll(RegExp(r'/+$'), '');
  return '$root/Access';
}
