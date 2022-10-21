using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace BegoneMessage
{
	[StaticConstructorOnStartup]
	static class HarmonyPatches
	{
		static HarmonyPatches()
		{
			new Harmony("Begone_Message").PatchAll();
		}

		public static IEnumerable<MethodInfo> FindLambdaMethods(this Type type, string parentMethodName, Type returnType,
			Func<MethodInfo, bool> predicate = null)
		{
			// A lambda on RimWorld 1.0 on Unity 5.6.5f1 (mono .NET Framework 3.5 equivalent) is compiled into
			// a CompilerGenerated-attributed non-public inner type with name prefix "<{parentMethodName}>"
			// including an instance method with name prefix "<>".
			// A lambda on RimWorld 1.1+ on Unity 2019.2.17f1 (mono .NET Framework 4.7.2 equivalent) is compiled into
			// a CompilerGenerated-attributed non-public inner type with name prefix "<>"
			// including an instance method with name prefix "<{parentMethodName}>".
			// Recent-ish versions of Visual Studio also compile this way.
			// So to be generic enough, return methods of a declaring inner type that:
			// a) either the method or the declaring inner type has name prefix "<{parentMethodName}>".
			// b) has given return type
			// c) satisfies predicate, if given
			var innerTypes = type.GetNestedTypes(AccessTools.all)
				.Where(innerType => innerType.IsDefined(typeof(CompilerGeneratedAttribute)));
			var foundMethod = false;
			foreach (var innerType in innerTypes)
			{
				if (innerType.Name.StartsWith("<" + parentMethodName + ">", StringComparison.Ordinal))
				{
					foreach (var method in innerType.GetMethods(AccessTools.allDeclared))
					{
						if (method.Name.StartsWith("<", StringComparison.Ordinal) &&
							method.ReturnType == returnType && (predicate == null || predicate(method)))
						{
							foundMethod = true;
							yield return method;
						}
					}
				}
				else if (innerType.Name.StartsWith("<", StringComparison.Ordinal))
				{
					foreach (var method in innerType.GetMethods(AccessTools.allDeclared))
					{
						if (method.Name.StartsWith("<" + parentMethodName + ">", StringComparison.Ordinal) &&
							method.ReturnType == returnType && (predicate == null || predicate(method)))
						{
							foundMethod = true;
							yield return method;
						}
					}
				}
			}
			if (!foundMethod)
			{
				throw new ArgumentException($"Could not find any lambda method for type {type} and method {parentMethodName}" +
					" that satisfies given predicate");
			}
		}
	}

	[HarmonyPatch]
	static class Messages_Draw_Patch
	{
		static readonly List<Message> liveMessages = (List<Message>)typeof(Messages).GetField("liveMessages", AccessTools.all).GetValue(null);
		static FieldInfo fieldof_Message_Draw_ImmediateWindowLambda_this;

		// Targets the ImmediateWindow lambda in Message.Draw.
		[HarmonyTargetMethod]
		static MethodBase CalculateMethod()
		{
			var method = typeof(Message).FindLambdaMethods(nameof(Message.Draw), typeof(void),
				method => method.GetParameters().Length == 0).First();
			fieldof_Message_Draw_ImmediateWindowLambda_this = method.DeclaringType.GetFields(AccessTools.allDeclared).First(field =>
				field.FieldType == typeof(Message) && field.Name.Contains("this"));
			return method;
		}

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
