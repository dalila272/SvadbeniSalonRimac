import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_desktop/layouts/master_screen.dart';
import 'package:svadbeni_salon_desktop/models/genre.dart';
import 'package:svadbeni_salon_desktop/providers/genre_provider.dart';
import 'package:svadbeni_salon_desktop/utils/input_validators.dart';
import 'package:svadbeni_salon_desktop/utils/utils_widgets.dart';

class GenreDetailsScreen extends StatefulWidget {
  final Genre? genre;

  const GenreDetailsScreen({super.key, this.genre});

  @override
  State<GenreDetailsScreen> createState() => _GenreDetailsScreenState();
}

class _GenreDetailsScreenState extends State<GenreDetailsScreen> {
  static const _brandColor = Color(0xFF875252);
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  late GenreProvider _provider;
  bool _isActive = true;
  bool _isSaving = false;

  bool get _isEditing => widget.genre != null;

  @override
  void initState() {
    super.initState();
    _provider = context.read<GenreProvider>();
    if (_isEditing) {
      _nameController.text = widget.genre!.name;
      _isActive = widget.genre!.isActive;
    }
  }

  @override
  void dispose() {
    _nameController.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _isSaving = true);
    try {
      final payload = {
        'naziv': _nameController.text.trim(),
        'isActive': _isActive,
      };
      if (_isEditing) {
        await _provider.update(widget.genre!.id, payload);
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
      title: _isEditing ? 'Uredi žanr' : 'Novi žanr',
      child: Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 480),
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
                      helperText: 'Najviše 100 karaktera',
                      border: OutlineInputBorder(),
                    ),
                    validator: (v) => InputValidators.requiredName(
                      v,
                      label: 'Naziv žanra',
                      maxLength: 100,
                    ),
                  ),
                  const SizedBox(height: 16),
                  SwitchListTile(
                    title: const Text('Aktivan'),
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
