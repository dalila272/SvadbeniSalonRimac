import 'package:svadbeni_salon_desktop/layouts/master_screen.dart';
import 'package:svadbeni_salon_desktop/models/user.dart';
import 'package:svadbeni_salon_desktop/providers/user_provider.dart';
import 'package:svadbeni_salon_desktop/utils/api_client_exception.dart';
import 'package:svadbeni_salon_desktop/utils/input_validators.dart';
import 'package:svadbeni_salon_desktop/utils/utils_widgets.dart';
import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';
import 'package:provider/provider.dart';
import 'package:svadbeni_salon_desktop/constants/role_names.dart';

class EmployeeDetailsScreen extends StatefulWidget {
  final User? employee;

  const EmployeeDetailsScreen({super.key, this.employee});

  @override
  State<EmployeeDetailsScreen> createState() => _EmployeeDetailsScreenState();
}

class _EmployeeDetailsScreenState extends State<EmployeeDetailsScreen> {
  final _formKey = GlobalKey<FormBuilderState>();
  Map<String, dynamic> _initialValue = {};
  bool _changePassword = false;

  late UserProvider _userProvider;

  @override
  void initState() {
    super.initState();
    _userProvider = context.read<UserProvider>();
    _initialValue = {
      'firstName': widget.employee?.firstName ?? '',
      'lastName': widget.employee?.lastName ?? '',
      'email': widget.employee?.email ?? '',
      'username': widget.employee?.username ?? '',
      'phoneNumber': widget.employee?.phoneNumber ?? '',
      'isActive': widget.employee?.isActive ?? true,
      'password': '',
      'confirmPassword': '',
      'newPassword': '',
      'confirmNewPassword': '',
    };
  }

  bool get _isEditing => widget.employee != null;

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: _isEditing ? 'Uredi zaposlenika' : 'Novi zaposlenik',
      child: Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 720),
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
      initialValue: _initialValue,
      autovalidateMode: AutovalidateMode.onUserInteraction,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Expanded(
                child: FormBuilderTextField(
                  name: 'firstName',
                  decoration: const InputDecoration(
                    labelText: 'Ime',
                    border: OutlineInputBorder(),
                  ),
                  validator: FormBuilderValidators.compose([
                    FormBuilderValidators.required(
                      errorText: 'Ime je obavezno.',
                    ),
                    FormBuilderValidators.maxLength(
                      50,
                      errorText: 'Ime ne smije imati više od 50 karaktera.',
                    ),
                  ]),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: FormBuilderTextField(
                  name: 'lastName',
                  decoration: const InputDecoration(
                    labelText: 'Prezime',
                    border: OutlineInputBorder(),
                  ),
                  validator: FormBuilderValidators.compose([
                    FormBuilderValidators.required(
                      errorText: 'Prezime je obavezno.',
                    ),
                    FormBuilderValidators.maxLength(
                      50,
                      errorText: 'Prezime ne smije imati više od 50 karaktera.',
                    ),
                  ]),
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          FormBuilderTextField(
            name: 'email',
            decoration: const InputDecoration(
              labelText: 'Email',
              helperText: 'Format: ime@domena.com',
              border: OutlineInputBorder(),
            ),
            keyboardType: TextInputType.emailAddress,
            validator: InputValidators.email,
          ),
          const SizedBox(height: 16),
          FormBuilderTextField(
            name: 'username',
            decoration: const InputDecoration(
              labelText: 'Korisničko ime',
              border: OutlineInputBorder(),
            ),
            validator: FormBuilderValidators.compose([
              FormBuilderValidators.required(
                errorText: 'Korisničko ime je obavezno.',
              ),
              FormBuilderValidators.minLength(
                3,
                errorText: 'Korisničko ime mora imati najmanje 3 karaktera.',
              ),
              FormBuilderValidators.maxLength(
                100,
                errorText:
                    'Korisničko ime ne smije imati više od 100 karaktera.',
              ),
            ]),
          ),
          const SizedBox(height: 16),
          FormBuilderTextField(
            name: 'phoneNumber',
            decoration: const InputDecoration(
              labelText: 'Telefon',
              helperText: 'Opciono — samo brojevi, npr. +387 61 123 456',
              border: OutlineInputBorder(),
            ),
            keyboardType: TextInputType.phone,
            validator: InputValidators.phone,
          ),
          if (!_isEditing) ...[
            const SizedBox(height: 16),
            FormBuilderTextField(
              name: 'password',
              decoration: const InputDecoration(
                labelText: 'Lozinka',
                helperText: 'Najmanje 8 karaktera, barem 1 slovo i 1 broj',
                border: OutlineInputBorder(),
              ),
              obscureText: true,
              validator: InputValidators.password,
            ),
            const SizedBox(height: 16),
            FormBuilderTextField(
              name: 'confirmPassword',
              decoration: const InputDecoration(
                labelText: 'Potvrdi lozinku',
                border: OutlineInputBorder(),
              ),
              obscureText: true,
              validator: (value) => InputValidators.confirmPassword(
                value,
                _formKey.currentState?.fields['password']?.value as String?,
              ),
            ),
          ],
          if (_isEditing) ...[
            const SizedBox(height: 16),
            CheckboxListTile(
              contentPadding: EdgeInsets.zero,
              title: const Text('Izmijeni lozinku'),
              subtitle: const Text(
                'Kao administrator ne unosite staru lozinku — samo novu i potvrdu.',
              ),
              value: _changePassword,
              onChanged: (v) => setState(() => _changePassword = v ?? false),
              controlAffinity: ListTileControlAffinity.leading,
            ),
            if (_changePassword) ...[
              const SizedBox(height: 8),
              FormBuilderTextField(
                name: 'newPassword',
                decoration: const InputDecoration(
                  labelText: 'Nova lozinka',
                  helperText: 'Najmanje 8 karaktera, barem 1 slovo i 1 broj',
                  border: OutlineInputBorder(),
                ),
                obscureText: true,
                validator: (v) =>
                    InputValidators.password(v, required: _changePassword),
              ),
              const SizedBox(height: 16),
              FormBuilderTextField(
                name: 'confirmNewPassword',
                decoration: const InputDecoration(
                  labelText: 'Potvrdi novu lozinku',
                  border: OutlineInputBorder(),
                ),
                obscureText: true,
                validator: (value) {
                  if (!_changePassword) return null;
                  return InputValidators.confirmPassword(
                    value,
                    _formKey.currentState?.fields['newPassword']?.value
                        as String?,
                  );
                },
              ),
            ],
          ],
          const SizedBox(height: 16),
          FormBuilderCheckbox(
            name: 'isActive',
            title: const Text('Aktivan nalog'),
            subtitle: const Text('Neaktivan zaposlenik se ne može prijaviti'),
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

    final formData = Map<String, dynamic>.from(_formKey.currentState!.value);

    final request = {
      'firstName': (formData['firstName'] as String?)?.trim(),
      'lastName': (formData['lastName'] as String?)?.trim(),
      'email': (formData['email'] as String?)?.trim(),
      'username': (formData['username'] as String?)?.trim(),
      'phoneNumber': _optionalTrimmed(formData['phoneNumber'] as String?),
      'isActive': formData['isActive'] ?? true,
      'role': RoleNames.zaposlenik,
    };

    try {
      if (_isEditing) {
        await _userProvider.update(widget.employee!.id!, request);
        if (_changePassword) {
          await _userProvider.setPassword(
            widget.employee!.id!,
            (formData['newPassword'] as String?)?.trim() ?? '',
            (formData['confirmNewPassword'] as String?)?.trim() ?? '',
          );
        }
        if (!mounted) return;
        Navigator.of(context).pop('updated');
      } else {
        request['password'] = (formData['password'] as String?)?.trim();
        await _userProvider.insert(request);
        if (!mounted) return;
        Navigator.of(context).pop('created');
      }
    } on ApiClientException catch (e) {
      if (mounted) alertBox(context, 'Greška', e.message);
    } on Exception catch (e) {
      if (mounted) alertBox(context, 'Greška', e.toString());
    }
  }

  String? _optionalTrimmed(String? value) {
    final trimmed = value?.trim() ?? '';
    return trimmed.isEmpty ? null : trimmed;
  }
}
