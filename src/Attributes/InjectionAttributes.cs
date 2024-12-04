using System;

namespace DCFApixels.DragonECS
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class DIAttribute : Attribute
    {
        public static readonly DIAttribute Dummy = new DIAttribute();
        public readonly Type NotNullDummyType = null;
        public readonly string NamedInjection = string.Empty;
        public DIAttribute() { }
        public DIAttribute(string namedInjection)
        {
            NamedInjection = namedInjection;
        }
        public DIAttribute(Type notNullDummyType)
        {
            NotNullDummyType = notNullDummyType;
        }
        public DIAttribute(string namedInjection, Type notNullDummyType)
        {
            NamedInjection = namedInjection;
            NotNullDummyType = notNullDummyType;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    [Obsolete("Use DI attribute")]
    public sealed class EcsInjectAttribute : DIAttribute
    {
        public EcsInjectAttribute(Type notNullDummyType = null) : base(notNullDummyType) { }
    }
}

