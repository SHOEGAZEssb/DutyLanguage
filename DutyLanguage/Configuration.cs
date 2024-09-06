using Dalamud.Configuration;
using ECommons.DalamudServices;
using Lumina.Excel.GeneratedSheets2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DutyLanguage;

/// <summary>
/// Language configuration for a duty.
/// </summary>
public enum DutyLanguageSetting
{
  Default = -1,
  Japanese = 0,
  English = 1,
  German = 2,
  French = 3,
  Random = 4
}

/// <summary>
/// Available language settings for the game settings.
/// </summary>
public enum Language : uint
{
  Client = uint.MaxValue,
  Japanese = 0,
  English = 1,
  German = 2,
  French = 3,
}

/// <summary>
/// Configuration for the plugin.
/// </summary>
[Serializable]
public class Configuration : IPluginConfiguration
{
  /// <summary>
  /// <inheritdoc/>
  /// </summary>
  public int Version { get; set; } = 0;

  /// <summary>
  /// The language to reset to when entering a territory
  /// that has no configured language, or on login.
  /// </summary>
  public Language DefaultLanguage { get; set; } = Language.Client;

  /// <summary>
  /// If checked, always sets a random language when entering a supported duty.
  /// Ignores per-duty settings.
  /// </summary>
  public bool Randomize { get; set; } = false;

  /// <summary>
  /// List of all duties whose language can be changed.
  /// Key is the ID of the territory.
  /// </summary>
  public Dictionary<uint, DutyLanguageSetting> Duties { get; private set; } = [];

  /// <summary>
  /// List of territory ids (duties) that should
  /// be excluded because they do not have voice acting.
  /// </summary>
  private readonly uint[] _filteredDuties =
  [
    1039, // totorak
    1037, // tamtara
    1038, // copperbell
    1036, // sastasha
    172, // aurum vale
    1040, // haukke manor
    162, // halatali
    1041, // brayflox longstop
    163, // qarn
    159, // wanderers palace
    1042, // stone vigil
    170, // cutters cry
    171, // dzemael darkhold
    167, // amdapor keep
    160, // pharos sirius
    349, // copperbell hard
    350, // haukke manor hard
    362, // brayflox longstop hard
    360, // halatali hard
    363, // lost city of amdapor
    361, // hullbreaker isle
    373, // tamtara hard
    365, // stone vigil hard
    367, // qarn hard
    1062, // snowcloak
    387, // sastasha hard
    189, // amdapor keep hard
    188, // wanderers palace hard
    1109, // gubal library
    1063, // keeper of the lake ???
    420, // neverreap
    430, // fractal continuum ???
    434, // dusk vigil
    1064, // sohm al
    1110, // aetherochemical research facility
    1064, // the aery
    510, // pharos sirius hard
    511, // saint mociannes arboretum ???
    1045, // bowl of embers
    1046, // the navel
    1047, // howling eye
    292, // bowl of embers hard
    293, // the navel hard
    294, // howling eye hard
    1067, // thornmarch hard
    364, // thornmarch extreme
    348, // ministrels ballad ultimas bane ???
    353, // special event 1
    354, // sepcial event 2
    281, // whorleater hard
    359, // whorleater extreme
    368, // relic reborn chimera
    369, // relic reborn hydra
    374, // striking tree hard
    375, // striking tree extreme
    377, // akh afah amphithreatre hard
    378, // akh afah amphitheatre extreme
    142, // dragons neck
    394, // urths fount
    426, // the chrysalis
    436, // limitless blue hard
    447, // limitless blue extreme
    174, // labyrinth of the ancients ???
    241, // coil of bahamut 1
    242, // coil of bahamut 2
    243, // coil of bahamut 3
    244, // coil of bahamut 4
    245, // coil of bahamut 5
    355, // 2 coil of bahamut 1
    356, // 2 coil of bahamut 2
    357, // 2 coil of bahamut 3
    358, // 2 coil of bahamut 4
    372, // syrcus tower,
    380, // 2 coil of bahamut 1 savage
    381, // 2 coil of bahamut 2 savage
    382, // 2 coil of bahamut 3 savage
    383, // 2 coil of bahamut 4 savage
    193, // final coil of bahamut 1
    194, // final coil of bahamut 2
    195, // final coil of bahamut 3
    196, // final coil of bahamut 4
    295, // bowl of embers extreme
    296, // the navel extreme
    297, // howling eye extreme
    151, // world of darkness
    442, // alexander fist of the father
    443, // alexander cuff of the father
    444, // alexander arm of the father
    445, // alexander burden of the father
    449, // alexander fist of the father savage
    450, // alexander cuff of the father savage
    451, // alexander arm of the father savage
    452, // alexander burden of the father savage
    508, // void ark
    517, // containment bay s1t7 ???
    524, // containment bay s1t7 extreme
    520, // alexander fist of the son
    521, // alexander cuff of the son
    522, // alexander arm of the son
    523, // alexander burden of the son
    519, // lost city of amdapor hard
    1111, // antitower
    529, // alexander fist of the son savage
    530, // alexander cuff of the son savage
    531, // alexander arm of the son savage
    532, // alexander burden of the son savage
    556, // weeping city of mhach
    559, // final steps of faith
    566, // ministrels ballad nidhoggs rage
    1112, // sohr khai
    557, // hullbreaker isle hard
    1113, // xelphatol
    580, // alexander eyes of the creator
    581, // alexander breath of the creator
    582, // alexander heart of the creator
    583, // alexander soul of the creator
    584, // alexander eyes of the creator savage
    585, // alexander breath of the creator savage
    586, // alexander heart of the creator savage
    587, // alexander sould of the creator savage
    578, // great gubal library hard
    627, // dun scaith
    617, // sohm al hard
    616, // shisui of the violet tides
    663, // temple of the fist
    1142, // sirensong sea
    679, // royal menagerie
    1142, // bardams mettle
    1145, // castrum abania
    691, // deltascape 1
    692, // deltascape 2
    693, // deltascape 3
    694, // deltascape 4
    695, // deltascape 1 savage
    696, // deltascape 2 savage
    697, // deltascape 3 savage
    698, // deltascape 4 savage
    662, // kugane castle
    730, // ministrels ballad shinryus domain
    1172, // drowned city of skalla
    733, // unending coil of bahamut ultimate ???
    742, // hells lid
    743, // fractal continuum hard
    748, // sigmascape 1
    749, // sigmascape 2
    750, // sigmascape 3
    751, // sigmascape 4
    752, // sigmascape 1 savage
    749, // sigmascape 2 savage
    750, // sigmascape 3 savage
    751, // sigmascape 4 savage
    761, // great hunt
    762, // great hunt extreme
    768, // swallows compass
    777, // weapons refrain ultimate
    788, // saint mociannes arboretum hard
    1173, // the burn
    798, // alphascape 1
    799, // alphascape 2
    800, // alphascape 3
    801, // alphascape 4
    802, // alphascape 1 savage
    803, // alphascape 2 savage
    804, // alphascape 3 savage
    804, // alphascape 4 savage
    806, // kugane ohashi
    821, // dohn mheg
    823, // qitana ravel
    849, // edens gate resurrection
    853, // edens gate resurrection savage
    840, // the twinning
    836, // malikahs well
    841, // akadaemia anyder
    837, // holminister switch
    851, // edens gate inundation
    855, // edens gate inundation savage
    850, // edens gate descent
    854, // edens gate descent savage
    852, // edens gate sepulture
    856, // edens gate sepulture savage
    887, // epic of alexander ultimate ???
    //882, // the copied factory ???
    898, // anamnesis anyder,
    902, // edens verse fulmination
    906, // edens verse fulmination savage
    897, // cinder drift
    912, // cinder drift extreme
    903, // edens verse furor
    907, // edens verse furor extreme
    913, // memoria misera extreme ???
    904, // edens verse iconoclasm
    908, // edens verse iconoclasm savage
    916, // heroes gauntlet,
    933, // matoyas relict ???
    943, // edens promise litany
    947, // edens promise litany savage
    942, // edens promise umbra
    946, // edens promise umbra savage
    934, // castrum marinum
    935, // castrum marinum extreme
    950, // cloud deck
    951, // cloud deck extreme
    952, // tower of zot,
    986, // stigma dreamscape
    969, // tower of babil
    968, // dragonsongs reprise ultimate ???
    976, // smileton
    1008, // asphedolos fourth circle
    1009, // asphedolos fourth circle savage
    1006, // asphedolos third circle
    1007, // asphedolos third circle savage
    1002, // asphedolos first circle
    1003, // asphedolos first circle savage
    1004, // asphedolos second circle
    1005, // asphedolos second circle savage
    1126, // aetherfont
    1208, // origenics
    1167, // ihuykatumu
    1194, // skydeep cenote
    1198, // vanguard
    1195, // worqor lar dor
    1196, // worqor lar dor extreme
    1203, // tender valley
    1050, // alzadaals legacy
    1081, // abyssos fifth circle
    1082, // abyssos fifth circle savage
    1085, // abyssos seventh circle
    1086, // abyssos seventh circle savage
    1083, // abyssos sixth circle
    1084, // abyssos sixth circle savage
    1122, // omega protocol ultimate ???
    1149, // anabaseios tenth circle
    1150, // anabaseios tenth circle savage
    1168, // abyssal fracture
    1169, // abyssal fracture extreme
    1204, // strayborough deadwalk
  ];

  /// <summary>
  /// List of valid ContentType ids for filtering duties.
  /// </summary>
  private readonly uint[] _validContentTypeIDs =
  [
    2, // dungeons
    4, // trials
    5, // raids
    28, // ultimate raids
    30 // V&C
  ];

  /// <summary>
  /// Saves the configuration to file.
  /// </summary>
  public void Save()
  {
    Plugin.PluginInterface.SavePluginConfig(this);
  }

  /// <summary>
  /// Setups the configuration object.
  /// Is used to add duties for the first time.
  /// </summary>
  public void Setup()
  {
    var allDuties = Svc.Data.GameData.GetExcelSheet<ContentFinderCondition>()?.ToArray() ?? [];
    foreach (var duty in allDuties.Where(d => _validContentTypeIDs.Contains(d.ContentType.Value?.RowId ?? 0)))
    {
      if (duty.TerritoryType.Value == null)
        continue;

      Duties.TryAdd(duty.TerritoryType.Value.RowId, DutyLanguageSetting.Default);
    }

    Duties = Duties.Where(kvp => !_filteredDuties.Contains(kvp.Key)).OrderBy(kvp => kvp.Key).ToDictionary();
  }
}
