using System;
using System.Reflection;

namespace DCFApixels.DragonECS
{
    public abstract class EcsSubjectDI : EcsSubject
    {
        protected sealed override void Init(Builder b)
        {
            EcsSubjectDIHelper.Fill(this, b);
            InitAfterDI(b);
        }
        protected virtual void InitAfterDI(Builder b) { }
    }

    internal static class EcsSubjectDIHelper
    {
        public static void Fill(EcsSubject s, EcsSubject.Builder b)
        {
            Type builderType = b.GetType();
            MethodInfo incluedMethod = builderType.GetMethod("Include", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo excludeMethod = builderType.GetMethod("Exclude", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo optionalMethod = builderType.GetMethod("Optional", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo includeImplicitMethod = builderType.GetMethod("IncludeImplicit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo excludeImplicitMethod = builderType.GetMethod("ExcludeImplicit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo combineMethod = builderType.GetMethod("Combine", BindingFlags.Instance | BindingFlags.Public);

            Type subjectType = s.GetType();

            foreach (var attribute in subjectType.GetCustomAttributes<ImplicitInjectAttribute>())//TODO убрать дублирование кода - вынести в отедльный метод
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


            FieldInfo[] fieldInfos = subjectType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
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

                if (!fieldInfo.TryGetAttribute(out InjectAttribute injectAttribute))
                    continue;

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
