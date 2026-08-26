class InputValidators {
  static final emailRegex = RegExp(r'^[^@\s]+@[^@\s]+\.[^@\s]+$');
  static final phoneRegex = RegExp(r'^[+0-9\s\-()/]+$');
  static final hasLetter = RegExp(r'[A-Za-z]');
  static final hasDigit = RegExp(r'[0-9]');

  static String? required(String? value, String label) {
    if ((value ?? '').trim().isEmpty) return '$label je obavezno.';
    return null;
  }

  static String? email(String? value) {
    final trimmed = value?.trim() ?? '';
    if (trimmed.isEmpty) return 'Email je obavezan.';
    if (trimmed.length > 100) {
      return 'Email ne smije imati više od 100 karaktera.';
    }
    if (!emailRegex.hasMatch(trimmed)) {
      return 'Unesite ispravan email u formatu: ime@domena.com';
    }
    return null;
  }

  static String? username(String? value) {
    final trimmed = value?.trim() ?? '';
    if (trimmed.isEmpty) return 'Korisničko ime je obavezno.';
    if (trimmed.length < 3) {
      return 'Korisničko ime mora imati najmanje 3 karaktera.';
    }
    if (trimmed.length > 100) {
      return 'Korisničko ime ne smije imati više od 100 karaktera.';
    }
    return null;
  }

  static String? name(String? value, String label) {
    final trimmed = value?.trim() ?? '';
    if (trimmed.isEmpty) return '$label je obavezno.';
    if (trimmed.length > 50) {
      return '$label ne smije imati više od 50 karaktera.';
    }
    return null;
  }

  static String? phone(String? value) {
    final trimmed = value?.trim() ?? '';
    if (trimmed.isEmpty) return null;
    if (trimmed.length > 20) {
      return 'Telefon ne smije imati više od 20 karaktera.';
    }
    if (!phoneRegex.hasMatch(trimmed)) {
      return 'Telefon smije sadržavati samo brojeve, razmake, +, -, ( ).';
    }
    return null;
  }

  static String? password(String? value, {bool required = true}) {
    final v = value ?? '';
    if (v.isEmpty) {
      return required ? 'Lozinka je obavezna.' : null;
    }
    if (v.length < 8) {
      return 'Lozinka mora imati najmanje 8 karaktera.';
    }
    if (v.length > 100) {
      return 'Lozinka ne smije imati više od 100 karaktera.';
    }
    if (!hasLetter.hasMatch(v)) {
      return 'Lozinka mora sadržavati barem jedno slovo.';
    }
    if (!hasDigit.hasMatch(v)) {
      return 'Lozinka mora sadržavati barem jedan broj.';
    }
    return null;
  }

  static String? confirmPassword(String? value, String? password) {
    if ((password ?? '').isEmpty && (value ?? '').isEmpty) return null;
    if ((value ?? '').isEmpty) return 'Potvrda lozinke je obavezna.';
    if (value != password) {
      return 'Potvrda lozinke se ne poklapa s novom lozinkom.';
    }
    return null;
  }
}
