// 1. SEED DEFAULT DATA WITH REAL TMDB POSTERS
using FilmFusion.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
                            // Real Poster Path from TMDB API
                            string posterPath = item.TryGetProperty("poster_path", out var pProp) ? pProp.GetString() ?? "" : "";
                            string fullPosterUrl = !string.IsNullOrEmpty(posterPath)
                                ? $"https://image.tmdb.org/t/p/w500{posterPath}"
                                : "https://images.unsplash.com/photo-1594909122845-11baa439b7bf?q=80&w=500";

                            var movieNode = new Movie
                            {
                                Title = title,
                                Genre = "Trending Material",
                                Duration = "145 Mins", // Safe string for your model layout
                                TargetAgeLimit = 13,
                                Description = item.TryGetProperty("overview", out var oProp) ? oProp.GetString() ?? "No summary log entry." : "No summary log entry.",
                                PosterUrl = fullPosterUrl // Injected real TMDB image link
                            };
                            newMovies.Add(movieNode);
                            addedCount++;
                        }
                    }
                    if (newMovies.Count >= 8) break; // Matches your exact grid view capacity
                }

                if (newMovies.Any())
                {
                    await _context.AddRangeAsync(newMovies);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Seeded {addedCount} live movies with genuine posters!";
                }
            }
        }
    }
    catch (Exception ex)
    {
        TempData["ErrorMessage"] = "API Stream Fault: " + ex.Message;
    }

    return RedirectToAction("Index", "Home");
}

// 2. LIVE SEARCH MOVIE PROBE WITH POSTER PREVIEW
[HttpPost]
public async Task<IActionResult> ProbeMovie(string searchQuery)
{
    if (string.IsNullOrEmpty(searchQuery))
    {
        TempData["ErrorMessage"] = "Please provide an asset target search string.";
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
                string desc = firstResult.TryGetProperty("overview", out var oProp) ? oProp.GetString() ?? "No overview available." : "No overview available.";
                string posterPath = firstResult.TryGetProperty("poster_path", out var pProp) ? pProp.GetString() ?? "" : "";

                // Absolute full TMDB image resolution mapping
                string fullPosterUrl = !string.IsNullOrEmpty(posterPath)
                    ? $"https://image.tmdb.org/t/p/w500{posterPath}"
                    : "https://images.unsplash.com/photo-1594909122845-11baa439b7bf?q=80&w=500";

                TempData["SearchResultTitle"] = title;
                TempData["SearchResultDesc"] = desc;
                TempData["SearchResultPoster"] = fullPosterUrl;
                TempData["SuccessMessage"] = "Real media vector analyzed successfully!";
            }
            else
            {
                TempData["InfoMessage"] = $"Zero data entries matching reference: '{searchQuery}'.";
            }
        }
    }
    catch (Exception ex)
    {
        TempData["ErrorMessage"] = "Telemetry Failure: " + ex.Message;
    }

    return RedirectToAction("Index", "Home");
}