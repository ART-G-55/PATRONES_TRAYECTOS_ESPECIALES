namespace Trayectos_Especiales.Models
{
    public class TrayectoPendiente
    {

        public int Id { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string NombreUsuario { get; set; }
        public string NombreOrigen { get; set; }
        public string NombreDestino { get; set; }
        public DateTime FechaServicio { get; set; }
        public string HoraServicio { get; set; }
        public string Estado { get; set; }
    }
}
