using System;
using System.Reflection;
using Entitas;
using Entitas.VisualDebugging.Unity;

namespace Osyacat.Ecs.EntitasEcs.System.Debug
{
  public class CustomSystemInfo
  {
    public CustomSystemInfo parentCustomSystemInfo;
    public bool isActive;
    private readonly ISystem _system;
    private readonly SystemInterfaceFlags _interfaceFlags;
    private readonly string _systemName;
    private double _accumulatedExecutionDuration;
    private double _minExecutionDuration;
    private double _maxExecutionDuration;
    private int _executionDurationsCount;
    private double _accumulatedCleanupDuration;
    private double _minCleanupDuration;
    private double _maxCleanupDuration;
    private int _cleanupDurationsCount;

    public ISystem system => this._system;

    public string systemName => this._systemName;

    public bool isInitializeSystems => (this._interfaceFlags & SystemInterfaceFlags.IInitializeSystem) == SystemInterfaceFlags.IInitializeSystem;

    public bool isExecuteSystems => (this._interfaceFlags & SystemInterfaceFlags.IExecuteSystem) == SystemInterfaceFlags.IExecuteSystem;

    public bool isCleanupSystems => (this._interfaceFlags & SystemInterfaceFlags.ICleanupSystem) == SystemInterfaceFlags.ICleanupSystem;

    public bool isTearDownSystems => (this._interfaceFlags & SystemInterfaceFlags.ITearDownSystem) == SystemInterfaceFlags.ITearDownSystem;

    public bool isReactiveSystems => (this._interfaceFlags & SystemInterfaceFlags.IReactiveSystem) == SystemInterfaceFlags.IReactiveSystem;

    public double initializationDuration { get; set; }

    public double accumulatedExecutionDuration => this._accumulatedExecutionDuration;

    public double minExecutionDuration => this._minExecutionDuration;

    public double maxExecutionDuration => this._maxExecutionDuration;

    public double averageExecutionDuration => this._executionDurationsCount != 0 ? this._accumulatedExecutionDuration / (double) this._executionDurationsCount : 0.0;

    public double accumulatedCleanupDuration => this._accumulatedCleanupDuration;

    public double minCleanupDuration => this._minCleanupDuration;

    public double maxCleanupDuration => this._maxCleanupDuration;

    public double averageCleanupDuration => this._cleanupDurationsCount != 0 ? this._accumulatedCleanupDuration / (double) this._cleanupDurationsCount : 0.0;

    public double cleanupDuration { get; set; }

    public double teardownDuration { get; set; }

    public bool areAllParentsActive
    {
      get
      {
        if (this.parentCustomSystemInfo == null)
          return true;
        return this.parentCustomSystemInfo.isActive && this.parentCustomSystemInfo.areAllParentsActive;
      }
    }

    public CustomSystemInfo(ISystem system)
    {
      this._system = system;
      this._interfaceFlags = CustomSystemInfo.getInterfaceFlags(system);
      Type type = system.GetType();
      TypeInfo typeInfo = type.GetTypeInfo();
      Type[] genericTypes = typeInfo.GenericTypeArguments;
      string systemName = genericTypes != null && genericTypes.Length > 0 ? genericTypes[0].Name : type.Name.RemoveSystemSuffix();
      this._systemName = system is DebugSystems debugSystems ? debugSystems.name : systemName;
      this.isActive = true;
    }

    public void AddExecutionDuration(double executionDuration)
    {
      if (executionDuration < this._minExecutionDuration || this._minExecutionDuration == 0.0)
        this._minExecutionDuration = executionDuration;
      if (executionDuration > this._maxExecutionDuration)
        this._maxExecutionDuration = executionDuration;
      this._accumulatedExecutionDuration += executionDuration;
      ++this._executionDurationsCount;
    }

    public void AddCleanupDuration(double cleanupDuration)
    {
      if (cleanupDuration < this._minCleanupDuration || this._minCleanupDuration == 0.0)
        this._minCleanupDuration = cleanupDuration;
      if (cleanupDuration > this._maxCleanupDuration)
        this._maxCleanupDuration = cleanupDuration;
      this._accumulatedCleanupDuration += cleanupDuration;
      ++this._cleanupDurationsCount;
    }

    public void ResetDurations()
    {
      this._accumulatedExecutionDuration = 0.0;
      this._executionDurationsCount = 0;
      this._accumulatedCleanupDuration = 0.0;
      this._cleanupDurationsCount = 0;
    }

    private static SystemInterfaceFlags getInterfaceFlags(ISystem system)
    {
      SystemInterfaceFlags systemInterfaceFlags = SystemInterfaceFlags.None;
      if (system is IInitializeSystem)
        systemInterfaceFlags |= SystemInterfaceFlags.IInitializeSystem;
      if (system is IReactiveSystem)
        systemInterfaceFlags |= SystemInterfaceFlags.IReactiveSystem;
      else if (system is IExecuteSystem)
        systemInterfaceFlags |= SystemInterfaceFlags.IExecuteSystem;
      if (system is ICleanupSystem)
        systemInterfaceFlags |= SystemInterfaceFlags.ICleanupSystem;
      if (system is ITearDownSystem)
        systemInterfaceFlags |= SystemInterfaceFlags.ITearDownSystem;
      return systemInterfaceFlags;
    }
  }
}
