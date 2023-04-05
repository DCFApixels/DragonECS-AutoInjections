using System;

namespace DCFApixels.DragonECS
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class AutoInjectAttribute : Attribute
    {
        public readonly Type notNullDummyType;

        public AutoInjectAttribute(Type notNullDummyType = null)
        {
            this.notNullDummyType = notNullDummyType;
        }
    }
}

