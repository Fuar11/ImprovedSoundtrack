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

        public static float m_TimeToPlayOnSceneLoadFog;
        public static float m_TimeToPlayOnSceneLoadClear;

        public static AudioSource m_OutdoorStingerSource = new AudioSource();

        [HarmonyPatch(typeof(GameManager), "Awake")]
        internal class GameManager_Awake
        {
            private static void Postfix()
            {
                MelonLoader.MelonLogger.Msg("Improved Soundtracks online!");
                m_TimeToPlayOnSceneLoadFog = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() + 0.005f;
                m_TimeToPlayOnSceneLoadClear = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() + Random.Range(0.005f, 4f);
            }

        }

        //Assists with music events
        [HarmonyPatch(typeof(MusicEventManager), "Awake")]
        internal class MusicEventManager_Awake
        {
            private static void Postfix(MusicEventManager __instance)
            {
                if (Settings.settings.active == Active.Enabled)
                {
                    m_stormLastPlayTime = float.NegativeInfinity;
                    m_ClearLastPlayedTime = float.NegativeInfinity;

                    m_OutdoorStingerSource = __instance.gameObject.AddComponent<AudioSource>();
                }
            }
        }

        //Adds more music events
        [HarmonyPatch(typeof(MusicEventManager), "Update")]
        internal class MusicEventManager_Update
        {
            private static void Postfix(MusicEventManager __instance)
            {
                if (Settings.settings.active == Active.Enabled)
                {

                    if (InterfaceManager.IsMainMenuEnabled())
                    {
                        return;
                    }

                    try
                    {
                        if (!GameManager.m_IsPaused)
                        {
                            CheckForBlizzard(__instance);
                            CheckForFog(__instance);
                            CheckForClearing(__instance);
                            m_OutdoorStingerSource.UnPause();
                            if (!GameManager.GetWeatherComponent().IsIndoorScene())
                            {
                                ExploreMusicManager.m_ExploreMusicSource.UnPause();
                            }
                        }
                        else
                        {
                            m_OutdoorStingerSource.Pause();
                            if (!GameManager.GetWeatherComponent().IsIndoorScene())
                            {
                                ExploreMusicManager.m_ExploreMusicSource.Pause();
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        
                    }
                }
            }

            private static void CheckForBlizzard(MusicEventManager __instance)
            {
                if (InterfaceManager.IsMainMenuEnabled())
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

                if (InterfaceManager.IsMainMenuEnabled())
                {
                    return;
                }

                if (GameManager.GetWeatherComponent().GetWeatherStage() != WeatherStage.DenseFog)
                {
                    return;
                }

                if (!GameManager.GetWeatherComponent().IsIndoorScene() || !GameManager.GetWeatherComponent().IsIndoorEnvironment())
                {

                    if (ExploreMusicManager.m_ExploreMusicSource.isPlaying)
                    {
                        return;
                    }

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
                if (InterfaceManager.IsMainMenuEnabled())
                {
                    return;
                }

                if (!GameManager.GetWeatherComponent().IsIndoorScene())
                {
                    if (ExploreMusicManager.m_ExploreMusicSource.isPlaying)
                    {
                        return;
                    }

                    if (GameManager.GetWeatherComponent().GetWeatherStage() == WeatherStage.Blizzard || GameManager.GetWeatherComponent().GetWeatherStage() == WeatherStage.DenseFog)
                    {
                        return;
                    }

                    if (GameManager.GetWindComponent().GetStrength() == WindStrength.VeryWindy || GameManager.GetWindComponent().GetStrength() == WindStrength.Windy)
                    {
                        return;
                    }

                    if (GameManager.GetWeatherComponent().GetWeatherStage() == WeatherStage.Clear)
                    {
                        if (!GameManager.GetTimeOfDayComponent().IsDawn())
                        {
                            return;
                        }
                    }

                    if (GameManager.GetTimeOfDayComponent().GetHour() == GameManager.GetTimeOfDayComponent().m_DawnStingerHourStart && GameManager.GetTimeOfDayComponent().GetMinutes() > GameManager.GetTimeOfDayComponent().m_DawnStingerMinutesStart)
                    {
                        return;
                    }
                    else if (GameManager.GetTimeOfDayComponent().GetHour() == GameManager.GetTimeOfDayComponent().m_NightStingerHourStart && GameManager.GetTimeOfDayComponent().GetMinutes() > GameManager.GetTimeOfDayComponent().m_NightStingerMinutesStart)
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

        }

        //Change the condition for Trepidation to play to be more common
        [HarmonyPatch(typeof(MusicEventManager), "CheckForHappySuccess")]
        internal class MusicEventManager_Trepidation
        {
            private static void Postfix(MusicEventManager __instance)
            {

                if (InterfaceManager.IsMainMenuEnabled() || Settings.settings.active == Active.Disabled)
                {
                    return;
                }

                if (!GameManager.GetWeatherComponent().IsIndoorScene())
                {
                    if (ExploreMusicManager.m_ExploreMusicSource.isPlaying)
                    {
                        return;
                    }
                }


                Fire closestFire = GameManager.GetFireManagerComponent().GetClosestFire(GameManager.GetPlayerTransform().position);
                if (!closestFire || Vector3.Distance(GameManager.GetPlayerTransform().position, ((Component)closestFire).transform.position) > closestFire.m_HeatSource.m_MaxTempIncreaseOuterRadius || GameManager.GetFatigueComponent().m_CurrentFatigue / GameManager.GetFatigueComponent().m_MaxFatigue > 0.90f || GameManager.GetThirstComponent().m_CurrentThirst / GameManager.GetThirstComponent().m_MaxThirst > 0.75f || GameManager.GetFreezingComponent().m_CurrentFreezing / GameManager.GetFreezingComponent().m_MaxFreezing > 0.75f || GameManager.GetWeatherComponent().m_CurrentWeatherStage == WeatherStage.ClearAurora)
                {
                    return;
                }

                if (GameManager.GetConditionComponent().HasNonRiskAffliction())
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

                    try
                    {
                        float hoursPlayedNotPaused = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
                        if (!(hoursPlayedNotPaused - __instance.m_HappySuccessLastPlayTime < m_trepidationMinHoursInBetween))
                        {

                            GameAudioManager.PlayMusic(__instance.m_HappySuccessMusic, ((Component)__instance).gameObject);
                            __instance.m_HappySuccessLastPlayTime = hoursPlayedNotPaused;
                        }
                    }
                    catch (Exception e)
                    {
                        return;
                    }
                }
            }



        }

        //Add Clearing to the dawn music by chance
        [HarmonyPatch(typeof(TimeOfDay), "MaybePlayTimeOfDayStingers")]
        internal class TimeOfDay_PlayStingers
        {

            public static void Prefix(TimeOfDay __instance)
            {
                if (Settings.settings.active == Active.Disabled)
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

        //Adds new location discovered stingers
        [HarmonyPatch(typeof(MusicEventManager), "PlayLocationSound")]
        internal class MusicEventManager_PlayLocationSound
        {

            private static void Postfix(ref bool hasPlayedBefore)
            {

                if (GameManager.GetWeatherComponent().IsIndoorScene())
                {
                    return;
                }

                if (!Settings.settings.locationStingers || Settings.settings.active == Active.Disabled || ExploreMusicManager.m_ExploreMusicSource.isPlaying)
                {
                    return;
                }

                if (InterfaceManager.IsMainMenuEnabled() || GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() < 0.01f)
                {
                    return;
                }

                int chosenStinger = Settings.settings.chosenStinger;

                if (chosenStinger == 3)
                {
                    chosenStinger = Random.Range(0, 3);
                }

                string[] stingers = { "Finder", "Shelter", "Shelter2" };

                AudioClip stinger = Implementation.TracksBundle.LoadAsset<AudioClip>(stingers[chosenStinger]);

                m_OutdoorStingerSource.volume = InterfaceManager.m_Panel_OptionsMenu.m_State.m_MusicVolume;

                if (!hasPlayedBefore)
                {
                    m_OutdoorStingerSource.clip = stinger;
                    m_OutdoorStingerSource.Play();
                }


            }

        }

        //disables wolf stalking audio
        [HarmonyPatch(typeof(MusicEventManager), "CheckForBeingStalked")]
        internal class MusicEventManager_CheckStalkedOverride
        {
            internal static bool Prefix()
            {
                return false;
            }
        }



    }

}
