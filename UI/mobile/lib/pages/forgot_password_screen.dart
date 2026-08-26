import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_rimac/providers/auth_provider.dart';
import 'package:svadbeni_salon_rimac/utils/routes.dart';

class ForgotPasswordScreen extends StatefulWidget {
  const ForgotPasswordScreen({super.key});

  @override
  State<ForgotPasswordScreen> createState() => _ForgotPasswordScreenState();
}

class _ForgotPasswordScreenState extends State<ForgotPasswordScreen> {
  final _emailOrUsernameController = TextEditingController();
  bool _isLoading = false;
  String? _errorMessage;

  @override
  void dispose() {
    _emailOrUsernameController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    final value = _emailOrUsernameController.text.trim();
    if (value.isEmpty) {
      setState(() => _errorMessage = 'Unesite email ili korisničko ime.');
      return;
    }

    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    try {
      final message = await context.read<AuthProvider>().forgotPassword(value);
      if (!mounted) return;
      Navigator.of(context).pushNamed(
        AppRoutes.resetPassword,
        arguments: value,
      );
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(message)),
      );
    } catch (e) {
      setState(() => _errorMessage = e.toString().replaceFirst('Exception: ', ''));
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Zaboravljena lozinka'),
        backgroundColor: const Color.fromRGBO(135, 82, 82, 0.9),
        foregroundColor: Colors.white,
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const Text(
                'Unesite email ili korisničko ime. Poslaćemo vam kod za reset lozinke.',
              ),
              const SizedBox(height: 20),
              TextField(
                controller: _emailOrUsernameController,
                keyboardType: TextInputType.emailAddress,
                decoration: InputDecoration(
                  hintText: 'Email ili korisničko ime',
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(20),
                  ),
                ),
              ),
              if (_errorMessage != null) ...[
                const SizedBox(height: 12),
                Text(_errorMessage!, style: const TextStyle(color: Colors.red)),
              ],
              const SizedBox(height: 24),
              SizedBox(
                height: 48,
                child: ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: const Color.fromRGBO(135, 82, 82, 0.9),
                    foregroundColor: Colors.white,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(20),
                    ),
                  ),
                  onPressed: _isLoading ? null : _submit,
                  child: _isLoading
                      ? const SizedBox(
                          width: 22,
                          height: 22,
                          child: CircularProgressIndicator(
                            strokeWidth: 2,
                            color: Colors.white,
                          ),
                        )
                      : const Text('Pošalji kod'),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
