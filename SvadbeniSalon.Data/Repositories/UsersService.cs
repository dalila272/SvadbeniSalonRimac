using AutoMapper;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using SvadbeniSalon.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace SvadbeniSalon.Database.Repositories
{
    public class UsersService : BaseCRUDService<Database.Models.Korisnik, Shared.Models.Korisnik>
    {

        public UsersService(LocalContext context, IMapper mapper) : base(context, mapper)
        {

        }

        public override Shared.Models.Korisnik Add(Shared.Models.Korisnik korisnik)
        {
            return base.Add(korisnik);
        }


        public void UpdatePassword(long userId, string oldPassword, string newPassword)
        {
            var user = _context.Korisnici.FirstOrDefault(u => u.Id == userId);

            if (user.PasswordHash != GeneratePassword(oldPassword, Convert.FromBase64String(user.PasswordSalt)))
            {
                throw new Exception("Old password incorrect");
            }

            var salt = GetSalt();
            user.PasswordSalt = Convert.ToBase64String(salt);
            user.PasswordHash = GeneratePassword(newPassword, salt);

            _context.SaveChanges();
        }

        public Shared.Models.Korisnik Update(long id, Shared.Models.Korisnik korisnik)
        {
            if (UsernameExists(korisnik.Username, id))
            {
                throw new Exception("User with the same username already exists");
            }

            if (UsernameExists(korisnik.Email, id))
            {
                throw new Exception("User with the same email already exists");
            }

            var dbKorisnik = base.GetById(id);

            if (dbKorisnik == null)
            {
                throw new Exception("User does not exist.");
            }

            dbKorisnik.Ime = korisnik.Ime;
            dbKorisnik.Prezime = korisnik.Prezime;
            dbKorisnik.Uloga = korisnik.Uloga;
            dbKorisnik.Email = korisnik.Email;
            dbKorisnik.Username = korisnik.Username;


            _context.Update(dbKorisnik);
            _context.SaveChanges();

            return _mapper.Map<Shared.Models.Korisnik>(dbKorisnik);
        }

        public bool UsernameExists(string username, long id) => _context.Korisnici.Any(x => x.Username == username && x.Id != id);
        public bool EmailExists(string email, long id) => _context.Korisnici.Any(x => x.Email == email && x.Id != id);

        public Korisnik GetByUsername(string username)
        {
            return _context.Korisnici.FirstOrDefault(x => x.Username == username);
        }


        private string GeneratePassword(string password, byte[] salt)
        {

            // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hashed;
        }

        private byte[] GetSalt()
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }


    }
}
