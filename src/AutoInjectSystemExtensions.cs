namespace DCFApixels.DragonECS
{
    public static class AutoInjectSystemExtensions
    {
        [MetaColor(MetaColor.DragonCyan)]
        public class AutoInjectModule : IEcsModule
        {
            public bool isAgressiveInjection;
            public void Import(EcsPipeline.Builder b)
            {
                b.AddUnique(new AutoInjectSystem(isAgressiveInjection));
            }
        }
        public static EcsPipeline.Builder AutoInject(this EcsPipeline.Builder self, bool isAgressiveInjection = false)
        {
            self.AddUnique(new AutoInjectSystem(isAgressiveInjection));
            return self;
        }
    }
}
