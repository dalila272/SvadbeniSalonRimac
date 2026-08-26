import 'dart:async';

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_desktop/layouts/master_screen.dart';
import 'package:svadbeni_salon_desktop/models/article_item.dart';
import 'package:svadbeni_salon_desktop/models/search_result.dart';
import 'package:svadbeni_salon_desktop/providers/article_provider.dart';
import 'package:svadbeni_salon_desktop/screens/article_details_screen.dart';
import 'package:svadbeni_salon_desktop/utils/form_ux.dart';
import 'package:svadbeni_salon_desktop/utils/utils_widgets.dart';

enum _ArticleStatusFilter { active, inactive, all }

enum _ArticleTypeFilter { all, food, drink }

class ArticleListScreen extends StatefulWidget {
  const ArticleListScreen({super.key});

  @override
  State<ArticleListScreen> createState() => _ArticleListScreenState();
}

class _ArticleListScreenState extends State<ArticleListScreen> {
  static const _debounceDuration = Duration(milliseconds: 400);
  static const _minAutoSearchLength = 3;

  late ArticleProvider _provider;
  SearchResult<Article>? result;
  bool isLoading = true;
  final _nameController = TextEditingController();
  _ArticleStatusFilter _statusFilter = _ArticleStatusFilter.active;
  _ArticleTypeFilter _typeFilter = _ArticleTypeFilter.all;
  Timer? _searchDebounce;

  @override
  void initState() {
    super.initState();
    _provider = context.read<ArticleProvider>();
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

    switch (_typeFilter) {
      case _ArticleTypeFilter.food:
        filter['tip'] = 1;
      case _ArticleTypeFilter.drink:
        filter['tip'] = 2;
      case _ArticleTypeFilter.all:
        break;
    }

    switch (_statusFilter) {
      case _ArticleStatusFilter.active:
        filter['isActive'] = true;
      case _ArticleStatusFilter.inactive:
        filter['isActive'] = false;
      case _ArticleStatusFilter.all:
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

  Future<void> _openDetails({Article? article}) async {
    final refresh = await Navigator.of(context).push(
      MaterialPageRoute(
        builder: (context) => ArticleDetailsScreen(article: article),
      ),
    );
    if (refresh == 'created' || refresh == 'updated' || refresh == 'reload') {
      if (refresh == 'created' && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Artikal je uspješno dodan.')),
        );
      } else if (refresh == 'updated' && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Artikal je uspješno sačuvan.')),
        );
      }
      _load();
    }
  }

  Future<void> _confirmDelete(Article article) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Brisanje'),
        content: Text('Obrisati artikal "${article.name}"?'),
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
      await _provider.remove(article.id);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Artikal je obrisan.')),
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
      title: 'Artikli',
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
                flex: 2,
                child: TextField(
                  controller: _nameController,
                  textInputAction: TextInputAction.search,
                  onSubmitted: (_) => _submitSearch(),
                  decoration: fieldDecoration.copyWith(labelText: 'Naziv artikla'),
                ),
              ),
              const SizedBox(width: 12),
              SizedBox(
                width: 160,
                child: InputDecorator(
                  decoration: fieldDecoration.copyWith(labelText: 'Tip'),
                  child: DropdownButtonHideUnderline(
                    child: DropdownButton<_ArticleTypeFilter>(
                      isExpanded: true,
                      isDense: true,
                      value: _typeFilter,
                      items: const [
                        DropdownMenuItem(
                          value: _ArticleTypeFilter.all,
                          child: Text('Svi'),
                        ),
                        DropdownMenuItem(
                          value: _ArticleTypeFilter.food,
                          child: Text('Hrana'),
                        ),
                        DropdownMenuItem(
                          value: _ArticleTypeFilter.drink,
                          child: Text('Piće'),
                        ),
                      ],
                      onChanged: (value) {
                        if (value == null) return;
                        setState(() => _typeFilter = value);
                        _submitSearch();
                      },
                    ),
                  ),
                ),
              ),
              const SizedBox(width: 12),
              SizedBox(
                width: 160,
                child: InputDecorator(
                  decoration: fieldDecoration.copyWith(labelText: 'Status'),
                  child: DropdownButtonHideUnderline(
                    child: DropdownButton<_ArticleStatusFilter>(
                      isExpanded: true,
                      isDense: true,
                      value: _statusFilter,
                      items: const [
                        DropdownMenuItem(
                          value: _ArticleStatusFilter.active,
                          child: Text('Aktivni'),
                        ),
                        DropdownMenuItem(
                          value: _ArticleStatusFilter.inactive,
                          child: Text('Neaktivni'),
                        ),
                        DropdownMenuItem(
                          value: _ArticleStatusFilter.all,
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
                  onPressed: () => _openDetails(),
                  child: const Text('Novi artikal'),
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
                    DataColumn(label: Text('Tip')),
                    DataColumn(label: Text('Cijena (KM)')),
                    DataColumn(label: Text('Aktivan')),
                    DataColumn(label: Text('Obriši')),
                  ],
                  rows: (result?.items ?? [])
                      .map(
                        (a) => DataRow(
                          cells: [
                            DataCell(
                              Text(a.name),
                              onTap: () => _openDetails(article: a),
                            ),
                            DataCell(
                              Text(a.tipLabel),
                              onTap: () => _openDetails(article: a),
                            ),
                            DataCell(
                              Text(a.price),
                              onTap: () => _openDetails(article: a),
                            ),
                            DataCell(
                              Text(a.isActive ? 'Da' : 'Ne'),
                              onTap: () => _openDetails(article: a),
                            ),
                            DataCell(
                              IconButton(
                                visualDensity: VisualDensity.compact,
                                icon: const Icon(Icons.delete_outline),
                                onPressed: () => _confirmDelete(a),
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
