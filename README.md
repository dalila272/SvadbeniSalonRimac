# Svadbeni Salon Rimac

Seminarski rad — Razvoj softvera II  
ASP.NET Core API, RabbitMQ worker, Flutter (mobile + desktop)

## Struktura

```
backend/          WebAPI, Services, Model, Subscriber
UI/mobile/        Flutter (klijent)
UI/desktop/       Flutter (admin / zaposlenik)
docker-compose.yml
docs/
```

## Potrebno

- .NET 9
- Flutter
- Docker Desktop

## Docker

```bash
cp .env.example .env
docker compose up --build
```

Migracije se pokreću same pri startu API-ja.

| Servis | URL |
|--------|-----|
| API | http://localhost:5121 |
| Swagger | http://localhost:5121/swagger |
| RabbitMQ | http://localhost:15672 (guest/guest) |

Napomena: u Visual Studio HTTPS profilu API ide na 5126, a Docker i Flutter na 5121.

## Lokalno (bez Dockera)

Potrebni su SQL Server i RabbitMQ. Zatim:

```bash
cd backend
dotnet restore
dotnet ef database update --project SvadbeniSalon.Services --startup-project SvadbeniSalon.WebAPI
dotnet run --project SvadbeniSalon.WebAPI
```

Worker:

```bash
dotnet run --project SvadbeniSalon.Subscriber
```

## Flutter

Default adrese
- Android emulator → `http://10.0.2.2:5121`
- Desktop / iOS → `http://localhost:5121` / `127.0.0.1`

```bash
# mobile
cd UI/mobile
flutter pub get
flutter run

# desktop
cd UI/desktop
flutter pub get
flutter run
```

Po potrebi:

```bash
flutter run --dart-define=API_BASE_URL=http://10.0.2.2:5121
flutter run --dart-define=API_BASE_URL=http://localhost:5121
```

## Test podaci

| | username | password |
|--|----------|----------|
| Desktop | desktop | test |
| Mobile | mobile | test |
| Admin | admin | test |
| Zaposlenik | zaposlenik | test |

Baza: `1210386`

## Auth

JWT (access + refresh token).

## Release build

**Android**

```bash
cd UI/mobile
flutter clean
flutter build apk --release
```

APK: `UI/mobile/build/app/outputs/flutter-apk/app-release.apk`  

**Windows**

```bash
cd UI/desktop
flutter clean
flutter build windows --release
```

EXE: `UI/desktop/build/windows/x64/runner/Release/`

## Napomene

- Javni GitHub repo (`.env` nije u gitu)
- Link na GitHub Release na DLWMS
- ZIP sa `.env` + šifra:

```bash
zip -e env-tajne.zip .env
```