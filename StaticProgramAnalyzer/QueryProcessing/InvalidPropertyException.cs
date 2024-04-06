using System;
using System.Runtime.Serialization;

namespace StaticProgramAnalyzer.QueryProcessing
{
    [Serializable]
    public class InvalidPropertyException : Exception
    {
        public InvalidPropertyException()
        {
        }

        public InvalidPropertyException(string message) : base(message)
        {
        }

        public InvalidPropertyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidPropertyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}