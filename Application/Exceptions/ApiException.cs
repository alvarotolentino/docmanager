using System;

namespace Application.Exceptions
{
    public class ApiException: Exception
    {
        public string Title { get; }
        public ApiException(string title, string message)
        :base (message)
        {
            Title = title;
        }
    }
}