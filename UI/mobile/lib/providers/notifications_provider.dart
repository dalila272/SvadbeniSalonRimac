import 'dart:async';
import 'dart:convert';

import 'package:flutter/foundation.dart';
import 'package:svadbeni_salon_rimac/models/app_notification.dart';
import 'package:svadbeni_salon_rimac/providers/auth_provider.dart';
import 'package:svadbeni_salon_rimac/utils/api_config.dart';
import 'package:svadbeni_salon_rimac/utils/authenticated_http.dart';

class NotificationsProvider with ChangeNotifier {
  static const _pollInterval = Duration(seconds: 25);

  List<AppNotification> _items = [];
  int _unreadCount = 0;
  bool _loading = false;
  String? _error;
  Timer? _pollTimer;
  bool _started = false;

  List<AppNotification> get items => List.unmodifiable(_items);
  int get unreadCount => _unreadCount;
  bool get loading => _loading;
  String? get error => _error;

  void startAutoRefresh() {
    if (_started) return;
    _started = true;
    refresh();
    _pollTimer?.cancel();
    _pollTimer = Timer.periodic(_pollInterval, (_) {
      if (AuthProvider.accessToken == null || AuthProvider.accessToken!.isEmpty) {
        return;
      }
      refresh(silent: true);
    });
  }

  void stopAutoRefresh() {
    _pollTimer?.cancel();
    _pollTimer = null;
    _started = false;
  }

  Future<void> refresh({bool silent = false}) async {
    if (!silent) {
      _loading = true;
      _error = null;
      notifyListeners();
    }

    try {
      final listResponse = await AuthenticatedHttp.get(
        Uri.parse('${resolveApiBaseUrl()}Notifikacije?take=50'),
      );
      AuthenticatedHttp.validate(listResponse);

      final countResponse = await AuthenticatedHttp.get(
        Uri.parse('${resolveApiBaseUrl()}Notifikacije/unread-count'),
      );
      AuthenticatedHttp.validate(countResponse);

      final listData = jsonDecode(listResponse.body);
      final countData = jsonDecode(countResponse.body) as Map<String, dynamic>;

      _items = listData is List
          ? listData
              .map((e) => AppNotification.fromJson(e as Map<String, dynamic>))
              .toList()
          : [];
      _unreadCount = countData['count'] is int
          ? countData['count'] as int
          : int.tryParse(countData['count']?.toString() ?? '') ?? 0;
      _error = null;
    } catch (e) {
      if (!silent) {
        _error = e.toString().replaceFirst('Exception: ', '');
      }
    } finally {
      _loading = false;
      notifyListeners();
    }
  }

  Future<void> markAsRead(int id) async {
    final response = await AuthenticatedHttp.put(
      Uri.parse('${resolveApiBaseUrl()}Notifikacije/$id/read'),
    );
    AuthenticatedHttp.validate(response);
    final updated =
        AppNotification.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
    _items = _items
        .map((n) => n.id == id ? updated : n)
        .toList(growable: false);
    _unreadCount = _items.where((n) => !n.isRead).length;
    notifyListeners();
  }

  Future<void> markAllAsRead() async {
    final response = await AuthenticatedHttp.put(
      Uri.parse('${resolveApiBaseUrl()}Notifikacije/read-all'),
    );
    AuthenticatedHttp.validate(response);
    _items = _items.map((n) => n.copyWith(isRead: true)).toList(growable: false);
    _unreadCount = 0;
    notifyListeners();
  }

  @override
  void dispose() {
    stopAutoRefresh();
    super.dispose();
  }
}
