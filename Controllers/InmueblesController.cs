
using Api_Inmobiliaria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api_Inmobiliaria.Controllers;

[Route("api/[controller]")]
public class InmueblesController : ControllerBase
{
    private readonly DataContext _context;
    public InmueblesController(DataContext context)
    {
        _context = context;
    }

	// GET: api/inmuebles/inquilinos/:id
	[HttpGet("inquilinos/{id}")]
	[Authorize]
	public async Task<IActionResult> GetInquilinos(int id) // id es del inmueble
	{
		try{
			var usuario = User.Identity.Name;
			if(usuario == null) return Unauthorized("Token no válido");
			var user =await _context.Propietarios.SingleOrDefaultAsync(x => x.Correo == usuario);
			var fecha = DateTime.Today;

			var inmueble = _context.Contratos
				.Include(e => e.Bien)
				.ThenInclude(i => i.Duenio)
				.Where(e => e.Bien.Duenio.Id == user.Id)
				.Where(e=> e.Estado==true && e.Desde<=fecha && e.Hasta>=fecha)
				.Single(e => e.InmuebleId == id);

			return Ok(_context.Inquilinos.Where(i => i.Id == inmueble.InquilinoId).Single());
		}catch(Exception e){
			return BadRequest(e.Message);
		}
	}

	// GET: api/inmuebles/pagos/:id
	[HttpGet("pagos/{id}")]
	[Authorize]
	public async Task<IActionResult> GetPagos(int id) // id es del contrato
	{
		try{
			var usuario = User.Identity.Name;
			if(usuario == null) return Unauthorized("Token no válido");
			return Ok(_context.Pagos.Include(e=>e.Contrato).Where(e => e.ContratoId == id).ToArray());
		}catch(Exception e){
			return BadRequest(e.Message);
		}
	}

	// GET: api/inmuebles/contrato/:id
	[HttpGet("contrato/{id}")]
	[Authorize]
	public async Task<IActionResult> getContrato(int id)// id es de la propiedad
	{
		try{
			var usuario = User.Identity.Name;
			if(usuario == null) return Unauthorized("Token no válido");
			var user =await _context.Propietarios.SingleOrDefaultAsync(x => x.Correo == usuario);
			var fecha = DateTime.Today;

			var inmueble = _context.Contratos
				.Include(e => e.Bien)
				.ThenInclude(i => i.Duenio)
				.Where(e => e.Bien.Duenio.Id == user.Id)
				.Where(e=> e.Estado==true && e.Desde<=fecha && e.Hasta>=fecha)
				.Single(e => e.InmuebleId == id);

			return Ok(_context.Contratos.Include(e => e.Arrendatario).Single(e => e.Id == inmueble.Id));
		}catch(Exception e){
			return BadRequest(e.Message);
		}
	}

	// GET: api/inmuebles/alquiladas
	[HttpGet("alquiladas")]
	[Authorize]
	public async Task<IActionResult> GetAlq()
	{
		try
		{
			var usuario = User.Identity.Name;
			if(usuario == null) return Unauthorized("Token no válido");
			var user =await _context.Propietarios.SingleOrDefaultAsync(x => x.Correo == usuario);
			var fecha = DateTime.Today;

			var inmuebles = _context.Contratos
				.Include(e => e.Bien)
				.ThenInclude(i => i.Duenio)
				.Where(e => e.Bien.Duenio.Id == user.Id)
				.Where(e=> e.Estado==true && e.Desde<=fecha && e.Hasta>=fecha)
				.Select(e => e.Bien)
				.ToList();

			return Ok(inmuebles);

		}
		catch (Exception e)
		{
			return BadRequest(e.Message);
		}
	}

    // GET: api/inmuebles/propiedadesUsuario
    [HttpGet("propiedadesUsuario")]
	[Authorize]
    public async Task<IActionResult> Get()
    {
        try
        {
            var usuario = User.Identity.Name;
            if(usuario == null) return Unauthorized("Token no válido");
            return Ok(_context.Inmuebles.Include(e => e.Duenio).Where(e => e.Duenio.Correo == usuario));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

	// PUT: api/inmuebles/estado/:id
	[HttpPut("estado/{id}")]
	[Authorize]
		public async Task<IActionResult> Put(int id){
			try
			{
				var usuario = User.Identity.Name;
				if(usuario == null) return Unauthorized("Token no válido");
				var inmueble = _context.Inmuebles.Include(e => e.Duenio).Where(e => e.Duenio.Correo == usuario).Single(e => e.Id == id);
				if(inmueble.Duenio.Correo != usuario) return Unauthorized("Acceso denegado");
				var actual = inmueble.Disponible;
				inmueble.Disponible = !actual;
				_context.Update(inmueble);
				_context.SaveChanges();
				return Ok(inmueble);
			}catch(Exception e){
				//return BadRequest(e.Message);
				throw e;
			}
		}

    // GET: api/inmuebles/:id
    [HttpGet("{id}")]
	[Authorize]
		public async Task<IActionResult> Get(int id)
		{
			try
			{
				var usuario = User.Identity.Name;
                if(usuario == null) return Unauthorized("Token no válido");
				return Ok(_context.Inmuebles.Include(e => e.Duenio).Where(e => e.Duenio.Correo == usuario).Single(e => e.Id == id));
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

    // POST api/inmuebles/subir
	[HttpPost("subir")]
	[Authorize]
	public async Task<IActionResult> Post([FromBody] Inmueble entidad)
	{
		try
		{
			if (ModelState.IsValid)
			{
				entidad.PropietarioId = _context.Propietarios.Single(e => e.Correo == User.Identity.Name).Id;
				_context.Inmuebles.Add(entidad);
				_context.SaveChanges();
				return CreatedAtAction(nameof(Get), new { id = entidad.Id }, entidad);
			}
			return BadRequest();
		}
		catch (Exception ex)
		{
			return BadRequest(ex.Message);
		}
	}
}
