using System;

namespace Application.Exceptions
{
    [Serializable]
    public class NotFoundException : ApiException
    {
        public NotFoundException(string message)
        : base((string)Resources.Reader.GetMessages()["ExceptionMessage"]["NotFound"], message)
        {

        }
    }
}