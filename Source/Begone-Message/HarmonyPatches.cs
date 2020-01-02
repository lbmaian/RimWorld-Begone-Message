using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using UnityEngine;
using Verse;

namespace BegoneMessage
{
	[StaticConstructorOnStartup]
	static class HarmonyPatches
	{
		const bool DEBUG = false;

		static HarmonyPatches()
		{
			HarmonyInstance.DEBUG = DEBUG;
			try
			{
				HarmonyInstance.Create("Begone_Message").PatchAll();
			}
			finally
			{
				HarmonyInstance.DEBUG = false;
			}
		}
	}

	[HarmonyPatch]
	static class Messages_Draw_Patch
	{
		static readonly Type typeof_Message_Draw_ImmediateWindowLambda = typeof(Message).GetNestedType("<Draw>c__AnonStorey0", AccessTools.all);
		static readonly FieldInfo fieldof_Message_Draw_ImmediateWindowLambda_this =
			typeof_Message_Draw_ImmediateWindowLambda.GetField("$this", AccessTools.all);
		static readonly List<Message> liveMessages = (List<Message>)typeof(Messages).GetField("liveMessages", AccessTools.all).GetValue(null);

		// Targets the ImmediateWindow lambda in Message.Draw.
		[HarmonyTargetMethod]
		static MethodBase Calculatemethod(HarmonyInstance _) => typeof_Message_Draw_ImmediateWindowLambda.GetMethod("<>m__0", AccessTools.all);

		// This must be done in a prefix, because the Widgets.ButtonInvisible within the lambda will use up mouse events.
		// Additionally, it allows us to have that lambda return immediately after handling right-click.
		[HarmonyPrefix]
		static bool Prefix(object __instance)
		{
			Event currentEvent = Event.current;
			if (currentEvent.type == EventType.MouseDown && currentEvent.button == 1)
			{
				if (currentEvent.shift)
				{
					RemoveMessage(__instance, BegoneMessageMod.Settings.shiftRightClickDismissAll);
				}
				else
				{
					RemoveMessage(__instance, BegoneMessageMod.Settings.rightClickDismissAll);
				}
				currentEvent.Use();
				return false;
			}
			return true;
		}

		static void RemoveMessage(object instance, bool dismissAll)
		{
			if (dismissAll)
			{
				liveMessages.Clear();
			}
			else
			{
				var msg = (Message)fieldof_Message_Draw_ImmediateWindowLambda_this.GetValue(instance);
				liveMessages.Remove(msg);
			}
		}
	}
}
