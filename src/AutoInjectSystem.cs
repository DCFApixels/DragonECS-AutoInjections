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

        private Dictionary<Type, List<FiledRecord>> _systems;
        private HashSet<Type> _notInjected;

        private Type dummyInstance = typeof(DummyInstance<>);

        private bool _isDummyInjected = false;

        public AutoInjectionMap(EcsPipeline source)
        {
            _source = source;
            var allsystems = _source.AllSystems;
            _systems = new Dictionary<Type, List<FiledRecord>>();
            _notInjected = new HashSet<Type>();
            foreach (var system in allsystems)
            {
                Type systemType = system.GetType();
                foreach (var field in systemType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    AutoInjectAttribute autoInjectAttribute = field.GetCustomAttribute<AutoInjectAttribute>();
                    if (autoInjectAttribute != null)
                    {
                        Type fieldType = field.FieldType;
                        List<FiledRecord> list;
                        if (!_systems.TryGetValue(fieldType, out list))
                        {
                            list = new List<FiledRecord>();
                            _systems.Add(fieldType, list);
                        }

                        list.Add(new FiledRecord(system, field, autoInjectAttribute));
                    }
                }
            }
            foreach (var item in _systems.Keys)
            {
                _notInjected.Add(item);
            }
        }

        public void Inject(object obj)
        {
            _notInjected.Remove(obj.GetType());
            Type objectType = obj.GetType();
            if(_systems.TryGetValue(objectType, out List<FiledRecord> list))
            {
                foreach (var item in list)
                {
                    item.field.SetValue(item.target, obj);
                }
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
                        dummyInstance.MakeGenericType(systemRecord.attribute.notNullDummyType).GetField("intsance", BindingFlags.Static | BindingFlags.Public).GetValue(null));
                }
            }

            _notInjected.Clear();
            _notInjected= null;
        }

        private readonly struct FiledRecord
        {
            public readonly IEcsSystem target;
            public readonly FieldInfo field;
            public readonly AutoInjectAttribute attribute;
            public FiledRecord(IEcsSystem target, FieldInfo field, AutoInjectAttribute attribute)
            {
                this.target = target;
                this.field = field;
                this.attribute = attribute;
            }
        }
    }

    [DebugHide, DebugColor(DebugColor.Gray)]
    public class AutoInjectSystem : IEcsPreInitSystem, IEcsPreInject, IEcsPreInitInjectCallbacks
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
            _autoInjectionMap.Inject(obj);
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