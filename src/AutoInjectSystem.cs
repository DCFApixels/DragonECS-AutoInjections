using System;
using System.Collections.Generic;
using System.Reflection;

namespace DCFApixels.DragonECS
{
    internal class AutoInjectionMap
    {
        private readonly EcsSystems _source;

        private Dictionary<Type, List<FiledRecord>> _systems;

        public AutoInjectionMap(EcsSystems source)
        {
            _source = source;

            var allsystems = _source.AllSystems;

            _systems = new Dictionary<Type, List<FiledRecord>>();
            foreach (var system in allsystems)
            {
                Type systemType = system.GetType();
                foreach (var field in systemType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if(field.GetCustomAttribute<AutoInjectAttribute>() != null)
                    {

                        Type fieldType = field.FieldType;
                        List<FiledRecord> list;
                        if (!_systems.TryGetValue(fieldType, out list))
                        {
                            list = new List<FiledRecord>();
                            _systems.Add(fieldType, list);
                        }

                        list.Add(new FiledRecord(system, field));


                    }
                }
            }
        }

        public void Inject(object obj)
        {
            Type objectType = obj.GetType();
            if(_systems.TryGetValue(objectType, out List<FiledRecord> list))
            {
                foreach (var item in list)
                {
                    item.field.SetValue(item.target, obj);
                }
            }
        }

        private readonly struct FiledRecord
        {
            public readonly IEcsSystem target;
            public readonly FieldInfo field;
            public FiledRecord(IEcsSystem target, FieldInfo field)
            {
                this.target = target;
                this.field = field;
            }
        }
    }

    [DebugHide, DebugColor(DebugColor.Gray)]
    public class AutoInjectSystem : IEcsPreInitSystem, IEcsPreInject
    {
        private EcsSystems _systems;
        private List<object> _injectQueue = new List<object>();

        private AutoInjectionMap _autoInjectionMap;

        public void PreInject(object obj)
        {
            if(_systems == null)
            {
                _injectQueue.Add(obj);
                return;
            }
            AutoInject(obj);
        }
        public void PreInit(EcsSystems systems)
        {
            _systems = systems;
            _autoInjectionMap = new AutoInjectionMap(_systems);

            foreach (var obj in _injectQueue)
            {
                AutoInject(obj);
            }
            _injectQueue.Clear();
            _injectQueue = null;
        }

        private void AutoInject(object obj)
        {
            _autoInjectionMap.Inject(obj);
        }
    }
}