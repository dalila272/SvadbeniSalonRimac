import 'dart:async';

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_desktop/layouts/master_screen.dart';
import 'package:svadbeni_salon_desktop/models/salon_decoration.dart';
import 'package:svadbeni_salon_desktop/models/search_result.dart';
import 'package:svadbeni_salon_desktop/providers/decoration_provider.dart';
import 'package:svadbeni_salon_desktop/screens/decoration_details_screen.dart';
import 'package:svadbeni_salon_desktop/utils/form_ux.dart';
import 'package:svadbeni_salon_desktop/utils/utils_widgets.dart';

enum _DecorationStatusFilter { active, inactive, all }

class DecorationListScreen extends StatefulWidget {
  const DecorationListScreen({super.key});

  @override
  State<DecorationListScreen> createState() => _DecorationListScreenState();
}

class _DecorationListScreenState extends State<DecorationListScreen> {
  static const _brandColor = Color(0xFF875252);
  static const _fieldDecoration = InputDecoration(
    border: OutlineInputBorder(),
    contentPadding: EdgeInsets.symmetric(horizontal: 12, vertical: 14),
  );

  late DecorationProvider _provider;
  SearchResult<SalonDecoration>? _result;
  bool _isLoading = true;
  final _nameController = TextEditingController();
  _DecorationStatusFilter _statusFilter = _DecorationStatusFilter.all;
  Timer? _debounce;

  @override
  void initState() {
    super.initState();
    _provider = context.read<DecorationProvider>();
    _load();
  }

  @override
  void dispose() {
    _nameController.dispose();
    _debounce?.cancel();
    super.dispose();
  }

  Map<String, dynamic> _buildFilter() {
    final filter = <String, dynamic>{
      'pageSize': 100,
      'sortBy': FormUx.newestFirst,
    };
    if (_nameController.text.trim().isNotEmpty) {
      filter['naziv'] = _nameController.text.trim();
    }
    switch (_statusFilter) {
      case _DecorationStatusFilter.active:
        filter['isActive'] = true;
      case _DecorationStatusFilter.inactive:
        filter['isActive'] = false;
      case _DecorationStatusFilter.all:
        break;
    }
    return filter;
  }

  Future<void> _load() async {
    setState(() => _isLoading = true);
    try {
      final data = await _provider.get(filter: _buildFilter());
      if (!mounted) return;
      setState(() {
        _result = data;
        _isLoading = false;
      });
    } on Exception catch (e) {
      if (mounted) {
        setState(() => _isLoading = false);
        alertBox(context, 'Greška', e.toString());
      }
    }
  }

  void _scheduleLoad() {
    _debounce?.cancel();
    _debounce = Timer(const Duration(milliseconds: 350), _load);
  }

  Future<void> _openDetails({SalonDecoration? item}) async {
    final refresh = await Navigator.of(context).push(
      MaterialPageRoute(builder: (_) => DecorationDetailsScreen(item: item)),
    );
    if (refresh == 'created' || refresh == 'updated' || refresh == 'reload') {
      if (refresh == 'created' && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Dekoracija je uspješno dodana.')),
        );
      } else if (refresh == 'updated' && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Dekoracija je uspješno sačuvana.')),
        );
      }
      _load();
    }
  }

  Future<void> _confirmDelete(SalonDecoration item) async {
    final ok = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Brisanje'),
        content: Text('Obrisati dekoraciju "${item.name}"?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Odustani'),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('Obriši'),
          ),
        ],
      ),
    );
    if (ok != true) return;
    try {
      await _provider.remove(item.id);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Dekoracija je obrisana.')),
        );
      }
      _load();
    } on Exception catch (e) {
      if (mounted) alertBox(context, 'Greška', e.toString());
    }
  }

  Widget _statusLabel(SalonDecoration item) {
    return Text(
      item.isActive ? 'Aktivna' : 'Neaktivna',
      style: TextStyle(
        color: item.isActive ? Colors.green.shade800 : Colors.grey.shade600,
        fontWeight: item.isActive ? FontWeight.normal : FontWeight.w500,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: 'Dekoracije',
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
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

  Widget _buildFilters() {
    return Padding(
      padding: const EdgeInsets.all(8),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Expanded(
                flex: 2,
                child: TextField(
                  controller: _nameController,
                  textInputAction: TextInputAction.search,
                  onSubmitted: (_) => _load(),
                  decoration: _fieldDecoration.copyWith(
                    labelText: 'Naziv dekoracije',
                  ),
                  onChanged: (_) => _scheduleLoad(),
                ),
              ),
              const SizedBox(width: 12),
              SizedBox(
                width: 160,
                child: InputDecorator(
                  decoration: _fieldDecoration.copyWith(labelText: 'Status'),
                  child: DropdownButtonHideUnderline(
                    child: DropdownButton<_DecorationStatusFilter>(
                      isExpanded: true,
                      isDense: true,
                      value: _statusFilter,
                      items: const [
                        DropdownMenuItem(
                          value: _DecorationStatusFilter.all,
                          child: Text('Sve'),
                        ),
                        DropdownMenuItem(
                          value: _DecorationStatusFilter.active,
                          child: Text('Aktivne'),
                        ),
                        DropdownMenuItem(
                          value: _DecorationStatusFilter.inactive,
                          child: Text('Neaktivne'),
                        ),
                      ],
                      onChanged: (value) {
                        if (value == null) return;
                        setState(() => _statusFilter = value);
                        _load();
                      },
                    ),
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Align(
            alignment: Alignment.centerRight,
            child: ElevatedButton.icon(
              onPressed: () => _openDetails(),
              icon: const Icon(Icons.add),
              label: const Text('Nova dekoracija'),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildTable() {
    final items = _result?.items ?? [];
    if (items.isEmpty) {
      return const Center(child: Text('Nema dekoracija za prikaz.'));
    }

    return Card(
      elevation: 2,
      clipBehavior: Clip.antiAlias,
      child: LayoutBuilder(
        builder: (context, constraints) {
          return SingleChildScrollView(
            child: SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              child: ConstrainedBox(
                constraints: BoxConstraints(minWidth: constraints.maxWidth),
                child: DataTable(
                  columnSpacing: 24,
                  headingRowColor: WidgetStateProperty.all(
                    _brandColor.withValues(alpha: 0.08),
                  ),
                  columns: const [
                    DataColumn(label: Text('Naziv')),
                    DataColumn(label: Text('Opis')),
                    DataColumn(label: Text('Cijena (KM)')),
                    DataColumn(label: Text('Status')),
                    DataColumn(label: Text('Akcije')),
                  ],
                  rows: items.map((d) {
                    final muted = !d.isActive;
                    TextStyle cellStyle({FontWeight? weight}) => TextStyle(
                          color: muted ? Colors.grey.shade600 : null,
                          fontWeight: weight,
                        );

                    void open() => _openDetails(item: d);

                    return DataRow(
                      color: muted
                          ? WidgetStateProperty.all(
                              Colors.grey.shade50,
                            )
                          : null,
                      cells: [
                        DataCell(
                          Text(d.name, style: cellStyle(weight: FontWeight.w600)),
                          onTap: open,
                        ),
                        DataCell(
                          ConstrainedBox(
                            constraints: const BoxConstraints(maxWidth: 320),
                            child: Text(
                              d.description.isNotEmpty ? d.description : '—',
                              maxLines: 2,
                              overflow: TextOverflow.ellipsis,
                              style: cellStyle(),
                            ),
                          ),
                          onTap: open,
                        ),
                        DataCell(
                          Text(d.price, style: cellStyle()),
                          onTap: open,
                        ),
                        DataCell(_statusLabel(d), onTap: open),
                        DataCell(
                          Row(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              IconButton(
                                icon: const Icon(Icons.edit, size: 20),
                                onPressed: open,
                              ),
                              IconButton(
                                icon: const Icon(Icons.delete_outline, size: 20),
                                onPressed: () => _confirmDelete(d),
                              ),
                            ],
                          ),
                        ),
                      ],
                    );
                  }).toList(),
                ),
              ),
            ),
          );
        },
      ),
    );
  }
}
