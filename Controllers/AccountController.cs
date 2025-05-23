using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Trayectos_Especiales.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string correo, string contrasena)
        {
            try
            {
                Console.WriteLine($"Intentando login con correo: {correo} y contraseña: {contrasena}");

                using var connection = new NpgsqlConnection(_configuration.GetConnectionString("PostgresConnection"));
                connection.Open();

                using var cmd = new NpgsqlCommand("SELECT id, nombrecompleto, esadministrador FROM tbl_users WHERE correo = @correo AND contrasena = @contrasena", connection);
                cmd.Parameters.AddWithValue("correo", correo);
                cmd.Parameters.AddWithValue("contrasena", contrasena);

                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string nombre = reader.GetString(1);
                    bool esAdmin = reader.GetBoolean(2);

                    Console.WriteLine($"Login exitoso: ID={id}, Nombre={nombre}, Admin={esAdmin}");

                    HttpContext.Session.SetInt32("UserId", id);
                    HttpContext.Session.SetString("UserName", nombre);
                    HttpContext.Session.SetString("UserRole", esAdmin ? "Admin" : "User");

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    Console.WriteLine("Login fallido: correo o contraseña incorrectos.");
                    ViewBag.Error = "Correo o contraseña incorrectos.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en la conexión o consulta: " + ex.Message);
                ViewBag.Error = "Error en la conexión o consulta: " + ex.Message;
                return View();
            }
        }

    }
}
