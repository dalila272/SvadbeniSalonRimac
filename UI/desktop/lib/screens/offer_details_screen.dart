import 'package:svadbeni_salon_desktop/layouts/master_screen.dart';
import 'package:svadbeni_salon_desktop/models/salon_decoration.dart';
import 'package:svadbeni_salon_desktop/models/menu.dart';
import 'package:svadbeni_salon_desktop/models/musician.dart';
import 'package:svadbeni_salon_desktop/models/offer.dart';
import 'package:svadbeni_salon_desktop/models/offer_detail.dart';
import 'package:svadbeni_salon_desktop/models/search_result.dart';
import 'package:svadbeni_salon_desktop/providers/decoration_provider.dart';
import 'package:svadbeni_salon_desktop/providers/menu_provider.dart';
import 'package:svadbeni_salon_desktop/providers/musician_provider.dart';
import 'package:svadbeni_salon_desktop/providers/offer_provider.dart';
import 'package:svadbeni_salon_desktop/utils/utils_widgets.dart';
import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';
import 'package:provider/provider.dart';

class OfferDetailsScreen extends StatefulWidget {
  final Offer? offer;

  const OfferDetailsScreen({super.key, this.offer});

  @override
  State<OfferDetailsScreen> createState() => _OfferDetailsScreenState();
}

class _OfferDetailsScreenState extends State<OfferDetailsScreen> {
  final _formKey = GlobalKey<FormBuilderState>();
  Map<String, dynamic> _initialValue = {};

  late OfferProvider _offerProvider;
  late MenuProvider _menuProvider;
  late MusicianProvider _musicianProvider;
  late DecorationProvider _decorationProvider;

  SearchResult<Menu>? _menus;
  SearchResult<Musician>? _musicians;
  SearchResult<SalonDecoration>? _decorations;

  bool isLoading = true;

  @override
  void initState() {
    super.initState();
    _offerProvider = context.read<OfferProvider>();
    _menuProvider = context.read<MenuProvider>();
    _musicianProvider = context.read<MusicianProvider>();
    _decorationProvider = context.read<DecorationProvider>();
    _loadForm();
  }

  Future<void> _loadForm() async {
    try {
      final menusFuture = _menuProvider.get(filter: {
        'isActive': true,
        'pageSize': 100,
        'includeTotalCount': true,
      });
      final musiciansFuture = _musicianProvider.get(filter: {
        'isActive': true,
        'pageSize': 100,
        'includeTotalCount': true,
      });
      final decorationsFuture = _decorationProvider.get(filter: {
        'isActive': true,
        'pageSize': 100,
        'includeTotalCount': true,
      });
      final menus = await menusFuture;
      final musicians = await musiciansFuture;
      final decorations = await decorationsFuture;

      OfferDetail? detail;
      if (widget.offer != null) {
        detail = await _offerProvider.getDetail(widget.offer!.id);
      }

      if (!mounted) return;

      setState(() {
        _menus = menus;
        _musicians = musicians;
        _decorations = decorations;
        _initialValue = {
          'naziv': detail?.name ?? '',
          'opis': detail?.description ?? '',
          'cijena': detail?.price ?? '',
          'meniId': detail?.menuId ?? _menus?.items?.firstOrNull?.id,
          'isActive': detail?.isActive ?? true,
          'muzicarIds': detail?.musicianIds ?? <int>[],
          'dekoracijaIds': detail?.decorationIds ?? <int>[],
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
      title: widget.offer == null ? 'Nova ponuda' : 'Uredi ponudu',
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

  Widget _buildCheckboxSection({
    required String title,
    required String fieldName,
    required List<_CheckboxItem> items,
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
        FormBuilderCheckboxGroup<int>(
          name: fieldName,
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
                errorText: 'Naziv ponude je obavezan.',
              ),
              FormBuilderValidators.maxLength(
                200,
                errorText: 'Naziv ne smije imati više od 200 karaktera.',
              ),
            ]),
          ),
          const SizedBox(height: 16),
          FormBuilderTextField(
            name: 'cijena',
            decoration: const InputDecoration(
              labelText: 'Cijena (KM)',
              helperText: 'Broj veći od 0, npr. 1500.00',
              prefixIcon: Icon(Icons.payments_outlined),
              border: OutlineInputBorder(),
            ),
            keyboardType: const TextInputType.numberWithOptions(decimal: true),
            validator: (value) {
              final raw = value?.toString().trim() ?? '';
              if (raw.isEmpty) return 'Cijena je obavezna.';
              final parsed = double.tryParse(raw.replaceAll(',', '.'));
              if (parsed == null) {
                return 'Unesite validan broj za cijenu (npr. 1500.00).';
              }
              if (parsed <= 0) return 'Cijena mora biti veća od 0.';
              return null;
            },
          ),
          const SizedBox(height: 16),
          FormBuilderTextField(
            name: 'opis',
            decoration: const InputDecoration(
              labelText: 'Opis',
              border: OutlineInputBorder(),
            ),
            maxLines: 4,
            validator: FormBuilderValidators.maxLength(
              2000,
              errorText: 'Opis ne smije imati više od 2000 karaktera.',
            ),
          ),
          const SizedBox(height: 16),
          FormBuilderDropdown<int>(
            name: 'meniId',
            decoration: const InputDecoration(
              labelText: 'Meni',
              border: OutlineInputBorder(),
            ),
            validator: FormBuilderValidators.required(
              errorText: 'Odaberite meni za ponudu.',
            ),
            items: (_menus?.items ?? [])
                .map(
                  (m) => DropdownMenuItem(value: m.id, child: Text(m.name)),
                )
                .toList(),
          ),
          const SizedBox(height: 16),
          _buildCheckboxSection(
            title: 'Muzičari',
            fieldName: 'muzicarIds',
            items: (_musicians?.items ?? [])
                .map((m) => _CheckboxItem(m.id, m.name))
                .toList(),
          ),
          const SizedBox(height: 16),
          _buildCheckboxSection(
            title: 'Dekoracije',
            fieldName: 'dekoracijaIds',
            items: (_decorations?.items ?? [])
                .map((d) => _CheckboxItem(d.id, d.name))
                .toList(),
          ),
          const SizedBox(height: 16),
          FormBuilderCheckbox(
            name: 'isActive',
            title: const Text('Aktivna ponuda'),
          ),
        ],
      ),
    );
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
          child: const Text('Spremi'),
        ),
      ],
    );
  }

  Future<void> _save() async {
    if (!(_formKey.currentState?.saveAndValidate() ?? false)) return;

    final formData = _formKey.currentState!.value;
    final cijenaRaw = formData['cijena']?.toString().trim() ?? '';
    final cijena = double.tryParse(cijenaRaw.replaceAll(',', '.')) ?? 0;
    final request = {
      'naziv': (formData['naziv'] as String?)?.trim(),
      'opis': (formData['opis'] as String?)?.trim() ?? '',
      'cijena': cijena,
      'meniId': formData['meniId'],
      'isActive': formData['isActive'] ?? true,
      'muzicarIds': _toIntList(formData['muzicarIds']),
      'dekoracijaIds': _toIntList(formData['dekoracijaIds']),
    };

    try {
      if (widget.offer != null) {
        await _offerProvider.update(widget.offer!.id, request);
        if (!mounted) return;
        Navigator.of(context).pop('updated');
      } else {
        await _offerProvider.insert(request);
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

class _CheckboxItem {
  final int id;
  final String name;
  const _CheckboxItem(this.id, this.name);
}
