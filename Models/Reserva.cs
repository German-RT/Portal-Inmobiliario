using System.ComponentModel.DataAnnotations.Schema;

namespace PortalInmobiliario.Models
{
    public class Reserva
    {
        public int Id { get; set; }
        
        public int InmuebleId { get; set; }
        public Inmueble? Inmueble { get; set; }
        
        public string? UsuarioId { get; set; }
        
        public DateTime FechaExpiracion { get; set; }
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}