# JWT Access Manager — dokumentacija

## Odluka

U skladu s RSII specifikacijom 2025/26, umjesto algoritma preporuke implementiran je **JWT Access Manager** s refresh tokenima — naprednija forma autentifikacije korisnika.

Implementacija prati [FIT-RS2-2026](https://github.com/amelmusic/FIT-RS2-2026) uzorak:

- `AccessController` — Login, Register, LoginWithRefreshToken
- `AccessManager` — generisanje JWT access tokena
- `RefreshTokenService` — čuvanje i invalidacija refresh tokena u bazi
- `CryptoService` — hash lozinki (salt + hash)

## Endpointi

| Metoda | Putanja | Opis |
|--------|---------|------|
| POST | `/Access/Login` | Prijava, vraća access + refresh token |
| POST | `/Access/Register` | Registracija novog korisnika |
| POST | `/Access/LoginWithRefreshToken` | Obnova access tokena |

## JWT claims

- Id, FirstName, LastName, Email, Role, IsActive

## Flutter klijent

- `AuthProvider` — login, čuvanje tokena, logout
- `BaseProvider` — `Authorization: Bearer {token}` na svim API pozivima
- HTTP 401 → korisniku se prikaže greška sesije; odjava iz menija vodi na login

## Konfiguracija

Tajne se čitaju iz `.env` / environment varijabli:

- `JWT_SECRET_KEY`
- `JWT_ISSUER`
- `JWT_AUDIENCE`
- `JWT_DURATION_MINUTES`

## Razlika od Duende Identity Server

Duende/OpenIddict kao zaseban auth servis nije korišten; FIT nastava koristi ugrađeni Access Manager u API projektu, što zadovoljava alternativu recommenderu iz specifikacije.
