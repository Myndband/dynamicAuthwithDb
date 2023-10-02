using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DotnetAuthAndFileHandling.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataContext _context;

        public UserController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterRequest request)
        {
            if (_context.Users.Any(u => u.Email == request.Email))
            {
                return BadRequest("User already exists.");
            }

            string passwordMessage = CheckPasswordStrength(request.Password);
            if (!string.IsNullOrEmpty(passwordMessage))
                return BadRequest(passwordMessage);

            CreatePasswordHash(request.Password,
                 out byte[] passwordwordHash,
                 out byte[] passwordwordSalt);

            var user = new User
            {
                Email = request.Email,
                Role = "User",
                PasswordHash = passwordwordHash,
                PasswordSalt = passwordwordSalt,
                VerificationToken = CreateRandomToken()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User successfully created!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Password is incorrect.");
            }

            if (user.VerifiedAt == null)
            {
                return BadRequest("Not verified!");
            }

            user.JwtToken = CreateJwt(user);
            await _context.SaveChangesAsync();

            return Ok($"Welcome back, {user.Email}!  your json web token is :{user.JwtToken})");
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            if (user == null)
            {
                return BadRequest("Invalid token.");
            }

            user.VerifiedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok("User verified! :)");
        }

        [HttpPost("forgot-passwordword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            user.PasswordResetToken = CreateRandomToken();
            user.ResetTokenExpires = DateTime.Now.AddDays(1);
            await _context.SaveChangesAsync();

            return Ok("You may now reset your passwordword.");
        }

        [HttpPost("reset-passwordword")]
        public async Task<IActionResult> ResettPassword(ResetPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);
            if (user == null || user.ResetTokenExpires < DateTime.Now)
            {
                return BadRequest("Invalid Token.");
            }

            string passwordMessage = CheckPasswordStrength(request.Password);
            if (!string.IsNullOrEmpty(passwordMessage))
                return BadRequest(passwordMessage);

            CreatePasswordHash(request.Password, out byte[] passwordwordHash, out byte[] passwordwordSalt);

            user.PasswordHash = passwordwordHash;
            user.PasswordSalt = passwordwordSalt;
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;

            await _context.SaveChangesAsync();

            return Ok("Password successfully reset.");
        }

        //password
        private string CheckPasswordStrength(string password)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (password.Length < 7)
                stringBuilder.Append("minimum passwordword length should be 6" + Environment.NewLine);
            if (!(Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]") && Regex.IsMatch(password, "[0-9]")))
                stringBuilder.Append("passwordword should be alphanumeric" + Environment.NewLine);
            if (!Regex.IsMatch(password, "[<,>,@,!,#,$,%,^,&,*,(,),_,+,\\[,\\],{,},?,:,;,|,',\\,.,/,~,`,-,=]"))
                stringBuilder.Append("paassword should contain special charcter" + Environment.NewLine);
            return stringBuilder.ToString();
        }

        private void CreatePasswordHash(string passwordword, out byte[] passwordwordHash, out byte[] passwordwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordwordSalt = hmac.Key;
                passwordwordHash = hmac
                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(passwordword));
            }
        }

        private bool VerifyPasswordHash(string passwordword, byte[] passwordwordHash, byte[] passwordwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordwordSalt))
            {
                var computedHash = hmac
                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(passwordword));
                return computedHash.SequenceEqual(passwordwordHash);
            }
        }

        private string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        //jwt
        private string CreateJwt(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("veryverysecret.....");
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Email,user.Email)
            });

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials,
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }
    }
}
