using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;

namespace DCFApixels.DragonECS
{
    namespace AutoInjections.Internal
    {
        internal static class Throw
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            internal static void PropertyIsCantWrite(MemberInfo obj)
            {
                throw new EcsAutoInjectionException($"{obj.Name} property is cant write");
            }
            [MethodImpl(MethodImplOptions.NoInlining)]
            internal static void MethodIsGeneric(MemberInfo obj)
            {
                throw new EcsAutoInjectionException($"{obj.Name} method is Generic");
            }
            [MethodImpl(MethodImplOptions.NoInlining)]
            internal static void MethodArgumentsGreater1(MemberInfo obj)
            {
                //method X has arguments greater than 1.
                throw new EcsAutoInjectionException($"{obj.Name} method Arguments != 1");
            }
            [MethodImpl(MethodImplOptions.NoInlining)]
            internal static void UndefinedException()
            {
                throw new Exception();
            }
        }
    }

    [Serializable]
    public class EcsAutoInjectionException : Exception
    {
        public EcsAutoInjectionException() { }
        public EcsAutoInjectionException(string message) : base(EcsConsts.EXCEPTION_MESSAGE_PREFIX + message) { }
        public EcsAutoInjectionException(string message, Exception inner) : base(EcsConsts.EXCEPTION_MESSAGE_PREFIX + message, inner) { }
    }
}
