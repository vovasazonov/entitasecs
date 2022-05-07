using DesperateDevs.Serialization;
using DesperateDevs.Unity.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using Osyacat.Ecs.EntitasEcs.System.Debug;
using UnityEditor;
using UnityEngine;

namespace Entitas.VisualDebugging.Unity.Editor
{
  [CustomEditor(typeof (CustomDebugSystemsBehaviour))]
  public class CustomDebugSystemsInspector : UnityEditor.Editor
  {
    private Graph _systemsMonitor;
    private Queue<float> _systemMonitorData;
    private const int SYSTEM_MONITOR_DATA_LENGTH = 60;
    private static bool _showDetails = false;
    private static bool _showSystemsMonitor = true;
    private static bool _showSystemsList = true;
    private static bool _showInitializeSystems = true;
    private static bool _showExecuteSystems = true;
    private static bool _showCleanupSystems = true;
    private static bool _showTearDownSystems = true;
    private static bool _hideEmptySystems = true;
    private static string _systemNameSearchString = string.Empty;
    private int _systemWarningThreshold;
    private float _threshold;
    private CustomDebugSystemsInspector.SortMethod _systemSortMethod;
    private int _lastRenderedFrameCount;
    private GUIContent _stepButtonContent;
    private GUIContent _pauseButtonContent;

    private void OnEnable() => this._systemWarningThreshold = new Preferences("Entitas.properties", Environment.UserName + ".userproperties").CreateAndConfigure<VisualDebuggingConfig>().systemWarningThreshold;

    public override void OnInspectorGUI()
    {
      CustomDebugSystems systems = ((CustomDebugSystemsBehaviour) this.target).systems;
      EditorGUILayout.Space();
      CustomDebugSystemsInspector.drawSystemsOverview(systems);
      EditorGUILayout.Space();
      this.drawSystemsMonitor(systems);
      EditorGUILayout.Space();
      this.drawSystemList(systems);
      EditorGUILayout.Space();
      EditorUtility.SetDirty(this.target);
    }

    private static void drawSystemsOverview(CustomDebugSystems systems)
    {
      CustomDebugSystemsInspector._showDetails = EditorLayout.DrawSectionHeaderToggle("Details", CustomDebugSystemsInspector._showDetails);
      if (!CustomDebugSystemsInspector._showDetails)
        return;
      EditorLayout.BeginSectionContent();
      EditorGUILayout.LabelField(systems.name, EditorStyles.boldLabel);
      EditorGUILayout.LabelField("Initialize Systems", systems.totalInitializeSystemsCount.ToString());
      EditorGUILayout.LabelField("Execute Systems", systems.totalExecuteSystemsCount.ToString());
      EditorGUILayout.LabelField("Cleanup Systems", systems.totalCleanupSystemsCount.ToString());
      EditorGUILayout.LabelField("TearDown Systems", systems.totalTearDownSystemsCount.ToString());
      EditorGUILayout.LabelField("Total Systems", systems.totalSystemsCount.ToString());
      EditorLayout.EndSectionContent();
    }

    private void drawSystemsMonitor(CustomDebugSystems systems)
    {
      if (this._systemsMonitor == null)
      {
        this._systemsMonitor = new Graph(60);
        this._systemMonitorData = new Queue<float>((IEnumerable<float>) new float[60]);
      }
      CustomDebugSystemsInspector._showSystemsMonitor = EditorLayout.DrawSectionHeaderToggle("Performance", CustomDebugSystemsInspector._showSystemsMonitor);
      if (!CustomDebugSystemsInspector._showSystemsMonitor)
        return;
      EditorLayout.BeginSectionContent();
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.BeginVertical();
      EditorGUILayout.LabelField("Execution duration", systems.executeDuration.ToString());
      EditorGUILayout.LabelField("Cleanup duration", systems.cleanupDuration.ToString());
      EditorGUILayout.EndVertical();
      if (this._stepButtonContent == null)
        this._stepButtonContent = EditorGUIUtility.IconContent("StepButton On");
      if (this._pauseButtonContent == null)
        this._pauseButtonContent = EditorGUIUtility.IconContent("PauseButton On");
      systems.paused = GUILayout.Toggle(systems.paused, this._pauseButtonContent, (GUIStyle) "CommandLeft");
      if (GUILayout.Button(this._stepButtonContent, (GUIStyle) "CommandRight"))
      {
        systems.paused = true;
        systems.StepExecute();
        systems.StepCleanup();
        this.addDuration((float) systems.executeDuration + (float) systems.cleanupDuration);
      }
      EditorGUILayout.EndHorizontal();
      if (!EditorApplication.isPaused && !systems.paused)
        this.addDuration((float) systems.executeDuration + (float) systems.cleanupDuration);
      this._systemsMonitor.Draw(this._systemMonitorData.ToArray(), 80f);
      EditorLayout.EndSectionContent();
    }

    private void drawSystemList(CustomDebugSystems systems)
    {
      CustomDebugSystemsInspector._showSystemsList = EditorLayout.DrawSectionHeaderToggle("Systems", CustomDebugSystemsInspector._showSystemsList);
      if (!CustomDebugSystemsInspector._showSystemsList)
        return;
      EditorLayout.BeginSectionContent();
      EditorGUILayout.BeginHorizontal();
      CustomDebugSystems.avgResetInterval = (AvgResetInterval) EditorGUILayout.EnumPopup("Reset average duration Ø", (Enum) CustomDebugSystems.avgResetInterval);
      if (GUILayout.Button("Reset Ø now", EditorStyles.miniButton, GUILayout.Width(88f)))
        systems.ResetDurations();
      EditorGUILayout.EndHorizontal();
      this._threshold = EditorGUILayout.Slider("Threshold Ø ms", this._threshold, 0.0f, 33f);
      CustomDebugSystemsInspector._hideEmptySystems = EditorGUILayout.Toggle("Hide empty systems", CustomDebugSystemsInspector._hideEmptySystems);
      EditorGUILayout.Space();
      EditorGUILayout.BeginHorizontal();
      this._systemSortMethod = (CustomDebugSystemsInspector.SortMethod) EditorGUILayout.EnumPopup((Enum) this._systemSortMethod, EditorStyles.popup, GUILayout.Width(150f));
      CustomDebugSystemsInspector._systemNameSearchString = EditorLayout.SearchTextField(CustomDebugSystemsInspector._systemNameSearchString);
      EditorGUILayout.EndHorizontal();
      EditorGUILayout.Space();
      CustomDebugSystemsInspector._showInitializeSystems = EditorLayout.DrawSectionHeaderToggle("Initialize Systems", CustomDebugSystemsInspector._showInitializeSystems);
      if (CustomDebugSystemsInspector._showInitializeSystems && CustomDebugSystemsInspector.shouldShowSystems(systems, SystemInterfaceFlags.IInitializeSystem))
      {
        EditorLayout.BeginSectionContent();
        if (this.drawSystemInfos(systems, SystemInterfaceFlags.IInitializeSystem) == 0)
          EditorGUILayout.LabelField(string.Empty);
        EditorLayout.EndSectionContent();
      }
      CustomDebugSystemsInspector._showExecuteSystems = EditorLayout.DrawSectionHeaderToggle("Execute Systems", CustomDebugSystemsInspector._showExecuteSystems);
      if (CustomDebugSystemsInspector._showExecuteSystems && CustomDebugSystemsInspector.shouldShowSystems(systems, SystemInterfaceFlags.IExecuteSystem))
      {
        EditorLayout.BeginSectionContent();
        if (this.drawSystemInfos(systems, SystemInterfaceFlags.IExecuteSystem) == 0)
          EditorGUILayout.LabelField(string.Empty);
        EditorLayout.EndSectionContent();
      }
      CustomDebugSystemsInspector._showCleanupSystems = EditorLayout.DrawSectionHeaderToggle("Cleanup Systems", CustomDebugSystemsInspector._showCleanupSystems);
      if (CustomDebugSystemsInspector._showCleanupSystems && CustomDebugSystemsInspector.shouldShowSystems(systems, SystemInterfaceFlags.ICleanupSystem))
      {
        EditorLayout.BeginSectionContent();
        if (this.drawSystemInfos(systems, SystemInterfaceFlags.ICleanupSystem) == 0)
          EditorGUILayout.LabelField(string.Empty);
        EditorLayout.EndSectionContent();
      }
      CustomDebugSystemsInspector._showTearDownSystems = EditorLayout.DrawSectionHeaderToggle("TearDown Systems", CustomDebugSystemsInspector._showTearDownSystems);
      if (CustomDebugSystemsInspector._showTearDownSystems && CustomDebugSystemsInspector.shouldShowSystems(systems, SystemInterfaceFlags.ITearDownSystem))
      {
        EditorLayout.BeginSectionContent();
        if (this.drawSystemInfos(systems, SystemInterfaceFlags.ITearDownSystem) == 0)
          EditorGUILayout.LabelField(string.Empty);
        EditorLayout.EndSectionContent();
      }
      EditorLayout.EndSectionContent();
    }

    private int drawSystemInfos(CustomDebugSystems systems, SystemInterfaceFlags type)
    {
      CustomSystemInfo[] systemInfos = (CustomSystemInfo[]) null;
      switch (type)
      {
        case SystemInterfaceFlags.IInitializeSystem:
          systemInfos = ((IEnumerable<CustomSystemInfo>) systems.initializeSystemInfos).Where<CustomSystemInfo>((Func<CustomSystemInfo, bool>) (systemInfo => systemInfo.initializationDuration >= (double) this._threshold)).ToArray<CustomSystemInfo>();
          break;
        case SystemInterfaceFlags.IExecuteSystem:
          systemInfos = ((IEnumerable<CustomSystemInfo>) systems.executeSystemInfos).Where<CustomSystemInfo>((Func<CustomSystemInfo, bool>) (systemInfo => systemInfo.averageExecutionDuration >= (double) this._threshold)).ToArray<CustomSystemInfo>();
          break;
        case SystemInterfaceFlags.ICleanupSystem:
          systemInfos = ((IEnumerable<CustomSystemInfo>) systems.cleanupSystemInfos).Where<CustomSystemInfo>((Func<CustomSystemInfo, bool>) (systemInfo => systemInfo.cleanupDuration >= (double) this._threshold)).ToArray<CustomSystemInfo>();
          break;
        case SystemInterfaceFlags.ITearDownSystem:
          systemInfos = ((IEnumerable<CustomSystemInfo>) systems.tearDownSystemInfos).Where<CustomSystemInfo>((Func<CustomSystemInfo, bool>) (systemInfo => systemInfo.teardownDuration >= (double) this._threshold)).ToArray<CustomSystemInfo>();
          break;
      }
      CustomSystemInfo[] sortedSystemInfos = CustomDebugSystemsInspector.getSortedSystemInfos(systemInfos, this._systemSortMethod);
      int num = 0;
      foreach (CustomSystemInfo systemInfo in sortedSystemInfos)
      {
        if (!(systemInfo.system is CustomDebugSystems system9) || CustomDebugSystemsInspector.shouldShowSystems(system9, type))
        {
          if (EditorLayout.MatchesSearchString(systemInfo.systemName.ToLower(), CustomDebugSystemsInspector._systemNameSearchString.ToLower()))
          {
            EditorGUILayout.BeginHorizontal();
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            bool isActive = systemInfo.isActive;
            if (systemInfo.areAllParentsActive)
            {
              systemInfo.isActive = (EditorGUILayout.Toggle((systemInfo.isActive ? 1 : 0) != 0, GUILayout.Width(20f)) ? 1 : 0) != 0;
            }
            else
            {
              EditorGUI.BeginDisabledGroup(true);
              EditorGUILayout.Toggle(false, GUILayout.Width(20f));
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel = indentLevel;
            if (systemInfo.isActive != isActive && systemInfo.system is IReactiveSystem system10)
            {
              if (systemInfo.isActive)
                system10.Activate();
              else
                system10.Deactivate();
            }
            switch (type)
            {
              case SystemInterfaceFlags.IInitializeSystem:
                EditorGUILayout.LabelField(systemInfo.systemName, systemInfo.initializationDuration.ToString(), this.getSystemStyle(systemInfo, SystemInterfaceFlags.IInitializeSystem));
                break;
              case SystemInterfaceFlags.IExecuteSystem:
                string str1 = string.Format("Ø {0:00.000}", (object) systemInfo.averageExecutionDuration).PadRight(12);
                string str2 = string.Format("▼ {0:00.000}", (object) systemInfo.minExecutionDuration).PadRight(12);
                string str3 = string.Format("▲ {0:00.000}", (object) systemInfo.maxExecutionDuration);
                EditorGUILayout.LabelField(systemInfo.systemName, str1 + str2 + str3, this.getSystemStyle(systemInfo, SystemInterfaceFlags.IExecuteSystem));
                break;
              case SystemInterfaceFlags.ICleanupSystem:
                string str4 = string.Format("Ø {0:00.000}", (object) systemInfo.averageCleanupDuration).PadRight(12);
                string str5 = string.Format("▼ {0:00.000}", (object) systemInfo.minCleanupDuration).PadRight(12);
                string str6 = string.Format("▲ {0:00.000}", (object) systemInfo.maxCleanupDuration);
                EditorGUILayout.LabelField(systemInfo.systemName, str4 + str5 + str6, this.getSystemStyle(systemInfo, SystemInterfaceFlags.ICleanupSystem));
                break;
              case SystemInterfaceFlags.ITearDownSystem:
                EditorGUILayout.LabelField(systemInfo.systemName, systemInfo.teardownDuration.ToString(), this.getSystemStyle(systemInfo, SystemInterfaceFlags.ITearDownSystem));
                break;
            }
            EditorGUILayout.EndHorizontal();
            ++num;
          }
          if (systemInfo.system is CustomDebugSystems system11)
          {
            int indentLevel = EditorGUI.indentLevel;
            ++EditorGUI.indentLevel;
            num += this.drawSystemInfos(system11, type);
            EditorGUI.indentLevel = indentLevel;
          }
        }
      }
      return num;
    }

    private static CustomSystemInfo[] getSortedSystemInfos(
      CustomSystemInfo[] systemInfos,
      CustomDebugSystemsInspector.SortMethod sortMethod)
    {
      switch (sortMethod)
      {
        case CustomDebugSystemsInspector.SortMethod.Name:
          return ((IEnumerable<CustomSystemInfo>) systemInfos).OrderBy<CustomSystemInfo, string>((Func<CustomSystemInfo, string>) (systemInfo => systemInfo.systemName)).ToArray<CustomSystemInfo>();
        case CustomDebugSystemsInspector.SortMethod.NameDescending:
          return ((IEnumerable<CustomSystemInfo>) systemInfos).OrderByDescending<CustomSystemInfo, string>((Func<CustomSystemInfo, string>) (systemInfo => systemInfo.systemName)).ToArray<CustomSystemInfo>();
        case CustomDebugSystemsInspector.SortMethod.ExecutionTime:
          return ((IEnumerable<CustomSystemInfo>) systemInfos).OrderBy<CustomSystemInfo, double>((Func<CustomSystemInfo, double>) (systemInfo => systemInfo.averageExecutionDuration)).ToArray<CustomSystemInfo>();
        case CustomDebugSystemsInspector.SortMethod.ExecutionTimeDescending:
          return ((IEnumerable<CustomSystemInfo>) systemInfos).OrderByDescending<CustomSystemInfo, double>((Func<CustomSystemInfo, double>) (systemInfo => systemInfo.averageExecutionDuration)).ToArray<CustomSystemInfo>();
        default:
          return systemInfos;
      }
    }

    private static bool shouldShowSystems(CustomDebugSystems systems, SystemInterfaceFlags type)
    {
      if (!CustomDebugSystemsInspector._hideEmptySystems)
        return true;
      switch (type)
      {
        case SystemInterfaceFlags.IInitializeSystem:
          return systems.totalInitializeSystemsCount > 0;
        case SystemInterfaceFlags.IExecuteSystem:
          return systems.totalExecuteSystemsCount > 0;
        case SystemInterfaceFlags.ICleanupSystem:
          return systems.totalCleanupSystemsCount > 0;
        case SystemInterfaceFlags.ITearDownSystem:
          return systems.totalTearDownSystemsCount > 0;
        default:
          return true;
      }
    }

    private GUIStyle getSystemStyle(CustomSystemInfo systemInfo, SystemInterfaceFlags systemFlag)
    {
      GUIStyle guiStyle = new GUIStyle(GUI.skin.label);
      Color color = !systemInfo.isReactiveSystems || !EditorGUIUtility.isProSkin ? guiStyle.normal.textColor : Color.white;
      if (systemFlag == SystemInterfaceFlags.IExecuteSystem && systemInfo.averageExecutionDuration >= (double) this._systemWarningThreshold)
        color = Color.red;
      if (systemFlag == SystemInterfaceFlags.ICleanupSystem && systemInfo.averageCleanupDuration >= (double) this._systemWarningThreshold)
        color = Color.red;
      guiStyle.normal.textColor = color;
      return guiStyle;
    }

    private void addDuration(float duration)
    {
      if (Time.renderedFrameCount == this._lastRenderedFrameCount)
        return;
      this._lastRenderedFrameCount = Time.renderedFrameCount;
      if (this._systemMonitorData.Count >= 60)
      {
        double num = (double) this._systemMonitorData.Dequeue();
      }
      this._systemMonitorData.Enqueue(duration);
    }

    private enum SortMethod
    {
      OrderOfOccurrence,
      Name,
      NameDescending,
      ExecutionTime,
      ExecutionTimeDescending,
    }
  }
}
