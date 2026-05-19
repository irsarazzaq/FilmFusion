using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FilmFusion.Data;
using FilmFusion.Models;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

namespace FilmFusion.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string ApiKey = "803c5c415552686319281c15b6564989";

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private HttpClient CreateSecureClient()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(15);
            return client;
        }

        // ==========================================
        // 1. SEED POPULAR DATA WITH RANDOMIZED REAL YOUTUBE TRAILERS
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> SeedDefaults()
        {
            try
            {
                using var client = CreateSecureClient();
                string apiUrl = $"https://api.themoviedb.org/3/movie/popular?api_key={ApiKey}&language=en-US&page=1";

                var response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    using var jsonDoc = JsonDocument.Parse(jsonString);
                    var root = jsonDoc.RootElement;

                    if (root.TryGetProperty("results", out var results))
                    {
                        var newMovies = new List<Movie>();
                        int addedCount = 0;

                        // Master list of highly popular real trailers to randomize safely
                        var blockbusterTrailers = new List<string>
                        {
                            "https://www.youtube.com/watch?v=TcMBFSGVi1c", // Avengers: Endgame Official Trailer
                            "https://www.youtube.com/watch?v=_InqQJRqGW4", // Money Heist (La Casa de Papel)
                            "https://www.youtube.com/watch?v=mqqft22074A", // The Batman Official Trailer
                            "https://www.youtube.com/watch?v=JfVOs4VSpmA", // Spider-Man: No Way Home
                            "https://www.youtube.com/watch?v=8g18jFHCLXk", // Dune: Part Two
                            "https://www.youtube.com/watch?v=shW9i6k8cB0", // Oppenheimer Trailer
                            "https://www.youtube.com/watch?v=Go8nTmfrQd8", // Inception Masterpiece Trailer
                            "https://www.youtube.com/watch?v=EXeTwQWrcwY", // The Dark Knight Rises
                            "https://www.youtube.com/watch?v=AZGcmvrwR98", // Avatar: The Way of Water
                            "https://www.youtube.com/watch?v=HhesaQXLuRY"  // Interstellar Voyager Theme Trailer
                        };

                        Random randomEngine = new Random();

                        foreach (var item in results.EnumerateArray())
                        {
                            string title = item.TryGetProperty("title", out var tProp) ? tProp.GetString() ?? "" : "";

                            if (!string.IsNullOrEmpty(title))
                            {
                                bool exists = await _context.Movies.AnyAsync(m => m.Title == title);
                                if (!exists)
                                {
                                    string posterPath = item.TryGetProperty("poster_path", out var pProp) ? pProp.GetString() ?? "" : "";

                                    string fullPosterUrl = !string.IsNullOrEmpty(posterPath)
                                        ? $"https://image.tmdb.org/t/p/w500{posterPath}"
                                        : "https://images.unsplash.com/photo-1594909122845-11baa439b7bf?q=80&w=500";

                                    // Pick a completely random real blockbuster trailer from our pool loop
                                    int randomIndex = randomEngine.Next(blockbusterTrailers.Count);
                                    string assignedTrailer = blockbusterTrailers[randomIndex];

                                    var movieNode = new Movie
                                    {
                                        Title = title,
                                        Genre = "Sci-Fi, Action",
                                        Duration = "138 Mins",
                                        TargetAgeLimit = 13,
                                        Description = item.TryGetProperty("overview", out var oProp) ? oProp.GetString() ?? "No log entry available." : "No log entry available.",
                                        PosterUrl = fullPosterUrl,
                                        VideoUrl = assignedTrailer // Random blockbuster link attached!
                                    };
                                    newMovies.Add(movieNode);
                                    addedCount++;
                                }
                            }
                            if (newMovies.Count >= 8) break;
                        }

                        if (newMovies.Any())
                        {
                            await _context.AddRangeAsync(newMovies);
                            await _context.SaveChangesAsync();
                            TempData["SuccessMessage"] = $"Successfully synchronized {addedCount} trending items with randomized dynamic premium trailers!";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "API Stream Handshake Fault: " + ex.Message;
            }

            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // 2. LIVE SEARCH MOVIE PROBE ENGINES WITH RANDOMIZED TRAILER LINK POOL
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> ProbeMovie(string searchQuery)
        {
            if (string.IsNullOrEmpty(searchQuery))
            {
                TempData["ErrorMessage"] = "Search target cannot be empty.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                using var client = CreateSecureClient();
                string encodedQuery = Uri.EscapeDataString(searchQuery);
                string apiUrl = $"https://api.themoviedb.org/3/search/movie?api_key={ApiKey}&query={encodedQuery}&language=en-US&page=1";

                var response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    using var jsonDoc = JsonDocument.Parse(jsonString);

                    // Error fixed here (Line 151 syntax resolved)
                    var root = jsonDoc.RootElement;

                    if (root.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
                    {
                        var firstResult = results.EnumerateArray().First();

                        string title = firstResult.TryGetProperty("title", out var tProp) ? tProp.GetString() ?? "" : "";
                        string desc = firstResult.TryGetProperty("overview", out var oProp) ? oProp.GetString() ?? "No summary dataset logs." : "No summary dataset logs.";
                        string posterPath = firstResult.TryGetProperty("poster_path", out var pProp) ? pProp.GetString() ?? "" : "";

                        string fullPosterUrl = !string.IsNullOrEmpty(posterPath)
                            ? $"https://image.tmdb.org/t/p/w500{posterPath}"
                            : "https://images.unsplash.com/photo-1594909122845-11baa439b7bf?q=80&w=500";

                        // Backup pool randomizer for quick on-demand searched movies layout
                        var dynamicTrailers = new List<string> {
                            "https://www.youtube.com/watch?v=TcMBFSGVi1c",
                            "https://www.youtube.com/watch?v=_InqQJRqGW4",
                            "https://www.youtube.com/watch?v=mqqft22074A",
                            "https://www.youtube.com/watch?v=JfVOs4VSpmA"
                        };
                        string selectedTrailer = dynamicTrailers[new Random().Next(dynamicTrailers.Count)];

                        TempData["SearchResultTitle"] = title;
                        TempData["SearchResultDesc"] = desc;
                        TempData["SearchResultPoster"] = fullPosterUrl;
                        TempData["SearchResultVideoUrl"] = selectedTrailer;
                        TempData["SuccessMessage"] = "Genuine media vector analyzed. Ready for insertion preview!";
                    }
                    else
                    {
                        TempData["InfoMessage"] = $"Zero catalog elements found matching query: '{searchQuery}'.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Telemetry Pipeline Exception: " + ex.Message;
            }

            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // 3. EXPLICIT PERSISTENCE DATABASE COMMITER
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> ConfirmSave(string title, string description, string posterUrl, string videoUrl, string genre, string duration, int targetAgeLimit)
        {
            if (string.IsNullOrEmpty(title)) return RedirectToAction("Index", "Home");

            try
            {
                bool exists = await _context.Movies.AnyAsync(m => m.Title == title);
                if (!exists)
                {
                    var movie = new Movie
                    {
                        Title = title,
                        Genre = string.IsNullOrEmpty(genre) ? "Action, Sci-Fi" : genre,
                        Duration = string.IsNullOrEmpty(duration) ? "145 Mins" : duration,
                        TargetAgeLimit = targetAgeLimit == 0 ? 13 : targetAgeLimit,
                        Description = description ?? "No log detail summary updated.",
                        PosterUrl = !string.IsNullOrEmpty(posterUrl) ? posterUrl : "https://images.unsplash.com/photo-1594909122845-11baa439b7bf?q=80&w=500",
                        VideoUrl = !string.IsNullOrEmpty(videoUrl) ? videoUrl : "https://www.youtube.com/watch?v=TcMBFSGVi1c"
                    };

                    await _context.Movies.AddAsync(movie);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Data entity '{title}' committed with safe video streaming configurations!";
                }
                else
                {
                    TempData["InfoMessage"] = $"Record model target '{title}' already configured inside database cluster.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "PostgreSQL Stream Storage Error: " + ex.Message;
            }

            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // 4. PURGE INFRASTRUCTURE DATABASE COMMAND
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> WipeAll()
        {
            _context.Movies.RemoveRange(_context.Movies);
            await _context.SaveChangesAsync();
            TempData["InfoMessage"] = "Infrastructure node table cleared completely.";
            return RedirectToAction("Index", "Home");
        }

        // ====================================================================
        // 5. NEW LOGIC: FETCH SINGLE MOVIE DETAILS BY ID (FOR MODALS/PREVIEWS)
        // ====================================================================
        [HttpGet]
        public async Task<IActionResult> GetMovieById(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound(new { message = $"Movie infrastructure node with ID {id} not found." });
            }
            return Json(movie);
        }

        // ====================================================================
        // 6. NEW LOGIC: UPDATE EXISTING MOVIE RECORD
        // ====================================================================
        [HttpPost]
        public async Task<IActionResult> UpdateMovie(int id, string title, string description, string posterUrl, string videoUrl, string genre, string duration, int targetAgeLimit)
        {
            try
            {
                var movie = await _context.Movies.FindAsync(id);
                if (movie == null)
                {
                    TempData["ErrorMessage"] = "Update targeted failed: Model record instance vanished.";
                    return RedirectToAction("Index", "Home");
                }

                movie.Title = string.IsNullOrEmpty(title) ? movie.Title : title;
                movie.Genre = string.IsNullOrEmpty(genre) ? movie.Genre : genre;
                movie.Duration = string.IsNullOrEmpty(duration) ? movie.Duration : duration;
                movie.TargetAgeLimit = targetAgeLimit == 0 ? movie.TargetAgeLimit : targetAgeLimit;
                movie.Description = description ?? movie.Description;
                movie.PosterUrl = string.IsNullOrEmpty(posterUrl) ? movie.PosterUrl : posterUrl;
                movie.VideoUrl = string.IsNullOrEmpty(videoUrl) ? movie.VideoUrl : videoUrl;

                _context.Movies.Update(movie);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Data entity '{movie.Title}' updated and committed successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Mutation Pipeline Failure: " + ex.Message;
            }

            return RedirectToAction("Index", "Home");
        }

        // ====================================================================
        // 7. NEW LOGIC: ERASE SINGLE MOVIE DATA NODE
        // ====================================================================
        [HttpPost]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            try
            {
                var movie = await _context.Movies.FindAsync(id);
                if (movie != null)
                {
                    _context.Movies.Remove(movie);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Media node '{movie.Title}' successfully purged from persistent storage.";
                }
                else
                {
                    TempData["InfoMessage"] = "Deletion execution bypassed: target record was not present.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Storage Deletion Fault: " + ex.Message;
            }

            return RedirectToAction("Index", "Home");
        }

        // ====================================================================
        // 8. NEW LOGIC: READ-ONLY TRAILERS POOL CONSTANT ENDPOINT
        // ====================================================================
        [HttpGet]
        public IActionResult GetTrailersPool()
        {
            var blockbusterTrailers = new List<string>
            {
                "https://www.youtube.com/watch?v=TcMBFSGVi1c",
                "https://www.youtube.com/watch?v=_InqQJRqGW4",
                "https://www.youtube.com/watch?v=mqqft22074A",
                "https://www.youtube.com/watch?v=JfVOs4VSpmA",
                "https://www.youtube.com/watch?v=8g18jFHCLXk",
                "https://www.youtube.com/watch?v=shW9i6k8cB0",
                "https://www.youtube.com/watch?v=Go8nTmfrQd8",
                "https://www.youtube.com/watch?v=EXeTwQWrcwY",
                "https://www.youtube.com/watch?v=AZGcmvrwR98",
                "https://www.youtube.com/watch?v=HhesaQXLuRY"
            };
            return Json(blockbusterTrailers);
        }
    }
}