using System.Collections.Generic;
using Celeste.Mod.DJMapHelper.Entities;
using Microsoft.Xna.Framework;
using YamlDotNet.Serialization;

namespace Celeste.Mod.DJMapHelper; 

public class DJMapHelperSession : EverestModuleSession {
    public bool? LastNoRefills { get; set; } = null;
    public Color? LastOverrideHairColor { get; set; } = null;
    public BadelineProtectorConfig BadelineProtectorConfig { get; set; }
    public bool DefeatedBoss { get; set; } = false;
    
    [YamlIgnore]
    public readonly Dictionary<CrystalStaticSpinner, CrystalColor> CachedSpinnerColors = new();
}