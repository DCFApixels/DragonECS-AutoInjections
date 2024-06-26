﻿using System;

namespace DCFApixels.DragonECS
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class EcsInjectAttribute : Attribute
    {
        public static readonly EcsInjectAttribute Dummy = new EcsInjectAttribute(null);
        public readonly Type notNullDummyType;
        public EcsInjectAttribute(Type notNullDummyType = null)
        {
            this.notNullDummyType = notNullDummyType;
        }
    }
}

