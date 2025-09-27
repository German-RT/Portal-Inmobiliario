using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortalInmobiliario.Models
{
    public class Inmueble
    {
        public int Id { get; set; }
        
        [Required]
        public string? Codigo { get; set; }
        
        [Required]
        public string? Titulo { get; set; }
        
        public string? Imagen { get; set; }
        
        [Required]
        public TipoInmueble Tipo { get; set; }
        
        [Required]
        public string? Ciudad { get; set; }
        
        [Required]
        public string? Direccion { get; set; }
        
        [Range(0, int.MaxValue)]
        public int Dormitorios { get; set; }
        
        [Range(0, int.MaxValue)]
        public int Banos { get; set; }
        
        [Range(0.1, double.MaxValue)]
        public double MetrosCuadrados { get; set; }
        
        [Range(0.1, double.MaxValue)]
        public decimal Precio { get; set; }
        
        public bool Activo { get; set; } = true;
    }

    public enum TipoInmueble
    {
        Departamento,
        Casa,
        Oficina,
        Local
    }
}