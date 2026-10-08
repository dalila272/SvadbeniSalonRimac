import 'package:flutter/material.dart';
import 'package:svadbeni_salon_rimac/constants/colors.dart';
import 'package:svadbeni_salon_rimac/providers/recommendations_provider.dart';
import 'package:svadbeni_salon_rimac/utils/routes.dart';

class InterestsScreen extends StatefulWidget {
  final bool fromRegistration;

  const InterestsScreen({super.key, this.fromRegistration = false});

  @override
  State<InterestsScreen> createState() => _InterestsScreenState();
}

class _InterestsScreenState extends State<InterestsScreen> {
  final _provider = RecommendationsProvider();
  final Set<int> _selected = {};

  bool _loading = true;
  bool _saving = false;
  String? _error;
  List<GenreOption> _genres = [];

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final genres = await _provider.fetchGenres();
      final mine = await _provider.fetchMyInterestIds();
      if (!mounted) return;
      setState(() {
        _genres = genres;
        _selected
          ..clear()
          ..addAll(mine);
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _error = e.toString().replaceFirst('Exception: ', '');
        _loading = false;
      });
    }
  }

  Future<void> _save() async {
    if (_selected.isEmpty) {
      setState(() => _error = 'Odaberite barem jedan žanr.');
      return;
    }

    setState(() {
      _saving = true;
      _error = null;
    });

    try {
      await _provider.saveInterests(_selected.toList());
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Interesi su sačuvani.')),
      );
      if (widget.fromRegistration) {
        Navigator.of(context).pushReplacementNamed(AppRoutes.home);
      } else {
        Navigator.of(context).pop(true);
      }
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _error = e.toString().replaceFirst('Exception: ', '');
        _saving = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Vaši interesi'),
        automaticallyImplyLeading: !widget.fromRegistration,
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : Padding(
              padding: const EdgeInsets.all(20),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Text(
                    'Odaberite žanrove koji vam se sviđaju. Na osnovu toga ćemo preporučiti pakete salona.',
                    style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                          color: Colors.grey[700],
                        ),
                  ),
                  const SizedBox(height: 16),
                  if (_error != null) ...[
                    Text(
                      _error!,
                      style: const TextStyle(color: AppColors.errorColor),
                    ),
                    const SizedBox(height: 12),
                  ],
                  Expanded(
                    child: SingleChildScrollView(
                      child: Wrap(
                        spacing: 8,
                        runSpacing: 8,
                        children: _genres.map((g) {
                          final selected = _selected.contains(g.id);
                          return FilterChip(
                            label: Text(g.name),
                            selected: selected,
                            selectedColor:
                                AppColors.accentColor.withValues(alpha: 0.8),
                            checkmarkColor: AppColors.primaryColor,
                            onSelected: (value) {
                              setState(() {
                                if (value) {
                                  _selected.add(g.id);
                                } else {
                                  _selected.remove(g.id);
                                }
                              });
                            },
                          );
                        }).toList(),
                      ),
                    ),
                  ),
                  const SizedBox(height: 12),
                  ElevatedButton(
                    onPressed: _saving ? null : _save,
                    child: _saving
                        ? const SizedBox(
                            height: 20,
                            width: 20,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          )
                        : Text(
                            widget.fromRegistration
                                ? 'Sačuvaj i nastavi'
                                : 'Sačuvaj interese',
                          ),
                  ),
                  if (widget.fromRegistration) ...[
                    const SizedBox(height: 8),
                    TextButton(
                      onPressed: _saving
                          ? null
                          : () => Navigator.of(context)
                              .pushReplacementNamed(AppRoutes.home),
                      child: const Text('Preskoči za sada'),
                    ),
                  ],
                ],
              ),
            ),
    );
  }
}
