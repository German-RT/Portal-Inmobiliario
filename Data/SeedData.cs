using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Models;

namespace PortalInmobiliario.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Verificar si ya hay datos
                if (context.Inmuebles.Any())
                {
                    return; // La BD ya tiene datos
                }

                // Agregar inmuebles de prueba
                context.Inmuebles.AddRange(
                    new Inmueble
                    {
                        Codigo = "DEP-001",
                        Titulo = "Departamento Moderno en Centro",
                        Imagen = "/images/depto1.jpg",
                        Tipo = TipoInmueble.Departamento,
                        Ciudad = "Lima",
                        Direccion = "Av. Arequipa 123",
                        Dormitorios = 2,
                        Banos = 2,
                        MetrosCuadrados = 80,
                        Precio = 150000,
                        Activo = true
                    },
                    new Inmueble
                    {
                        Codigo = "CASA-001",
                        Titulo = "Casa Familiar en Surco",
                        Imagen = "/images/casa1.jpg",
                        Tipo = TipoInmueble.Casa,
                        Ciudad = "Lima",
                        Direccion = "Calle Los Pinos 456",
                        Dormitorios = 4,
                        Banos = 3,
                        MetrosCuadrados = 200,
                        Precio = 450000,
                        Activo = true
                    },
                    new Inmueble
                    {
                        Codigo = "OFI-001",
                        Titulo = "Oficina en Miraflores",
                        Imagen = "/images/oficina1.jpg",
                        Tipo = TipoInmueble.Oficina,
                        Ciudad = "Lima",
                        Direccion = "Av. Larco 789",
                        Dormitorios = 0,
                        Banos = 2,
                        MetrosCuadrados = 60,
                        Precio = 120000,
                        Activo = true
                    },
                    new Inmueble
                    {
                        Codigo = "LOC-001",
                        Titulo = "Local Comercial en Centro",
                        Imagen = "/images/local1.jpg",
                        Tipo = TipoInmueble.Local,
                        Ciudad = "Arequipa",
                        Direccion = "Calle Mercaderes 321",
                        Dormitorios = 0,
                        Banos = 1,
                        MetrosCuadrados = 100,
                        Precio = 200000,
                        Activo = false  // Este estará inactivo para pruebas
                    }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}