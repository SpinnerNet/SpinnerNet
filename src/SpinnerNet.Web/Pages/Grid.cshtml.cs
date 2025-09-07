using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpinnerNet.Web.Models;
using SpinnerNet.Web.Services;

namespace SpinnerNet.Web.Pages;

public class GridModel : PageModel
{
    private readonly MockContentService _mockService;
    private readonly ILogger<GridModel> _logger;

    public GridModel(MockContentService mockService, ILogger<GridModel> logger)
    {
        _mockService = mockService;
        _logger = logger;
    }

    public PageDto PageData { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(string? slug = "home")
    {
        try
        {
            PageData = await _mockService.GetMockPageAsync(slug ?? "home");
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading page {Slug}", slug);
            return NotFound();
        }
    }
}