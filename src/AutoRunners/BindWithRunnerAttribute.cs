using System;
[AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
public sealed class BindWithRunnerAttribute : Attribute
{
    public readonly Type runnerType;
    public BindWithRunnerAttribute(Type runnerType)
    {
        this.runnerType = runnerType;
    }
}