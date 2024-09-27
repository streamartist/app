using System;

namespace StreamArtist.Domain
{
    public class GoogleAccessTokenException : Exception
    {
        public GoogleAccessTokenException() : base() { }

        public GoogleAccessTokenException(string message) : base(message) { }

        public GoogleAccessTokenException(string message, Exception innerException) : base(message, innerException) { }
    }
}