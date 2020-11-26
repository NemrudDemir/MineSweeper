using System;
using System.Runtime.Serialization;

namespace SweeperModel.Exceptions
{
    [Serializable]
    public class InvalidFieldSizeException : Exception
    {
        public InvalidFieldSizeException()
        {
        }

        protected InvalidFieldSizeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
