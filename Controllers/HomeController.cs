using ControlInventario.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ControlInventario.Controllers
{
    [Authorize] 
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Prototipo 1: Tablero de Control Principal (con Semaforización y Drill-Down)
        public IActionResult Index()
        {
            return View();
        }

        // Prototipo 2: Gestión de Productos y Catálogo (ABM sin Drill-Down)
        public IActionResult Productos()
        {
            return View();
        }

        // Prototipo 3: Registro de Movimientos de Stock (Entradas / Salidas / Ajustes sin Drill-Down)
        public IActionResult Movimientos()
        {
            return View();
        }

        // Prototipo 4: Gestión de Proveedores y Órdenes de Compra (sin Drill-Down)
        public IActionResult ProveedoresOrdenes()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
