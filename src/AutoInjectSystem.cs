using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static DCFApixels.DragonECS.EcsThrowHalper;

namespace DCFApixels.DragonECS
{
    internal class AutoInjectionMap
    {
        private readonly EcsPipeline _source;
        private Dictionary<Type, List<InjectedPropertyRecord>> _systemProoperties;
        private HashSet<Type> _notInjected;
        private bool _isDummyInjected = false;

        private bool _isPreInitInjectionComplete = false;

        public AutoInjectionMap(EcsPipeline source)
        {
            _source = source;
            var allsystems = _source.AllSystems;
            _systemProoperties = new Dictionary<Type, List<InjectedPropertyRecord>>();
            _notInjected = new HashSet<Type>();
            foreach (var system in allsystems)
            {
                Type systemType = system.GetType();
                foreach (var property in GetAllPropertiesFor(systemType))
                {
                    Type fieldType = property.PropertyType;
                    List<InjectedPropertyRecord> list;
                    if (!_systemProoperties.TryGetValue(fieldType, out list))
                    {
                        list = new List<InjectedPropertyRecord>();
                        _systemProoperties.Add(fieldType, list);
                    }
                    list.Add(new InjectedPropertyRecord(system, property));
                }
            }
            foreach (var item in _systemProoperties.Keys)
                _notInjected.Add(item);
        }

        private static void Do(Type type, List<IInjectedProperty> result)
        {
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            result.AddRange(type.GetFields(bindingFlags)
                .Where(o => o.GetCustomAttribute<EcsInjectAttribute>() != null)
                .Select(o => new InjectedField(o)));
            result.AddRange(type.GetProperties(bindingFlags)
                .Where(o =>
                {
                    if (o.GetCustomAttribute<EcsInjectAttribute>() == null)
                        return false;
#if (DEBUG && !DISABLE_DEBUG) || ENABLE_DRAGONECS_ASSERT_CHEKS
                    if (o.CanWrite == false) Throw.PropertyIsCantWrite(o);
#endif
                    return true;
                })
                .Select(o => new InjectedProperty(o)));
            result.AddRange(type.GetMethods(bindingFlags)
                .Where(o =>
                {
                    if (o.GetCustomAttribute<EcsInjectAttribute>() == null)
                        return false;
#if (DEBUG && !DISABLE_DEBUG) || ENABLE_DRAGONECS_ASSERT_CHEKS
                    if (o.IsGenericMethod) Throw.MethodIsGeneric(o);
                    if (o.GetParameters().Length != 1) Throw.MethodArgumentsGreater1(o);
#endif
                    return true;
                })
                .Select(o => new InjectedMethod(o)));
            if (type.BaseType != null)
                Do(type.BaseType, result);
        }

        private static List<IInjectedProperty> GetAllPropertiesFor(Type type)
        {
            List<IInjectedProperty> result = new List<IInjectedProperty>();
            Do(type, result);
            return result;
        }
        public void Inject(Type fieldType, object obj)
        {
            if (!_isPreInitInjectionComplete)
                _notInjected.Remove(fieldType);

            Type baseType = fieldType.BaseType;
            if (baseType != null)
                Inject(baseType, obj);

            if (_systemProoperties.TryGetValue(fieldType, out List<InjectedPropertyRecord> list))
            {
                foreach (var item in list)
                    item.property.Inject(item.target, obj);
            }
        }

        public void InjectDummy()
        {
            if (_isDummyInjected)
                return;
            _isDummyInjected = true;
            foreach (var notInjectedItem in _notInjected)
            {
                foreach (var systemRecord in _systemProoperties[notInjectedItem])
                {
                    if (systemRecord.Attribute.notNullDummyType == null)
                        continue;
                    if (systemRecord.property.IsInjected)
                        continue;
                    if (systemRecord.property.PropertyType.IsAssignableFrom(systemRecord.Attribute.notNullDummyType) == false)
                    {
                        EcsDebug.Print(EcsConsts.DEBUG_ERROR_TAG, $"The {systemRecord.Attribute.notNullDummyType} dummy cannot be assigned to the {systemRecord.property.PropertyType.Name} field");
                        continue;
                    }
                    systemRecord.property.Inject(systemRecord.target, DummyInstance.GetInstance(systemRecord.Attribute.notNullDummyType));
                }
            }
            WarningMissedInjections();
            _notInjected.Clear();
            _notInjected = null;
        }

        private void WarningMissedInjections()
        {
#if DEBUG   
            foreach (var item in _notInjected)
            {
                foreach (var systemRecord in _systemProoperties[item])
                    EcsDebug.PrintWarning($"in system {EcsDebugUtility.GetGenericTypeFullName(systemRecord.target.GetType(), 1)} is missing an injection of {EcsDebugUtility.GetGenericTypeFullName(item, 1)}.");
            }
#endif
        }

        public void OnPreInitInjectionComplete()
        {
            _isPreInitInjectionComplete = true;
        }

        private readonly struct InjectedPropertyRecord
        {
            public readonly IEcsProcess target;
            public readonly IInjectedProperty property;
            public EcsInjectAttribute Attribute => property.GetAutoInjectAttribute();
            public InjectedPropertyRecord(IEcsProcess target, IInjectedProperty property)
            {
                this.target = target;
                this.property = property;
            }
        }
    }

    [DebugHide, DebugColor(DebugColor.Gray)]
    public class AutoInjectSystem : IEcsPreInject, IEcsInject<EcsPipeline>, IEcsPreInitInjectProcess
    {
        private EcsPipeline _pipeline;
        private List<object> _delayedInjects = new List<object>();
        private AutoInjectionMap _autoInjectionMap;
        private bool _preInitInjectCompleted = false;
        public void Inject(EcsPipeline obj) => _pipeline = obj;
        public void PreInject(object obj)
        {
            if (!_preInitInjectCompleted)
                _delayedInjects.Add(obj);
            else
                _autoInjectionMap.Inject(obj.GetType(), obj);
        }
        public void OnPreInitInjectionBefore() { }
        public void OnPreInitInjectionAfter()
        {
            _autoInjectionMap = new AutoInjectionMap(_pipeline);
            _preInitInjectCompleted = true;

            foreach (var obj in _delayedInjects)
                _autoInjectionMap.Inject(obj.GetType(), obj);
            _autoInjectionMap.InjectDummy();
            _autoInjectionMap.OnPreInitInjectionComplete();

            _delayedInjects.Clear();
            _delayedInjects = null;
            GC.Collect(0);
        }
    }

    #region Utils
    internal interface IInjectedProperty
    {
        bool IsInjected { get; }
        Type PropertyType { get; }
        EcsInjectAttribute GetAutoInjectAttribute();
        void Inject(object target, object value);
    }
    internal class InjectedField : IInjectedProperty
    {
        private FieldInfo _member;
        private EcsInjectAttribute _injectAttribute;
        public bool IsInjected { get; private set; }
        public Type PropertyType => _member.FieldType;
        public InjectedField(FieldInfo member)
        {
            _member = member;
            _injectAttribute = member.GetCustomAttribute<EcsInjectAttribute>();
        }
        public EcsInjectAttribute GetAutoInjectAttribute() => _injectAttribute;
        public void Inject(object target, object value)
        {
            _member.SetValue(target, value);
            IsInjected = true;
        }
    }
    internal class InjectedProperty : IInjectedProperty
    {
        private PropertyInfo _member;
        private EcsInjectAttribute _injectAttribute;
        public bool IsInjected { get; private set; }
        public Type PropertyType => _member.PropertyType;
        public InjectedProperty(PropertyInfo member)
        {
            _member = member;
            _injectAttribute = member.GetCustomAttribute<EcsInjectAttribute>();
        }
        public EcsInjectAttribute GetAutoInjectAttribute() => _injectAttribute;
        public void Inject(object target, object value)
        {
            _member.SetValue(target, value);
            IsInjected = true;
        }
    }
    internal class InjectedMethod : IInjectedProperty
    {
        private MethodInfo _member;
        private EcsInjectAttribute _injectAttribute;
        private Type propertyType;
        public bool IsInjected { get; private set; }
        public Type PropertyType => propertyType;
        public InjectedMethod(MethodInfo member)
        {
            _member = member;
            _injectAttribute = member.GetCustomAttribute<EcsInjectAttribute>();
            propertyType = _member.GetParameters()[0].ParameterType;
        }
        public EcsInjectAttribute GetAutoInjectAttribute() => _injectAttribute;
        public void Inject(object target, object value)
        {
            _member.Invoke(target, new object[] { value });
            IsInjected = true;
        }
    }
    #endregion
}