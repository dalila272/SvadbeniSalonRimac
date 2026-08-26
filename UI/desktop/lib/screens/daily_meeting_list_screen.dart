import 'dart:async';

import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_desktop/layouts/master_screen.dart';
import 'package:svadbeni_salon_desktop/models/daily_meeting.dart';
import 'package:svadbeni_salon_desktop/providers/daily_meeting_provider.dart';
import 'package:svadbeni_salon_desktop/screens/daily_meeting_details_screen.dart';
import 'package:svadbeni_salon_desktop/utils/utils_widgets.dart';
import 'package:table_calendar/table_calendar.dart';

class DailyMeetingListScreen extends StatefulWidget {
  const DailyMeetingListScreen({super.key});

  @override
  State<DailyMeetingListScreen> createState() =>
      _DailyMeetingListScreenState();
}

class _DailyMeetingListScreenState extends State<DailyMeetingListScreen>
    with SingleTickerProviderStateMixin {
  static const _brandColor = Color(0xFF875252);
  static const _debounceDuration = Duration(milliseconds: 350);

  late DailyMeetingProvider _provider;
  late TabController _tabController;

  DateTime _focusedDay = DateTime.now();
  DateTime? _selectedDay;
  List<DailyMeeting> _monthMeetings = [];
  List<DailyMeeting> _listMeetings = [];
  bool _isLoading = true;

  final _clientController = TextEditingController();
  DateTime? _filterFrom;
  DateTime? _filterTo;
  int? _filterStatus;
  Timer? _searchDebounce;

  @override
  void initState() {
    super.initState();
    _provider = context.read<DailyMeetingProvider>();
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

  bool _isSameDay(DateTime a, DateTime b) =>
      a.year == b.year && a.month == b.month && a.day == b.day;

  List<DailyMeeting> _meetingsOnDay(DateTime day) {
    return _monthMeetings
        .where((m) => _isSameDay(m.meetingDate, day))
        .toList()
      ..sort((a, b) => a.meetingDate.compareTo(b.meetingDate));
  }

  List<DailyMeeting> _activeMeetingsOnDay(DateTime day) {
    return _meetingsOnDay(day).where((m) => m.isActive).toList();
  }

  int _activeCountOnDay(DateTime day) => _activeMeetingsOnDay(day).length;

  bool _hasConfirmedOnDay(DateTime day) {
    return _activeMeetingsOnDay(day).any((m) => m.status == 1);
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
    if (!mounted) return;
    setState(() => _monthMeetings = monthResult.items ?? []);
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
          .where((m) => m.displayName.toLowerCase().contains(q))
          .toList();
    }
    if (!mounted) return;
    setState(() {
      _listMeetings = items
        ..sort((a, b) => b.meetingDate.compareTo(a.meetingDate));
    });
  }

  void _scheduleListReload() {
    _searchDebounce?.cancel();
    _searchDebounce = Timer(_debounceDuration, _loadList);
  }

  Future<void> _openDetails({
    DailyMeeting? meeting,
    required DateTime initialDateTime,
  }) async {
    final refresh = await Navigator.of(context).push(
      MaterialPageRoute(
        builder: (context) => DailyMeetingDetailsScreen(
          meeting: meeting,
          initialDateTime: initialDateTime,
        ),
      ),
    );
    if (!mounted) return;
    if (refresh == 'reload') await _refresh();
  }

  Future<void> _openDay(DateTime day) async {
    final onDay = _meetingsOnDay(day);
    if (onDay.isEmpty) {
      await _openDetails(
        initialDateTime: DateTime(day.year, day.month, day.day, 9, 0),
      );
      return;
    }

    if (!mounted) return;
    await showDialog<void>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text('Sastanci — ${DateFormat('dd.MM.yyyy.').format(day)}'),
        content: SizedBox(
          width: 420,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              ...onDay.map(
                (m) => ListTile(
                  contentPadding: EdgeInsets.zero,
                  title: Text(
                    DateFormat('HH:mm').format(m.meetingDate),
                    style: const TextStyle(fontWeight: FontWeight.w600),
                  ),
                  subtitle: Text(
                    '${m.displayName} · ${m.statusLabel}',
                  ),
                  onTap: () {
                    Navigator.pop(ctx);
                    _openDetails(
                      meeting: m,
                      initialDateTime: m.meetingDate,
                    );
                  },
                ),
              ),
            ],
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('Zatvori'),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.pop(ctx);
              _openDetails(
                initialDateTime: DateTime(day.year, day.month, day.day, 9, 0),
              );
            },
            child: const Text('Novi sastanak'),
          ),
        ],
      ),
    );
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

  Widget? _dayCellBuilder(
    BuildContext context,
    DateTime day,
    DateTime focusedDay,
  ) {
    final count = _activeCountOnDay(day);
    final hasConfirmed = _hasConfirmedOnDay(day);
    final isSelected = _selectedDay != null && _isSameDay(day, _selectedDay!);
    final isToday = _isSameDay(day, DateTime.now());
    final isOutsideMonth = day.month != focusedDay.month;

    Color? background;
    BoxBorder? border;
    if (count > 0) {
      background =
          hasConfirmed ? Colors.blue.shade100 : Colors.blue.shade50;
    }
    if (isSelected) {
      border = Border.all(color: _brandColor, width: 2);
    } else if (isToday) {
      border = Border.all(color: _brandColor, width: 1.5);
    }

    final badgeColor =
        hasConfirmed ? Colors.blue.shade700 : _brandColor.withValues(alpha: 0.85);

    return Container(
      margin: const EdgeInsets.all(2),
      clipBehavior: Clip.antiAlias,
      decoration: BoxDecoration(
        color: background,
        borderRadius: BorderRadius.circular(8),
        border: border,
      ),
      child: FittedBox(
        fit: BoxFit.scaleDown,
        alignment: Alignment.topCenter,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const SizedBox(height: 2),
            Text(
              '${day.day}',
              style: TextStyle(
                fontSize: 13,
                color: isOutsideMonth ? Colors.grey : null,
                fontWeight: isToday ? FontWeight.bold : FontWeight.normal,
              ),
            ),
            if (count > 0) ...[
              const SizedBox(height: 1),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 5, vertical: 1),
                decoration: BoxDecoration(
                  color: badgeColor,
                  borderRadius: BorderRadius.circular(10),
                ),
                child: Text(
                  '$count',
                  style: const TextStyle(color: Colors.white, fontSize: 9),
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: 'Dnevni sastanci',
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
                'Kliknite na datum — prikaz termina ili novi sastanak.',
                style: Theme.of(context).textTheme.bodySmall?.copyWith(
                      color: Colors.grey.shade700,
                    ),
              ),
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 8, 16, 0),
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
                    const verticalBuffer = 8.0;
                    final availableHeight =
                        constraints.maxHeight - verticalBuffer;
                    final rowHeight = ((availableHeight -
                                headerHeight -
                                daysOfWeekHeight) /
                            rowCount)
                        .clamp(38.0, 64.0);
                    final calendarHeight =
                        headerHeight + daysOfWeekHeight + rowCount * rowHeight;

                    return Align(
                      alignment: Alignment.topCenter,
                      child: SizedBox(
                        height: calendarHeight.clamp(0, constraints.maxHeight),
                        width: constraints.maxWidth,
                        child: TableCalendar<DailyMeeting>(
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
                            titleTextFormatter: (date, locale) =>
                                _monthTitle(date),
                          ),
                          calendarStyle: const CalendarStyle(
                            outsideDaysVisible: true,
                            cellMargin: EdgeInsets.zero,
                            markersMaxCount: 0,
                          ),
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
                        ),
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
                  DropdownMenuItem(value: 1, child: Text('Potvrđeni')),
                  DropdownMenuItem(value: 2, child: Text('Otkazani')),
                  DropdownMenuItem(value: 3, child: Text('Završeni')),
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
                onPressed: () => _openDetails(
                  initialDateTime: DateTime.now().add(const Duration(days: 1)),
                ),
                icon: const Icon(Icons.add),
                label: const Text('Novi sastanak'),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Expanded(
            child: _listMeetings.isEmpty
                ? const Center(child: Text('Nema sastanaka.'))
                : Card(
                    elevation: 2,
                    child: SingleChildScrollView(
                      scrollDirection: Axis.horizontal,
                      child: SingleChildScrollView(
                        child: DataTable(
                          columns: const [
                            DataColumn(label: Text('Datum i vrijeme')),
                            DataColumn(label: Text('Klijent')),
                            DataColumn(label: Text('Status')),
                            DataColumn(label: Text('Napomena')),
                          ],
                          rows: _listMeetings
                              .map(
                                (m) => DataRow(
                                  cells: [
                                    DataCell(
                                      Text(
                                        DateFormat('dd.MM.yyyy. HH:mm')
                                            .format(m.meetingDate),
                                      ),
                                      onTap: () => _openDetails(
                                        meeting: m,
                                        initialDateTime: m.meetingDate,
                                      ),
                                    ),
                                    DataCell(
                                      Text(m.displayName),
                                      onTap: () => _openDetails(
                                        meeting: m,
                                        initialDateTime: m.meetingDate,
                                      ),
                                    ),
                                    DataCell(Text(m.statusLabel)),
                                    DataCell(Text(m.note ?? '—')),
                                  ],
                                ),
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

  Widget _buildCalendarLegend() {
    final labelStyle = Theme.of(context).textTheme.bodySmall?.copyWith(
          color: Colors.grey.shade800,
        );

    return Wrap(
      spacing: 12,
      runSpacing: 6,
      crossAxisAlignment: WrapCrossAlignment.center,
      children: [
        _legendSwatch(
          color: Colors.blue.shade50,
          label: 'Na čekanju',
          labelStyle: labelStyle,
        ),
        _legendSwatch(
          color: Colors.blue.shade100,
          label: 'Potvrđen sastanak',
          labelStyle: labelStyle,
        ),
        _legendSwatch(
          color: Colors.transparent,
          border: Border.all(color: _brandColor, width: 2),
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
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 1),
              decoration: BoxDecoration(
                color: Colors.blue.shade700,
                borderRadius: BorderRadius.circular(10),
              ),
              child: const Text(
                '2',
                style: TextStyle(color: Colors.white, fontSize: 10),
              ),
            ),
            const SizedBox(width: 6),
            Text('Broj sastanaka', style: labelStyle),
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
