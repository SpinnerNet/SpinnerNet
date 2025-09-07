using System.Text.Json;
using SpinnerNet.Web.Models;

namespace SpinnerNet.Web.Services;

public class MockContentService
{
    private readonly IWebHostEnvironment _env;
    
    public MockContentService(IWebHostEnvironment env) => _env = env;

    public async Task<PageDto> GetMockPageAsync(string slug = "home")
    {
        var path = Path.Combine(_env.WebRootPath, "mock", $"mock_{slug}.json");
        var json = await File.ReadAllTextAsync(path);
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        var title = root.GetProperty("title").GetString()!;
        var blocksEl = root.GetProperty("blocks");
        var blocks = new List<BlockDto>();

        foreach (var b in blocksEl.EnumerateArray())
        {
            string kind = b.GetProperty("kind").GetString()!;

            GridSettings ReadGS(JsonElement e) =>
                new(
                    Col: e.GetProperty("col").GetInt32(),
                    Row: e.GetProperty("row").GetInt32(),
                    ColSpan: e.GetProperty("colSpan").ValueKind == JsonValueKind.String 
                        ? (e.GetProperty("colSpan").GetString() == "*" ? int.MinValue : int.Parse(e.GetProperty("colSpan").GetString()!))
                        : e.GetProperty("colSpan").GetInt32(),
                    RowSpan: e.GetProperty("rowSpan").ValueKind == JsonValueKind.String 
                        ? (e.GetProperty("rowSpan").GetString() == "*" ? int.MinValue : int.Parse(e.GetProperty("rowSpan").GetString()!))
                        : e.GetProperty("rowSpan").GetInt32(),
                    Bg: e.TryGetProperty("bg", out var bg) ? bg.GetString() : null
                );

            var layoutEl = b.GetProperty("layout");
            var layout = new ResponsiveLayout(
                ReadGS(layoutEl.GetProperty("desktop")),
                ReadGS(layoutEl.GetProperty("tablet")),
                ReadGS(layoutEl.GetProperty("mobile"))
            );

            BlockDto dto = kind switch
            {
                "Logo" => new BlockLogoDto(kind, layout, b.GetProperty("media").GetProperty("svgUrl").GetString()!),
                "HeaderMarquee" => new BlockHeaderMarqueeDto(kind, layout, b.GetProperty("text").GetString()!),
                "Title" => new BlockTitleDto(kind, layout, b.GetProperty("headline").GetString()!),
                "Body" => new BlockBodyDto(kind, layout, b.GetProperty("bodyHtml").GetString()!),
                "Image" => new BlockImageDto(kind, layout, 
                    b.GetProperty("imageUrl").GetString()!,
                    b.TryGetProperty("alt", out var alt) ? alt.GetString() : null,
                    b.TryGetProperty("objectFit", out var fit) ? fit.GetString() : "cover"),
                "Video" => new BlockVideoDto(kind, layout,
                    b.GetProperty("videoUrl").GetString()!,
                    b.TryGetProperty("autoplay", out var auto) && auto.GetBoolean(),
                    b.TryGetProperty("loop", out var loop) && loop.GetBoolean(),
                    b.TryGetProperty("muted", out var muted) && muted.GetBoolean()),
                "Spacer" => new BlockSpacerDto(kind, layout),
                "Quote" => new BlockQuoteDto(kind, layout,
                    b.GetProperty("quote").GetString()!,
                    b.TryGetProperty("author", out var author) ? author.GetString() : null),
                "List" => new BlockListDto(kind, layout,
                    b.GetProperty("items").EnumerateArray().Select(i => i.GetString()!).ToList(),
                    b.TryGetProperty("listType", out var listType) ? listType.GetString()! : "ul"),
                "Embed" => new BlockEmbedDto(kind, layout,
                    b.GetProperty("embedUrl").GetString()!,
                    b.TryGetProperty("title", out var embedTitle) ? embedTitle.GetString() : null),
                _ => throw new NotSupportedException($"Unknown block kind {kind}")
            };

            blocks.Add(dto);
        }

        return new PageDto(title, slug, blocks);
    }
}