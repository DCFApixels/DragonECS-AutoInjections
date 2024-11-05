using System;

namespace DCFApixels.DragonECS
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class DIAttribute : Attribute
    {
        public static readonly DIAttribute Dummy = new DIAttribute(null);
        public readonly Type notNullDummyType;
        public DIAttribute(Type notNullDummyType = null)
        {
            this.notNullDummyType = notNullDummyType;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    [Obsolete("Use DI attribute")]
    public sealed class EcsInjectAttribute : DIAttribute
    {
        public EcsInjectAttribute(Type notNullDummyType = null) : base(notNullDummyType) { }
    }
}

