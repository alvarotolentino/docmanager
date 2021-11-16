using System;
using Application.Interfaces;

namespace Infrastructure.Shared.Services
{
    public class DateTimeServices : IDateTimeService
    {
        public DateTime UtcDateTime => DateTime.UtcNow;
    }
}