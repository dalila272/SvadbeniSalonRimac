import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_desktop/layouts/master_screen.dart';
import 'package:svadbeni_salon_desktop/models/genre.dart';
import 'package:svadbeni_salon_desktop/models/musician.dart';
import 'package:svadbeni_salon_desktop/providers/genre_provider.dart';
import 'package:svadbeni_salon_desktop/providers/musician_provider.dart';
import 'package:svadbeni_salon_desktop/utils/input_validators.dart';
import 'package:svadbeni_salon_desktop/utils/utils_widgets.dart';

class MusicianDetailsScreen extends StatefulWidget {
  final Musician? musician;

  const MusicianDetailsScreen({super.key, this.musician});

  @override
  State<MusicianDetailsScreen> createState() => _MusicianDetailsScreenState();
}

class _MusicianDetailsScreenState extends State<MusicianDetailsScreen> {
  static const _brandColor = Color(0xFF875252);
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _descController = TextEditingController();
  late MusicianProvider _provider;
  late GenreProvider _genreProvider;
  List<Genre> _genres = [];
  List<int> _selectedGenreIds = [];
  bool _isActive = true;
  bool _isLoading = true;
  bool _isSaving = false;

  bool get _isEditing => widget.musician != null;

  @override
  void initState() {
    super.initState();
    _provider = context.read<MusicianProvider>();
    _genreProvider = context.read<GenreProvider>();
    _loadForm();
  }

  @override
  void dispose() {
    _nameController.dispose();
    _descController.dispose();
    super.dispose();
  }

  Future<void> _loadForm() async {
    try {
      final genresResult = await _genreProvider.get(
        filter: {'pageSize': 100, 'isActive': true},
      );
      Musician? detail = widget.musician;
      if (_isEditing) {
        detail = await _provider.getById(widget.musician!.id);
      }
      if (!mounted) return;
      setState(() {
        _genres = genresResult.items ?? [];
        if (detail != null) {
          _nameController.text = detail.name;
          _descController.text = detail.description;
          _isActive = detail.isActive;
          _selectedGenreIds = List.from(detail.genreIds);
        }
        _isLoading = false;
      });
    } catch (e) {
      if (mounted) {
        setState(() => _isLoading = false);
        alertBox(context, 'Greška', e.toString());
      }
    }
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _isSaving = true);
    try {
      final payload = {
        'naziv': _nameController.text.trim(),
        'opis': _descController.text.trim(),
        'isActive': _isActive,
        'zanrIds': _selectedGenreIds,
      };
      if (_isEditing) {
        await _provider.update(widget.musician!.id, payload);
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

  void _toggleGenre(int id) {
    setState(() {
      if (_selectedGenreIds.contains(id)) {
        _selectedGenreIds.remove(id);
      } else {
        _selectedGenreIds.add(id);
      }
    });
  }

  Future<void> _addGenreModal() async {
    final nameController = TextEditingController();
    final created = await showDialog<Genre>(
      context: context,
      builder: (ctx) {
        final formKey = GlobalKey<FormState>();
        return AlertDialog(
          title: Row(
            children: [
              const Expanded(child: Text('Novi žanr')),
              IconButton(
                icon: const Icon(Icons.close),
                tooltip: 'Zatvori',
                onPressed: () => Navigator.pop(ctx),
              ),
            ],
          ),
          content: Form(
            key: formKey,
            child: TextFormField(
              controller: nameController,
              autofocus: true,
              decoration: const InputDecoration(
                labelText: 'Naziv žanra',
                helperText: 'Najviše 100 karaktera',
                border: OutlineInputBorder(),
              ),
              validator: (v) => InputValidators.requiredName(
                v,
                label: 'Naziv žanra',
                maxLength: 100,
              ),
            ),
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(ctx),
              child: const Text('Odustani'),
            ),
            ElevatedButton(
              onPressed: () async {
                if (!(formKey.currentState?.validate() ?? false)) return;
                try {
                  final g = await _genreProvider.insert({
                    'naziv': nameController.text.trim(),
                    'isActive': true,
                  });
                  if (ctx.mounted) Navigator.pop(ctx, g);
                } on Exception catch (e) {
                  if (ctx.mounted) alertBox(ctx, 'Greška', e.toString());
                }
              },
              child: const Text('Dodaj'),
            ),
          ],
        );
      },
    );
    nameController.dispose();
    if (created == null || !mounted) return;
    setState(() {
      _genres = [..._genres, created];
      if (!_selectedGenreIds.contains(created.id)) {
        _selectedGenreIds.add(created.id);
      }
    });
    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Žanr "${created.name}" je dodan.')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: _isEditing ? 'Uredi muzičara' : 'Novi muzičar',
      child: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : Center(
              child: SingleChildScrollView(
                padding: const EdgeInsets.all(24),
                child: ConstrainedBox(
                  constraints: const BoxConstraints(maxWidth: 560),
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
                            label: 'Naziv muzičara',
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
                        Row(
                          children: [
                            Text(
                              'Žanrovi',
                              style: Theme.of(context).textTheme.titleSmall,
                            ),
                            const Spacer(),
                            TextButton.icon(
                              onPressed: _isSaving ? null : _addGenreModal,
                              icon: const Icon(Icons.add, size: 18),
                              label: const Text('Dodaj žanr'),
                            ),
                          ],
                        ),
                        Text(
                          'Opciono — odaberite ili dodajte žanr bez napuštanja forme',
                          style: TextStyle(
                            color: Colors.grey[600],
                            fontSize: 12,
                          ),
                        ),
                        const SizedBox(height: 8),
                        if (_genres.isEmpty)
                          const Text(
                            'Nema aktivnih žanrova. Koristite „Dodaj žanr“.',
                          )
                        else
                          Wrap(
                            spacing: 8,
                            runSpacing: 8,
                            children: _genres
                                .map(
                                  (g) => FilterChip(
                                    label: Text(g.name),
                                    selected: _selectedGenreIds.contains(g.id),
                                    onSelected: (_) => _toggleGenre(g.id),
                                  ),
                                )
                                .toList(),
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
