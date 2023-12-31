//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Runtime.CompilerServices;
//
//namespace DCFApixels.DragonECS
//{
//    public static class AutoProcesses
//    {
//        private static Dictionary<string, BuilderHandler> _builders;
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static bool TryGetCustomAttribute<T>(Type self, out T attribute) where T : Attribute
//        {
//            attribute = self.GetCustomAttribute<T>();
//            return attribute != null;
//        }
//        static AutoProcesses()
//        {
//            _builders = new Dictionary<string, BuilderHandler>();
//            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
//            {
//                var types = assembly.GetTypes();
//                foreach (var type in types)
//                {
//                    if (type.IsGenericType)
//                        continue;
//                    if (TryGetCustomAttribute(type, out EcsProcessWrapperBuilderAttribute attr))
//                    {
//                        MethodInfo method = type.GetMethod(attr.wrapperBuilderMethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
//                        BuilderHandler action = (BuilderHandler)Delegate.CreateDelegate(typeof(BuilderHandler), null, method);
//                        _builders.Add(attr.processMethodName, action);
//                    }
//                }
//            }
//        }
//        public static EcsPipeline.Builder AddAuto(this EcsPipeline.Builder self, object system, string layerName = null)
//        {
//            var methods = system.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
//            foreach (var method in methods)
//            {
//                if (_builders.TryGetValue(method.Name, out var builder))
//                {
//                    var process = builder(system, method);
//                    if (process != null)
//                        self.Add(process, layerName);
//                }
//            }
//            if (system is IEcsProcess systemInterface)
//                self.Add(systemInterface, layerName);
//            return self;
//        }
//        private delegate IEcsProcess BuilderHandler(object targete, MethodInfo method);
//    }
//
//    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
//    sealed class EcsProcessWrapperBuilderAttribute : Attribute
//    {
//        public readonly string processMethodName;
//        public readonly string wrapperBuilderMethodName;
//        public EcsProcessWrapperBuilderAttribute(string processMethodName, string wrapperBuilderMethodName = "Builder")
//        {
//            this.processMethodName = processMethodName;
//            this.wrapperBuilderMethodName = wrapperBuilderMethodName;
//        }
//    }
//    public static class EcsAutoProcessUtility
//    {
//        public static TDelegate CreateDelegate<TDelegate>(object system, MethodInfo method) where TDelegate : Delegate
//        {
//            return (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), system, method);
//        }
//    }
//
//    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    internal class EcsProcessWrapperBase
//    {
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static bool CheckParameters(MethodInfo method)
//        {
//            return method.GetParameters().Length <= 0;
//        }
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public static Action CreateAction(object target, MethodInfo method)
//        {
//            return EcsAutoProcessUtility.CreateDelegate<Action>(target, method);
//        }
//    }
//    internal class IEcsProcessWrapper : EcsProcessWrapperBase, IEcsMetaProvider
//    {
//        public object system;
//        public Action a;
//        public object MetaSource => system;
//    }
//    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    [EcsProcessWrapperBuilder(nameof(PreInit), nameof(Builder))]
//    internal class EcsPreInitProcessWrapper : IEcsProcessWrapper, IEcsPreInitProcess
//    {
//        public EcsPreInitProcessWrapper(object target, Action a) { system = target; this.a = a; }
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public void PreInit() => a();
//        public static IEcsProcess Builder(object target, MethodInfo method)
//        {
//            if (target is IEcsPreInitProcess) return null;
//            if (CheckParameters(method))
//                return new EcsPreInitProcessWrapper(target, CreateAction(target, method));
//            return null;
//        }
//    }
//    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    [EcsProcessWrapperBuilder(nameof(Init), nameof(Builder))]
//    internal class EcsInitProcessWrapper : IEcsProcessWrapper, IEcsInitProcess
//    {
//        public EcsInitProcessWrapper(object target, Action a) { system = target; this.a = a; }
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public void Init() => a();
//        public static IEcsProcess Builder(object target, MethodInfo method)
//        {
//            if (target is IEcsInitProcess) return null;
//            if (CheckParameters(method))
//                return new EcsInitProcessWrapper(target, CreateAction(target, method));
//            return null;
//        }
//    }
//    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    [EcsProcessWrapperBuilder(nameof(Run), nameof(Builder))]
//    internal class EcsRunProcessEmptyWrapper : IEcsProcessWrapper, IEcsRunProcess
//    {
//        public EcsRunProcessEmptyWrapper(object target, Action a) { system = target; this.a = a; }
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public void Run() => a();
//        public static IEcsProcess Builder(object target, MethodInfo method)
//        {
//            if (target is IEcsRunProcess) return null;
//            if (CheckParameters(method))
//                return new EcsRunProcessEmptyWrapper(target, CreateAction(target, method));
//            return null;
//        }
//    }
//    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    [EcsProcessWrapperBuilder(nameof(Destroy), nameof(Builder))]
//    internal class EcsDestroyProcessEmptyWrapper : IEcsProcessWrapper, IEcsDestroyProcess
//    {
//        public EcsDestroyProcessEmptyWrapper(object target, Action a) { system = target; this.a = a; }
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public void Destroy() => a();
//        public static IEcsProcess Builder(object target, MethodInfo method)
//        {
//            if (target is IEcsRunProcess) return null;
//            if (CheckParameters(method))
//                return new EcsDestroyProcessEmptyWrapper(target, CreateAction(target, method));
//            return null;
//        }
//    }
//
//    /*
//    public interface ISomeCustomeProcess : IEcsProcess
//    {
//        void DoSomething();
//    }
//    //Только при наличии этого атрибута будет вызван метод Builder который создаст обертку для DoSomething
//    [EcsProcessWrapperBuilder(nameof(DoSomething), nameof(Builder))]
//    internal class SomeCustomeProcessWrapper : ISomeCustomeProcess, IEcsDebugMetaProvider
//    {
//        public object system;
//        public Action action;
//        //IEcsDebugMetaProvider.DebugMetaSource используется чтобы для обертки отображалось данные из debug-атрибутов вроде DebugName 
//        public object DebugMetaSource => system;
//        public SomeCustomeProcessWrapper(object system, Action action) { this.system = system; this.action = action; }
//        public void DoSomething() => action();
//        public static IEcsProcess Builder(object system, MethodInfo method)
//        {
//            //Исключает те системы которые уже имеют интерфейс, иначе в рантайме вызов метода-процесса будет дублироваться
//            if (system is ISomeCustomeProcess) return null; //возвращение null
//            return new SomeCustomeProcessWrapper(system, EcsAutoProcessUtility.CreateDelegate<Action>(system, method));
//        }
//    }
//    */
//}
//