# Modelo Canónico de Base de Datos - Control de Inventario y Stock

El modelo de datos relacional para el sistema de Control de Inventario se compone de **cinco entidades** principales que permiten la gestión eficiente de los productos, el control de stock, la organización en categorías, las distintas sucursales y la autenticación de usuarios para el tablero.

## Entidades y Relaciones

### 1. `Usuarios`
Maneja la autenticación y el control de acceso al tablero.
- **Id** (PK): Identificador único (SERIAL).
- **NombreUsuario**: Nombre del usuario (UNIQUE, VARCHAR).
- **PasswordHash**: Contraseña del usuario (VARCHAR).
- **Rol**: Rol del usuario (ej. 'Administrador', 'Usuario').

### 2. `Categorias`
Agrupa los productos en distintas familias para facilitar el filtrado y visualización.
- **Id** (PK): Identificador único (SERIAL).
- **Nombre**: Nombre de la categoría (VARCHAR).
- **Descripcion**: Detalle adicional de la categoría (TEXT).

### 3. `Productos`
Almacena la información principal de los artículos que maneja la distribuidora.
- **Id** (PK): Identificador único (SERIAL).
- **CategoriaId** (FK): Clave foránea referenciando a `Categorias(Id)`.
- **Nombre**: Nombre del producto (VARCHAR).
- **Precio**: Precio de venta (DECIMAL).
- **StockActual**: Cantidad disponible actualmente (INTEGER).
- **StockMinimo**: Punto de reorden, utilizado para los indicadores visuales (Semaforización).

### 4. `Sucursales`
Representa los puntos de almacenamiento o distribución físicos.
- **Id** (PK): Identificador único (SERIAL).
- **Nombre**: Nombre comercial de la sucursal (VARCHAR).
- **Ubicacion**: Dirección o zona geográfica (VARCHAR).

### 5. `MovimientosStock`
Registra el historial de entradas y salidas de mercadería, permitiendo el desglose (Drill Down) temporal o por sucursal.
- **Id** (PK): Identificador único (SERIAL).
- **ProductoId** (FK): Clave foránea referenciando a `Productos(Id)`.
- **SucursalId** (FK): Clave foránea referenciando a `Sucursales(Id)`.
- **TipoMovimiento**: Determina si es una 'ENTRADA' o 'SALIDA' (VARCHAR).
- **Cantidad**: Unidades movidas (INTEGER).
- **Fecha**: Fecha y hora exacta del movimiento (TIMESTAMP).

## Explicación de Relevancia
Estas entidades fueron seleccionadas porque son **fundamentales para cualquier proceso de logística y distribución**.
- **Productos** y **Categorias** permiten un análisis de inventario agrupado, clave para un nivel inicial del *Dashboard*.
- **Sucursales** ofrece un eje geográfico para distribuir la información de ventas/movimientos.
- **MovimientosStock** es el nivel transaccional (más profundo del *Drill Down*) donde un gerente puede auditar exactamente de dónde salió o entró la mercadería.
- **Usuarios** cumple con el requisito estricto del sistema de Autenticación y Login de la entrega.
