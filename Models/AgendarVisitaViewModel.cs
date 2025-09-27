using System.ComponentModel.DataAnnotations;

namespace PortalInmobiliario.Models
{
    public class AgendarVisitaViewModel
    {
        public int InmuebleId { get; set; }
        public string? InmuebleTitulo { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        [Display(Name = "Fecha y Hora de Inicio")]
        public DateTime FechaInicio { get; set; } = DateTime.Today.AddHours(10); // Por defecto 10 AM

        [Required(ErrorMessage = "La fecha de fin es requerida")]
        [Display(Name = "Fecha y Hora de Fin")]
        public DateTime FechaFin { get; set; } = DateTime.Today.AddHours(11); // Por defecto 11 AM

        [Display(Name = "Notas adicionales")]
        [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
        public string? Notas { get; set; }
    }
}