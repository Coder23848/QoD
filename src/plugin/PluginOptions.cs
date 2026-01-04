using Menu.Remix.MixedUI;
using System.Collections.Generic;
using UnityEngine;

namespace QoD
{
    public class PluginOptions : OptionInterface
    {
        public static PluginOptions Instance = new();

        // Misc
        public static Configurable<float> FixedDynamicDifficulty = Instance.config.Bind("FixedDynamicDifficulty", 1f, new ConfigurableInfo("The fixed dynamic difficulty."));
        public static Configurable<bool> UseFixedDynamicDifficulty = Instance.config.Bind("UseFixedDynamicDifficulty", false, new ConfigurableInfo("Sets the dynamic difficulty to a fixed value."));
        public static Configurable<bool> NoSurvivorLeniency = Instance.config.Bind("NoSurvivorLeniency", false, new ConfigurableInfo("Removes certain subtle leniency mechanics exclusive to the Survivor's and Monk's campaigns."));
        public static Configurable<bool> AlwaysFloodPrecycles = Instance.config.Bind("AlwaysFloodPrecycles", false, new ConfigurableInfo("When enabled, pre-cycle rain will always cause flooding."));
        public static Configurable<bool> StrongerBarnacles = Instance.config.Bind("StrongerBarnacles", false, new ConfigurableInfo("Makes barnacles harder to deal with."));
        public static Configurable<bool> AudibleGooieducks = Instance.config.Bind("AudibleGooieducks", false, new ConfigurableInfo("Allows other creatures to hear gooieducks' popping sounds."));
        public static Configurable<bool> AudibleSpearmaster = Instance.config.Bind("AudibleSpearmaster", false, new ConfigurableInfo("Allows other creatures to hear the Spearmaster's spear-picking sounds."));

        // LessUI
        public static Configurable<bool> RemoveKillFeed = Instance.config.Bind("RemoveKillFeed", false, new ConfigurableInfo("Removes the kill feed from the shelter screen."));
        public static Configurable<bool> RemoveTokenTracker = Instance.config.Bind("RemoveTokenTracker", false, new ConfigurableInfo("Removes the token tracker from the shelter screen."));

        // NoMap
        public static Configurable<bool> NoMap = Instance.config.Bind("NoMap", false, new ConfigurableInfo("Removes the map."));
        //public static Configurable<bool> NoWatcherMap = Instance.config.Bind("NoWatcherMap", false, new ConfigurableInfo("Removes the Watcher's region map."));
        public static Configurable<bool> NoWatcherGoldRings = Instance.config.Bind("NoWatcherGoldRings", false, new ConfigurableInfo("Removes the golden rings on the Watcher's region map."));
        public static Configurable<bool> NoWatcherPurpleRings = Instance.config.Bind("NoWatcherPurpleRings", false, new ConfigurableInfo("Removes the purple rings on the Watcher's region map."));
        public static Configurable<bool> NoWatcherFeathers = Instance.config.Bind("NoWatcherFeathers", false, new ConfigurableInfo("Removes the feathers on the Watcher's region map."));
        public static Configurable<bool> NoWatcherConnections = Instance.config.Bind("NoWatcherConnections", false, new ConfigurableInfo("Removes the region connections on the Watcher's region map."));
        public static Configurable<bool> NoWatcherWarps = Instance.config.Bind("NoWatcherWarps", false, new ConfigurableInfo("Removes the warp points on the Watcher's map."));

        // SmarterCritters
        public static Configurable<bool> LizardPatience = Instance.config.Bind("LizardPatience", false, new ConfigurableInfo("When enabled, lizards that rely on stealth are more patient."));
        public static Configurable<bool> DropwigPitAvoidance = Instance.config.Bind("DropwigPitAvoidance", false, new ConfigurableInfo("When enabled, dropwigs avoid setting up ambushes over bottomless pits."));
        public static Configurable<bool> LizardsUnderstandSlugcatCombat = Instance.config.Bind("LizardsUnderstandSlugcatCombat", false, new ConfigurableInfo("When enabled, lizards are capable of strategizing around your weapons."));

        // NoIteratorKarma
        public static Configurable<bool> NoIteratorKarma = Instance.config.Bind("NoIteratorKarma", false, new ConfigurableInfo("When enabled, iterators are incapable of raising your maximum karma level."));

        // ConsistentCycles
        public static Configurable<bool> ConsistentCycles = Instance.config.Bind("ConsistentCycles", false, new ConfigurableInfo("When enabled, certain randomized properties of a cycle (such as its length) will not be rerandomized when you die."));

        // Glow Nerf
        public const int MAX_FADE_TIME = 1000000;
        public static Configurable<float> IntensityMultiplier = Instance.config.Bind("IntensityMultiplier", 1f, new ConfigurableInfo("The intensity of the glow effect; 0 is no light, 1 is the normal amount."));
        public static Configurable<float> LanternIntensityMultiplier = Instance.config.Bind("LanternIntensityMultiplier", 1f, new ConfigurableInfo("The intensity of lantern light; 0 is no light, 1 is the normal amount."));
        public static Configurable<bool> GlowFades = Instance.config.Bind("GlowFades", true, new ConfigurableInfo("Causes the glow effect to disappear over time."));
        public static Configurable<bool> LanternsFade = Instance.config.Bind("LanternsFade", true, new ConfigurableInfo("Causes lanterns to lose their light over time."));
        public static Configurable<int> GlowFadeTime = Instance.config.Bind("GlowFadeTime", 10, new ConfigurableInfo("The number of cycles it takes to lose the glow effect.", new ConfigAcceptableRange<int>(1, MAX_FADE_TIME)));
        public static Configurable<int> LanternFadeTime = Instance.config.Bind("LanternFadeTime", 20, new ConfigurableInfo("The number of cycles it takes for a lantern to die out.", new ConfigAcceptableRange<int>(1, MAX_FADE_TIME)));
        public static Configurable<bool> GlowNerfOn = Instance.config.Bind("GlowNerfOn", false, new ConfigurableInfo("Enables glow nerfing features."));
        public static Configurable<bool> LanternNerfOn = Instance.config.Bind("LanternNerfOn", false, new ConfigurableInfo("Enables lantern nerfing features."));

        List<UIelement> GlowNerfUIelements = new();
        private void UpdateGlowNerfUIelements(bool show)
        {
            foreach (UIelement i in GlowNerfUIelements)
            {
                if (show)
                {
                    i.Show();
                }
                else
                {
                    i.Hide();
                }
            }
        }
        private void GlowNerfOnCheckBox_OnValueUpdate(UIconfig config, string value, string oldValue)
        {
            UpdateGlowNerfUIelements(bool.Parse(value));
        }


        List<UIelement> LanternNerfUIelements = new();
        private void UpdateLanternNerfUIelements(bool show)
        {
            foreach (UIelement i in LanternNerfUIelements)
            {
                if (show)
                {
                    i.Show();
                }
                else
                {
                    i.Hide();
                }
            }
        }
        private void LanternNerfOnCheckBox_OnValueUpdate(UIconfig config, string value, string oldValue)
        {
            UpdateLanternNerfUIelements(bool.Parse(value));
        }

        public override void Initialize()
        {
            base.Initialize();
            Tabs = new OpTab[3];

            OpTab MiscTab = new(Instance, "General");
            
            // SmarterCritters
            CheckBoxOption(MiscTab, LizardPatience, 0, "Patient Lizards");
            CheckBoxOption(MiscTab, DropwigPitAvoidance, 1, "Dropwig Pit Avoidance");
            CheckBoxOption(MiscTab, LizardsUnderstandSlugcatCombat, 2, "Lizards Understand Slugcat Combat");
            // NoIteratorKarma
            CheckBoxOption(MiscTab, NoIteratorKarma, 3, "No Karma From Iterators");
            // ConsistentCycles
            CheckBoxOption(MiscTab, ConsistentCycles, 4, "Cycle Consistency");
            // Misc
            MiscTab.AddItems(new OpCheckBox(UseFixedDynamicDifficulty, new(50, 550 - 5 * 30)) { description = UseFixedDynamicDifficulty.info.description }, new OpFloatSlider(FixedDynamicDifficulty, new(90, 550 - 5 * 30 - 5), 100) { description = FixedDynamicDifficulty.info.description, min = -1f, max = 1f }, new OpLabel(new Vector2(110 + 100, 550 - 5 * 30), new Vector2(), "Fixed Dynamic Difficulty", FLabelAlignment.Left));
            CheckBoxOption(MiscTab, NoSurvivorLeniency, 6, "Remove Survivor/Monk Leniency Mechanics");
            CheckBoxOption(MiscTab, AlwaysFloodPrecycles, 7, "Precycles Always Flood");
            CheckBoxOption(MiscTab, AudibleGooieducks, 8, "Audible Gooieducks");
            CheckBoxOption(MiscTab, AudibleSpearmaster, 9, "Audible Spearmaster Spears");

            if (ModManager.Watcher)
            {
                CheckBoxOption(MiscTab, StrongerBarnacles, 10, "Stronger Barnacles");
            }

            Tabs[0] = MiscTab;

            

            OpTab GlowNerfTab = new(Instance, "Light Nerfs");

            int uiy = 520;

            // GlowNerf
            OpCheckBox GlowNerfOnCheckBox = new OpCheckBox(GlowNerfOn, new(50, uiy)) { description = GlowNerfOn.info.description };
            GlowNerfTab.AddItems(
                GlowNerfOnCheckBox,
                new OpLabel(new Vector2(90, uiy), new Vector2(), "Nerf Neuron Glow", FLabelAlignment.Left));
            uiy -= 45;
            GlowNerfUIelements.Clear();
            GlowNerfUIelements.Add(new OpFloatSlider(IntensityMultiplier, new(50, uiy - 5), 200) { description = IntensityMultiplier.info.description });
            GlowNerfUIelements.Add(new OpLabel(new Vector2(60 + 200, uiy), new Vector2(), "Glow Intensity", FLabelAlignment.Left));
            uiy -= 30;
            GlowNerfUIelements.Add(new OpCheckBox(GlowFades, new(50, uiy)) { description = GlowFades.info.description });
            GlowNerfUIelements.Add(new OpLabel(new Vector2(90, uiy), new Vector2(), "Glow fades over ", FLabelAlignment.Left));
            GlowNerfUIelements.Add(new OpUpdown(GlowFadeTime, new(150 + 35, uiy - 5), 50) { description = GlowFadeTime.info.description });
            GlowNerfUIelements.Add(new OpLabel(new Vector2(205 + 35, uiy), new Vector2(), "cycles", FLabelAlignment.Left));
            
            GlowNerfOnCheckBox.OnValueUpdate += GlowNerfOnCheckBox_OnValueUpdate;
            UpdateGlowNerfUIelements(GlowNerfOn.Value);

            GlowNerfTab.AddItems(GlowNerfUIelements.ToArray());

            uiy -= 60;

            // LanternNerf
            OpCheckBox LanternNerfOnCheckBox = new OpCheckBox(LanternNerfOn, new(50, uiy)) { description = LanternNerfOn.info.description };
            GlowNerfTab.AddItems(
                LanternNerfOnCheckBox,
                new OpLabel(new Vector2(90, uiy), new Vector2(), "Nerf Lanterns", FLabelAlignment.Left));
            uiy -= 45;
            LanternNerfUIelements.Clear();
            LanternNerfUIelements.Add(new OpFloatSlider(LanternIntensityMultiplier, new(50, uiy - 5), 200) { description = LanternIntensityMultiplier.info.description });
            LanternNerfUIelements.Add(new OpLabel(new Vector2(60 + 200, uiy), new Vector2(), "Lantern Intensity", FLabelAlignment.Left));
            uiy -= 30;
            LanternNerfUIelements.Add(new OpCheckBox(LanternsFade, new(50, uiy)) { description = LanternsFade.info.description });
            LanternNerfUIelements.Add(new OpLabel(new Vector2(90, uiy), new Vector2(), "Lanterns fade over ", FLabelAlignment.Left));
            LanternNerfUIelements.Add(new OpUpdown(LanternFadeTime, new(150 + 49, uiy - 5), 50) { description = LanternFadeTime.info.description });
            LanternNerfUIelements.Add(new OpLabel(new Vector2(205 + 49, uiy), new Vector2(), "cycles", FLabelAlignment.Left));

            LanternNerfOnCheckBox.OnValueUpdate += LanternNerfOnCheckBox_OnValueUpdate;
            UpdateLanternNerfUIelements(LanternNerfOn.Value);

            GlowNerfTab.AddItems(LanternNerfUIelements.ToArray());

            Tabs[1] = GlowNerfTab;

            OpTab UIRemovalTab = new(Instance, "UI Removal");

            // LessUI
            CheckBoxOption(UIRemovalTab, RemoveKillFeed, 0, "Remove Kill Feed");
            CheckBoxOption(UIRemovalTab, RemoveTokenTracker, 1, "Remove Token Tracker");
            // NoMap
            CheckBoxOption(UIRemovalTab, NoMap, 2, "Remove Map");

            if (ModManager.Watcher)
            {
                //CheckBoxOption(UIRemovalTab, NoWatcherMap, 3, "Remove The Watcher's Region Map");
                CheckBoxOption(UIRemovalTab, NoWatcherGoldRings, 3, "Remove The Watcher's Gold Ring Indicators");
                CheckBoxOption(UIRemovalTab, NoWatcherPurpleRings, 4, "Remove The Watcher's Purple Ring Indicators");
                CheckBoxOption(UIRemovalTab, NoWatcherFeathers, 5, "Remove The Watcher's Gold Feather Indicators");
                CheckBoxOption(UIRemovalTab, NoWatcherConnections, 6, "Remove The Watcher's Connection Indicators");
                CheckBoxOption(UIRemovalTab, NoWatcherWarps, 7, "Remove The Watcher's Warp Point Indicators");
            }

            Tabs[2] = UIRemovalTab;
        }

        private void CheckBoxOption(OpTab tab, Configurable<bool> setting, float pos, string label)
        {
            tab.AddItems(new OpCheckBox(setting, new(50, 550 - pos * 30)) { description = setting.info.description }, new OpLabel(new Vector2(90, 550 - pos * 30), new Vector2(), label, FLabelAlignment.Left));
        }
        private void SliderOption(OpTab tab, Configurable<float> setting, int size, float pos, string label)
        {
            tab.AddItems(new OpFloatSlider(setting, new(50, 545 - pos * 30), size) { description = setting.info.description }, new OpLabel(new Vector2(60 + size, 550 - pos * 30), new Vector2(), label, FLabelAlignment.Left));
        }
    }
}