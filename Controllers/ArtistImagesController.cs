using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Models;
using newUser.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

[Authorize(Roles = "Admin,User,Artist")]
public class ArtistImagesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ArtistImagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var artistImages = await _context.ArtistImages.ToListAsync();
      
        return View(artistImages);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")] // Only allow Admin role to delete images
    public IActionResult DeleteImage(int imageId)
    {
        var image = _context.ArtistImages.Find(imageId);

        if (image == null)
        {
            return NotFound(); // Image not found
        }

        _context.ArtistImages.Remove(image);
        _context.SaveChanges();

        return RedirectToAction(nameof(Index)); 
    }

    [Authorize(Roles = "Artist")]
    public async Task<IActionResult> UserImages()
    {
        // Get the ID of the currently logged-in user
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Ensure the logged-in user is an artist
        if (string.IsNullOrEmpty(currentUserId) || !User.IsInRole("Artist"))
        {
            return Forbid(); // User is not authorized
        }

        // Use parameterized SQL query to fetch images for the logged-in artist
        var userImages = await _context.ArtistImages
            .FromSqlInterpolated($"SELECT * FROM ArtistImages WHERE ArtistId = {currentUserId}")
            .ToListAsync();

        return View(userImages);
    }

    [HttpPost]
    [Authorize(Roles = "Artist")]
    public IActionResult DeleteUserImage(int imageId)
    {
        var image = _context.ArtistImages.Find(imageId);

        if (image == null)
        {
            return NotFound(); // Image not found
        }

        // Check if the logged-in user is the owner of the image
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && image.ArtistId != currentUserId)
        {
            return Forbid(); // User is not authorized to delete this image
        }

        _context.ArtistImages.Remove(image);
        _context.SaveChanges();

        return RedirectToAction(nameof(UserImages));
    }
}
