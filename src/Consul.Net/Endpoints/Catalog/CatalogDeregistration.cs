namespace Consul.Net.Endpoints.Catalog
{
  public class CatalogDeregistration
  {
    public string Node { get; set; }
    public string Address { get; set; }
    public string Datacenter { get; set; }
    public string ServiceID { get; set; }
    public string CheckID { get; set; }
  }
}