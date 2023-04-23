using HarmonyLib;
using System;
using UnityEngine;

namespace BeatSaber_JDFixerModifiers.HarmonyPatches
{
	[HarmonyPatch(typeof(BeatmapObjectSpawnMovementData), "Init")]
	[HarmonyAfter(new string[] { "com.zephyr.BeatSaber.JDFixer" })]
	static class Patches
	{
		static bool Prefix(int noteLinesCount, float startNoteJumpMovementSpeed, float startBpm, BeatmapObjectSpawnMovementData.NoteJumpValueType noteJumpValueType, 
			float noteJumpValue, IJumpOffsetYProvider jumpOffsetYProvider, Vector3 rightVec, Vector3 forwardVec, ref int ____noteLinesCount, ref float ____noteJumpMovementSpeed,
			ref float ____jumpDuration, ref float ____noteJumpStartBeatOffset, ref Vector3 ____rightVec, ref Vector3 ____forwardVec, ref IJumpOffsetYProvider ____jumpOffsetYProvider,
			ref float ____moveDistance, ref float ____moveSpeed, ref float ____moveDuration, ref float ____jumpDistance, ref Vector3 ____moveEndPos, ref Vector3 ____centerPos,
			ref Vector3 ____jumpEndPos, ref Vector3 ____moveStartPos, ref float ____spawnAheadTime, ref float ____startHalfJumpDurationInBeats, ref float ____maxHalfJumpDistance)
		{
			if(Configs.Configs.Instance.Enabled)
            {
				____noteLinesCount = noteLinesCount;
				____noteJumpMovementSpeed = startNoteJumpMovementSpeed;
				____jumpDuration = startBpm.OneBeatDuration() * (Configs.Configs.Instance.Jump / Configs.Configs.Instance.Precision);
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
