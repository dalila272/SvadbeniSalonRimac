import 'dart:async';

import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_desktop/layouts/master_screen.dart';
import 'package:svadbeni_salon_desktop/models/salon_review.dart';
import 'package:svadbeni_salon_desktop/models/search_result.dart';
import 'package:svadbeni_salon_desktop/providers/salon_review_provider.dart';
import 'package:svadbeni_salon_desktop/utils/utils_widgets.dart';

class SalonReviewListScreen extends StatefulWidget {
  const SalonReviewListScreen({super.key});

  @override
  State<SalonReviewListScreen> createState() => _SalonReviewListScreenState();
}

class _SalonReviewListScreenState extends State<SalonReviewListScreen> {
  static const _brandColor = Color(0xFF875252);
  static const _debounceDuration = Duration(milliseconds: 350);

  late SalonReviewProvider _provider;
  SearchResult<SalonReview>? _result;
  bool _isLoading = true;
  int? _filterRating;
  final _clientController = TextEditingController();
  Timer? _searchDebounce;

  @override
  void initState() {
    super.initState();
    _provider = context.read<SalonReviewProvider>();
    _load();
  }

  @override
  void dispose() {
    _clientController.dispose();
    _searchDebounce?.cancel();
    super.dispose();
  }

  Future<void> _load() async {
    setState(() => _isLoading = true);
    try {
      final filter = <String, dynamic>{'pageSize': 100};
      if (_filterRating != null) filter['ocjena'] = _filterRating;

      final data = await _provider.get(filter: filter);
      var items = data.items ?? [];
      final q = _clientController.text.trim().toLowerCase();
      if (q.isNotEmpty) {
        items = items
            .where((r) => r.displayClientName.toLowerCase().contains(q))
            .toList();
      }

      if (!mounted) return;
      setState(() {
        _result = SearchResult<SalonReview>()
          ..items = items
          ..totalCount = items.length;
        _isLoading = false;
      });
    } on Exception catch (e) {
      if (mounted) {
        setState(() => _isLoading = false);
        alertBox(context, 'Greška', e.toString());
      }
    }
  }

  void _scheduleReload() {
    _searchDebounce?.cancel();
    _searchDebounce = Timer(_debounceDuration, _load);
  }

  double? get _averageRating {
    final items = _result?.items ?? [];
    if (items.isEmpty) return null;
    return items.map((r) => r.rating).reduce((a, b) => a + b) / items.length;
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: 'Recenzije',
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            if (!_isLoading && (_result?.items?.isNotEmpty ?? false))
              _buildSummary(),
            const SizedBox(height: 12),
            _buildFilters(),
            const SizedBox(height: 12),
            Expanded(
              child: _isLoading
                  ? const Center(child: CircularProgressIndicator())
                  : _buildTable(),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildSummary() {
    final count = _result?.items?.length ?? 0;
    final avg = _averageRating;

    return Card(
      elevation: 1,
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
        child: Row(
          children: [
            Icon(Icons.star, color: Colors.amber.shade700, size: 28),
            const SizedBox(width: 12),
            Text(
              count == 0
                  ? 'Nema recenzija'
                  : 'Prosječna ocjena: ${avg!.toStringAsFixed(1)} / 5',
              style: Theme.of(context).textTheme.titleMedium,
            ),
            const Spacer(),
            Text(
              '$count ukupno',
              style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                    color: Colors.grey.shade700,
                  ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildFilters() {
    return Wrap(
      spacing: 12,
      runSpacing: 12,
      crossAxisAlignment: WrapCrossAlignment.center,
      children: [
        SizedBox(
          width: 220,
          child: TextField(
            controller: _clientController,
            decoration: const InputDecoration(
              labelText: 'Klijent',
              border: OutlineInputBorder(),
              isDense: true,
              prefixIcon: Icon(Icons.search, size: 20),
            ),
            onChanged: (_) => _scheduleReload(),
          ),
        ),
        DropdownButton<int?>(
          value: _filterRating,
          hint: const Text('Ocjena'),
          items: const [
            DropdownMenuItem(value: null, child: Text('Sve ocjene')),
            DropdownMenuItem(value: 5, child: Text('5 zvjezdica')),
            DropdownMenuItem(value: 4, child: Text('4 zvjezdice')),
            DropdownMenuItem(value: 3, child: Text('3 zvjezdice')),
            DropdownMenuItem(value: 2, child: Text('2 zvjezdice')),
            DropdownMenuItem(value: 1, child: Text('1 zvjezdica')),
          ],
          onChanged: (v) {
            setState(() => _filterRating = v);
            _load();
          },
        ),
        if (_filterRating != null || _clientController.text.isNotEmpty)
          TextButton.icon(
            onPressed: () {
              setState(() {
                _filterRating = null;
                _clientController.clear();
              });
              _load();
            },
            icon: const Icon(Icons.clear, size: 18),
            label: const Text('Očisti filtere'),
          ),
      ],
    );
  }

  Widget _buildTable() {
    final items = _result?.items ?? [];
    if (items.isEmpty) {
      return const Center(
        child: Text('Nema recenzija za prikaz.'),
      );
    }

    return Card(
      elevation: 2,
      child: SingleChildScrollView(
        scrollDirection: Axis.horizontal,
        child: SingleChildScrollView(
          child: DataTable(
            headingRowColor: WidgetStateProperty.all(
              _brandColor.withValues(alpha: 0.08),
            ),
            columns: const [
              DataColumn(label: Text('Klijent')),
              DataColumn(label: Text('Paket')),
              DataColumn(label: Text('Datum svadbe')),
              DataColumn(label: Text('Ocjena')),
              DataColumn(label: Text('Komentar')),
              DataColumn(label: Text('Ocijenjeno')),
            ],
            rows: items
                .map(
                  (r) => DataRow(
                    cells: [
                      DataCell(Text(r.displayClientName)),
                      DataCell(Text(r.offerName)),
                      DataCell(Text(
                        r.weddingDate != null
                            ? DateFormat('dd.MM.yyyy.').format(r.weddingDate!)
                            : '—',
                      )),
                      DataCell(_starRating(r.rating)),
                      DataCell(
                        ConstrainedBox(
                          constraints: const BoxConstraints(maxWidth: 420),
                          child: Text(
                            r.comment?.trim().isNotEmpty == true
                                ? r.comment!
                                : '—',
                            softWrap: true,
                          ),
                        ),
                      ),
                      DataCell(Text(
                        DateFormat('dd.MM.yyyy.').format(r.createdAt),
                      )),
                    ],
                  ),
                )
                .toList(),
          ),
        ),
      ),
    );
  }

  Widget _starRating(int rating) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: List.generate(
        5,
        (i) => Icon(
          i < rating ? Icons.star : Icons.star_border,
          color: Colors.amber.shade700,
          size: 18,
        ),
      ),
    );
  }
}
