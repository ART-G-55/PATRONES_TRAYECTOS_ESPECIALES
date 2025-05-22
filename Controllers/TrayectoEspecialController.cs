using Microsoft.AspNetCore.Mvc;
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
            var usuarios = new List<Usuario>();
            var lugares = new List<OrigenDestino>();

            using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("PostgresConnection")))
            {
                connection.Open();

                using (var cmd = new NpgsqlCommand("SELECT id, nombrecompleto FROM tbl_users", connection))
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

                using (var cmd = new NpgsqlCommand("SELECT id, nombrelugar FROM tbl_origendestino WHERE esactivo = true", connection))
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
            }

            ViewBag.Usuarios = usuarios;
            ViewBag.Lugares = lugares;

            return View();
        }

        [HttpPost]
        public IActionResult Crear(Trayecto trayecto)
        {
            // Validar origen y destino
            if (trayecto.OrigenId == trayecto.DestinoId)
            {
                ModelState.AddModelError("", "El origen no puede ser igual al destino.");
            }

            // Validar fecha del servicio (debe ser hoy o después)
            if (trayecto.FechaServicio < DateTime.Today)
            {
                ModelState.AddModelError("", "La fecha del servicio debe ser igual o mayor a la fecha actual.");
            }

            // Validar duplicados (usuario, fecha y hora)
            using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("PostgresConnection")))
            {
                connection.Open();

                using (var cmd = new NpgsqlCommand(
                    "SELECT COUNT(*) FROM tbl_trayectos WHERE id_usuario = @usuario AND fechaservicio = @fecha AND horaservicio = @hora",
                    connection))
                {
                    cmd.Parameters.AddWithValue("usuario", trayecto.UsuarioId);
                    cmd.Parameters.AddWithValue("fecha", trayecto.FechaServicio);
                    cmd.Parameters.AddWithValue("hora", TimeSpan.Parse(trayecto.HoraServicio));

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if (count > 0)
                    {
                        ModelState.AddModelError("", "Ya existe un trayecto para este usuario en la misma fecha y hora.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                // Cargar usuarios y lugares para volver a la vista con datos
                var usuarios = new List<Usuario>();
                var lugares = new List<OrigenDestino>();

                using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("PostgresConnection")))
                {
                    connection.Open();

                    using (var cmd = new NpgsqlCommand("SELECT id, nombrecompleto FROM tbl_users", connection))
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

                    using (var cmd = new NpgsqlCommand("SELECT id, nombrelugar FROM tbl_origendestino WHERE esactivo = true", connection))
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
                }

                ViewBag.Usuarios = usuarios;
                ViewBag.Lugares = lugares;

                return View(trayecto);
            }

            // Si pasó todas las validaciones, insertar trayecto
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
                        cmd.Parameters.AddWithValue("horaservicio", TimeSpan.Parse(trayecto.HoraServicio));

                        cmd.ExecuteNonQuery();
                    }
                }

                TempData["Mensaje"] = "Trayecto guardado exitosamente.";
                return RedirectToAction("Crear");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al guardar el trayecto: " + ex.Message;

                // Recargar datos para la vista en caso de error
                var usuarios = new List<Usuario>();
                var lugares = new List<OrigenDestino>();

                using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("PostgresConnection")))
                {
                    connection.Open();

                    using (var cmd = new NpgsqlCommand("SELECT id, nombrecompleto FROM tbl_users", connection))
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

                    using (var cmd = new NpgsqlCommand("SELECT id, nombrelugar FROM tbl_origendestino WHERE esactivo = true", connection))
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
                }

                ViewBag.Usuarios = usuarios;
                ViewBag.Lugares = lugares;

                return View(trayecto);
            }
        }
        [HttpGet]
        public IActionResult Listado()
        {
            var trayectos = new List<Trayecto>();

            using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("PostgresConnection")))
            {
                connection.Open();

                using (var cmd = new NpgsqlCommand("SELECT * FROM obtener_trayectos()", connection))
                using (var reader = cmd.ExecuteReader())
                {
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
                }
            }

            return View(trayectos);
        }
        [HttpPost]
        public IActionResult ConfirmarEstado(int idTrayecto, bool confirmar)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("PostgresConnection")))
                {
                    connection.Open();

                    // Llama al procedimiento almacenado para cambiar estado
                    using (var command = new NpgsqlCommand("CALL sp_cambiar_estado_trayecto(@p_id_trayecto, @p_confirmar)", connection))
                    {
                        command.Parameters.AddWithValue("p_id_trayecto", idTrayecto);
                        command.Parameters.AddWithValue("p_confirmar", confirmar);
                        command.ExecuteNonQuery();
                    }
                }

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

            using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("PostgresConnection")))
            {
                connection.Open();

                using (var cmd = new NpgsqlCommand("SELECT * FROM sp_listar_trayectos_por_estado(@p_nombre_estado)", connection))
                {
                    cmd.Parameters.AddWithValue("p_nombre_estado", "PENDIENTE");

                    using (var reader = cmd.ExecuteReader())
                    {
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
                    }
                }
            }

            return View(trayectos);
        }
    }
}
    