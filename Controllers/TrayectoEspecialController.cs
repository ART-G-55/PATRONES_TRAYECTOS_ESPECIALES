using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using Trayectos_Especiales.Models;

namespace Trayectos_Especiales.Controllers
{
    public class TrayectoEspecialController : Controller
    {
        private readonly IConfiguration _configuration;

        public TrayectoEspecialController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Crear()
        {
            CargarUsuariosYLugares();
            return View();
        }

        // Método para cargar usuarios y lugares en ViewBag como SelectListItems para dropdowns
        private void CargarUsuariosYLugares()
        {
            var usuarios = new List<SelectListItem>();
            var lugares = new List<SelectListItem>();

            using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("PostgresConnection")))
            {
                connection.Open();

                using (var cmd = new NpgsqlCommand("SELECT id, nombrecompleto FROM tbl_users", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        usuarios.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }

                using (var cmd = new NpgsqlCommand("SELECT id, nombrelugar FROM tbl_origendestino WHERE esactivo = true", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lugares.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }
            }

            ViewBag.Usuarios = usuarios;
            ViewBag.Lugares = lugares;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Trayecto trayecto)
        {
            if (trayecto.OrigenId == trayecto.DestinoId)
                ModelState.AddModelError("", "El origen no puede ser igual al destino.");

            if (trayecto.FechaServicio < DateTime.Today)
                ModelState.AddModelError("", "La fecha del servicio debe ser igual o mayor a la actual.");

            if (!ModelState.IsValid)
            {
                CargarUsuariosYLugares();
                return View(trayecto);
            }

            try
            {
                using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("PostgresConnection")))
                {
                    connection.Open();

                    using (var cmd = new NpgsqlCommand("SELECT insertar_trayecto(@fechasolicitud, @idusuario, @idorigen, @iddestino, @fechaservicio, @horaservicio)", connection))
                    {
                        cmd.Parameters.AddWithValue("fechasolicitud", trayecto.FechaSolicitud);
                        cmd.Parameters.AddWithValue("idusuario", trayecto.UsuarioId);
                        cmd.Parameters.AddWithValue("idorigen", trayecto.OrigenId);
                        cmd.Parameters.AddWithValue("iddestino", trayecto.DestinoId);
                        cmd.Parameters.AddWithValue("fechaservicio", trayecto.FechaServicio);

                        TimeSpan horaServicio;
                        if (!TimeSpan.TryParse(trayecto.HoraServicio, out horaServicio))
                        {
                            ModelState.AddModelError("", "Hora del servicio inválida.");
                            CargarUsuariosYLugares();
                            return View(trayecto);
                        }

                        cmd.Parameters.AddWithValue("horaservicio", horaServicio);

                        cmd.ExecuteScalar();
                    }
                }

                return RedirectToAction("Listado");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al guardar el trayecto: " + ex.Message;
                CargarUsuariosYLugares();
                return View(trayecto);
            }
        }

        [HttpGet]
        public IActionResult Listado()
        {
            var trayectos = new List<Trayecto>();

            using var connection = new NpgsqlConnection(_configuration.GetConnectionString("PostgresConnection"));
            connection.Open();

            using var cmd = new NpgsqlCommand("SELECT * FROM obtener_trayectos()", connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                trayectos.Add(new Trayecto
                {
                    FechaSolicitud = reader.GetDateTime(0),
                    NombreUsuario = reader.GetString(1),
                    NombreOrigen = reader.GetString(2),
                    NombreDestino = reader.GetString(3),
                    FechaServicio = reader.GetDateTime(4),
                    HoraServicio = reader.GetTimeSpan(5).ToString()
                });
            }

            return View(trayectos);
        }

        [HttpPost]
        public IActionResult ConfirmarEstado(int idTrayecto, bool confirmar)
        {
            try
            {
                using var connection = new NpgsqlConnection(_configuration.GetConnectionString("PostgresConnection"));
                connection.Open();

                using var command = new NpgsqlCommand("CALL sp_cambiar_estado_trayecto(@p_id_trayecto, @p_confirmar)", connection);
                command.Parameters.AddWithValue("p_id_trayecto", idTrayecto);
                command.Parameters.AddWithValue("p_confirmar", confirmar);

                command.ExecuteNonQuery();

                TempData["Mensaje"] = confirmar ? "Trayecto aprobado exitosamente." : "Trayecto rechazado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar el estado: " + ex.Message;
            }

            return RedirectToAction("ConfirmarEstado");
        }

        [HttpGet]
        public IActionResult ConfirmarEstado()
        {
            var trayectos = new List<TrayectoPendiente>();

            using var connection = new NpgsqlConnection(_configuration.GetConnectionString("PostgresConnection"));
            connection.Open();

            using var cmd = new NpgsqlCommand("SELECT * FROM sp_listar_trayectos_por_estado(@p_nombre_estado)", connection);
            cmd.Parameters.AddWithValue("p_nombre_estado", "PENDIENTE");

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                trayectos.Add(new TrayectoPendiente
                {
                    Id = reader.GetInt32(0),
                    FechaSolicitud = reader.GetDateTime(1),
                    NombreUsuario = reader.GetString(2),
                    NombreOrigen = reader.GetString(3),
                    NombreDestino = reader.GetString(4),
                    FechaServicio = reader.GetDateTime(5),
                    HoraServicio = reader.GetTimeSpan(6).ToString(),
                    Estado = reader.GetString(7)
                });
            }

            return View(trayectos);
        }
    }
}
