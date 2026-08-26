import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_desktop/layouts/master_screen.dart';
import 'package:svadbeni_salon_desktop/models/salon_decoration.dart';
import 'package:svadbeni_salon_desktop/providers/decoration_provider.dart';
import 'package:svadbeni_salon_desktop/utils/input_validators.dart';
import 'package:svadbeni_salon_desktop/utils/utils_widgets.dart';

class DecorationDetailsScreen extends StatefulWidget {
  final SalonDecoration? item;

  const DecorationDetailsScreen({super.key, this.item});

  @override
  State<DecorationDetailsScreen> createState() =>
      _DecorationDetailsScreenState();
}

class _DecorationDetailsScreenState extends State<DecorationDetailsScreen> {
  static const _brandColor = Color(0xFF875252);
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _descController = TextEditingController();
  final _priceController = TextEditingController();
  late DecorationProvider _provider;
  bool _isActive = true;
  bool _isSaving = false;

  bool get _isEditing => widget.item != null;

  @override
  void initState() {
    super.initState();
    _provider = context.read<DecorationProvider>();
    if (_isEditing) {
      final d = widget.item!;
      _nameController.text = d.name;
      _descController.text = d.description;
      _priceController.text = d.price;
      _isActive = d.isActive;
    }
  }

  @override
  void dispose() {
    _nameController.dispose();
    _descController.dispose();
    _priceController.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _isSaving = true);
    try {
      final payload = {
        'naziv': _nameController.text.trim(),
        'opis': _descController.text.trim(),
        'cijena': double.parse(_priceController.text.replaceAll(',', '.')),
        'isActive': _isActive,
      };
      if (_isEditing) {
        await _provider.update(widget.item!.id, payload);
        if (mounted) Navigator.pop(context, 'updated');
      } else {
        await _provider.insert(payload);
        if (mounted) Navigator.pop(context, 'created');
      }
    } on Exception catch (e) {
      if (mounted) alertBox(context, 'Greška', e.toString());
    } finally {
      if (mounted) setState(() => _isSaving = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: _isEditing ? 'Uredi dekoraciju' : 'Nova dekoracija',
      child: Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 520),
            child: Form(
              key: _formKey,
              autovalidateMode: AutovalidateMode.onUserInteraction,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  TextFormField(
                    controller: _nameController,
                    decoration: const InputDecoration(
                      labelText: 'Naziv',
                      helperText: 'Najviše 200 karaktera',
                      border: OutlineInputBorder(),
                    ),
                    validator: (v) => InputValidators.requiredName(
                      v,
                      label: 'Naziv dekoracije',
                    ),
                  ),
                  const SizedBox(height: 16),
                  TextFormField(
                    controller: _descController,
                    decoration: const InputDecoration(
                      labelText: 'Opis',
                      helperText: 'Opciono — najviše 2000 karaktera',
                      border: OutlineInputBorder(),
                    ),
                    maxLines: 3,
                    validator: InputValidators.optionalDescription,
                  ),
                  const SizedBox(height: 16),
                  TextFormField(
                    controller: _priceController,
                    decoration: const InputDecoration(
                      labelText: 'Cijena (KM)',
                      helperText: 'Broj ≥ 0, npr. 150.00',
                      prefixIcon: Icon(Icons.payments_outlined),
                      border: OutlineInputBorder(),
                    ),
                    keyboardType:
                        const TextInputType.numberWithOptions(decimal: true),
                    validator: (v) => InputValidators.price(v, allowZero: true),
                  ),
                  const SizedBox(height: 16),
                  SwitchListTile(
                    title: const Text('Aktivna'),
                    value: _isActive,
                    onChanged: (v) => setState(() => _isActive = v),
                  ),
                  const SizedBox(height: 24),
                  Align(
                    alignment: Alignment.centerRight,
                    child: ElevatedButton(
                      style: ElevatedButton.styleFrom(
                        backgroundColor: _brandColor,
                        foregroundColor: Colors.white,
                      ),
                      onPressed: _isSaving ? null : _save,
                      child: _isSaving
                          ? const SizedBox(
                              width: 24,
                              height: 24,
                              child: CircularProgressIndicator(
                                strokeWidth: 2,
                                color: Colors.white,
                              ),
                            )
                          : const Text('Spremi'),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
