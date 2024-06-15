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

The extension is designed to reduce the amount of code by simplifying dependency injection by doing injections automatically.
> **NOTICE:** The project is a work in progress, API may change.  
> While the English version of the README is incomplete, you can view the [Russian version](https://github.com/DCFApixels/DragonECS-AutoInjections/blob/main/README-RU.md).


</br>

# Installation
Versioning semantics - [Open](https://gist.github.com/DCFApixels/e53281d4628b19fe5278f3e77a7da9e8#file-dcfapixels_versioning_ru-md)
## Environment
Requirements:
+ Dependency: [DragonECS](https://github.com/DCFApixels/DragonECS)
+ Minimum version of C# 7.3;
  
Optional:
+ Game engines with C#: Unity, Godot, MonoGame, etc.
  
Tested with:
+ **Unity:** Minimum version 2020.1.0;

## Unity Installation
* ### Unity Package
The framework can be installed as a Unity package by adding the Git URL [in the PackageManager](https://docs.unity3d.com/2023.2/Documentation/Manual/upm-ui-giturl.html) or manually adding it to `Packages/manifest.json`: 
```
https://github.com/DCFApixels/DragonECS-AutoInjections.git
```
* ### Source Code
The framework can also be added to the project as source code.

</br>

# Code Example
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
<summary>Same code but without AutoInjections</summary>
    
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
