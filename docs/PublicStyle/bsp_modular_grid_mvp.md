
# Bamberger Spinnerei — Modular Grid CMS: MVP & Future Plan

**Owner:** Peter Paul Clinton (Bamberger Spinnerei gGmbH i. G.)  
**Goal:** A strictly metric, modular grid system that renders blocks *on whole modules only*, driven first by a mock “CMS” (JSON) and later by **Umbraco Heartcore (Headless)**. The same content model will power the public website and (later) multi‑channel distribution.

---

## A. Design Principles (Hard Rules)

1. **Metric modular grid**  
   - Base unit `--mod` (px, fixed): the sole “currency” for spacing, block sizes, and line‑height rhythm.  
   - The grid starts at **(0,0)** (top‑left of the viewport/container).

2. **Whole modules only for blocks**  
   - Every block declares **integers**: `col`, `row`, `colSpan`, `rowSpan`.  
   - Blocks may *never* occupy fractional modules.

3. **Dynamic width via whole columns**  
   - Let **leftover fractional space** exist at the right (and optionally bottom) edge.  
   - Compute whole columns as: `cols = floor(viewportWidth / --mod)` (or container width via `ResizeObserver`).  
   - Define tracks with `grid-template-columns: repeat(var(--cols), var(--mod));`  
   - When the viewport width changes, only *whole columns* appear/disappear; blocks do **not** stretch.

4. **“Fill to end” semantics**  
   - In content, `colSpan: "*"` (or `rowSpan: "*"`): fill from the start line to the **last whole line** (end at `-1`).  
   - Example: Marquee at `col = 13` uses CSS `grid-column: 13 / -1` (auto-follows changes in `--cols`).

5. **Typography locks to the grid**  
   - Use `text-box-trim` where supported, with an **X/Y transform fallback**.  
   - Values (current best fit):  
     - `h1`: `--trim-h1: 0.34 * --mod`, `--xtrim-h1: 0.10 * --mod`  
     - `p`:  `--trim-p:  0.30 * --mod`, `--xtrim-p:  0.38 * --mod`

6. **Overlay grid for verification**  
   - A global overlay draws grid lines *over everything* to verify alignment and cap‑trim visually (toggleable).

---

## B. MVP Scope (Mock First)

We deliberately **exclude Heartcore** in the first iteration to stabilize layout and block specs. The mock mimics future CMS payloads.

### B.0 Project Structure (proposal)

```
/Pages (or /Views)
/Services/MockContentService.cs
/Models/Blocks.cs
/Controllers/PageController.cs
/Views/Page/Index.cshtml
/Views/Shared/_BlockLogo.cshtml
/Views/Shared/_BlockHeaderMarquee.cshtml
/Views/Shared/_BlockTitle.cshtml
/Views/Shared/_BlockBody.cshtml
/wwwroot/mock/mock_home.json
/wwwroot/fonts/ABC ROM Mono HEADLINE.woff2|.woff
/wwwroot/images/hearts/WEB_1.svg ... WEB_9.svg
```

### B.1 Mock JSON (`wwwroot/mock/mock_home.json`)

> NOTE: You may use `"*"` in `colSpan`/`rowSpan` to signify “fill to end”. The mock service resolves `"*"` to an integer at render time.

```jsonc
{
  "title": "Start",
  "slug": "home",
  "blocks": [
    {
      "kind": "Logo",
      "media": { "svgUrl": "/images/hearts/WEB_3.svg" },
      "layout": {
        "desktop": { "col": 1,  "row": 1,  "colSpan": 12,  "rowSpan": 12, "bg": "#FF3300" },
        "tablet":  { "col": 1,  "row": 1,  "colSpan": 10,  "rowSpan": 10, "bg": "#FF3300" },
        "mobile":  { "col": 1,  "row": 1,  "colSpan": 8,   "rowSpan": 8,  "bg": "#FF3300" }
      }
    },
    {
      "kind": "HeaderMarquee",
      "text": "Bamberger Spinnerei • ",
      "layout": {
        "desktop": { "col": 13, "row": 1,  "colSpan": "*", "rowSpan": 12, "bg": "#0066FF" },
        "tablet":  { "col": 11, "row": 1,  "colSpan": "*", "rowSpan": 10, "bg": "#0066FF" },
        "mobile":  { "col": 9,  "row": 1,  "colSpan": "*", "rowSpan": 8,  "bg": "#0066FF" }
      }
    },

    { "kind": "Title", "headline": "Vision",
      "layout": {
        "desktop": { "col": 1,  "row": 13, "colSpan": 40, "rowSpan": 10, "bg": "#33FF00" },
        "tablet":  { "col": 1,  "row": 11, "colSpan": 26, "rowSpan": 10, "bg": "#33FF00" },
        "mobile":  { "col": 1,  "row": 9,  "colSpan": 24, "rowSpan": 10, "bg": "#33FF00" }
      }
    },
    { "kind": "Title", "headline": "Zeit",
      "layout": {
        "desktop": { "col": 41, "row": 13, "colSpan": 40, "rowSpan": 10, "bg": "#33FF00" },
        "tablet":  { "col": 27, "row": 11, "colSpan": 26, "rowSpan": 10, "bg": "#33FF00" },
        "mobile":  { "col": 25, "row": 9,  "colSpan": 24, "rowSpan": 10, "bg": "#33FF00" }
      }
    },
    { "kind": "Title", "headline": "Raum",
      "layout": {
        "desktop": { "col": 81, "row": 13, "colSpan": 40, "rowSpan": 10, "bg": "#33FF00" },
        "tablet":  { "col": 53, "row": 11, "colSpan": 26, "rowSpan": 10, "bg": "#33FF00" },
        "mobile":  { "col": 49, "row": 9,  "colSpan": 24, "rowSpan": 10, "bg": "#33FF00" }
      }
    },

    { "kind": "Body",
      "bodyHtml": "<p>Wir sind losgegangen.</p><p>Nichts ist fertig. Alles ist möglich.</p>",
      "layout": {
        "desktop": { "col": 1,  "row": 23, "colSpan": 120, "rowSpan": 14, "bg": "#26c626" },
        "tablet":  { "col": 1,  "row": 21, "colSpan": 84,  "rowSpan": 14, "bg": "#26c626" },
        "mobile":  { "col": 1,  "row": 19, "colSpan": 72,  "rowSpan": 14, "bg": "#26c626" }
      }
    },

    { "kind": "Body",
      "bodyHtml": "<p>Aus der Verwicklung, in die Entwicklung.</p>",
      "layout": {
        "desktop": { "col": 1,  "row": 37, "colSpan": 120, "rowSpan": 20, "bg": "#FFE450" },
        "tablet":  { "col": 1,  "row": 35, "colSpan": 84,  "rowSpan": 20, "bg": "#FFE450" },
        "mobile":  { "col": 1,  "row": 33, "colSpan": 72,  "rowSpan": 20, "bg": "#FFE450" }
      }
    }
  ]
}
```

_All `col/row/span` values are integers. Variant layouts for tablet/mobile only change **integers**, never fractions._

### B.2 Models/DTOs (`Models/Blocks.cs`)

```csharp
public record GridSettings(int Col, int Row, int ColSpan, int RowSpan, string? Bg);
public record ResponsiveLayout(GridSettings Desktop, GridSettings Tablet, GridSettings Mobile);

public abstract record BlockDto(string Kind, ResponsiveLayout Layout);

public record BlockLogoDto(string Kind, ResponsiveLayout Layout, string SvgUrl)
  : BlockDto(Kind, Layout);

public record BlockHeaderMarqueeDto(string Kind, ResponsiveLayout Layout, string Text)
  : BlockDto(Kind, Layout);

public record BlockTitleDto(string Kind, ResponsiveLayout Layout, string Headline)
  : BlockDto(Kind, Layout);

public record BlockBodyDto(string Kind, ResponsiveLayout Layout, string BodyHtml)
  : BlockDto(Kind, Layout);

public record PageDto(string Title, string Slug, List<BlockDto> Blocks);
```

### B.3 Mock Service (`Services/MockContentService.cs`)

- Loads JSON, maps to DTOs, **clamps** values into the active grid bounds.
- Resolves `"*"` for `colSpan`/`rowSpan` at request time using the **current** `--cols` (and optional `--rows`).

```csharp
using System.Text.Json;

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
                    ColSpan: e.GetProperty("colSpan").ValueKind == JsonValueKind.String ? int.MinValue : e.GetProperty("colSpan").GetInt32(),
                    RowSpan: e.GetProperty("rowSpan").ValueKind == JsonValueKind.String ? int.MinValue : e.GetProperty("rowSpan").GetInt32(),
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
                _ => throw new NotSupportedException($"Unknown block kind {kind}")
            };

            blocks.Add(dto);
        }

        return new PageDto(title, slug, blocks);
    }
}
```

> **Note:** We treat `"*"` as `int.MinValue` here and resolve spans **in the view** using CSS Grid `start / -1`, which better reflects the live `--cols` at runtime. If you prefer server resolution, inject current `cols` (from query or a small API) and compute integers there.

### B.4 Controller (`Controllers/PageController.cs`)

```csharp
using Microsoft.AspNetCore.Mvc;

public class PageController : Controller
{
    private readonly MockContentService _mock;
    public PageController(MockContentService mock) => _mock = mock;

    [HttpGet("/{slug?}")]
    public async Task<IActionResult> Index(string? slug = "home")
        => View(await _mock.GetMockPageAsync(slug ?? "home"));
}
```

> `Program.cs`  
```csharp
builder.Services.AddSingleton<MockContentService>();
```

### B.5 Main View (`Views/Page/Index.cshtml`)

- Defines the **explicit grid** with dynamic `--cols`.  
- Uses **ResizeObserver** (optional) to derive `--cols` from a container instead of `window`.  
- Applies **global overlay**, **typo trim**, and supports `"*"` by emitting `grid-column: <start> / -1` (and same idea for rows).

```cshtml
@model PageDto
@{
    ViewData["Title"] = Model.Title;
}
<style>
  :root{
    --c-grid: rgba(0,0,0,.10);
    --c-text: #111;

    /* Typo Trim */
    --mod: 12px;
    --trim-h1:  calc(var(--mod) * 0.34);
    --xtrim-h1: calc(var(--mod) * 0.10);
    --trim-p:   calc(var(--mod) * 0.30);
    --xtrim-p:  calc(var(--mod) * 0.38);
    --cols: 120; /* will be updated at runtime */
  }

  @font-face{
    font-family: "ROM MONO";
    src: url("/fonts/ABC ROM Mono HEADLINE.woff2") format("woff2"),
         url("/fonts/ABC ROM Mono HEADLINE.woff")  format("woff");
    font-weight: 400; font-style: normal; font-display: swap;
  }

  html, body { height:100%; }
  body { margin:0; color:var(--c-text); overflow:hidden; }

  .stage { width: 100vw; height: 100svh; overflow: auto; }
  .canvas{
    position: relative;
    min-height: 100%;
    display: grid;
    grid-template-columns: repeat(var(--cols), var(--mod)); /* whole tracks only */
    grid-auto-rows: var(--mod);
  }

  .gridOverlay{
    position: fixed; inset:0; pointer-events:none; z-index: 10;
    background-image:
      repeating-linear-gradient(to right, var(--c-grid) 0 1px, transparent 1px var(--mod)),
      repeating-linear-gradient(to bottom, var(--c-grid) 0 1px, transparent 1px var(--mod));
  }

  .block{
    grid-column: var(--col-start) / var(--col-end);
    grid-row: var(--row-start) / var(--row-end);
    margin:0; padding:0; position:relative;
    background: var(--bg, transparent);
  }
  .inset{ padding: calc(var(--mod) * 2); }

  .mono{ font-family:"ROM MONO", system-ui, -apple-system, Segoe UI, Roboto, Arial, sans-serif; }
  .h1{ margin:0; text-transform:uppercase; font-weight:800; font-size: clamp(1.6rem, 2.2vw + 0.9rem, 3rem); line-height: calc(var(--mod) * 4); }
  .p { margin:0; font-size: clamp(1rem, 0.5vw + .85rem, 1.25rem); line-height: calc(var(--mod) * 2); hyphens:auto; text-wrap:pretty; }
  .stack > * + *{ margin-top: calc(var(--mod) * 2); }

  @supports (text-box-trim: both) {
    .h1, .p { text-box-trim: both; text-box-edge: cap alphabetic; }
  }
  @supports not (text-box-trim: both) {
    .h1{ transform: translateX(calc(-1 * var(--xtrim-h1))) translateY(calc(-1 * var(--trim-h1))); }
    .p { transform: translateX(calc(-1 * var(--xtrim-p)))  translateY(calc(-1 * var(--trim-p))); }
  }

  .marquee { overflow: hidden; white-space: nowrap; }
  .marquee__track { display:inline-block; padding-right: 50vw; animation: marquee 18s linear infinite; }
  @keyframes marquee { from{transform:translateX(0)} to{transform:translateX(-50%)} }

  .heart { width:100%; height:auto; aspect-ratio:1/1; display:block; }
</style>

<div class="gridOverlay"></div>

<div class="stage">
  <div class="canvas" id="grid">
    @* Example of rendering with "*" support in CSS *@
    @foreach (var block in Model.Blocks)
    {
      switch (block)
      {
        case BlockLogoDto b:
          @await Html.PartialAsync("_BlockLogo", b)
          break;
        case BlockHeaderMarqueeDto b:
          @await Html.PartialAsync("_BlockHeaderMarquee", b)
          break;
        case BlockTitleDto b:
          @await Html.PartialAsync("_BlockTitle", b)
          break;
        case BlockBodyDto b:
          @await Html.PartialAsync("_BlockBody", b)
          break;
      }
    }
  </div>
</div>

<script>
  // Keep --cols in sync with viewport (or grid container) using whole modules
  const root = document.documentElement;
  const mod = () => parseFloat(getComputedStyle(root).getPropertyValue('--mod'));

  function setColsFromViewport() {
    const cols = Math.max(1, Math.floor(window.innerWidth / mod()));
    root.style.setProperty('--cols', cols);
  }
  setColsFromViewport();
  addEventListener('resize', setColsFromViewport, { passive: true });
</script>
```

### B.6 Partials (with `"*"` → `-1` conversion)

**\_BlockLogo.cshtml**  
```cshtml
@model BlockLogoDto
@{
  var d = Model.Layout.Desktop;
  string colEnd = (d.ColSpan == int.MinValue) ? "-1" : (d.Col + d.ColSpan).ToString();
  string rowEnd = (d.RowSpan == int.MinValue) ? "-1" : (d.Row + d.RowSpan).ToString();
}
<section class="block"
         style="--col-start:@d.Col; --col-end:@colEnd; --row-start:@d.Row; --row-end:@rowEnd; --bg:@d.Bg;">
  <div class="inset">
    <img class="heart" src="@Model.SvgUrl" alt="Logo Herz" />
  </div>
</section>
```

**\_BlockHeaderMarquee.cshtml**  
```cshtml
@model BlockHeaderMarqueeDto
@{
  var d = Model.Layout.Desktop;
  string colEnd = (d.ColSpan == int.MinValue) ? "-1" : (d.Col + d.ColSpan).ToString();
  string rowEnd = (d.RowSpan == int.MinValue) ? "-1" : (d.Row + d.RowSpan).ToString();
}
<section class="block mono"
         style="--col-start:@d.Col; --col-end:@colEnd; --row-start:@d.Row; --row-end:@rowEnd; --bg:@d.Bg;">
  <div class="inset marquee">
    <h1 class="h1 marquee__track">@Model.Text @Model.Text @Model.Text</h1>
  </div>
</section>
```

**\_BlockTitle.cshtml**  
```cshtml
@model BlockTitleDto
@{
  var d = Model.Layout.Desktop;
  string colEnd = (d.ColSpan == int.MinValue) ? "-1" : (d.Col + d.ColSpan).ToString();
  string rowEnd = (d.RowSpan == int.MinValue) ? "-1" : (d.Row + d.RowSpan).ToString();
}
<section class="block mono"
         style="--col-start:@d.Col; --col-end:@colEnd; --row-start:@d.Row; --row-end:@rowEnd; --bg:@d.Bg;">
  <div class="inset">
    <h1 class="h1">@Model.Headline</h1>
  </div>
</section>
```

**\_BlockBody.cshtml**  
```cshtml
@model BlockBodyDto
@{
  var d = Model.Layout.Desktop;
  string colEnd = (d.ColSpan == int.MinValue) ? "-1" : (d.Col + d.ColSpan).ToString();
  string rowEnd = (d.RowSpan == int.MinValue) ? "-1" : (d.Row + d.RowSpan).ToString();
}
<section class="block mono"
         style="--col-start:@d.Col; --col-end:@colEnd; --row-start:@d.Row; --row-end:@rowEnd; --bg:@d.Bg;">
  <div class="inset stack">
    @Html.Raw(Model.BodyHtml)
  </div>
</section>
```

> **Tablet/Mobile**: If you need variant layouts, mirror the same logic or compute active values from CSS custom props per breakpoint (advanced). For MVP, desktop coordinates demonstrate the grid rigor.


---

## C. Task List (MVP)

1. **Grid container & overlay**
   - [ ] Implement grid with `repeat(var(--cols), var(--mod))` and overlay.  
   - [ ] Hook JS (or `ResizeObserver`) to update `--cols` on resize (whole columns only).

2. **Mock content**
   - [ ] Finalize mock JSON schema (allow `"*"`).  
   - [ ] Implement `MockContentService` + DTOs.

3. **Blocks (partials)**
   - [ ] `_BlockLogo`, `_BlockHeaderMarquee`, `_BlockTitle`, `_BlockBody`.  
   - [ ] Convert `"*"` → `-1` in Razor (or resolve server‑side).

4. **Typography**
   - [ ] Embed `ROM MONO` font.  
   - [ ] Apply `text-box-trim` + X/Y fallback with current trim values.

5. **Validation**
   - [ ] Clamp all coordinates/spans to grid bounds.  
   - [ ] Visual verification using the overlay at multiple widths.


---

## D. Future: Umbraco Heartcore Integration

1. **Content Modeling**  
   - Document Type `Page`: `slug`, `title`, `blocks[]`  
   - Mixin `GridSettings`: `col`, `row`, `colSpan`, `rowSpan`, `bg`  
   - Block Types: `BlockLogo(svgUrl)`, `BlockHeaderMarquee(text)`, `BlockTitle(headline)`, `BlockBody(richText)`, `BlockImage(asset, alt)`

2. **Delivery**  
   - Replace `MockContentService` with `HeartcoreService` (.NET client or GraphQL).  
   - Keep DTOs & partials unchanged; same grid & `"*"` semantics.  
   - Apply server‑side clamping & optional preview endpoints.

3. **Beyond MVP (Roadmap)**  
   - [ ] Live preview with draft content.  
   - [ ] Webhooks → Distributor service for **multi‑channel** posts (IG/FB/LinkedIn/YouTube/X).  
   - [ ] Asset processing (renditions/thumbnails).  
   - [ ] Editor UX: presets for common spans (e.g., 12×12, 40×10) and color tokens.


---

## E. Implementation Notes (Standards & Techniques)

- **CSS Grid repeat() and track lists** keep columns whole and predictable.  
- Ending at the last line (`/ -1`) is idiomatic for “fill to end” and pairs well with runtime `--cols`.  
- **CSS Custom Properties** (`--mod`, `--cols`) allow runtime recalculation without re‑rendering markup.  
- **ResizeObserver** is ideal if the grid lives inside a container rather than spanning the viewport.  
- **`text-box-trim`** improves typographic alignment; keep the transform fallback for cross‑browser support.

---

**Done is beautiful.** This plan gives the designer a faithful, grid‑true prototype **now**, and the dev team a clear path to Heartcore without rework.
