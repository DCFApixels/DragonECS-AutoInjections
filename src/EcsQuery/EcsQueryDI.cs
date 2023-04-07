using System;
using System.Reflection;

namespace DCFApixels.DragonECS
{
    public class EcsQueryDI<TWorldArhetype> :EcsQuery<TWorldArhetype> where TWorldArhetype : EcsWorld<TWorldArhetype>
    {
        protected override void Init(Builder b)
        {
            Type builderType= b.GetType();
            MethodInfo incluedMethod= builderType.GetMethod("Include", BindingFlags.Instance| BindingFlags.Public);
            MethodInfo excludeMethod= builderType.GetMethod("Exclude", BindingFlags.Instance| BindingFlags.Public);
            MethodInfo optionalMethod= builderType.GetMethod("Optional", BindingFlags.Instance| BindingFlags.Public);

            Type thisType = GetType();
            FieldInfo[] fieldInfos = thisType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                Type fiedlType = fieldInfo.FieldType;
                if (fiedlType.IsGenericType == false)
                    continue;
                Type fiedlTypeDefinition = fiedlType.GetGenericTypeDefinition();
                Type genericArg = fiedlType.GenericTypeArguments[0];

                if (fiedlTypeDefinition == typeof(inc<>))
                {
                    fieldInfo.SetValue(this, incluedMethod.MakeGenericMethod(genericArg).Invoke(b, null));
                }
                if(fiedlTypeDefinition == typeof(exc<>))
                {
                    fieldInfo.SetValue(this, excludeMethod.MakeGenericMethod(genericArg).Invoke(b, null));
                }
                if (fiedlTypeDefinition == typeof(opt<>))
                {
                    fieldInfo.SetValue(this, optionalMethod.MakeGenericMethod(genericArg).Invoke(b, null));
                }
            }
        }
    }
}
