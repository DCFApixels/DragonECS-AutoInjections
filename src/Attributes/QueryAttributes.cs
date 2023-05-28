﻿using System;
using System.Linq;

namespace DCFApixels.DragonECS
{
    public class InjectAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class IncAttribute : InjectAttribute { }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class ExcAttribute : InjectAttribute { }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class OptAttribute : InjectAttribute { }


    public abstract class ImplicitInjectAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class IncImplicitAttribute : ImplicitInjectAttribute
    {
        public readonly Type type;
        public readonly bool isPool;

        public IncImplicitAttribute(Type type)
        {
            if (type.IsValueType && !type.IsPrimitive)
            {
                isPool = false;
                this.type = type;
                return;
            }
            if (!type.GetInterfaces().Any(o => o == typeof(IEcsPoolImplementation)))
                throw new ArgumentException("Можно использовать только пулы наследованные от IEcsPoolImplementation<T>");
            this.type = type;
            isPool = true;
        }
    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class ExcImplicitAttribute : ImplicitInjectAttribute
    {
        public readonly Type type;
        public readonly bool isPool;
        public ExcImplicitAttribute(Type type)
        {
            if (type.IsValueType && !type.IsPrimitive)
            {
                isPool = false;
                this.type = type;
                return;
            }
            if (!type.GetInterfaces().Any(o => o == typeof(IEcsPoolImplementation)))
                throw new ArgumentException("Можно использовать только пулы наследованные от IEcsPoolImplementation<T>");
            this.type = type;
            isPool = true;
        }
    }
}
