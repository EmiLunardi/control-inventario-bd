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
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
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
                Usuario? user = null;
                // Encriptamos la contraseña ingresada con SHA-256 antes de consultar la base de datos
                string passwordHash = PasswordHelper.EncriptarPassword(model.Password);

                try 
                {
                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        // Si en la base local quedó la contraseña previa en texto plano, se migra automáticamente a su hash SHA-256
                        await connection.ExecuteAsync(@"
                            UPDATE Usuarios 
                            SET PasswordHash = @PasswordHash 
                            WHERE NombreUsuario = @Username AND PasswordHash = @RawPassword",
                            new { Username = model.Username, RawPassword = model.Password, PasswordHash = passwordHash });

                        var query = @"
                            SELECT Id, NombreUsuario, PasswordHash, Rol
                            FROM Usuarios
                            WHERE NombreUsuario = @Username AND PasswordHash = @PasswordHash";
                        user = await connection.QuerySingleOrDefaultAsync<Usuario>(query, new 
                        { 
                            Username = model.Username, 
                            PasswordHash = passwordHash 
                        });
                    }
                }
                catch (Exception ex)
                {
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
                
                ModelState.AddModelError(string.Empty, "Usuario o contraseña inválidos.");
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
