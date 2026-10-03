using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Dapper;
using Npgsql;
using ControlInventario.Models;

namespace ControlInventario.Controllers
{
    public class AccountController : Controller
    {
        private readonly string _connectionString;

        public AccountController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                Usuario user = null;
                try 
                {
                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        var query = "SELECT * FROM Usuarios WHERE NombreUsuario = @Username AND PasswordHash = @Password";
                        user = await connection.QuerySingleOrDefaultAsync<Usuario>(query, new { Username = model.Username, Password = model.Password });
                    }
                }
                catch (Exception ex)
                {
                    // Manejo de errores por si no está configurada la BD localmente (solo para evitar que rompa la demo sin BD)
                    ModelState.AddModelError(string.Empty, "Error de conexión a la Base de Datos: " + ex.Message);
                    return View(model);
                }

                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.NombreUsuario),
                        new Claim(ClaimTypes.Role, user.Rol)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Index", "Home");
                }
                
                try {
                    using (var connection = new NpgsqlConnection(_connectionString)) {
                        var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Usuarios");
                        ModelState.AddModelError(string.Empty, $"Usuario o contraseña inválidos. (Debug: Hay {count} usuarios registrados en la base de datos)");
                    }
                } catch {
                    ModelState.AddModelError(string.Empty, "Usuario o contraseña inválidos.");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
