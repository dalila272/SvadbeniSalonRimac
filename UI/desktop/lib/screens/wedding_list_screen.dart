import 'dart:async';

import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_desktop/layouts/master_screen.dart';
import 'package:svadbeni_salon_desktop/models/wedding.dart';
import 'package:svadbeni_salon_desktop/providers/offer_provider.dart';
import 'package:svadbeni_salon_desktop/providers/user_provider.dart';
import 'package:svadbeni_salon_desktop/providers/wedding_provider.dart';
import 'package:svadbeni_salon_desktop/screens/wedding_details_screen.dart';
import 'package:svadbeni_salon_desktop/utils/form_ux.dart';
import 'package:svadbeni_salon_desktop/utils/utils_widgets.dart';
import 'package:svadbeni_salon_desktop/constants/role_names.dart';
import 'package:table_calendar/table_calendar.dart';

class WeddingListScreen extends StatefulWidget {
  const WeddingListScreen({super.key});

  @override
  State<WeddingListScreen> createState() => _WeddingListScreenState();
}

class _WeddingListScreenState extends State<WeddingListScreen>
    with SingleTickerProviderStateMixin {
  static const _brandColor = Color(0xFF875252);
  static const _debounceDuration = Duration(milliseconds: 350);

  late WeddingProvider _provider;
  late TabController _tabController;

  DateTime _focusedDay = DateTime.now();
  DateTime? _selectedDay;
  List<Wedding> _weddings = [];
  List<Wedding> _allWeddings = [];
  Set<DateTime> _occupiedDates = {};
  bool _isLoading = true;

  final _clientController = TextEditingController();
  DateTime? _filterFrom;
  DateTime? _filterTo;
  int? _filterStatus;
  Timer? _searchDebounce;

  @override
  void initState() {
    super.initState();
    _provider = context.read<WeddingProvider>();
    _tabController = TabController(length: 2, vsync: this);
    _selectedDay = DateTime.now();
    _refresh();
  }

  @override
  void dispose() {
    _tabController.dispose();
    _clientController.dispose();
    _searchDebounce?.cancel();
    super.dispose();
  }

  DateTime _dateOnly(DateTime date) =>
      DateTime(date.year, date.month, date.day);

  bool _isSameDay(DateTime a, DateTime b) =>
      a.year == b.year && a.month == b.month && a.day == b.day;

  List<Wedding> _weddingsOnDay(DateTime day) {
    return _weddings.where((w) => _isSameDay(w.weddingDate, day)).toList();
  }

  Wedding? _activeWeddingOnDay(DateTime day) {
    for (final w in _weddingsOnDay(day)) {
      if (w.isActive) return w;
    }
    return null;
  }

  Future<void> _refresh() async {
    setState(() => _isLoading = true);
    try {
      await Future.wait([_loadCalendarMonth(), _loadList()]);
      if (mounted) setState(() => _isLoading = false);
    } catch (e) {
      if (mounted) {
        setState(() => _isLoading = false);
        alertBox(context, 'Greška', e.toString());
      }
    }
  }

  Future<void> _loadCalendarMonth() async {
    final monthResult = await _provider.getForMonth(_focusedDay);
    final occupied = await _provider.getOccupiedDates();
    if (!mounted) return;
    setState(() {
      _weddings = monthResult.items ?? [];
      _occupiedDates = occupied.map(_dateOnly).toSet();
    });
  }

  Map<String, dynamic> _listFilter() {
    final filter = <String, dynamic>{'pageSize': 100};
    if (_filterFrom != null) {
      filter['datumOd'] = DateTime(
        _filterFrom!.year,
        _filterFrom!.month,
        _filterFrom!.day,
      ).toIso8601String();
    }
    if (_filterTo != null) {
      filter['datumDo'] = DateTime(
        _filterTo!.year,
        _filterTo!.month,
        _filterTo!.day,
        23,
        59,
        59,
      ).toIso8601String();
    }
    if (_filterStatus != null) filter['status'] = _filterStatus;
    return filter;
  }

  Future<void> _loadList() async {
    final data = await _provider.get(filter: _listFilter());
    var items = data.items ?? [];
    final q = _clientController.text.trim().toLowerCase();
    if (q.isNotEmpty) {
      items = items
          .where((w) => w.clientName.toLowerCase().contains(q))
          .toList();
    }
    if (!mounted) return;
    setState(() {
      _allWeddings = items
        ..sort((a, b) => b.weddingDate.compareTo(a.weddingDate));
    });
  }

  void _scheduleListReload() {
    _searchDebounce?.cancel();
    _searchDebounce = Timer(_debounceDuration, _loadList);
  }

  static const _monthNames = [
    'Januar',
    'Februar',
    'Mart',
    'April',
    'Maj',
    'Juni',
    'Juli',
    'August',
    'Septembar',
    'Oktobar',
    'Novembar',
    'Decembar',
  ];

  String _monthTitle(DateTime date) =>
      '${_monthNames[date.month - 1]} ${date.year}';

  Future<void> _openCreate({required DateTime date}) async {
    final now = DateTime.now();
    final minDate = DateTime(now.year, now.month, now.day)
        .add(const Duration(days: 3));
    final day = DateTime(date.year, date.month, date.day);
    if (day.isBefore(minDate)) {
      if (mounted) {
        alertBox(
          context,
          'Datum',
          'Rezervacija mora biti najmanje 3 dana unaprijed.',
        );
      }
      return;
    }
    try {
      final offers = await context.read<OfferProvider>().get(
            filter: {'isActive': true, 'pageSize': 1},
          );
      if (!mounted) return;
      final customers = await context.read<UserProvider>().get(
            filter: {
              'pageSize': 1,
              'roleName': RoleNames.customer,
              'isActive': true,
            },
          );
      if (!mounted) return;
      final ok = await FormUx.ensurePrerequisites(
        context,
        missing: [
          if ((offers.items ?? []).isEmpty) 'barem jednu aktivnu ponudu',
          if ((customers.items ?? []).isEmpty) 'barem jednog klijenta (Customer)',
        ],
      );
      if (!ok || !mounted) return;
      await _openDetails(date: date);
    } on Exception catch (e) {
      if (mounted) alertBox(context, 'Greška', e.toString());
    }
  }

  Future<void> _openDetails({Wedding? wedding, required DateTime date}) async {
    final refresh = await Navigator.of(context).push(
      MaterialPageRoute(
        builder: (context) => WeddingDetailsScreen(
          wedding: wedding,
          initialDate: date,
        ),
      ),
    );
    if (!mounted) return;
    if (refresh == 'reload') {
      await _refresh();
    }
  }

  Future<void> _openDay(DateTime day) async {
    await _openDetails(wedding: _activeWeddingOnDay(day), date: day);
  }

  Widget? _dayCellBuilder(
    BuildContext context,
    DateTime day,
    DateTime focusedDay,
  ) {
    final wedding = _activeWeddingOnDay(day);
    final isOccupied = _occupiedDates.any((d) => _isSameDay(d, day));
    final isSelected = _selectedDay != null && _isSameDay(day, _selectedDay!);
    final isToday = _isSameDay(day, DateTime.now());
    final isOutsideMonth = day.month != focusedDay.month;

    Color? background;
    if (isSelected) {
      background = _brandColor.withValues(alpha: 0.25);
    } else if (wedding != null && wedding.isActive) {
      background = Colors.grey.shade200;
    }

    return Container(
      margin: const EdgeInsets.all(2),
      decoration: BoxDecoration(
        color: background,
        borderRadius: BorderRadius.circular(8),
        border: isToday
            ? Border.all(color: _brandColor, width: 1.5)
            : null,
      ),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.start,
        children: [
          const SizedBox(height: 4),
          Text(
            '${day.day}',
            style: TextStyle(
              color: isOutsideMonth ? Colors.grey : null,
              fontWeight: isToday ? FontWeight.bold : FontWeight.normal,
            ),
          ),
          if (wedding != null && wedding.isActive) ...[
            const SizedBox(height: 2),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 2),
              child: Text(
                wedding.clientName.isNotEmpty
                    ? wedding.clientName
                    : 'Rezervacija',
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
                textAlign: TextAlign.center,
                style: TextStyle(
                  fontSize: 10,
                  color: Colors.blue.shade700,
                  height: 1.1,
                ),
              ),
            ),
          ] else if (isOccupied && !isOutsideMonth) ...[
            const SizedBox(height: 4),
            Container(
              width: 6,
              height: 6,
              decoration: BoxDecoration(
                color: _brandColor.withValues(alpha: 0.6),
                shape: BoxShape.circle,
              ),
            ),
          ],
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: 'Svadbe',
      child: Column(
        children: [
          Material(
            color: Theme.of(context).colorScheme.surface,
            child: TabBar(
              controller: _tabController,
              labelColor: _brandColor,
              indicatorColor: _brandColor,
              tabs: const [
                Tab(icon: Icon(Icons.calendar_month), text: 'Kalendar'),
                Tab(icon: Icon(Icons.list), text: 'Lista'),
              ],
            ),
          ),
          Expanded(
            child: _isLoading
                ? const Center(child: CircularProgressIndicator())
                : TabBarView(
                    controller: _tabController,
                    children: [
                      _buildCalendarTab(),
                      _buildListTab(),
                    ],
                  ),
          ),
        ],
      ),
    );
  }

  Widget _buildCalendarTab() {
    final monthLabel = _monthTitle(_focusedDay);

    return Padding(
      padding: const EdgeInsets.all(12),
      child: Card(
        elevation: 2,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 12, 16, 0),
              child: Text(
                'Kliknite na datum za pregled ili novu rezervaciju.',
                style: Theme.of(context).textTheme.bodySmall?.copyWith(
                      color: Colors.grey.shade700,
                    ),
              ),
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 10, 16, 0),
              child: _buildCalendarLegend(),
            ),
            Expanded(
              child: Padding(
                padding: const EdgeInsets.all(12),
                child: LayoutBuilder(
                  builder: (context, constraints) {
                    const headerHeight = 52.0;
                    const daysOfWeekHeight = 16.0;
                    const rowCount = 6.0;
                    final rowHeight = ((constraints.maxHeight -
                                headerHeight -
                                daysOfWeekHeight) /
                            rowCount)
                        .clamp(42.0, 64.0);

                    return TableCalendar<Wedding>(
                      firstDay: DateTime.utc(2020, 1, 1),
                      lastDay: DateTime.utc(2035, 12, 31),
                      focusedDay: _focusedDay,
                      rowHeight: rowHeight,
                      selectedDayPredicate: (day) =>
                          _selectedDay != null &&
                          _isSameDay(day, _selectedDay!),
                      startingDayOfWeek: StartingDayOfWeek.monday,
                      calendarFormat: CalendarFormat.month,
                      availableCalendarFormats: const {
                        CalendarFormat.month: 'Mjesec',
                      },
                      headerStyle: HeaderStyle(
                        titleCentered: true,
                        formatButtonVisible: false,
                        titleTextFormatter: (date, locale) => monthLabel,
                      ),
                      calendarStyle: const CalendarStyle(
                        outsideDaysVisible: true,
                        cellMargin: EdgeInsets.zero,
                        markersMaxCount: 0,
                      ),
                      eventLoader: _weddingsOnDay,
                      onPageChanged: (focusedDay) {
                        setState(() => _focusedDay = focusedDay);
                        _loadCalendarMonth();
                      },
                      onDaySelected: (selectedDay, focusedDay) {
                        setState(() {
                          _selectedDay = selectedDay;
                          _focusedDay = focusedDay;
                        });
                        _openDay(selectedDay);
                      },
                      calendarBuilders: CalendarBuilders(
                        defaultBuilder: _dayCellBuilder,
                        todayBuilder: _dayCellBuilder,
                        selectedBuilder: _dayCellBuilder,
                        outsideBuilder: _dayCellBuilder,
                      ),
                    );
                  },
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildListTab() {
    return Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Wrap(
            spacing: 12,
            runSpacing: 12,
            crossAxisAlignment: WrapCrossAlignment.center,
            children: [
              SizedBox(
                width: 200,
                child: TextField(
                  controller: _clientController,
                  decoration: const InputDecoration(
                    labelText: 'Klijent',
                    border: OutlineInputBorder(),
                    isDense: true,
                  ),
                  onChanged: (_) => _scheduleListReload(),
                ),
              ),
              DropdownButton<int?>(
                value: _filterStatus,
                hint: const Text('Status'),
                items: const [
                  DropdownMenuItem(value: null, child: Text('Svi statusi')),
                  DropdownMenuItem(value: 0, child: Text('Na čekanju')),
                  DropdownMenuItem(value: 1, child: Text('Potvrđena')),
                  DropdownMenuItem(value: 2, child: Text('Otkazana')),
                  DropdownMenuItem(value: 3, child: Text('Završena')),
                ],
                onChanged: (v) {
                  setState(() => _filterStatus = v);
                  _loadList();
                },
              ),
              OutlinedButton.icon(
                onPressed: () async {
                  final picked = await showDatePicker(
                    context: context,
                    initialDate: _filterFrom ?? DateTime.now(),
                    firstDate: DateTime(2020),
                    lastDate: DateTime(DateTime.now().year + 2),
                  );
                  if (picked != null) {
                    setState(() => _filterFrom = picked);
                    _loadList();
                  }
                },
                icon: const Icon(Icons.date_range, size: 18),
                label: Text(
                  _filterFrom == null
                      ? 'Od datuma'
                      : DateFormat('dd.MM.yyyy.').format(_filterFrom!),
                ),
              ),
              OutlinedButton.icon(
                onPressed: () async {
                  final picked = await showDatePicker(
                    context: context,
                    initialDate: _filterTo ?? DateTime.now(),
                    firstDate: DateTime(2020),
                    lastDate: DateTime(DateTime.now().year + 2),
                  );
                  if (picked != null) {
                    setState(() => _filterTo = picked);
                    _loadList();
                  }
                },
                icon: const Icon(Icons.date_range, size: 18),
                label: Text(
                  _filterTo == null
                      ? 'Do datuma'
                      : DateFormat('dd.MM.yyyy.').format(_filterTo!),
                ),
              ),
              if (_filterFrom != null ||
                  _filterTo != null ||
                  _filterStatus != null ||
                  _clientController.text.isNotEmpty)
                TextButton(
                  onPressed: () {
                    setState(() {
                      _filterFrom = null;
                      _filterTo = null;
                      _filterStatus = null;
                      _clientController.clear();
                    });
                    _loadList();
                  },
                  child: const Text('Očisti'),
                ),
              ElevatedButton.icon(
                onPressed: () => _openCreate(
                  date: DateTime.now().add(const Duration(days: 30)),
                ),
                icon: const Icon(Icons.add),
                label: const Text('Nova rezervacija'),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Text(
            'Rezervacije (${_allWeddings.length})',
            style: Theme.of(context).textTheme.titleMedium,
          ),
          const SizedBox(height: 8),
          Expanded(
            child: _allWeddings.isEmpty
                ? const Center(
                    child: Text('Nema rezervacija za odabrane filtere.'),
                  )
                : Card(
                    elevation: 2,
                    child: SingleChildScrollView(
                      scrollDirection: Axis.horizontal,
                      child: SingleChildScrollView(
                        child: DataTable(
                          columns: const [
                            DataColumn(label: Text('Datum')),
                            DataColumn(label: Text('Klijent')),
                            DataColumn(label: Text('Paket')),
                            DataColumn(label: Text('Vrijeme')),
                            DataColumn(label: Text('Gosti')),
                            DataColumn(label: Text('Status')),
                          ],
                          rows: _allWeddings
                              .map(
                                (w) {
                                  void openRow() => _openDetails(
                                        wedding: w,
                                        date: w.weddingDate,
                                      );
                                  return DataRow(
                                    cells: [
                                      DataCell(
                                        Text(
                                          DateFormat('dd.MM.yyyy.')
                                              .format(w.weddingDate),
                                        ),
                                        onTap: openRow,
                                      ),
                                      DataCell(
                                        Text(
                                          w.clientName.isNotEmpty
                                              ? w.clientName
                                              : 'ID ${w.userId}',
                                        ),
                                        onTap: openRow,
                                      ),
                                      DataCell(
                                        Text(w.offerName),
                                        onTap: openRow,
                                      ),
                                      DataCell(
                                        Text(_formatTime(w.time)),
                                        onTap: openRow,
                                      ),
                                      DataCell(
                                        Text(w.guestCount.toString()),
                                        onTap: openRow,
                                      ),
                                      DataCell(
                                        _statusChip(w),
                                        onTap: openRow,
                                      ),
                                    ],
                                  );
                                },
                              )
                              .toList(),
                        ),
                      ),
                    ),
                  ),
          ),
        ],
      ),
    );
  }

  String _formatTime(String time) {
    final parts = time.split(':');
    if (parts.length >= 2) {
      return '${parts[0]}:${parts[1]}';
    }
    return time;
  }

  Widget _statusChip(Wedding wedding) {
    Color color;
    switch (wedding.status) {
      case 0:
        color = Colors.orange;
      case 1:
        color = Colors.green;
      case 2:
        color = Colors.red;
      case 3:
        color = Colors.blueGrey;
      default:
        color = Colors.grey;
    }

    return Chip(
      label: Text(
        wedding.statusLabel,
        style: const TextStyle(fontSize: 12, color: Colors.white),
      ),
      backgroundColor: color,
      padding: EdgeInsets.zero,
      visualDensity: VisualDensity.compact,
    );
  }

  Widget _buildCalendarLegend() {
    final labelStyle = Theme.of(context).textTheme.bodySmall?.copyWith(
          color: Colors.grey.shade800,
        );

    return Wrap(
      spacing: 16,
      runSpacing: 8,
      crossAxisAlignment: WrapCrossAlignment.center,
      children: [
        _legendSwatch(
          color: Colors.grey.shade200,
          label: 'Aktivna svadba',
          labelStyle: labelStyle,
        ),
        _legendSwatch(
          color: _brandColor.withValues(alpha: 0.25),
          label: 'Odabrani dan',
          labelStyle: labelStyle,
        ),
        _legendSwatch(
          color: Colors.transparent,
          border: Border.all(color: _brandColor, width: 1.5),
          label: 'Danas',
          labelStyle: labelStyle,
        ),
        Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(
              'Klijent',
              style: TextStyle(
                fontSize: 10,
                color: Colors.blue.shade700,
              ),
            ),
            const SizedBox(width: 6),
            Text('Ime klijenta u ćeliji', style: labelStyle),
          ],
        ),
      ],
    );
  }

  Widget _legendSwatch({
    required Color color,
    required String label,
    required TextStyle? labelStyle,
    BoxBorder? border,
  }) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Container(
          width: 18,
          height: 18,
          decoration: BoxDecoration(
            color: color,
            borderRadius: BorderRadius.circular(4),
            border: border,
          ),
        ),
        const SizedBox(width: 6),
        Text(label, style: labelStyle),
      ],
    );
  }
}
