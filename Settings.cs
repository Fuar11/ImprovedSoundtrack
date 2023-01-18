using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ModSettings;

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

        [Name("Pleasant Valley Explore Music")]
        [Description("The Pleasant Valley track from Episode 3 will play at random when exploring during the daytime in Pleasant Valley.")]
        [Choice("Yes", "No")]
        public bool thePleasantValleyEnabled = true;

        [Section("Location Discovered Stingers")]

        [Name("Enable Stingers")]
        [Description("Enable or Disable new location discovered music.")]
        [Choice("Disabled", "Enabled")]
        public bool locationStingers = true;

        [Name("Location Discovered Stinger")]
        [Description("The stinger to play when you discover a new location.")]
        [Choice("Finder", "Shelter", "Shelter2", "Random")]
        public int chosenStinger = 3;

       
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

            SetFieldVisible(nameof(thePleasantValleyEnabled), Settings.settings.active != Active.Disabled);

            SetFieldVisible(nameof(locationStingers), Settings.settings.active != Active.Disabled);

            SetFieldVisible(nameof(chosenStinger), Settings.settings.active != Active.Disabled && locationStingers);

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
