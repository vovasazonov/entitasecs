using System.Collections.Generic;
using System.Diagnostics;
using Entitas;
using Entitas.VisualDebugging.Unity;
using UnityEngine;

namespace Osyacat.Ecs.EntitasEcs.System.Debug
{
  public class CustomDebugSystems : Entitas.Systems
  {
    public static AvgResetInterval avgResetInterval = AvgResetInterval.Never;
    public bool paused;
    private string _name;
    private List<ISystem> _systems;
    private GameObject _gameObject;
    private CustomSystemInfo _systemInfo;
    private List<CustomSystemInfo> _initializeSystemInfos;
    private List<CustomSystemInfo> _executeSystemInfos;
    private List<CustomSystemInfo> _cleanupSystemInfos;
    private List<CustomSystemInfo> _tearDownSystemInfos;
    private Stopwatch _stopwatch;
    private double _executeDuration;
    private double _cleanupDuration;

    public int totalInitializeSystemsCount
    {
      get
      {
        int num = 0;
        foreach (IInitializeSystem initializeSystem in this._initializeSystems)
        {
          DebugSystems debugSystems = initializeSystem as DebugSystems;
          num += debugSystems != null ? debugSystems.totalInitializeSystemsCount : 1;
        }
        return num;
      }
    }

    public int totalExecuteSystemsCount
    {
      get
      {
        int num = 0;
        foreach (IExecuteSystem executeSystem in this._executeSystems)
        {
          DebugSystems debugSystems = executeSystem as DebugSystems;
          num += debugSystems != null ? debugSystems.totalExecuteSystemsCount : 1;
        }
        return num;
      }
    }

    public int totalCleanupSystemsCount
    {
      get
      {
        int num = 0;
        foreach (ICleanupSystem cleanupSystem in this._cleanupSystems)
        {
          DebugSystems debugSystems = cleanupSystem as DebugSystems;
          num += debugSystems != null ? debugSystems.totalCleanupSystemsCount : 1;
        }
        return num;
      }
    }

    public int totalTearDownSystemsCount
    {
      get
      {
        int num = 0;
        foreach (ITearDownSystem tearDownSystem in this._tearDownSystems)
        {
          DebugSystems debugSystems = tearDownSystem as DebugSystems;
          num += debugSystems != null ? debugSystems.totalTearDownSystemsCount : 1;
        }
        return num;
      }
    }

    public int totalSystemsCount
    {
      get
      {
        int num = 0;
        foreach (ISystem system in this._systems)
        {
          DebugSystems debugSystems = system as DebugSystems;
          num += debugSystems != null ? debugSystems.totalSystemsCount : 1;
        }
        return num;
      }
    }

    public int initializeSystemsCount => this._initializeSystems.Count;

    public int executeSystemsCount => this._executeSystems.Count;

    public int cleanupSystemsCount => this._cleanupSystems.Count;

    public int tearDownSystemsCount => this._tearDownSystems.Count;

    public string name => this._name;

    public GameObject gameObject => this._gameObject;

    public CustomSystemInfo systemInfo => this._systemInfo;

    public double executeDuration => this._executeDuration;

    public double cleanupDuration => this._cleanupDuration;

    public CustomSystemInfo[] initializeSystemInfos => this._initializeSystemInfos.ToArray();

    public CustomSystemInfo[] executeSystemInfos => this._executeSystemInfos.ToArray();

    public CustomSystemInfo[] cleanupSystemInfos => this._cleanupSystemInfos.ToArray();

    public CustomSystemInfo[] tearDownSystemInfos => this._tearDownSystemInfos.ToArray();

    public CustomDebugSystems(string name) => this.initialize(name);

    protected CustomDebugSystems(bool noInit)
    {
    }

    protected void initialize(string name)
    {
      this._name = name;
      this._gameObject = new GameObject(name);
      this._gameObject.AddComponent<CustomDebugSystemsBehaviour>().Init(this);
      this._systemInfo = new CustomSystemInfo((ISystem) this);
      this._systems = new List<ISystem>();
      this._initializeSystemInfos = new List<CustomSystemInfo>();
      this._executeSystemInfos = new List<CustomSystemInfo>();
      this._cleanupSystemInfos = new List<CustomSystemInfo>();
      this._tearDownSystemInfos = new List<CustomSystemInfo>();
      this._stopwatch = new Stopwatch();
    }

    public override Entitas.Systems Add(ISystem system)
    {
      this._systems.Add(system);
      CustomSystemInfo systemInfo;
      if (system is CustomDebugSystems debugSystems)
      {
        systemInfo = debugSystems.systemInfo;
        debugSystems.gameObject.transform.SetParent(this._gameObject.transform, false);
      }
      else
        systemInfo = new CustomSystemInfo(system);
      systemInfo.parentCustomSystemInfo = this._systemInfo;
      if (systemInfo.isInitializeSystems)
        this._initializeSystemInfos.Add(systemInfo);
      if (systemInfo.isExecuteSystems || systemInfo.isReactiveSystems)
        this._executeSystemInfos.Add(systemInfo);
      if (systemInfo.isCleanupSystems)
        this._cleanupSystemInfos.Add(systemInfo);
      if (systemInfo.isTearDownSystems)
        this._tearDownSystemInfos.Add(systemInfo);
      return base.Add(system);
    }

    public void ResetDurations()
    {
      foreach (CustomSystemInfo executeSystemInfo in this._executeSystemInfos)
        executeSystemInfo.ResetDurations();
      foreach (ISystem system in this._systems)
      {
        if (system is CustomDebugSystems debugSystems1)
          debugSystems1.ResetDurations();
      }
    }

    public override void Initialize()
    {
      for (int index = 0; index < this._initializeSystems.Count; ++index)
      {
        CustomSystemInfo initializeSystemInfo = this._initializeSystemInfos[index];
        if (initializeSystemInfo.isActive)
        {
          this._stopwatch.Reset();
          this._stopwatch.Start();
          this._initializeSystems[index].Initialize();
          this._stopwatch.Stop();
          initializeSystemInfo.initializationDuration = this._stopwatch.Elapsed.TotalMilliseconds;
        }
      }
    }

    public override void Execute()
    {
      if (this.paused)
        return;
      this.StepExecute();
    }

    public override void Cleanup()
    {
      if (this.paused)
        return;
      this.StepCleanup();
    }

    public void StepExecute()
    {
      this._executeDuration = 0.0;
      if (Time.frameCount % (int) DebugSystems.avgResetInterval == 0)
        this.ResetDurations();
      for (int index = 0; index < this._executeSystems.Count; ++index)
      {
        CustomSystemInfo executeSystemInfo = this._executeSystemInfos[index];
        if (executeSystemInfo.isActive)
        {
          this._stopwatch.Reset();
          this._stopwatch.Start();
          this._executeSystems[index].Execute();
          this._stopwatch.Stop();
          double totalMilliseconds = this._stopwatch.Elapsed.TotalMilliseconds;
          this._executeDuration += totalMilliseconds;
          executeSystemInfo.AddExecutionDuration(totalMilliseconds);
        }
      }
    }

    public void StepCleanup()
    {
      this._cleanupDuration = 0.0;
      for (int index = 0; index < this._cleanupSystems.Count; ++index)
      {
        CustomSystemInfo cleanupSystemInfo = this._cleanupSystemInfos[index];
        if (cleanupSystemInfo.isActive)
        {
          this._stopwatch.Reset();
          this._stopwatch.Start();
          this._cleanupSystems[index].Cleanup();
          this._stopwatch.Stop();
          double totalMilliseconds = this._stopwatch.Elapsed.TotalMilliseconds;
          this._cleanupDuration += totalMilliseconds;
          cleanupSystemInfo.AddCleanupDuration(totalMilliseconds);
        }
      }
    }

    public override void TearDown()
    {
      for (int index = 0; index < this._tearDownSystems.Count; ++index)
      {
        CustomSystemInfo tearDownSystemInfo = this._tearDownSystemInfos[index];
        if (tearDownSystemInfo.isActive)
        {
          this._stopwatch.Reset();
          this._stopwatch.Start();
          this._tearDownSystems[index].TearDown();
          this._stopwatch.Stop();
          tearDownSystemInfo.teardownDuration = this._stopwatch.Elapsed.TotalMilliseconds;
        }
      }
    }
  }
}
