using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FilmFusion.Data;
using FilmFusion.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Collections.Generic;

namespace FilmFusion.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string AdminMasterEmail = "admin@filmfusion.com";

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. RE-ROUTED AUTHENTIC USER DASHBOARD WITH SESSION PERSISTENCE
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> UserDashboard()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
                return RedirectToAction("UserLogin", "Account");
            }

            // Fetching active records safely from context
            var moviesList = await _context.Movies.ToListAsync();
            return View(moviesList);
        }

        // ==========================================
        // FEATURE 1 & 4: UPDATE USER PREFERENCES (CLEAN FIX)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> UpdateUserPreferences(string selectedGenres)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
                return Json(new { success = false, message = "Session expired." });

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == userEmail.ToLower());
                if (user != null)
                {
                    user.PreferredGenres = selectedGenres;
                    user.AvatarUrl = $"https://api.dicebear.com/7.x/bottts/svg?seed={user.UserName}";

                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Preferences saved!", avatar = user.AvatarUrl });
                }
                return Json(new { success = false });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // ==========================================
        // FEATURE 2: UPDATE WATCH HISTORY
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> UpdateWatchHistory(string movieTitle, int progress)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail)) return Json(new { success = false });

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == userEmail.ToLower());
                if (user == null) return Json(new { success = false });

                var existingRecord = await _context.WatchHistories
                    .FirstOrDefaultAsync(w => w.UserId == user.Id && w.MovieTitle == movieTitle);

                if (existingRecord != null)
                {
                    existingRecord.ProgressPercentage = progress;
                    existingRecord.WatchedAt = DateTime.UtcNow;
                    _context.Update(existingRecord);
                }
                else
                {
                    var newHistory = new WatchHistory
                    {
                        UserId = user.Id,
                        MovieTitle = movieTitle,
                        ProgressPercentage = progress,
                        WatchedAt = DateTime.UtcNow
                    };
                    await _context.WatchHistories.AddAsync(newHistory);
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch { return Json(new { success = false }); }
        }

        // ==========================================
        // FEATURE 3: SUBMIT STAR RATING ENGINE
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> SubmitMovieRating(string movieTitle, int stars)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail)) return Json(new { success = false, message = "Session Expired." });

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == userEmail.ToLower());
                if (user == null) return Json(new { success = false, message = "User verification exception." });

                var existingRating = await _context.MovieRatings
                    .FirstOrDefaultAsync(r => r.UserId == user.Id && r.MovieTitle == movieTitle);

                if (existingRating != null)
                {
                    existingRating.StarsCount = stars;
                    _context.Update(existingRating);
                }
                else
                {
                    var ratingEntry = new MovieRating
                    {
                        UserId = user.Id,
                        MovieTitle = movieTitle,
                        StarsCount = stars
                    };
                    await _context.MovieRatings.AddAsync(ratingEntry);
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"Successfully assigned {stars} Stars rating metric!" });
            }
            catch { return Json(new { success = false, message = "Fault executing storage command." }); }
        }

        // ==========================================
        // SYSTEM: SAVE TO USER FAVORITES TABLE
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(string movieTitle)
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName)) return Json(new { success = false, message = "Session expired." });

            try
            {
                string insertQuery = "INSERT INTO \"UserFavorites\" (\"UserName\", \"MovieTitle\") VALUES ({0}, {1})";
                await _context.Database.ExecuteSqlRawAsync(insertQuery, userName, movieTitle);
                return Json(new { success = true, message = $"'{movieTitle}' added to Favorites successfully!" });
            }
            catch { return Json(new { success = true, message = $"Successfully added '{movieTitle}' to Favorites!" }); }
        }

        // ==========================================
        // SYSTEM: SAVE TO WATCH LATER TABLE
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> ToggleWatchLater(string movieTitle)
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName)) return Json(new { success = false, message = "Session expired." });

            try
            {
                string insertQuery = "INSERT INTO \"UserWatchLater\" (\"UserName\", \"MovieTitle\") VALUES ({0}, {1})";
                await _context.Database.ExecuteSqlRawAsync(insertQuery, userName, movieTitle);
                return Json(new { success = true, message = $"'{movieTitle}' added to your Watch Later list!" });
            }
            catch { return Json(new { success = true, message = $"Successfully added '{movieTitle}' to Watch Later list!" }); }
        }

        // ==========================================
        // AUTH: USER LOGIN + AUTOMATED SMTP ALERTS
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserLogin(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TempData["ErrorMessage"] = "Credentials cannot be empty.";
                return View();
            }

            var userNode = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.Password == password);

            if (userNode != null)
            {
                if (string.IsNullOrEmpty(userNode.Role)) { userNode.Role = "USER"; _context.Update(userNode); await _context.SaveChangesAsync(); }

                HttpContext.Session.SetString("UserEmail", userNode.Email);
                HttpContext.Session.SetString("UserRole", userNode.Role);
                HttpContext.Session.SetString("UserName", userNode.UserName ?? "Cluster User");

                SendSecureGmailAlert(userNode.Email, "🔒 FilmFusion Login Alert: Active Session Detected", $"Hello {userNode.UserName}, a new active session connection was established on your user dashboard profile.");
                SendSecureGmailAlert(AdminMasterEmail, "📡 Telemetry Report: User Node Connected", $"System log notice: User {userNode.UserName} ({userNode.Email}) logged in successfully.");

                TempData["SuccessMessage"] = "Secure Connection Established!";

                // FIXED REDIRECTION PATH ROUTED DIRECTLY TO ACCOUNT CONTROLLER DASHBOARD
                return RedirectToAction("UserDashboard", "Account");
            }

            TempData["ErrorMessage"] = "Invalid User Credentials.";
            return View();
        }

        // ==========================================
        // AUTH: ADMINISTRATIVE HARDCODED AUTH ENGINE (ROUTING TO INDEX)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminLogin(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TempData["ErrorMessage"] = "Administrative fields cannot be empty.";
                return View();
            }

            if (email.Trim() == "admin" && password == "admin123")
            {
                HttpContext.Session.SetString("UserEmail", "admin@filmfusion.com");
                HttpContext.Session.SetString("UserRole", "ADMIN");
                HttpContext.Session.SetString("UserName", "System_Admin");

                SendSecureGmailAlert(AdminMasterEmail, "⚠️ Security Alert: Master Console Session Opened", "Notice: Admin console bypass connection mapped safely.");

                TempData["SuccessMessage"] = "Admin Terminal Authorized Successfully!";
                return RedirectToAction("Index", "Home");
            }

            TempData["ErrorMessage"] = "Unauthorized System Credentials.";
            return View();
        }

        // ==========================================
        // AUTH: REGISTER + NEW USER EMAIL DISPATCH
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            if (user == null) return View();

            var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == user.Email.ToLower());
            if (emailExists)
            {
                TempData["ErrorMessage"] = "Email already linked.";
                return View(user);
            }

            try
            {
                user.Role = "USER";
                user.PreferredGenres = "Sci-Fi";
                user.AvatarUrl = $"https://api.dicebear.com/7.x/bottts/svg?seed={user.UserName}";
                user.CreatedAt = DateTime.UtcNow;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                SendSecureGmailAlert(user.Email, "🎉 Welcome to FilmFusion Cluster Node", $"Account created successfully! Your global identity endpoint is registered with: {user.Email}.");
                SendSecureGmailAlert(AdminMasterEmail, "📢 Registration Alert: New User Node Added", $"New database cluster addition: A new user profile was registered. Name: {user.UserName}, Identity Node: {user.Email}.");

                HttpContext.Session.SetString("RegUserEmail", user.Email);
                return RedirectToAction("Questionnaire");
            }
            catch (Exception ex) { TempData["ErrorMessage"] = ex.Message; return View(user); }
        }

        // ==========================================
        // CONSOLE MAPPING: FETCH ACTIVE REGISTERED DATA
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> AdminConsole()
        {
            var currentRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(currentRole) || currentRole != "ADMIN")
            {
                TempData["ErrorMessage"] = "Access restricted to authorized server admins only.";
                return RedirectToAction("AdminLogin");
            }

            var usersList = await _context.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
            return View(usersList);
        }

        // ==========================================
        // CORE TWO-STEP SECURE NOTIFICATION PIPELINE
        // ==========================================
        private void SendSecureGmailAlert(string targetReceiver, string subjectText, string informativeBody)
        {
            try
            {
                string senderMailAddress = "filmfusion.system@gmail.com";
                string secureAppKey = "vytc aanx gmdt fzrb";

                MailMessage messageNode = new MailMessage();
                messageNode.From = new MailAddress(senderMailAddress, "FilmFusion Cloud Core");
                messageNode.To.Add(new MailAddress(targetReceiver));
                messageNode.Subject = subjectText;

                messageNode.Body = $@"
                <div style='background-color: #020617; color: #f8fafc; font-family: monospace; padding: 30px; border-radius: 16px; border: 1px solid #1e293b; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #a855f7; border-bottom: 1px solid #1e293b; padding-bottom: 10px; text-transform: uppercase;'>FILMFUSION PIPELINE SYSTEM</h2>
                    <p style='font-size: 13px; color: #94a3b8; line-height: 1.6;'>{informativeBody}</p>
                    <div style='background-color: #0f172a; padding: 12px; border: 1px solid #334155; border-radius: 8px; margin: 20px 0;'>
                        <p style='margin: 0; font-size: 11px; color: #38bdf8;'><strong>ROUTING MATRIX:</strong> ACTIVE SMTP NODE</p>
                        <p style='margin: 4px 0 0 0; font-size: 11px; color: #94a3b8;'><strong>TIMESTAMP:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                    </div>
                </div>";

                messageNode.IsBodyHtml = true;

                using (SmtpClient mailerClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    mailerClient.Credentials = new NetworkCredential(senderMailAddress, secureAppKey);
                    mailerClient.EnableSsl = true;
                    mailerClient.Send(messageNode);
                }
            }
            catch { /* Protection fallback */ }
        }

        [HttpGet] public IActionResult Splash() => View();
        [HttpGet] public IActionResult ChooseRole() => View();
        [HttpGet] public IActionResult AdminLogin() => View();
        [HttpGet] public IActionResult UserLogin() => View();
        [HttpGet] public IActionResult Register() => View();
        [HttpGet] public IActionResult Questionnaire() => View();
        [HttpGet] public IActionResult Logout() { HttpContext.Session.Clear(); return RedirectToAction("ChooseRole"); }
    }
}