using System.Collections.Generic;

namespace Consul.Net.Endpoints.Coordinate
{
  public class CoordinateDatacenterMap
  {
    public string Datacenter { get; set; }
    public List<CoordinateEntry> Coordinates { get; set; }
    public CoordinateDatacenterMap()
    {
      Coordinates = new List<CoordinateEntry>();
    }
  }
}