<p align="center">
<img width="400" src="https://github.com/DCFApixels/DragonECS-AutoInjections/assets/99481254/11868b2e-21f7-4f47-8970-03ad6329cf0e">
</p>

<p align="center">
<img alt="Version" src="https://img.shields.io/github/package-json/v/DCFApixels/DragonECS-AutoInjections?color=%23ff4e85&style=for-the-badge">
<img alt="License" src="https://img.shields.io/github/license/DCFApixels/DragonECS-AutoInjections?color=ff4e85&style=for-the-badge">
<a href="https://discord.gg/kqmJjExuCf"><img alt="Discord" src="https://img.shields.io/badge/Discord-JOIN-00b269?logo=discord&logoColor=%23ffffff&style=for-the-badge"></a>
<a href="http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=IbDcH43vhfArb30luGMP1TMXB3GCHzxm&authKey=s%2FJfqvv46PswFq68irnGhkLrMR6y9tf%2FUn2mogYizSOGiS%2BmB%2B8Ar9I%2Fnr%2Bs4oS%2B&noverify=0&group_code=949562781"><img alt="QQ" src="https://img.shields.io/badge/QQ-JOIN-00b269?logo=tencentqq&logoColor=%23ffffff&style=for-the-badge"></a>
</p>

# Auto Injections for [DragonECS](https://github.com/DCFApixels/DragonECS)

| Languages: | [Русский](https://github.com/DCFApixels/DragonECS-AutoInjections/blob/main/README-RU.md) | [English(WIP)](https://github.com/DCFApixels/DragonECS-AutoInjections) |
| :--- | :--- | :--- |

Расширение призвано сократить объем кода, упростив инъекцию  зависимостей, делая их автоматически.
> **ВАЖНО!** Проект в стадии разработки. API может меняться.
# Оглавление
- [Установка](#установка)
- [Интеграция](#интеграция)
- [Инъекция зависимостей](#инъекция-зависимостей)
- [Auto Builder аспектов](#auto-builder-аспектов)
- [Auto Runner-ы](#auto-runner-ы)
- [Пример кода](#пример-кода)
- [Не null инъекции](#не-null-инъекции)

</br>

# Установка
Семантика версионирования - [Открыть](https://gist.github.com/DCFApixels/e53281d4628b19fe5278f3e77a7da9e8#file-dcfapixels_versioning_ru-md)
## Окружение
Обязательные требования:
+ Зависимость: [DragonECS](https://github.com/DCFApixels/DragonECS)
+ Минимальная версия C# 7.3;

Опционально:
+ Игровые движки с C#: Unity, Godot, MonoGame и т.д.

Протестированно:
+ **Unity:** Минимальная версия 2020.1.0;

## Установка для Unity
* ### Unity-модуль
Поддерживается установка в виде Unity-модуля в  при помощи добавления git-URL [в PackageManager](https://docs.unity3d.com/2023.2/Documentation/Manual/upm-ui-giturl.html) или ручного добавления в `Packages/manifest.json`: 
```
https://github.com/DCFApixels/DragonECS-AutoInjections.git
```
* ### В виде иходников
Фреймворк так же может быть добавлен в проект в виде исходников.

</br>

# Интеграция
Добавьте вызов метода `AutoInject()` для Builder-а пайплайна. Пример:
```csharp
_pipeline = EcsPipeline.New()
    .Inject(world)
    .Inject(_timeService)
    .Add(new TestSystem())
    .Add(new VelocitySystem())
    .Add(new ViewSystem())
    .AutoInject() // Готово, автоматические внедрения активированы
    .BuildAndInit();
```

</br>

# Инъекция зависимостей
Атрибут `[EcsInject]` убирает необходимость использования интерфейса `IEcsInject<T>`, поля помеченные таким атрибутом автоматически подхватят зависимости внедренные в Pipeline. Пример： 
```csharp
[EcsInject] EcsDefaultWorld _world;
```
Так же можно делать внедрение через свойство или метод:
```csharp
EcsDefaultWorld _world;

//Обязательно наличие set блока.  
[EcsInject] EcsDefaultWorld World { set => _world = value; } 

//Количество аргументов должно быть равно 1.
[EcsInject] void InjectWorld(EcsDefaultWorld world) => _world = world;
```

> Поддерживается агрессивная инъекция, инъекция будет произведена без атрибута `[EcsInject]`, для этого нужно вызвать `.AutoInject(true)`.

</br>

# Auto Builder аспектов
Так же AutoInjections упрощает построение аспектов. Для начала наследуйте аспект не от `EcsAspect`, а от `EcsAspectAuto`, а далее добавьте специальные атрибуты.

Атрибуты для инициализации полей с пулами: 
* `[Inc]` - кеширует пул и добавит тип компонента в включающее ограничение аспекта, аналог метода `Include`;
* `[Exc]` - кеширует пул и добавит тип компонента в исключающее ограничение аспекта, аналог метода `Exclude`;
* `[Opt]` - только кеширует пул, аналог метода `Optional`;

Атрибут для комбинирования аспектов:
* `[Combine(order)]` - кеширует аспект и скомбинирует ограничения аспектов, аналог метода `Combine`, аргумент `order` задает порядок комбинирования, по умлочанию `order = 0`;

Дополнительные атрибуты только для задания ограничений аспекта. Их можно применить к самому аспекту, либо к любому полю внутри. Используйте атрибуты: 
* `[IncImplicit(type)]` - добавит в включающее ограничение указанный в конструкторе тип `type`, аналог метода `Include`;
* `[ExcImplicit(type)]` - добавит в исключающее ограничение указанный в конструкторе тип `type`, аналог метода `Exclude`;

</br>

# Auto Runner-ы
Для получения раннеров без добавления, есть атрибут `[BindWithRunner(type)]` и метод `GetRunnerAuto<T>()`. 
``` c#
[BindWithRunner(typeof(DoSomethingProcessRunner))]
interface IDoSomethingProcess : IEcsProcess
{
    void Do();
}
//Реализация раннера. Пример реализации можно так же посмотреть в встроенных процессах 
sealed class DoSomethingProcessRunner : EcsRunner<IDoSomethingProcess>, IDoSomethingProcess
{
    public void Do() 
    {
        foreach (var item in Process) item.Do();
    }
}

//...
// Если в пайплайн небыл добавлен раннер, то GetRunnerAuto автоматически добавит экземпляр DoSomethingProcessRunner.
_pipeline.GetRunnerAuto<IDoSomethingProcess>().Do();
```

</br>

# Пример кода
```csharp
class VelocitySystemDI : IEcsRun
{
    class Aspect : EcsAspectAuto
    {
        [ExcImplicit(typeof(FreezedTag))]
        [Inc] public EcsPool<Pose> poses;
        [Inc] public EcsPool<Velocity> velocities;
    }

    [EcsInject] EcsDefaultWorld _world;
    [EcsInject] TimeService _time;

    public void Run()
    {
        foreach (var e in _world.Where(out Aspect a))
        {
            a.poses.Get(e).position += a.velocities.Read(e).value * _time.DeltaTime;
        }
    }
}
```
<details>
<summary>Тот же код но без AutoInjections</summary>
    
```csharp
class VelocitySystem : IEcsRun, IEcsInject<EcsDefaultWorld>, IEcsInject<TimeService>
{
    class Aspect : EcsAspect
    {
        public EcsPool<Pose> poses;
        public EcsPool<Velocity> velocities;
        public Aspect(Builder b)
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

    public void Run()
    {
        foreach (var e in _world.Where(out Aspect a))
        {
            a.poses.Get(e).position += a.velocities.Read(e).value * _time.DeltaTime;
        }
    }
}
```

</details>
    
</br>

# Не null инъекции

Чтобы поле помеченное `[EcsInject]` было проинициализированно даже в случае отстувия инъекции, в конструктор атрибута можно передать тип болванку. В примере ниже поле `foo` получит экземпляр класса `Foo` из инъекции или экземпляр `FooDummy : Foo` если инъекции небыло.
``` csharp
[EcsInject(typeof(FooDummy))] Foo foo;
```
> Переданный тип должен иметь конструктор без параметров и быть либо того же типа что и тип поля, либо производного типа. 
  
Расширение так же сообщит если по завершению предварительной инъекции, остались не проинициализированные поля с `[EcsInject]`.
