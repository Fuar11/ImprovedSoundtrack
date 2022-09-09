using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using HarmonyLib;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using MelonLoader;

namespace ImprovedSoundtrack
{
    internal class Patches
    {

        public static float m_stormLastPlayTime;
        public static float m_stormMinHoursInBetween = 2f;

        public static float m_FogMinHoursBetween = 6f;
        public static float m_fogLastPlayTime;

        public static float m_trepidationMinHoursInBetween = 12f;

        public static float m_ClearMinHoursBetween = Settings.settings.timeInBetweenClearing;

        public static float m_ClearLastPlayedTime;

        public static float m_LocationDiscoveredHoursBetween = Settings.settings.timeInBetweenStingers;
        public static float m_LocationDiscoveredLastPlayedTime;

        public static float m_TimeToPlayOnSceneLoadFog;
        public static float m_TimeToPlayOnSceneLoadClear;
        public static float m_TimeToPlayOnSceneLoadPV;

        public static float m_PVLastPlayedTime;
        public static float m_PVHoursInBetween = 24f;

        public static uint[] playingIds = new uint[1500];

        public static AudioSource m_OutdoorStingerSource = new AudioSource();
        public static AudioSource m_PleasantValleyAmbient = new AudioSource();
        //public static bool m_isPlayingOutdoorStinger = false;

        [HarmonyPatch(typeof(GameManager), "Awake")]
        internal class GameManager_Awake
        {
            private static void Postfix()
            {
                MelonLoader.MelonLogger.Msg("Improved Soundtracks online!");
                m_TimeToPlayOnSceneLoadFog = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() + 0.005f;
                m_TimeToPlayOnSceneLoadClear = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() + Random.Range(0.005f, 4f);
                m_TimeToPlayOnSceneLoadPV = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() + Random.Range(4f, 24f);
            }

        }

        //Assists with music events
        [HarmonyPatch(typeof(MusicEventManager), "Awake")]
        internal class MusicEventManager_Awake
        {
            private static void Postfix(ref MusicEventManager __instance)
            {
                if (Settings.settings.active == Active.Enabled) {
                    m_stormLastPlayTime = float.NegativeInfinity;
                    m_ClearLastPlayedTime = float.NegativeInfinity;
                    m_PVLastPlayedTime = float.NegativeInfinity;
                    m_LocationDiscoveredLastPlayedTime = float.NegativeInfinity;

                    m_OutdoorStingerSource = __instance.gameObject.AddComponent<AudioSource>();
                    m_PleasantValleyAmbient = __instance.gameObject.AddComponent<AudioSource>();
                }
            }
        }

        //Adds more music events
        [HarmonyPatch(typeof(MusicEventManager), "Update")]
        internal class MusicEventManager_Update
        {
            private static void Postfix(ref MusicEventManager __instance)
            {
                if (Settings.settings.active == Active.Enabled)
                {
                    if (!GameManager.m_IsPaused)
                    {
                        CheckForBlizzard(__instance);
                        CheckForFog(__instance);
                        CheckForClearing(__instance);
                        CheckForPV();
                        m_OutdoorStingerSource.UnPause();
                        m_PleasantValleyAmbient.UnPause();
                    }
                    else
                    {
                        m_OutdoorStingerSource.Pause();
                        m_PleasantValleyAmbient.Pause();
                    }
                }
            }

            private static void CheckForBlizzard(MusicEventManager __instance)
            {
                if (InterfaceManager.IsMainMenuEnabled() || m_PleasantValleyAmbient.isPlaying)
                {
                    return;
                }

                if (GameManager.GetWeatherComponent().GetWeatherStage() == WeatherStage.Blizzard && (GameManager.GetWeatherComponent().IsIndoorEnvironment() || GameManager.GetWeatherComponent().IsIndoorScene()))
                {

                    if (GameManager.GetHypothermiaComponent().HasHypothermiaRisk() || GameManager.GetHypothermiaComponent().HasHypothermia())
                    {

                        float hoursPlayedNotPaused = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
                        if (!(hoursPlayedNotPaused - m_stormLastPlayTime < m_stormMinHoursInBetween))
                        {

                            GameAudioManager.PlayMusic("Play_MusicStorm", ((Component)__instance).gameObject);
                            m_stormLastPlayTime = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
                        }
                    }
                }

            }

            private static void CheckForFog(MusicEventManager __instance)
            {
                if (InterfaceManager.IsMainMenuEnabled() || m_PleasantValleyAmbient.isPlaying)
                {
                    return;
                }

                if(GameManager.GetWeatherComponent().GetWeatherStage() != WeatherStage.DenseFog)
                {
                    return;
                }

                if (!GameManager.GetWeatherComponent().IsIndoorScene() || !GameManager.GetWeatherComponent().IsIndoorEnvironment())
                {

                    float hoursPlayedNotPaused = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
                    if (!(hoursPlayedNotPaused - m_fogLastPlayTime < m_FogMinHoursBetween))
                    {

                        if (GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() > m_TimeToPlayOnSceneLoadFog)
                        {

                            GameAudioManager.PlayMusic("Play_MusicFoggy", ((Component)__instance).gameObject);
                            m_fogLastPlayTime = hoursPlayedNotPaused;
                        }
                    }
                }
            }

            public static void CheckForClearing(MusicEventManager __instance)
            {
                if (InterfaceManager.IsMainMenuEnabled() || m_PleasantValleyAmbient.isPlaying)
                {
                    return;
                }

                if (!GameManager.GetWeatherComponent().IsIndoorScene())
                {
                  if(GameManager.GetWeatherComponent().GetWeatherStage() == WeatherStage.Blizzard || GameManager.GetWeatherComponent().GetWeatherStage() == WeatherStage.DenseFog)
                    {
                        return;
                    }

                  if(GameManager.GetWindComponent().GetStrength() == WindStrength.VeryWindy || GameManager.GetWindComponent().GetStrength() == WindStrength.Windy)
                    {
                        return;
                    }

                  if(GameManager.GetWeatherComponent().GetWeatherStage() == WeatherStage.Clear)
                    {
                        if (!GameManager.GetTimeOfDayComponent().IsDawn())
                        {
                            return;
                        }
                    }

                  if(GameManager.GetTimeOfDayComponent().GetHour() == GameManager.GetTimeOfDayComponent().m_DawnStingerHourStart && GameManager.GetTimeOfDayComponent().GetMinutes() > GameManager.GetTimeOfDayComponent().m_DawnStingerMinutesStart)
                  {
                        return;
                  }
                  else if(GameManager.GetTimeOfDayComponent().GetHour() == GameManager.GetTimeOfDayComponent().m_NightStingerHourStart && GameManager.GetTimeOfDayComponent().GetMinutes() > GameManager.GetTimeOfDayComponent().m_NightStingerMinutesStart)
                  {
                        return;
                  }

                        float hoursPlayedNotPaused = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
                        if (!(hoursPlayedNotPaused - m_ClearLastPlayedTime < m_ClearMinHoursBetween))
                        {
                            if (GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() > m_TimeToPlayOnSceneLoadClear)
                            {
                                GameAudioManager.PlayMusic("Play_MusicClear", ((Component)__instance).gameObject);
                                m_ClearLastPlayedTime = hoursPlayedNotPaused;
                            }
                        }
                }

            }

            public static void CheckForPV()
            {

                if (InterfaceManager.IsMainMenuEnabled() || GameManager.GetWeatherComponent().IsIndoorScene() || (RegionManager.GetCurrentRegion() != GameRegion.RuralRegion))
                {
                    return;
                }

                if (GameManager.GetWeatherComponent().GetWeatherStage() == WeatherStage.Blizzard)
                {
                    return;
                }

                AudioClip track = Implementation.TracksBundle.LoadAsset<AudioClip>("TPV");

                float hoursPlayedNotPaused = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
                if (!(hoursPlayedNotPaused - m_PVLastPlayedTime < m_PVHoursInBetween))
                {

                    if (GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() > m_TimeToPlayOnSceneLoadPV)
                    {

                        m_PleasantValleyAmbient.clip = track;
                        m_PleasantValleyAmbient.Play();
                        m_PVLastPlayedTime = hoursPlayedNotPaused;
                    }
                }

            }

        }

        //Change the condition for Trepidation to play to be more common
        [HarmonyPatch(typeof(MusicEventManager), "CheckForHappySuccess")]
        internal class MusicEventManager_Trepidation
        {
            private static void Postfix(ref MusicEventManager __instance)
            {

                if (InterfaceManager.IsMainMenuEnabled() || Settings.settings.active == Active.Disabled)
                {
                    return;
                }

                Fire closestFire = GameManager.GetFireManagerComponent().GetClosestFire(GameManager.GetPlayerTransform().position);
                if (!closestFire || Vector3.Distance(GameManager.GetPlayerTransform().position, ((Component)closestFire).transform.position) > closestFire.m_HeatSource.m_MaxTempIncreaseOuterRadius || GameManager.GetFatigueComponent().m_CurrentFatigue / GameManager.GetFatigueComponent().m_MaxFatigue > 0.90f || GameManager.GetThirstComponent().m_CurrentThirst / GameManager.GetThirstComponent().m_MaxThirst > 0.75f || GameManager.GetFreezingComponent().m_CurrentFreezing / GameManager.GetFreezingComponent().m_MaxFreezing > 0.75f || GameManager.GetWeatherComponent().m_CurrentWeatherStage == WeatherStage.ClearAurora)
                {
                    return;
                }

                if (GameManager.GetConditionComponent().HasNonRiskAffliction() || GameManager.GetConditionComponent().GetConditionLevel() <= ConditionLevel.Injured)
                {
                    return;
                }

                bool flag = false;
                if (InterfaceManager.m_Panel_Harvest.HarvestInProgress())
                {
                    flag = true;   
                }
                if (InterfaceManager.m_Panel_IceFishing.IsFishing())
                {
                    flag = true;
                }
                if (InterfaceManager.m_Panel_BodyHarvest.IsHarvestingOrQuartering())
                {
                    flag = true;
                }
                if (InterfaceManager.m_Panel_Inventory_Examine.IsReading() || InterfaceManager.m_Panel_Inventory_Examine.IsRepairing() || InterfaceManager.m_Panel_Inventory_Examine.IsHarvesting())
                {
                    flag = true;
                }
                if (GameManager.GetPassTime().IsPassingTime())
                {
                    flag = true;
                }
                if (flag)
                {

                    float hoursPlayedNotPaused = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
                    if (!(hoursPlayedNotPaused - __instance.m_HappySuccessLastPlayTime < m_trepidationMinHoursInBetween))
                    {
                        GameAudioManager.PlayMusic(__instance.m_HappySuccessMusic, ((Component)__instance).gameObject);
                        __instance.m_HappySuccessLastPlayTime = hoursPlayedNotPaused;
                    }
                }
            }

         

        }

        //Does nothing right now
        [HarmonyPatch(typeof(WeatherTransition), "MaybePlayStinger")]
        internal class WeatherTransition_PlayStinger
        {
            private static void Prefix(ref WeatherTransition __instance, ref string stinger)
            {
               /* if(__instance.m_CurrentWeatherSet.m_CharacterizingType == WeatherStage.Blizzard && __instance.m_PreviousWeatherSetType == WeatherStage.HeavySnow)
                stinger = "Play_MusicStorm"; */
            }

        }

        //Add Clearing to the dawn music by chance
        [HarmonyPatch(typeof(TimeOfDay), "MaybePlayTimeOfDayStingers")]
        internal class TimeOfDay_PlayStingers
        {

            public static void Prefix(ref TimeOfDay __instance)
            {
                if(Settings.settings.active == Active.Disabled)
                {
                    return;
                }

                var num = Random.Range(0, 11);

                if (num % 2 == 0)
                {
                    __instance.m_DawnStingerAudio = "Play_MusicClear";
                }

            }

        }

        //Adds new location discovered sounds
        [HarmonyPatch(typeof(MusicEventManager), "PlayLocationSound")]
        internal class MusicEventManager_PlayLocationSound
        {

            private static void Postfix(ref MusicEventManager __instance, ref bool hasPlayedBefore)
            {

                if (!Settings.settings.locationStingers || Settings.settings.active == Active.Disabled || m_PleasantValleyAmbient.isPlaying)
                {
                    return;
                }

                if (InterfaceManager.IsMainMenuEnabled() || GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() < 0.01f)
                {
                    return;
                }


                if (GameManager.GetWeatherComponent().IsIndoorEnvironment())
                {
                    return;
                }

                int chosenStinger = Settings.settings.chosenStinger;

                if(chosenStinger == 3)
                {
                    chosenStinger = Random.Range(0, 3);
                }

                string[] stingers = { "Finder", "Shelter", "Shelter2"};

                AudioClip stinger = Implementation.TracksBundle.LoadAsset<AudioClip>(stingers[chosenStinger]);

                m_OutdoorStingerSource.volume = 0.5f;

                if(GameManager.GetWindComponent().m_CurrentStrength == WindStrength.VeryWindy || GameManager.GetWindComponent().m_CurrentStrength == WindStrength.Blizzard)
                {
                    m_OutdoorStingerSource.volume = 1f;
                }

                float hoursPlayedNotPaused = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
                if (!(hoursPlayedNotPaused - m_LocationDiscoveredLastPlayedTime < m_LocationDiscoveredHoursBetween))
                {
                    if (!hasPlayedBefore)
                    {
                        m_OutdoorStingerSource.clip = stinger;
                        m_OutdoorStingerSource.Play();
                        m_LocationDiscoveredLastPlayedTime = hoursPlayedNotPaused;
                    }

                }
            }

        }

        //checks if track is playing and stops from playing
        [HarmonyPatch(typeof(SceneMusicManager), "PlayExploreMusic")]
        internal class SceneMusicManager_Override
        {
            public static bool Prefix()
            {
                if (m_PleasantValleyAmbient.isPlaying || m_OutdoorStingerSource.isPlaying)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

        }
    


    }

}
