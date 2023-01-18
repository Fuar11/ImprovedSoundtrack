using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Il2Cpp;
using Il2CppInterop;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;
using AudioMgr;

namespace ImprovedSoundtrack
{
    public class Implementation : MelonMod
    {

        public static ClipManager ShotMusicManager;
        public static Shot TrepSource;
        public static Shot StingerSource;

        private static AssetBundle? assetBundle;
        internal static AssetBundle TracksBundle
        {
            get => assetBundle ?? throw new System.NullReferenceException(nameof(assetBundle));
        }

        public override void OnInitializeMelon()
        {
            Debug.Log($"[{Info.Name}] Version {Info.Version} loaded!");
            Settings.onLoad();

            assetBundle = LoadAssetBundle("ImprovedSoundtrack.tracksbundle");
            MelonLogger.Msg("Asset Bundle: {0}", assetBundle);
            GetAssetNames(assetBundle);
        }

        public override void OnUpdate()
        {
            
            //testing
            if (InputManager.GetKeyDown(InputManager.m_CurrentContext, KeyCode.Keypad0))
            {
                TrepSource.AssignClip(ShotMusicManager.GetClip("shelter"));
                TrepSource.Play();
                MelonLogger.Msg("Playing...");
            }

        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName.Contains("Menu"))
            {

                ShotMusicManager = AudioMaster.NewClipManager();
                ShotMusicManager.LoadAllClipsFromBundle(TracksBundle);

                TrepSource = AudioMaster.CreatePlayerShot(AudioMaster.SourceType.BGM);
                StingerSource = AudioMaster.CreatePlayerShot(AudioMaster.SourceType.BGM);
            }
        }



        private static AssetBundle LoadAssetBundle(string path)
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            //MelonLoader.MelonLogger.Msg("Stream: {0}", stream);
            MemoryStream memoryStream = new MemoryStream((int)stream.Length);
            //MelonLoader.MelonLogger.Msg("Memory Stream: {0}", memoryStream);
            stream.CopyTo(memoryStream);

            return memoryStream.Length != 0
                ? AssetBundle.LoadFromMemory(memoryStream.ToArray())
                : throw new System.Exception("No data loaded!");
        }
        private static void GetAssetNames(AssetBundle assetBundle)
        {
            foreach (string ass in assetBundle.GetAllAssetNames())
            {
                MelonLoader.MelonLogger.Msg("Assets: {0}", ass);
            }
        }

    }
}
