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

        private List<Producto> ObtenerCatalogoSistema()
        {
            return new List<Producto>
            {
                new Producto { Id = 1, CodigoSku = "SKU-BEB-001", Nombre = "Agua Mineral 1.5L", CategoriaId = 1, CategoriaNombre = "Bebidas", ProveedorNombre = "Distribuidora Quilmes S.A.", PrecioCosto = 520m, Precio = 800m, StockActual = 150, StockMinimo = 50, StockOptimo = 200 },
                new Producto { Id = 2, CodigoSku = "SKU-BEB-002", Nombre = "Cerveza Lager 1L", CategoriaId = 1, CategoriaNombre = "Bebidas", ProveedorNombre = "Distribuidora Quilmes S.A.", PrecioCosto = 1650m, Precio = 2500m, StockActual = 105, StockMinimo = 100, StockOptimo = 250 },
                new Producto { Id = 3, CodigoSku = "SKU-ALM-001", Nombre = "Arroz Blanco 1kg", CategoriaId = 2, CategoriaNombre = "Almacén", ProveedorNombre = "Molinos del Sur S.R.L.", PrecioCosto = 780m, Precio = 1200m, StockActual = 200, StockMinimo = 30, StockOptimo = 220 },
                new Producto { Id = 4, CodigoSku = "SKU-ALM-002", Nombre = "Fideos Espagueti 500g", CategoriaId = 2, CategoriaNombre = "Almacén", ProveedorNombre = "Molinos del Sur S.R.L.", PrecioCosto = 580m, Precio = 900m, StockActual = 43, StockMinimo = 40, StockOptimo = 120 },
                new Producto { Id = 5, CodigoSku = "SKU-LAC-001", Nombre = "Leche Entera 1L", CategoriaId = 3, CategoriaNombre = "Lácteos", ProveedorNombre = "Lácteos La Serenísima S.A.", PrecioCosto = 720m, Precio = 1100m, StockActual = 15, StockMinimo = 50, StockOptimo = 180 },
                new Producto { Id = 6, CodigoSku = "SKU-LIM-001", Nombre = "Lavandina Concentrada 1L", CategoriaId = 4, CategoriaNombre = "Limpieza", ProveedorNombre = "Química Limpia S.A.", PrecioCosto = 490m, Precio = 850m, StockActual = 120, StockMinimo = 20, StockOptimo = 150 }
            };
        }

        public IActionResult Index()
        {
            var productos = ObtenerCatalogoSistema();
            return View(productos);
        }

        public IActionResult Productos()
        {
            var productos = ObtenerCatalogoSistema();
            return View(productos);
        }

        public IActionResult Movimientos()
        {
            return View();
        }

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
