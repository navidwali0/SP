using System;
using System.Runtime.Serialization;

namespace SPCommon.CustomException
{
    [Serializable]
    public class BaseException : Exception
    {
        public BaseException()
        {}

        protected BaseException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public BaseException(string message) : base(message)
        {}

        public BaseException(string message, System.Exception innerException) : base(message, innerException)
        {}
    }
}
