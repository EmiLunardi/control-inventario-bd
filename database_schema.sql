-- Modelo Canónico - Control de Inventario y Stock
-- Script para crear la base de datos y sus tablas en PostgreSQL

-- 1. Tabla de Usuarios (Para autenticación y permisos)
CREATE TABLE IF NOT EXISTS Usuarios (
    Id SERIAL PRIMARY KEY,
    NombreUsuario VARCHAR(50) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL, -- Para simplificar en esta entrega usaremos texto plano o un hash simple simulado, pero el nombre denota seguridad.
    Rol VARCHAR(20) NOT NULL DEFAULT 'Usuario'
);

-- 2. Tabla de Categorias
CREATE TABLE IF NOT EXISTS Categorias (
    Id SERIAL PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion TEXT
);

-- 3. Tabla de Productos
CREATE TABLE IF NOT EXISTS Productos (
    Id SERIAL PRIMARY KEY,
    CategoriaId INTEGER REFERENCES Categorias(Id),
    Nombre VARCHAR(150) NOT NULL,
    Precio DECIMAL(10, 2) NOT NULL,
    StockActual INTEGER NOT NULL DEFAULT 0,
    StockMinimo INTEGER NOT NULL DEFAULT 10
);

-- 4. Tabla de Sucursales
CREATE TABLE IF NOT EXISTS Sucursales (
    Id SERIAL PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Ubicacion VARCHAR(200) NOT NULL
);

-- 5. Tabla de Movimientos de Stock (Registro histórico/Inventario)
CREATE TABLE IF NOT EXISTS MovimientosStock (
    Id SERIAL PRIMARY KEY,
    ProductoId INTEGER REFERENCES Productos(Id),
    SucursalId INTEGER REFERENCES Sucursales(Id),
    TipoMovimiento VARCHAR(20) NOT NULL, -- 'ENTRADA' o 'SALIDA'
    Cantidad INTEGER NOT NULL,
    Fecha TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ==========================================
-- Inserción de Datos Iniciales (Mock Data)
-- ==========================================

-- Insertar un usuario para el sistema de inicio de sesión (Contraseña: admin123)
INSERT INTO Usuarios (NombreUsuario, PasswordHash, Rol) 
VALUES ('admin', 'admin123', 'Administrador')
ON CONFLICT DO NOTHING;

-- Categorias
INSERT INTO Categorias (Nombre, Descripcion) VALUES
('Bebidas', 'Bebidas alcohólicas y sin alcohol'),
('Almacén', 'Productos no perecederos'),
('Lácteos', 'Productos derivados de la leche'),
('Limpieza', 'Artículos de limpieza para el hogar');

-- Productos
INSERT INTO Productos (CategoriaId, Nombre, Precio, StockActual, StockMinimo) VALUES
(1, 'Agua Mineral 1.5L', 800.00, 150, 50),
(1, 'Cerveza Lager 1L', 2500.00, 30, 100), -- Alerta: bajo stock
(2, 'Arroz Blanco 1kg', 1200.00, 200, 30),
(2, 'Fideos Espagueti 500g', 900.00, 80, 40),
(3, 'Leche Entera 1L', 1100.00, 15, 50), -- Alerta crítica: bajo stock
(4, 'Lavandina 1L', 850.00, 120, 20);

-- Sucursales
INSERT INTO Sucursales (Nombre, Ubicacion) VALUES
('Central', 'Av. Corrientes 1234, CABA'),
('Sucursal Norte', 'Av. Cabildo 4500, CABA'),
('Sucursal Sur', 'Av. Mitre 500, Avellaneda');

-- Movimientos de Stock (Ejemplos)
INSERT INTO MovimientosStock (ProductoId, SucursalId, TipoMovimiento, Cantidad, Fecha) VALUES
(1, 1, 'ENTRADA', 100, CURRENT_TIMESTAMP - INTERVAL '5 days'),
(1, 2, 'SALIDA', 20, CURRENT_TIMESTAMP - INTERVAL '2 days'),
(2, 1, 'ENTRADA', 50, CURRENT_TIMESTAMP - INTERVAL '10 days'),
(2, 3, 'SALIDA', 20, CURRENT_TIMESTAMP - INTERVAL '1 day'),
(3, 1, 'ENTRADA', 200, CURRENT_TIMESTAMP - INTERVAL '15 days');
