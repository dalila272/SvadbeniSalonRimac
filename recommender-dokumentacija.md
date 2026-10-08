# Sistem preporuke paketa

## Pregled

Nakon registracije korisnik bira **interesne skupine** (muzičke žanrove). Na osnovu tih interesa i ocjena drugih korisnika sa sličnim ukusom, API rangira aktivne ponude salona i vraća personalizovane preporuke s obrazloženjem.

JWT autentifikacija ostaje zaseban mehanizam (Access Manager); **ne zamjenjuje** sistem preporuke.

## Podaci

| Entitet | Uloga |
|---------|--------|
| `UserZanr` | Interesi korisnika (veza User ↔ Zanr) |
| `MuzicarZanr` / `MuzicarPonuda` | Veza žanr → muzičar → paket |
| `Recenzija` | Ocjene nakon završene svadbe (kolaborativni signal) |

## API

| Metoda | Putanja | Opis |
|--------|---------|------|
| GET | `/Access/Interests` | Trenutni interesi prijavljenog korisnika |
| PUT | `/Access/Interests` | Postavljanje interesa `{ "zanrIds": [1,2,…] }` |
| GET | `/Preporuke?limit=5` | Rangirane preporuke s poljem `razlog` |

## Algoritam (hibrid)

Za svaku aktivnu ponudu računa se skor:

1. **Interesi (55%)** — udio korisnikovih žanrova koji se poklapaju sa žanrovima muzičara na paketu
2. **Slični korisnici (30%)** — korisnici koji su visoko ocijenili iste pakete (ili dijele interese), pa se favorizuju paketi koje su oni ocijenili ≥ 4
3. **Popularnost (15%)** — prosječna ocjena paketa

Ako korisnik ima interese, u rezultat ulaze **samo paketi s barem jednim poklapanjem žanra** (popularnost sama ne gurа sve ponude u „Preporučeno“).

Odgovor uključuje `razlog` (npr. „Odgovara vašim interesima: Pop, Rock“) i `matchingZanrovi`.

Ako korisnik nema interesa/ocjena, vraćaju se popularni paketi s odgovarajućim obrazloženjem.

### Primjer seed mapiranja (demo)

| Paket | Muzički profil | Tipični interesi |
|-------|----------------|------------------|
| Gold | Hit Parade Bend (Pop, Evergreen) + Jazz Club Trio | Pop, Jazz, Evergreen |
| Silver | Kvartet Armonija (Klasična) + Duo Tišina (Akustična, Evergreen) | Klasična, Akustična |
| Bronze | Sevdah Ansambl (Narodna, Starogradska) | Narodna, Starogradska |
| Regular | Hit Parade Bend + Electric Avenue (Rock) | Pop, Rock |

Seed klijent `mobile` ima interese Pop + Evergreen, pa bi Gold/Regular trebali imati jak interest skor. Drugi seed klijenti (`ana.popovic`, `marko.softic`, `ena.kovac`, lozinka `test`) nose ocjene za collaborative dio.

## Mobilna aplikacija

1. Nakon registracije → ekran odabira interesa  
2. Početna → sekcija **Preporučeno za vas** (naziv, cijena, razlog)  
3. Profil → mogućnost izmjene interesa  
 
