namespace ControlInventario.Models
{
    public class OrdenCompra
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public int ProveedorId { get; set; }
        public int SucursalDestinoId { get; set; }
        public int? UsuarioId { get; set; }
        public DateTime FechaEmision { get; set; } = DateTime.Now;
        public DateTime? FechaRecepcion { get; set; }
        public string Estado { get; set; } = "PENDIENTE"; // 'PENDIENTE', 'RECIBIDA', 'CANCELADA'
        public decimal Total { get; set; }
        public string? Observaciones { get; set; }
    }
}
