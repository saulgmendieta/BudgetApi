using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System;
using BasicApi.DTO;

namespace BudgetApi.Controllers
{
    [Route("api/v1/login")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IConfiguration configuration;
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public AccountsController(UserManager<IdentityUser> userManager,
                                  SignInManager<IdentityUser> signInManager,
                                  IConfiguration configuration,
                                  ApplicationDbContext context,
                                  IMapper mapper)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.context = context;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<AutenticationResponse>> Login([FromBody] UserData userData)
        {
            var result = await signInManager.PasswordSignInAsync(userData.Username, userData.Password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return await MakeToken(userData);
            }
            else
            {
                return Unauthorized("User Not Authorized");
            }
        }

        [HttpPost("make")]
        public async Task<ActionResult<AutenticationResponse>> Make([FromBody] UserData userData)
        {
            var user = new IdentityUser { UserName = userData.Username };
            var result = await userManager.CreateAsync(user, userData.Password);
            if (result.Succeeded)
            {
                return await MakeToken(userData);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        private async Task<AutenticationResponse> MakeToken(UserData userData)
        {
            var claims = new List<Claim>()
            {
                new Claim("username", userData.Username),
            };
            var user = await userManager.FindByNameAsync(userData.Username);
            var claimsDB = await userManager.GetClaimsAsync(user);
            claims.AddRange(claimsDB);
            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["jwtkey"]));
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);
            var expiracy = DateTime.UtcNow.AddYears(1);
            var expire = (int)(expiracy - new DateTime(1970, 1, 1)).TotalSeconds;
            var expiresIn = (int)(expiracy - DateTime.UtcNow).TotalSeconds;
            var token = new JwtSecurityToken(issuer: null, audience: null, claims: claims,
                                                      expires: expiracy, signingCredentials: creds);
            return new AutenticationResponse()
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresIn = expiresIn,
                Expire = expire
            };
        }

    }
}
