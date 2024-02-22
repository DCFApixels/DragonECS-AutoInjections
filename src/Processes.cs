namespace DCFApixels.DragonECS
{
    public interface IInjectRaw : IEcsSystem
    {
        void Inject(object obj);
    }
}
