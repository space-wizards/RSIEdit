using System;
using Splat;

namespace Editor.Extensions;

public static class DependencyResolvedExtensions
{
    public static T GetRequiredService<T>(this IReadonlyDependencyResolver resolver, string? contract = null)
    {
        return (T) (resolver.GetService<T>(contract) ??
                    throw new NullReferenceException($"No service found of type {typeof(T)}"));
    }
}