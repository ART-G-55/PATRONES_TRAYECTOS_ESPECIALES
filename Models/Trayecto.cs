namespace Trayectos_Especiales.Models
{
    public class Trayecto
    {
        public int Id { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public int UsuarioId { get; set; }
        public int OrigenId { get; set; }
        public int DestinoId { get; set; }
        public DateTime FechaServicio { get; set; }
        public TimeSpan HoraServicio { get; set; }
        public int EstadoId { get; set; }
    }
}
