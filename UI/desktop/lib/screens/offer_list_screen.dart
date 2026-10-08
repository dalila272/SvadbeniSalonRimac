import 'dart:async';

import 'package:svadbeni_salon_desktop/layouts/master_screen.dart';
import 'package:svadbeni_salon_desktop/models/catalog_item.dart';
import 'package:svadbeni_salon_desktop/models/offer.dart';
import 'package:svadbeni_salon_desktop/models/search_result.dart';
import 'package:svadbeni_salon_desktop/providers/decoration_provider.dart';
import 'package:svadbeni_salon_desktop/providers/menu_provider.dart';
import 'package:svadbeni_salon_desktop/providers/musician_provider.dart';
import 'package:svadbeni_salon_desktop/providers/offer_provider.dart';
import 'package:svadbeni_salon_desktop/screens/offer_details_screen.dart';
import 'package:svadbeni_salon_desktop/utils/form_ux.dart';
import 'package:svadbeni_salon_desktop/utils/utils_widgets.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

enum _OfferStatusFilter { active, inactive, all }

class OfferListScreen extends StatefulWidget {
  const OfferListScreen({super.key});

  @override
  State<OfferListScreen> createState() => _OfferListScreenState();
}

class _OfferListScreenState extends State<OfferListScreen> {
  static const _debounceDuration = Duration(milliseconds: 400);
  static const _minAutoSearchLength = 3;
  static const _fieldDecoration = InputDecoration(
    border: OutlineInputBorder(),
    contentPadding: EdgeInsets.symmetric(horizontal: 12, vertical: 14),
  );

  late OfferProvider _provider;
  late MenuProvider _menuProvider;
  late MusicianProvider _musicianProvider;
  late DecorationProvider _decorationProvider;

  SearchResult<Offer>? result;
  List<CatalogItem> _menus = [];
  List<CatalogItem> _musicians = [];
  List<CatalogItem> _decorations = [];
  bool isLoading = true;

  final _nameController = TextEditingController();
  _OfferStatusFilter _statusFilter = _OfferStatusFilter.active;
  int? _selectedMenuId;
  int? _selectedMusicianId;
  int? _selectedDecorationId;
  Timer? _searchDebounce;

  @override
  void initState() {
    super.initState();
    _provider = context.read<OfferProvider>();
    _menuProvider = context.read<MenuProvider>();
    _musicianProvider = context.read<MusicianProvider>();
    _decorationProvider = context.read<DecorationProvider>();
    _nameController.addListener(_onSearchTextChanged);
    _loadCatalogs();
    _load();
  }

  Future<void> _loadCatalogs() async {
    try {
      final menus = await _menuProvider.get(filter: {'pageSize': 100});
      final musicians = await _musicianProvider.get(filter: {'pageSize': 100});
      final decorations = await _decorationProvider.get(filter: {'pageSize': 100});

      if (!mounted) return;
      setState(() {
        _menus = (menus.items ?? [])
            .map((m) => CatalogItem(id: m.id, name: m.name))
            .toList();
        _musicians = (musicians.items ?? [])
            .map((m) => CatalogItem(id: m.id, name: m.name))
            .toList();
        _decorations = (decorations.items ?? [])
            .map((d) => CatalogItem(id: d.id, name: d.name))
            .toList();
      });
    } catch (e) {
      if (mounted) {
        alertBox(context, 'Greška', e.toString());
      }
    }
  }

  @override
  void dispose() {
    _searchDebounce?.cancel();
    _nameController.removeListener(_onSearchTextChanged);
    _nameController.dispose();
    super.dispose();
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

  Map<String, dynamic> _buildFilter() {
    final filter = <String, dynamic>{
      'pageSize': 100,
      'sortBy': FormUx.newestFirst,
    };

    if (_nameController.text.isNotEmpty) {
      filter['naziv'] = _nameController.text;
    }

    switch (_statusFilter) {
      case _OfferStatusFilter.active:
        filter['isActive'] = true;
      case _OfferStatusFilter.inactive:
        filter['isActive'] = false;
      case _OfferStatusFilter.all:
        filter['includeInactive'] = true;
    }

    if (_selectedMenuId != null) {
      filter['meniId'] = _selectedMenuId;
    }
    if (_selectedMusicianId != null) {
      filter['muzicarId'] = _selectedMusicianId;
    }
    if (_selectedDecorationId != null) {
      filter['dekoracijaId'] = _selectedDecorationId;
    }

    return filter;
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
    final ok = await FormUx.ensurePrerequisites(
      context,
      missing: [
        if (_menus.isEmpty) 'barem jedan meni (Meniji)',
      ],
    );
    if (!ok) return;
    await _openDetails();
  }

  Future<void> _openDetails({Offer? offer}) async {
    final refresh = await Navigator.of(context).push(
      MaterialPageRoute(
        builder: (context) => OfferDetailsScreen(offer: offer),
      ),
    );
    if (refresh == 'created' || refresh == 'updated' || refresh == 'reload') {
      if (refresh == 'created' && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Ponuda je uspješno dodana.')),
        );
      } else if (refresh == 'updated' && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Ponuda je uspješno sačuvana.')),
        );
      }
      _load();
    }
  }

  Future<void> _confirmDelete(Offer offer) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Brisanje / deaktivacija'),
        content: Text(
          'Ukloniti ponudu "${offer.name}"?\n\n'
          'Ako postoje rezervacije ili recenzije, ponuda će biti deaktivirana umjesto trajnog brisanja.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Odustani'),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Ukloni'),
          ),
        ],
      ),
    );

    if (confirmed != true) return;

    try {
      await _provider.remove(offer.id);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Ponuda je uklonjena ili deaktivirana.'),
          ),
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
      title: 'Ponude',
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
                  decoration: _fieldDecoration.copyWith(labelText: 'Naziv'),
                ),
              ),
              const SizedBox(width: 12),
              SizedBox(
                width: 180,
                child: InputDecorator(
                  decoration: _fieldDecoration.copyWith(labelText: 'Status'),
                  child: DropdownButtonHideUnderline(
                    child: DropdownButton<_OfferStatusFilter>(
                      isExpanded: true,
                      isDense: true,
                      value: _statusFilter,
                      items: const [
                        DropdownMenuItem(
                          value: _OfferStatusFilter.active,
                          child: Text('Aktivne'),
                        ),
                        DropdownMenuItem(
                          value: _OfferStatusFilter.inactive,
                          child: Text('Neaktivne'),
                        ),
                        DropdownMenuItem(
                          value: _OfferStatusFilter.all,
                          child: Text('Sve'),
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
          Row(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Expanded(
                child: _catalogDropdown(
                  label: 'Meni',
                  value: _selectedMenuId,
                  items: _menus,
                  onChanged: (value) {
                    setState(() => _selectedMenuId = value);
                    _submitSearch();
                  },
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: _catalogDropdown(
                  label: 'Muzičar',
                  value: _selectedMusicianId,
                  items: _musicians,
                  onChanged: (value) {
                    setState(() => _selectedMusicianId = value);
                    _submitSearch();
                  },
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: _catalogDropdown(
                  label: 'Dekoracija',
                  value: _selectedDecorationId,
                  items: _decorations,
                  onChanged: (value) {
                    setState(() => _selectedDecorationId = value);
                    _submitSearch();
                  },
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
                  child: const Text('Nova ponuda'),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _catalogDropdown({
    required String label,
    required int? value,
    required List<CatalogItem> items,
    required ValueChanged<int?> onChanged,
  }) {
    return InputDecorator(
      decoration: _fieldDecoration.copyWith(labelText: label),
      child: DropdownButtonHideUnderline(
        child: DropdownButton<int?>(
          isExpanded: true,
          isDense: true,
          value: value,
          hint: const Text('Svi'),
          items: [
            const DropdownMenuItem<int?>(
              value: null,
              child: Text('Svi'),
            ),
            ...items.map(
              (item) => DropdownMenuItem<int?>(
                value: item.id,
                child: Text(item.name, overflow: TextOverflow.ellipsis),
              ),
            ),
          ],
          onChanged: onChanged,
        ),
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
                  dataRowColor: WidgetStateProperty.resolveWith((states) {
                    if (states.contains(WidgetState.hovered)) {
                      return Theme.of(context)
                          .colorScheme
                          .surfaceContainerHighest;
                    }
                    return Theme.of(context).colorScheme.surface;
                  }),
                  columns: const [
                    DataColumn(label: Text('Naziv')),
                    DataColumn(label: Text('Cijena (KM)')),
                    DataColumn(label: Text('Opis')),
                    DataColumn(label: Text('Aktivna')),
                    DataColumn(label: Text('Obriši')),
                  ],
                  rows: (result?.items ?? [])
                      .map(
                        (e) => DataRow(
                          cells: [
                            DataCell(
                              SizedBox(
                                width: 140,
                                child: Text(
                                  e.name,
                                  overflow: TextOverflow.ellipsis,
                                  style: _inactiveTextStyle(context, e.isActive),
                                ),
                              ),
                              onTap: () => _openDetails(offer: e),
                            ),
                            DataCell(
                              Text(
                                e.price,
                                style: _inactiveTextStyle(context, e.isActive),
                              ),
                              onTap: () => _openDetails(offer: e),
                            ),
                            DataCell(
                              SizedBox(
                                width: 260,
                                child: Text(
                                  e.description,
                                  overflow: TextOverflow.ellipsis,
                                  maxLines: 2,
                                  style: _inactiveTextStyle(context, e.isActive),
                                ),
                              ),
                              onTap: () => _openDetails(offer: e),
                            ),
                            DataCell(
                              Text(
                                e.isActive ? 'Da' : 'Ne',
                                style: TextStyle(
                                  color: e.isActive
                                      ? null
                                      : Theme.of(context).colorScheme.error,
                                  fontWeight: e.isActive
                                      ? FontWeight.normal
                                      : FontWeight.w600,
                                ),
                              ),
                              onTap: () => _openDetails(offer: e),
                            ),
                            DataCell(
                              IconButton(
                                visualDensity: VisualDensity.compact,
                                icon: const Icon(Icons.delete_outline),
                                onPressed: () => _confirmDelete(e),
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

  TextStyle? _inactiveTextStyle(BuildContext context, bool isActive) {
    if (isActive) return null;
    return TextStyle(color: Theme.of(context).colorScheme.outline);
  }
}
