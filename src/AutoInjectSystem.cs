using System;
using System.Collections.Generic;
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
        private Dictionary<Type, List<FieldRecord>> _systems;
        private HashSet<Type> _notInjected;
        private Type _dummyInstance = typeof(DummyInstance<>);
        private bool _isDummyInjected = false;

        public AutoInjectionMap(EcsPipeline source)
        {
            _source = source;
            var allsystems = _source.AllSystems;
            _systems = new Dictionary<Type, List<FieldRecord>>();
            _notInjected = new HashSet<Type>();
            foreach (var system in allsystems)
            {
                Type systemType = system.GetType();
                foreach (var field in GetAllFieldsFor(systemType))
                {
                    EcsInjectAttribute autoInjectAttribute = field.GetCustomAttribute<EcsInjectAttribute>();
                    if (autoInjectAttribute != null)
                    {
                        Type fieldType = field.FieldType;
                        List<FieldRecord> list;
                        if (!_systems.TryGetValue(fieldType, out list))
                        {
                            list = new List<FieldRecord>();
                            _systems.Add(fieldType, list);
                        }

                        list.Add(new FieldRecord(system, field, autoInjectAttribute));
                    }
                }
            }
            foreach (var item in _systems.Keys)
                _notInjected.Add(item);
        }
        private static List<FieldInfo> GetAllFieldsFor(Type type)
        {
            List<FieldInfo> result = new List<FieldInfo>();
            Do(type, result);
            static void Do(Type type, List<FieldInfo> result)
            {
                result.AddRange(type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
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

            if (_systems.TryGetValue(fieldType, out List<FieldRecord> list))
            {
                foreach (var item in list)
                    item.field.SetValue(item.target, obj);
            }
        }

        public void InjectDummy()
        {
            if (_isDummyInjected)
                return;
            _isDummyInjected = true;
            foreach (var notInjectedItem in _notInjected)
            {
                foreach (var systemRecord in _systems[notInjectedItem])
                {
                    if (systemRecord.attribute.notNullDummyType == null)
                        continue;
                    if (systemRecord.field.GetValue(systemRecord.target) != null)
                        continue;
                    if (systemRecord.field.FieldType.IsAssignableFrom(systemRecord.attribute.notNullDummyType) == false)
                    {
                        EcsDebug.Print(EcsConsts.DEBUG_ERROR_TAG, $"The {systemRecord.attribute.notNullDummyType} dummy cannot be assigned to the {systemRecord.field.FieldType.Name} field");
                        continue;
                    }

                    systemRecord.field.SetValue(systemRecord.target,
                        _dummyInstance.MakeGenericType(systemRecord.attribute.notNullDummyType).GetField("intsance", BindingFlags.Static | BindingFlags.Public).GetValue(null));
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
                foreach (var systemRecord in _systems[item])
                {
                    EcsDebug.PrintWarning($"in system {EcsDebugUtility.GetGenericTypeFullName(systemRecord.target.GetType(), 1)} is missing an injection of {EcsDebugUtility.GetGenericTypeFullName(item, 1)}.");
                }
            }
#endif
        }

        private readonly struct FieldRecord
        {
            public readonly IEcsSystem target;
            public readonly FieldInfo field;
            public readonly EcsInjectAttribute attribute;
            public FieldRecord(IEcsSystem target, FieldInfo field, EcsInjectAttribute attribute)
            {
                this.target = target;
                this.field = field;
                this.attribute = attribute;
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
        }
    }
}