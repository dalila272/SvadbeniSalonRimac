import 'package:flutter/material.dart';

/// Dialog for cancel reason without Form/GlobalKey (avoids hot-reload key crashes).
Future<String?> showCancelReasonDialog({
  required BuildContext context,
  required String title,
  required String message,
  String confirmLabel = 'Otkaži',
}) {
  return showDialog<String>(
    context: context,
    builder: (ctx) => _CancelReasonDialog(
      title: title,
      message: message,
      confirmLabel: confirmLabel,
    ),
  );
}

class _CancelReasonDialog extends StatefulWidget {
  final String title;
  final String message;
  final String confirmLabel;

  const _CancelReasonDialog({
    required this.title,
    required this.message,
    required this.confirmLabel,
  });

  @override
  State<_CancelReasonDialog> createState() => _CancelReasonDialogState();
}

class _CancelReasonDialogState extends State<_CancelReasonDialog> {
  final _controller = TextEditingController();
  String? _error;

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _submit() {
    final text = _controller.text.trim();
    if (text.length < 3) {
      setState(() => _error = 'Najmanje 3 karaktera');
      return;
    }
    Navigator.pop(context, text);
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text(widget.title),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Text(widget.message),
          const SizedBox(height: 12),
          TextField(
            controller: _controller,
            maxLines: 2,
            autofocus: true,
            decoration: InputDecoration(
              labelText: 'Razlog',
              border: const OutlineInputBorder(),
              errorText: _error,
            ),
            onChanged: (_) {
              if (_error != null) setState(() => _error = null);
            },
            onSubmitted: (_) => _submit(),
          ),
        ],
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context),
          child: const Text('Odustani'),
        ),
        ElevatedButton(
          onPressed: _submit,
          child: Text(widget.confirmLabel),
        ),
      ],
    );
  }
}
