using System;
namespace DocManager.Api.Health
{
  public class ItemHealthCheckResponse
  {
    public string Status { get; set; }
    public string Component { get; set; }
    public string Description { get; set; }
  }

}