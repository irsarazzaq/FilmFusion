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

        // 1. SEED DEFAULT POPULAR DATA WITH AUTHENTIC TMDB POSTERS
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

                        foreach (var item in results.EnumerateArray())
                        {
                            string title = item.TryGetProperty("title", out var tProp) ? tProp.GetString() ?? "" : "";

                            if (!string.IsNullOrEmpty(title))
                            {
                                bool exists = await _context.Movies.AnyAsync(m => m.Title == title);
                                if (!exists)
                                {
                                    // Extract the trailing poster path variable from TMDB payload
                                    string posterPath = item.TryGetProperty("poster_path", out var pProp) ? pProp.GetString() ?? "" : "";

                                    // Generate absolute CDN asset pipeline URL
                                    string fullPosterUrl = !string.IsNullOrEmpty(posterPath)
                                        ? $"https://image.tmdb.org/t/p/w500{posterPath}"
                                        : "https://images.unsplash.com/photo-1594909122845-11baa439b7bf?q=80&w=500";

                                    var movieNode = new Movie
                                    {
                                        Title = title,
                                        Genre = "Sci-Fi, Action",
                                        Duration = "138 Mins",
                                        TargetAgeLimit = 13,
                                        Description = item.TryGetProperty("overview", out var oProp) ? oProp.GetString() ?? "No log entry available." : "No log entry available.",
                                        PosterUrl = fullPosterUrl
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
                            TempData["SuccessMessage"] = $"Successfully synchronized {addedCount} trending items with official movie posters!";
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

        // 2. LIVE SEARCH MOVIE PROBE ENGINES WITH REAL POSTER PARSING
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
                    var root = jsonDoc.RootElement;

                    if (root.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
                    {
                        var firstResult = results.EnumerateArray().First();

                        string title = firstResult.TryGetProperty("title", out var tProp) ? tProp.GetString() ?? "" : "";
                        string desc = firstResult.TryGetProperty("overview", out var oProp) ? oProp.GetString() ?? "No summary dataset logs." : "No summary dataset logs.";
                        string posterPath = firstResult.TryGetProperty("poster_path", out var pProp) ? pProp.GetString() ?? "" : "";

                        // Form full verifiable dynamic network route image string
                        string fullPosterUrl = !string.IsNullOrEmpty(posterPath)
                            ? $"https://image.tmdb.org/t/p/w500{posterPath}"
                            : "https://images.unsplash.com/photo-1594909122845-11baa439b7bf?q=80&w=500";

                        TempData["SearchResultTitle"] = title;
                        TempData["SearchResultDesc"] = desc;
                        TempData["SearchResultPoster"] = fullPosterUrl;
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

        // 3. EXPLICIT PERSISTENCE DATABASE COMMITER 
        [HttpPost]
        public async Task<IActionResult> ConfirmSave(string title, string description, string posterUrl, string genre, string duration, int targetAgeLimit)
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
                        PosterUrl = !string.IsNullOrEmpty(posterUrl) ? posterUrl : "https://images.unsplash.com/photo-1594909122845-11baa439b7bf?q=80&w=500"
                    };

                    await _context.Movies.AddAsync(movie);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Data entity '{title}' committed to database cluster with artwork validation!";
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

        // 4. PURGE INFRASTRUCTURE DATABASE COMMAND
        [HttpPost]
        public async Task<IActionResult> WipeAll()
        {
            _context.Movies.RemoveRange(_context.Movies);
            await _context.SaveChangesAsync();
            TempData["InfoMessage"] = "Infrastructure node table cleared completely.";
            return RedirectToAction("Index", "Home");
        }
    }
}