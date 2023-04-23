using HarmonyLib;
using System;
using UnityEngine;

namespace BeatSaber_OffsetFixer.HarmonyPatches
{
	[HarmonyPatch(typeof(BeatmapObjectSpawnMovementData), "Init")]
	[HarmonyAfter(new string[] { "com.zephyr.BeatSaber.JDFixer" })]
	static class Patches
	{
		static bool Prefix(int noteLinesCount, float startNoteJumpMovementSpeed, float startBpm, IJumpOffsetYProvider jumpOffsetYProvider, Vector3 rightVec, Vector3 forwardVec, ref int ____noteLinesCount, ref float ____noteJumpMovementSpeed,
			ref float ____jumpDuration, ref float ____noteJumpStartBeatOffset, ref Vector3 ____rightVec, ref Vector3 ____forwardVec, ref IJumpOffsetYProvider ____jumpOffsetYProvider,
			ref float ____moveDistance, ref float ____moveSpeed, ref float ____moveDuration, ref float ____jumpDistance, ref Vector3 ____moveEndPos, ref Vector3 ____centerPos,
			ref Vector3 ____jumpEndPos, ref Vector3 ____moveStartPos, ref float ____spawnAheadTime, ref float ____startHalfJumpDurationInBeats, ref float ____maxHalfJumpDistance)
		{
			if(Configs.Configs.Instance.Enabled)
            {
				// No touch
				____noteLinesCount = noteLinesCount;
				____noteJumpMovementSpeed = startNoteJumpMovementSpeed;

				// Yeeted from JDFixer
				var desiredJumpDistance = Configs.Configs.Instance.ReactionTime * startNoteJumpMovementSpeed / 500;
				var halfJumpDuration = 4f;
				while (startNoteJumpMovementSpeed * (60f / startBpm) * halfJumpDuration > 17.999)
					halfJumpDuration /= 2f;
				if (halfJumpDuration < 0.25f)
					halfJumpDuration = 0.25f;
				float desiredJumpDur = desiredJumpDistance / startNoteJumpMovementSpeed;
				float jumpDurationMultiplier = desiredJumpDur / (halfJumpDuration * (60f / startBpm) * 2f);
				____noteJumpStartBeatOffset = (halfJumpDuration * jumpDurationMultiplier) - halfJumpDuration;

				// Here we deal with modifiers
				if(Configs.Configs.Instance.Overwrite)
                {
					if (Plugin.levelData.GameplayCoreSceneSetupData.practiceSettings != null) // Practice mode
					{
						if (Plugin.levelData.GameplayCoreSceneSetupData.practiceSettings.songSpeedMul >= 1)
						{
							____noteJumpStartBeatOffset += Math.Abs(____noteJumpStartBeatOffset - Plugin.levelData.GameplayCoreSceneSetupData.practiceSettings.songSpeedMul * ____noteJumpStartBeatOffset);
						}
						else
						{
							____noteJumpStartBeatOffset -= Math.Abs(____noteJumpStartBeatOffset - Plugin.levelData.GameplayCoreSceneSetupData.practiceSettings.songSpeedMul * ____noteJumpStartBeatOffset);
						}
					}
					else // Non-practice
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

						if (multi >= 1)
						{
							____noteJumpStartBeatOffset += Math.Abs(____noteJumpStartBeatOffset - multi * ____noteJumpStartBeatOffset);
						}
						else
						{
							____noteJumpStartBeatOffset -= Math.Abs(____noteJumpStartBeatOffset - multi * ____noteJumpStartBeatOffset);
						}
					}
				}

				// Get the closest beat snap, based on RT and precision
				float beatDuration = startBpm.OneBeatDuration();
				float rtOffset = CoreMathUtils.CalculateHalfJumpDurationInBeats(____startHalfJumpDurationInBeats, ____maxHalfJumpDistance, ____noteJumpMovementSpeed, beatDuration, ____noteJumpStartBeatOffset) * 2f;
				var value = beatDuration * rtOffset;
				var factor = beatDuration / Configs.Configs.Instance.Precision;
				if(Configs.Configs.Instance.Snap)
                {
					var nearestMultiple = (float)Math.Round(value / factor, MidpointRounding.AwayFromZero) * factor;
					____jumpDuration = nearestMultiple;
				}
				else
                {
					____jumpDuration = rtOffset;
				}

				// No touch
				____rightVec = rightVec;
				____forwardVec = forwardVec;
				____jumpOffsetYProvider = jumpOffsetYProvider;
				____moveDistance = ____moveSpeed * ____moveDuration;
				____jumpDistance = ____noteJumpMovementSpeed * ____jumpDuration;
				____moveEndPos = ____centerPos + ____forwardVec * (____jumpDistance * 0.5f);
				____jumpEndPos = ____centerPos - ____forwardVec * (____jumpDistance * 0.5f);
				____moveStartPos = ____centerPos + ____forwardVec * (____moveDistance + ____jumpDistance * 0.5f);
				____spawnAheadTime = ____moveDuration + ____jumpDuration * 0.5f;

				return false;
			}

			return true;
		}
	}
}
