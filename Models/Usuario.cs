namespace Trayectos_Especiales.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        // Cambiamos "Nombre" por "NombreCompleto" como está en la base de datos
        public string NombreCompleto { get; set; } = string.Empty;
    }
}
