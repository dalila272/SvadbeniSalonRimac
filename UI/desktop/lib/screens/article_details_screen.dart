import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_desktop/layouts/master_screen.dart';
import 'package:svadbeni_salon_desktop/models/article_item.dart';
import 'package:svadbeni_salon_desktop/providers/article_provider.dart';
import 'package:svadbeni_salon_desktop/utils/utils_widgets.dart';

class ArticleDetailsScreen extends StatefulWidget {
  final Article? article;

  const ArticleDetailsScreen({super.key, this.article});

  @override
  State<ArticleDetailsScreen> createState() => _ArticleDetailsScreenState();
}

class _ArticleDetailsScreenState extends State<ArticleDetailsScreen> {
  final _formKey = GlobalKey<FormBuilderState>();
  late ArticleProvider _provider;

  bool get _isEditing => widget.article != null;

  @override
  void initState() {
    super.initState();
    _provider = context.read<ArticleProvider>();
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: _isEditing ? 'Uredi artikal' : 'Novi artikal',
      child: Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 520),
            child: Column(
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
      initialValue: {
        'naziv': widget.article?.name ?? '',
        'tip': widget.article?.tip ?? 1,
        'cijena': widget.article?.price ?? '',
        'isActive': widget.article?.isActive ?? true,
      },
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
                errorText: 'Naziv artikla je obavezan.',
              ),
              FormBuilderValidators.maxLength(
                200,
                errorText: 'Naziv ne smije imati više od 200 karaktera.',
              ),
            ]),
          ),
          const SizedBox(height: 16),
          FormBuilderDropdown<int>(
            name: 'tip',
            decoration: const InputDecoration(
              labelText: 'Tip',
              helperText: 'Odaberite: Hrana ili Piće',
              border: OutlineInputBorder(),
            ),
            items: const [
              DropdownMenuItem(value: 1, child: Text('Hrana')),
              DropdownMenuItem(value: 2, child: Text('Piće')),
            ],
            validator: FormBuilderValidators.required(
              errorText: 'Tip artikla je obavezan (Hrana ili Piće).',
            ),
          ),
          const SizedBox(height: 16),
          FormBuilderTextField(
            name: 'cijena',
            decoration: const InputDecoration(
              labelText: 'Cijena (KM)',
              helperText: 'Broj veći od 0, npr. 15.00',
              prefixIcon: Icon(Icons.payments_outlined),
              border: OutlineInputBorder(),
            ),
            keyboardType: const TextInputType.numberWithOptions(decimal: true),
            validator: (value) {
              final raw = value?.toString().trim() ?? '';
              if (raw.isEmpty) return 'Cijena je obavezna.';
              final parsed = double.tryParse(raw.replaceAll(',', '.'));
              if (parsed == null) {
                return 'Unesite validan broj za cijenu (npr. 15.00).';
              }
              if (parsed <= 0) return 'Cijena mora biti veća od 0.';
              return null;
            },
          ),
          const SizedBox(height: 16),
          FormBuilderCheckbox(
            name: 'isActive',
            title: const Text('Aktivan artikal'),
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
          child: Text(_isEditing ? 'Spremi' : 'Dodaj'),
        ),
      ],
    );
  }

  Future<void> _save() async {
    if (!(_formKey.currentState?.saveAndValidate() ?? false)) return;

    final formData = _formKey.currentState!.value;
    final request = {
      'naziv': formData['naziv'],
      'tip': formData['tip'],
      'cijena': double.tryParse(
            (formData['cijena']?.toString() ?? '').replaceAll(',', '.'),
          ) ??
          0,
      'isActive': formData['isActive'] ?? true,
    };

    try {
      if (_isEditing) {
        await _provider.update(widget.article!.id, request);
        if (!mounted) return;
        Navigator.of(context).pop('updated');
      } else {
        await _provider.insert(request);
        if (!mounted) return;
        Navigator.of(context).pop('created');
      }
    } on Exception catch (e) {
      if (mounted) alertBox(context, 'Greška', e.toString());
    }
  }
}
