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

        /// <summary>
        /// Calcula el estado del semáforo aplicando la regla del 10% sobre el PuntoReorden de la sucursal.
        /// </summary>
        public string ObtenerEstadoSemaforo()
        {
            if (CantidadActual <= PuntoReorden)
                return "Crítico";

            if (CantidadActual <= PuntoReorden * 1.10)
                return "Advertencia";

            return "Óptimo";
        }
    }
}
