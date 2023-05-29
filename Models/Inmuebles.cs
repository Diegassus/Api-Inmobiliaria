

namespace Api_Inmobiliaria.Models;

public class Inmueble
{
        public int Id { get ; set ;}
        public string? Lat { get ; set ; }
        public string? Lng { get ; set ; }
        public int? Uso { get ; set ; }
        public int? Tipo { get ; set ; }
        public int? Ambientes { get ; set ; }
        public bool Disponible { get ; set ;}
        public string? Direccion { get ; set ; }
        public decimal Precio { get ; set ; }
        public string? Foto { get ; set ; }
        public int PropietarioId { get ; set ; }
        public Propietario Duenio {get ; set ;}
}