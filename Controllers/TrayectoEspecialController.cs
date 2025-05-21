using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Trayectos_Especiales.Models;

namespace Trayectos_Especiales.Controllers
{
    public class TrayectoEspecialController : Controller
    {
        private readonly NpgsqlConnection _connection;

        public TrayectoEspecialController(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        [HttpGet]
        public IActionResult Crear()
        {
            var usuarios = new List<Usuario>();
            var lugares = new List<OrigenDestino>();

            _connection.Open();

            // Obtener usuarios
            using (var cmd = new NpgsqlCommand("SELECT id, nombrecompleto FROM tbl_users", _connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    usuarios.Add(new Usuario
                    {
                        Id = reader.GetInt32(0),
                        NombreCompleto = reader.GetString(1)
                    });
                }
            }

            // Obtener lugares activos (orígenes y destinos)
            using (var cmd = new NpgsqlCommand("SELECT id, nombrelugar FROM tbl_origendestino WHERE esactivo = true", _connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lugares.Add(new OrigenDestino
                    {
                        Id = reader.GetInt32(0),
                        NombreLugar = reader.GetString(1),
                        EsActivo = true
                    });
                }
            }

            _connection.Close();

            ViewBag.Usuarios = usuarios;
            ViewBag.Lugares = lugares;

            return View();
        }

        [HttpPost]
        public IActionResult Crear(Trayecto trayecto)
        {
            try
            {
                _connection.Open();

                using (var cmd = new NpgsqlCommand("SELECT insertar_trayecto(@fechasolicitud, @usuarioid, @origenid, @destinoid, @fechaservicio, @horaservicio)", _connection))
                {
                    cmd.Parameters.AddWithValue("fechasolicitud", trayecto.FechaSolicitud);
                    cmd.Parameters.AddWithValue("usuarioid", trayecto.UsuarioId);
                    cmd.Parameters.AddWithValue("origenid", trayecto.OrigenId);
                    cmd.Parameters.AddWithValue("destinoid", trayecto.DestinoId);
                    cmd.Parameters.AddWithValue("fechaservicio", trayecto.FechaServicio);
                    cmd.Parameters.AddWithValue("horaservicio", trayecto.HoraServicio);

                    cmd.ExecuteNonQuery();
                }

                _connection.Close();

                TempData["Mensaje"] = "Trayecto guardado exitosamente.";
                return RedirectToAction("Crear");
            }
            catch (Exception ex)
            {
                _connection.Close();
                ViewBag.Error = "Error al guardar el trayecto: " + ex.Message;

                // Recargar datos para la vista en caso de error
                var usuarios = new List<Usuario>();
                var lugares = new List<OrigenDestino>();

                _connection.Open();

                using (var cmd = new NpgsqlCommand("SELECT id, nombrecompleto FROM tbl_users", _connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        usuarios.Add(new Usuario
                        {
                            Id = reader.GetInt32(0),
                            NombreCompleto = reader.GetString(1)
                        });
                    }
                }

                using (var cmd = new NpgsqlCommand("SELECT id, nombrelugar FROM tbl_origendestino WHERE esactivo = true", _connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lugares.Add(new OrigenDestino
                        {
                            Id = reader.GetInt32(0),
                            NombreLugar = reader.GetString(1),
                            EsActivo = true
                        });
                    }
                }

                _connection.Close();

                ViewBag.Usuarios = usuarios;
                ViewBag.Lugares = lugares;

                return View(trayecto);
            }
        }
    }
}
