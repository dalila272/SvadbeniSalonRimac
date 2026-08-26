class UserProfile {
  final int id;
  final String firstName;
  final String lastName;
  final String email;
  final String username;
  final String? phoneNumber;
  final String? profileImageBase64;
  final bool isActive;

  UserProfile({
    required this.id,
    required this.firstName,
    required this.lastName,
    required this.email,
    required this.username,
    this.phoneNumber,
    this.profileImageBase64,
    required this.isActive,
  });

  factory UserProfile.fromJson(Map<String, dynamic> json) {
    return UserProfile(
      id: json['id'] is int ? json['id'] as int : int.parse(json['id'].toString()),
      firstName: json['firstName']?.toString() ?? '',
      lastName: json['lastName']?.toString() ?? '',
      email: json['email']?.toString() ?? '',
      username: json['username']?.toString() ?? '',
      phoneNumber: json['phoneNumber']?.toString(),
      profileImageBase64: json['profileImageBase64']?.toString(),
      isActive: json['isActive'] == true,
    );
  }
}
