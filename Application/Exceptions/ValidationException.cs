using System;
using System.Collections.Generic;

namespace Application.Exceptions
{
    public class ValidationException : ApiException
    {
        public IReadOnlyDictionary<string, string[]> Errors { get; }
        // 
        public ValidationException(string message, IReadOnlyDictionary<string, string[]> errors)
        : base((string)Resources.Reader.GetMessages()["ExceptionMessage"]["Validation"], message)
        {
            Errors = errors;
        }
    }
}