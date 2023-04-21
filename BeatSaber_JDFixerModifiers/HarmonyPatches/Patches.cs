using HarmonyLib;
using System;

namespace BeatSaber_JDFixerModifiers.HarmonyPatches
{
	[HarmonyPatch(typeof(BeatmapObjectSpawnMovementData), "Init")]
    [HarmonyAfter(new string[] { "com.zephyr.BeatSaber.JDFixer" })]
	static class Patches
	{
		static void Prefix(ref float noteJumpValue)
		{
			if (Configs.Configs.Instance.Enabled)
			{
				if (Plugin.levelData.GameplayCoreSceneSetupData.practiceSettings != null) // Practice mode
				{
					if(Plugin.levelData.GameplayCoreSceneSetupData.practiceSettings.songSpeedMul >= 1)
                    {
						noteJumpValue += Math.Abs(noteJumpValue - Plugin.levelData.GameplayCoreSceneSetupData.practiceSettings.songSpeedMul * noteJumpValue);
					}
					else
                    {
						noteJumpValue -= Math.Abs(noteJumpValue - Plugin.levelData.GameplayCoreSceneSetupData.practiceSettings.songSpeedMul * noteJumpValue);
					}
				}
				else // Non-practice
				{
					if (!Configs.Configs.Instance.Overwrite) // Custom multiplier
					{
						if (Plugin.levelData.GameplayCoreSceneSetupData.gameplayModifiers.songSpeedMul >= 1)
						{
							noteJumpValue += Math.Abs(noteJumpValue - Plugin.levelData.GameplayCoreSceneSetupData.gameplayModifiers.songSpeedMul * noteJumpValue);
						}
						else
						{
							noteJumpValue -= Math.Abs(noteJumpValue - Plugin.levelData.GameplayCoreSceneSetupData.gameplayModifiers.songSpeedMul * noteJumpValue);
						}
					}
					else // Default multiplier
					{
						var multi = 1f;
						if (Plugin.levelData.GameplayCoreSceneSetupData.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Slower)
						{
							multi = Configs.Configs.Instance.SS;
						}
						else if (Plugin.levelData.GameplayCoreSceneSetupData.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Faster)
						{
							multi = Configs.Configs.Instance.FS;
						}
						else if (Plugin.levelData.GameplayCoreSceneSetupData.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.SuperFast)
						{
							multi = Configs.Configs.Instance.SF;
						}

						if (Plugin.levelData.GameplayCoreSceneSetupData.gameplayModifiers.songSpeedMul >= 1)
						{
							noteJumpValue += Math.Abs(noteJumpValue - multi * noteJumpValue);
						}
						else
						{
							noteJumpValue -= Math.Abs(noteJumpValue - multi * noteJumpValue);
						}
					}
				}
			}
		}
	}
}
