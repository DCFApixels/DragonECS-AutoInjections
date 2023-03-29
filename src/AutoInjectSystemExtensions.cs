namespace DCFApixels.DragonECS
{
    public static class AutoInjectSystemExtensions
    {
        public static EcsSystems.Builder AutoInject(this EcsSystems.Builder self)
        {
            self.Add(new AutoInjectSystem());
            return self;
        }
    }
}
