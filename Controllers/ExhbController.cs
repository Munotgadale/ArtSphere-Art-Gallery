using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Models;
using newUser.Data;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

[Authorize(Roles = "Admin,User,Artist")]
public class ExhbController : Controller
{
    private readonly ApplicationDbContext _context;

    public ExhbController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var exhibitions = await _context.exhb.ToListAsync();

        return View(exhibitions);
    }

    [HttpPost, ActionName("DeleteExhibition")]
    [Authorize(Roles = "Admin")] // Only allow Admin role to delete exhibitions
    public IActionResult DeleteExhibition(int exhibitionId)
    {
        var exhibition = _context.exhb.Find(exhibitionId);

        if (exhibition == null)
        {
            return NotFound(); // Exhibition not found
        }

        _context.exhb.Remove(exhibition);
        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }
}
