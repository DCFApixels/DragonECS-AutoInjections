﻿using System;
using System.Linq;

namespace DCFApixels.DragonECS
{
    public abstract class InjectAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class IncAttribute : InjectAttribute { }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class ExcAttribute : InjectAttribute { }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class OptAttribute : InjectAttribute { }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class CombineAttribute : InjectAttribute
    {
        public readonly int order = 0;
        public CombineAttribute(int order = 0) => this.order = order;
    }

    public abstract class ImplicitInjectAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class IncImplicitAttribute : ImplicitInjectAttribute
    {
        public readonly Type type;
        public readonly bool isPool;
        public IncImplicitAttribute(Type type)
        {
            this.type = type;
            isPool = type.GetInterfaces().Any(o => o == typeof(IEcsPoolImplementation));
        }
    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class ExcImplicitAttribute : ImplicitInjectAttribute
    {
        public readonly Type type;
        public readonly bool isPool;
        public ExcImplicitAttribute(Type type)
        {
            this.type = type;
            isPool = type.GetInterfaces().Any(o => o == typeof(IEcsPoolImplementation));
        }
    }
}

