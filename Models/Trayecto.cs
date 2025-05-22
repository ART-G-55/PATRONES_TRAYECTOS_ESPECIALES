using System.ComponentModel.DataAnnotations;

namespace Trayectos_Especiales.Models
{
    public class Trayecto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha de solicitud es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Solicitud")]
        public DateTime FechaSolicitud { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un usuario.")]
        [Display(Name = "Usuario")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un origen.")]
        [Display(Name = "Origen")]
        public int OrigenId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un destino.")]
        [Display(Name = "Destino")]
        public int DestinoId { get; set; }

        [Required(ErrorMessage = "La fecha del servicio es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha del Servicio")]
        public DateTime FechaServicio { get; set; }

        [Required(ErrorMessage = "La hora del servicio es obligatoria.")]
        [DataType(DataType.Time)]
        [Display(Name = "Hora del Servicio")]
        public String HoraServicio { get; set; }
        // Propiedades auxiliares para mostrar nombres
        public string NombreUsuario { get; set; }
        public string NombreOrigen { get; set; }
        public string NombreDestino { get; set; }
        public string Estado { get; set; }

    }
}
