using System.Collections.Generic;

namespace Application.Common
{
    public class Response<T>
    {
        public Response(T data = default(T),
        string message = null,
        bool succeeded = true,
        List<string> errors = default(List<string>))
        {
            Data = data;
            Message = message;
            Succeeded = succeeded;
            Errors = errors;
        }
        public bool Succeeded { get; }
        public string Message { get; }
        public List<string> Errors { get; }
        public T Data { get; }
    }
}