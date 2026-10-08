import 'dart:io';
import 'dart:typed_data';

import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:open_file/open_file.dart';
import 'package:path_provider/path_provider.dart';
import 'package:printing/printing.dart';
import 'package:svadbeni_salon_desktop/layouts/master_screen.dart';
import 'package:svadbeni_salon_desktop/providers/report_provider.dart';
import 'package:svadbeni_salon_desktop/utils/api_client_exception.dart';
import 'package:svadbeni_salon_desktop/utils/utils_widgets.dart';

class ReportsScreen extends StatefulWidget {
  const ReportsScreen({super.key});

  @override
  State<ReportsScreen> createState() => _ReportsScreenState();
}

class _ReportsScreenState extends State<ReportsScreen> {
  static const _brandColor = Color(0xFF875252);
  final _reportProvider = ReportProvider();
  final _dateFormat = DateFormat('dd.MM.yyyy.');

  late DateTime _weddingsFrom;
  late DateTime _weddingsTo;
  int? _weddingStatus;

  late DateTime _paymentsFrom;
  late DateTime _paymentsTo;

  bool _loadingWeddings = false;
  bool _loadingPayments = false;
  bool _printingWeddings = false;
  bool _printingPayments = false;

  @override
  void initState() {
    super.initState();
    final now = DateTime.now();
    _weddingsFrom = DateTime(now.year, now.month, 1);
    _weddingsTo = DateTime(now.year, now.month + 1, 0);
    _paymentsFrom = _weddingsFrom;
    _paymentsTo = _weddingsTo;
  }

  Future<void> _pickDate({
    required DateTime initial,
    required ValueChanged<DateTime> onPicked,
  }) async {
    final picked = await showDatePicker(
      context: context,
      initialDate: initial,
      firstDate: DateTime(2020),
      lastDate: DateTime(DateTime.now().year + 3),
    );
    if (picked != null) onPicked(picked);
  }

  Future<File?> _savePdf(String fileName, List<int> bytes) async {
    final dir = await getApplicationDocumentsDirectory();
    final folder = Directory('${dir.path}/izvjestaji');
    if (!await folder.exists()) {
      await folder.create(recursive: true);
    }
    final file = File('${folder.path}/$fileName');
    await file.writeAsBytes(bytes);
    return file;
  }

  Future<void> _printPdf(List<int> bytes, String jobName) async {
    await Printing.layoutPdf(
      name: jobName,
      onLayout: (_) async => Uint8List.fromList(bytes),
    );
  }

  bool _validateRange(DateTime from, DateTime to) {
    if (to.isBefore(from)) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Datum do mora biti nakon datuma od.')),
      );
      return false;
    }
    return true;
  }

  Future<void> _downloadWeddingsReport() async {
    if (!_validateRange(_weddingsFrom, _weddingsTo)) return;

    setState(() => _loadingWeddings = true);
    try {
      final bytes = await _reportProvider.downloadWeddingsReport(
        dateFrom: _weddingsFrom,
        dateTo: _weddingsTo,
        status: _weddingStatus,
      );
      final name =
          'svadbe-${_fileDate(_weddingsFrom)}-${_fileDate(_weddingsTo)}.pdf';
      final file = await _savePdf(name, bytes);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Izvještaj svadbi preuzet.')),
      );
      if (file != null) await OpenFile.open(file.path);
    } on ApiClientException catch (e) {
      if (mounted) alertBox(context, 'Greška', e.message);
    } catch (e) {
      if (mounted) alertBox(context, 'Greška', e.toString());
    } finally {
      if (mounted) setState(() => _loadingWeddings = false);
    }
  }

  Future<void> _printWeddingsReport() async {
    if (!_validateRange(_weddingsFrom, _weddingsTo)) return;

    setState(() => _printingWeddings = true);
    try {
      final bytes = await _reportProvider.downloadWeddingsReport(
        dateFrom: _weddingsFrom,
        dateTo: _weddingsTo,
        status: _weddingStatus,
      );
      await _printPdf(bytes, 'Pregled svadbi');
    } on ApiClientException catch (e) {
      if (mounted) alertBox(context, 'Greška', e.message);
    } catch (e) {
      if (mounted) alertBox(context, 'Greška', e.toString());
    } finally {
      if (mounted) setState(() => _printingWeddings = false);
    }
  }

  Future<void> _downloadPaymentsReport() async {
    if (!_validateRange(_paymentsFrom, _paymentsTo)) return;

    setState(() => _loadingPayments = true);
    try {
      final bytes = await _reportProvider.downloadPaymentsReport(
        dateFrom: _paymentsFrom,
        dateTo: _paymentsTo,
      );
      final name =
          'uplate-${_fileDate(_paymentsFrom)}-${_fileDate(_paymentsTo)}.pdf';
      final file = await _savePdf(name, bytes);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Izvještaj uplata preuzet.')),
      );
      if (file != null) await OpenFile.open(file.path);
    } on ApiClientException catch (e) {
      if (mounted) alertBox(context, 'Greška', e.message);
    } catch (e) {
      if (mounted) alertBox(context, 'Greška', e.toString());
    } finally {
      if (mounted) setState(() => _loadingPayments = false);
    }
  }

  Future<void> _printPaymentsReport() async {
    if (!_validateRange(_paymentsFrom, _paymentsTo)) return;

    setState(() => _printingPayments = true);
    try {
      final bytes = await _reportProvider.downloadPaymentsReport(
        dateFrom: _paymentsFrom,
        dateTo: _paymentsTo,
      );
      await _printPdf(bytes, 'Pregled uplata');
    } on ApiClientException catch (e) {
      if (mounted) alertBox(context, 'Greška', e.message);
    } catch (e) {
      if (mounted) alertBox(context, 'Greška', e.toString());
    } finally {
      if (mounted) setState(() => _printingPayments = false);
    }
  }

  String _fileDate(DateTime d) =>
      '${d.year}${d.month.toString().padLeft(2, '0')}${d.day.toString().padLeft(2, '0')}';

  Widget _dateTile({
    required String label,
    required DateTime value,
    required VoidCallback onTap,
  }) {
    return Expanded(
      child: InputDecorator(
        decoration: InputDecoration(
          labelText: label,
          border: const OutlineInputBorder(),
          contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
        ),
        child: InkWell(
          onTap: onTap,
          child: Padding(
            padding: const EdgeInsets.symmetric(vertical: 10),
            child: Row(
              children: [
                Expanded(child: Text(_dateFormat.format(value))),
                const Icon(Icons.calendar_today, size: 18),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _actionButton({
    required bool loading,
    required VoidCallback onPressed,
    required IconData icon,
    required String label,
    bool outlined = false,
  }) {
    final Widget iconWidget = loading
        ? SizedBox(
            width: 18,
            height: 18,
            child: CircularProgressIndicator(
              strokeWidth: 2,
              color: outlined ? _brandColor : Colors.white,
            ),
          )
        : Icon(icon);

    if (outlined) {
      return OutlinedButton.icon(
        style: OutlinedButton.styleFrom(
          foregroundColor: _brandColor,
          side: const BorderSide(color: _brandColor),
        ),
        onPressed: loading ? null : onPressed,
        icon: iconWidget,
        label: Text(label),
      );
    }

    return ElevatedButton.icon(
      style: ElevatedButton.styleFrom(
        backgroundColor: _brandColor,
        foregroundColor: Colors.white,
      ),
      onPressed: loading ? null : onPressed,
      icon: iconWidget,
      label: Text(label),
    );
  }

  Widget _reportCard({
    required String title,
    required String description,
    required Widget filters,
    required bool downloading,
    required bool printing,
    required VoidCallback onDownload,
    required VoidCallback onPrint,
  }) {
    return Card(
      elevation: 2,
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(title, style: Theme.of(context).textTheme.titleLarge),
            const SizedBox(height: 8),
            Text(
              description,
              style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                    color: Colors.grey.shade700,
                  ),
            ),
            const SizedBox(height: 16),
            filters,
            const SizedBox(height: 16),
            Align(
              alignment: Alignment.centerRight,
              child: Wrap(
                spacing: 12,
                runSpacing: 8,
                children: [
                  _actionButton(
                    loading: downloading,
                    onPressed: onDownload,
                    icon: Icons.download,
                    label: 'Preuzmi PDF',
                  ),
                  _actionButton(
                    loading: printing,
                    onPressed: onPrint,
                    icon: Icons.print,
                    label: 'Print',
                    outlined: true,
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: 'Izvještaji',
      child: SingleChildScrollView(
        padding: const EdgeInsets.all(24),
        child: Align(
          alignment: Alignment.topCenter,
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 720),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                _reportCard(
                  title: 'Pregled svadbi u periodu',
                  description:
                      'Lista svadbi u odabranom datumu s klijentom, paketom, brojem gostiju i statusom.',
                  downloading: _loadingWeddings,
                  printing: _printingWeddings,
                  onDownload: _downloadWeddingsReport,
                  onPrint: _printWeddingsReport,
                  filters: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      Row(
                        children: [
                          _dateTile(
                            label: 'Datum od',
                            value: _weddingsFrom,
                            onTap: () => _pickDate(
                              initial: _weddingsFrom,
                              onPicked: (d) => setState(() => _weddingsFrom = d),
                            ),
                          ),
                          const SizedBox(width: 12),
                          _dateTile(
                            label: 'Datum do',
                            value: _weddingsTo,
                            onTap: () => _pickDate(
                              initial: _weddingsTo,
                              onPicked: (d) => setState(() => _weddingsTo = d),
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 12),
                      DropdownButtonFormField<int?>(
                        decoration: const InputDecoration(
                          labelText: 'Status',
                          border: OutlineInputBorder(),
                        ),
                        initialValue: _weddingStatus,
                        items: const [
                          DropdownMenuItem(value: null, child: Text('Svi statusi')),
                          DropdownMenuItem(value: 0, child: Text('Na čekanju')),
                          DropdownMenuItem(value: 1, child: Text('Potvrđene')),
                          DropdownMenuItem(value: 2, child: Text('Otkazane')),
                          DropdownMenuItem(value: 3, child: Text('Završene')),
                        ],
                        onChanged: (v) => setState(() => _weddingStatus = v),
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 20),
                _reportCard(
                  title: 'Pregled uplata u periodu',
                  description:
                      'Lista svih evidentiranih uplata po svadbama u odabranom periodu s ukupnim iznosom.',
                  downloading: _loadingPayments,
                  printing: _printingPayments,
                  onDownload: _downloadPaymentsReport,
                  onPrint: _printPaymentsReport,
                  filters: Row(
                    children: [
                      _dateTile(
                        label: 'Datum od',
                        value: _paymentsFrom,
                        onTap: () => _pickDate(
                          initial: _paymentsFrom,
                          onPicked: (d) => setState(() => _paymentsFrom = d),
                        ),
                      ),
                      const SizedBox(width: 12),
                      _dateTile(
                        label: 'Datum do',
                        value: _paymentsTo,
                        onTap: () => _pickDate(
                          initial: _paymentsTo,
                          onPicked: (d) => setState(() => _paymentsTo = d),
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
