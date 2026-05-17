using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FilmFusion.Data;
using FilmFusion.Models;
using System.Linq;

namespace FilmFusion.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. SPLASH SCREEN
        [HttpGet]
        public IActionResult Splash()
        {
            return View();
        }

        // 2. CHOOSE ROLE SCREEN
        [HttpGet]
        public IActionResult ChooseRole()
        {
            return View();
        }

        // 3. LOGIN SCREEN (GET)
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // LOGIN SCREEN (POST) - Handling input mappings securely
        [HttpPost]
        public IActionResult Login(string username, string email, string password)
        {
            // Form input fallback binding mechanism
            string loginInput = !string.IsNullOrEmpty(username) ? username : email;

            // --- ADMIN BYPASS (Hardcoded) ---
            if ((loginInput == "admin" || username == "admin") && password == "admin124")
            {
                HttpContext.Session.SetString("UserRole", "Admin");
                HttpContext.Session.SetString("Username", "Admin");
                return RedirectToAction("Index", "Home"); // Opens TMDB API Admin Dashboard
            }

            // --- DYNAMIC USER CHECK ---
            if (!string.IsNullOrEmpty(loginInput))
            {
                var dbUser = _context.Users.FirstOrDefault(u =>
                    (u.Username == loginInput || u.Email == loginInput) && u.Password == password);

                if (dbUser != null)
                {
                    HttpContext.Session.SetInt32("UserId", dbUser.Id);
                    HttpContext.Session.SetString("Username", dbUser.Username);
                    HttpContext.Session.SetString("UserRole", "User");

                    return RedirectToAction("Index", "Home"); // Opens general simple screen
                }
            }

            ModelState.AddModelError("", "Invalid username or password.");
            return View();
        }

        // 4. REGISTER SCREEN
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User modelUser)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Username == modelUser.Username || u.Email == modelUser.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "User already exists.");
                    return View(modelUser);
                }

                modelUser.Role = "User";
                _context.Users.Add(modelUser);
                _context.SaveChanges();

                // Clear session map injection
                HttpContext.Session.SetInt32("UserId", modelUser.Id);
                HttpContext.Session.SetString("Username", modelUser.Username);
                HttpContext.Session.SetString("UserRole", "User");

                // New User moves to the questionnaire interface profile node
                return RedirectToAction("SetupProfile", "Account");
            }
            return View(modelUser);
        }

        // 5. NEW USER PROFILE QUESTIONNAIRE
        [HttpGet]
        public IActionResult SetupProfile()
        {
            return View(); // Opens SetupProfile.cshtml for age/interests
        }

        // 6. LOGOUT
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}