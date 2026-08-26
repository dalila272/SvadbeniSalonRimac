import 'dart:async';

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_desktop/layouts/master_screen.dart';
import 'package:svadbeni_salon_desktop/models/menu.dart';
import 'package:svadbeni_salon_desktop/models/search_result.dart';
import 'package:svadbeni_salon_desktop/providers/article_provider.dart';
import 'package:svadbeni_salon_desktop/providers/menu_provider.dart';
import 'package:svadbeni_salon_desktop/screens/menu_details_screen.dart';
import 'package:svadbeni_salon_desktop/utils/form_ux.dart';
import 'package:svadbeni_salon_desktop/utils/utils_widgets.dart';

enum _MenuStatusFilter { active, inactive, all }

class MenuListScreen extends StatefulWidget {
  const MenuListScreen({super.key});

  @override
  State<MenuListScreen> createState() => _MenuListScreenState();
}

class _MenuListScreenState extends State<MenuListScreen> {
  static const _debounceDuration = Duration(milliseconds: 400);
  static const _minAutoSearchLength = 3;

  late MenuProvider _provider;
  SearchResult<Menu>? result;
  bool isLoading = true;
  final _nameController = TextEditingController();
  _MenuStatusFilter _statusFilter = _MenuStatusFilter.active;
  Timer? _searchDebounce;

  @override
  void initState() {
    super.initState();
    _provider = context.read<MenuProvider>();
    _nameController.addListener(_onSearchTextChanged);
    _load();
  }

  @override
  void dispose() {
    _searchDebounce?.cancel();
    _nameController.removeListener(_onSearchTextChanged);
    _nameController.dispose();
    super.dispose();
  }

  Map<String, dynamic> _buildFilter() {
    final filter = <String, dynamic>{
      'pageSize': 100,
      'sortBy': FormUx.newestFirst,
    };

    if (_nameController.text.isNotEmpty) {
      filter['naziv'] = _nameController.text;
    }

    switch (_statusFilter) {
      case _MenuStatusFilter.active:
        filter['isActive'] = true;
      case _MenuStatusFilter.inactive:
        filter['isActive'] = false;
      case _MenuStatusFilter.all:
        break;
    }

    return filter;
  }

  void _onSearchTextChanged() {
    final query = _nameController.text.trim();
    _searchDebounce?.cancel();

    if (query.isEmpty) {
      _searchDebounce = Timer(_debounceDuration, _load);
      return;
    }

    if (query.length >= _minAutoSearchLength) {
      _searchDebounce = Timer(_debounceDuration, _load);
    }
  }

  void _submitSearch() {
    _searchDebounce?.cancel();
    _load();
  }

  Future<void> _load() async {
    setState(() => isLoading = true);
    try {
      final data = await _provider.get(filter: _buildFilter());
      setState(() {
        result = data;
        isLoading = false;
      });
    } on Exception catch (e) {
      setState(() => isLoading = false);
      if (mounted) alertBox(context, 'Greška', e.toString());
    }
  }

  Future<void> _openCreate() async {
    try {
      final articles = await context.read<ArticleProvider>().get(
            filter: {'pageSize': 1, 'isActive': true},
          );
      if (!mounted) return;
      final count = articles.items?.length ?? 0;
      final ok = await FormUx.ensurePrerequisites(
        context,
        missing: [
          if (count == 0) 'barem jedan artikal (hrana ili piće)',
        ],
      );
      if (!ok || !mounted) return;
      await _openDetails();
    } on Exception catch (e) {
      if (mounted) alertBox(context, 'Greška', e.toString());
    }
  }

  Future<void> _openDetails({Menu? menu}) async {
    final refresh = await Navigator.of(context).push(
      MaterialPageRoute(
        builder: (context) => MenuDetailsScreen(menu: menu),
      ),
    );
    if (refresh == 'created' || refresh == 'updated' || refresh == 'reload') {
      if (refresh == 'created' && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Meni je uspješno dodan.')),
        );
      } else if (refresh == 'updated' && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Meni je uspješno sačuvan.')),
        );
      }
      _load();
    }
  }

  Future<void> _confirmDelete(Menu menu) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Brisanje'),
        content: Text('Obrisati meni "${menu.name}"?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Odustani'),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Obriši'),
          ),
        ],
      ),
    );

    if (confirmed != true) return;

    try {
      await _provider.remove(menu.id);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Meni je obrisan.')),
        );
      }
      _load();
    } on Exception catch (e) {
      if (mounted) alertBox(context, 'Greška', e.toString());
    }
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: 'Meniji',
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            _buildSearch(),
            isLoading
                ? const Expanded(
                    child: Center(child: CircularProgressIndicator()),
                  )
                : _buildTable(),
          ],
        ),
      ),
    );
  }

  Widget _buildSearch() {
    const fieldDecoration = InputDecoration(
      border: OutlineInputBorder(),
      contentPadding: EdgeInsets.symmetric(horizontal: 12, vertical: 14),
    );

    return Padding(
      padding: const EdgeInsets.all(8),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Expanded(
                child: TextField(
                  controller: _nameController,
                  textInputAction: TextInputAction.search,
                  onSubmitted: (_) => _submitSearch(),
                  decoration: fieldDecoration.copyWith(labelText: 'Naziv menija'),
                ),
              ),
              const SizedBox(width: 12),
              SizedBox(
                width: 180,
                child: InputDecorator(
                  decoration: fieldDecoration.copyWith(labelText: 'Status'),
                  child: DropdownButtonHideUnderline(
                    child: DropdownButton<_MenuStatusFilter>(
                      isExpanded: true,
                      isDense: true,
                      value: _statusFilter,
                      items: const [
                        DropdownMenuItem(
                          value: _MenuStatusFilter.active,
                          child: Text('Aktivni'),
                        ),
                        DropdownMenuItem(
                          value: _MenuStatusFilter.inactive,
                          child: Text('Neaktivni'),
                        ),
                        DropdownMenuItem(
                          value: _MenuStatusFilter.all,
                          child: Text('Svi'),
                        ),
                      ],
                      onChanged: (value) {
                        if (value == null) return;
                        setState(() => _statusFilter = value);
                        _submitSearch();
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
            child: Wrap(
              spacing: 8,
              children: [
                ElevatedButton(
                  onPressed: _submitSearch,
                  child: const Text('Pretraži'),
                ),
                ElevatedButton(
                  onPressed: _openCreate,
                  child: const Text('Novi meni'),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildTable() {
    return Expanded(
      child: LayoutBuilder(
        builder: (context, constraints) {
          return SingleChildScrollView(
            child: SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              child: ConstrainedBox(
                constraints: BoxConstraints(minWidth: constraints.maxWidth),
                child: DataTable(
                  columnSpacing: 20,
                  columns: const [
                    DataColumn(label: Text('Naziv')),
                    DataColumn(label: Text('Cijena (KM)')),
                    DataColumn(label: Text('Opis')),
                    DataColumn(label: Text('Aktivan')),
                    DataColumn(label: Text('Obriši')),
                  ],
                  rows: (result?.items ?? [])
                      .map(
                        (m) => DataRow(
                          cells: [
                            DataCell(
                              Text(m.name),
                              onTap: () => _openDetails(menu: m),
                            ),
                            DataCell(
                              Text(m.price),
                              onTap: () => _openDetails(menu: m),
                            ),
                            DataCell(
                              Text(
                                m.description,
                                overflow: TextOverflow.ellipsis,
                              ),
                              onTap: () => _openDetails(menu: m),
                            ),
                            DataCell(
                              Text(m.isActive ? 'Da' : 'Ne'),
                              onTap: () => _openDetails(menu: m),
                            ),
                            DataCell(
                              IconButton(
                                visualDensity: VisualDensity.compact,
                                icon: const Icon(Icons.delete_outline),
                                onPressed: () => _confirmDelete(m),
                              ),
                            ),
                          ],
                        ),
                      )
                      .toList(),
                ),
              ),
            ),
          );
        },
      ),
    );
  }
}
