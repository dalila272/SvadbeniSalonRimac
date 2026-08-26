import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:intl/intl.dart';
import 'package:open_file/open_file.dart';
import 'package:path_provider/path_provider.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_desktop/models/offer.dart';
import 'package:svadbeni_salon_desktop/models/user.dart';
import 'package:svadbeni_salon_desktop/models/wedding.dart';
import 'package:svadbeni_salon_desktop/models/wedding_invoice.dart';
import 'package:svadbeni_salon_desktop/models/wedding_payment.dart';
import 'package:svadbeni_salon_desktop/providers/offer_provider.dart';
import 'package:svadbeni_salon_desktop/providers/user_provider.dart';
import 'package:svadbeni_salon_desktop/providers/wedding_invoice_provider.dart';
import 'package:svadbeni_salon_desktop/providers/wedding_payment_provider.dart';
import 'package:svadbeni_salon_desktop/providers/wedding_provider.dart';
import 'package:svadbeni_salon_desktop/utils/api_client_exception.dart';
import 'package:svadbeni_salon_desktop/utils/form_ux.dart';
import 'package:svadbeni_salon_desktop/utils/utils_widgets.dart';
import 'package:svadbeni_salon_desktop/constants/role_names.dart';

class WeddingDetailsScreen extends StatefulWidget {
  final Wedding? wedding;
  final DateTime initialDate;

  const WeddingDetailsScreen({
    super.key,
    this.wedding,
    required this.initialDate,
  });

  @override
  State<WeddingDetailsScreen> createState() => _WeddingDetailsScreenState();
}

class _WeddingDetailsScreenState extends State<WeddingDetailsScreen> {
  static const _brandColor = Color(0xFF875252);
  static const _fieldDecoration = InputDecoration(
    border: OutlineInputBorder(),
    contentPadding: EdgeInsets.symmetric(horizontal: 12, vertical: 14),
  );

  final _formKey = GlobalKey<FormState>();
  final _currencyFormat = NumberFormat('#,##0.00');

  late WeddingProvider _weddingProvider;
  late OfferProvider _offerProvider;
  late UserProvider _userProvider;
  late WeddingPaymentProvider _paymentProvider;
  late WeddingInvoiceProvider _invoiceProvider;

  Wedding? _wedding;
  List<Offer> _offers = [];
  List<User> _customers = [];
  List<WeddingPayment> _payments = [];
  List<WeddingInvoice> _invoices = [];

  int? _selectedCustomerId;
  int? _selectedOfferId;
  late DateTime _selectedDate;
  TimeOfDay _selectedTime = const TimeOfDay(hour: 18, minute: 0);
  final _guestController = TextEditingController(text: '50');
  final _installmentController = TextEditingController(text: '1');

  double _offerPrice = 0;
  double _paidAmount = 0;

  bool _isLoading = true;
  bool _isSaving = false;
  String? _errorMessage;

  bool get _isEditing => _wedding != null;
  bool get _canEditFields => !_isEditing || (_wedding!.status == 0);
  bool get _isFullyPaid =>
      _wedding?.isFullyPaid == true ||
      (_offerPrice > 0 && _paidAmount >= _offerPrice);
  bool get _canAddPayment =>
      _isEditing &&
      !_isFullyPaid &&
      _wedding!.status != 2 &&
      _wedding!.status != 3;

  @override
  void initState() {
    super.initState();
    _wedding = widget.wedding;
    _weddingProvider = context.read<WeddingProvider>();
    _offerProvider = context.read<OfferProvider>();
    _userProvider = context.read<UserProvider>();
    _paymentProvider = context.read<WeddingPaymentProvider>();
    _invoiceProvider = context.read<WeddingInvoiceProvider>();
    _selectedDate = DateTime(
      widget.initialDate.year,
      widget.initialDate.month,
      widget.initialDate.day,
    );
    _loadForm();
  }

  @override
  void dispose() {
    _guestController.dispose();
    _installmentController.dispose();
    super.dispose();
  }

  Future<void> _loadForm() async {
    setState(() => _isLoading = true);
    try {
      final offersFuture =
          _offerProvider.get(filter: {'isActive': true, 'pageSize': 100});
      final customersFuture = _userProvider.get(filter: {
        'roleName': RoleNames.customer,
        'isActive': true,
        'pageSize': 100,
      });
      final offersResult = await offersFuture;
      final customersResult = await customersFuture;

      Wedding? detail = _wedding;
      if (_wedding != null) {
        detail = await _weddingProvider.getById(_wedding!.id);
        final paymentsFuture = _paymentProvider.get(filter: {
          'svadbaId': _wedding!.id,
          'pageSize': 100,
        });
        final invoicesFuture = _invoiceProvider.get(filter: {
          'svadbaId': _wedding!.id,
          'pageSize': 100,
        });
        final paymentsResult = await paymentsFuture;
        final invoicesResult = await invoicesFuture;
        _payments = paymentsResult.items ?? [];
        _invoices = invoicesResult.items ?? [];
      }

      if (!mounted) return;

      setState(() {
        _offers = offersResult.items ?? [];
        _customers = customersResult.items ?? [];
        _wedding = detail;

        if (detail != null) {
          _selectedCustomerId = detail.userId;
          _selectedOfferId = detail.offerId;
          _selectedDate = DateTime(
            detail.weddingDate.year,
            detail.weddingDate.month,
            detail.weddingDate.day,
          );
          _selectedTime = _parseTime(detail.time);
          _guestController.text = detail.guestCount.toString();
          _installmentController.text = detail.installmentCount.toString();
          _offerPrice = detail.offerPrice;
          _paidAmount = detail.paidAmount;
        } else {
          _selectedOfferId = _offers.isNotEmpty ? _offers.first.id : null;
          _updateOfferPrice();
        }

        if (_selectedOfferId != null &&
            !_offers.any((o) => o.id == _selectedOfferId)) {
          _selectedOfferId = _offers.isNotEmpty ? _offers.first.id : null;
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

  void _updateOfferPrice() {
    final offer = _offers.where((o) => o.id == _selectedOfferId).firstOrNull;
    _offerPrice = double.tryParse(offer?.price ?? '0') ?? 0;
  }

  TimeOfDay _parseTime(String time) {
    final parts = time.split(':');
    return TimeOfDay(
      hour: int.parse(parts[0]),
      minute: int.parse(parts[1]),
    );
  }

  String _formatTimeForApi(TimeOfDay time) {
    return '${time.hour.toString().padLeft(2, '0')}:${time.minute.toString().padLeft(2, '0')}:00';
  }

  Map<String, dynamic> _buildRequest() {
    return {
      'ponudaId': _selectedOfferId,
      'datumSvadbe': DateTime(
        _selectedDate.year,
        _selectedDate.month,
        _selectedDate.day,
      ).toIso8601String(),
      'vrijeme': _formatTimeForApi(_selectedTime),
      'brojGostiju': int.parse(_guestController.text),
      'brojRata': int.parse(_installmentController.text),
    };
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;

    if (!_isEditing && _selectedCustomerId == null) {
      setState(() => _errorMessage = 'Odaberite klijenta.');
      return;
    }
    if (_selectedOfferId == null) {
      setState(() => _errorMessage = 'Odaberite paket.');
      return;
    }

    setState(() {
      _isSaving = true;
      _errorMessage = null;
    });

    try {
      if (_isEditing) {
        await _weddingProvider.update(_wedding!.id, _buildRequest());
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('Svadba sačuvana.')),
          );
          await _loadForm();
        }
      } else {
        final request = _buildRequest()..['userId'] = _selectedCustomerId;
        await _weddingProvider.insert(request);
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
    if (_wedding == null) return;

    String? razlog;
    if (status == 2) {
      razlog = await FormUx.confirmWithReason(
        context,
        title: 'Otkazivanje rezervacije',
        message: _paidAmount > 0 && _wedding!.status == 1
            ? 'Rezervacija ima uplate. U razlogu navedite tok povrata '
                '(npr. „povrat uplate dogovoren s klijentom“).'
            : 'Otkazati rezervaciju? Ova akcija je nepovratna — unesite razlog.',
      );
      if (razlog == null) return;
    } else {
      final labels = {
        1: ('Potvrda rezervacije', 'Potvrditi ovu rezervaciju svadbe?'),
        3: ('Završetak', 'Označiti svadbu kao završenu?'),
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

    setState(() {
      _isSaving = true;
      _errorMessage = null;
    });

    try {
      await _weddingProvider.changeStatus(
        _wedding!.id,
        status,
        razlog: razlog,
      );
      if (mounted) Navigator.pop(context, 'reload');
    } on ApiClientException catch (e) {
      setState(() => _errorMessage = e.message);
    } catch (e) {
      setState(() => _errorMessage = e.toString());
    } finally {
      if (mounted) setState(() => _isSaving = false);
    }
  }

  Future<void> _pickDate() async {
    if (!_canEditFields) return;
    final picked = await showDatePicker(
      context: context,
      initialDate: _selectedDate,
      firstDate: DateTime.now(),
      lastDate: DateTime(DateTime.now().year + 2),
    );
    if (picked != null) setState(() => _selectedDate = picked);
  }

  Future<void> _pickTime() async {
    if (!_canEditFields) return;
    final picked = await showTimePicker(
      context: context,
      initialTime: _selectedTime,
    );
    if (picked == null) return;
    if (picked.hour < 14 || picked.hour > 21) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Vrijeme mora biti između 14:00 i 21:00'),
          ),
        );
      }
      return;
    }
    setState(() => _selectedTime = picked);
  }

  Future<void> _reloadPayments() async {
    if (_wedding == null) return;
    try {
      final detailFuture = _weddingProvider.getById(_wedding!.id);
      final paymentsFuture = _paymentProvider.get(filter: {
        'svadbaId': _wedding!.id,
        'pageSize': 100,
      });
      final invoicesFuture = _invoiceProvider.get(filter: {
        'svadbaId': _wedding!.id,
        'pageSize': 100,
      });
      final detail = await detailFuture;
      final paymentsResult = await paymentsFuture;
      final invoicesResult = await invoicesFuture;
      if (!mounted) return;
      setState(() {
        _wedding = detail;
        _payments = paymentsResult.items ?? [];
        _invoices = invoicesResult.items ?? [];
        _paidAmount = detail.paidAmount;
        _offerPrice = detail.offerPrice;
      });
    } catch (e) {
      if (mounted) alertBox(context, 'Greška', e.toString());
    }
  }

  Future<void> _addPayment() async {
    if (_wedding == null) return;

    final payment = await showDialog<({double amount, DateTime date})>(
      context: context,
      builder: (_) => const _AddPaymentDialog(),
    );

    if (payment == null || !mounted) return;

    try {
      final d = payment.date;
      await _paymentProvider.addPayment({
        'svadbaId': _wedding!.id,
        'iznos': payment.amount,
        'datumUplate':
            '${d.year}-${d.month.toString().padLeft(2, '0')}-${d.day.toString().padLeft(2, '0')}',
      });

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Uplata dodana.')),
      );
      await _reloadPayments();
    } on ApiClientException catch (e) {
      if (mounted) alertBox(context, 'Greška', e.message);
    } catch (e) {
      if (mounted) alertBox(context, 'Greška', e.toString());
    }
  }

  Future<void> _generateInvoice() async {
    if (_wedding == null) return;

    if (_payments.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Prije izdavanja računa evidentirajte barem jednu uplatu.'),
        ),
      );
      return;
    }

    setState(() => _isSaving = true);
    try {
      final invoice = await _invoiceProvider.createForWedding(_wedding!.id);
      final pdfBytes = await _invoiceProvider.downloadPdf(invoice.id);
      final file = await _savePdf(invoice.invoiceNumber, pdfBytes);
      if (!mounted) return;

      setState(() => _invoices = [invoice, ..._invoices]);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Račun ${invoice.invoiceNumber} generisan.')),
      );

      if (file != null) {
        await OpenFile.open(file.path);
      }
    } on ApiClientException catch (e) {
      if (mounted) alertBox(context, 'Greška', e.message);
    } catch (e) {
      if (mounted) alertBox(context, 'Greška', e.toString());
    } finally {
      if (mounted) setState(() => _isSaving = false);
    }
  }

  Future<void> _openInvoicePdf(WeddingInvoice invoice) async {
    setState(() => _isSaving = true);
    try {
      final pdfBytes = await _invoiceProvider.downloadPdf(invoice.id);
      final file = await _savePdf(invoice.invoiceNumber, pdfBytes);
      if (file != null) {
        await OpenFile.open(file.path);
      }
    } on ApiClientException catch (e) {
      if (mounted) alertBox(context, 'Greška', e.message);
    } catch (e) {
      if (mounted) alertBox(context, 'Greška', e.toString());
    } finally {
      if (mounted) setState(() => _isSaving = false);
    }
  }

  Future<File?> _savePdf(String invoiceNumber, List<int> bytes) async {
    final dir = await getApplicationDocumentsDirectory();
    final invoicesDir = Directory('${dir.path}/racuni');
    if (!await invoicesDir.exists()) {
      await invoicesDir.create(recursive: true);
    }
    final safeName = invoiceNumber.replaceAll('/', '-');
    final file = File('${invoicesDir.path}/$safeName.pdf');
    await file.writeAsBytes(bytes);
    return file;
  }

  String _customerLabel(User user) {
    final name = '${user.firstName ?? ''} ${user.lastName ?? ''}'.trim();
    if (name.isNotEmpty && user.username != null) {
      return '$name (${user.username})';
    }
    return user.username ?? user.email ?? 'Korisnik #${user.id}';
  }

  int? get _validOfferId {
    if (_selectedOfferId == null) return null;
    return _offers.any((o) => o.id == _selectedOfferId)
        ? _selectedOfferId
        : null;
  }

  int? get _validCustomerId {
    if (_selectedCustomerId == null) return null;
    return _customers.any((c) => c.id == _selectedCustomerId)
        ? _selectedCustomerId
        : null;
  }

  @override
  Widget build(BuildContext context) {
    return PopScope(
      canPop: true,
      child: Scaffold(
        appBar: AppBar(
          title: Text(_isEditing ? 'Uredi svadbu' : 'Nova svadba'),
          centerTitle: true,
          leading: BackButton(
            onPressed: () => Navigator.of(context).pop(),
          ),
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
                    constraints: const BoxConstraints(maxWidth: 640),
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
                            DropdownButtonFormField<int>(
                              decoration: _fieldDecoration.copyWith(
                                labelText: 'Klijent',
                              ),
                              value: _validCustomerId,
                              items: _customers
                                  .map(
                                    (c) => DropdownMenuItem(
                                      value: c.id,
                                      child: Text(_customerLabel(c)),
                                    ),
                                  )
                                  .toList(),
                              onChanged: null,
                            )
                          else
                            DropdownButtonFormField<int>(
                              decoration: _fieldDecoration.copyWith(
                                labelText: 'Klijent',
                              ),
                              value: _validCustomerId,
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
                          const SizedBox(height: 16),
                          DropdownButtonFormField<int>(
                            decoration: _fieldDecoration.copyWith(
                              labelText: 'Paket',
                            ),
                            value: _validOfferId,
                            items: _offers
                                .map(
                                  (o) => DropdownMenuItem(
                                    value: o.id,
                                    child: Text('${o.name} (${o.price} KM)'),
                                  ),
                                )
                                .toList(),
                            onChanged: _canEditFields
                                ? (v) => setState(() {
                                      _selectedOfferId = v;
                                      _updateOfferPrice();
                                    })
                                : null,
                            validator: (v) =>
                                v == null ? 'Odaberite paket' : null,
                          ),
                          const SizedBox(height: 16),
                          Row(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Expanded(
                                child: TextFormField(
                                  controller: _guestController,
                                  enabled: _canEditFields,
                                  decoration: _fieldDecoration.copyWith(
                                    labelText: 'Broj očekivanih gostiju',
                                  ),
                                  keyboardType: TextInputType.number,
                                  inputFormatters: [
                                    FilteringTextInputFormatter.digitsOnly,
                                  ],
                                  validator: (v) {
                                    final n = int.tryParse(v ?? '');
                                    if (n == null || n <= 0) {
                                      return 'Unesite broj gostiju';
                                    }
                                    return null;
                                  },
                                ),
                              ),
                              const SizedBox(width: 12),
                              Expanded(
                                child: InputDecorator(
                                  decoration: _fieldDecoration.copyWith(
                                    labelText: 'Vrijeme',
                                  ),
                                  child: InkWell(
                                    onTap: _pickTime,
                                    child: Row(
                                      children: [
                                        Expanded(
                                          child: Text(
                                            _selectedTime.format(context),
                                          ),
                                        ),
                                        const Icon(Icons.access_time, size: 20),
                                      ],
                                    ),
                                  ),
                                ),
                              ),
                            ],
                          ),
                          const SizedBox(height: 16),
                          Row(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Expanded(
                                child: TextFormField(
                                  controller: _installmentController,
                                  enabled: _canEditFields,
                                  decoration: _fieldDecoration.copyWith(
                                    labelText: 'Broj rata',
                                  ),
                                  keyboardType: TextInputType.number,
                                  inputFormatters: [
                                    FilteringTextInputFormatter.digitsOnly,
                                    LengthLimitingTextInputFormatter(2),
                                  ],
                                  validator: (v) {
                                    final n = int.tryParse(v ?? '1');
                                    if (n == null || n < 1 || n > 12) {
                                      return 'Broj rata mora biti 1–12';
                                    }
                                    return null;
                                  },
                                ),
                              ),
                              const SizedBox(width: 12),
                              Expanded(
                                child: InputDecorator(
                                  decoration: _fieldDecoration.copyWith(
                                    labelText: 'Datum',
                                  ),
                                  child: InkWell(
                                    onTap: _canEditFields ? _pickDate : null,
                                    child: Row(
                                      children: [
                                        Expanded(
                                          child: Text(
                                            DateFormat('dd.MM.yyyy.')
                                                .format(_selectedDate),
                                          ),
                                        ),
                                        const Icon(
                                          Icons.calendar_today,
                                          size: 20,
                                        ),
                                      ],
                                    ),
                                  ),
                                ),
                              ),
                            ],
                          ),
                          if (_isEditing) ...[
                            const SizedBox(height: 24),
                            Row(
                              children: [
                                Expanded(
                                  child: _summaryTile(
                                    'Ukupno za uplatu',
                                    '${_currencyFormat.format(_offerPrice)} KM',
                                  ),
                                ),
                                const SizedBox(width: 16),
                                Expanded(
                                  child: _summaryTile(
                                    _isFullyPaid ? 'Status uplate' : 'Uplaćeni iznos',
                                    _isFullyPaid
                                        ? 'Uplaćeno'
                                        : '${_currencyFormat.format(_paidAmount)} KM',
                                    color: Colors.green.shade700,
                                  ),
                                ),
                              ],
                            ),
                            if (_isFullyPaid)
                              Padding(
                                padding: const EdgeInsets.only(top: 8),
                                child: Text(
                                  'Rezervacija je u potpunosti uplaćena.',
                                  style: TextStyle(
                                    color: Colors.green.shade800,
                                    fontWeight: FontWeight.w600,
                                  ),
                                ),
                              )
                            else if (_offerPrice > 0)
                              Padding(
                                padding: const EdgeInsets.only(top: 8),
                                child: Text(
                                  'Preostalo: ${_currencyFormat.format(_offerPrice - _paidAmount)} KM',
                                  style: TextStyle(color: Colors.grey.shade700),
                                ),
                              ),
                            const SizedBox(height: 16),
                            Text(
                              'Uplate',
                              style: Theme.of(context).textTheme.titleMedium,
                            ),
                            const SizedBox(height: 8),
                            if (_payments.isEmpty)
                              const Padding(
                                padding: EdgeInsets.symmetric(vertical: 8),
                                child: Text('Nema evidentiranih uplata.'),
                              )
                            else
                              ..._payments.map(
                                (p) => ListTile(
                                  contentPadding: EdgeInsets.zero,
                                  title: Text(
                                    '${_currencyFormat.format(p.amount)} KM',
                                    style: const TextStyle(
                                      fontWeight: FontWeight.w600,
                                    ),
                                  ),
                                  subtitle: Text(
                                    DateFormat('dd.MM.yyyy.')
                                        .format(p.paymentDate),
                                  ),
                                ),
                              ),
                            if (_invoices.isNotEmpty) ...[
                              const SizedBox(height: 16),
                              Text(
                                'Računi',
                                style: Theme.of(context).textTheme.titleMedium,
                              ),
                              const SizedBox(height: 8),
                              ..._invoices.map(
                                (inv) => ListTile(
                                  contentPadding: EdgeInsets.zero,
                                  title: Text(
                                    inv.invoiceNumber,
                                    style: const TextStyle(
                                      fontWeight: FontWeight.w600,
                                    ),
                                  ),
                                  subtitle: Text(
                                    '${DateFormat('dd.MM.yyyy.').format(inv.issueDate)} · '
                                    '${_currencyFormat.format(inv.paidAmount)} / '
                                    '${_currencyFormat.format(inv.totalAmount)} KM',
                                  ),
                                  trailing: IconButton(
                                    icon: const Icon(Icons.picture_as_pdf),
                                    tooltip: 'Otvori PDF',
                                    onPressed: _isSaving
                                        ? null
                                        : () => _openInvoicePdf(inv),
                                  ),
                                ),
                              ),
                            ],
                            if (_wedding!.status != 2) ...[
                              const SizedBox(height: 8),
                              Wrap(
                                spacing: 8,
                                runSpacing: 8,
                                children: [
                                  if (_wedding!.status == 0)
                                    OutlinedButton(
                                      onPressed: _isSaving
                                          ? null
                                          : () => _changeStatus(1),
                                      child: const Text('Potvrdi'),
                                    ),
                                  if (_wedding!.status == 0 ||
                                      _wedding!.status == 1)
                                    OutlinedButton(
                                      onPressed: _isSaving
                                          ? null
                                          : () => _changeStatus(2),
                                      child: const Text('Otkaži rezervaciju'),
                                    ),
                                  if (_wedding!.status == 1)
                                    OutlinedButton(
                                      onPressed: _isSaving
                                          ? null
                                          : () => _changeStatus(3),
                                      child: const Text('Završi'),
                                    ),
                                ],
                              ),
                            ],
                          ],
                          const SizedBox(height: 24),
                          Row(
                            children: [
                              if (_isEditing) ...[
                                if (_canAddPayment)
                                  OutlinedButton(
                                    onPressed: _isSaving ? null : _addPayment,
                                    child: const Text('Dodaj uplatu'),
                                  )
                                else if (_isFullyPaid)
                                  Padding(
                                    padding: const EdgeInsets.only(right: 8),
                                    child: Text(
                                      'Uplaćeno',
                                      style: TextStyle(
                                        color: Colors.green.shade800,
                                        fontWeight: FontWeight.w600,
                                      ),
                                    ),
                                  ),
                                if (_canAddPayment) const SizedBox(width: 8),
                                OutlinedButton(
                                  onPressed: _isSaving ? null : _generateInvoice,
                                  child: const Text('Račun'),
                                ),
                              ],
                              const Spacer(),
                              if (_canEditFields)
                                ElevatedButton(
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
                                )
                              else
                                const Tooltip(
                                  message:
                                      'Izmjena nije moguća — rezervacija više nije na čekanju.',
                                  child: Text(
                                    'Samo pregled (nije na čekanju)',
                                    style: TextStyle(color: Colors.grey),
                                  ),
                                ),
                            ],
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

  Widget _summaryTile(String label, String value, {Color? color}) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(label, style: TextStyle(color: Colors.grey.shade700)),
        const SizedBox(height: 4),
        Text(
          value,
          style: TextStyle(
            fontSize: 18,
            fontWeight: FontWeight.bold,
            color: color,
          ),
        ),
      ],
    );
  }
}

class _AddPaymentDialog extends StatefulWidget {
  const _AddPaymentDialog();

  @override
  State<_AddPaymentDialog> createState() => _AddPaymentDialogState();
}

class _AddPaymentDialogState extends State<_AddPaymentDialog> {
  final _amountController = TextEditingController();
  final _formKey = GlobalKey<FormState>();
  DateTime _selectedDate = DateTime.now();

  @override
  void dispose() {
    _amountController.dispose();
    super.dispose();
  }

  Future<void> _pickDate() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: _selectedDate,
      firstDate: DateTime(2020),
      lastDate: DateTime.now().add(const Duration(days: 1)),
    );
    if (picked != null) {
      setState(() => _selectedDate = picked);
    }
  }

  void _submit() {
    if (!_formKey.currentState!.validate()) return;

    final normalized = _amountController.text.replaceAll(',', '.').trim();
    final amount = double.parse(normalized);
    Navigator.pop(
      context,
      (amount: amount, date: _selectedDate),
    );
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Row(
        children: [
          const Expanded(child: Text('Dodaj uplatu')),
          IconButton(
            icon: const Icon(Icons.close),
            tooltip: 'Zatvori',
            onPressed: () => Navigator.pop(context),
          ),
        ],
      ),
      content: Form(
        key: _formKey,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            TextFormField(
              controller: _amountController,
              autofocus: true,
              decoration: const InputDecoration(
                labelText: 'Iznos (KM)',
                prefixIcon: Icon(Icons.payments_outlined),
                border: OutlineInputBorder(),
              ),
              keyboardType: const TextInputType.numberWithOptions(
                decimal: true,
              ),
              inputFormatters: [
                FilteringTextInputFormatter.allow(RegExp(r'[0-9.,]')),
              ],
              validator: (v) {
                final normalized = (v ?? '').replaceAll(',', '.').trim();
                final n = double.tryParse(normalized);
                if (n == null || n <= 0) return 'Unesite iznos';
                return null;
              },
            ),
            const SizedBox(height: 12),
            ListTile(
              contentPadding: EdgeInsets.zero,
              title: const Text('Datum uplate'),
              subtitle: Text(DateFormat('dd.MM.yyyy.').format(_selectedDate)),
              trailing: const Icon(Icons.calendar_today),
              onTap: _pickDate,
            ),
          ],
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context),
          child: const Text('Odustani'),
        ),
        ElevatedButton(
          onPressed: _submit,
          child: const Text('Dodaj'),
        ),
      ],
    );
  }
}
