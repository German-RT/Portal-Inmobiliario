using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Models;
using PortalInmobiliario.Data;

namespace PortalInmobiliario.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatalogController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string ciudad, TipoInmueble? tipo, 
            decimal? precioMin, decimal? precioMax, int? dormitorios, int page = 1)
        {
            // Inicializar la query de manera segura
            IQueryable<Inmueble> inmueblesQuery = _context.Inmuebles.Where(i => i.Activo);

            // Aplicar filtros de manera segura
            if (!string.IsNullOrEmpty(ciudad))
                inmueblesQuery = inmueblesQuery.Where(i => i.Ciudad.ToLower().Contains(ciudad.Trim().ToLower()));

            if (tipo.HasValue)
                inmueblesQuery = inmueblesQuery.Where(i => i.Tipo == tipo.Value);

            if (precioMin.HasValue)
                inmueblesQuery = inmueblesQuery.Where(i => i.Precio >= precioMin.Value);

            if (precioMax.HasValue)
                inmueblesQuery = inmueblesQuery.Where(i => i.Precio <= precioMax.Value);

            if (dormitorios.HasValue)
                inmueblesQuery = inmueblesQuery.Where(i => i.Dormitorios >= dormitorios.Value);

            // Validaciones server-side
            if (precioMin.HasValue && precioMax.HasValue && precioMin > precioMax)
            {
                ModelState.AddModelError("precioMax", "El precio máximo debe ser mayor o igual al precio mínimo");
            }

            // Paginación simple (6 elementos por página)
            int pageSize = 6;
            var totalInmuebles = await inmueblesQuery.CountAsync();
            var inmuebles = await inmueblesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalPages = (int)Math.Ceiling(totalInmuebles / (double)pageSize);
            ViewBag.CurrentPage = page;
            
            // Pasar los filtros actuales a la vista
            ViewBag.CiudadFiltro = ciudad;
            ViewBag.TipoFiltro = tipo;
            ViewBag.PrecioMinFiltro = precioMin;
            ViewBag.PrecioMaxFiltro = precioMax;
            ViewBag.DormitoriosFiltro = dormitorios;

            // Obtener lista de ciudades únicas para el dropdown
            ViewBag.Ciudades = await _context.Inmuebles
                .Where(i => i.Activo)
                .Select(i => i.Ciudad)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return View(inmuebles);
        }

        public async Task<IActionResult> Details(int id)
        {
            var inmueble = await _context.Inmuebles
                .FirstOrDefaultAsync(i => i.Id == id && i.Activo);

            if (inmueble == null)
            {
                return NotFound();
            }

            return View(inmueble);
        }
    }
}