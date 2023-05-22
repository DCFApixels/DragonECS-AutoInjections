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
            MethodInfo includeImplicitMethod = builderType.GetMethod("IncludeImplicit", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo excludeImplicitMethod = builderType.GetMethod("ExcludeImplicit", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo optionalMethod = builderType.GetMethod("Optional", BindingFlags.Instance | BindingFlags.Public);

            Type subjectType = s.GetType();

            foreach (var attribute in subjectType.GetCustomAttributes<ImplicitInjectAttribute>())//TODO убрать дублирование кода - вынести в отедльный метод
            {
                if (attribute is IncImplicitAttribute incImplicit)
                {
                    if (incImplicit.isPool)
                        incluedMethod.MakeGenericMethod(incImplicit.type.GenericTypeArguments[0], incImplicit.type).Invoke(b, null);
                    else
                        includeImplicitMethod.MakeGenericMethod(incImplicit.type).Invoke(b, null);
                    continue;
                }
                if (attribute is ExcImplicitAttribute excImplicit)
                {
                    if (excImplicit.isPool)
                        excludeMethod.MakeGenericMethod(excImplicit.type.GenericTypeArguments[0], excImplicit.type).Invoke(b, null);
                    else
                        excludeImplicitMethod.MakeGenericMethod(excImplicit.type).Invoke(b, null);
                    continue;
                }
            }//TODO КОНЕЦ убрать дублирование кода - вынести в отедльный метод


            FieldInfo[] fieldInfos = subjectType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                Type fieldType = fieldInfo.FieldType;

                foreach (var attribute in fieldInfo.GetCustomAttributes<ImplicitInjectAttribute>())//TODO убрать дублирование кода - вынести в отедльный метод
                {
                    if(attribute is IncImplicitAttribute incImplicit)
                    {
                        if(incImplicit.isPool)
                            incluedMethod.MakeGenericMethod(incImplicit.type.GenericTypeArguments[0], incImplicit.type).Invoke(b, null);
                        else
                            includeImplicitMethod.MakeGenericMethod(incImplicit.type).Invoke(b, null);
                        continue;
                    }
                    if (attribute is ExcImplicitAttribute excImplicit)
                    {
                        if (excImplicit.isPool)
                            excludeMethod.MakeGenericMethod(excImplicit.type.GenericTypeArguments[0], excImplicit.type).Invoke(b, null);
                        else
                            excludeImplicitMethod.MakeGenericMethod(excImplicit.type).Invoke(b, null);
                        continue;
                    }
                }//TODO КОНЕЦ убрать дублирование кода - вынести в отедльный метод

                if (fieldInfo.GetCustomAttribute<InjectAttribute>() == null)
                    continue;
                if (fieldType.IsGenericType == false)
                    continue;

                Type componentType = fieldType.GenericTypeArguments[0];

                if (fieldInfo.GetCustomAttribute<IncAttribute>() != null)
                {
                    fieldInfo.SetValue(s, incluedMethod.MakeGenericMethod(componentType, fieldType).Invoke(b, null));
                    continue;
                }
                if (fieldInfo.GetCustomAttribute<ExcAttribute>() != null)
                {
                    fieldInfo.SetValue(s, excludeMethod.MakeGenericMethod(componentType, fieldType).Invoke(b, null));
                    continue;
                }
                if (fieldInfo.GetCustomAttribute<OptAttribute>() != null)
                {
                    fieldInfo.SetValue(s, optionalMethod.MakeGenericMethod(componentType, fieldType).Invoke(b, null));
                    continue;
                }
            }
        }
    }
}
