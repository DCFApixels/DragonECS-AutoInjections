namespace DCFApixels.DragonECS
{
    public static class AutoInjectSystemExtensions
    {
        public static EcsPipeline.Builder AutoInject(this EcsPipeline.Builder self, bool isAgressiveInjection = false)
        {
            self.Add(new AutoInjectSystem(isAgressiveInjection));
            return self;
        }
    }
}
