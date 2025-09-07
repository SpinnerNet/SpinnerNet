namespace SpinnerNet.Web.Models;

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

public record BlockImageDto(string Kind, ResponsiveLayout Layout, string ImageUrl, string? Alt, string? ObjectFit = "cover")
  : BlockDto(Kind, Layout);

public record BlockVideoDto(string Kind, ResponsiveLayout Layout, string VideoUrl, bool Autoplay = true, bool Loop = true, bool Muted = true)
  : BlockDto(Kind, Layout);

public record BlockSpacerDto(string Kind, ResponsiveLayout Layout)
  : BlockDto(Kind, Layout);

public record BlockQuoteDto(string Kind, ResponsiveLayout Layout, string Quote, string? Author = null)
  : BlockDto(Kind, Layout);

public record BlockListDto(string Kind, ResponsiveLayout Layout, List<string> Items, string ListType = "ul")
  : BlockDto(Kind, Layout);

public record BlockEmbedDto(string Kind, ResponsiveLayout Layout, string EmbedUrl, string? Title = null)
  : BlockDto(Kind, Layout);

public record PageDto(string Title, string Slug, List<BlockDto> Blocks);