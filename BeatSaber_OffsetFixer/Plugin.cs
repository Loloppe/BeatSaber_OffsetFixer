using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.Settings;
using HarmonyLib;
using IPA;
using IPA.Config.Stores;
using System.Reflection;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace BeatSaber_OffsetFixer
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static Plugin Instance;
        internal static IPALogger Log;
        internal static Harmony harmony;
        internal static BS_Utils.Gameplay.LevelData levelData = null;

        [Init]
        public Plugin(IPALogger logger, IPA.Config.Config conf)
        {
            Instance = this;
            Log = logger;
            Configs.Configs.Instance = conf.Generated<Configs.Configs>();
            harmony = new Harmony("Loloppe.BeatSaber.OffsetFixer");
        }

        [OnEnable]
        public void OnEnable()
        {
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            BSMLSettings.instance.AddSettingsMenu("OffsetFixer", "BeatSaber_OffsetFixer.Views.views.bsml", Configs.Configs.Instance);
            GameplaySetup.instance.AddTab("OffsetFixer", "BeatSaber_OffsetFixer.Views.views.bsml", Configs.Configs.Instance, MenuType.All);
        }

        public void OnActiveSceneChanged(Scene arg0, Scene scene)
        {
            if (BS_Utils.SceneNames.Game == scene.name)
            {
                levelData = BS_Utils.Plugin.LevelData;
            }
        }

        [OnDisable]
        public void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            harmony.UnpatchSelf();
            BSMLSettings.instance.RemoveSettingsMenu(Configs.Configs.Instance);
            GameplaySetup.instance.RemoveTab("OffsetFixer");
        }
    }
}
