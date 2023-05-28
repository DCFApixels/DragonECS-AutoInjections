<p align="center">
<img width="400" src="https://github.com/DCFApixels/DragonECS-AutoInjections/assets/99481254/f120bb2b-5117-4546-aa13-4734b2608712.png">
</p>

# [DragonECS](https://github.com/DCFApixels/DragonECS) - Auto Injections Extension

Данное ресширение призвано скоратить объем кода, упростив инбъекцию зависимостей, делая их автоматическими.

## Инжекция зависимостей
Аттрибут `[EcsInject]` скрывает необходимость использования интерфейса `IEcsInject<T>`, поля помеченные таким атрибутом автоматически подхватят зависимости переданные в метод Inject. 

## Фабрикаа субъектов
Так же данное расширение упрощает построение субъектов, добавляя 3 сппециальных аттрибута `[Inc]`, `[Exc]`, `[Opt]`. Данные атрибуты аналогичны вызовам мтодов `Include`, `Exclude`, `Optional` в фабрике субъекта. Так же еще существует 2 атрибута для неявного задания ограничения `[IncImplicit]`, `[ExcImplicit]`, эти атрибуты в обход кеширования пула, задают ограничения длясубъекта.
Атрибуты
* `[Inc]` -
* `[Exc]` -
* `[Opt]` -


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
## Не null инъекции

Чтобы поле помеченное `[EcsInject]` было проинициализированно даже в случае отстувия инъекции, в конструктор атрибута можно передать тип болванку. В примере ниже поле `foo` получит экземпляр класса `Foo` из инъекции или экземпляр `FooDummy` если инъекции небыло.
``` csharp
[EcsInject(typeof(FooDummy))] Foo foo;
```
Расширение так же сообщит если по заврешению предварительной инъекции, остались непроинициализированные поля с `[EcsInject]`.
