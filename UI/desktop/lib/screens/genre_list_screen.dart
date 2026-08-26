import 'dart:async';

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_desktop/layouts/master_screen.dart';
import 'package:svadbeni_salon_desktop/models/genre.dart';
import 'package:svadbeni_salon_desktop/models/search_result.dart';
import 'package:svadbeni_salon_desktop/providers/genre_provider.dart';
import 'package:svadbeni_salon_desktop/screens/genre_details_screen.dart';
import 'package:svadbeni_salon_desktop/utils/form_ux.dart';
import 'package:svadbeni_salon_desktop/utils/utils_widgets.dart';

enum _GenreStatusFilter { active, inactive, all }

class GenreListScreen extends StatefulWidget {
  const GenreListScreen({super.key});

  @override
  State<GenreListScreen> createState() => _GenreListScreenState();
}

class _GenreListScreenState extends State<GenreListScreen> {
  static const _brandColor = Color(0xFF875252);
  static const _fieldDecoration = InputDecoration(
    border: OutlineInputBorder(),
    contentPadding: EdgeInsets.symmetric(horizontal: 12, vertical: 14),
  );

  late GenreProvider _provider;
  SearchResult<Genre>? _result;
  bool _isLoading = true;
  final _nameController = TextEditingController();
  _GenreStatusFilter _statusFilter = _GenreStatusFilter.all;
  Timer? _debounce;

  @override
  void initState() {
    super.initState();
    _provider = context.read<GenreProvider>();
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
      case _GenreStatusFilter.active:
        filter['isActive'] = true;
      case _GenreStatusFilter.inactive:
        filter['isActive'] = false;
      case _GenreStatusFilter.all:
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

  Future<void> _openDetails({Genre? genre}) async {
    final refresh = await Navigator.of(context).push(
      MaterialPageRoute(
        builder: (_) => GenreDetailsScreen(genre: genre),
      ),
    );
    if (refresh == 'created' || refresh == 'updated' || refresh == 'reload') {
      if (refresh == 'created' && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Žanr je uspješno dodan.')),
        );
      } else if (refresh == 'updated' && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Žanr je uspješno sačuvan.')),
        );
      }
      _load();
    }
  }

  Future<void> _confirmDelete(Genre genre) async {
    final ok = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Brisanje'),
        content: Text('Obrisati žanr "${genre.name}"?'),
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
      await _provider.remove(genre.id);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Žanr je obrisan.')),
        );
      }
      _load();
    } on Exception catch (e) {
      if (mounted) alertBox(context, 'Greška', e.toString());
    }
  }

  Widget _statusLabel(Genre genre) {
    return Text(
      genre.isActive ? 'Aktivan' : 'Neaktivan',
      style: TextStyle(
        color: genre.isActive ? Colors.green.shade800 : Colors.grey.shade600,
        fontWeight: genre.isActive ? FontWeight.normal : FontWeight.w500,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: 'Žanrovi',
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
                    labelText: 'Naziv žanra',
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
                    child: DropdownButton<_GenreStatusFilter>(
                      isExpanded: true,
                      isDense: true,
                      value: _statusFilter,
                      items: const [
                        DropdownMenuItem(
                          value: _GenreStatusFilter.all,
                          child: Text('Sve'),
                        ),
                        DropdownMenuItem(
                          value: _GenreStatusFilter.active,
                          child: Text('Aktivni'),
                        ),
                        DropdownMenuItem(
                          value: _GenreStatusFilter.inactive,
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
              label: const Text('Novi žanr'),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildTable() {
    final items = _result?.items ?? [];
    if (items.isEmpty) {
      return const Center(child: Text('Nema žanrova za prikaz.'));
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
                    DataColumn(label: Text('Status')),
                    DataColumn(label: Text('Akcije')),
                  ],
                  rows: items.map((g) {
                    final muted = !g.isActive;
                    TextStyle cellStyle({FontWeight? weight}) => TextStyle(
                          color: muted ? Colors.grey.shade600 : null,
                          fontWeight: weight,
                        );

                    void open() => _openDetails(genre: g);

                    return DataRow(
                      color: muted
                          ? WidgetStateProperty.all(Colors.grey.shade50)
                          : null,
                      cells: [
                        DataCell(
                          Text(g.name, style: cellStyle(weight: FontWeight.w600)),
                          onTap: open,
                        ),
                        DataCell(_statusLabel(g), onTap: open),
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
                                onPressed: () => _confirmDelete(g),
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
