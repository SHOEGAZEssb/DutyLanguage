﻿using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using DutyLanguage.Windows;
using ECommons;
using ECommons.DalamudServices;
using Dalamud.Game.Config;
using System;

namespace DutyLanguage;

public sealed class Plugin : IDalamudPlugin
{
  [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
  [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;

  private const string CommandName = "/dlang";

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
  public static Configuration Configuration { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

  public readonly WindowSystem WindowSystem = new("DutyLanguage");
  private ConfigWindow ConfigWindow { get; init; }

  public Plugin()
  {
    ECommonsMain.Init(PluginInterface, this);

    Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
    Configuration.Setup();

    ConfigWindow = new ConfigWindow();

    WindowSystem.AddWindow(ConfigWindow);

    CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
    {
      HelpMessage = "Shows the DutyLanguage configuration window"
    });

    PluginInterface.UiBuilder.Draw += DrawUI;
    PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
    PluginInterface.UiBuilder.OpenMainUi += ToggleConfigUI;

    Svc.ClientState.TerritoryChanged += ClientState_TerritoryChanged;
    Svc.ClientState.Login += ClientState_Login;
  }

  public void Dispose()
  {
    Svc.ClientState.TerritoryChanged -= ClientState_TerritoryChanged;
    Svc.ClientState.Login -= ClientState_Login;
    WindowSystem.RemoveAllWindows();
    CommandManager.RemoveHandler(CommandName);
    ECommonsMain.Dispose();
  }

  private void OnCommand(string command, string args)
  {
    ToggleConfigUI();
  }

  private void DrawUI() => WindowSystem.Draw();

  public void ToggleConfigUI() => ConfigWindow.Toggle();

  private void ClientState_TerritoryChanged(ushort e)
  {
    if (Configuration.Duties.TryGetValue(e, out DutyLanguageSetting val))
    {
      uint setting;
      if (val == DutyLanguageSetting.Random || Configuration.Randomize)
      {
        var rng = new Random(DateTime.Now.Ticks.GetHashCode());
        setting = (uint)rng.Next(0, 4);
      }
      else if (val == DutyLanguageSetting.Default)
        return;
      else
        setting = (uint)val;

      Svc.GameConfig.Set(SystemConfigOption.CutsceneMovieVoice, setting);
      Svc.Log.Info($"Duty started, setting language to {setting}");
    }
    else
    {
      Svc.GameConfig.Set(SystemConfigOption.CutsceneMovieVoice, (uint)Configuration.DefaultLanguage);
      Svc.Log.Info($"Resetting language to default language ({(uint)Configuration.DefaultLanguage})");
    }
  }

  private void ClientState_Login()
  {
    Svc.GameConfig.Set(SystemConfigOption.CutsceneMovieVoice, (uint)Configuration.DefaultLanguage);
    Svc.Log.Info($"Logging in and resetting language to default language ({(uint)Configuration.DefaultLanguage})");
  }
}