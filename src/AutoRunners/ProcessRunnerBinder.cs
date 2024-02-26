using DCFApixels.DragonECS.AutoInjections.Internal;
using System;
using System.Reflection;

namespace DCFApixels.DragonECS
{
    public static class ProcessRunnerBinder
    {
        private static MethodInfo _declareRunnerMethod = typeof(EcsPipeline).GetMethod(nameof(EcsPipeline.GetRunnerInstance));
        public static T GetRunnerAuto<T>(this EcsPipeline self) where T : IEcsProcess
        {
            if(self.TryGetRunner(out T process))
            {
                return process;
            }
            Type type = typeof(T);
            if (type.TryGetCustomAttribute(out BindWithRunnerAttribute atr))
            {
                Type runnerType = atr.runnerType;
                if (type.IsGenericType)
                {
                    if(runnerType.IsGenericType == false || 
                        runnerType.IsGenericTypeDefinition == false)
                    {
                        Throw.UndefinedException();
                    }

                    Type[] genericArguments = type.GetGenericArguments();
                    runnerType = runnerType.MakeGenericType(genericArguments);
                }
                return (T)_declareRunnerMethod.MakeGenericMethod(runnerType).Invoke(self, null);
            }
            Throw.UndefinedException();
            return default;
        }
    }
}
