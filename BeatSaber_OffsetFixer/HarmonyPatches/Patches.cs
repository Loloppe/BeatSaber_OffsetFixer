using HarmonyLib;
using System;
using UnityEngine;

namespace BeatSaber_OffsetFixer.HarmonyPatches
{
	[HarmonyPatch(typeof(VariableMovementDataProvider), "Init")]
    [HarmonyAfter(new string[] { "com.zephyr.BeatSaber.JDFixer", "Kinsi55.BeatSaber.EasyRT" })]
    static class Patches
	{
		static bool Prefix(VariableMovementDataProvider __instance, float startHalfJumpDurationInBeats, float maxHalfJumpDistance, float noteJumpMovementSpeed, 
			float minRelativeNoteJumpSpeed, float bpm, BeatmapObjectSpawnMovementData.NoteJumpValueType noteJumpValueType, float noteJumpValue, Vector3 centerPosition, Vector3 forwardVector)
		{
			if(Configs.Configs.Instance.Enabled)
			{
                __instance._initOneBeatDuration = TimeExtensions.OneBeatDuration(bpm);
                __instance._initNoteJumpMovementSpeed = noteJumpMovementSpeed;
                __instance._targetNoteJumpMovementSpeed = noteJumpMovementSpeed;
                __instance._noteJumpValueType = noteJumpValueType;
                __instance._relativeNoteJumpSpeedInterpolation.SetValues(0f, 0f, 0f, 0f, (EaseType)0);
                __instance._centerPosition = centerPosition;
                __instance._forwardVector = forwardVector;

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
                // Also take into consideration modifiers
                var multi = 1f;
                if (Configs.Configs.Instance.NJSMultiplier)
                {
                    multi = Configs.Configs.Instance.NJS / noteJumpMovementSpeed;
                }

                // Calculation yoinked from JDFixer
                var desiredJumpDistance = (Configs.Configs.Instance.ReactionTime * multi * multiplier) * noteJumpMovementSpeed / 500;
                var halfJumpDuration = 4f;
                while (noteJumpMovementSpeed * (60f / bpm) * halfJumpDuration > 17.999)
                    halfJumpDuration /= 2f;
                if (halfJumpDuration < 0.25f)
                    halfJumpDuration = 0.25f;

                // Apply the new JD here
                float desiredJumpDur = desiredJumpDistance / noteJumpMovementSpeed;
                float jumpDurationMultiplier = desiredJumpDur / (halfJumpDuration * (60f / bpm) * 2f);
                noteJumpValue = (halfJumpDuration * jumpDurationMultiplier) - halfJumpDuration;

                // Get the closest beat snap, based on RT and precision
                __instance._halfJumpDurationInBeats = CoreMathUtils.CalculateHalfJumpDurationInBeats(startHalfJumpDurationInBeats, maxHalfJumpDistance, noteJumpMovementSpeed, __instance._initOneBeatDuration, noteJumpValue);
                if (Configs.Configs.Instance.Snap)
                {
                    var value = __instance._initOneBeatDuration * __instance._halfJumpDurationInBeats;
                    var factor = __instance._initOneBeatDuration / Configs.Configs.Instance.Precision;
                    var nearestMultiple = (float)Math.Round(value / factor, MidpointRounding.AwayFromZero) * factor;
                    __instance._jumpDuration = nearestMultiple * 2;
                    __instance._halfJumpDuration = nearestMultiple;
                }
                else // Default implementation
                {
                    if (__instance._noteJumpValueType == BeatmapObjectSpawnMovementData.NoteJumpValueType.JumpDuration)
                    {
                        __instance._jumpDuration = noteJumpValue * 2f;
                        __instance._halfJumpDuration = noteJumpValue;
                    }
                }

                __instance.ManualUpdate(0f);
                float num = Mathf.Min(noteJumpMovementSpeed, noteJumpMovementSpeed + minRelativeNoteJumpSpeed);
                __instance._spawnAheadTime = 0.5f + __instance._noteJumpValueType switch
                {
                    BeatmapObjectSpawnMovementData.NoteJumpValueType.BeatOffset => __instance._initOneBeatDuration * __instance._halfJumpDurationInBeats / (num / __instance._initNoteJumpMovementSpeed),
                    BeatmapObjectSpawnMovementData.NoteJumpValueType.JumpDuration => __instance._halfJumpDuration,
                    _ => throw new ArgumentOutOfRangeException(),
                };

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
