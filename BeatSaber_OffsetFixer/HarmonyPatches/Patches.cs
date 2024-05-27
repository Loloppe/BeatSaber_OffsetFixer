using HarmonyLib;
using System;
using System.Reflection;
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

				// Modifiers
				float multiplier = 1f;
				if (Configs.Configs.Instance.Overwrite)
				{
					if (Plugin.levelData.GameplayCoreSceneSetupData.practiceSettings != null) // Practice mode
					{
						multiplier = Plugin.levelData.GameplayCoreSceneSetupData.practiceSettings.songSpeedMul;
					}
					else // Non-practice
					{
						if (Plugin.levelData.GameplayCoreSceneSetupData.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Slower)
						{
							multiplier = Configs.Configs.Instance.SS;
						}
						else if (Plugin.levelData.GameplayCoreSceneSetupData.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Faster)
						{
							multiplier = Configs.Configs.Instance.FS;
						}
						else if (Plugin.levelData.GameplayCoreSceneSetupData.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.SuperFast)
						{
							multiplier = Configs.Configs.Instance.SF;
						}
					}
				}

                // RT Multiplier based on NJS
				// The higher the NJS, the lower the RT should be
                var multi = 1f;
				if (Configs.Configs.Instance.NJSMultiplier)
				{
					multi = Configs.Configs.Instance.NJS / startNoteJumpMovementSpeed;
				}

				// Calculation yoinked from JDFixer
				var desiredJumpDistance = (Configs.Configs.Instance.ReactionTime * multiplier) * startNoteJumpMovementSpeed / 500;
				var halfJumpDuration = 4f;
				while (startNoteJumpMovementSpeed * (60f / startBpm) * halfJumpDuration > 17.999)
					halfJumpDuration /= 2f;
				if (halfJumpDuration < 0.25f)
					halfJumpDuration = 0.25f;
				float desiredJumpDur = desiredJumpDistance / startNoteJumpMovementSpeed;
				float jumpDurationMultiplier = desiredJumpDur / (halfJumpDuration * (60f / startBpm) * 2f);
				____noteJumpStartBeatOffset = (halfJumpDuration * jumpDurationMultiplier) - halfJumpDuration;

                // Get the closest beat snap, based on RT and precision
                float beatDuration = OneBeatDuration(startBpm);
                float rtOffset = CoreMathUtils.CalculateHalfJumpDurationInBeats(____startHalfJumpDurationInBeats, ____maxHalfJumpDistance, ____noteJumpMovementSpeed, beatDuration, ____noteJumpStartBeatOffset) * 2f;
				if(Configs.Configs.Instance.Snap)
				{
					var value = beatDuration * rtOffset * multi;
					var factor = beatDuration / Configs.Configs.Instance.Precision;
					var nearestMultiple = (float)Math.Round(value / factor, MidpointRounding.AwayFromZero) * factor;
					____jumpDuration = nearestMultiple;
				}
				else
				{
					____jumpDuration = beatDuration * rtOffset * multi;
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

        public static float OneBeatDuration(this float bpm)
        {
            if (bpm <= 0f)
            {
                return 0f;
            }
            return 60f / bpm;
        }
    }
}
