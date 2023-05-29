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
[Route("api/[controller]")]
public class PropietariosController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IConfiguration config;
    public PropietariosController(DataContext context, IConfiguration config)
    {
        _context = context;
        this.config = config;
    }

    // GET: api/propietarios/user
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

    // POST: api/propietarios/editar 
    [HttpPut("editar")]
    [Authorize]
        public async Task<IActionResult> Editar(Propietario propietario)
        {
            try
            {
                var usuario = User.Identity.Name;
                if(usuario == null) return Unauthorized("Token no válido");
                var dbUser = await _context.Propietarios.SingleOrDefaultAsync(x => x.Correo == usuario);
                if(dbUser == null) return BadRequest("El usuario no existe");
                dbUser.Nombre = propietario.Nombre;
                dbUser.Apellido = propietario.Apellido;
                dbUser.Dni = propietario.Dni;
                dbUser.Telefono = propietario.Telefono;
                dbUser.Correo = propietario.Correo;
                if(propietario.Clave != null && propietario.Clave != "" && propietario.Clave != dbUser.Clave) {
                    dbUser.Clave = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: propietario.Clave,
                    salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 1000,
                    numBytesRequested: 256 / 8));
                }
                _context.Update(dbUser);
                _context.SaveChanges();
                return Ok(dbUser);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    // POST: api/propietarios/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginView loginView) // para loguearse
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
    
    // GET: propietarios/:id
    // [HttpGet("{id}")] // devuelve un propietario
    // public ActionResult<Propietario> Get(int id)
    // {
    //     var propietario = _context.Propietarios.Find(id);
    //     if(propietario == null) return NotFound();
    //     return propietario;
    // }

    // [HttpGet("test")] // devuelve todos los propietarios con el DNI hasheado como clave
    // public async Task<IActionResult> Get(){
    //     try{
    //         var propietarios = await _context.Propietarios.ToListAsync();
    //         foreach(var p in propietarios){
    //             p.Clave = Convert.ToBase64String(KeyDerivation.Pbkdf2(
    //                 password: p.Dni,
    //                 salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
    //                 prf: KeyDerivationPrf.HMACSHA1,
    //                 iterationCount: 1000,
    //                 numBytesRequested: 256 / 8
    //             ));
    //             // aplicar cambios del propietario en la DB
    //             _context.Entry(p).State = EntityState.Modified;
    //         }
    //         // guardar cambios en la DB
    //         await _context.SaveChangesAsync();
    //         return Ok(propietarios);
    //     }catch(Exception e){
    //         return BadRequest(e.Message);
    //     }
    // }

}
