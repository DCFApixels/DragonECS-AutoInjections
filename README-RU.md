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

<table>
  <tr></tr>
  <tr>
    <td colspan="3">Readme Languages:</td>
  </tr>
  <tr></tr>
  <tr>
    <td nowrap width="100">
      <a href="https://github.com/DCFApixels/DragonECS-AutoInjections/blob/main/README-RU.md">
        <img src="https://github.com/user-attachments/assets/3c699094-f8e6-471d-a7c1-6d2e9530e721"></br>
        <span>Русский</span>
      </a>  
    </td>
    <td nowrap width="100">
      <a href="https://github.com/DCFApixels/DragonECS-AutoInjections">
        <img src="https://github.com/user-attachments/assets/30528cb5-f38e-49f0-b23e-d001844ae930"></br>
        <span>English(WIP)</span>
      </a>  
    </td>
  </tr>
</table>

</br>

Расширение автоматизирует внедрение зависимостей, что позволяет сократить объем кода и упростить разработку.
> [!WARNING] 
> Проект в стадии разработки. API может меняться.

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
Семантика версионирования - [Открыть](https://gist.github.com/DCFApixels/af79284955bf40e9476cdcac79d7b098#file-dcfapixels_versioning-md)
## Окружение
Обязательные требования:
+ Зависимость: [DragonECS](https://github.com/DCFApixels/DragonECS)
+ Минимальная версия C# 7.3;

Поддерживает:
+ Игровые движки с C#: Unity, Godot, MonoGame и т.д.

Протестировано на:
* **Unity:** Минимальная версия 2021.2.0.

## Установка для Unity
* ### Unity-модуль
Поддерживается установка в виде Unity-модуля при помощи добавления git-URL [в PackageManager](https://docs.unity3d.com/2023.2/Documentation/Manual/upm-ui-giturl.html): 
```
https://github.com/DCFApixels/DragonECS-AutoInjections.git
```
Или ручного добавления этой строчки в `Packages/manifest.json`:
```
"com.dcfa_pixels.dragonecs-auto_injections": "https://github.com/DCFApixels/DragonECS-AutoInjections.git",
```

* ### В виде исходников
Можно так же напрямую скопировать в проект исходники фреймворка.

</br>

# Интеграция
Добавьте вызов метода `AutoInject()` для Builder-а пайплайна. Пример:
```c#
_pipeline = EcsPipeline.New()
    .Inject(world)
    .Inject(_timeService)
    .Add(new TestSystem())
    .Add(new VelocitySystem())
    .Add(new ViewSystem())
    .AutoInject() // Готово, автоматические внедрения активированы
    .BuildAndInit();
```

> [!IMPORTANT] 
> Проверьте что в инициализации добавлен вызов `AutoInject()`, иначе ничего не будет работать.

</br>

# Инъекция зависимостей
Атрибут `[DI]` заменяет интерфейс `IEcsInject<T>`, Поля, отмеченные этим атрибутом, автоматически получают зависимости, внедрённые в Pipeline. Пример： 
```c#
[DI] EcsDefaultWorld _world;
```
Так же можно делать внедрение через свойство или метод:
```c#
EcsDefaultWorld _world;

//Обязательно наличие set блока.  
[DI] EcsDefaultWorld World { set => _world = value; } 

//Количество аргументов должно быть равно 1.
[DI] void InjectWorld(EcsDefaultWorld world) => _world = world;
```

> Агрессивная инъекция (без атрибута `[DI]`) включается вызовом `.AutoInject(true)`.

</br>

# Auto Builder аспектов
Так же AutoInjections упрощает построение аспектов. Теперь аспекту Для этого есть следующие атрибуты:

Атрибуты для инициализации полей с пулами: 
* `[Inc]` - кеширует пул и добавит тип компонента в включающее ограничение аспекта, аналог метода `Inc<T>()`;
* `[Exc]` - кеширует пул и добавит тип компонента в исключающее ограничение аспекта, аналог метода `Exc<T>()`;
* `[Opt]` - только кеширует пул, аналог метода `Opt<T>()`;

Атрибут для комбинирования аспектов:
* `[Combine(order)]` - кеширует аспект и скомбинирует ограничения аспектов, аналог метода `Combine<TOtherAspect>(int)`, аргумент `order` задает порядок комбинирования, по умлочанию `order = 0`;

Дополнительные атрибуты только для задания ограничений аспекта. Их можно применить к самому аспекту, либо к любому полю внутри. Используйте атрибуты: 
* `[IncImplicit(type)]` - добавит в включающее ограничение указанный в конструкторе тип `type`, аналог метода `Inc<T>()`;
* `[ExcImplicit(type)]` - добавит в исключающее ограничение указанный в конструкторе тип `type`, аналог метода `Exc<T>()`;

Для инициализации аспекта не обязательно наследоваться от `EcsAspect`, Пример:
```c#
class Aspect
{
    [ExcImplicit(typeof(FreezedTag))]
    [Inc] public EcsPool<Pose> poses;
    [Inc] public EcsPool<Velocity> velocities;
}
```

</br>

# Auto Runner-ы
Для получения раннеров без добавления, есть атрибут `[BindWithRunner(type)]` и метод `GetRunnerAuto<T>()`. 

```c#
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
// Если в пайплайн не был добавлен раннер, то GetRunnerAuto автоматически добавит экземпляр DoSomethingProcessRunner.
_pipeline.GetRunnerAuto<IDoSomethingProcess>().Do();
```

</br>

# Пример кода

```c#
class VelocitySystemDI : IEcsRun
{
    class Aspect
    {
        [ExcImplicit(typeof(FreezedTag))]
        [Inc] public EcsPool<Pose> poses;
        [Inc] public EcsPool<Velocity> velocities;
    }

    [DI] EcsDefaultWorld _world;
    [DI] TimeService _time;

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

```c#
class VelocitySystem : IEcsRun, IEcsInject<EcsDefaultWorld>, IEcsInject<TimeService>
{
    class Aspect : EcsAspect
    {
        public EcsPool<Pose> poses;
        public EcsPool<Velocity> velocities;
        public Aspect(Builder b)
        {
            b.Exc<FreezedTag>();
            poses = b.Ince<Pose>();
            velocities = b.Inc<Velocity>();
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

Чтобы поле, отмеченное атрибутом `[DI]`, было проинициализировано даже в случае отсутствия инъекции, в конструктор атрибута можно передать тип-заглушку. В примере ниже поле `foo` получит экземпляр `Foo` из инъекции или экземпляр `FooDummy : Foo`, если инъекция не была выполнена.
```c#
[DI(typeof(FooDummy))] Foo foo;
```
> Переданный тип должен иметь конструктор без параметров и быть либо того же типа, что и поле, либо производным от него. 
  
Расширение также сообщит, если после завершения предварительной инъекции остались непроинициализированные поля с атрибутом `[DI]`.

</br>

# Лицензия
MIT Лицензия: [Открыть](LICENSE.md)