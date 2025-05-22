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
            using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("PostgresConnection")))
            {
                connection.Open();

                using (var command = new NpgsqlCommand("CALL sp_cambiar_estado_trayecto(@p_id_trayecto, @p_confirmar)", connection))
                {
                    command.Parameters.AddWithValue("p_id_trayecto", idTrayecto);
                    command.Parameters.AddWithValue("p_confirmar", confirmar);
                    command.ExecuteNonQuery();
                }
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
    