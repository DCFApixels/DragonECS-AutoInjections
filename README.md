<p align="center">
<img width="400" src="https://github.com/DCFApixels/DragonECS-AutoInjections/assets/99481254/11868b2e-21f7-4f47-8970-03ad6329cf0e.png">
</p>

<p align="center">
<img alt="Version" src="https://img.shields.io/github/package-json/v/DCFApixels/DragonECS-AutoInjections?color=%23ff4e85&style=for-the-badge">
<img alt="License" src="https://img.shields.io/github/license/DCFApixels/DragonECS-AutoInjections?color=ff4e85&style=for-the-badge">
<!--<img alt="Discord" src="https://img.shields.io/discord/1111696966208999525?color=%23ff4e85&label=Discord&logo=Discord&logoColor=%23ff4e85&style=for-the-badge">-->
</p>

# Auto Injections for [DragonECS](https://github.com/DCFApixels/DragonECS)

| Languages: | [Русский](https://github.com/DCFApixels/DragonECS-AutoInjections/blob/main/README-RU.md) | [English(WIP)](https://github.com/DCFApixels/DragonECS-AutoInjections) |
| :--- | :--- | :--- |

The extension is designed to reduce the amount of code by simplifying dependency injection by doing injections automatically.
> **NOTICE:** The project is a work in progress, API may change.  
> While the English version of the README is incomplete, you can view the [Russian version](https://github.com/DCFApixels/DragonECS-AutoInjections/blob/main/README-RU.md).

# Versioning
DragonECS uses this versioning semantics: [Open](https://gist.github.com/DCFApixels/c3b178a308b411f530361d1d56f1f929#versioning)

# Code Example
```csharp
class VelocitySystemDI : IEcsRunProcess
{
    class Aspect : EcsAspectAuto
    {
        [ExcImplicit(typeof(FreezedTag))]
        [Inc] public EcsPool<Pose> poses;
        [Inc] public EcsPool<Velocity> velocities;
    }

    [EcsInject] EcsDefaultWorld _world;
    [EcsInject] TimeService _time;

    public void Run(EcsPipeline pipeline)
    {
        foreach (var e in _world.Where(out Aspect s))
        {
            s.poses.Write(e).position += s.velocities.Read(e).value * _time.DeltaTime;
        }
    }
}
```
<details>
<summary>Same code but without AutoInjections</summary>
    
```csharp
class VelocitySystem : IEcsRunProcess, IEcsInject<EcsDefaultWorld>, IEcsInject<TimeService>
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

    public void Run(EcsPipeline pipeline)
    {
        foreach (var e in _world.Where(out Aspect s))
        {
            s.poses.Write(e).position += s.velocities.Read(e).value * _time.DeltaTime;
        }
    }
}
```

</details>
