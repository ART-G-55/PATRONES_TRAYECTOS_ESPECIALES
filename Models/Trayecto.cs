using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Trayectos_Especiales.Models
{
    public class Trayecto
    {
        public int Id { get; set; }

       
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Solicitud")]
        public DateTime FechaSolicitud { get; set; }

      
        [Display(Name = "Usuario")]
        public int UsuarioId { get; set; }

       
        [Display(Name = "Origen")]
        public int OrigenId { get; set; }

       
        [Display(Name = "Destino")]
        public int DestinoId { get; set; }

       
        [DataType(DataType.Date)]
        [Display(Name = "Fecha del Servicio")]
        public DateTime FechaServicio { get; set; }

       
        [DataType(DataType.Time)]
        [Display(Name = "Hora del Servicio")]
        public String HoraServicio { get; set; }

        [BindNever]
        public string? NombreUsuario { get; set; }

        [BindNever]
        public string? NombreOrigen { get; set; }

        [BindNever]
        public string? NombreDestino { get; set; }

        [BindNever]
        public string? Estado { get; set; }
    }
}
