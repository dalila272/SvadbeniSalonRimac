# Mapiranje domene (salon)

Trenutni backend koristi salon entitete + JWT auth (User/Role).

| Entitet | Napomena |
|---------|----------|
| User, Role, UserRole, RefreshToken | Auth |
| Ponuda, Meni, Artikal, Muzicar, Dekoracija, Zanr | Katalog |
| Svadba, DnevniSastanak | Termini |
| Recenzija, Rata, Racun | Recenzije / uplate / računi |

## API (glavni)

- `/Access` — Login, Register, Logout, Profile, ChangePassword, Forgot/ResetPassword, LoginWithRefreshToken
- `/Ponude`, `/Meniji`, `/Artikli`, `/Muzicari`, `/Dekoracije`, `/Zanrovi`
- `/Svadbe`, `/DnevniSastanci` (+ `/zauzeti`, `/{id}/status`)
- `/Recenzije`, `/Rate`, `/Racuni`, `/Izvjestaji`
- `/Users`

eCommerce entiteti (Product, Order, …) uklonjeni migracijom `RemoveEcommerceEntities`.
