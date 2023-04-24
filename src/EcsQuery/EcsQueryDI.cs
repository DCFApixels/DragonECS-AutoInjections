using System;
using System.Reflection;

namespace DCFApixels.DragonECS
{
    public abstract class EcsJoinAttachQueryDI<TAttachComponent> : EcsJoinAttachQuery<TAttachComponent>
        where TAttachComponent : struct, IEcsAttachComponent
    {
        protected override void Init(Builder b) => EcsQueryDIHelper.Fill(this, b);
    }
    public abstract class EcsQueryDI : EcsQuery
    {
        protected override void Init(Builder b) => EcsQueryDIHelper.Fill(this, b);
    }

    internal static class EcsQueryDIHelper
    {
        public static void Fill(EcsQueryBase q, EcsQueryBase.Builder b)
        {
            Type builderType = b.GetType();
            MethodInfo incluedMethod = builderType.GetMethod("Include", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo excludeMethod = builderType.GetMethod("Exclude", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo optionalMethod = builderType.GetMethod("Optional", BindingFlags.Instance | BindingFlags.Public);

            Type thisType = q.GetType();
            FieldInfo[] fieldInfos = thisType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                Type fieldType = fieldInfo.FieldType;
                if (fieldType.IsSubclassOf(typeof(EcsPoolBase)) == false)
                    continue;
                if (fieldType.IsGenericType == false)
                    continue;

                Type componentType = fieldType.GenericTypeArguments[0];

                if (fieldInfo.GetCustomAttribute<IncAttribute>() != null)
                {
                    fieldInfo.SetValue(q, incluedMethod.MakeGenericMethod(componentType, fieldType).Invoke(b, null));
                    continue;
                }
                if (fieldInfo.GetCustomAttribute<ExcAttribute>() != null)
                {
                    fieldInfo.SetValue(q, excludeMethod.MakeGenericMethod(componentType, fieldType).Invoke(b, null));
                    continue;
                }
                if (fieldInfo.GetCustomAttribute<OptAttribute>() != null)
                {
                    fieldInfo.SetValue(q, optionalMethod.MakeGenericMethod(componentType, fieldType).Invoke(b, null));
                    continue;
                }
            }
        }
    }
}
