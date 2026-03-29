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
        <img src="https://github.com/user-attachments/assets/7bc29394-46d6-44a3-bace-0a3bae65d755"></br>
        <span>Русский</span>
      </a>  
    </td>
    <td nowrap width="100">
      <a href="https://github.com/DCFApixels/DragonECS-AutoInjections">
        <img src="https://github.com/user-attachments/assets/3c699094-f8e6-471d-a7c1-6d2e9530e721"></br>
        <span>English(WIP)</span>
      </a>  
    </td>
  </tr>
</table>

</br>

The extension is designed to reduce the amount of code by simplifying dependency injection by doing injections automatically.
> [!WARNING]
> The project is a work in progress, API may change.  
>
> While the English version of the README is incomplete, you can view the [Russian version](https://github.com/DCFApixels/DragonECS-AutoInjections/blob/main/README-RU.md).

# Оглавление
- [Installation](#Installation)
- [Integration](#Integration)
- [Dependency Injection](#Dependency-Injection)
- [Auto Builder for aspects](#Auto-Builder-for-aspects)
- [Auto Runners](#Auto-Runners)
- [Code Example](#Code-Example)
- [Non-null injections](#Non-null-injections)

</br>

# Installation
Versioning semantics - [Open](https://gist.github.com/DCFApixels/af79284955bf40e9476cdcac79d7b098#file-dcfapixels_versioning-md)
## Environment
Requirements:
+ Dependency: [DragonECS](https://github.com/DCFApixels/DragonECS)
+ Minimum version of C# 7.3;
  
Optional:
* Game engines with C#: Unity, Godot, MonoGame, etc.

Tested with:
* **Unity:** Minimum version 2021.2.0.

## Unity Installation
* ### Unity Package
The package supports installation as a Unity package by adding the Git URL [in the PackageManager](https://docs.unity3d.com/2023.2/Documentation/Manual/upm-ui-giturl.html): 
```
https://github.com/DCFApixels/DragonECS-AutoInjections.git
```
Or add the package entry to `Packages/manifest.json`:
```
"com.dcfa_pixels.dragonecs-auto_injections": "https://github.com/DCFApixels/DragonECS-AutoInjections.git",
```

* ### Source Code
The package source code can also be copied directly into the project.


</br>

# Integration
Add the AutoInject() call to the pipeline Builder. Example:
```c#
_pipeline = EcsPipeline.New()
    .Inject(world)
    .Inject(_timeService)
    .Add(new TestSystem())
    .Add(new VelocitySystem())
    .Add(new ViewSystem())
    .AutoInject() // Done — automatic injections enabled
    .BuildAndInit();
```

> [!IMPORTANT] 
> Ensure AutoInject() is called during initialization; otherwise nothing will work.

# Dependency Injection
The `[DI]` attribute replaces the `IEcsInject<T>` interface. Fields marked with this attribute automatically receive dependencies injected into the pipeline. Example：
```c#
[DI] EcsDefaultWorld _world;
```
Injection can also be done via a property or method:
```c#
EcsDefaultWorld _world;

//Обязательно наличие set блока.  
[DI] EcsDefaultWorld World { set => _world = value; } 

//Количество аргументов должно быть равно 1.
[DI] void InjectWorld(EcsDefaultWorld world) => _world = world;
```

> Aggressive injection (without the `[DI]` attribute) is enabled by calling `.AutoInject(true)`.

</br>

# Auto Builder for aspects
AutoInjections also simplifies building aspects. The following attributes are available:

Attributes for initializing pool fields:
* `[Inc]` - caches the pool and adds the component type to the include constraint of the aspect (equivalent to `Inc<T>()`);
* `[Exc]` - caches the pool and adds the component type to the exclude constraint (equivalent to `Exc<T>()`);
* `[Opt]` - only caches the pool (equivalent to `Opt<T>`);
* 
Attribute for combining aspects:
* `[Combine(order)]` - caches the aspect and merges constraints from aspects (equivalent to `Combine<TOtherAspect>(int)`); order sets combine order (default 0);

Additional attributes for specifying aspect constraints. They can be applied to the aspect itself or any field inside:
* `[IncImplicit(type)]` - adds Type from the constructor to the include constraint (equivalent to `Inc<T>()`);
* `[ExcImplicit(type)]` - adds Type from the constructor to the exclude constraint (equivalent to `Exc<T>()`);

</br>

# Auto Runners

To obtain runners without adding them manually, use `[BindWithRunner(type)]` and `GetRunnerAuto<T>()`.

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

# Code Example

```c#
class VelocitySystemDI : IEcsRun
{
    class Aspect : EcsAspectAuto
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
<summary>Same code but without AutoInjections</summary>

    
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
            poses = b.Inc<Pose>();
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


# Non-null injections
To ensure a field marked with `[DI]` is initialized even if injection does not occur, pass a fallback type to the attribute constructor. In the example below the field `Foo` will receive the injected `Foo` instance or an instance of `FooDummy : Foo` if injection was not performed.

> The provided type must have a parameterless constructor and be either the same type as the field or derived from it.

The extension will also report if any `[DI]`-marked fields remain uninitialized after the pre-injection phase.