-- =========================================================================
-- Modelo Canónico Completo - Control de Inventario y Stock (12 Entidades)
-- Motor: PostgreSQL
-- Nota de Seguridad: Las contraseñas se almacenan encriptadas con SHA-256
-- =========================================================================

-- 1. Tabla de Roles (Seguridad RBAC)
CREATE TABLE IF NOT EXISTS Roles (
    Id SERIAL PRIMARY KEY,
    Nombre VARCHAR(50) UNIQUE NOT NULL,
    Descripcion TEXT
);

-- 2. Tabla de Permisos (Seguridad RBAC)
CREATE TABLE IF NOT EXISTS Permisos (
    Id SERIAL PRIMARY KEY,
    Nombre VARCHAR(80) UNIQUE NOT NULL,
    Descripcion TEXT
);

-- 3. Tabla Intermedia RolesPermisos (Relación N:M)
CREATE TABLE IF NOT EXISTS RolesPermisos (
    RolId INTEGER NOT NULL REFERENCES Roles(Id) ON DELETE CASCADE,
    PermisoId INTEGER NOT NULL REFERENCES Permisos(Id) ON DELETE CASCADE,
    PRIMARY KEY (RolId, PermisoId)
);

-- 4. Tabla de Sucursales
CREATE TABLE IF NOT EXISTS Sucursales (
    Id SERIAL PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Ubicacion VARCHAR(200) NOT NULL,
    Telefono VARCHAR(50),
    Activa BOOLEAN NOT NULL DEFAULT TRUE
);

-- 5. Tabla de Usuarios (Con contraseña encriptada mediante SHA-256)
CREATE TABLE IF NOT EXISTS Usuarios (
    Id SERIAL PRIMARY KEY,
    SucursalId INTEGER REFERENCES Sucursales(Id),
    RolId INTEGER REFERENCES Roles(Id),
    NombreUsuario VARCHAR(50) UNIQUE NOT NULL,
    Email VARCHAR(120),
    PasswordHash VARCHAR(64) NOT NULL, -- Hash SHA-256 hexadecimal (64 caracteres)
    Rol VARCHAR(30) NOT NULL DEFAULT 'Administrador',
    Activo BOOLEAN NOT NULL DEFAULT TRUE,
    FechaCreacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 6. Tabla de Categorias
CREATE TABLE IF NOT EXISTS Categorias (
    Id SERIAL PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion TEXT,
    Activa BOOLEAN NOT NULL DEFAULT TRUE
);

-- 7. Tabla de Proveedores
CREATE TABLE IF NOT EXISTS Proveedores (
    Id SERIAL PRIMARY KEY,
    RazonSocial VARCHAR(150) NOT NULL,
    Cuit VARCHAR(20) UNIQUE NOT NULL,
    Telefono VARCHAR(50),
    Email VARCHAR(120),
    Direccion VARCHAR(200),
    Activo BOOLEAN NOT NULL DEFAULT TRUE
);

-- 8. Tabla de Productos
CREATE TABLE IF NOT EXISTS Productos (
    Id SERIAL PRIMARY KEY,
    CodigoSku VARCHAR(30) UNIQUE,
    CategoriaId INTEGER REFERENCES Categorias(Id),
    ProveedorId INTEGER REFERENCES Proveedores(Id),
    Nombre VARCHAR(150) NOT NULL,
    Descripcion TEXT,
    PrecioCosto DECIMAL(10, 2) NOT NULL DEFAULT 0.00,
    Precio DECIMAL(10, 2) NOT NULL,
    StockActual INTEGER NOT NULL DEFAULT 0,
    StockMinimo INTEGER NOT NULL DEFAULT 10,
    Activo BOOLEAN NOT NULL DEFAULT TRUE
);

-- 9. Tabla de StockSucursales (Existencias desglosadas por sucursal y producto)
CREATE TABLE IF NOT EXISTS StockSucursales (
    Id SERIAL PRIMARY KEY,
    ProductoId INTEGER NOT NULL REFERENCES Productos(Id) ON DELETE CASCADE,
    SucursalId INTEGER NOT NULL REFERENCES Sucursales(Id) ON DELETE CASCADE,
    CantidadActual INTEGER NOT NULL DEFAULT 0,
    PuntoReorden INTEGER NOT NULL DEFAULT 10,
    UltimaActualizacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (ProductoId, SucursalId)
);

-- 10. Tabla de Movimientos de Stock (Auditoría de Entradas, Salidas y Ajustes)
CREATE TABLE IF NOT EXISTS MovimientosStock (
    Id SERIAL PRIMARY KEY,
    ProductoId INTEGER NOT NULL REFERENCES Productos(Id),
    SucursalId INTEGER NOT NULL REFERENCES Sucursales(Id),
    UsuarioId INTEGER REFERENCES Usuarios(Id),
    TipoMovimiento VARCHAR(20) NOT NULL, -- 'ENTRADA', 'SALIDA', 'AJUSTE'
    Cantidad INTEGER NOT NULL,
    Motivo VARCHAR(250),
    Fecha TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 11. Tabla de OrdenesCompra (Cabecera de pedidos de reposición a proveedores)
CREATE TABLE IF NOT EXISTS OrdenesCompra (
    Id SERIAL PRIMARY KEY,
    NumeroOrden VARCHAR(30) UNIQUE NOT NULL,
    ProveedorId INTEGER NOT NULL REFERENCES Proveedores(Id),
    SucursalDestinoId INTEGER NOT NULL REFERENCES Sucursales(Id),
    UsuarioId INTEGER REFERENCES Usuarios(Id),
    FechaEmision TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaRecepcion TIMESTAMP,
    Estado VARCHAR(20) NOT NULL DEFAULT 'PENDIENTE', -- 'PENDIENTE', 'RECIBIDA', 'CANCELADA'
    Total DECIMAL(12, 2) NOT NULL DEFAULT 0.00,
    Observaciones TEXT
);

-- 12. Tabla de DetallesOrdenCompra (Renglones de cada orden de compra)
CREATE TABLE IF NOT EXISTS DetallesOrdenCompra (
    Id SERIAL PRIMARY KEY,
    OrdenCompraId INTEGER NOT NULL REFERENCES OrdenesCompra(Id) ON DELETE CASCADE,
    ProductoId INTEGER NOT NULL REFERENCES Productos(Id),
    CantidadSolicitada INTEGER NOT NULL,
    PrecioUnitario DECIMAL(10, 2) NOT NULL
);

-- ==========================================
-- Inserción de Datos Iniciales (Mock Data)
-- ==========================================

-- Roles y Permisos
INSERT INTO Roles (Nombre, Descripcion) VALUES
('Administrador', 'Acceso total al sistema y configuración'),
('Gerente Logística', 'Supervisión de stock, proveedores y órdenes de compra'),
('Operador Depósito', 'Registro de entradas y salidas de mercadería por sucursal')
ON CONFLICT (Nombre) DO NOTHING;

INSERT INTO Permisos (Nombre, Descripcion) VALUES
('VER_DASHBOARD', 'Visualizar tablero de control y semaforización'),
('GESTIONAR_PRODUCTOS', 'Alta, baja y modificación de catálogo de productos'),
('REGISTRAR_MOVIMIENTOS', 'Asentar entradas, salidas y ajustes de stock'),
('EMITIR_ORDENES_COMPRA', 'Gestionar proveedores y emitir órdenes de reposición')
ON CONFLICT (Nombre) DO NOTHING;

-- Sucursales
INSERT INTO Sucursales (Nombre, Ubicacion, Telefono) VALUES
('Central', 'Av. Corrientes 1234, CABA', '011-4381-1000'),
('Sucursal Norte', 'Av. Cabildo 4500, CABA', '011-4701-2000'),
('Sucursal Sur', 'Av. Mitre 500, Avellaneda', '011-4201-3000');

-- Usuarios con Contraseña Encriptada en SHA-256:
-- 'admin123' -> 240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9
INSERT INTO Usuarios (SucursalId, RolId, NombreUsuario, Email, PasswordHash, Rol) 
VALUES (
    1,
    1,
    'admin',
    'admin@distribuidora.com.ar',
    '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9',
    'Administrador'
)
ON CONFLICT (NombreUsuario) DO UPDATE 
SET PasswordHash = '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9';

-- Categorias
INSERT INTO Categorias (Nombre, Descripcion) VALUES
('Bebidas', 'Bebidas alcohólicas y sin alcohol'),
('Almacén', 'Productos secos y no perecederos'),
('Lácteos', 'Productos derivados de la leche refrigerados'),
('Limpieza', 'Artículos de limpieza para el hogar e industria');

-- Proveedores
INSERT INTO Proveedores (RazonSocial, Cuit, Telefono, Email, Direccion) VALUES
('Distribuidora Quilmes S.A.', '30-54892104-9', '011-4321-8890', 'ventas@distquilmes.com.ar', 'Av. Calchaquí 3100, Quilmes'),
('Molinos del Sur S.R.L.', '30-61029384-2', '011-4582-1102', 'pedidos@molinosdelsur.com.ar', 'Ruta 205 Km 42, Ezeiza'),
('Lácteos La Serenísima S.A.', '30-50129381-7', '0800-555-5228', 'mayoristas@laserenisima.com.ar', 'Parque Industrial Gral. Rodríguez'),
('Química Limpia S.A.', '30-70918273-4', '011-4756-9012', 'contacto@quimicalimpia.com.ar', 'Calle 14 N° 850, San Martín')
ON CONFLICT (Cuit) DO NOTHING;

-- Productos
INSERT INTO Productos (CodigoSku, CategoriaId, ProveedorId, Nombre, PrecioCosto, Precio, StockActual, StockMinimo) VALUES
('SKU-BEB-001', 1, 1, 'Agua Mineral 1.5L', 520.00, 800.00, 150, 50),
('SKU-BEB-002', 1, 1, 'Cerveza Lager 1L', 1650.00, 2500.00, 30, 100),
('SKU-ALM-001', 2, 2, 'Arroz Blanco 1kg', 780.00, 1200.00, 200, 30),
('SKU-ALM-002', 2, 2, 'Fideos Espagueti 500g', 580.00, 900.00, 80, 40),
('SKU-LAC-001', 3, 3, 'Leche Entera 1L', 720.00, 1100.00, 15, 50),
('SKU-LIM-001', 4, 4, 'Lavandina Concentrada 1L', 490.00, 850.00, 120, 20)
ON CONFLICT (CodigoSku) DO NOTHING;

-- StockSucursales
INSERT INTO StockSucursales (ProductoId, SucursalId, CantidadActual, PuntoReorden) VALUES
(1, 1, 90, 30),
(1, 2, 60, 20),
(2, 3, 30, 100),
(3, 1, 200, 30),
(5, 1, 15, 50)
ON CONFLICT (ProductoId, SucursalId) DO NOTHING;

-- Movimientos de Stock
INSERT INTO MovimientosStock (ProductoId, SucursalId, UsuarioId, TipoMovimiento, Cantidad, Motivo, Fecha) VALUES
(1, 1, 1, 'ENTRADA', 100, 'Recepción OC-2026-001', CURRENT_TIMESTAMP - INTERVAL '5 days'),
(1, 2, 1, 'SALIDA', 20, 'Despacho a cliente mayorista', CURRENT_TIMESTAMP - INTERVAL '2 days'),
(2, 1, 1, 'ENTRADA', 50, 'Ingreso inicial', CURRENT_TIMESTAMP - INTERVAL '10 days'),
(2, 3, 1, 'SALIDA', 20, 'Despacho Pedido #4412', CURRENT_TIMESTAMP - INTERVAL '1 day'),
(3, 1, 1, 'ENTRADA', 200, 'Recepción OC-2026-002', CURRENT_TIMESTAMP - INTERVAL '15 days');

-- Ordenes de Compra y sus Detalles
INSERT INTO OrdenesCompra (NumeroOrden, ProveedorId, SucursalDestinoId, UsuarioId, Estado, Total, Observaciones) VALUES
('OC-2026-001', 1, 1, 1, 'RECIBIDA', 52000.00, 'Abastecimiento mensual bebidas'),
('OC-2026-003', 1, 3, 1, 'PENDIENTE', 198000.00, 'Reposición urgente por stock crítico')
ON CONFLICT (NumeroOrden) DO NOTHING;

INSERT INTO DetallesOrdenCompra (OrdenCompraId, ProductoId, CantidadSolicitada, PrecioUnitario) VALUES
(1, 1, 100, 520.00),
(2, 2, 120, 1650.00);
