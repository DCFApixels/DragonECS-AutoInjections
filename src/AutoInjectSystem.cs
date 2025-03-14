using DCFApixels.DragonECS.AutoInjections;
using DCFApixels.DragonECS.AutoInjections.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DCFApixels.DragonECS
{
    internal class AutoInjectionMap
    {
        private readonly EcsPipeline _source;
        private Dictionary<Type, List<InjectedPropertyRecord>> _injectedTypeToPropertiesMap = new Dictionary<Type, List<InjectedPropertyRecord>>();
        private HashSet<Type> _notInjected = new HashSet<Type>();
        private bool _isDummyInjected = false;

        private bool _isPreInitInjectionComplete = false;

        public AutoInjectionMap(EcsPipeline source, bool isAgressiveInjection = false)
        {
            _source = source;
            var allsystems = _source.AllSystems;
            foreach (var system in allsystems)
            {
                Type systemType = system.GetType();
                if (systemType == typeof(AutoInjectSystem)) { continue; }

                foreach (var property in GetAllPropertiesFor(systemType, isAgressiveInjection))
                {
                    Type propertType = property.PropertyType;
                    List<InjectedPropertyRecord> list;
                    if (_injectedTypeToPropertiesMap.TryGetValue(propertType, out list) == false)
                    {
                        list = new List<InjectedPropertyRecord>();
                        _injectedTypeToPropertiesMap.Add(propertType, list);
                    }
                    list.Add(new InjectedPropertyRecord(system, property));
                    if (property.GetAutoInjectAttribute() != DIAttribute.Dummy)
                    {
                        _notInjected.Add(propertType);
                    }
                }
            }
        }
        private static List<IInjectedProperty> GetAllPropertiesFor(Type type, bool isAgressiveInjection)
        {
            List<IInjectedProperty> result = new List<IInjectedProperty>();
            GetAllPropertiesFor(type, isAgressiveInjection, result);
            return result;
        }
        private static void GetAllPropertiesFor(Type type, bool isAgressiveInjection, List<IInjectedProperty> result)
        {
            const BindingFlags REFL_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            result.AddRange(type.GetFields(REFL_FLAGS)
                .Where(o => isAgressiveInjection || o.HasAttribute<DIAttribute>())
                .Select(o => new InjectedField(o)));

            result.AddRange(type.GetProperties(REFL_FLAGS)
                .Where(o =>
                {
                    if (!isAgressiveInjection && o.HasAttribute<DIAttribute>() == false)
                    {
                        return false;
                    }
#if (DEBUG && !DISABLE_DEBUG) || ENABLE_DRAGONECS_ASSERT_CHEKS
                    if (!isAgressiveInjection && o.CanWrite == false) { Throw.PropertyIsCantWrite(o); }
#endif
                    return o.CanWrite == false;
                })
                .Select(o => new InjectedProperty(o)));

            result.AddRange(type.GetMethods(REFL_FLAGS)
                .Where(o =>
                {
                    if (!isAgressiveInjection && o.HasAttribute<DIAttribute>() == false)
                    {
                        return false;
                    }
                    var parameters = o.GetParameters();
#if (DEBUG && !DISABLE_DEBUG) || ENABLE_DRAGONECS_ASSERT_CHEKS
                    if (!isAgressiveInjection)
                    {
                        if (o.IsGenericMethod) { Throw.MethodIsGeneric(o); }
                        if (parameters.Length != 1) { Throw.MethodArgumentsGreater1(o); }
                    }
#endif
                    return o.IsGenericMethod == false && parameters.Length == 1;
                })
                .Select(o => new InjectedMethod(o)));

            if (type.BaseType != null)
            {
                GetAllPropertiesFor(type.BaseType, isAgressiveInjection, result);
            }
        }

        private Type[] _relatedTypesBuffer;
        public void Inject(Type fieldType, object obj)
        {
            if (_isPreInitInjectionComplete == false)
            {
                _notInjected.Remove(fieldType);
            }
            if (_relatedTypesBuffer == null || _relatedTypesBuffer.Length < _injectedTypeToPropertiesMap.Count)
            {
                _relatedTypesBuffer = new Type[_injectedTypeToPropertiesMap.Count];
            }
            int relatedTypesCount = 0;
            foreach (var pair in _injectedTypeToPropertiesMap)
            {
                if (pair.Key == fieldType || pair.Key.IsAssignableFrom(fieldType))
                {
                    _relatedTypesBuffer[relatedTypesCount++] = pair.Key;
                }
            }

            foreach (var type in new ReadOnlySpan<Type>(_relatedTypesBuffer, 0, relatedTypesCount))
            {
                if (_injectedTypeToPropertiesMap.TryGetValue(type, out List<InjectedPropertyRecord> list))
                {
                    string name = string.Empty;
                    if (obj is INamedMember named)
                    {
                        name = named.Name;
                    }
                    foreach (var item in list)
                    {
                        string propertyName = item.Attribute.NamedInjection;
                        if (string.IsNullOrEmpty(propertyName) || propertyName == name)
                        {
                            item.property.Inject(item.target, obj);
                        }
                    }
                }
            }

        }

        public void InjectDummy()
        {
            if (_isDummyInjected) { return; }

            _isDummyInjected = true;
            foreach (var notInjectedItem in _notInjected)
            {
                foreach (var systemRecord in _injectedTypeToPropertiesMap[notInjectedItem])
                {
                    if (systemRecord.Attribute.NotNullDummyType == null)
                        continue;
                    if (systemRecord.property.IsInjected)
                        continue;
                    if (systemRecord.property.PropertyType.IsAssignableFrom(systemRecord.Attribute.NotNullDummyType) == false)
                    {
                        EcsDebug.Print(EcsConsts.DEBUG_ERROR_TAG, $"The {systemRecord.Attribute.NotNullDummyType} dummy cannot be assigned to the {systemRecord.property.PropertyType.Name} field");
                        continue;
                    }
                    systemRecord.property.Inject(systemRecord.target, DummyInstance.GetInstance(systemRecord.Attribute.NotNullDummyType));
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
                foreach (var systemRecord in _injectedTypeToPropertiesMap[item])
                {
                    EcsDebug.PrintWarning($"in system {EcsDebugUtility.GetGenericTypeFullName(systemRecord.target.GetType(), 1)} is missing an injection of {EcsDebugUtility.GetGenericTypeFullName(item, 1)}.");
                }
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
            public DIAttribute Attribute { get { return property.GetAutoInjectAttribute(); } }
            public InjectedPropertyRecord(IEcsProcess target, IInjectedProperty property)
            {
                this.target = target;
                this.property = property;
            }
        }
    }

    [MetaTags(MetaTags.HIDDEN)]
    [MetaColor(MetaColor.Gray)]
    [MetaGroup(EcsAutoInjectionsConsts.PACK_GROUP, EcsConsts.DI_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, "The system responsible for the processing of automatic injections. The .AutoInject() method adds an AutoInjectSystem to the systems pipelines.")]
    public class AutoInjectSystem : IEcsInject<object>, IEcsPipelineMember, IOnInitInjectionComplete, IEcsDefaultAddParams
    {
        public AddParams AddParams => new AddParams(layerName: EcsConsts.PRE_BEGIN_LAYER, isUnique: true);

        private EcsPipeline _pipeline;
        private List<object> _delayedInjects = new List<object>();
        private AutoInjectionMap _autoInjectionMap;
        private bool _isInitInjectionCompleted;
        private bool _isAgressiveInjection;

        static AutoInjectSystem()
        {
            EcsAspect.OnInit -= EcsAspect_OnInit;
            EcsAspect.OnInit += EcsAspect_OnInit;
            EcsAspect.OnAfterInit -= EcsAspect_OnBuild;
            EcsAspect.OnAfterInit += EcsAspect_OnBuild;
        }

        private static void EcsAspect_OnInit(object aspect, EcsAspect.Builder builder)
        {
            EcsAspectAutoHelper.FillFields(aspect, builder);
        }
        private static void EcsAspect_OnBuild(object aspect, EcsMask mask)
        {
            EcsAspectAutoHelper.FillMaskFields(aspect, mask);
        }

        public AutoInjectSystem(bool isAgressiveInjection = false)
        {
            _isAgressiveInjection = isAgressiveInjection;
        }

        public void Inject(object obj)
        {
            if (_isInitInjectionCompleted)
            {
                _autoInjectionMap.Inject(obj.GetType(), obj);
            }
            else
            {
                _delayedInjects.Add(obj);
            }
        }

        public void OnBeforeInitInjection() { }
        public void OnInitInjectionComplete()
        {
            _autoInjectionMap = new AutoInjectionMap(_pipeline, _isAgressiveInjection);
            _isInitInjectionCompleted = true;

            foreach (var obj in _delayedInjects)
            {
                _autoInjectionMap.Inject(obj.GetType(), obj);
            }
            _autoInjectionMap.InjectDummy();
            _autoInjectionMap.OnPreInitInjectionComplete();

            _delayedInjects.Clear();
            _delayedInjects = null;
            GC.Collect(0);
        }

        EcsPipeline IEcsPipelineMember.Pipeline { get => _pipeline; set => _pipeline = value; }
    }

    #region Utils
    internal interface IInjectedProperty
    {
        bool IsInjected { get; }
        Type PropertyType { get; }
        DIAttribute GetAutoInjectAttribute();
        void Inject(object target, object value);
    }
    internal class InjectedField : IInjectedProperty
    {
        private FieldInfo _member;
        private DIAttribute _injectAttribute;
        public bool IsInjected { get; private set; }
        public Type PropertyType { get { return _member.FieldType; } }
        public InjectedField(FieldInfo member)
        {
            _member = member;
            _injectAttribute = member.GetCustomAttribute<DIAttribute>();
            if (_injectAttribute == null) { _injectAttribute = DIAttribute.Dummy; }
        }
        public DIAttribute GetAutoInjectAttribute() { return _injectAttribute; }
        public void Inject(object target, object value)
        {
            _member.SetValue(target, value);
            IsInjected = true;
        }
    }
    internal class InjectedProperty : IInjectedProperty
    {
        private PropertyInfo _member;
        private DIAttribute _injectAttribute;
        public bool IsInjected { get; private set; }
        public Type PropertyType { get { return _member.PropertyType; } }
        public InjectedProperty(PropertyInfo member)
        {
            _member = member;
            _injectAttribute = member.GetCustomAttribute<DIAttribute>();
            if (_injectAttribute == null) { _injectAttribute = DIAttribute.Dummy; }
        }
        public DIAttribute GetAutoInjectAttribute() { return _injectAttribute; }
        public void Inject(object target, object value)
        {
            _member.SetValue(target, value);
            IsInjected = true;
        }
    }
    internal class InjectedMethod : IInjectedProperty
    {
        private MethodInfo _member;
        private DIAttribute _injectAttribute;
        private Type propertyType;
        public bool IsInjected { get; private set; }
        public Type PropertyType { get { return propertyType; } }
        public InjectedMethod(MethodInfo member)
        {
            _member = member;
            _injectAttribute = member.GetCustomAttribute<DIAttribute>();
            propertyType = _member.GetParameters()[0].ParameterType;
            if (_injectAttribute == null) { _injectAttribute = DIAttribute.Dummy; }
        }
        public DIAttribute GetAutoInjectAttribute() { return _injectAttribute; }
        public void Inject(object target, object value)
        {
            _member.Invoke(target, new object[] { value });
            IsInjected = true;
        }
    }
    #endregion
}