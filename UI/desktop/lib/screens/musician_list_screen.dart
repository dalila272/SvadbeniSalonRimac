import 'dart:async';

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_desktop/layouts/master_screen.dart';
import 'package:svadbeni_salon_desktop/models/musician.dart';
import 'package:svadbeni_salon_desktop/models/search_result.dart';
import 'package:svadbeni_salon_desktop/providers/musician_provider.dart';
import 'package:svadbeni_salon_desktop/screens/musician_details_screen.dart';
import 'package:svadbeni_salon_desktop/utils/form_ux.dart';
import 'package:svadbeni_salon_desktop/utils/utils_widgets.dart';

enum _MusicianStatusFilter { active, inactive, all }

class MusicianListScreen extends StatefulWidget {
  const MusicianListScreen({super.key});

  @override
  State<MusicianListScreen> createState() => _MusicianListScreenState();
}

class _MusicianListScreenState extends State<MusicianListScreen> {
  static const _brandColor = Color(0xFF875252);
  static const _fieldDecoration = InputDecoration(
    border: OutlineInputBorder(),
    contentPadding: EdgeInsets.symmetric(horizontal: 12, vertical: 14),
  );

  late MusicianProvider _provider;
  SearchResult<Musician>? _result;
  bool _isLoading = true;
  final _nameController = TextEditingController();
  _MusicianStatusFilter _statusFilter = _MusicianStatusFilter.all;
  Timer? _debounce;

  @override
  void initState() {
    super.initState();
    _provider = context.read<MusicianProvider>();
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
      case _MusicianStatusFilter.active:
        filter['isActive'] = true;
      case _MusicianStatusFilter.inactive:
        filter['isActive'] = false;
      case _MusicianStatusFilter.all:
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

  Future<void> _openDetails({Musician? musician}) async {
    final refresh = await Navigator.of(context).push(
      MaterialPageRoute(
        builder: (_) => MusicianDetailsScreen(musician: musician),
      ),
    );
    if (refresh == 'created' || refresh == 'updated' || refresh == 'reload') {
      if (refresh == 'created' && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Muzičar je uspješno dodan.')),
        );
      } else if (refresh == 'updated' && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Muzičar je uspješno sačuvan.')),
        );
      }
      _load();
    }
  }

  Future<void> _confirmDelete(Musician musician) async {
    final ok = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Brisanje'),
        content: Text('Obrisati muzičara "${musician.name}"?'),
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
      await _provider.remove(musician.id);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Muzičar je obrisan.')),
        );
      }
      _load();
    } on Exception catch (e) {
      if (mounted) alertBox(context, 'Greška', e.toString());
    }
  }

  Widget _statusLabel(Musician musician) {
    return Text(
      musician.isActive ? 'Aktivan' : 'Neaktivan',
      style: TextStyle(
        color: musician.isActive ? Colors.green.shade800 : Colors.grey.shade600,
        fontWeight: musician.isActive ? FontWeight.normal : FontWeight.w500,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: 'Muzičari',
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
                    labelText: 'Naziv muzičara',
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
                    child: DropdownButton<_MusicianStatusFilter>(
                      isExpanded: true,
                      isDense: true,
                      value: _statusFilter,
                      items: const [
                        DropdownMenuItem(
                          value: _MusicianStatusFilter.all,
                          child: Text('Sve'),
                        ),
                        DropdownMenuItem(
                          value: _MusicianStatusFilter.active,
                          child: Text('Aktivni'),
                        ),
                        DropdownMenuItem(
                          value: _MusicianStatusFilter.inactive,
                          child: Text('Neaktivni'),
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
              label: const Text('Novi muzičar'),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildTable() {
    final items = _result?.items ?? [];
    if (items.isEmpty) {
      return const Center(child: Text('Nema muzičara za prikaz.'));
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
                    DataColumn(label: Text('Žanrovi')),
                    DataColumn(label: Text('Status')),
                    DataColumn(label: Text('Akcije')),
                  ],
                  rows: items.map((m) {
                    final muted = !m.isActive;
                    TextStyle cellStyle({FontWeight? weight}) => TextStyle(
                          color: muted ? Colors.grey.shade600 : null,
                          fontWeight: weight,
                        );

                    void open() => _openDetails(musician: m);

                    return DataRow(
                      color: muted
                          ? WidgetStateProperty.all(Colors.grey.shade50)
                          : null,
                      cells: [
                        DataCell(
                          Text(m.name, style: cellStyle(weight: FontWeight.w600)),
                          onTap: open,
                        ),
                        DataCell(
                          ConstrainedBox(
                            constraints: const BoxConstraints(maxWidth: 280),
                            child: Text(
                              m.description.isNotEmpty ? m.description : '—',
                              maxLines: 2,
                              overflow: TextOverflow.ellipsis,
                              style: cellStyle(),
                            ),
                          ),
                          onTap: open,
                        ),
                        DataCell(
                          Text(m.genresLabel, style: cellStyle()),
                          onTap: open,
                        ),
                        DataCell(_statusLabel(m), onTap: open),
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
                                onPressed: () => _confirmDelete(m),
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
