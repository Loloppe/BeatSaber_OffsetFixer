using HarmonyLib;
using System;
using System.Reflection;

namespace BeatSaber_JDFixerModifiers.HarmonyPatches
{
	[HarmonyPatch(typeof(BeatmapObjectSpawnMovementData), "Init")]
	[HarmonyPriority(Priority.Low)]
	static class Patches
	{
		static void Prefix(ref float noteJumpValue)
		{
			if (Configs.Configs.Instance.Enabled)
			{
				if (Plugin.levelData.GameplayCoreSceneSetupData.practiceSettings != null)
				{
					noteJumpValue *= Plugin.levelData.GameplayCoreSceneSetupData.practiceSettings.songSpeedMul;
				}
				else
                {
					if (!Configs.Configs.Instance.Overwrite)
					{
						noteJumpValue *= Plugin.levelData.GameplayCoreSceneSetupData.gameplayModifiers.songSpeedMul;
					}
					else
					{
						if (Plugin.levelData.GameplayCoreSceneSetupData.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Slower)
						{
							noteJumpValue *= Configs.Configs.Instance.SS;
						}
						else if (Plugin.levelData.GameplayCoreSceneSetupData.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Faster)
						{
							noteJumpValue *= Configs.Configs.Instance.FS;
						}
						else if (Plugin.levelData.GameplayCoreSceneSetupData.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.SuperFast)
						{
							noteJumpValue *= Configs.Configs.Instance.SF;
						}
						else
                        {
							noteJumpValue *= Plugin.levelData.GameplayCoreSceneSetupData.gameplayModifiers.songSpeedMul;
						}
					}
				}
			}
		}
	}
}
