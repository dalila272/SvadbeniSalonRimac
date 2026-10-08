import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_rimac/constants/colors.dart';
import 'package:svadbeni_salon_rimac/models/app_notification.dart';
import 'package:svadbeni_salon_rimac/providers/notifications_provider.dart';
import 'package:svadbeni_salon_rimac/utils/master_screen.dart';

class NotificationsScreen extends StatefulWidget {
  const NotificationsScreen({super.key});

  @override
  State<NotificationsScreen> createState() => _NotificationsScreenState();
}

class _NotificationsScreenState extends State<NotificationsScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<NotificationsProvider>().refresh();
    });
  }

  @override
  Widget build(BuildContext context) {
    final provider = context.watch<NotificationsProvider>();

    return MasterScreenWidget(
      title: 'Notifikacije',
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 12, 16, 8),
            child: Row(
              children: [
                Expanded(
                  child: Text(
                    provider.unreadCount > 0
                        ? 'Nepročitano: ${provider.unreadCount}'
                        : 'Sve pročitano',
                    style: TextStyle(color: Colors.grey[700]),
                  ),
                ),
                TextButton(
                  onPressed: provider.unreadCount == 0
                      ? null
                      : () => provider.markAllAsRead(),
                  child: const Text('Pročitaj sve'),
                ),
              ],
            ),
          ),
          if (provider.error != null)
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 16),
              child: Text(
                provider.error!,
                style: const TextStyle(color: AppColors.errorColor),
              ),
            ),
          Expanded(
            child: RefreshIndicator(
              onRefresh: () => provider.refresh(),
              color: AppColors.primaryColor,
              child: provider.loading && provider.items.isEmpty
                  ? const Center(child: CircularProgressIndicator())
                  : provider.items.isEmpty
                      ? ListView(
                          physics: const AlwaysScrollableScrollPhysics(),
                          children: const [
                            SizedBox(height: 80),
                            Center(child: Text('Nemate notifikacija.')),
                          ],
                        )
                      : ListView.separated(
                          physics: const AlwaysScrollableScrollPhysics(),
                          padding: const EdgeInsets.fromLTRB(16, 0, 16, 24),
                          itemCount: provider.items.length,
                          separatorBuilder: (_, __) => const SizedBox(height: 8),
                          itemBuilder: (context, index) {
                            final item = provider.items[index];
                            return _NotificationTile(
                              item: item,
                              onTap: () async {
                                if (!item.isRead) {
                                  await provider.markAsRead(item.id);
                                }
                              },
                            );
                          },
                        ),
            ),
          ),
        ],
      ),
    );
  }
}

class _NotificationTile extends StatelessWidget {
  final AppNotification item;
  final VoidCallback onTap;

  const _NotificationTile({required this.item, required this.onTap});

  @override
  Widget build(BuildContext context) {
    final date = DateFormat('dd.MM.yyyy. HH:mm').format(item.createdAt);
    return Material(
      color: item.isRead
          ? Colors.white
          : AppColors.accentColor.withValues(alpha: 0.45),
      borderRadius: BorderRadius.circular(12),
      child: InkWell(
        borderRadius: BorderRadius.circular(12),
        onTap: onTap,
        child: Padding(
          padding: const EdgeInsets.all(14),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  if (!item.isRead)
                    const Padding(
                      padding: EdgeInsets.only(right: 8),
                      child: Icon(
                        Icons.circle,
                        size: 10,
                        color: AppColors.primaryColor,
                      ),
                    ),
                  Expanded(
                    child: Text(
                      item.naslov,
                      style: TextStyle(
                        fontWeight:
                            item.isRead ? FontWeight.w500 : FontWeight.w700,
                        fontSize: 15,
                      ),
                    ),
                  ),
                  Text(
                    date,
                    style: TextStyle(fontSize: 12, color: Colors.grey[600]),
                  ),
                ],
              ),
              const SizedBox(height: 6),
              Text(
                item.tekst,
                style: TextStyle(color: Colors.grey[800], height: 1.35),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
