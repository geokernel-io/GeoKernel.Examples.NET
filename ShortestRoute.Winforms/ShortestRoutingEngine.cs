using GeoKernel.NET.WinForms;

namespace GeoKernel.ShortestRoute.Winforms;

internal readonly record struct RoutePoint(double X, double Y);
internal sealed record RouteNode(int Id, RoutePoint Position);
internal sealed record RouteEdge(int Id, int FromId, int ToId, double Distance, double SpeedKmh,
    IReadOnlyList<RoutePoint> Geometry, string Name);
internal sealed record ShortestRouteResult(IReadOnlyList<int> EdgeIds, IReadOnlyList<RoutePoint> WorldGeometry,
    double Distance, double Time);

internal sealed class ShortestRoutingEngine
{
    private const double EarthRadius = 6378137.0;
    private readonly Dictionary<int, RouteNode> _nodes;
    private readonly Dictionary<int, RouteEdge> _edges;
    private readonly Dictionary<int, List<RouteEdge>> _outEdges;

    public ShortestRoutingEngine(IEnumerable<RouteNode> nodes, IEnumerable<RouteEdge> edges)
    {
        _nodes = nodes.ToDictionary(node => node.Id);
        _edges = edges.ToDictionary(edge => edge.Id);
        _outEdges = _edges.Values.GroupBy(edge => edge.FromId)
            .ToDictionary(group => group.Key, group => group.ToList());
    }

    public RouteNode? NearestNode(RoutePoint point, double maxDistance)
    {
        RouteNode? nearest = null; var best = maxDistance;
        foreach (var node in _nodes.Values)
        {
            var distance = GeographicDistance(point, node.Position);
            if (distance >= best) continue;
            best = distance; nearest = node;
        }
        return nearest;
    }

    public ShortestRouteResult? FindRoute(int start, int finish)
    {
        var distances = new Dictionary<int, double> { [start] = 0 };
        var previous = new Dictionary<int, RouteEdge>();
        var queue = new PriorityQueue<int, double>(); queue.Enqueue(start, 0);
        while (queue.TryDequeue(out var node, out var distance))
        {
            if (distance > distances.GetValueOrDefault(node, double.PositiveInfinity)) continue;
            if (node == finish) break;
            foreach (var edge in _outEdges.GetValueOrDefault(node) ?? [])
            {
                var candidate = distance + edge.Distance;
                if (candidate >= distances.GetValueOrDefault(edge.ToId, double.PositiveInfinity)) continue;
                distances[edge.ToId] = candidate; previous[edge.ToId] = edge;
                queue.Enqueue(edge.ToId, candidate);
            }
        }
        if (!distances.ContainsKey(finish)) return null;
        var edgeIds = new List<int>();
        for (var node = finish; node != start;)
        {
            if (!previous.TryGetValue(node, out var edge)) return null;
            edgeIds.Add(edge.Id); node = edge.FromId;
        }
        edgeIds.Reverse(); var geometry = new List<RoutePoint>(); double totalDistance = 0, totalTime = 0;
        foreach (var id in edgeIds)
        {
            var edge = _edges[id]; totalDistance += edge.Distance;
            if (edge.SpeedKmh > 0) totalTime += edge.Distance / (edge.SpeedKmh * 1000 / 3600);
            foreach (var point in edge.Geometry.Select(ToWebMercator))
                if (geometry.Count == 0 || geometry[^1] != point) geometry.Add(point);
        }
        return geometry.Count < 2 ? null : new ShortestRouteResult(edgeIds, geometry, totalDistance, totalTime);
    }

    public IReadOnlyList<(string Name, double Distance)> RoadSteps(ShortestRouteResult route)
    {
        var result = new List<(string, double)>();
        foreach (var id in route.EdgeIds)
        {
            var edge = _edges[id]; var name = string.IsNullOrWhiteSpace(edge.Name) ? "Unnamed road" : edge.Name.Trim();
            if (result.Count > 0 && string.Equals(result[^1].Item1, name, StringComparison.OrdinalIgnoreCase))
                result[^1] = (name, result[^1].Item2 + edge.Distance);
            else result.Add((name, edge.Distance));
        }
        return result;
    }

    public static RoutePoint ToWgs84(RoutePoint point) => new(point.X / EarthRadius * 180 / Math.PI,
        (2 * Math.Atan(Math.Exp(point.Y / EarthRadius)) - Math.PI / 2) * 180 / Math.PI);
    public static RoutePoint ToWebMercator(RoutePoint point) => new(EarthRadius * point.X * Math.PI / 180,
        EarthRadius * Math.Log(Math.Tan(Math.PI / 4 + point.Y * Math.PI / 360)));
    private static double GeographicDistance(RoutePoint a, RoutePoint b)
    {
        var latitude1 = a.Y * Math.PI / 180; var latitude2 = b.Y * Math.PI / 180;
        var dLatitude = latitude2 - latitude1; var dLongitude = (b.X - a.X) * Math.PI / 180;
        var h = Math.Sin(dLatitude / 2) * Math.Sin(dLatitude / 2) + Math.Cos(latitude1) * Math.Cos(latitude2) *
            Math.Sin(dLongitude / 2) * Math.Sin(dLongitude / 2);
        return 2 * EarthRadius * Math.Asin(Math.Min(1, Math.Sqrt(h)));
    }
}
