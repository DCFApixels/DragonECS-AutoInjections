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
        private static readonly MethodInfo _includeImplicitMethod;
        private static readonly MethodInfo _excludeImplicitMethod;
        private static readonly MethodInfo _combineMethod;
        static EcsAspectAutoHelper()
        {
            const BindingFlags REFL_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            Type builderType = typeof(EcsAspect.Builder);

            _incluedMethod = builderType.GetMethod("IncludePool", REFL_FLAGS);
            _excludeMethod = builderType.GetMethod("ExcludePool", REFL_FLAGS);
            _optionalMethod = builderType.GetMethod("OptionalPool", REFL_FLAGS);
            _includeImplicitMethod = builderType.GetMethod("IncludeImplicit", REFL_FLAGS);
            _excludeImplicitMethod = builderType.GetMethod("ExcludeImplicit", REFL_FLAGS);
            _combineMethod = builderType.GetMethod("Combine", REFL_FLAGS);
        }
        public static void FillMaskFields(object aspect, EcsMask mask)
        {
            const BindingFlags REFL_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

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
            const BindingFlags REFL_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;


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

                switch (injectAttribute)
                {
                    case IncAttribute atr:
                        var x1 = _incluedMethod;
                        fieldInfo.SetValue(aspect, _incluedMethod.MakeGenericMethod(fieldType).Invoke(builder, null));
                        break;
                    case ExcAttribute atr:
                        var x2 = _excludeMethod;
                        fieldInfo.SetValue(aspect, _excludeMethod.MakeGenericMethod(fieldType).Invoke(builder, null));
                        break;
                    case OptAttribute atr:
                        var x3 = _optionalMethod;
                        fieldInfo.SetValue(aspect, _optionalMethod.MakeGenericMethod(fieldType).Invoke(builder, null));
                        break;
                    case CombineAttribute atr:
                        var x4 = _combineMethod;
                        fieldInfo.SetValue(aspect, _combineMethod.MakeGenericMethod(fieldType).Invoke(builder, new object[] { atr.Order }));
                        break;
                    default:
                        break;
                }
            }


            void Inject(ImplicitInjectAttribute atr_, MethodInfo method_, MethodInfo implicitMethod_)
            {
                if (atr_.IsPool)
                {
                    method_.MakeGenericMethod(atr_.Type).Invoke(builder, null);
                }
                else
                {
                    implicitMethod_.Invoke(builder, new object[] { atr_.Type });
                }
            }
            foreach (var attribute in implicitInjectAttributes)
            {
                if (attribute is IncImplicitAttribute incImplicit)
                {
                    Inject(incImplicit, _incluedMethod, _includeImplicitMethod);
                    continue;
                }
                if (attribute is ExcImplicitAttribute excImplicit)
                {
                    Inject(excImplicit, _excludeMethod, _excludeImplicitMethod);
                    continue;
                }
            }


        }
    }
}
