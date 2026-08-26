import 'dart:io';

import 'package:flutter/foundation.dart';

String get _defaultApiHost {
  if (kIsWeb) return 'localhost';
  if (!kIsWeb && Platform.isAndroid) return '10.0.2.2';
  return '127.0.0.1';
}

String resolveApiBaseUrl() {
  const fromEnv = String.fromEnvironment('API_BASE_URL');
  var base = fromEnv.trim();
  if (base.isEmpty) {
    base = 'http://$_defaultApiHost:5121';
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
