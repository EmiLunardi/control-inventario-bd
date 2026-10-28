namespace ControlInventario.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string CodigoSku { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public int? ProveedorId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal PrecioCosto { get; set; }
        public decimal Precio { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public bool Activo { get; set; } = true;
    }
}
