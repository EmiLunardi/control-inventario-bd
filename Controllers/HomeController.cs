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

        // Estado persistente en memoria durante la ejecución de la aplicación para reflejar en tiempo real
        // altas, bajas, movimientos de stock, recepción de órdenes y recálculo de semaforización/KPIs.
        private static readonly object _lock = new();
        private static bool _inicializado = false;

        private static List<Categoria> _categorias = new();
        private static List<Sucursal> _sucursales = new();
        private static List<Proveedor> _proveedores = new();
        private static List<Producto> _productos = new();
        private static List<StockSucursal> _stockSucursales = new();
        private static List<MovimientoStock> _movimientos = new();
        private static List<OrdenCompra> _ordenesCompra = new();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            AsegurarDatosInicializados();
        }

        private static void AsegurarDatosInicializados()
        {
            lock (_lock)
            {
                if (_inicializado) return;

                _categorias = new List<Categoria>
                {
                    new Categoria { Id = 1, Nombre = "Bebidas", Descripcion = "Bebidas sin alcohol, aguas, gaseosas y cervezas" },
                    new Categoria { Id = 2, Nombre = "Almacén", Descripcion = "Productos secos, harinas, arroz, pastas y aceites" },
                    new Categoria { Id = 3, Nombre = "Lácteos", Descripcion = "Leches fluidas, quesos, yogures y mantecas refrigeradas" },
                    new Categoria { Id = 4, Nombre = "Limpieza", Descripcion = "Desinfectantes, detergentes, lavandinas e higiene" }
                };

                _sucursales = new List<Sucursal>
                {
                    new Sucursal { Id = 1, Nombre = "Central", Ubicacion = "Av. Corrientes 1234, CABA", Telefono = "011-4381-1000" },
                    new Sucursal { Id = 2, Nombre = "Sucursal Norte", Ubicacion = "Av. Cabildo 4500, CABA", Telefono = "011-4701-2000" },
                    new Sucursal { Id = 3, Nombre = "Sucursal Sur", Ubicacion = "Av. Mitre 500, Avellaneda", Telefono = "011-4201-3000" }
                };

                _proveedores = new List<Proveedor>
                {
                    new Proveedor { Id = 1, RazonSocial = "Distribuidora Quilmes S.A.", Cuit = "30-54892104-9", Telefono = "011-4321-8890", Email = "ventas@distquilmes.com.ar", Direccion = "Av. Calchaquí 3100, Quilmes" },
                    new Proveedor { Id = 2, RazonSocial = "Molinos del Sur S.R.L.", Cuit = "30-61029384-2", Telefono = "011-4582-1102", Email = "pedidos@molinosdelsur.com.ar", Direccion = "Ruta 205 Km 42, Ezeiza" },
                    new Proveedor { Id = 3, RazonSocial = "Lácteos La Serenísima S.A.", Cuit = "30-50129381-7", Telefono = "0800-555-5228", Email = "mayoristas@laserenisima.com.ar", Direccion = "Parque Industrial Gral. Rodríguez" },
                    new Proveedor { Id = 4, RazonSocial = "Química Limpia S.A.", Cuit = "30-70918273-4", Telefono = "011-4756-9012", Email = "contacto@quimicalimpia.com.ar", Direccion = "Calle 14 N° 850, San Martín" }
                };

                _productos = new List<Producto>
                {
                    // Bebidas (1 Óptimo, 1 Advertencia, 1 Crítico)
                    new Producto { Id = 1, CodigoSku = "SKU-BEB-001", Nombre = "Agua Mineral 1.5L", CategoriaId = 1, CategoriaNombre = "Bebidas", ProveedorId = 1, ProveedorNombre = "Distribuidora Quilmes S.A.", PrecioCosto = 520m, Precio = 800m, StockActual = 150, StockMinimo = 50, StockOptimo = 200 },
                    new Producto { Id = 2, CodigoSku = "SKU-BEB-002", Nombre = "Cerveza Lager 1L", CategoriaId = 1, CategoriaNombre = "Bebidas", ProveedorId = 1, ProveedorNombre = "Distribuidora Quilmes S.A.", PrecioCosto = 1650m, Precio = 2500m, StockActual = 105, StockMinimo = 100, StockOptimo = 250 }, // Advertencia (+5% sobre mínimo)
                    new Producto { Id = 3, CodigoSku = "SKU-BEB-003", Nombre = "Gaseosa Cola 2.25L", CategoriaId = 1, CategoriaNombre = "Bebidas", ProveedorId = 1, ProveedorNombre = "Distribuidora Quilmes S.A.", PrecioCosto = 1400m, Precio = 2150m, StockActual = 35, StockMinimo = 60, StockOptimo = 180 }, // Crítico

                    // Almacén (2 Óptimos, 1 Advertencia)
                    new Producto { Id = 4, CodigoSku = "SKU-ALM-001", Nombre = "Arroz Blanco 1kg", CategoriaId = 2, CategoriaNombre = "Almacén", ProveedorId = 2, ProveedorNombre = "Molinos del Sur S.R.L.", PrecioCosto = 780m, Precio = 1200m, StockActual = 200, StockMinimo = 30, StockOptimo = 220 },
                    new Producto { Id = 5, CodigoSku = "SKU-ALM-002", Nombre = "Fideos Espagueti 500g", CategoriaId = 2, CategoriaNombre = "Almacén", ProveedorId = 2, ProveedorNombre = "Molinos del Sur S.R.L.", PrecioCosto = 580m, Precio = 900m, StockActual = 43, StockMinimo = 40, StockOptimo = 120 }, // Advertencia (+7.5% sobre mínimo)
                    new Producto { Id = 6, CodigoSku = "SKU-ALM-003", Nombre = "Aceite de Girasol 1.5L", CategoriaId = 2, CategoriaNombre = "Almacén", ProveedorId = 2, ProveedorNombre = "Molinos del Sur S.R.L.", PrecioCosto = 1850m, Precio = 2700m, StockActual = 110, StockMinimo = 45, StockOptimo = 150 },

                    // Lácteos (1 Crítico, 1 Advertencia, 1 Óptimo)
                    new Producto { Id = 7, CodigoSku = "SKU-LAC-001", Nombre = "Leche Entera 1L", CategoriaId = 3, CategoriaNombre = "Lácteos", ProveedorId = 3, ProveedorNombre = "Lácteos La Serenísima S.A.", PrecioCosto = 720m, Precio = 1100m, StockActual = 15, StockMinimo = 50, StockOptimo = 180 }, // Crítico
                    new Producto { Id = 8, CodigoSku = "SKU-LAC-002", Nombre = "Queso Cremoso Horma 4kg", CategoriaId = 3, CategoriaNombre = "Lácteos", ProveedorId = 3, ProveedorNombre = "Lácteos La Serenísima S.A.", PrecioCosto = 14200m, Precio = 21000m, StockActual = 22, StockMinimo = 20, StockOptimo = 60 }, // Advertencia (+10% exacto)
                    new Producto { Id = 9, CodigoSku = "SKU-LAC-003", Nombre = "Manteca Primera Calidad 200g", CategoriaId = 3, CategoriaNombre = "Lácteos", ProveedorId = 3, ProveedorNombre = "Lácteos La Serenísima S.A.", PrecioCosto = 1350m, Precio = 2100m, StockActual = 85, StockMinimo = 30, StockOptimo = 100 },

                    // Limpieza (2 Óptimos, 1 Crítico)
                    new Producto { Id = 10, CodigoSku = "SKU-LIM-001", Nombre = "Lavandina Concentrada 1L", CategoriaId = 4, CategoriaNombre = "Limpieza", ProveedorId = 4, ProveedorNombre = "Química Limpia S.A.", PrecioCosto = 490m, Precio = 850m, StockActual = 120, StockMinimo = 20, StockOptimo = 150 },
                    new Producto { Id = 11, CodigoSku = "SKU-LIM-002", Nombre = "Detergente Vajilla 750ml", CategoriaId = 4, CategoriaNombre = "Limpieza", ProveedorId = 4, ProveedorNombre = "Química Limpia S.A.", PrecioCosto = 820m, Precio = 1350m, StockActual = 95, StockMinimo = 35, StockOptimo = 120 },
                    new Producto { Id = 12, CodigoSku = "SKU-LIM-003", Nombre = "Desinfectante Pisos 5L", CategoriaId = 4, CategoriaNombre = "Limpieza", ProveedorId = 4, ProveedorNombre = "Química Limpia S.A.", PrecioCosto = 2400m, Precio = 3900m, StockActual = 18, StockMinimo = 25, StockOptimo = 80 } // Crítico
                };

                // Desglose por sucursal coherente con el StockActual de cada producto
                _stockSucursales = new List<StockSucursal>
                {
                    // Prod 1: Agua Mineral (150 total = 80 + 45 + 25)
                    new StockSucursal { Id = 1, ProductoId = 1, CodigoSku = "SKU-BEB-001", ProductoNombre = "Agua Mineral 1.5L", CategoriaNombre = "Bebidas", SucursalId = 1, SucursalNombre = "Central", CantidadActual = 80, PuntoReorden = 20 },
                    new StockSucursal { Id = 2, ProductoId = 1, CodigoSku = "SKU-BEB-001", ProductoNombre = "Agua Mineral 1.5L", CategoriaNombre = "Bebidas", SucursalId = 2, SucursalNombre = "Sucursal Norte", CantidadActual = 45, PuntoReorden = 15 },
                    new StockSucursal { Id = 3, ProductoId = 1, CodigoSku = "SKU-BEB-001", ProductoNombre = "Agua Mineral 1.5L", CategoriaNombre = "Bebidas", SucursalId = 3, SucursalNombre = "Sucursal Sur", CantidadActual = 25, PuntoReorden = 15 },

                    // Prod 2: Cerveza Lager (105 total = 50 + 33 + 22)
                    new StockSucursal { Id = 4, ProductoId = 2, CodigoSku = "SKU-BEB-002", ProductoNombre = "Cerveza Lager 1L", CategoriaNombre = "Bebidas", SucursalId = 1, SucursalNombre = "Central", CantidadActual = 50, PuntoReorden = 40 },
                    new StockSucursal { Id = 5, ProductoId = 2, CodigoSku = "SKU-BEB-002", ProductoNombre = "Cerveza Lager 1L", CategoriaNombre = "Bebidas", SucursalId = 2, SucursalNombre = "Sucursal Norte", CantidadActual = 33, PuntoReorden = 30 },
                    new StockSucursal { Id = 6, ProductoId = 2, CodigoSku = "SKU-BEB-002", ProductoNombre = "Cerveza Lager 1L", CategoriaNombre = "Bebidas", SucursalId = 3, SucursalNombre = "Sucursal Sur", CantidadActual = 22, PuntoReorden = 30 },

                    // Prod 3: Gaseosa Cola (35 total = 15 + 12 + 8)
                    new StockSucursal { Id = 7, ProductoId = 3, CodigoSku = "SKU-BEB-003", ProductoNombre = "Gaseosa Cola 2.25L", CategoriaNombre = "Bebidas", SucursalId = 1, SucursalNombre = "Central", CantidadActual = 15, PuntoReorden = 25 },
                    new StockSucursal { Id = 8, ProductoId = 3, CodigoSku = "SKU-BEB-003", ProductoNombre = "Gaseosa Cola 2.25L", CategoriaNombre = "Bebidas", SucursalId = 2, SucursalNombre = "Sucursal Norte", CantidadActual = 12, PuntoReorden = 20 },
                    new StockSucursal { Id = 9, ProductoId = 3, CodigoSku = "SKU-BEB-003", ProductoNombre = "Gaseosa Cola 2.25L", CategoriaNombre = "Bebidas", SucursalId = 3, SucursalNombre = "Sucursal Sur", CantidadActual = 8, PuntoReorden = 15 },

                    // Prod 4: Arroz Blanco (200 total = 110 + 50 + 40)
                    new StockSucursal { Id = 10, ProductoId = 4, CodigoSku = "SKU-ALM-001", ProductoNombre = "Arroz Blanco 1kg", CategoriaNombre = "Almacén", SucursalId = 1, SucursalNombre = "Central", CantidadActual = 110, PuntoReorden = 15 },
                    new StockSucursal { Id = 11, ProductoId = 4, CodigoSku = "SKU-ALM-001", ProductoNombre = "Arroz Blanco 1kg", CategoriaNombre = "Almacén", SucursalId = 2, SucursalNombre = "Sucursal Norte", CantidadActual = 50, PuntoReorden = 10 },
                    new StockSucursal { Id = 12, ProductoId = 4, CodigoSku = "SKU-ALM-001", ProductoNombre = "Arroz Blanco 1kg", CategoriaNombre = "Almacén", SucursalId = 3, SucursalNombre = "Sucursal Sur", CantidadActual = 40, PuntoReorden = 5 },

                    // Prod 5: Fideos Espagueti (43 total = 20 + 11 + 12)
                    new StockSucursal { Id = 13, ProductoId = 5, CodigoSku = "SKU-ALM-002", ProductoNombre = "Fideos Espagueti 500g", CategoriaNombre = "Almacén", SucursalId = 1, SucursalNombre = "Central", CantidadActual = 20, PuntoReorden = 18 },
                    new StockSucursal { Id = 14, ProductoId = 5, CodigoSku = "SKU-ALM-002", ProductoNombre = "Fideos Espagueti 500g", CategoriaNombre = "Almacén", SucursalId = 2, SucursalNombre = "Sucursal Norte", CantidadActual = 11, PuntoReorden = 11 },
                    new StockSucursal { Id = 15, ProductoId = 5, CodigoSku = "SKU-ALM-002", ProductoNombre = "Fideos Espagueti 500g", CategoriaNombre = "Almacén", SucursalId = 3, SucursalNombre = "Sucursal Sur", CantidadActual = 12, PuntoReorden = 11 },

                    // Prod 6: Aceite de Girasol (110 total = 60 + 30 + 20)
                    new StockSucursal { Id = 16, ProductoId = 6, CodigoSku = "SKU-ALM-003", ProductoNombre = "Aceite de Girasol 1.5L", CategoriaNombre = "Almacén", SucursalId = 1, SucursalNombre = "Central", CantidadActual = 60, PuntoReorden = 20 },
                    new StockSucursal { Id = 17, ProductoId = 6, CodigoSku = "SKU-ALM-003", ProductoNombre = "Aceite de Girasol 1.5L", CategoriaNombre = "Almacén", SucursalId = 2, SucursalNombre = "Sucursal Norte", CantidadActual = 30, PuntoReorden = 15 },
                    new StockSucursal { Id = 18, ProductoId = 6, CodigoSku = "SKU-ALM-003", ProductoNombre = "Aceite de Girasol 1.5L", CategoriaNombre = "Almacén", SucursalId = 3, SucursalNombre = "Sucursal Sur", CantidadActual = 20, PuntoReorden = 10 },

                    // Prod 7: Leche Entera (15 total = 8 + 4 + 3)
                    new StockSucursal { Id = 19, ProductoId = 7, CodigoSku = "SKU-LAC-001", ProductoNombre = "Leche Entera 1L", CategoriaNombre = "Lácteos", SucursalId = 1, SucursalNombre = "Central", CantidadActual = 8, PuntoReorden = 25 },
                    new StockSucursal { Id = 20, ProductoId = 7, CodigoSku = "SKU-LAC-001", ProductoNombre = "Leche Entera 1L", CategoriaNombre = "Lácteos", SucursalId = 2, SucursalNombre = "Sucursal Norte", CantidadActual = 4, PuntoReorden = 15 },
                    new StockSucursal { Id = 21, ProductoId = 7, CodigoSku = "SKU-LAC-001", ProductoNombre = "Leche Entera 1L", CategoriaNombre = "Lácteos", SucursalId = 3, SucursalNombre = "Sucursal Sur", CantidadActual = 3, PuntoReorden = 10 },

                    // Prod 8: Queso Cremoso (22 total = 11 + 6 + 5)
                    new StockSucursal { Id = 22, ProductoId = 8, CodigoSku = "SKU-LAC-002", ProductoNombre = "Queso Cremoso Horma 4kg", CategoriaNombre = "Lácteos", SucursalId = 1, SucursalNombre = "Central", CantidadActual = 11, PuntoReorden = 10 },
                    new StockSucursal { Id = 23, ProductoId = 8, CodigoSku = "SKU-LAC-002", ProductoNombre = "Queso Cremoso Horma 4kg", CategoriaNombre = "Lácteos", SucursalId = 2, SucursalNombre = "Sucursal Norte", CantidadActual = 6, PuntoReorden = 5 },
                    new StockSucursal { Id = 24, ProductoId = 8, CodigoSku = "SKU-LAC-002", ProductoNombre = "Queso Cremoso Horma 4kg", CategoriaNombre = "Lácteos", SucursalId = 3, SucursalNombre = "Sucursal Sur", CantidadActual = 5, PuntoReorden = 5 },

                    // Prod 9: Manteca 200g (85 total = 45 + 25 + 15)
                    new StockSucursal { Id = 25, ProductoId = 9, CodigoSku = "SKU-LAC-003", ProductoNombre = "Manteca Primera Calidad 200g", CategoriaNombre = "Lácteos", SucursalId = 1, SucursalNombre = "Central", CantidadActual = 45, PuntoReorden = 15 },
                    new StockSucursal { Id = 26, ProductoId = 9, CodigoSku = "SKU-LAC-003", ProductoNombre = "Manteca Primera Calidad 200g", CategoriaNombre = "Lácteos", SucursalId = 2, SucursalNombre = "Sucursal Norte", CantidadActual = 25, PuntoReorden = 10 },
                    new StockSucursal { Id = 27, ProductoId = 9, CodigoSku = "SKU-LAC-003", ProductoNombre = "Manteca Primera Calidad 200g", CategoriaNombre = "Lácteos", SucursalId = 3, SucursalNombre = "Sucursal Sur", CantidadActual = 15, PuntoReorden = 5 },

                    // Prod 10: Lavandina 1L (120 total = 65 + 35 + 20)
                    new StockSucursal { Id = 28, ProductoId = 10, CodigoSku = "SKU-LIM-001", ProductoNombre = "Lavandina Concentrada 1L", CategoriaNombre = "Limpieza", SucursalId = 1, SucursalNombre = "Central", CantidadActual = 65, PuntoReorden = 10 },
                    new StockSucursal { Id = 29, ProductoId = 10, CodigoSku = "SKU-LIM-001", ProductoNombre = "Lavandina Concentrada 1L", CategoriaNombre = "Limpieza", SucursalId = 2, SucursalNombre = "Sucursal Norte", CantidadActual = 35, PuntoReorden = 5 },
                    new StockSucursal { Id = 30, ProductoId = 10, CodigoSku = "SKU-LIM-001", ProductoNombre = "Lavandina Concentrada 1L", CategoriaNombre = "Limpieza", SucursalId = 3, SucursalNombre = "Sucursal Sur", CantidadActual = 20, PuntoReorden = 5 },

                    // Prod 11: Detergente 750ml (95 total = 50 + 25 + 20)
                    new StockSucursal { Id = 31, ProductoId = 11, CodigoSku = "SKU-LIM-002", ProductoNombre = "Detergente Vajilla 750ml", CategoriaNombre = "Limpieza", SucursalId = 1, SucursalNombre = "Central", CantidadActual = 50, PuntoReorden = 15 },
                    new StockSucursal { Id = 32, ProductoId = 11, CodigoSku = "SKU-LIM-002", ProductoNombre = "Detergente Vajilla 750ml", CategoriaNombre = "Limpieza", SucursalId = 2, SucursalNombre = "Sucursal Norte", CantidadActual = 25, PuntoReorden = 10 },
                    new StockSucursal { Id = 33, ProductoId = 11, CodigoSku = "SKU-LIM-002", ProductoNombre = "Detergente Vajilla 750ml", CategoriaNombre = "Limpieza", SucursalId = 3, SucursalNombre = "Sucursal Sur", CantidadActual = 20, PuntoReorden = 10 },

                    // Prod 12: Desinfectante Pisos 5L (18 total = 10 + 5 + 3)
                    new StockSucursal { Id = 34, ProductoId = 12, CodigoSku = "SKU-LIM-003", ProductoNombre = "Desinfectante Pisos 5L", CategoriaNombre = "Limpieza", SucursalId = 1, SucursalNombre = "Central", CantidadActual = 10, PuntoReorden = 12 },
                    new StockSucursal { Id = 35, ProductoId = 12, CodigoSku = "SKU-LIM-003", ProductoNombre = "Desinfectante Pisos 5L", CategoriaNombre = "Limpieza", SucursalId = 2, SucursalNombre = "Sucursal Norte", CantidadActual = 5, PuntoReorden = 8 },
                    new StockSucursal { Id = 36, ProductoId = 12, CodigoSku = "SKU-LIM-003", ProductoNombre = "Desinfectante Pisos 5L", CategoriaNombre = "Limpieza", SucursalId = 3, SucursalNombre = "Sucursal Sur", CantidadActual = 3, PuntoReorden = 5 }
                };

                _movimientos = new List<MovimientoStock>
                {
                    new MovimientoStock { Id = 1, ProductoId = 1, CodigoSku = "SKU-BEB-001", ProductoNombre = "Agua Mineral 1.5L", CategoriaNombre = "Bebidas", SucursalId = 1, SucursalNombre = "Central", UsuarioNombre = "admin", TipoMovimiento = "ENTRADA", Cantidad = 120, Motivo = "Recepción OC-2026-001", Fecha = DateTime.Now.AddDays(-8) },
                    new MovimientoStock { Id = 2, ProductoId = 1, CodigoSku = "SKU-BEB-001", ProductoNombre = "Agua Mineral 1.5L", CategoriaNombre = "Bebidas", SucursalId = 2, SucursalNombre = "Sucursal Norte", UsuarioNombre = "gerente_log", TipoMovimiento = "SALIDA", Cantidad = 25, Motivo = "Despacho Pedido Mayorista #1042", Fecha = DateTime.Now.AddDays(-3) },
                    new MovimientoStock { Id = 3, ProductoId = 2, CodigoSku = "SKU-BEB-002", ProductoNombre = "Cerveza Lager 1L", CategoriaNombre = "Bebidas", SucursalId = 1, SucursalNombre = "Central", UsuarioNombre = "admin", TipoMovimiento = "ENTRADA", Cantidad = 90, Motivo = "Abastecimiento mensual cervecería", Fecha = DateTime.Now.AddDays(-7) },
                    new MovimientoStock { Id = 4, ProductoId = 2, CodigoSku = "SKU-BEB-002", ProductoNombre = "Cerveza Lager 1L", CategoriaNombre = "Bebidas", SucursalId = 3, SucursalNombre = "Sucursal Sur", UsuarioNombre = "operador_sur", TipoMovimiento = "SALIDA", Cantidad = 35, Motivo = "Despacho bares zona sur #4412", Fecha = DateTime.Now.AddDays(-2) },
                    new MovimientoStock { Id = 5, ProductoId = 3, CodigoSku = "SKU-BEB-003", ProductoNombre = "Gaseosa Cola 2.25L", CategoriaNombre = "Bebidas", SucursalId = 1, SucursalNombre = "Central", UsuarioNombre = "operador_central", TipoMovimiento = "SALIDA", Cantidad = 45, Motivo = "Venta mayorista supermercados", Fecha = DateTime.Now.AddDays(-1) },
                    new MovimientoStock { Id = 6, ProductoId = 4, CodigoSku = "SKU-ALM-001", ProductoNombre = "Arroz Blanco 1kg", CategoriaNombre = "Almacén", SucursalId = 1, SucursalNombre = "Central", UsuarioNombre = "admin", TipoMovimiento = "ENTRADA", Cantidad = 200, Motivo = "Recepción OC-2026-002", Fecha = DateTime.Now.AddDays(-10) },
                    new MovimientoStock { Id = 7, ProductoId = 5, CodigoSku = "SKU-ALM-002", ProductoNombre = "Fideos Espagueti 500g", CategoriaNombre = "Almacén", SucursalId = 2, SucursalNombre = "Sucursal Norte", UsuarioNombre = "gerente_log", TipoMovimiento = "SALIDA", Cantidad = 30, Motivo = "Remito entrega #0004-00018290", Fecha = DateTime.Now.AddDays(-4) },
                    new MovimientoStock { Id = 8, ProductoId = 6, CodigoSku = "SKU-ALM-003", ProductoNombre = "Aceite de Girasol 1.5L", CategoriaNombre = "Almacén", SucursalId = 1, SucursalNombre = "Central", UsuarioNombre = "admin", TipoMovimiento = "ENTRADA", Cantidad = 80, Motivo = "Ingreso lote aceitera", Fecha = DateTime.Now.AddDays(-6) },
                    new MovimientoStock { Id = 9, ProductoId = 7, CodigoSku = "SKU-LAC-001", ProductoNombre = "Leche Entera 1L", CategoriaNombre = "Lácteos", SucursalId = 1, SucursalNombre = "Central", UsuarioNombre = "operador_central", TipoMovimiento = "SALIDA", Cantidad = 50, Motivo = "Distribución diaria comercios", Fecha = DateTime.Now.AddDays(-2) },
                    new MovimientoStock { Id = 10, ProductoId = 7, CodigoSku = "SKU-LAC-001", ProductoNombre = "Leche Entera 1L", CategoriaNombre = "Lácteos", SucursalId = 2, SucursalNombre = "Sucursal Norte", UsuarioNombre = "gerente_log", TipoMovimiento = "AJUSTE", Cantidad = -5, Motivo = "Merma por envase dañado en transporte", Fecha = DateTime.Now.AddDays(-1) },
                    new MovimientoStock { Id = 11, ProductoId = 8, CodigoSku = "SKU-LAC-002", ProductoNombre = "Queso Cremoso Horma 4kg", CategoriaNombre = "Lácteos", SucursalId = 3, SucursalNombre = "Sucursal Sur", UsuarioNombre = "operador_sur", TipoMovimiento = "SALIDA", Cantidad = 12, Motivo = "Despacho fiambrerías zona sur", Fecha = DateTime.Now.AddDays(-3) },
                    new MovimientoStock { Id = 12, ProductoId = 9, CodigoSku = "SKU-LAC-003", ProductoNombre = "Manteca Primera Calidad 200g", CategoriaNombre = "Lácteos", SucursalId = 1, SucursalNombre = "Central", UsuarioNombre = "admin", TipoMovimiento = "ENTRADA", Cantidad = 60, Motivo = "Ingreso cámara frigorífica Central", Fecha = DateTime.Now.AddDays(-5) },
                    new MovimientoStock { Id = 13, ProductoId = 10, CodigoSku = "SKU-LIM-001", ProductoNombre = "Lavandina Concentrada 1L", CategoriaNombre = "Limpieza", SucursalId = 1, SucursalNombre = "Central", UsuarioNombre = "admin", TipoMovimiento = "ENTRADA", Cantidad = 100, Motivo = "Compra mayorista Química Limpia", Fecha = DateTime.Now.AddDays(-9) },
                    new MovimientoStock { Id = 14, ProductoId = 11, CodigoSku = "SKU-LIM-002", ProductoNombre = "Detergente Vajilla 750ml", CategoriaNombre = "Limpieza", SucursalId = 2, SucursalNombre = "Sucursal Norte", UsuarioNombre = "gerente_log", TipoMovimiento = "SALIDA", Cantidad = 15, Motivo = "Despacho clientes gastronómicos", Fecha = DateTime.Now.AddDays(-2) },
                    new MovimientoStock { Id = 15, ProductoId = 12, CodigoSku = "SKU-LIM-003", ProductoNombre = "Desinfectante Pisos 5L", CategoriaNombre = "Limpieza", SucursalId = 3, SucursalNombre = "Sucursal Sur", UsuarioNombre = "operador_sur", TipoMovimiento = "SALIDA", Cantidad = 22, Motivo = "Entrega consorcios y clínicas", Fecha = DateTime.Now.AddHours(-12) }
                };

                _ordenesCompra = new List<OrdenCompra>
                {
                    new OrdenCompra { Id = 1, NumeroOrden = "OC-2026-001", ProveedorId = 1, ProveedorNombre = "Distribuidora Quilmes S.A.", SucursalDestinoId = 1, SucursalDestinoNombre = "Central", ProductoId = 1, ProductoNombre = "Agua Mineral 1.5L", CantidadSolicitada = 100, PrecioUnitario = 520m, Total = 52000m, FechaEmision = DateTime.Now.AddDays(-10), FechaRecepcion = DateTime.Now.AddDays(-8), Estado = "RECIBIDA", Observaciones = "Abastecimiento mensual bebidas" },
                    new OrdenCompra { Id = 2, NumeroOrden = "OC-2026-002", ProveedorId = 2, ProveedorNombre = "Molinos del Sur S.R.L.", SucursalDestinoId = 1, SucursalDestinoNombre = "Central", ProductoId = 4, ProductoNombre = "Arroz Blanco 1kg", CantidadSolicitada = 200, PrecioUnitario = 780m, Total = 156000m, FechaEmision = DateTime.Now.AddDays(-12), FechaRecepcion = DateTime.Now.AddDays(-10), Estado = "RECIBIDA", Observaciones = "Lote secos temporada alta" },
                    new OrdenCompra { Id = 3, NumeroOrden = "OC-2026-003", ProveedorId = 1, ProveedorNombre = "Distribuidora Quilmes S.A.", SucursalDestinoId = 3, SucursalDestinoNombre = "Sucursal Sur", ProductoId = 3, ProductoNombre = "Gaseosa Cola 2.25L", CantidadSolicitada = 100, PrecioUnitario = 1400m, Total = 140000m, FechaEmision = DateTime.Now.AddDays(-2), Estado = "PENDIENTE", Observaciones = "Reposición urgente por stock crítico" },
                    new OrdenCompra { Id = 4, NumeroOrden = "OC-2026-004", ProveedorId = 3, ProveedorNombre = "Lácteos La Serenísima S.A.", SucursalDestinoId = 1, SucursalDestinoNombre = "Central", ProductoId = 7, ProductoNombre = "Leche Entera 1L", CantidadSolicitada = 80, PrecioUnitario = 720m, Total = 57600m, FechaEmision = DateTime.Now.AddDays(-1), Estado = "PENDIENTE", Observaciones = "Reposición crítica lácteos Central" },
                    new OrdenCompra { Id = 5, NumeroOrden = "OC-2026-005", ProveedorId = 4, ProveedorNombre = "Química Limpia S.A.", SucursalDestinoId = 1, SucursalDestinoNombre = "Central", ProductoId = 12, ProductoNombre = "Desinfectante Pisos 5L", CantidadSolicitada = 50, PrecioUnitario = 2400m, Total = 120000m, FechaEmision = DateTime.Now.AddHours(-6), Estado = "PENDIENTE", Observaciones = "Refuerzo stock crítico limpieza" }
                };

                _inicializado = true;
            }
        }

        private DashboardViewModel ConstruirViewModel()
        {
            lock (_lock)
            {
                var resumenCategorias = _categorias.Select(cat =>
                {
                    var prodsCat = _productos.Where(p => p.CategoriaId == cat.Id || p.CategoriaNombre == cat.Nombre).ToList();
                    return new CategoriaResumenViewModel
                    {
                        CategoriaId = cat.Id,
                        Nombre = cat.Nombre,
                        Descripcion = cat.Descripcion ?? string.Empty,
                        TotalProductos = prodsCat.Count,
                        StockTotalUnidades = prodsCat.Sum(p => p.StockActual),
                        StockMinimoTotal = prodsCat.Sum(p => p.StockMinimo),
                        ValorizadoCosto = prodsCat.Sum(p => p.StockActual * p.PrecioCosto),
                        CantidadOptimo = prodsCat.Count(p => p.ObtenerEstadoSemaforo() == "Óptimo"),
                        CantidadAdvertencia = prodsCat.Count(p => p.ObtenerEstadoSemaforo() == "Advertencia"),
                        CantidadCritico = prodsCat.Count(p => p.ObtenerEstadoSemaforo() == "Crítico")
                    };
                }).ToList();

                return new DashboardViewModel
                {
                    Productos = _productos.OrderBy(p => p.CategoriaId).ThenBy(p => p.CodigoSku).ToList(),
                    CategoriasResumen = resumenCategorias,
                    StockPorSucursal = _stockSucursales.OrderBy(s => s.ProductoId).ThenBy(s => s.SucursalId).ToList(),
                    Movimientos = _movimientos.OrderByDescending(m => m.Fecha).ToList(),
                    Sucursales = _sucursales.ToList(),
                    Proveedores = _proveedores.ToList(),
                    OrdenesCompra = _ordenesCompra.OrderByDescending(o => o.FechaEmision).ToList()
                };
            }
        }

        public IActionResult Index()
        {
            var vm = ConstruirViewModel();
            return View(vm);
        }

        public IActionResult Productos()
        {
            var vm = ConstruirViewModel();
            return View(vm);
        }

        [HttpPost]
        public IActionResult CrearProducto(string codigoSku, string nombre, int categoriaId, int proveedorId, decimal precioCosto, decimal precio, int stockInicial, int stockMinimo)
        {
            lock (_lock)
            {
                var cat = _categorias.FirstOrDefault(c => c.Id == categoriaId) ?? _categorias.First();
                var prov = _proveedores.FirstOrDefault(p => p.Id == proveedorId) ?? _proveedores.First();
                int nuevoId = _productos.Any() ? _productos.Max(p => p.Id) + 1 : 1;

                var nuevoProd = new Producto
                {
                    Id = nuevoId,
                    CodigoSku = string.IsNullOrWhiteSpace(codigoSku) ? $"SKU-GEN-{nuevoId:D3}" : codigoSku.Trim().ToUpper(),
                    Nombre = nombre.Trim(),
                    CategoriaId = cat.Id,
                    CategoriaNombre = cat.Nombre,
                    ProveedorId = prov.Id,
                    ProveedorNombre = prov.RazonSocial,
                    PrecioCosto = precioCosto,
                    Precio = precio,
                    StockActual = stockInicial,
                    StockMinimo = stockMinimo,
                    StockOptimo = Math.Max(stockMinimo * 3, stockInicial)
                };

                _productos.Add(nuevoProd);

                // Distribuir el stock inicial en Casa Central y crear registros en las demás sucursales
                int idStock = _stockSucursales.Any() ? _stockSucursales.Max(s => s.Id) + 1 : 1;
                _stockSucursales.Add(new StockSucursal
                {
                    Id = idStock++,
                    ProductoId = nuevoProd.Id,
                    CodigoSku = nuevoProd.CodigoSku,
                    ProductoNombre = nuevoProd.Nombre,
                    CategoriaNombre = nuevoProd.CategoriaNombre,
                    SucursalId = 1,
                    SucursalNombre = "Central",
                    CantidadActual = stockInicial,
                    PuntoReorden = Math.Max(5, stockMinimo / 2)
                });

                if (stockInicial > 0)
                {
                    _movimientos.Add(new MovimientoStock
                    {
                        Id = _movimientos.Any() ? _movimientos.Max(m => m.Id) + 1 : 1,
                        ProductoId = nuevoProd.Id,
                        CodigoSku = nuevoProd.CodigoSku,
                        ProductoNombre = nuevoProd.Nombre,
                        CategoriaNombre = nuevoProd.CategoriaNombre,
                        SucursalId = 1,
                        SucursalNombre = "Central",
                        UsuarioNombre = User.Identity?.Name ?? "admin",
                        TipoMovimiento = "ENTRADA",
                        Cantidad = stockInicial,
                        Motivo = "Alta inicial de producto en catálogo",
                        Fecha = DateTime.Now
                    });
                }

                TempData["Mensaje"] = $"Producto '{nuevoProd.Nombre}' ({nuevoProd.CodigoSku}) registrado exitosamente con estado '{nuevoProd.ObtenerEstadoSemaforo()}'.";
            }

            return RedirectToAction(nameof(Productos));
        }

        [HttpPost]
        public IActionResult ActualizarStockProducto(int id, int stockActual, int stockMinimo, decimal precio)
        {
            lock (_lock)
            {
                var prod = _productos.FirstOrDefault(p => p.Id == id);
                if (prod != null)
                {
                    int diferencia = stockActual - prod.StockActual;
                    prod.StockActual = Math.Max(0, stockActual);
                    prod.StockMinimo = Math.Max(1, stockMinimo);
                    if (precio > 0) prod.Precio = precio;

                    var stockCentral = _stockSucursales.FirstOrDefault(s => s.ProductoId == id && s.SucursalId == 1);
                    if (stockCentral != null)
                    {
                        stockCentral.CantidadActual = Math.Max(0, stockCentral.CantidadActual + diferencia);
                    }

                    if (diferencia != 0)
                    {
                        _movimientos.Add(new MovimientoStock
                        {
                            Id = _movimientos.Any() ? _movimientos.Max(m => m.Id) + 1 : 1,
                            ProductoId = prod.Id,
                            CodigoSku = prod.CodigoSku,
                            ProductoNombre = prod.Nombre,
                            CategoriaNombre = prod.CategoriaNombre,
                            SucursalId = 1,
                            SucursalNombre = "Central",
                            UsuarioNombre = User.Identity?.Name ?? "admin",
                            TipoMovimiento = "AJUSTE",
                            Cantidad = diferencia,
                            Motivo = "Actualización de parámetros desde Catálogo",
                            Fecha = DateTime.Now
                        });
                    }

                    TempData["Mensaje"] = $"Producto '{prod.Nombre}' actualizado. Estado actual del semáforo: {prod.ObtenerEstadoSemaforo()}.";
                }
            }
            return RedirectToAction(nameof(Productos));
        }

        [HttpPost]
        public IActionResult EliminarProducto(int id)
        {
            lock (_lock)
            {
                var prod = _productos.FirstOrDefault(p => p.Id == id);
                if (prod != null)
                {
                    _productos.Remove(prod);
                    _stockSucursales.RemoveAll(s => s.ProductoId == id);
                    TempData["Mensaje"] = $"Producto '{prod.Nombre}' dado de baja correctamente.";
                }
            }
            return RedirectToAction(nameof(Productos));
        }

        public IActionResult Movimientos()
        {
            var vm = ConstruirViewModel();
            return View(vm);
        }

        [HttpPost]
        public IActionResult RegistrarMovimiento(string tipoMovimiento, int sucursalId, int productoId, int cantidad, string motivo)
        {
            lock (_lock)
            {
                var prod = _productos.FirstOrDefault(p => p.Id == productoId);
                var suc = _sucursales.FirstOrDefault(s => s.Id == sucursalId);

                if (prod != null && suc != null && cantidad > 0)
                {
                    int delta = tipoMovimiento == "SALIDA" ? -Math.Abs(cantidad) :
                                tipoMovimiento == "ENTRADA" ? Math.Abs(cantidad) : cantidad;

                    var regSucursal = _stockSucursales.FirstOrDefault(s => s.ProductoId == productoId && s.SucursalId == sucursalId);
                    if (regSucursal == null)
                    {
                        regSucursal = new StockSucursal
                        {
                            Id = _stockSucursales.Any() ? _stockSucursales.Max(s => s.Id) + 1 : 1,
                            ProductoId = prod.Id,
                            CodigoSku = prod.CodigoSku,
                            ProductoNombre = prod.Nombre,
                            CategoriaNombre = prod.CategoriaNombre,
                            SucursalId = suc.Id,
                            SucursalNombre = suc.Nombre,
                            CantidadActual = 0,
                            PuntoReorden = Math.Max(5, prod.StockMinimo / 3)
                        };
                        _stockSucursales.Add(regSucursal);
                    }

                    regSucursal.CantidadActual = Math.Max(0, regSucursal.CantidadActual + delta);
                    regSucursal.UltimaActualizacion = DateTime.Now;

                    // Recalcular StockActual consolidado del producto sumando todas las sucursales
                    prod.StockActual = _stockSucursales.Where(s => s.ProductoId == prod.Id).Sum(s => s.CantidadActual);

                    _movimientos.Add(new MovimientoStock
                    {
                        Id = _movimientos.Any() ? _movimientos.Max(m => m.Id) + 1 : 1,
                        ProductoId = prod.Id,
                        CodigoSku = prod.CodigoSku,
                        ProductoNombre = prod.Nombre,
                        CategoriaNombre = prod.CategoriaNombre,
                        SucursalId = suc.Id,
                        SucursalNombre = suc.Nombre,
                        UsuarioNombre = User.Identity?.Name ?? "admin",
                        TipoMovimiento = tipoMovimiento,
                        Cantidad = tipoMovimiento == "SALIDA" ? -Math.Abs(cantidad) : Math.Abs(cantidad),
                        Motivo = string.IsNullOrWhiteSpace(motivo) ? "Operación registrada en depósito" : motivo.Trim(),
                        Fecha = DateTime.Now
                    });

                    TempData["Mensaje"] = $"Movimiento ({tipoMovimiento}) de {cantidad} u. sobre '{prod.Nombre}' en {suc.Nombre} asentado. Nuevo stock consolidado: {prod.StockActual} u. ({prod.ObtenerEstadoSemaforo()}).";
                }
            }

            return RedirectToAction(nameof(Movimientos));
        }

        public IActionResult ProveedoresOrdenes()
        {
            var vm = ConstruirViewModel();
            return View(vm);
        }

        [HttpPost]
        public IActionResult CrearProveedor(string razonSocial, string cuit, string telefono, string email, string direccion)
        {
            lock (_lock)
            {
                if (!string.IsNullOrWhiteSpace(razonSocial))
                {
                    int nuevoId = _proveedores.Any() ? _proveedores.Max(p => p.Id) + 1 : 1;
                    _proveedores.Add(new Proveedor
                    {
                        Id = nuevoId,
                        RazonSocial = razonSocial.Trim(),
                        Cuit = string.IsNullOrWhiteSpace(cuit) ? $"30-{10000000 + nuevoId}-9" : cuit.Trim(),
                        Telefono = telefono ?? "-",
                        Email = email ?? "-",
                        Direccion = direccion ?? "Zona Industrial",
                        Activo = true
                    });
                    TempData["Mensaje"] = $"Proveedor '{razonSocial}' homologado correctamente.";
                }
            }
            return RedirectToAction(nameof(ProveedoresOrdenes));
        }

        [HttpPost]
        public IActionResult EmitirOrden(int proveedorId, int sucursalDestinoId, int productoId, int cantidad, decimal costoUnitario, string observaciones)
        {
            lock (_lock)
            {
                var prov = _proveedores.FirstOrDefault(p => p.Id == proveedorId) ?? _proveedores.First();
                var suc = _sucursales.FirstOrDefault(s => s.Id == sucursalDestinoId) ?? _sucursales.First();
                var prod = _productos.FirstOrDefault(p => p.Id == productoId) ?? _productos.First();

                int nuevoId = _ordenesCompra.Any() ? _ordenesCompra.Max(o => o.Id) + 1 : 1;
                string numOrden = $"OC-2026-{nuevoId:D3}";
                decimal costo = costoUnitario > 0 ? costoUnitario : prod.PrecioCosto;

                _ordenesCompra.Add(new OrdenCompra
                {
                    Id = nuevoId,
                    NumeroOrden = numOrden,
                    ProveedorId = prov.Id,
                    ProveedorNombre = prov.RazonSocial,
                    SucursalDestinoId = suc.Id,
                    SucursalDestinoNombre = suc.Nombre,
                    ProductoId = prod.Id,
                    ProductoNombre = prod.Nombre,
                    CantidadSolicitada = Math.Max(1, cantidad),
                    PrecioUnitario = costo,
                    Total = Math.Max(1, cantidad) * costo,
                    FechaEmision = DateTime.Now,
                    Estado = "PENDIENTE",
                    Observaciones = string.IsNullOrWhiteSpace(observaciones) ? "Reposición programada" : observaciones.Trim()
                });

                TempData["Mensaje"] = $"Orden de Compra {numOrden} emitida por {cantidad} u. de '{prod.Nombre}' hacia {suc.Nombre}.";
            }
            return RedirectToAction(nameof(ProveedoresOrdenes));
        }

        [HttpPost]
        public IActionResult RecibirOrden(int id)
        {
            lock (_lock)
            {
                var orden = _ordenesCompra.FirstOrDefault(o => o.Id == id);
                if (orden != null && orden.Estado == "PENDIENTE")
                {
                    orden.Estado = "RECIBIDA";
                    orden.FechaRecepcion = DateTime.Now;

                    var prod = _productos.FirstOrDefault(p => p.Id == orden.ProductoId);
                    var suc = _sucursales.FirstOrDefault(s => s.Id == orden.SucursalDestinoId);

                    if (prod != null && suc != null)
                    {
                        var regSucursal = _stockSucursales.FirstOrDefault(s => s.ProductoId == prod.Id && s.SucursalId == suc.Id);
                        if (regSucursal == null)
                        {
                            regSucursal = new StockSucursal
                            {
                                Id = _stockSucursales.Any() ? _stockSucursales.Max(s => s.Id) + 1 : 1,
                                ProductoId = prod.Id,
                                CodigoSku = prod.CodigoSku,
                                ProductoNombre = prod.Nombre,
                                CategoriaNombre = prod.CategoriaNombre,
                                SucursalId = suc.Id,
                                SucursalNombre = suc.Nombre,
                                CantidadActual = 0,
                                PuntoReorden = Math.Max(5, prod.StockMinimo / 3)
                            };
                            _stockSucursales.Add(regSucursal);
                        }

                        regSucursal.CantidadActual += orden.CantidadSolicitada;
                        regSucursal.UltimaActualizacion = DateTime.Now;
                        prod.StockActual = _stockSucursales.Where(s => s.ProductoId == prod.Id).Sum(s => s.CantidadActual);

                        _movimientos.Add(new MovimientoStock
                        {
                            Id = _movimientos.Any() ? _movimientos.Max(m => m.Id) + 1 : 1,
                            ProductoId = prod.Id,
                            CodigoSku = prod.CodigoSku,
                            ProductoNombre = prod.Nombre,
                            CategoriaNombre = prod.CategoriaNombre,
                            SucursalId = suc.Id,
                            SucursalNombre = suc.Nombre,
                            UsuarioNombre = User.Identity?.Name ?? "admin",
                            TipoMovimiento = "ENTRADA",
                            Cantidad = orden.CantidadSolicitada,
                            Motivo = $"Recepción de Orden de Compra {orden.NumeroOrden}",
                            Fecha = DateTime.Now
                        });

                        TempData["Mensaje"] = $"Mercadería de {orden.NumeroOrden} recibida: +{orden.CantidadSolicitada} u. de '{prod.Nombre}' ingresadas en {suc.Nombre}. Estado actual del producto: {prod.ObtenerEstadoSemaforo()}.";
                    }
                }
            }
            return RedirectToAction(nameof(ProveedoresOrdenes));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
