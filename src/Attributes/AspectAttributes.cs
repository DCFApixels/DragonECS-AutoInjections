#if DISABLE_DEBUG
#undef DEBUG
#endif
using DCFApixels.DragonECS.PoolsCore;
using System;
using System.Linq;

namespace DCFApixels.DragonECS
{
    public abstract class InjectAspectMemberAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class IncAttribute : InjectAspectMemberAttribute { }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class ExcAttribute : InjectAspectMemberAttribute { }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class OptAttribute : InjectAspectMemberAttribute { }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class CombineAttribute : InjectAspectMemberAttribute
    {
        public readonly int Order = 0;
        public CombineAttribute(int order = 0) { Order = order; }
    }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class MaskAttribute : InjectAspectMemberAttribute { }


    public abstract class ImplicitInjectAttribute : Attribute
    {
        public readonly Type Type;
        public readonly bool IsPool;
        public ImplicitInjectAttribute(Type type)
        {
            Type = type;
            IsPool = type.GetInterfaces().Any(o => o == typeof(IEcsPoolImplementation));
        }
    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class IncImplicitAttribute : ImplicitInjectAttribute
    {
        public IncImplicitAttribute(Type type) : base(type) { }
    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class ExcImplicitAttribute : ImplicitInjectAttribute
    {
        public ExcImplicitAttribute(Type type) : base(type) { }
    }
}

