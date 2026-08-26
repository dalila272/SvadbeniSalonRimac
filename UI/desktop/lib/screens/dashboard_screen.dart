import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_desktop/layouts/master_screen.dart';
import 'package:svadbeni_salon_desktop/providers/auth_provider.dart';
import 'package:svadbeni_salon_desktop/providers/daily_meeting_provider.dart';
import 'package:svadbeni_salon_desktop/providers/offer_provider.dart';
import 'package:svadbeni_salon_desktop/providers/salon_review_provider.dart';
import 'package:svadbeni_salon_desktop/providers/wedding_provider.dart';
import 'package:svadbeni_salon_desktop/screens/daily_meeting_list_screen.dart';
import 'package:svadbeni_salon_desktop/screens/offer_list_screen.dart';
import 'package:svadbeni_salon_desktop/screens/reports_screen.dart';
import 'package:svadbeni_salon_desktop/screens/salon_review_list_screen.dart';
import 'package:svadbeni_salon_desktop/screens/wedding_list_screen.dart';
import 'package:svadbeni_salon_desktop/utils/utils_widgets.dart';
import 'package:svadbeni_salon_desktop/constants/role_names.dart';

class DashboardScreen extends StatefulWidget {
  const DashboardScreen({super.key});

  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> {
  static const _brandColor = Color(0xFF875252);
  static final _monthFormat = DateFormat('MMMM yyyy.');

  bool _loading = true;
  int _weddingsThisMonth = 0;
  int _pendingMeetings = 0;
  int _activeOffers = 0;
  double? _averageRating;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final now = DateTime.now();
      final auth = context.read<AuthProvider>();
      final weddingProvider = context.read<WeddingProvider>();
      final meetingProvider = context.read<DailyMeetingProvider>();
      final offerProvider = auth.isAdmin ? context.read<OfferProvider>() : null;
      final reviewProvider = auth.isAdmin ? context.read<SalonReviewProvider>() : null;

      final todayStart = DateTime(now.year, now.month, now.day);
      final weddingFuture = weddingProvider.getForMonth(now);
      final pendingFuture = meetingProvider.get(filter: {
        'status': 0,
        'datumOd': todayStart.toIso8601String(),
        'pageSize': 1,
        'includeTotalCount': true,
      });
      final offersFuture = (auth.isAdmin && offerProvider != null)
          ? offerProvider.get(filter: {
              'isActive': true,
              'pageSize': 1,
              'includeTotalCount': true,
            })
          : null;
      final reviewsFuture = (auth.isAdmin && reviewProvider != null)
          ? reviewProvider.get(filter: {
              'pageSize': 100,
              'includeTotalCount': true,
            })
          : null;

      final weddingResult = await weddingFuture;
      final pendingResult = await pendingFuture;

      var activeOffers = 0;
      double? avgRating;

      if (offersFuture != null && reviewsFuture != null) {
        final offersResult = await offersFuture;
        final reviewsResult = await reviewsFuture;
        activeOffers = offersResult.totalCount ??
            offersResult.items?.length ??
            0;

        final reviews = reviewsResult.items ?? [];
        if (reviews.isNotEmpty) {
          final sum = reviews.map((r) => r.rating).reduce((a, b) => a + b);
          avgRating = sum / reviews.length;
        }
      }

      if (!mounted) return;
      setState(() {
        _weddingsThisMonth = weddingResult.totalCount ??
            weddingResult.items?.length ??
            0;
        _pendingMeetings = pendingResult.totalCount ?? 0;
        _activeOffers = activeOffers;
        _averageRating = avgRating;
        _loading = false;
      });
    } on Exception catch (e) {
      if (mounted) {
        setState(() => _loading = false);
        alertBox(context, 'Greška', e.toString());
      }
    }
  }

  void _goTo(Widget screen) {
    Navigator.pushReplacement(
      context,
      MaterialPageRoute(builder: (_) => screen),
    );
  }

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthProvider>();
    final monthLabel = _monthFormat.format(DateTime.now());

    return MasterScreen(
      title: 'Početna',
      child: RefreshIndicator(
        onRefresh: _load,
        color: _brandColor,
        child: _loading
            ? const Center(child: CircularProgressIndicator())
            : SingleChildScrollView(
                physics: const AlwaysScrollableScrollPhysics(),
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    _buildWelcome(auth),
                    const SizedBox(height: 20),
                    Text(
                      monthLabel[0].toUpperCase() + monthLabel.substring(1),
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(
                            color: Colors.grey.shade700,
                            fontWeight: FontWeight.w600,
                          ),
                    ),
                    const SizedBox(height: 12),
                    _buildStatsGrid(auth),
                    const SizedBox(height: 28),
                    Text(
                      'Brzi pristup',
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(
                            fontWeight: FontWeight.w600,
                          ),
                    ),
                    const SizedBox(height: 12),
                    _buildQuickLinks(auth),
                  ],
                ),
              ),
      ),
    );
  }

  Widget _buildWelcome(AuthProvider auth) {
    final roleLabel = auth.isAdmin ? RoleNames.admin : RoleNames.zaposlenik;
    return Card(
      elevation: 2,
      clipBehavior: Clip.antiAlias,
      child: Container(
        decoration: BoxDecoration(
          gradient: LinearGradient(
            colors: [
              _brandColor.withValues(alpha: 0.12),
              _brandColor.withValues(alpha: 0.04),
            ],
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
          ),
        ),
        padding: const EdgeInsets.all(20),
        child: Row(
          children: [
            Image.asset(
              'assets/wedding.png',
              width: 64,
              height: 64,
              fit: BoxFit.contain,
            ),
            const SizedBox(width: 16),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Dobrodošli',
                    style: TextStyle(
                      color: Colors.grey.shade600,
                      fontSize: 14,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    roleLabel,
                    style: const TextStyle(
                      fontSize: 22,
                      fontWeight: FontWeight.bold,
                      color: _brandColor,
                    ),
                  ),
                  Text(
                    'Svadbeni Salon Rimac · $roleLabel',
                    style: TextStyle(color: Colors.grey.shade600),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildStatsGrid(AuthProvider auth) {
    final cards = <Widget>[
      _StatCard(
        icon: Icons.favorite,
        label: 'Svadbe ovaj mjesec',
        value: '$_weddingsThisMonth',
        color: _brandColor,
        onTap: () => _goTo(const WeddingListScreen()),
      ),
      _StatCard(
        icon: Icons.event,
        label: 'Sastanci na čekanju',
        value: '$_pendingMeetings',
        color: Colors.blue.shade700,
        onTap: () => _goTo(const DailyMeetingListScreen()),
      ),
    ];

    if (auth.isAdmin) {
      cards.addAll([
        _StatCard(
          icon: Icons.card_giftcard,
          label: 'Aktivne ponude',
          value: '$_activeOffers',
          color: Colors.deepPurple.shade400,
          onTap: () => _goTo(const OfferListScreen()),
        ),
        _StatCard(
          icon: Icons.star_rate,
          label: 'Prosječna ocjena',
          value: _averageRating != null
              ? _averageRating!.toStringAsFixed(1)
              : '—',
          color: Colors.amber.shade800,
          onTap: () => _goTo(const SalonReviewListScreen()),
        ),
      ]);
    }

    return LayoutBuilder(
      builder: (context, constraints) {
        final crossAxisCount = constraints.maxWidth > 900 ? 4 : 2;
        return GridView.count(
          crossAxisCount: crossAxisCount,
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          mainAxisSpacing: 12,
          crossAxisSpacing: 12,
          childAspectRatio: crossAxisCount == 4 ? 1.6 : 1.35,
          children: cards,
        );
      },
    );
  }

  Widget _buildQuickLinks(AuthProvider auth) {
    final links = <_QuickLink>[
      _QuickLink(
        icon: Icons.favorite,
        label: 'Svadbe',
        screen: const WeddingListScreen(),
      ),
      _QuickLink(
        icon: Icons.event,
        label: 'Dnevni sastanci',
        screen: const DailyMeetingListScreen(),
      ),
      if (auth.isAdmin) ...[
        _QuickLink(
          icon: Icons.card_giftcard,
          label: 'Ponude',
          screen: const OfferListScreen(),
        ),
        _QuickLink(
          icon: Icons.star_rate,
          label: 'Recenzije',
          screen: const SalonReviewListScreen(),
        ),
      ],
      if (auth.isAdmin || auth.isEmployee)
        _QuickLink(
          icon: Icons.summarize,
          label: 'Izvještaji',
          screen: const ReportsScreen(),
        ),
    ];

    return Wrap(
      spacing: 10,
      runSpacing: 10,
      children: links
          .map(
            (link) => ActionChip(
              avatar: Icon(link.icon, size: 18, color: _brandColor),
              label: Text(link.label),
              onPressed: () => _goTo(link.screen),
              backgroundColor: _brandColor.withValues(alpha: 0.08),
              side: BorderSide(color: _brandColor.withValues(alpha: 0.2)),
            ),
          )
          .toList(),
    );
  }
}

class _StatCard extends StatelessWidget {
  const _StatCard({
    required this.icon,
    required this.label,
    required this.value,
    required this.color,
    this.onTap,
  });

  final IconData icon;
  final String label;
  final String value;
  final Color color;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    return Card(
      elevation: 1,
      clipBehavior: Clip.antiAlias,
      child: InkWell(
        onTap: onTap,
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Icon(icon, color: color, size: 28),
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    value,
                    style: TextStyle(
                      fontSize: 28,
                      fontWeight: FontWeight.bold,
                      color: color,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    label,
                    style: TextStyle(
                      fontSize: 13,
                      color: Colors.grey.shade600,
                    ),
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _QuickLink {
  const _QuickLink({
    required this.icon,
    required this.label,
    required this.screen,
  });

  final IconData icon;
  final String label;
  final Widget screen;
}
