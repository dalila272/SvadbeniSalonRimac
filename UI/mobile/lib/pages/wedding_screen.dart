import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:intl/intl.dart';
import 'package:svadbeni_salon_rimac/models/package.dart';
import 'package:svadbeni_salon_rimac/models/wedding.dart';
import 'package:svadbeni_salon_rimac/providers/packages_provider.dart';
import 'package:svadbeni_salon_rimac/providers/wedding_provider.dart';
import 'package:svadbeni_salon_rimac/utils/master_screen.dart';
import 'package:svadbeni_salon_rimac/widgets/availability_calendar.dart';

class WeddingScreen extends StatefulWidget {
  const WeddingScreen({super.key});

  @override
  State<WeddingScreen> createState() => _WeddingScreenState();
}

class _WeddingScreenState extends State<WeddingScreen> {
  final _formKey = GlobalKey<FormState>();
  final _packagesProvider = PackagesProvider();
  final _weddingProvider = WeddingProvider();

  List<Package> _packages = [];
  Wedding? _existingWedding;
  int? _editingWeddingId;
  int? _selectedOfferId;
  DateTime? _selectedDate;
  DateTime? _ownWeddingDate;
  DateTime _calendarFocusedDay = DateTime.now();
  final Set<String> _occupiedDateKeys = {};
  int _expectedGuests = 0;
  TimeOfDay _selectedTime = const TimeOfDay(hour: 18, minute: 0);
  int _installments = 1;
  bool _isLoading = true;
  bool _isSaving = false;
  String _errorMessage = '';

  String _dateKey(DateTime d) =>
      '${d.year}-${d.month.toString().padLeft(2, '0')}-${d.day.toString().padLeft(2, '0')}';

  bool _isDateAvailable(DateTime day) {
    final key = _dateKey(day);
    if (_ownWeddingDate != null && _dateKey(_ownWeddingDate!) == key) {
      return true;
    }
    return !_occupiedDateKeys.contains(key);
  }

  @override
  void initState() {
    super.initState();
    _loadData();
  }

  Future<void> _loadData() async {
    setState(() {
      _isLoading = true;
      _errorMessage = '';
    });

    try {
      final packagesResult = await _packagesProvider.get(filter: {
        'pageSize': 100,
        'includeTotalCount': true,
        'isActive': true,
      });
      final pendingFuture = _weddingProvider.get(filter: {
        'status': 0,
        'pageSize': 1,
      });
      final confirmedFuture = _weddingProvider.get(filter: {
        'status': 1,
        'pageSize': 1,
      });
      final occupiedFuture = _weddingProvider.getOccupiedDates();

      final pending = await pendingFuture;
      final confirmed = await confirmedFuture;
      final occupied = await occupiedFuture;

      Wedding? active;
      if (pending.items.isNotEmpty) {
        active = pending.items.first;
      } else if (confirmed.items.isNotEmpty) {
        active = confirmed.items.first;
      }

      setState(() {
        _packages = packagesResult.items;
        _existingWedding = active;
        _editingWeddingId = null;
        _ownWeddingDate = null;
        _occupiedDateKeys
          ..clear()
          ..addAll(occupied.map(_dateKey));
        _isLoading = false;
      });
    } catch (e) {
      setState(() {
        _isLoading = false;
        _errorMessage = e.toString().replaceFirst('Exception: ', '');
      });
    }
  }

  String _formatTimeForApi(TimeOfDay time) {
    return '${time.hour.toString().padLeft(2, '0')}:${time.minute.toString().padLeft(2, '0')}:00';
  }

  TimeOfDay _parseTime(String time) {
    final parts = time.split(':');
    return TimeOfDay(
      hour: int.parse(parts[0]),
      minute: int.parse(parts[1]),
    );
  }

  Future<void> _saveWedding() async {
    if (!_formKey.currentState!.validate()) return;
    _formKey.currentState!.save();

    if (_selectedOfferId == null || _selectedDate == null) {
      setState(() => _errorMessage = 'Odaberite paket i datum svadbe.');
      return;
    }

    if (!_isDateAvailable(_selectedDate!)) {
      setState(() => _errorMessage = 'Odabrani datum je zauzet. Odaberite slobodan (zeleni) dan.');
      return;
    }

    setState(() {
      _isSaving = true;
      _errorMessage = '';
    });

    try {
      final request = {
        'ponudaId': _selectedOfferId,
        'datumSvadbe': DateTime(
          _selectedDate!.year,
          _selectedDate!.month,
          _selectedDate!.day,
        ).toIso8601String(),
        'vrijeme': _formatTimeForApi(_selectedTime),
        'brojGostiju': _expectedGuests,
        'brojRata': _installments,
      };

      if (_editingWeddingId != null) {
        await _weddingProvider.update(_editingWeddingId!, request);
      } else {
        await _weddingProvider.insert(request);
      }

      await _loadData();
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Svadba uspješno sačuvana!')),
        );
      }
    } catch (e) {
      setState(() {
        _errorMessage = e.toString().replaceFirst('Exception: ', '');
      });
    } finally {
      if (mounted) setState(() => _isSaving = false);
    }
  }

  Future<void> _cancelWedding() async {
    if (_existingWedding == null) return;

    final reasonController = TextEditingController();
    final formKey = GlobalKey<FormState>();
    final razlog = await showDialog<String>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Row(
          children: [
            const Expanded(child: Text('Otkazivanje')),
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
              const Text(
                'Otkazati rezervaciju? Unesite razlog (obavezno).',
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
            child: const Text('Ne'),
          ),
          ElevatedButton(
            onPressed: () {
              if (!(formKey.currentState?.validate() ?? false)) return;
              Navigator.pop(ctx, reasonController.text.trim());
            },
            child: const Text('Otkaži rezervaciju'),
          ),
        ],
      ),
    );
    reasonController.dispose();
    if (razlog == null) return;

    setState(() => _isSaving = true);
    try {
      await _weddingProvider.cancel(_existingWedding!.id, razlog: razlog);
      await _loadData();
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Rezervacija je otkazana.')),
        );
      }
    } catch (e) {
      setState(() {
        _errorMessage = e.toString().replaceFirst('Exception: ', '');
      });
    } finally {
      if (mounted) setState(() => _isSaving = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreenWidget(
      title: 'Zakazivanje svadbe',
      child: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : SingleChildScrollView(
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: _existingWedding != null && _existingWedding!.isActive
                    ? _buildWeddingDetails()
                    : _buildWeddingForm(),
              ),
            ),
    );
  }

  Widget _buildWeddingForm() {
    return Form(
      key: _formKey,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          if (_errorMessage.isNotEmpty)
            Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: Text(_errorMessage, style: const TextStyle(color: Colors.red)),
            ),
          const Text(
            'Odaberite paket',
            style: TextStyle(
              fontWeight: FontWeight.bold,
              fontSize: 18,
              color: Color.fromRGBO(135, 82, 82, 0.8),
            ),
          ),
          DropdownButtonFormField<int>(
            initialValue: _selectedOfferId,
            decoration: const InputDecoration(),
            items: _packages
                .map((pkg) => DropdownMenuItem(
                      value: pkg.id,
                      child: Text('${pkg.title} (${pkg.price} KM)'),
                    ))
                .toList(),
            onChanged: (value) => setState(() => _selectedOfferId = value),
            validator: (value) =>
                value == null ? 'Molimo odaberite paket' : null,
          ),
          const SizedBox(height: 16),
          const Text(
            'Datum svadbe',
            style: TextStyle(
              fontWeight: FontWeight.bold,
              fontSize: 18,
              color: Color.fromRGBO(135, 82, 82, 0.8),
            ),
          ),
          const SizedBox(height: 4),
          Text(
            _selectedDate == null
                ? 'Zeleni dani su slobodni. Zauzete datume nije moguće odabrati.'
                : 'Odabrano: ${DateFormat('dd.MM.yyyy.').format(_selectedDate!)}',
            style: TextStyle(color: Colors.grey[700], fontSize: 13),
          ),
          const SizedBox(height: 8),
          AvailabilityCalendar(
            focusedDay: _calendarFocusedDay,
            selectedDay: _selectedDate,
            firstDay: DateTime.now(),
            lastDay: DateTime(DateTime.now().year + 2, 12, 31),
            isDayAvailable: _isDateAvailable,
            onDaySelected: (day) {
              setState(() {
                _selectedDate = day;
                _calendarFocusedDay = day;
              });
            },
            onPageChanged: (focused) {
              setState(() => _calendarFocusedDay = focused);
            },
            freeLabel: 'Slobodan datum',
            busyLabel: 'Zauzet datum',
          ),
          const SizedBox(height: 16),
          const Text(
            'Broj očekivanih gostiju',
            style: TextStyle(
              fontWeight: FontWeight.bold,
              fontSize: 18,
              color: Color.fromRGBO(135, 82, 82, 0.8),
            ),
          ),
          TextFormField(
            decoration: const InputDecoration(hintText: 'Unesite broj gostiju'),
            keyboardType: TextInputType.number,
            inputFormatters: [FilteringTextInputFormatter.digitsOnly],
            onSaved: (value) =>
                _expectedGuests = int.tryParse(value ?? '0') ?? 0,
            validator: (value) =>
                value == null || value.isEmpty ? 'Unesite broj gostiju' : null,
          ),
          const SizedBox(height: 16),
          const Text(
            'Vrijeme',
            style: TextStyle(
              fontWeight: FontWeight.bold,
              fontSize: 18,
              color: Color.fromRGBO(135, 82, 82, 0.8),
            ),
          ),
          ListTile(
            contentPadding: EdgeInsets.zero,
            title: Text('Vrijeme: ${_selectedTime.format(context)}'),
            trailing: const Icon(Icons.access_time),
            onTap: () async {
              final picked = await showTimePicker(
                context: context,
                initialTime: _selectedTime,
              );
              if (picked != null) {
                if (picked.hour >= 14 && picked.hour <= 21) {
                  setState(() => _selectedTime = picked);
                } else if (mounted) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(
                      content: Text('Molimo odaberite vrijeme između 14:00 i 21:00'),
                    ),
                  );
                }
              }
            },
          ),
          const SizedBox(height: 16),
          const Text(
            'Broj rata',
            style: TextStyle(
              fontWeight: FontWeight.bold,
              fontSize: 18,
              color: Color.fromRGBO(135, 82, 82, 0.8),
            ),
          ),
          TextFormField(
            decoration: const InputDecoration(hintText: 'Unesite broj rata'),
            keyboardType: TextInputType.number,
            initialValue: '1',
            inputFormatters: [
              FilteringTextInputFormatter.digitsOnly,
              LengthLimitingTextInputFormatter(2),
            ],
            onSaved: (value) => _installments = int.tryParse(value ?? '1') ?? 1,
            validator: (value) {
              final installments = int.tryParse(value ?? '1');
              if (installments == null ||
                  installments <= 0 ||
                  installments > 12) {
                return 'Broj rata mora biti između 1 i 12';
              }
              return null;
            },
          ),
          const SizedBox(height: 24),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: _isSaving ? null : _saveWedding,
              child: _isSaving
                  ? const SizedBox(
                      width: 24,
                      height: 24,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Text('Rezerviši'),
            ),
          ),
        ],
      ),
    );
  }

  Widget _detailRow(IconData icon, String label, String value) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 10),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icon, size: 20, color: const Color.fromRGBO(135, 82, 82, 0.9)),
          const SizedBox(width: 10),
          SizedBox(
            width: 110,
            child: Text(
              label,
              style: TextStyle(color: Colors.grey[700], fontSize: 15),
            ),
          ),
          Expanded(
            child: Text(
              value,
              style: const TextStyle(
                fontSize: 16,
                fontWeight: FontWeight.w600,
                color: Color.fromRGBO(135, 82, 82, 0.95),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildWeddingDetails() {
    final wedding = _existingWedding!;
    final time = _parseTime(wedding.time);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        if (_errorMessage.isNotEmpty)
          Padding(
            padding: const EdgeInsets.only(bottom: 12),
            child: Text(_errorMessage, style: const TextStyle(color: Colors.red)),
          ),
        _detailRow(Icons.card_giftcard, 'Paket', wedding.offerName),
        _detailRow(
          Icons.calendar_today,
          'Datum',
          DateFormat('dd.MM.yyyy.').format(wedding.weddingDate),
        ),
        _detailRow(Icons.schedule, 'Vrijeme', time.format(context)),
        _detailRow(Icons.groups, 'Gosti', '${wedding.guestCount}'),
        _detailRow(Icons.payments_outlined, 'Rate', '${wedding.installmentCount}'),
        _detailRow(Icons.account_balance_wallet_outlined, 'Uplata', wedding.paymentLabel),
        _detailRow(Icons.info_outline, 'Status', wedding.statusLabel),
        const SizedBox(height: 16),
        if (wedding.status == 0) ...[
          ElevatedButton(
            style: ElevatedButton.styleFrom(
              backgroundColor: const Color.fromRGBO(135, 82, 82, 0.8),
            ),
            onPressed: _isSaving
                ? null
                : () {
                    setState(() {
                      _selectedOfferId = wedding.offerId;
                      _selectedDate = wedding.weddingDate;
                      _ownWeddingDate = DateTime(
                        wedding.weddingDate.year,
                        wedding.weddingDate.month,
                        wedding.weddingDate.day,
                      );
                      _calendarFocusedDay = _ownWeddingDate!;
                      _expectedGuests = wedding.guestCount;
                      _selectedTime = time;
                      _installments = wedding.installmentCount;
                      _editingWeddingId = wedding.id;
                      _existingWedding = null;
                    });
                  },
            child: const Text('Promijeni rezervaciju',
                style: TextStyle(color: Colors.white)),
          ),
          const SizedBox(height: 12),
          OutlinedButton(
            onPressed: _isSaving ? null : _cancelWedding,
            child: const Text('Otkaži rezervaciju'),
          ),
        ],
      ],
    );
  }
}
