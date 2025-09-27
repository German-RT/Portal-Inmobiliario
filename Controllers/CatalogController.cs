using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Models;
using PortalInmobiliario.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

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

            // Verificar si tiene reserva activa
            var tieneReservaActiva = await _context.Reservas
                .AnyAsync(r => r.InmuebleId == id && r.FechaExpiracion > DateTime.Now);

            ViewBag.TieneReservaActiva = tieneReservaActiva;

            return View(inmueble);
        }
        
        
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AgendarVisita(AgendarVisitaViewModel model)
{
    if (!ModelState.IsValid)
    {
        var inmueble = await _context.Inmuebles.FindAsync(model.InmuebleId);
        model.InmuebleTitulo = inmueble?.Titulo;
        return View("AgendarVisita", model);
    }

    // Validar horario laboral (08:00 - 19:00)
    if (model.FechaInicio.TimeOfDay < TimeSpan.FromHours(8) || 
        model.FechaFin.TimeOfDay > TimeSpan.FromHours(19))
    {
        ModelState.AddModelError("", "Las visitas solo pueden agendarse en horario laboral (08:00 - 19:00)");
        var inmueble = await _context.Inmuebles.FindAsync(model.InmuebleId);
        model.InmuebleTitulo = inmueble?.Titulo;
        return View("AgendarVisita", model);
    }

    if (model.FechaInicio >= model.FechaFin)
    {
        ModelState.AddModelError("", "La fecha de fin debe ser posterior a la fecha de inicio");
        var inmueble = await _context.Inmuebles.FindAsync(model.InmuebleId);
        model.InmuebleTitulo = inmueble?.Titulo;
        return View("AgendarVisita", model);
    }

    // Validar visitas solapadas
    var visitaSolapada = await _context.Visitas
        .AnyAsync(v => v.InmuebleId == model.InmuebleId && 
                      v.Estado != EstadoVisita.Cancelada &&
                      ((model.FechaInicio >= v.FechaInicio && model.FechaInicio < v.FechaFin) ||
                       (model.FechaFin > v.FechaInicio && model.FechaFin <= v.FechaFin) ||
                       (model.FechaInicio <= v.FechaInicio && model.FechaFin >= v.FechaFin)));

    if (visitaSolapada)
    {
        ModelState.AddModelError("", "Ya existe una visita agendada para este inmueble en el horario seleccionado");
        var inmueble = await _context.Inmuebles.FindAsync(model.InmuebleId);
        model.InmuebleTitulo = inmueble?.Titulo;
        return View("AgendarVisita", model);
    }

    // Crear la visita (SIN USER REAL)
    var visita = new Visita
    {
        InmuebleId = model.InmuebleId,
        UsuarioId = "user-test-id-" + Guid.NewGuid().ToString(), // Simulado
        FechaInicio = model.FechaInicio,
        FechaFin = model.FechaFin,
        Notas = model.Notas,
        Estado = EstadoVisita.Solicitada
    };

    _context.Visitas.Add(visita);
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Visita agendada exitosamente. Te contactaremos para confirmar.";
    return RedirectToAction("Details", new { id = model.InmuebleId });
}

        [HttpGet]
        public async Task<IActionResult> AgendarVisita(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble == null || !inmueble.Activo)
            {
                return NotFound();
            }

            var model = new AgendarVisitaViewModel
            {
                InmuebleId = id,
                InmuebleTitulo = inmueble.Titulo
            };

            return View(model);
        }

        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Reservar(int id)
{
    var inmueble = await _context.Inmuebles.FindAsync(id);
    if (inmueble == null || !inmueble.Activo)
    {
        return NotFound();
    }

    // Validar que no existe una reserva activa
    var reservaActiva = await _context.Reservas
        .AnyAsync(r => r.InmuebleId == id && r.FechaExpiracion > DateTime.Now);

    if (reservaActiva)
    {
        TempData["ErrorMessage"] = "Este inmueble ya tiene una reserva activa.";
        return RedirectToAction("Details", new { id });
    }

    // Crear reserva (SIN USER REAL)
    var reserva = new Reserva
    {
        InmuebleId = id,
        UsuarioId = "user-test-id-" + Guid.NewGuid().ToString(), // Simulado
        FechaCreacion = DateTime.Now,
        FechaExpiracion = DateTime.Now.AddHours(48)
    };

    _context.Reservas.Add(reserva);
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Inmueble reservado exitosamente por 48 horas.";
    return RedirectToAction("Details", new { id });
}
    }
}