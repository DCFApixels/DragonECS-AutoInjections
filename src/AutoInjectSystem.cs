using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DCFApixels.DragonECS
{
    internal static class DummyInstance<T>
    {
        public static T intsance = (T)Activator.CreateInstance(typeof(T));
    }
    internal class AutoInjectionMap
    {
        private readonly EcsPipeline _source;
        private Dictionary<Type, List<InjectedPropertyRecord>> _systemProoperties;
        private HashSet<Type> _notInjected;
        private Type _dummyInstance = typeof(DummyInstance<>);
        private bool _isDummyInjected = false;

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
                    if (property.GetAutoInjectAttribute() == null)
                        continue;

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
        private static List<IInjectedProperty> GetAllPropertiesFor(Type type)
        {
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            List<IInjectedProperty> result = new List<IInjectedProperty>();
            Do(type, result);
            static void Do(Type type, List<IInjectedProperty> result)
            {
                result.AddRange(type.GetFields(bindingFlags)
                    .Where(o => o.GetCustomAttribute<EcsInjectAttribute>() != null)
                    .Select(o => new InjectedField(o)));
                result.AddRange(type.GetProperties(bindingFlags)
                    .Where(o =>{
                        if (o.GetCustomAttribute<EcsInjectAttribute>() == null)
                            return false;
#if (DEBUG && !DISABLE_DEBUG) || ENABLE_DRAGONECS_ASSERT_CHEKS
                        if (o.CanWrite == false)
                            throw new EcsAutoInjectionException($"{o.Name} property is cant write");
#endif
                        return true;
                    })
                    .Select(o => new InjectedProperty(o)));
                result.AddRange(type.GetMethods(bindingFlags)
                    .Where(o => {
                        if (o.GetCustomAttribute<EcsInjectAttribute>() == null)
                            return false;
#if (DEBUG && !DISABLE_DEBUG) || ENABLE_DRAGONECS_ASSERT_CHEKS
                        if (o.IsGenericMethod)
                            throw new EcsAutoInjectionException($"{o.Name} method is Generic");
                        if (o.GetParameters().Length != 1)
                            throw new EcsAutoInjectionException($"{o.Name} method Arguments != 1");
#endif
                        return true;
                    })
                    .Select(o => new InjectedMethod(o))); 
                if (type.BaseType != null)
                    Do(type.BaseType, result);
            }
            return result;
        }
        public void Inject(Type fieldType, object obj)
        {
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

                    systemRecord.property.Inject(systemRecord.target,
                        _dummyInstance.MakeGenericType(systemRecord.Attribute.notNullDummyType).GetField("intsance", BindingFlags.Static | BindingFlags.Public).GetValue(null));
                }
            }
            WarningMissedInjections();
            _notInjected.Clear();
            _notInjected= null;
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
    public class AutoInjectSystem : IEcsPreInitProcess, IEcsPreInject, IEcsPreInitInjectProcess
    {
        private EcsPipeline _pipeline;
        private List<object> _injectQueue = new List<object>();
        private AutoInjectionMap _autoInjectionMap;
        private bool _isPreInjectionComplete = false;
        public void PreInject(object obj)
        {
            if(_pipeline == null)
            {
                _injectQueue.Add(obj);
                return;
            }
            AutoInject(obj);
        }
        public void PreInit(EcsPipeline pipeline)
        {
            _pipeline = pipeline;
            _autoInjectionMap = new AutoInjectionMap(_pipeline);

            foreach (var obj in _injectQueue)
            {
                AutoInject(obj);
            }
            _injectQueue.Clear();
            _injectQueue = null;
            if (_isPreInjectionComplete)
            {
                _autoInjectionMap.InjectDummy();
            }
        }

        private void AutoInject(object obj)
        {
            _autoInjectionMap.Inject(obj.GetType(), obj);
        }

        public void OnPreInitInjectionBefore() { }
        public void OnPreInitInjectionAfter()
        {
            _isPreInjectionComplete = true;
            if (_autoInjectionMap != null)
            {
                _autoInjectionMap.InjectDummy();
            }
            ClearUsless();
        }

        private void ClearUsless()
        {
            _autoInjectionMap = null;
            GC.Collect(0); //Собрать все хламовые созданне время мини классы
        }
    }

    #region Utils
    internal interface IInjectedProperty
    {
        public bool IsInjected { get; }
        public Type PropertyType { get; }
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