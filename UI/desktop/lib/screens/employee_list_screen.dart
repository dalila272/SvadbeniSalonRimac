import 'dart:async';

import 'package:svadbeni_salon_desktop/layouts/master_screen.dart';
import 'package:svadbeni_salon_desktop/models/user.dart';
import 'package:svadbeni_salon_desktop/models/search_result.dart';
import 'package:svadbeni_salon_desktop/providers/user_provider.dart';
import 'package:svadbeni_salon_desktop/screens/employee_details_screen.dart';
import 'package:svadbeni_salon_desktop/utils/form_ux.dart';
import 'package:svadbeni_salon_desktop/utils/utils_widgets.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_desktop/constants/role_names.dart';

enum _EmployeeStatusFilter { active, inactive, all }

class EmployeeListScreen extends StatefulWidget {
  const EmployeeListScreen({super.key});

  @override
  State<EmployeeListScreen> createState() => _EmployeeListScreenState();
}

class _EmployeeListScreenState extends State<EmployeeListScreen> {
  static const _debounceDuration = Duration(milliseconds: 400);
  static const _minAutoSearchLength = 3;

  late UserProvider _provider;
  SearchResult<User>? result;
  bool isLoading = true;
  final _nameController = TextEditingController();
  _EmployeeStatusFilter _statusFilter = _EmployeeStatusFilter.active;
  Timer? _searchDebounce;

  @override
  void initState() {
    super.initState();
    _provider = context.read<UserProvider>();
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
      'roleName': RoleNames.zaposlenik,
    };

    if (_nameController.text.isNotEmpty) {
      filter['name'] = _nameController.text;
    }

    switch (_statusFilter) {
      case _EmployeeStatusFilter.active:
        filter['isActive'] = true;
      case _EmployeeStatusFilter.inactive:
        filter['isActive'] = false;
      case _EmployeeStatusFilter.all:
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

  Future<void> _openDetails({User? employee}) async {
    final refresh = await Navigator.of(context).push(
      MaterialPageRoute(
        builder: (context) => EmployeeDetailsScreen(employee: employee),
      ),
    );
    if (refresh == 'created' || refresh == 'updated' || refresh == 'reload') {
      if (refresh == 'created' && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Zaposlenik je uspješno dodan.')),
        );
      } else if (refresh == 'updated' && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Podaci zaposlenika su sačuvani.')),
        );
      }
      _load();
    }
  }

  Future<void> _confirmDelete(User employee) async {
    final fullName = '${employee.firstName ?? ''} ${employee.lastName ?? ''}'.trim();
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Brisanje / deaktivacija'),
        content: Text(
          'Ukloniti zaposlenika "$fullName"?\n\n'
          'Ako postoji historija u sistemu, nalog će biti deaktiviran umjesto trajnog brisanja.',
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
      await _provider.remove(employee.id!);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Zaposlenik je uklonjen ili deaktiviran.'),
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
      title: 'Zaposlenici',
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
                  decoration: fieldDecoration.copyWith(labelText: 'Ime ili prezime'),
                ),
              ),
              const SizedBox(width: 12),
              SizedBox(
                width: 180,
                child: InputDecorator(
                  decoration: fieldDecoration.copyWith(labelText: 'Status'),
                  child: DropdownButtonHideUnderline(
                    child: DropdownButton<_EmployeeStatusFilter>(
                      isExpanded: true,
                      isDense: true,
                      value: _statusFilter,
                      items: const [
                        DropdownMenuItem(
                          value: _EmployeeStatusFilter.active,
                          child: Text('Aktivni'),
                        ),
                        DropdownMenuItem(
                          value: _EmployeeStatusFilter.inactive,
                          child: Text('Neaktivni'),
                        ),
                        DropdownMenuItem(
                          value: _EmployeeStatusFilter.all,
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
                  child: const Text('Novi zaposlenik'),
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
                  dataRowColor: WidgetStateProperty.resolveWith((states) {
                    if (states.contains(WidgetState.hovered)) {
                      return Theme.of(context)
                          .colorScheme
                          .surfaceContainerHighest;
                    }
                    return Theme.of(context).colorScheme.surface;
                  }),
                  columns: const [
                    DataColumn(label: Text('Ime')),
                    DataColumn(label: Text('Prezime')),
                    DataColumn(label: Text('Email')),
                    DataColumn(label: Text('Korisničko ime')),
                    DataColumn(label: Text('Telefon')),
                    DataColumn(label: Text('Aktivan')),
                    DataColumn(label: Text('Obriši')),
                  ],
                  rows: (result?.items ?? [])
                      .map(
                        (e) => DataRow(
                          cells: [
                            DataCell(
                              Text(
                                e.firstName ?? '',
                                style: _inactiveTextStyle(context, e.isActive ?? true),
                              ),
                              onTap: () => _openDetails(employee: e),
                            ),
                            DataCell(
                              Text(
                                e.lastName ?? '',
                                style: _inactiveTextStyle(context, e.isActive ?? true),
                              ),
                              onTap: () => _openDetails(employee: e),
                            ),
                            DataCell(
                              Text(
                                e.email ?? '',
                                style: _inactiveTextStyle(context, e.isActive ?? true),
                              ),
                              onTap: () => _openDetails(employee: e),
                            ),
                            DataCell(
                              Text(
                                e.username ?? '',
                                style: _inactiveTextStyle(context, e.isActive ?? true),
                              ),
                              onTap: () => _openDetails(employee: e),
                            ),
                            DataCell(
                              Text(
                                e.phoneNumber ?? '-',
                                style: _inactiveTextStyle(context, e.isActive ?? true),
                              ),
                              onTap: () => _openDetails(employee: e),
                            ),
                            DataCell(
                              Text(
                                (e.isActive ?? true) ? 'Da' : 'Ne',
                                style: TextStyle(
                                  color: (e.isActive ?? true)
                                      ? null
                                      : Theme.of(context).colorScheme.error,
                                  fontWeight: (e.isActive ?? true)
                                      ? FontWeight.normal
                                      : FontWeight.w600,
                                ),
                              ),
                              onTap: () => _openDetails(employee: e),
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
