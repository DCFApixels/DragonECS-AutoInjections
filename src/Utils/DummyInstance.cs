using System;
using System.Reflection;

namespace DCFApixels.DragonECS
{
#if UNITY_2020_3_OR_NEWER
    [UnityEngine.Scripting.Preserve]
#endif
    internal static class DummyInstance<T>
    {
#if UNITY_2020_3_OR_NEWER
        [UnityEngine.Scripting.Preserve]
#endif
        public static T intsance = (T)Activator.CreateInstance(typeof(T));
    }
    internal static class DummyInstance
    {
        private static Type _dummyInstance = typeof(DummyInstance<>);
        public static object GetInstance(Type type)
        {
            return _dummyInstance.MakeGenericType(type).GetField("intsance", BindingFlags.Static | BindingFlags.Public).GetValue(null);
        }
    }
}
