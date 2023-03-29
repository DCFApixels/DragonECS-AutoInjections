namespace DCFApixels.DragonECS
{
    public static class AutoInjectSystemExtensions
    {
        public static EcsPipeline.Builder AutoInject(this EcsPipeline.Builder self)
        {
            self.Add(new AutoInjectSystem());
            return self;
        }
    }
}
