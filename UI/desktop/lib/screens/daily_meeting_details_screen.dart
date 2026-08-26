import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_desktop/models/daily_meeting.dart';
import 'package:svadbeni_salon_desktop/models/user.dart';
import 'package:svadbeni_salon_desktop/providers/daily_meeting_provider.dart';
import 'package:svadbeni_salon_desktop/providers/user_provider.dart';
import 'package:svadbeni_salon_desktop/utils/api_client_exception.dart';
import 'package:svadbeni_salon_desktop/utils/daily_meeting_slots.dart';
import 'package:svadbeni_salon_desktop/utils/form_ux.dart';
import 'package:svadbeni_salon_desktop/utils/utils_widgets.dart';
import 'package:svadbeni_salon_desktop/constants/role_names.dart';

class DailyMeetingDetailsScreen extends StatefulWidget {
  final DailyMeeting? meeting;
  final DateTime initialDateTime;

  const DailyMeetingDetailsScreen({
    super.key,
    this.meeting,
    required this.initialDateTime,
  });

  @override
  State<DailyMeetingDetailsScreen> createState() =>
      _DailyMeetingDetailsScreenState();
}

class _DailyMeetingDetailsScreenState extends State<DailyMeetingDetailsScreen> {
  static const _brandColor = Color(0xFF875252);
  static const _fieldDecoration = InputDecoration(
    border: OutlineInputBorder(),
    contentPadding: EdgeInsets.symmetric(horizontal: 12, vertical: 14),
  );

  final _formKey = GlobalKey<FormState>();
  final _noteController = TextEditingController();
  final _contactNameController = TextEditingController();

  late DailyMeetingProvider _provider;
  late UserProvider _userProvider;

  DailyMeeting? _meeting;
  List<User> _customers = [];
  List<DateTime> _busySlots = [];

  int? _selectedCustomerId;
  bool _isGuestBooking = false;
  late DateTime _selectedDateTime;

  bool _isLoading = true;
  bool _isSaving = false;
  String? _errorMessage;

  bool get _isEditing => _meeting != null;
  bool get _canEditFields => !_isEditing || _meeting!.canEdit;

  @override
  void initState() {
    super.initState();
    _meeting = widget.meeting;
    _selectedDateTime = widget.initialDateTime;
    _provider = context.read<DailyMeetingProvider>();
    _userProvider = context.read<UserProvider>();
    _loadForm();
  }

  @override
  void dispose() {
    _noteController.dispose();
    _contactNameController.dispose();
    super.dispose();
  }

  Future<void> _loadForm() async {
    setState(() => _isLoading = true);
    try {
      final customersResult = await _userProvider.get(filter: {
        'roleName': RoleNames.customer,
        'isActive': true,
        'pageSize': 100,
      });
      _busySlots = await _provider.getBusySlots();

      DailyMeeting? detail = _meeting;
      if (_meeting != null) {
        detail = await _provider.getById(_meeting!.id);
      }

      if (!mounted) return;
      setState(() {
        _customers = customersResult.items ?? [];
        _meeting = detail;
        if (detail != null) {
          _selectedCustomerId = detail.userId;
          _isGuestBooking = detail.userId == null;
          _contactNameController.text = detail.contactName ?? '';
          _selectedDateTime = detail.meetingDate;
          _noteController.text = detail.note ?? '';
        }
        _isLoading = false;
      });
    } catch (e) {
      if (mounted) {
        setState(() => _isLoading = false);
        alertBox(context, 'Greška', e.toString());
      }
    }
  }

  bool _isSlotTaken(DateTime slot, {DateTime? excludeSlot}) {
    return DailyMeetingSlots.isSlotTaken(
      slot,
      _busySlots,
      excludeSlot: excludeSlot,
    );
  }

  Future<void> _pickDateTime() async {
    if (!_canEditFields) return;

    final pickedDate = await showDatePicker(
      context: context,
      initialDate: _selectedDateTime,
      firstDate: DateTime.now(),
      lastDate: DateTime(DateTime.now().year + 2),
    );
    if (pickedDate == null || !mounted) return;

    final pickedTime = await _pickTimeWithinRange();
    if (pickedTime == null) return;

    setState(() {
      _selectedDateTime = DateTime(
        pickedDate.year,
        pickedDate.month,
        pickedDate.day,
        pickedTime.hour,
        pickedTime.minute,
      );
    });
  }

  Future<TimeOfDay?> _pickTimeWithinRange() async {
    while (true) {
      final selectedTime = await showTimePicker(
        context: context,
        initialTime: TimeOfDay.fromDateTime(_selectedDateTime),
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

      if (mounted) {
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

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;

    if (!_isEditing) {
      if (_isGuestBooking) {
        final name = _contactNameController.text.trim();
        if (name.isEmpty) {
          setState(() => _errorMessage = 'Unesite ime gosta.');
          return;
        }
      } else if (_selectedCustomerId == null) {
        setState(() => _errorMessage = 'Odaberite klijenta.');
        return;
      }
    }

    final excludeSlot =
        _isEditing ? _meeting!.meetingDate : null;
    if (_isSlotTaken(_selectedDateTime, excludeSlot: excludeSlot)) {
      setState(
        () => _errorMessage =
            'Odabrani termin se preklapa s postojećim sastankom.',
      );
      return;
    }

    setState(() {
      _isSaving = true;
      _errorMessage = null;
    });

    try {
      final payload = <String, dynamic>{
        'datumSastanka': _selectedDateTime.toIso8601String(),
        'napomena': _noteController.text.trim().isEmpty
            ? null
            : _noteController.text.trim(),
      };

      if (_isEditing) {
        await _provider.update(_meeting!.id, payload);
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('Sastanak sačuvan.')),
          );
          Navigator.pop(context, 'reload');
        }
      } else {
        if (_isGuestBooking) {
          payload['kontaktIme'] = _contactNameController.text.trim();
        } else {
          payload['userId'] = _selectedCustomerId;
        }
        await _provider.insert(payload);
        if (mounted) Navigator.pop(context, 'reload');
      }
    } on ApiClientException catch (e) {
      setState(() => _errorMessage = e.message);
    } catch (e) {
      setState(() => _errorMessage = e.toString());
    } finally {
      if (mounted) setState(() => _isSaving = false);
    }
  }

  Future<void> _changeStatus(int status) async {
    if (_meeting == null) return;
    String? razlog;
    if (status == 2) {
      razlog = await FormUx.confirmWithReason(
        context,
        title: 'Otkazivanje sastanka',
        message: 'Otkazati sastanak? Unesite razlog (šalje se klijentu u notifikaciji).',
      );
      if (razlog == null) return;
    } else {
      final labels = {
        1: ('Potvrda sastanka', 'Potvrditi ovaj sastanak?'),
        3: ('Završetak', 'Označiti sastanak kao završen?'),
      };
      final info = labels[status];
      if (info != null) {
        final ok = await FormUx.confirm(
          context,
          title: info.$1,
          message: info.$2,
        );
        if (!ok) return;
      }
    }
    setState(() => _isSaving = true);
    try {
      await _provider.changeStatus(_meeting!.id, status, razlog: razlog);
      if (mounted) Navigator.pop(context, 'reload');
    } on ApiClientException catch (e) {
      setState(() => _errorMessage = e.message);
    } catch (e) {
      setState(() => _errorMessage = e.toString());
    } finally {
      if (mounted) setState(() => _isSaving = false);
    }
  }

  String _customerLabel(User user) {
    final name = '${user.firstName ?? ''} ${user.lastName ?? ''}'.trim();
    if (name.isNotEmpty && user.username != null) {
      return '$name (${user.username})';
    }
    return user.username ?? user.email ?? 'Korisnik #${user.id}';
  }

  @override
  Widget build(BuildContext context) {
    return PopScope(
      canPop: true,
      child: Scaffold(
        appBar: AppBar(
          title: Text(_isEditing ? 'Sastanak' : 'Novi sastanak'),
          centerTitle: true,
          leading: const BackButton(),
          actions: [
            IconButton(
              icon: const Icon(Icons.close),
              tooltip: 'Zatvori',
              onPressed: () => Navigator.of(context).pop(),
            ),
          ],
        ),
        body: _isLoading
            ? const Center(child: CircularProgressIndicator())
            : SingleChildScrollView(
                padding: const EdgeInsets.all(24),
                child: Align(
                  alignment: Alignment.topCenter,
                  child: ConstrainedBox(
                    constraints: const BoxConstraints(maxWidth: 520),
                    child: Form(
                      key: _formKey,
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: [
                          if (_errorMessage != null)
                            Padding(
                              padding: const EdgeInsets.only(bottom: 16),
                              child: Text(
                                _errorMessage!,
                                style: const TextStyle(color: Colors.red),
                              ),
                            ),
                          if (_isEditing)
                            InputDecorator(
                              decoration: _fieldDecoration.copyWith(
                                labelText: 'Klijent / gost',
                              ),
                              child: Text(_meeting!.displayName),
                            )
                          else ...[
                            SegmentedButton<bool>(
                              segments: const [
                                ButtonSegment(
                                  value: false,
                                  label: Text('Klijent'),
                                  icon: Icon(Icons.person),
                                ),
                                ButtonSegment(
                                  value: true,
                                  label: Text('Gost'),
                                  icon: Icon(Icons.person_outline),
                                ),
                              ],
                              selected: {_isGuestBooking},
                              onSelectionChanged: (selected) {
                                setState(() {
                                  _isGuestBooking = selected.first;
                                  _selectedCustomerId = null;
                                  _contactNameController.clear();
                                });
                              },
                            ),
                            const SizedBox(height: 16),
                            if (_isGuestBooking)
                              TextFormField(
                                controller: _contactNameController,
                                decoration: _fieldDecoration.copyWith(
                                  labelText: 'Ime gosta',
                                ),
                                validator: (v) => v == null || v.trim().isEmpty
                                    ? 'Unesite ime gosta'
                                    : null,
                              )
                            else
                              DropdownButtonFormField<int>(
                                decoration: _fieldDecoration.copyWith(
                                  labelText: 'Klijent iz aplikacije',
                                ),
                                initialValue: _selectedCustomerId,
                                items: _customers
                                    .map(
                                      (c) => DropdownMenuItem(
                                        value: c.id,
                                        child: Text(_customerLabel(c)),
                                      ),
                                    )
                                    .toList(),
                                onChanged: (v) =>
                                    setState(() => _selectedCustomerId = v),
                                validator: (v) =>
                                    v == null ? 'Odaberite klijenta' : null,
                              ),
                          ],
                          const SizedBox(height: 16),
                          InputDecorator(
                            decoration: _fieldDecoration.copyWith(
                              labelText: 'Datum i vrijeme',
                              helperText: 'Trajanje sastanka: 30 min',
                            ),
                            child: InkWell(
                              onTap: _canEditFields ? _pickDateTime : null,
                              child: Row(
                                children: [
                                  Expanded(
                                    child: Text(
                                      DateFormat('dd.MM.yyyy. HH:mm')
                                          .format(_selectedDateTime),
                                    ),
                                  ),
                                  const Icon(Icons.calendar_today, size: 20),
                                ],
                              ),
                            ),
                          ),
                          if (_isEditing) ...[
                            const SizedBox(height: 16),
                            InputDecorator(
                              decoration: _fieldDecoration.copyWith(
                                labelText: 'Status',
                              ),
                              child: Text(_meeting!.statusLabel),
                            ),
                          ],
                          const SizedBox(height: 16),
                          TextFormField(
                            controller: _noteController,
                            enabled: _canEditFields,
                            decoration: _fieldDecoration.copyWith(
                              labelText: 'Napomena',
                            ),
                            maxLines: 3,
                          ),
                          if (_isEditing && _meeting!.status != 2) ...[
                            const SizedBox(height: 16),
                            Wrap(
                              spacing: 8,
                              runSpacing: 8,
                              children: [
                                if (_meeting!.status == 0)
                                  OutlinedButton(
                                    onPressed: _isSaving
                                        ? null
                                        : () => _changeStatus(1),
                                    child: const Text('Potvrdi'),
                                  ),
                                if (_meeting!.status == 0 ||
                                    _meeting!.status == 1)
                                  OutlinedButton(
                                    onPressed: _isSaving
                                        ? null
                                        : () => _changeStatus(2),
                                    child: const Text('Otkaži'),
                                  ),
                                if (_meeting!.status == 1)
                                  OutlinedButton(
                                    onPressed: _isSaving
                                        ? null
                                        : () => _changeStatus(3),
                                    child: const Text('Završi'),
                                  ),
                              ],
                            ),
                          ],
                          const SizedBox(height: 24),
                          if (_canEditFields)
                            Align(
                              alignment: Alignment.centerRight,
                              child: ElevatedButton(
                                style: ElevatedButton.styleFrom(
                                  backgroundColor: _brandColor,
                                  foregroundColor: Colors.white,
                                ),
                                onPressed: _isSaving ? null : _save,
                                child: _isSaving
                                    ? const SizedBox(
                                        width: 24,
                                        height: 24,
                                        child: CircularProgressIndicator(
                                          strokeWidth: 2,
                                          color: Colors.white,
                                        ),
                                      )
                                    : const Text('Spremi'),
                              ),
                            ),
                        ],
                      ),
                    ),
                  ),
                ),
              ),
      ),
    );
  }
}
