namespace ControlInventario.Models
{
    public class MovimientoStock
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public int SucursalId { get; set; }
        public int? UsuarioId { get; set; }
        public string TipoMovimiento { get; set; } = string.Empty; // 'ENTRADA', 'SALIDA', 'AJUSTE'
        public int Cantidad { get; set; }
        public string? Motivo { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
