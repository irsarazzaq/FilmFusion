using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FilmFusion.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Linq;

namespace FilmFusion.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. ADMIN DESK -> MAIN INDEX DASHBOARD (WITH LIVE CHART DATA)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            // SECURE GATEWAY CHECK
            if (HttpContext.Session.GetString("UserRole") != "ADMIN")
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            try
            {
                var movies = await _context.Movies.ToListAsync();
                var totalUsers = await _context.Users.ToListAsync();

                ViewBag.RegisteredUsers = totalUsers;
                ViewBag.TotalMoviesCount = movies.Count;
                ViewBag.TotalUsersCount = totalUsers.Count;

                ViewBag.HighRatedMovies = movies.Count;
                ViewBag.AvgRatedMovies = 0;

                // TRENDING COUNT FIX
                ViewBag.TrendingCount = movies.Count(m => m.IsTrending);

                return View(movies);
            }
            catch (System.Exception)
            {
                TempData["ErrorMessage"] = "Database connection transient alert. Showing cached frame.";

                ViewBag.RegisteredUsers = new System.Collections.Generic.List<FilmFusion.Models.User>();
                ViewBag.TotalMoviesCount = 0;
                ViewBag.TotalUsersCount = 0;
                ViewBag.HighRatedMovies = 0;
                ViewBag.AvgRatedMovies = 0;
                ViewBag.TrendingCount = 0;

                return View(new System.Collections.Generic.List<FilmFusion.Models.Movie>());
            }
        }

        // ==========================================
        // 2. USER ENTERTAINMENT DESK -> DASHBOARD
        // ==========================================
        public async Task<IActionResult> UserDashboard()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
            {
                return RedirectToAction("UserLogin", "Account");
            }

            var movies = await _context.Movies.ToListAsync();
            return View(movies);
        }

        // ==========================================
        // 3. ADMIN'S USER MANAGEMENT SYSTEM
        // ==========================================
        public async Task<IActionResult> UsersControl()
        {
            if (HttpContext.Session.GetString("UserRole") != "ADMIN")
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            try
            {
                var usersList = await _context.Users.ToListAsync();
                return View(usersList);
            }
            catch (System.Exception)
            {
                return View(new System.Collections.Generic.List<FilmFusion.Models.User>());
            }
        }

        // ==========================================
        // 4. DOWNLOAD EXPORT INTERFACE: USERS TO CSV
        // ==========================================
        public async Task<IActionResult> ExportUsersCSV()
        {
            if (HttpContext.Session.GetString("UserRole") != "ADMIN")
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            var users = await _context.Users.ToListAsync();

            var builder = new StringBuilder();

            builder.AppendLine("Id,FullName,Email,Role");

            foreach (var user in users)
            {
                builder.AppendLine(
                    $"{EscapeCSV(user.Id.ToString())}," +
                    $"{EscapeCSV(user.FullName)}," +
                    $"{EscapeCSV(user.Email)}," +
                    $"{EscapeCSV(user.Role)}"
                );
            }

            return File(
                Encoding.UTF8.GetBytes(builder.ToString()),
                "text/csv",
                "FilmFusion_Users_Report.csv"
            );
        }

        // ==========================================
        // 5. DOWNLOAD EXPORT INTERFACE: MOVIES TO CSV
        // ==========================================
        public async Task<IActionResult> ExportMoviesCSV()
        {
            if (HttpContext.Session.GetString("UserRole") != "ADMIN")
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            var movies = await _context.Movies.ToListAsync();

            var builder = new StringBuilder();

            builder.AppendLine("Id,Title,Genre,Duration,AgeLimit,Trending,Rating");

            foreach (var movie in movies)
            {
                builder.AppendLine(
                    $"{EscapeCSV(movie.Id.ToString())}," +
                    $"{EscapeCSV(movie.Title)}," +
                    $"{EscapeCSV(movie.Genre)}," +
                    $"{EscapeCSV(movie.Duration)}," +
                    $"{EscapeCSV(movie.TargetAgeLimit.ToString())}," +
                    $"{EscapeCSV(movie.IsTrending.ToString())}," +
                    $"{EscapeCSV(movie.TmdbRating.ToString())}"
                );
            }

            return File(
                Encoding.UTF8.GetBytes(builder.ToString()),
                "text/csv",
                "FilmFusion_Movies_Inventory.csv"
            );
        }

        // ==========================================
        // 6. SAFE CSV FORMATTER
        // ==========================================
        private string EscapeCSV(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            value = value.Replace("\"", "\"\"");

            return $"\"{value}\"";
        }

        // ==========================================
        // 7. NEW DIRECTORY: ASYNCHRONOUS DATA CLUSTER PURGE ENGINE
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> DeleteMovieAsync(int id)
        {
            try
            {
                var movie = await _context.Movies.FindAsync(id);

                if (movie == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Movie entity cluster index targeted not found."
                    });
                }

                _context.Movies.Remove(movie);

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Successfully purged from PostgreSQL node."
                });
            }
            catch (System.Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}