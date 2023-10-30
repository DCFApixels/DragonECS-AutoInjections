using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DCFApixels.DragonECS
{
    public static class AutoProcesses
    {
        private static Dictionary<string, BuilderHandler> _builders;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryGetCustomAttribute<T>(Type self, out T attribute) where T : Attribute
        {
            attribute = self.GetCustomAttribute<T>();
            return attribute != null;
        }
        static AutoProcesses()
        {
            _builders = new Dictionary<string, BuilderHandler>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsGenericType)
                        continue;
                    if (TryGetCustomAttribute(type, out EcsProcessWrapperBuilderAttribute attr))
                    {
                        MethodInfo method = type.GetMethod(attr.wrapperBuilderMethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        BuilderHandler action = (BuilderHandler)Delegate.CreateDelegate(typeof(BuilderHandler), null, method);
                        _builders.Add(attr.processMethodName, action);
                    }
                }
            }
        }
        public static EcsPipeline.Builder AddAuto(this EcsPipeline.Builder self, object system, string layerName = null)
        {
            var methods = system.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                if(_builders.TryGetValue(method.Name, out var builder))
                {
                    var process = builder(system, method);
                    if(process != null)
                        self.Add(process, layerName);
                }
            }
            if(system is IEcsProcess systemInterface)
                self.Add(systemInterface, layerName);
            return self;
        }
        private delegate IEcsProcess BuilderHandler(object targete, MethodInfo method);
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    sealed class EcsProcessWrapperBuilderAttribute : Attribute
    {
        public readonly string processMethodName;
        public readonly string wrapperBuilderMethodName;
        public EcsProcessWrapperBuilderAttribute(string processMethodName, string wrapperBuilderMethodName = "Builder")
        {
            this.processMethodName = processMethodName;
            this.wrapperBuilderMethodName = wrapperBuilderMethodName;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    internal class IEcsProcessWrapperBase
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckParameters(MethodInfo method, out bool isHasParam)
        {
            var parametres = method.GetParameters();
            isHasParam = false;
            if (parametres.Length != 0 && (parametres[0].ParameterType != typeof(EcsPipeline)))
                return false;
            isHasParam = parametres.Length > 0;
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action CreateEmptyAction(object target, MethodInfo method)
        {
            return (Action)Delegate.CreateDelegate(typeof(Action), target, method);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<EcsPipeline> CreateAction(object target, MethodInfo method)
        {
            return (Action<EcsPipeline>)Delegate.CreateDelegate(typeof(Action<EcsPipeline>), target, method);
        }
    }
    internal class IEcsProcessEmptyWrapper : IEcsProcessWrapperBase, IEcsDebugName
    {
        public object system;
        public Action a;
        public string DebugName => EcsDebugUtility.GetNameForObject(system);
    }
    internal class IEcsProcessWrapper : IEcsProcessWrapperBase, IEcsDebugName
    {
        public object system;
        public Action<EcsPipeline> a;
        public string DebugName => EcsDebugUtility.GetNameForObject(system);
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [EcsProcessWrapperBuilder(nameof(PreInit), nameof(Builder))]
    internal class IEcsPreInitProcessEmptyWrapper : IEcsProcessEmptyWrapper, IEcsPreInitProcess
    {
        public IEcsPreInitProcessEmptyWrapper(object target, Action a) { system = target; this.a = a; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        public void PreInit(EcsPipeline pipeline) => a();
        public static IEcsProcess Builder(object target, MethodInfo method)
        {
            if (target is IEcsPreInitProcess) return null;
            if (CheckParameters(method, out bool isHasParam))
                if (isHasParam) new IEcsPreInitProcessWrapper(target, CreateAction(target, method));
                else new IEcsPreInitProcessEmptyWrapper(target, CreateEmptyAction(target, method));
            return null;
        }
    }
    internal class IEcsPreInitProcessWrapper : IEcsProcessWrapper, IEcsPreInitProcess
    {
        public IEcsPreInitProcessWrapper(object target, Action<EcsPipeline> a) { system = target; this.a = a; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        public void PreInit(EcsPipeline pipeline) => a(pipeline);
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [EcsProcessWrapperBuilder(nameof(Init), nameof(Builder))]
    internal class IEcsInitProcessEmptyWrapper : IEcsProcessEmptyWrapper, IEcsInitProcess
    {
        public IEcsInitProcessEmptyWrapper(object target, Action a) { system = target; this.a = a; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        public void Init(EcsPipeline pipeline) => a();
        public static IEcsProcess Builder(object target, MethodInfo method)
        {
            if (target is IEcsInitProcess) return null;
            if (CheckParameters(method, out bool isHasParam))
                if (isHasParam) new IEcsInitProcessWrapper(target, CreateAction(target, method));
                else new IEcsInitProcessEmptyWrapper(target, CreateEmptyAction(target, method));
            return null;
        }
    }
    internal class IEcsInitProcessWrapper: IEcsProcessWrapper, IEcsInitProcess
    {
        public IEcsInitProcessWrapper(object target, Action<EcsPipeline> a) { system = target; this.a = a; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        public void Init(EcsPipeline pipeline) => a(pipeline);
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [EcsProcessWrapperBuilder(nameof(Run), nameof(Builder))]
    internal class IEcsRunProcessEmptyWrapper : IEcsProcessEmptyWrapper, IEcsRunProcess
    {
        public IEcsRunProcessEmptyWrapper(object target, Action a) { system = target; this.a = a; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        public void Run(EcsPipeline pipeline) => a();
        public static IEcsProcess Builder(object target, MethodInfo method)
        {
            if (target is IEcsRunProcess) return null;
            if (CheckParameters(method, out bool isHasParam))
                if (isHasParam) new IEcsRunProcessWrapper(target, CreateAction(target, method));
                else new IEcsRunProcessEmptyWrapper(target, CreateEmptyAction(target, method));
            return null;
        }
    }
    internal class IEcsRunProcessWrapper : IEcsProcessWrapper, IEcsRunProcess
    {
        public IEcsRunProcessWrapper(object target, Action<EcsPipeline> a) { system = target; this.a = a; } 
        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        public void Run(EcsPipeline pipeline) => a(pipeline);
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [EcsProcessWrapperBuilder(nameof(Destroy), nameof(Builder))]
    internal class IEcsDestroyProcessEmptyWrapper : IEcsProcessEmptyWrapper, IEcsDestroyProcess
    {
        public IEcsDestroyProcessEmptyWrapper(object target, Action a) { system = target; this.a = a; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        public void Destroy(EcsPipeline pipeline) => a();
        public static IEcsProcess Builder(object target, MethodInfo method)
        {
            if (target is IEcsDestroyProcess) return null;
            if (CheckParameters(method, out bool isHasParam))
                if (isHasParam) new IEcsDestroyProcessWrapper(target, CreateAction(target, method));
                else new IEcsDestroyProcessEmptyWrapper(target, CreateEmptyAction(target, method));
            return null;
        }
    }
    internal class IEcsDestroyProcessWrapper : IEcsProcessWrapper, IEcsDestroyProcess
    {
        public IEcsDestroyProcessWrapper(object target, Action<EcsPipeline> a) { system = target; this.a = a; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Destroy(EcsPipeline pipeline) => a(pipeline);
    }
}
