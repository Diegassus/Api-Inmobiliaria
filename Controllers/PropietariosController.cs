using Microsoft.AspNetCore.Mvc;
using Api_Inmobiliaria.Models;

namespace Api_Inmobiliaria.Controllers;

[ApiController]
[Route("[controller]")]
public class PropietariosController : ControllerBase
{
    private readonly DataContext _context;
    public PropietariosController(DataContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")] // devuelve un propietario
    public Propietario Get(int id)
    {
        return _context.Propietarios.Find(id);
    }

    [HttpGet("test")] // devuelve todos los propietarios
    public IEnumerable<Propietario> Get(){
        return _context.Propietarios;
    }
}
