import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_rimac/constants/colors.dart';
import 'package:svadbeni_salon_rimac/models/daily_meeting.dart';
import 'package:svadbeni_salon_rimac/models/recommendation.dart';
import 'package:svadbeni_salon_rimac/models/wedding.dart';
import 'package:svadbeni_salon_rimac/providers/auth_provider.dart';
import 'package:svadbeni_salon_rimac/providers/daily_meeting_provider.dart';
import 'package:svadbeni_salon_rimac/providers/recommendations_provider.dart';
import 'package:svadbeni_salon_rimac/providers/wedding_provider.dart';
import 'package:svadbeni_salon_rimac/utils/master_screen.dart';
import 'package:svadbeni_salon_rimac/utils/routes.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  final _weddingProvider = WeddingProvider();
  final _meetingProvider = DailyMeetingProvider();
  final _recommendationsProvider = RecommendationsProvider();

  bool _isLoading = true;
  String? _errorMessage;
  Wedding? _activeWedding;
  DailyMeeting? _nextMeeting;
  List<Recommendation> _recommendations = [];

  @override
  void initState() {
    super.initState();
    _loadData();
  }

  Future<void> _loadData() async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    try {
      final now = DateTime.now();
      final today = DateTime(now.year, now.month, now.day);

      final pendingWeddingFuture = _weddingProvider.get(filter: {
        'status': 0,
        'pageSize': 1,
      });
      final confirmedWeddingFuture = _weddingProvider.get(filter: {
        'status': 1,
        'pageSize': 1,
      });
      final pendingMeetingsFuture = _meetingProvider.get(filter: {
        'status': 0,
        'datumOd': today.toIso8601String(),
        'pageSize': 50,
      });
      final confirmedMeetingsFuture = _meetingProvider.get(filter: {
        'status': 1,
        'datumOd': today.toIso8601String(),
        'pageSize': 50,
      });

      final pendingWedding = await pendingWeddingFuture;
      final confirmedWedding = await confirmedWeddingFuture;
      final pendingMeetings = await pendingMeetingsFuture;
      final confirmedMeetings = await confirmedMeetingsFuture;

      Wedding? activeWedding;
      if (pendingWedding.items.isNotEmpty) {
        activeWedding = pendingWedding.items.first;
      } else if (confirmedWedding.items.isNotEmpty) {
        activeWedding = confirmedWedding.items.first;
      }

      // Preporuke/ponude su relevantne dok klijent još bira paket.
      final recommendations = activeWedding == null
          ? await _recommendationsProvider.fetchRecommendations(limit: 5)
          : <Recommendation>[];

      final upcomingMeetings = <DailyMeeting>[
        ...pendingMeetings.items,
        ...confirmedMeetings.items,
      ]..sort((a, b) => a.meetingDate.compareTo(b.meetingDate));

      DailyMeeting? nextMeeting =
          upcomingMeetings.isEmpty ? null : upcomingMeetings.first;

      if (!mounted) return;
      setState(() {
        _activeWedding = activeWedding;
        _nextMeeting = nextMeeting;
        _recommendations = recommendations;
        _isLoading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _errorMessage = e.toString().replaceFirst('Exception: ', '');
        _isLoading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final displayName = context.watch<AuthProvider>().displayName;

    return MasterScreenWidget(
      title: 'Početna',
      child: RefreshIndicator(
        onRefresh: _loadData,
        color: AppColors.primaryColor,
        child: _isLoading
            ? const Center(child: CircularProgressIndicator())
            : SingleChildScrollView(
                physics: const AlwaysScrollableScrollPhysics(),
                padding: const EdgeInsets.all(20),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    _buildWelcomeHeader(displayName),
                    const SizedBox(height: 28),
                    if (_errorMessage != null) ...[
                      Text(
                        _errorMessage!,
                        style: const TextStyle(color: AppColors.errorColor),
                        textAlign: TextAlign.center,
                      ),
                      const SizedBox(height: 16),
                    ],
                    _buildUpcomingSection(),
                    if (_activeWedding == null) ...[
                      const SizedBox(height: 28),
                      _buildRecommendationsSection(),
                    ],
                    const SizedBox(height: 28),
                    Text(
                      'Brze akcije',
                      style: Theme.of(context).textTheme.titleLarge?.copyWith(
                            fontWeight: FontWeight.w600,
                          ),
                    ),
                    const SizedBox(height: 12),
                    if (_activeWedding == null) ...[
                      _buildActionCard(
                        icon: Icons.card_giftcard_outlined,
                        title: 'Pogledaj ponude',
                        subtitle: 'Paketi i cijene za vašu svadbu',
                        onTap: () =>
                            Navigator.pushNamed(context, AppRoutes.packages),
                      ),
                      const SizedBox(height: 12),
                    ],
                    _buildActionCard(
                      icon: Icons.favorite_outline,
                      title: 'Moja svadba',
                      subtitle: _activeWedding != null
                          ? 'Status, datum i detalji rezervacije'
                          : 'Rezervacija, datum i detalji',
                      onTap: () =>
                          Navigator.pushNamed(context, AppRoutes.wedding),
                    ),
                    const SizedBox(height: 12),
                    _buildActionCard(
                      icon: Icons.history,
                      title: 'Moje aktivnosti',
                      subtitle: 'Sastanci — predstojeći i historija',
                      onTap: () => Navigator.pushNamed(
                        context,
                        AppRoutes.dailyMeetings,
                      ),
                    ),
                    if (_activeWedding != null) ...[
                      const SizedBox(height: 12),
                      _buildActionCard(
                        icon: Icons.star_rate_outlined,
                        title: 'Ocjena paketa',
                        subtitle: 'Dostupno nakon završene svadbe',
                        onTap: () =>
                            Navigator.pushNamed(context, AppRoutes.review),
                      ),
                    ],
                  ],
                ),
              ),
      ),
    );
  }

  Widget _buildWelcomeHeader(String displayName) {
    return Column(
      children: [
        Image.asset(
          'assets/wedding.png',
          width: 88,
          height: 88,
          fit: BoxFit.contain,
        ),
        const SizedBox(height: 16),
        Text(
          'Dobrodošli, $displayName!',
          style: Theme.of(context).textTheme.titleLarge?.copyWith(
                fontWeight: FontWeight.bold,
                color: AppColors.primaryColor,
              ),
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: 8),
        Text(
          _activeWedding != null
              ? 'Vaša svadba je zakazana — pratite termine i detalje ispod.'
              : 'Planirajte svoj savršeni dan u salonu Rimac.',
          style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                color: Colors.grey[600],
              ),
          textAlign: TextAlign.center,
        ),
      ],
    );
  }

  Widget _buildRecommendationsSection() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Row(
          children: [
            Expanded(
              child: Text(
                'Preporučeno za vas',
                style: Theme.of(context).textTheme.titleLarge?.copyWith(
                      fontWeight: FontWeight.w600,
                    ),
              ),
            ),
            TextButton(
              onPressed: () =>
                  Navigator.pushNamed(context, AppRoutes.interests),
              child: const Text('Interesi'),
            ),
          ],
        ),
        const SizedBox(height: 8),
        if (_recommendations.isEmpty)
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: AppColors.accentColor.withValues(alpha: 0.35),
              borderRadius: BorderRadius.circular(16),
            ),
            child: Text(
              'Postavite interese da dobijete personalizovane pakete.',
              style: TextStyle(color: Colors.grey[700]),
              textAlign: TextAlign.center,
            ),
          )
        else
          ..._recommendations.map(_buildRecommendationCard),
      ],
    );
  }

  Widget _buildRecommendationCard(Recommendation item) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Material(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        elevation: 1,
        child: InkWell(
          borderRadius: BorderRadius.circular(16),
          onTap: () => Navigator.pushNamed(
            context,
            AppRoutes.packageDetails,
            arguments: item.ponudaId,
          ),
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Expanded(
                      child: Text(
                        item.naziv,
                        style: const TextStyle(
                          fontWeight: FontWeight.w600,
                          fontSize: 16,
                        ),
                      ),
                    ),
                    Text(
                      '${item.cijena} KM',
                      style: const TextStyle(
                        color: AppColors.primaryColor,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 8),
                Text(
                  item.razlog,
                  style: TextStyle(color: Colors.grey[700], height: 1.35),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildUpcomingSection() {
    final hasUpcoming = _activeWedding != null || _nextMeeting != null;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text(
          'Sljedeći termini',
          style: Theme.of(context).textTheme.titleLarge?.copyWith(
                fontWeight: FontWeight.w600,
              ),
        ),
        const SizedBox(height: 12),
        if (!hasUpcoming)
          Container(
            padding: const EdgeInsets.all(20),
            decoration: BoxDecoration(
              color: AppColors.accentColor.withValues(alpha: 0.35),
              borderRadius: BorderRadius.circular(16),
            ),
            child: Text(
              'Nemate aktivnih termina. Pogledajte ponude ili zakažite sastanak.',
              style: TextStyle(color: Colors.grey[700]),
              textAlign: TextAlign.center,
            ),
          )
        else ...[
          if (_activeWedding != null) _buildWeddingCard(_activeWedding!),
          if (_activeWedding != null && _nextMeeting != null)
            const SizedBox(height: 12),
          if (_nextMeeting != null) _buildMeetingCard(_nextMeeting!),
        ],
      ],
    );
  }

  Widget _buildWeddingCard(Wedding wedding) {
    final date = DateFormat('dd.MM.yyyy.').format(wedding.weddingDate);
    final time = wedding.time.substring(0, 5);

    return _buildInfoCard(
      icon: Icons.favorite,
      title: wedding.offerName,
      lines: [
        'Datum: $date u $time',
        'Gostiju: ${wedding.guestCount}',
        'Status: ${wedding.statusLabel}',
      ],
      onTap: () => Navigator.pushNamed(context, AppRoutes.wedding),
    );
  }

  Widget _buildMeetingCard(DailyMeeting meeting) {
    final date = DateFormat('dd.MM.yyyy. HH:mm').format(meeting.meetingDate);

    return _buildInfoCard(
      icon: Icons.event,
      title: 'Sastanak u salonu',
      lines: [
        'Termin: $date',
        'Status: ${meeting.statusLabel}',
      ],
      onTap: () => Navigator.pushNamed(context, AppRoutes.dailyMeetings),
    );
  }

  Widget _buildInfoCard({
    required IconData icon,
    required String title,
    required List<String> lines,
    required VoidCallback onTap,
  }) {
    return Material(
      color: Colors.white,
      borderRadius: BorderRadius.circular(16),
      elevation: 1,
      shadowColor: AppColors.primaryColor.withValues(alpha: 0.15),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(16),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                padding: const EdgeInsets.all(10),
                decoration: BoxDecoration(
                  color: AppColors.accentColor.withValues(alpha: 0.5),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Icon(icon, color: AppColors.primaryColor),
              ),
              const SizedBox(width: 14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      title,
                      style: const TextStyle(
                        fontWeight: FontWeight.w600,
                        fontSize: 16,
                      ),
                    ),
                    const SizedBox(height: 6),
                    for (final line in lines)
                      Padding(
                        padding: const EdgeInsets.only(bottom: 2),
                        child: Text(
                          line,
                          style: TextStyle(
                            fontSize: 13,
                            color: Colors.grey[700],
                          ),
                        ),
                      ),
                  ],
                ),
              ),
              Icon(Icons.chevron_right, color: Colors.grey[400]),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildActionCard({
    required IconData icon,
    required String title,
    required String subtitle,
    required VoidCallback onTap,
  }) {
    return Material(
      color: Colors.white,
      borderRadius: BorderRadius.circular(16),
      elevation: 1,
      shadowColor: AppColors.primaryColor.withValues(alpha: 0.12),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(16),
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
          child: Row(
            children: [
              Icon(icon, color: AppColors.primaryColor, size: 28),
              const SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      title,
                      style: const TextStyle(
                        fontWeight: FontWeight.w600,
                        fontSize: 16,
                      ),
                    ),
                    const SizedBox(height: 2),
                    Text(
                      subtitle,
                      style: TextStyle(fontSize: 13, color: Colors.grey[600]),
                    ),
                  ],
                ),
              ),
              Icon(Icons.arrow_forward_ios, size: 16, color: Colors.grey[400]),
            ],
          ),
        ),
      ),
    );
  }
}
