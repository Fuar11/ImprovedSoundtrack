using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ModSettings;
using static Panel_Confirmation;
using static TintMaterials;

namespace ImprovedSoundtrack
{
    public enum Active
    {
        Disabled, Enabled
    }

    internal class CustomSettings : JsonModSettings 
    {
        [Section("Mod Options")]

        [Name("Mod Active")]
        [Description("Enable or Disable this mod")]
        [Choice("Disabled", "Enabled")]
        public Active active = Active.Enabled;

        [Section("Ambient Tracks")]
        [Name("Time In Between Clearing")]
        [Description("Number of in game hours from last play time that is needed for 'Clearing' to play. Based on unpaused in game time, not in game passed time.")]
        [Slider(1f, 24f, 1)]
        public float timeInBetweenClearing = 6f;

        [Section("Location Discovered Stingers")]

        [Name("Enable Stingers")]
        [Description("Enable or Disable new location discovered music.")]
        [Choice("Disabled", "Enabled")]
        public bool locationStingers = true;

        [Name("Location Discovered Stinger")]
        [Description("The stinger to play when you discover a new location.")]
        [Choice("Finder", "Shelter", "Shelter2", "Random")]
        public int chosenStinger = 1;

        [Name("Time In Between Plays")]
        [Description("Number of in game hours from last play time that is needed for stinger to play. Based on unpaused in game time, not in game passed time.")]
        [Slider(0.1f, 1f, 1)]
        public float timeInBetweenStingers = 0.1f;
        protected override void OnChange(FieldInfo field, object oldValue, object newValue)
        {
            if (field.Name == nameof(active) || 
                field.Name == nameof(locationStingers)) 
            {
                RefreshSections();
            }
        }

        internal void RefreshSections()
        {
            SetFieldVisible(nameof(locationStingers), Settings.settings.active != Active.Disabled);

            SetFieldVisible(nameof(chosenStinger), Settings.settings.active != Active.Disabled && locationStingers);

            SetFieldVisible(nameof(timeInBetweenStingers), Settings.settings.active != Active.Disabled && locationStingers);

        }

    }

    internal static class Settings
    {

        public static CustomSettings settings = new CustomSettings();

        public static void onLoad()
        {
            settings.AddToModSettings("Improved Soundtrack", MenuType.Both);
            settings.RefreshSections();
        }


    }


}
