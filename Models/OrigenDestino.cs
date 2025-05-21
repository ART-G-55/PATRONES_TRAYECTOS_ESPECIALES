namespace Trayectos_Especiales.Models
{
    public class OrigenDestino
    {
        public int Id { get; set; }
        public string Lugar { get; set; }
        public bool Activo { get; set; }
        // Cambiamos "Lugar" por "NombreLugar" y "Activo" por "EsActivo"
        public string NombreLugar { get; set; } = string.Empty;

        public bool EsActivo { get; set; }
    }
}
