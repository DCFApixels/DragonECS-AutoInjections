<p align="center">
<img width="400" src="https://github.com/DCFApixels/DragonECS-AutoInjections/assets/99481254/f120bb2b-5117-4546-aa13-4734b2608712.png">
</p>

# [DragonECS](https://github.com/DCFApixels/DragonECS) - Auto Injections Extension
Расширение призвано сократить объем кода, упростив инъекцию  зависимостей, делая их автоматически.
> **ВАЖНО!** Проект в стадии разработки. API может меняться.
# Оглавление
* [Установка](#Установка)
   * [Зависимости](#Зависимости)
   * [Unity-модуль](#Unity-модуль)
   * [В виде исходников](#В-виде-иходников)
* [Инъекция зависимостей](#Инъекция-зависимостей)
* [Auto Builder субъектов](#Auto-Builder-субъектов)
* [Пример кода](#Пример-кода)
* [Не null инъекции](#Не-null-инъекции)

# Установка
### Зависимости
Убедитесь что в проекте установлен фреймворк [DragonECS](https://github.com/DCFApixels/DragonECS).
* ### Unity-модуль
Поддерживается установка в виде Unity-модуля в  при помощи добавления git-URL [в PackageManager](https://docs.unity3d.com/2023.2/Documentation/Manual/upm-ui-giturl.html) или ручного добавления в `Packages/manifest.json`: 
```
https://github.com/DCFApixels/DragonECS-AutoInjections.git
```
* ### В виде исходников
Фреймворк так же может быть добавлен в проект в виде исходников. 

# Интеграция
Добавьте вызов метода `AutoInject()` для фабрики Pipeline. Пример:
```csharp
_pipeline = EcsPipeline.New()
    .Inject(world)
    .Inject(_timeService)
    .Add(new TestSystem())
    .Add(new VelocitySystem())
    .Add(new ViewSystem())
    .AutoInject()
    .BuildAndInit();
```
  
# Инъекция зависимостей
Атрибут `[EcsInject]` убирает необходимость использования интерфейса `IEcsInject<T>`, поля помеченные таким атрибутом автоматически подхватят зависимости внедренные в Pipeline. Пример： 
```csharp
[EcsInject] EcsDefaultWorld _world;
```
# Auto Builder субъектов
Так же AutoInjections упрощает построение субъектов. Для начала наследуйте субъект не от `EcsSubject`, а от `EcsSubjectDI`, а далее добавьте специальные атрибуты.

Атрибуты для инициализации полей с пулами: 
* `[Inc]` - кеширует пул и добавит тип компонента в включающее ограничение субъекта, аналог метода `Include`;
* `[Exc]` - кеширует пул и добавит тип компонента в исключающее ограничение субъекта, аналог метода `Exclude`;
* `[Opt]` - только кеширует пул, аналог метода `Optional`;

Дополнительные атрибуты только для задания ограничений субъекта. Их можно применить к самому субъекту, либо к любому полю внутри. Используйте атрибуты: 
* `[IncImplicit(type)]` - добавит в включающее ограничение указанный в конструкторе тип `type`, аналог метода `Include`;
* `[ExcImplicit(type)]` - добавит в исключающее ограничение указанный в конструкторе тип `type`, аналог метода `Exclude`;

# Пример кода
```csharp
class VelocitySystemDI : IEcsRunProcess
{
    class Subject : EcsSubjectDI
    {
        [ExcImplicit(typeof(FreezedTag))]
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
<details>
<summary>Тот же код но без AutoInjections</summary>
    
```csharp
class VelocitySystem : IEcsRunProcess, IEcsInject<EcsDefaultWorld>, IEcsInject<TimeService>
{
    class Subject : EcsSubject
    {
        public EcsPool<Pose> poses;
        public EcsPool<Velocity> velocities;
        public Subject(Builder b)
        {
            b.Exclude<FreezedTag>();
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

</details>
    
# Не null инъекции

Чтобы поле помеченное `[EcsInject]` было проинициализированно даже в случае отстувия инъекции, в конструктор атрибута можно передать тип болванку. В примере ниже поле `foo` получит экземпляр класса `Foo` из инъекции или экземпляр `FooDummy : Foo` если инъекции небыло.
``` csharp
[EcsInject(typeof(FooDummy))] Foo foo;
```
> Для корректной работы переданный тип должен иметь конструктор без парамтров и быть либо того же типа что и поле, либо производного типа.
  
Расширение так же сообщит если по завершению предварительной инъекции, остались не проинициализированные поля с `[EcsInject]`.
