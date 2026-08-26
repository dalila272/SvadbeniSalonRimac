import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_desktop/layouts/master_screen.dart';
import 'package:svadbeni_salon_desktop/models/article_item.dart';
import 'package:svadbeni_salon_desktop/models/menu.dart';
import 'package:svadbeni_salon_desktop/providers/article_provider.dart';
import 'package:svadbeni_salon_desktop/providers/menu_provider.dart';
import 'package:svadbeni_salon_desktop/utils/utils_widgets.dart';

class MenuDetailsScreen extends StatefulWidget {
  final Menu? menu;

  const MenuDetailsScreen({super.key, this.menu});

  @override
  State<MenuDetailsScreen> createState() => _MenuDetailsScreenState();
}

class _MenuDetailsScreenState extends State<MenuDetailsScreen> {
  final _formKey = GlobalKey<FormBuilderState>();
  Map<String, dynamic> _initialValue = {};

  late MenuProvider _menuProvider;
  late ArticleProvider _articleProvider;

  List<Article> _foodItems = [];
  List<Article> _drinkItems = [];
  bool isLoading = true;

  bool get _isEditing => widget.menu != null;

  @override
  void initState() {
    super.initState();
    _menuProvider = context.read<MenuProvider>();
    _articleProvider = context.read<ArticleProvider>();
    _loadForm();
  }

  Future<void> _loadForm() async {
    try {
      final articles = await _articleProvider.get(
        filter: {'isActive': true, 'pageSize': 100},
      );
      final food = articles.items?.where((a) => a.tip == 1).toList() ?? [];
      final drinks = articles.items?.where((a) => a.tip == 2).toList() ?? [];

      MenuDetail? detail;
      if (widget.menu != null) {
        detail = await _menuProvider.getDetail(widget.menu!.id);
      }

      if (!mounted) return;

      setState(() {
        _foodItems = food;
        _drinkItems = drinks;
        _initialValue = {
          'naziv': detail?.name ?? '',
          'opis': detail?.description ?? '',
          'cijena': detail?.price ?? '',
          'isActive': detail?.isActive ?? true,
          'hranaIds': detail?.hranaIds ?? <int>[],
          'piceIds': detail?.piceIds ?? <int>[],
        };
        isLoading = false;
      });
    } on Exception catch (e) {
      if (mounted) alertBox(context, 'Greška', e.toString());
    }
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: _isEditing ? 'Uredi meni' : 'Novi meni',
      child: Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 720),
            child: isLoading
                ? const Center(child: CircularProgressIndicator())
                : Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      _buildForm(),
                      const SizedBox(height: 24),
                      _buildActions(),
                    ],
                  ),
          ),
        ),
      ),
    );
  }

  Widget _buildForm() {
    return FormBuilder(
      key: _formKey,
      initialValue: _initialValue,
      autovalidateMode: AutovalidateMode.onUserInteraction,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          FormBuilderTextField(
            name: 'naziv',
            decoration: const InputDecoration(
              labelText: 'Naziv',
              border: OutlineInputBorder(),
            ),
            validator: FormBuilderValidators.compose([
              FormBuilderValidators.required(
                errorText: 'Naziv menija je obavezan.',
              ),
              FormBuilderValidators.maxLength(
                200,
                errorText: 'Naziv ne smije imati više od 200 karaktera.',
              ),
            ]),
          ),
          const SizedBox(height: 16),
          FormBuilderTextField(
            name: 'opis',
            decoration: const InputDecoration(
              labelText: 'Opis',
              helperText: 'Opciono — najviše 2000 karaktera',
              border: OutlineInputBorder(),
            ),
            maxLines: 4,
            validator: FormBuilderValidators.maxLength(
              2000,
              errorText: 'Opis ne smije imati više od 2000 karaktera.',
            ),
          ),
          const SizedBox(height: 16),
          FormBuilderTextField(
            name: 'cijena',
            decoration: const InputDecoration(
              labelText: 'Cijena (KM)',
              helperText: 'Broj ≥ 0, npr. 50.00',
              prefixIcon: Icon(Icons.payments_outlined),
              border: OutlineInputBorder(),
            ),
            keyboardType: const TextInputType.numberWithOptions(decimal: true),
            validator: (value) {
              final raw = value?.toString().trim() ?? '';
              if (raw.isEmpty) return 'Cijena je obavezna.';
              final parsed = double.tryParse(raw.replaceAll(',', '.'));
              if (parsed == null) {
                return 'Unesite validan broj za cijenu (npr. 50.00).';
              }
              if (parsed < 0) return 'Cijena ne smije biti negativna.';
              return null;
            },
          ),
          const SizedBox(height: 16),
          _buildCheckboxSection(
            title: 'Hrana',
            fieldName: 'hranaIds',
            items: _foodItems,
          ),
          const SizedBox(height: 16),
          _buildCheckboxSection(
            title: 'Piće',
            fieldName: 'piceIds',
            items: _drinkItems,
          ),
          const SizedBox(height: 16),
          FormBuilderCheckbox(
            name: 'isActive',
            title: const Text('Aktivan meni'),
          ),
        ],
      ),
    );
  }

  Widget _buildCheckboxSection({
    required String title,
    required String fieldName,
    required List<Article> items,
  }) {
    final allIds = items.map((e) => e.id).toList();

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Row(
          children: [
            Text(title, style: const TextStyle(fontWeight: FontWeight.w600)),
            const Spacer(),
            TextButton(
              onPressed: allIds.isEmpty
                  ? null
                  : () => _setCheckboxField(fieldName, allIds),
              child: const Text('Odaberi sve'),
            ),
            TextButton(
              onPressed: () => _setCheckboxField(fieldName, []),
              child: const Text('Poništi'),
            ),
          ],
        ),
        const SizedBox(height: 8),
        if (items.isEmpty)
          Text(
            'Nema dostupnih stavki.',
            style: TextStyle(color: Colors.grey[600]),
          )
        else
          FormBuilderCheckboxGroup<int>(
            name: fieldName,
            validator: (value) {
              final form = _formKey.currentState?.instantValue;
              final hrana = _toIntList(form?['hranaIds']);
              final pice = _toIntList(form?['piceIds']);
              final current = _toIntList(value);
              final other = fieldName == 'hranaIds' ? pice : hrana;
              if (current.isEmpty && other.isEmpty) {
                return 'Odaberite barem jednu hranu ili piće.';
              }
              return null;
            },
            options: items
                .map(
                  (item) => FormBuilderFieldOption(
                    value: item.id,
                    child: Text(item.name),
                  ),
                )
                .toList(),
          ),
      ],
    );
  }

  void _setCheckboxField(String fieldName, List<int> values) {
    _formKey.currentState?.fields[fieldName]?.didChange(values);
  }

  Widget _buildActions() {
    return Row(
      mainAxisAlignment: MainAxisAlignment.end,
      children: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: const Text('Odustani'),
        ),
        const SizedBox(width: 12),
        ElevatedButton(
          onPressed: _save,
          child: Text(_isEditing ? 'Spremi' : 'Dodaj'),
        ),
      ],
    );
  }

  Future<void> _save() async {
    if (!(_formKey.currentState?.saveAndValidate() ?? false)) return;

    final formData = _formKey.currentState!.value;
    final hranaIds = _toIntList(formData['hranaIds']);
    final piceIds = _toIntList(formData['piceIds']);

    if (hranaIds.isEmpty && piceIds.isEmpty) {
      alertBox(context, 'Greška', 'Odaberite barem jednu hranu ili piće.');
      return;
    }

    final request = {
      'naziv': formData['naziv'],
      'opis': formData['opis'] ?? '',
      'cijena': double.tryParse(
            (formData['cijena']?.toString() ?? '').replaceAll(',', '.'),
          ) ??
          0,
      'isActive': formData['isActive'] ?? true,
      'hranaIds': hranaIds,
      'piceIds': piceIds,
    };

    try {
      if (_isEditing) {
        await _menuProvider.update(widget.menu!.id, request);
        if (!mounted) return;
        Navigator.of(context).pop('updated');
      } else {
        await _menuProvider.insert(request);
        if (!mounted) return;
        Navigator.of(context).pop('created');
      }
    } on Exception catch (e) {
      if (mounted) alertBox(context, 'Greška', e.toString());
    }
  }

  List<int> _toIntList(dynamic value) {
    if (value is! List) return [];
    return value.map((e) => e is int ? e : int.parse(e.toString())).toList();
  }
}
