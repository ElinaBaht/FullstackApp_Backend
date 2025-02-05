﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostBackend.Data;
using PostBackend.Models;
using System.Security.Cryptography;

namespace PostBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        
            public static User user = new User();

            //public IConfiguration _configuration;

            public DataContext DataContext;

            public AuthController(DataContext dataContext)
            {
                DataContext = dataContext;
            }
            /*public AuthController(IConfiguration configuration)
            {
                _configuration = configuration;
            }*/

            [HttpPost("register")]
            public async Task<ActionResult<User>> Register(UserDto request)
            {
                CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

                user.Username = request.Username;
                user.Email = request.Email;
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;

                DataContext.Users.Add(user);
                await DataContext.SaveChangesAsync();

                return Ok(await DataContext.Users.ToListAsync());

            }

            [HttpPost("login")]

            public async Task<ActionResult<string>> Login(UserLogin request)
            {
                if (user.Email != request.Email)
                {
                    return BadRequest("User not found");
                }

                if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                {
                    return BadRequest("Wrong password");
                }
                //string token = CreateToken(user);
                return Ok("Ok");
            }

            /*private string CreateToken(User user)
            {
                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username)
                };
                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                    _configuration.GetSection("AppSettings:Token").Value));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: creds);
                var jwt = new JwtSecurityTokenHandler().WriteToken(token);
                return jwt;
            }*/

            private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
            {
                using (var hmac = new HMACSHA512())
                {
                    passwordSalt = hmac.Key;
                    passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                }
            }

            private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
            {
                using (var hmac = new HMACSHA512(passwordSalt))
                {
                    var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                    return computedHash.SequenceEqual(passwordHash);
                }
            }
        
    }
}
