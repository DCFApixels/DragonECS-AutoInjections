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
                if (_builders.TryGetValue(method.Name, out var builder))
                {
                    var process = builder(system, method);
                    if (process != null)
                        self.Add(process, layerName);
                }
            }
            if (system is IEcsProcess systemInterface)
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
    public static class EcsAutoProcessUtility
    {
        public static TDelegate CreateDelegate<TDelegate>(object system, MethodInfo method) where TDelegate : Delegate
        {
            return (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), system, method);
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
            return EcsAutoProcessUtility.CreateDelegate<Action>(target, method);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<EcsPipeline> CreateAction(object target, MethodInfo method)
        {
            return EcsAutoProcessUtility.CreateDelegate<Action<EcsPipeline>>(target, method);
        }
    }
    internal class IEcsProcessEmptyWrapper : IEcsProcessWrapperBase, IEcsDebugMetaProvider
    {
        public object system;
        public Action a;
        public object DebugMetaSource => system;
    }
    internal class IEcsProcessWrapper : IEcsProcessWrapperBase, IEcsDebugMetaProvider
    {
        public object system;
        public Action<EcsPipeline> a;
        public object DebugMetaSource => system;
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [EcsProcessWrapperBuilder(nameof(PreInit), nameof(Builder))]
    internal class EcsPreInitProcessEmptyWrapper : IEcsProcessEmptyWrapper, IEcsPreInitProcess
    {
        public EcsPreInitProcessEmptyWrapper(object target, Action a) { system = target; this.a = a; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PreInit(EcsPipeline pipeline) => a();
        public static IEcsProcess Builder(object target, MethodInfo method)
        {
            if (target is IEcsPreInitProcess) return null;
            if (CheckParameters(method, out bool isHasParam))
                if (isHasParam) 
                    return new EcsPreInitProcessWrapper(target, CreateAction(target, method));
                else 
                    return new EcsPreInitProcessEmptyWrapper(target, CreateEmptyAction(target, method));
            return null;
        }
    }
    internal class EcsPreInitProcessWrapper : IEcsProcessWrapper, IEcsPreInitProcess
    {
        public EcsPreInitProcessWrapper(object target, Action<EcsPipeline> a) { system = target; this.a = a; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PreInit(EcsPipeline pipeline) => a(pipeline);
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [EcsProcessWrapperBuilder(nameof(Init), nameof(Builder))]
    internal class EcsInitProcessEmptyWrapper : IEcsProcessEmptyWrapper, IEcsInitProcess
    {
        public EcsInitProcessEmptyWrapper(object target, Action a) { system = target; this.a = a; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(EcsPipeline pipeline) => a();
        public static IEcsProcess Builder(object target, MethodInfo method)
        {
            if (target is IEcsInitProcess) return null;
            if (CheckParameters(method, out bool isHasParam))
                if (isHasParam) 
                    return new EcsInitProcessWrapper(target, CreateAction(target, method));
                else 
                    return new EcsInitProcessEmptyWrapper(target, CreateEmptyAction(target, method));
            return null;
        }
    }
    internal class EcsInitProcessWrapper : IEcsProcessWrapper, IEcsInitProcess
    {
        public EcsInitProcessWrapper(object target, Action<EcsPipeline> a) { system = target; this.a = a; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(EcsPipeline pipeline) => a(pipeline);
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [EcsProcessWrapperBuilder(nameof(Run), nameof(Builder))]
    internal class EcsRunProcessEmptyWrapper : IEcsProcessEmptyWrapper, IEcsRunProcess
    {
        public EcsRunProcessEmptyWrapper(object target, Action a) { system = target; this.a = a; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run(EcsPipeline pipeline) => a();
        public static IEcsProcess Builder(object target, MethodInfo method)
        {
            if (target is IEcsRunProcess) return null;
            if (CheckParameters(method, out bool isHasParam))
                if (isHasParam) 
                    return new EcsRunProcessWrapper(target, CreateAction(target, method));
                else 
                    return new EcsRunProcessEmptyWrapper(target, CreateEmptyAction(target, method));
            return null;
        }
    }
    internal class EcsRunProcessWrapper : IEcsProcessWrapper, IEcsRunProcess
    {
        public EcsRunProcessWrapper(object target, Action<EcsPipeline> a) { system = target; this.a = a; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run(EcsPipeline pipeline) => a(pipeline);
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [EcsProcessWrapperBuilder(nameof(Destroy), nameof(Builder))]
    internal class EcsDestroyProcessEmptyWrapper : IEcsProcessEmptyWrapper, IEcsDestroyProcess
    {
        public EcsDestroyProcessEmptyWrapper(object target, Action a) { system = target; this.a = a; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Destroy(EcsPipeline pipeline) => a();
        public static IEcsProcess Builder(object target, MethodInfo method)
        {
            if (target is IEcsDestroyProcess) return null;
            if (CheckParameters(method, out bool isHasParam))
                if (isHasParam) 
                    return new EcsDestroyProcessWrapper(target, CreateAction(target, method));
                else 
                    return new EcsDestroyProcessEmptyWrapper(target, CreateEmptyAction(target, method));
            return null;
        }
    }
    internal class EcsDestroyProcessWrapper : IEcsProcessWrapper, IEcsDestroyProcess
    {
        public EcsDestroyProcessWrapper(object target, Action<EcsPipeline> a) { system = target; this.a = a; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Destroy(EcsPipeline pipeline) => a(pipeline);
    }





    public interface ISomeCustomeProcess : IEcsProcess
    {
        void DoSomething();
    }
    //Только при наличии этого атрибута будет вызван метод Builder который создаст обертку для DoSomething
    [EcsProcessWrapperBuilder(nameof(DoSomething), nameof(Builder))]
    internal class SomeCustomeProcessWrapper : ISomeCustomeProcess, IEcsDebugMetaProvider
    {
        public object system;
        public Action action;
        //IEcsDebugMetaProvider.DebugMetaSource используется чтобы для обертки отображалось данные из debug-атрибутов вроде DebugName 
        public object DebugMetaSource => system;
        public SomeCustomeProcessWrapper(object system, Action action) { this.system = system; this.action = action; }
        public void DoSomething() => action();
        public static IEcsProcess Builder(object system, MethodInfo method)
        {
            //Исключает те системы которые уже имеют интерфейс, иначе в рантайме вызов метода-процесса будет дублироваться
            if (system is ISomeCustomeProcess) return null; //возвращение null
            return new SomeCustomeProcessWrapper(system, EcsAutoProcessUtility.CreateDelegate<Action>(system, method));
        }
    }
}
