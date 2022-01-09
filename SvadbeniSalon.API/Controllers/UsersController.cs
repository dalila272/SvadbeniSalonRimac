using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SvadbeniSalon.API.DTO.Requests.User;
using SvadbeniSalon.Database.Models;
using SvadbeniSalon.Database.Repositories;
using SvadbeniSalon.Shared.Enums;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SvadbeniSalon.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _usersService;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        public UsersController(IConfiguration config, UsersService usersRepository, IMapper mapper)
        {
            _usersService = usersRepository;
            _config = config;
            _mapper = mapper;
        }


        [HttpGet("username/{username}")]
        public ActionResult<Korisnik> GetByUsername(string username)
        {
            var user = _usersService.GetByUsername(username);
            return Ok(user);
        }



        [HttpPost]
        // [Authorize(Roles = "Administrator")]
        public ActionResult<Korisnik> Add(AddUser userReq)
        {

            var salt = GetSalt();
            var hash = GeneratePassword(userReq.Lozinka, salt);

            var user = _usersService.Add(new Shared.Models.Korisnik()
            {
                Username = userReq.Username,
                PasswordHash = hash,
                PasswordSalt = Convert.ToBase64String(salt),
                Uloga = userReq.Uloga
            });
            return Ok(user);
        }

        [HttpPut("{id}")]
        // [Authorize(Roles = "Administrator")]
        public ActionResult<Korisnik> Update(long id, AddUser userReq)
        {
            var user = _usersService.Update(id, _mapper.Map<Shared.Models.Korisnik>(userReq));


            return Ok(user);
        }

        [HttpPut("{id}/password")]
        // [Authorize(Roles = "Administrator")]
        public ActionResult Update(long id, PasswordChange passwordReq)
        {
            _usersService.UpdatePassword(id, passwordReq.OldPassword, passwordReq.NewPassword);
            return Ok();
        }

        [HttpDelete]
        // [Authorize(Roles = "Administrator")]
        public ActionResult Delete(string guid)
        {
            _usersService.Remove(new Guid(guid));
            return Ok();
        }


        [HttpPost("authenticate")]
        public ActionResult<string> Authenticate(LoginVM request)
        {

            var user = _usersService.GetByUsername(request.Username);

            if (user == null)
                return Unauthorized("Incorrect username or password");


            var hash = GeneratePassword(request.Password, Convert.FromBase64String(user.PasswordSalt));

            if (hash == user.PasswordHash)
            {
                var token = GenerateJSONWebToken(GetClaims(user));
                return Ok(token);
            }

            else
            {
                return Unauthorized("Incorrect username or password");
            }
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



        private List<Claim> GetClaims(Korisnik user)
        {
            return new List<Claim>
            {
                new Claim(ClaimTypes.Role,user.Uloga.ToString())

            };
        }

        private string GenerateJSONWebToken(List<Claim> claims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var expiresAt = DateTime.Now.AddHours(8);
            var token = new JwtSecurityToken
            (
                _config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                expires: expiresAt,
                claims: claims,
                signingCredentials: credentials
            );

            var identity = new ClaimsIdentity(claims);
            var claimPrincipal = new ClaimsPrincipal(identity);

            return (new JwtSecurityTokenHandler().WriteToken(token));
        }
    }
}



