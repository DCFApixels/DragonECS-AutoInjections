using System;
using System.Runtime.Serialization;

namespace DCFApixels.DragonECS
{
    [Serializable]
    public class EcsAutoInjectionException : Exception
    {
        public EcsAutoInjectionException() { }
        public EcsAutoInjectionException(string message) : base(EcsConsts.EXCEPTION_MESSAGE_PREFIX + message) { }
        public EcsAutoInjectionException(string message, Exception inner) : base(EcsConsts.EXCEPTION_MESSAGE_PREFIX + message, inner) { }
        protected EcsAutoInjectionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
