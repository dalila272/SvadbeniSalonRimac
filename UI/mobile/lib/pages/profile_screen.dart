import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_rimac/constants/colors.dart';
import 'package:svadbeni_salon_rimac/models/user_profile.dart';
import 'package:svadbeni_salon_rimac/providers/auth_provider.dart';
import 'package:svadbeni_salon_rimac/utils/input_validators.dart';
import 'package:svadbeni_salon_rimac/utils/master_screen.dart';
import 'package:svadbeni_salon_rimac/utils/util.dart';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  final _formKey = GlobalKey<FormState>();
  final _firstNameController = TextEditingController();
  final _lastNameController = TextEditingController();
  final _emailController = TextEditingController();
  final _usernameController = TextEditingController();
  final _phoneController = TextEditingController();
  final _currentPasswordController = TextEditingController();
  final _newPasswordController = TextEditingController();
  final _confirmPasswordController = TextEditingController();

  UserProfile? _profile;
  String? _imageBase64;
  MemoryImage? _avatarProvider;
  bool _isLoading = true;
  bool _isSaving = false;
  bool _changePassword = false;
  String? _formError;

  void _setImageBase64(String? value) {
    _imageBase64 = value;
    _avatarProvider = null;
    if (value == null || value.isEmpty) return;
    try {
      final cleaned = value.contains(',') ? value.split(',').last : value;
      _avatarProvider = MemoryImage(dataFromBase64String(cleaned));
    } catch (_) {
      _avatarProvider = null;
    }
  }

  @override
  void initState() {
    super.initState();
    _loadProfile();
  }

  @override
  void dispose() {
    _firstNameController.dispose();
    _lastNameController.dispose();
    _emailController.dispose();
    _usernameController.dispose();
    _phoneController.dispose();
    _currentPasswordController.dispose();
    _newPasswordController.dispose();
    _confirmPasswordController.dispose();
    super.dispose();
  }

  Future<void> _loadProfile() async {
    setState(() {
      _isLoading = true;
      _formError = null;
    });

    try {
      final profile = await context.read<AuthProvider>().getProfile();
      _firstNameController.text = profile.firstName;
      _lastNameController.text = profile.lastName;
      _emailController.text = profile.email;
      _usernameController.text = profile.username;
      _phoneController.text = profile.phoneNumber ?? '';
      setState(() {
        _profile = profile;
        _setImageBase64(profile.profileImageBase64);
        _isLoading = false;
      });
    } catch (e) {
      setState(() {
        _isLoading = false;
        _formError = e.toString().replaceFirst('Exception: ', '');
      });
    }
  }

  Future<void> _pickImage() async {
    final picker = ImagePicker();
    final file = await picker.pickImage(
      source: ImageSource.gallery,
      maxWidth: 800,
      maxHeight: 800,
      imageQuality: 75,
    );
    if (file == null) return;

    final bytes = await file.readAsBytes();
    setState(() => _setImageBase64(base64Encode(bytes)));
  }

  Future<void> _save() async {
    setState(() => _formError = null);
    if (!(_formKey.currentState?.validate() ?? false)) return;

    setState(() => _isSaving = true);

    try {
      final auth = context.read<AuthProvider>();
      final updated = await auth.updateProfile({
        'firstName': _firstNameController.text.trim(),
        'lastName': _lastNameController.text.trim(),
        'email': _emailController.text.trim(),
        'username': _usernameController.text.trim(),
        'phoneNumber': _phoneController.text.trim().isEmpty
            ? null
            : _phoneController.text.trim(),
        'isActive': _profile?.isActive ?? true,
        'profileImageBase64': _imageBase64,
      });

      if (_changePassword) {
        final message = await auth.changePassword(
          currentPassword: _currentPasswordController.text,
          newPassword: _newPasswordController.text,
          confirmNewPassword: _confirmPasswordController.text,
        );
        _currentPasswordController.clear();
        _newPasswordController.clear();
        _confirmPasswordController.clear();
        setState(() => _changePassword = false);
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text(message)),
          );
        }
      } else if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Profil je uspješno sačuvan.')),
        );
      }

      setState(() {
        _profile = updated;
        _setImageBase64(updated.profileImageBase64);
      });
      await auth.refreshProfileCache();
    } catch (e) {
      setState(() {
        _formError = e.toString().replaceFirst('Exception: ', '');
      });
    } finally {
      if (mounted) setState(() => _isSaving = false);
    }
  }

  Widget _buildAvatar() {
    final provider = _avatarProvider;

    return Stack(
      alignment: Alignment.bottomRight,
      children: [
        CircleAvatar(
          radius: 56,
          backgroundColor: AppColors.accentColor,
          backgroundImage: provider,
          child: provider == null
              ? const Icon(Icons.person, size: 56, color: AppColors.primaryColor)
              : null,
        ),
        Material(
          color: AppColors.primaryColor,
          shape: const CircleBorder(),
          child: IconButton(
            icon: const Icon(Icons.camera_alt, color: Colors.white, size: 20),
            onPressed: _isSaving ? null : _pickImage,
          ),
        ),
      ],
    );
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreenWidget(
      title: 'Profil',
      child: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : SingleChildScrollView(
              padding: const EdgeInsets.all(24),
              child: Form(
                key: _formKey,
                autovalidateMode: AutovalidateMode.onUserInteraction,
                child: Column(
                  children: [
                    _buildAvatar(),
                    const SizedBox(height: 8),
                    TextButton(
                      onPressed: _isSaving ? null : _pickImage,
                      child: const Text('Promijeni sliku'),
                    ),
                    const SizedBox(height: 16),
                    TextFormField(
                      controller: _firstNameController,
                      decoration: const InputDecoration(labelText: 'Ime'),
                      validator: (v) => InputValidators.name(v, 'Ime'),
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: _lastNameController,
                      decoration: const InputDecoration(labelText: 'Prezime'),
                      validator: (v) => InputValidators.name(v, 'Prezime'),
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: _emailController,
                      keyboardType: TextInputType.emailAddress,
                      decoration: const InputDecoration(
                        labelText: 'Email',
                        helperText: 'Format: ime@domena.com',
                      ),
                      validator: InputValidators.email,
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: _usernameController,
                      decoration:
                          const InputDecoration(labelText: 'Korisničko ime'),
                      validator: InputValidators.username,
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: _phoneController,
                      keyboardType: TextInputType.phone,
                      decoration: const InputDecoration(
                        labelText: 'Telefon',
                        helperText: 'Opciono — npr. +387 61 123 456',
                      ),
                      validator: InputValidators.phone,
                    ),
                    const SizedBox(height: 8),
                    CheckboxListTile(
                      contentPadding: EdgeInsets.zero,
                      title: const Text('Izmijeni lozinku'),
                      subtitle: const Text(
                        'Za vlastitu lozinku unesite trenutnu, zatim novu i potvrdu.',
                      ),
                      value: _changePassword,
                      onChanged: _isSaving
                          ? null
                          : (v) => setState(() => _changePassword = v ?? false),
                      controlAffinity: ListTileControlAffinity.leading,
                    ),
                    if (_changePassword) ...[
                      TextFormField(
                        controller: _currentPasswordController,
                        obscureText: true,
                        decoration: const InputDecoration(
                          labelText: 'Trenutna lozinka',
                        ),
                        validator: (v) {
                          if (!_changePassword) return null;
                          if ((v ?? '').isEmpty) {
                            return 'Trenutna lozinka je obavezna.';
                          }
                          return null;
                        },
                      ),
                      const SizedBox(height: 12),
                      TextFormField(
                        controller: _newPasswordController,
                        obscureText: true,
                        decoration: const InputDecoration(
                          labelText: 'Nova lozinka',
                          helperText:
                              'Najmanje 8 karaktera, barem 1 slovo i 1 broj',
                        ),
                        validator: (v) => InputValidators.password(
                          v,
                          required: _changePassword,
                        ),
                      ),
                      const SizedBox(height: 12),
                      TextFormField(
                        controller: _confirmPasswordController,
                        obscureText: true,
                        decoration: const InputDecoration(
                          labelText: 'Potvrdi novu lozinku',
                        ),
                        validator: (v) {
                          if (!_changePassword) return null;
                          return InputValidators.confirmPassword(
                            v,
                            _newPasswordController.text,
                          );
                        },
                      ),
                    ],
                    if (_formError != null) ...[
                      const SizedBox(height: 12),
                      Text(
                        _formError!,
                        style: const TextStyle(color: Colors.red),
                        textAlign: TextAlign.center,
                      ),
                    ],
                    const SizedBox(height: 24),
                    SizedBox(
                      width: double.infinity,
                      height: 48,
                      child: ElevatedButton(
                        onPressed: _isSaving ? null : _save,
                        child: _isSaving
                            ? const SizedBox(
                                width: 22,
                                height: 22,
                                child: CircularProgressIndicator(
                                  strokeWidth: 2,
                                  color: Colors.white,
                                ),
                              )
                            : const Text('Sačuvaj'),
                      ),
                    ),
                  ],
                ),
              ),
            ),
    );
  }
}
