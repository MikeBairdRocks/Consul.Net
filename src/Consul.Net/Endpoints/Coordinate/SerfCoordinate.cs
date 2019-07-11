using System.Collections.Generic;

namespace Consul.Net.Endpoints.Coordinate
{
  public class SerfCoordinate
  {
    public List<double> Vec { get; set; }
    public double Error { get; set; }
    public double Adjustment { get; set; }
    public double Height { get; set; }
    public SerfCoordinate()
    {
      Vec = new List<double>();
    }
  }
}