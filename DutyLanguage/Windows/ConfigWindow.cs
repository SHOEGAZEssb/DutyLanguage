using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ECommons;
using ECommons.DalamudServices;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets2;

namespace DutyLanguage.Windows;

public class ConfigWindow : Window
{
  private readonly IEnumerable<ContentFinderCondition> _dutyInfos = Svc.Data.GameData?.GetExcelSheet<ContentFinderCondition>() ?? Enumerable.Empty<ContentFinderCondition>();
  private IEnumerable<ContentFinderCondition> _filteredDutyInfos;
  private string _dutySearchText = string.Empty;

  public ConfigWindow() 
    : base("DutyLanguage Configuration")
  {
    _filteredDutyInfos = _dutyInfos;
  }

  public override void Draw()
  {
    ImGui.BeginGroup();
    ImGui.Text("Default Language:");
    ImGui.SameLine();

    var languageEnumValues = Enum.GetValues<Language>();
    var languageEnumStrings = languageEnumValues.Select(x => x.ToString()).ToArray();
    var languageIndex = Array.IndexOf(languageEnumValues, Plugin.Configuration.DefaultLanguage);
    if (ImGui.Combo("###defaultLanguageCombo", ref languageIndex, languageEnumStrings, languageEnumStrings.Length))
    {
      Plugin.Configuration.DefaultLanguage = (Language)languageIndex;
      Plugin.Configuration.Save();
    }
    ImGui.EndGroup();
    if (ImGui.IsItemHovered())
    {
      ImGui.BeginTooltip();
      ImGui.SetTooltip("Default language to reset to when finishing a duty or logging in");
      ImGui.EndTooltip();
    }

    bool randomize = Plugin.Configuration.Randomize;
    if (ImGui.Checkbox("Randomize", ref randomize))
    {
      Plugin.Configuration.Randomize = randomize;
      Plugin.Configuration.Save();
    }
    if (ImGui.IsItemHovered())
    {
      ImGui.BeginTooltip();
      ImGui.SetTooltip("If checked, always sets a random language when entering a supported duty.\r\nIgnores per-duty settings.");
      ImGui.EndTooltip();
    }

    ImGui.Separator();

    ImGui.Text("Search");
    ImGui.SameLine();
    if (ImGui.InputText("##dutyFilterText", ref _dutySearchText, 128))
      _filteredDutyInfos = string.IsNullOrEmpty(_dutySearchText) ? _dutyInfos : _dutyInfos.Where(a => a.Name.RawString.Contains(_dutySearchText, StringComparison.CurrentCultureIgnoreCase));

    var dutyLanguageEnumValues = Enum.GetValues<DutyLanguageSetting>();
    var dutyLanguageEnumStrings = dutyLanguageEnumValues.Select(x => x.ToString()).ToArray();
    var dutyLanguageCount = dutyLanguageEnumStrings.Length;
    var tableFlags = ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable
                   | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.NoBordersInBody
                   | ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY
                   | ImGuiTableFlags.SizingFixedFit;
    if (ImGui.BeginTable("##dutiesTable", 3, tableFlags))
    {
      ImGui.TableSetupColumn("ID");
      ImGui.TableSetupColumn("Duty Name");
      ImGui.TableSetupColumn("Language Setting");
      ImGui.TableSetupScrollFreeze(0, 1);
      ImGui.TableHeadersRow();

      foreach (var duty in _filteredDutyInfos)
      {
        if (duty == null || duty.TerritoryType.Value == null) 
          continue;

        var configuredDuty = Plugin.Configuration.Duties.FirstOrDefault(d => d.Key == duty.TerritoryType.Row);
        if (configuredDuty.Key == 0)
          continue;

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text(configuredDuty.Key.ToString());

        ImGui.TableNextColumn();
        if (configuredDuty.Value == DutyLanguageSetting.Default)
          ImGui.Text(FixName(duty.Name.ExtractText()));
        else
          ImGui.TextColored(new Vector4(0, 255, 0, 255), duty.Name.ExtractText());

        ImGui.TableNextColumn();
        var index = Array.IndexOf(dutyLanguageEnumValues, configuredDuty.Value);
        if (ImGui.Combo($"###{duty.Name}language", ref index, dutyLanguageEnumStrings, dutyLanguageCount))
        {
          Plugin.Configuration.Duties[configuredDuty.Key] = dutyLanguageEnumValues[index];
          Plugin.Configuration.Save();
        }
      }

      ImGui.EndTable();
    }
  }

  private static string FixName(string name)
  {
    return char.ToUpper(name.First()) + name[1..];
  }
}
