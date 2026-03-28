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
