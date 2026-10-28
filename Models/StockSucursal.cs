namespace ControlInventario.Models
{
    public class StockSucursal
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public int SucursalId { get; set; }
        public int CantidadActual { get; set; }
        public int PuntoReorden { get; set; }
        public DateTime UltimaActualizacion { get; set; } = DateTime.Now;
    }
}
