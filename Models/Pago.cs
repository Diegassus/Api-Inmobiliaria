

using System.ComponentModel.DataAnnotations.Schema;

namespace Api_Inmobiliaria.Models;

public class Pago{

    public int Id { get ; set ;}
    public int Nro { get ; set;}
    public DateTime Fecha { get ; set ; }
    public Decimal Importe { get ; set ; }
    public int ContratoId { get ; set ; }
    [ForeignKey(nameof(ContratoId))]
    public Contrato Contrato { get ; set ; }

}