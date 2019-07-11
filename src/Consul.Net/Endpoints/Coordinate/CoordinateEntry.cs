namespace Consul.Net.Endpoints.Coordinate
{
  public class CoordinateEntry
  {
    public string Node { get; set; }
    public SerfCoordinate Coord { get; set; }
  }
}