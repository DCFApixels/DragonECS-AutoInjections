# DragonECS-AutoInjections
 Automatic dependency implementations

Данное ресширение призвано скоратить объем кода, упростив инбъекцию зависимостей, делая их автоматическими.

## Инжекция зависимостей
Аттрибут `[EcsInject]` скрывает необходимость использования интерфейса `IEcsInject<T>`, поля помеченные таким атрибутом автоматически подхватят зависимости переданные в метод Inject. У атрибута есть не обязательный аргумент notNullDummyType, если он указан, то во время предварительной инъекции, если  поле небыло проинициализировано, ему будет присвоен экземпляр этого типа.
 
## Фабрикаа субъектов
Так же данное расширение упрощает построение субъектов, добавляя 3 сппециальных аттрибута `[Inc]`, `[Exc]`, `[Opt]`. Данные атрибуты аналогичны вызовам мтодов `Include`, `Exclude`, `Optional` в фабрике субъекта. Так же еще существует 2 атрибута для неявного задания ограничения `[IncImplicit]`, `[ExcImplicit]`, эти атрибуты в обход кеширования пула, задают ограничения длясубъекта.
 
## Пример кода
* ### С использованием AutoInjections
```csharp
class VelocitySystemDI : IEcsRunProcess
{
    class Subject : EcsSubjectDI
    {
        [IncImplicit(typeof(PlayerTag))]
        [Inc] public EcsPool<Pose> poses;
        [Inc] public EcsPool<Velocity> velocities;
    }

    [EcsInject] EcsDefaultWorld _world;
    [EcsInject] TimeService _time;

    public void Run(EcsPipeline pipeline)
    {
        foreach (var e in _world.Where(out Subject s))
        {
            s.poses.Write(e).position += s.velocities.Read(e).value * _time.DeltaTime;
        }
    }
}
```
 * ### Без AutoInjections
```csharp
class VelocitySystem : IEcsRunProcess, IEcsInject<EcsDefaultWorld>, IEcsInject<TimeService>
{
    class Subject : EcsSubject
    {
        public EcsPool<Pose> poses;
        public EcsPool<Velocity> velocities;
        public Subject(Builder b)
        {
            b.Include<PlayerTag>();
            poses = b.Include<Pose>();
            velocities = b.Include<Velocity>();
        }
    }

    EcsDefaultWorld _world;
    TimeService _time;

    public void Inject(EcsDefaultWorld obj) => _world = obj;
    public void Inject(TimeService obj) => _time = obj;

    public void Run(EcsPipeline pipeline)
    {
        foreach (var e in _world.Where(out Subject s))
        {
            s.poses.Write(e).position += s.velocities.Read(e).value * _time.DeltaTime;
        }
    }
}
```
 
