namespace GeoKernel.Examples.GeoAi;

internal readonly record struct Sam3MapPoint(double X, double Y, bool Positive = true);
internal readonly record struct Sam3MapBox(double MinimumX, double MinimumY, double MaximumX, double MaximumY);
internal enum Sam3MapTool { Pan, PositivePoint, NegativePoint, Box }
internal sealed record Sam3Generation(bool TextMode, string Provider, string Text, double ScoreThreshold,
    Sam3MapPoint[] Points, Sam3MapBox? Box);

internal sealed class Sam3Prompts
{
    private readonly List<Sam3MapPoint> points = [];
    public IReadOnlyList<Sam3MapPoint> Points => points;
    public Sam3MapPoint? FirstCorner { get; private set; }
    public Sam3MapBox? Box { get; private set; }
    public string Description => $"{points.Count} points · " +
        (FirstCorner.HasValue ? "click the opposite box corner" : Box.HasValue ? "box set" : "no box");

    public void Add(Sam3MapTool tool, double x, double y)
    {
        if (!double.IsFinite(x) || !double.IsFinite(y) || Math.Abs(x) > 180 || Math.Abs(y) > 90)
            throw new ArgumentException("Click inside the geographic map.");
        if (tool == Sam3MapTool.Box)
        {
            if (FirstCorner is not Sam3MapPoint first)
            {
                Box = null;
                FirstCorner = new(x, y);
            }
            else
            {
                if (first.X == x || first.Y == y) throw new ArgumentException("Choose a box with positive width and height.");
                Box = new(Math.Min(first.X, x), Math.Min(first.Y, y), Math.Max(first.X, x), Math.Max(first.Y, y));
                FirstCorner = null;
            }
        }
        else if (tool is Sam3MapTool.PositivePoint or Sam3MapTool.NegativePoint)
        {
            if (points.Count >= 62) throw new ArgumentException("At most 62 points plus one box are supported.");
            points.Add(new(x, y, tool == Sam3MapTool.PositivePoint));
        }
    }

    public void Clear()
    {
        points.Clear();
        FirstCorner = null;
        Box = null;
    }
}
