import 'package:flutter/material.dart';

class FormUx {
  static const String newestFirst = 'Id descending';

  static Future<bool> confirm(
    BuildContext context, {
    required String title,
    required String message,
    String confirmLabel = 'Potvrdi',
    String cancelLabel = 'Odustani',
  }) async {
    final result = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Row(
          children: [
            Expanded(child: Text(title)),
            IconButton(
              icon: const Icon(Icons.close),
              tooltip: 'Zatvori',
              onPressed: () => Navigator.pop(ctx, false),
            ),
          ],
        ),
        content: Text(message),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: Text(cancelLabel),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: Text(confirmLabel),
          ),
        ],
      ),
    );
    return result == true;
  }

  static Future<String?> confirmWithReason(
    BuildContext context, {
    required String title,
    required String message,
    String confirmLabel = 'Otkaži',
    String helperText = 'Obavezno — navedite razlog (min. 3 karaktera)',
  }) async {
    final controller = TextEditingController();
    final formKey = GlobalKey<FormState>();
    final result = await showDialog<String>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Row(
          children: [
            Expanded(child: Text(title)),
            IconButton(
              icon: const Icon(Icons.close),
              tooltip: 'Zatvori',
              onPressed: () => Navigator.pop(ctx),
            ),
          ],
        ),
        content: Form(
          key: formKey,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Text(message),
              const SizedBox(height: 12),
              TextFormField(
                controller: controller,
                autofocus: true,
                maxLines: 3,
                decoration: InputDecoration(
                  labelText: 'Razlog',
                  helperText: helperText,
                  border: const OutlineInputBorder(),
                ),
                validator: (v) {
                  final t = v?.trim() ?? '';
                  if (t.length < 3) {
                    return 'Unesite razlog (najmanje 3 karaktera).';
                  }
                  return null;
                },
              ),
            ],
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('Odustani'),
          ),
          ElevatedButton(
            onPressed: () {
              if (!(formKey.currentState?.validate() ?? false)) return;
              Navigator.pop(ctx, controller.text.trim());
            },
            child: Text(confirmLabel),
          ),
        ],
      ),
    );
    controller.dispose();
    return result;
  }

  static Future<bool> ensurePrerequisites(
    BuildContext context, {
    required List<String> missing,
  }) async {
    if (missing.isEmpty) return true;
    await showDialog<void>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Row(
          children: [
            const Expanded(child: Text('Nedostaju preduslovi')),
            IconButton(
              icon: const Icon(Icons.close),
              tooltip: 'Zatvori',
              onPressed: () => Navigator.pop(ctx),
            ),
          ],
        ),
        content: Text(
          'Prije unosa potrebno je dodati:\n\n• ${missing.join('\n• ')}',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('U redu'),
          ),
        ],
      ),
    );
    return false;
  }

  static Widget disabledHint({
    required bool enabled,
    required String reason,
    required Widget child,
  }) {
    if (enabled) return child;
    return Tooltip(
      message: reason,
      child: child,
    );
  }
}
