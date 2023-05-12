using Microsoft.AspNetCore.Mvc;
using Api_Inmobiliaria.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace Api_Inmobiliaria.Controllers;

[ApiController]
[Route("[controller]")]
public class PropietariosController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IConfiguration config;
    public PropietariosController(DataContext context, IConfiguration config)
    {
        _context = context;
        this.config = config;
    }

    // GET: propietarios/
    [HttpGet("user")]
    [Authorize]
    public async Task<ActionResult<Propietario>> GetUser() // devuelve el propietario logueado
    {
        try
        {
            var usuario = User.Identity.Name;
            if(usuario == null) return Unauthorized("Token no válido");
            var dbUser = await _context.Propietarios.SingleOrDefaultAsync(x => x.Correo == usuario);
            if(dbUser == null) return BadRequest("El usuario no existe");
            return dbUser;
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    } 

    // GET: propietarios/:id
    [HttpGet("{id}")] // devuelve un propietario
    public ActionResult<Propietario> Get(int id)
    {
        var propietario = _context.Propietarios.Find(id);
        if(propietario == null) return NotFound();
        return propietario;
    }

    [HttpGet("test")] // devuelve todos los propietarios
    public async Task<IActionResult> Get(){
        try{
            return Ok(await _context.Propietarios.ToListAsync());
        }catch(Exception e){
            return BadRequest(e.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] LoginView loginView)
    {
        try
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: loginView.Clave,
                salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 1000,
                numBytesRequested: 256 / 8
            ));
            Console.WriteLine(hashed);
            var p = await _context.Propietarios.FirstOrDefaultAsync( x => x.Correo == loginView.Correo);
            if(p == null || p.Clave != hashed)
            {
                return BadRequest("Nombre de usuario o clave incorrectos");
            }
            else
            {
                var key = new SymmetricSecurityKey(
                    System.Text.Encoding.ASCII.GetBytes(
                        config["TokenAuthentication:SecretKey"]
                    )
                );
                var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, p.Correo),
                    new Claim("FullName", p.Nombre + " " + p.Apellido),
                    new Claim(ClaimTypes.Role, "Propietario")
                };

                var token = new JwtSecurityToken(
                    issuer: config["TokenAuthentication:Issuer"],
                    audience: config["TokenAuthentication:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(60),
                    signingCredentials: credenciales
                );

                return Ok(new JwtSecurityTokenHandler().WriteToken(token));
            }
        }
        catch(Exception e)
        {
            return BadRequest(e.Message);
        }
    }

}
