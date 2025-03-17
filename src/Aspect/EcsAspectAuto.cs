#if DISABLE_DEBUG
#undef DEBUG
#endif
using DCFApixels.DragonECS.AutoInjections.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DCFApixels.DragonECS
{
    public abstract class EcsAspectAuto : EcsAspect
    {
        protected sealed override void Init(Builder b)
        {
            //EcsAspectAutoHelper.Fill(this, b);
            InitAfterDI(b);
        }
        protected virtual void InitAfterDI(Builder b) { }
    }

    internal static class EcsAspectAutoHelper
    {
        private static readonly MethodInfo _incluedMethod;
        private static readonly MethodInfo _excludeMethod;
        private static readonly MethodInfo _optionalMethod;
        private static readonly MethodInfo _combineMethod;
        private const BindingFlags REFL_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        static EcsAspectAutoHelper()
        {
            Type builderType = typeof(EcsAspect.Builder);

            _incluedMethod = builderType.GetMethod("IncludePool", REFL_FLAGS);
            _excludeMethod = builderType.GetMethod("ExcludePool", REFL_FLAGS);
            _optionalMethod = builderType.GetMethod("OptionalPool", REFL_FLAGS);
            _combineMethod = builderType.GetMethod("Combine", REFL_FLAGS);
        }
        public static void FillMaskFields(object aspect, EcsMask mask)
        {
            foreach (FieldInfo fieldInfo in aspect.GetType().GetFields(REFL_FLAGS))
            {
                if (fieldInfo.GetCustomAttribute<MaskAttribute>() == null)
                {
                    continue;
                }

                if (fieldInfo.FieldType == typeof(EcsMask))
                {
                    fieldInfo.SetValue(aspect, mask);
                }
                else if (fieldInfo.FieldType == typeof(EcsStaticMask))
                {
                    fieldInfo.SetValue(aspect, mask.ToStatic());
                }
            }
        }
        public static void FillFields(object aspect, EcsAspect.Builder builder)
        {
            Type aspectType = aspect.GetType();

            var implicitInjectAttributes = (IEnumerable<ImplicitInjectAttribute>)aspectType.GetCustomAttributes<ImplicitInjectAttribute>();

            FieldInfo[] fieldInfos = aspectType.GetFields(REFL_FLAGS);
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                Type fieldType = fieldInfo.FieldType;

                implicitInjectAttributes = implicitInjectAttributes.Concat(fieldInfo.GetCustomAttributes<ImplicitInjectAttribute>());

                if (fieldInfo.TryGetCustomAttribute(out InjectAspectMemberAttribute injectAttribute) == false)
                {
                    continue;
                }
                IEcsPool pool;
                switch (injectAttribute)
                {
                    case IncAttribute incAtr:
                        if (builder.World.TryFindPoolInstance(fieldType, out pool))
                        {
                            builder.SetMaskInclude(fieldType);
                            fieldInfo.SetValue(aspect, pool);
                        }
                        else
                        {
                            pool = (IEcsPool)_incluedMethod.MakeGenericMethod(fieldType).Invoke(builder, null);
                        }
                        fieldInfo.SetValue(aspect, pool);
                        break;
                    case ExcAttribute extAtr:
                        if (builder.World.TryFindPoolInstance(fieldType, out pool))
                        {
                            builder.SetMaskExclude(fieldType);
                            fieldInfo.SetValue(aspect, pool);
                        }
                        else
                        {
                            pool = (IEcsPool)_excludeMethod.MakeGenericMethod(fieldType).Invoke(builder, null);
                        }
                        fieldInfo.SetValue(aspect, pool);
                        break;
                    case OptAttribute optAtr:
                        if (builder.World.TryFindPoolInstance(fieldType, out pool))
                        {
                            fieldInfo.SetValue(aspect, pool);
                        }
                        else
                        {
                            pool = (IEcsPool)_optionalMethod.MakeGenericMethod(fieldType).Invoke(builder, null);
                        }
                        fieldInfo.SetValue(aspect, pool);
                        break;
                    case CombineAttribute combineAtr:
                        pool = builder.World.FindPoolInstance(fieldType);
                        fieldInfo.SetValue(aspect, _combineMethod.MakeGenericMethod(fieldType).Invoke(builder, new object[] { combineAtr.Order }));
                        break;
                    default:
                        break;
                }
            }


            foreach (var attribute in implicitInjectAttributes)
            {
                if (attribute is IncImplicitAttribute incImplicitAtr)
                {
                    builder.SetMaskInclude(incImplicitAtr.ComponentType);
                    continue;
                }
                if (attribute is ExcImplicitAttribute excImplicitAtr)
                {
                    builder.SetMaskExclude(excImplicitAtr.ComponentType);
                    continue;
                }
            }


        }
    }
}
