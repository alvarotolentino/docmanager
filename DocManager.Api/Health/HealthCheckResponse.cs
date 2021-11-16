using System;
using System.Collections.Generic;

namespace DocManager.Api.Health
{
  public class HealthCheckResponse
  {
    public string Status { get; set; }
    public IEnumerable<ItemHealthCheckResponse> HealthChecks { get; set; }
    public TimeSpan HealthCheckDuration { get; set; }
  }

}