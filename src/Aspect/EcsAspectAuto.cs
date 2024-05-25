using DCFApixels.DragonECS.AutoInjections.Internal;
using System;
using System.Reflection;

namespace DCFApixels.DragonECS
{
    public abstract class EcsAspectAuto : EcsAspect
    {
        protected sealed override void Init(Builder b)
        {
            EcsAspectAutoHelper.Fill(this, b);
            InitAfterDI(b);
        }
        protected virtual void InitAfterDI(Builder b) { }
    }

    internal static class EcsAspectAutoHelper
    {
        public static void Fill(EcsAspect s, EcsAspect.Builder b)
        {
            Type builderType = b.GetType();
            MethodInfo incluedMethod = builderType.GetMethod("IncludePool", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo excludeMethod = builderType.GetMethod("ExcludePool", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo optionalMethod = builderType.GetMethod("OptionalPool", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo includeImplicitMethod = builderType.GetMethod("IncludeImplicit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo excludeImplicitMethod = builderType.GetMethod("ExcludeImplicit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo combineMethod = builderType.GetMethod("Combine", BindingFlags.Instance | BindingFlags.Public);

            Type aspectType = s.GetType();

            foreach (var attribute in aspectType.GetCustomAttributes<ImplicitInjectAttribute>())//TODO убрать дублирование кода - вынести в отедльный метод
            {
                if (attribute is IncImplicitAttribute incImplicit)
                {
                    if (incImplicit.isPool)
                        incluedMethod.MakeGenericMethod(incImplicit.type).Invoke(b, null);
                    else
                        includeImplicitMethod.Invoke(b, new object[] { incImplicit.type });
                    continue;
                }
                if (attribute is ExcImplicitAttribute excImplicit)
                {
                    if (excImplicit.isPool)
                        excludeMethod.MakeGenericMethod(excImplicit.type).Invoke(b, null);
                    else
                        excludeImplicitMethod.Invoke(b, new object[] { excImplicit.type });
                    continue;
                }
            }//TODO КОНЕЦ убрать дублирование кода - вынести в отедльный метод


            FieldInfo[] fieldInfos = aspectType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                Type fieldType = fieldInfo.FieldType;

                foreach (var attribute in fieldInfo.GetCustomAttributes<ImplicitInjectAttribute>())//TODO убрать дублирование кода - вынести в отедльный метод
                {
                    if (attribute is IncImplicitAttribute incImplicit)
                    {
                        if (incImplicit.isPool)
                            incluedMethod.MakeGenericMethod(incImplicit.type).Invoke(b, null);
                        else
                            includeImplicitMethod.Invoke(b, new object[] { incImplicit.type });
                        continue;
                    }
                    if (attribute is ExcImplicitAttribute excImplicit)
                    {
                        if (excImplicit.isPool)
                            excludeMethod.MakeGenericMethod(excImplicit.type).Invoke(b, null);
                        else
                            excludeImplicitMethod.Invoke(b, new object[] { excImplicit.type });
                        continue;
                    }
                }//TODO КОНЕЦ убрать дублирование кода - вынести в отедльный метод

                if (!fieldInfo.TryGetCustomAttribute(out InjectAspectMemberAttribute injectAttribute))
                {
                    continue;
                }

                if (injectAttribute is IncAttribute)
                {
                    fieldInfo.SetValue(s, incluedMethod.MakeGenericMethod(fieldType).Invoke(b, null));
                    continue;
                }
                if (injectAttribute is ExcAttribute)
                {
                    fieldInfo.SetValue(s, excludeMethod.MakeGenericMethod(fieldType).Invoke(b, null));
                    continue;
                }
                if (injectAttribute is OptAttribute)
                {
                    fieldInfo.SetValue(s, optionalMethod.MakeGenericMethod(fieldType).Invoke(b, null));
                    continue;
                }
                if (injectAttribute is CombineAttribute combAttribute)
                {
                    fieldInfo.SetValue(s, combineMethod.MakeGenericMethod(fieldType).Invoke(b, new object[] { combAttribute.order }));
                    continue;
                }
            }
        }
    }
}
