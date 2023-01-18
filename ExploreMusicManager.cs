using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Il2Cpp;
using UnityEngine.Audio;
using HarmonyLib;
using Random = UnityEngine.Random;

namespace ImprovedSoundtrack
{
    internal class ExploreMusicManager
    {

        public static List<string> m_ExploreMusicDay = new List<string>();
        public static List<string> m_ExploreMusicNight = new List<string>();
        public static List<string> m_ExploreMusicAurora = new List<string>();

        public static AudioSource m_ExploreMusicSource = new AudioSource();

        //adds explore music tracks to custom lists
        [HarmonyPatch(typeof(SceneMusicManager), "Awake")]
        internal class SceneMusicManager_Awake
        {
            private static void Postfix(SceneMusicManager __instance)
            {

                /*
                m_ExploreMusicSource = __instance.gameObject.AddComponent<AudioSource>();

                m_ExploreMusicDay.Clear();
                m_ExploreMusicNight.Clear();
                m_ExploreMusicAurora.Clear();

                m_ExploreMusicDay.Add("Desolation");
                m_ExploreMusicDay.Add("Heaven");
                m_ExploreMusicDay.Add("Heavy");
                m_ExploreMusicDay.Add("Hope");
                m_ExploreMusicDay.Add("Illuminate");
                m_ExploreMusicDay.Add("Majestic");
                m_ExploreMusicDay.Add("Disobedience");
                m_ExploreMusicDay.Add("Patience");
                m_ExploreMusicDay.Add("Stellar");
                m_ExploreMusicDay.Add("Unknown");
                m_ExploreMusicDay.Add("Wind");

                if (RegionManager.GetCurrentRegion() == GameRegion.RuralRegion && Settings.settings.thePleasantValleyEnabled)
                {
                    m_ExploreMusicDay.Add("TPV");
                }

                m_ExploreMusicNight.Add("Prepare");
                m_ExploreMusicNight.Add("Nocturnal");
                m_ExploreMusicNight.Add("Firmament");
                m_ExploreMusicNight.Add("Stellar");
                m_ExploreMusicNight.Add("Unknown");
                m_ExploreMusicNight.Add("Entropy");

                m_ExploreMusicAurora.Add("Firmament");
                m_ExploreMusicAurora.Add("Prepare");

                */
            }
        }

        //overloads the explore music with Unity audio engine
        [HarmonyPatch(typeof(SceneMusicManager), "PlayExploreMusic")]
        internal class SceneMusicManager_Override
        {
            private static void Prefix(SceneMusicManager __instance)
            {
              //return false;
            }
            private static void Postfix(SceneMusicManager __instance)
            {

                /*
                if (GameManager.GetWeatherComponent().IsIndoorScene() && GameManager.GetWeatherComponent().IsIndoorEnvironment())
                {
                    return;
                }

                __instance.ResetExploreMusicTimer();

                if (__instance.m_SupressExploreMusic || __instance.m_MusicSuppressorInRange)
                {
                    Debug.Log("Suppressed explore music");
                }

                List<string> list = new List<string>();

                list = ((!GameManager.GetTimeOfDayComponent().IsNight()) ? m_ExploreMusicDay : ((!GameManager.GetAuroraManager().AuroraIsActive()) ? m_ExploreMusicNight : m_ExploreMusicAurora));
                if (list.Count > 0)
                {
                    AudioClip track = Implementation.TracksBundle.LoadAsset<AudioClip>(list[Random.Range(0, list.Count)]);
                    m_ExploreMusicSource.clip = track;
                    m_ExploreMusicSource.volume = InterfaceManager.GetPanel<Panel_OptionsMenu>().m_State.m_MusicVolume;
                    m_ExploreMusicSource.Play();
                }

                */
            }

        }

        //for testing purposes only
        /*[HarmonyPatch(typeof(SceneMusicManager), "ResetExploreMusicTimer")]
        internal class ExploreMusicTimerOverride
        {
            private static void Postfix(SceneMusicManager __instance)
            {
                __instance.m_TimeToPlayNextExploreMusic = 0.1f;
            }
        } */

    }
}
