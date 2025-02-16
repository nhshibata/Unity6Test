using System;
using System.Collections.Generic;
using Zenject;

public class ZenjectSample : MonoInstaller
{
    public override void InstallBindings()
    {
        Container
            .Bind<ServiceLocator>()
            .AsSingle(); // シングルトンで登録

        // 初期化を専用のクラスで行う
        Container
            .Bind<IInitializable>()
            .To<ServiceInitializer>()
            .AsSingle();
    }
}

public interface IExample { }
public class Example : IExample { }

public class ServiceLocator
{
    private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public void RegisterService<T>(T instance)
    {
        _services[typeof(T)] = instance;
    }

    public T GetService<T>()
    {
        if (_services.TryGetValue(typeof(T), out var instance))
        {
            return (T)instance;
        }

        throw new KeyNotFoundException($"Service of type {typeof(T)} is not registered.");
    }
}

public class ServiceInitializer : IInitializable
{
    private readonly ServiceLocator _serviceLocator;

    public ServiceInitializer(ServiceLocator serviceLocator)
    {
        _serviceLocator = serviceLocator;
    }

    public void Initialize()
    {
        // 初期化処理として ServiceLocator にサービスを登録
        _serviceLocator.RegisterService<IExample>(new Example());
        UnityEngine.Debug.Log("ServiceLocator initialized!");
    }
}