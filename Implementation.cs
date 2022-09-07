using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;

namespace ImprovedSoundtrack
{
    public class Implementation : MelonMod
    {

        private static AssetBundle? assetBundle;

        internal static AssetBundle TracksBundle
        {
            get => assetBundle ?? throw new System.NullReferenceException(nameof(assetBundle));
        }

        public override void OnApplicationStart()
        {
            Debug.Log($"[{Info.Name}] Version {Info.Version} loaded!");
            Settings.onLoad();

            assetBundle = LoadAssetBundle("ImprovedSoundtrack.tracksbundle");
            MelonLoader.MelonLogger.Msg("Asset Bundle: {0}", assetBundle);
            GetAssetNames(assetBundle);
        }

        private static AssetBundle LoadAssetBundle(string path)
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            MelonLoader.MelonLogger.Msg("Stream: {0}", stream);
            MemoryStream memoryStream = new MemoryStream((int)stream.Length);
            MelonLoader.MelonLogger.Msg("Memory Stream: {0}", memoryStream);
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
