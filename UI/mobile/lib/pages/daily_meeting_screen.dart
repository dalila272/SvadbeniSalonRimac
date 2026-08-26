import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:svadbeni_salon_rimac/constants/colors.dart';
import 'package:svadbeni_salon_rimac/models/daily_meeting.dart';
import 'package:svadbeni_salon_rimac/providers/daily_meeting_provider.dart';
import 'package:svadbeni_salon_rimac/utils/daily_meeting_slots.dart';
import 'package:svadbeni_salon_rimac/utils/master_screen.dart';

class DailyMeetingScreen extends StatefulWidget {
  const DailyMeetingScreen({super.key});

  @override
  State<DailyMeetingScreen> createState() => _DailyMeetingScreenState();
}

class _DailyMeetingScreenState extends State<DailyMeetingScreen> {
  final _provider = DailyMeetingProvider();

  List<DailyMeeting> _meetings = [];
  List<DateTime> _busySlots = [];
  bool _showNewMeetingFields = false;
  DateTime? _newMeetingDate;
  bool _isLoading = true;
  bool _isSaving = false;
  String _errorMessage = '';

  @override
  void initState() {
    super.initState();
    _loadMeetings();
  }

  Future<void> _loadMeetings() async {
    setState(() {
      _isLoading = true;
      _errorMessage = '';
    });

    try {
      final result = await _provider.get(filter: {'pageSize': 100});
      final busySlots = await _provider.getBusySlots();
      final items = List<DailyMeeting>.from(result.items)
        ..sort((a, b) => b.meetingDate.compareTo(a.meetingDate));
      setState(() {
        _meetings = items;
        _busySlots = busySlots;
        _isLoading = false;
      });
    } catch (e) {
      setState(() {
        _isLoading = false;
        _errorMessage = e.toString().replaceFirst('Exception: ', '');
      });
    }
  }

  bool _isUpcoming(DailyMeeting m) {
    final now = DateTime.now();
    return (m.status == 0 || m.status == 1) && m.meetingDate.isAfter(now);
  }

  List<DailyMeeting> get _upcoming =>
      _meetings.where(_isUpcoming).toList()
        ..sort((a, b) => a.meetingDate.compareTo(b.meetingDate));

  List<DailyMeeting> get _history =>
      _meetings.where((m) => !_isUpcoming(m)).toList();

  bool _isSlotTaken(DateTime slot, {DateTime? excludeSlot}) {
    return DailyMeetingSlots.isSlotTaken(
      slot,
      _busySlots,
      excludeSlot: excludeSlot,
    );
  }

  Future<void> _addMeeting() async {
    if (_newMeetingDate == null) return;

    if (_isSlotTaken(_newMeetingDate!)) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Odabrani termin se preklapa s postojećim sastankom.'),
        ),
      );
      return;
    }

    setState(() => _isSaving = true);
    try {
      await _provider.insert({
        'datumSastanka': _newMeetingDate!.toIso8601String(),
      });
      setState(() {
        _showNewMeetingFields = false;
        _newMeetingDate = null;
      });
      await _loadMeetings();
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(e.toString().replaceFirst('Exception: ', ''))),
        );
      }
    } finally {
      if (mounted) setState(() => _isSaving = false);
    }
  }

  Future<void> _removeMeeting(DailyMeeting meeting) async {
    if (!meeting.canEdit) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Samo sastanci na čekanju se mogu obrisati.'),
        ),
      );
      return;
    }

    final reasonController = TextEditingController();
    final formKey = GlobalKey<FormState>();
    final razlog = await showDialog<String>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Row(
          children: [
            const Expanded(child: Text('Otkazivanje sastanka')),
            IconButton(
              icon: const Icon(Icons.close),
              onPressed: () => Navigator.pop(ctx),
            ),
          ],
        ),
        content: Form(
          key: formKey,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(
                'Otkazati sastanak od ${DateFormat('dd.MM.yyyy. – HH:mm').format(meeting.meetingDate)}? '
                'Status će biti „otkazan“ (nema brisanja zapisa).',
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: reasonController,
                decoration: const InputDecoration(
                  labelText: 'Razlog',
                  border: OutlineInputBorder(),
                ),
                maxLines: 2,
                validator: (v) =>
                    (v == null || v.trim().length < 3)
                        ? 'Najmanje 3 karaktera'
                        : null,
              ),
            ],
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('Odustani'),
          ),
          ElevatedButton(
            onPressed: () {
              if (!(formKey.currentState?.validate() ?? false)) return;
              Navigator.pop(ctx, reasonController.text.trim());
            },
            child: const Text('Otkaži'),
          ),
        ],
      ),
    );
    reasonController.dispose();
    if (razlog == null) return;

    setState(() => _isSaving = true);
    try {
      await _provider.cancel(meeting.id, razlog: razlog);
      await _loadMeetings();
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(e.toString().replaceFirst('Exception: ', ''))),
        );
      }
    } finally {
      if (mounted) setState(() => _isSaving = false);
    }
  }

  Future<void> _editMeeting(DailyMeeting meeting) async {
    if (!meeting.canEdit) return;

    final picked = await showDatePicker(
      context: context,
      initialDate: meeting.meetingDate,
      firstDate: DateTime.now(),
      lastDate: DateTime(DateTime.now().year + 1, 12, 31),
    );

    if (picked == null) return;

    if (!mounted) return;
    final time = await _selectTimeWithinRange(context);
    if (time == null) return;

    final updated = DateTime(
      picked.year,
      picked.month,
      picked.day,
      time.hour,
      time.minute,
    );

    if (_isSlotTaken(updated, excludeSlot: meeting.meetingDate)) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text(
              'Odabrani termin se preklapa s postojećim sastankom.',
            ),
          ),
        );
      }
      return;
    }

    setState(() => _isSaving = true);
    try {
      await _provider.update(meeting.id, {
        'datumSastanka': updated.toIso8601String(),
      });
      await _loadMeetings();
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(e.toString().replaceFirst('Exception: ', ''))),
        );
      }
    } finally {
      if (mounted) setState(() => _isSaving = false);
    }
  }

  Future<void> _selectNewMeetingDate() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: DateTime.now(),
      firstDate: DateTime.now(),
      lastDate: DateTime(DateTime.now().year + 1, 12, 31),
    );

    if (picked != null) {
      if (!mounted) return;
      final time = await _selectTimeWithinRange(context);
      if (time != null) {
        setState(() {
          _newMeetingDate = DateTime(
            picked.year,
            picked.month,
            picked.day,
            time.hour,
            time.minute,
          );
        });
      }
    }
  }

  Future<TimeOfDay?> _selectTimeWithinRange(BuildContext context) async {
    while (true) {
      final selectedTime = await showTimePicker(
        context: context,
        initialTime: const TimeOfDay(hour: 8, minute: 0),
      );

      if (selectedTime == null) return null;

      if (DailyMeetingSlots.isValidStart(DateTime(
        2000,
        1,
        1,
        selectedTime.hour,
        selectedTime.minute,
      ))) {
        return selectedTime;
      }

      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text(
              'Termin mora biti na puni sat ili pola sata.',
            ),
          ),
        );
      }
    }
  }

  Color _statusColor(int status) {
    switch (status) {
      case 0:
        return Colors.orange.shade700;
      case 1:
        return Colors.green.shade700;
      case 2:
        return Colors.red.shade700;
      case 3:
        return Colors.blueGrey.shade600;
      default:
        return Colors.grey;
    }
  }

  Widget _statusChip(DailyMeeting meeting) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
      decoration: BoxDecoration(
        color: _statusColor(meeting.status),
        borderRadius: BorderRadius.circular(12),
      ),
      child: Text(
        meeting.statusLabel,
        style: const TextStyle(fontSize: 11, color: Colors.white, height: 1.1),
      ),
    );
  }

  Widget _sectionTitle(String text) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
      child: Text(
        text,
        style: const TextStyle(
          fontSize: 16,
          fontWeight: FontWeight.w600,
          color: AppColors.textColor,
        ),
      ),
    );
  }

  Widget _meetingTile(DailyMeeting meeting) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisSize: MainAxisSize.min,
              children: [
                Text(
                  DateFormat('dd.MM.yyyy. – HH:mm').format(meeting.meetingDate),
                  style: const TextStyle(fontWeight: FontWeight.w500),
                ),
                const SizedBox(height: 6),
                _statusChip(meeting),
              ],
            ),
          ),
          if (meeting.canEdit) ...[
            IconButton(
              visualDensity: VisualDensity.compact,
              constraints: const BoxConstraints(minWidth: 36, minHeight: 36),
              icon: const Icon(Icons.edit_outlined),
              onPressed: _isSaving ? null : () => _editMeeting(meeting),
            ),
            IconButton(
              visualDensity: VisualDensity.compact,
              constraints: const BoxConstraints(minWidth: 36, minHeight: 36),
              icon: const Icon(Icons.delete_outline),
              onPressed: _isSaving ? null : () => _removeMeeting(meeting),
            ),
          ],
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final upcoming = _upcoming;
    final history = _history;

    return MasterScreenWidget(
      title: 'Moje aktivnosti',
      child: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : Column(
              children: [
                Padding(
                  padding: const EdgeInsets.fromLTRB(16, 16, 16, 0),
                  child: Text(
                    'Pregled vaših sastanaka u salonu — predstojeći termini i historija.',
                    style: TextStyle(color: Colors.grey[700], height: 1.35),
                  ),
                ),
                if (_errorMessage.isNotEmpty)
                  Padding(
                    padding: const EdgeInsets.all(8.0),
                    child: Text(
                      _errorMessage,
                      style: const TextStyle(color: Colors.red),
                    ),
                  ),
                Expanded(
                  child: _meetings.isEmpty
                      ? Center(
                          child: Padding(
                            padding: const EdgeInsets.all(24),
                            child: Text(
                              'Još nemate aktivnosti. Zakažite sastanak da dogovorite posjet salonu.',
                              textAlign: TextAlign.center,
                              style: TextStyle(color: Colors.grey[700]),
                            ),
                          ),
                        )
                      : ListView(
                          children: [
                            if (upcoming.isNotEmpty) ...[
                              _sectionTitle('Predstojeći'),
                              ...upcoming.map(_meetingTile),
                            ],
                            if (history.isNotEmpty) ...[
                              _sectionTitle('Historija'),
                              ...history.map(_meetingTile),
                            ],
                            const SizedBox(height: 8),
                          ],
                        ),
                ),
                SafeArea(
                  top: false,
                  minimum: const EdgeInsets.only(bottom: 8),
                  child: Padding(
                    padding: const EdgeInsets.fromLTRB(16, 8, 16, 0),
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        if (_showNewMeetingFields) ...[
                          ElevatedButton(
                            onPressed: _isSaving ? null : _selectNewMeetingDate,
                            child: Text(
                              _newMeetingDate == null
                                  ? 'Odaberi datum i vrijeme'
                                  : 'Odabrano: ${DateFormat('dd.MM.yyyy. – HH:mm').format(_newMeetingDate!)}',
                            ),
                          ),
                          const SizedBox(height: 8),
                          if (_newMeetingDate != null) ...[
                            ElevatedButton(
                              onPressed: _isSaving ? null : _addMeeting,
                              child: _isSaving
                                  ? const SizedBox(
                                      width: 20,
                                      height: 20,
                                      child: CircularProgressIndicator(
                                        strokeWidth: 2,
                                      ),
                                    )
                                  : const Text('Spasi sastanak'),
                            ),
                            const SizedBox(height: 8),
                          ],
                        ],
                        ElevatedButton(
                          onPressed: _isSaving
                              ? null
                              : () {
                                  setState(() {
                                    _showNewMeetingFields =
                                        !_showNewMeetingFields;
                                    _newMeetingDate = null;
                                  });
                                },
                          child: Text(
                            _showNewMeetingFields
                                ? 'Otkaži dodavanje'
                                : 'Zakaži novi sastanak',
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
    );
  }
}
