using System;
using System.Collections.Generic;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace BegoneMessage
{
	public static class ListingStandardExtensions
	{
		public static void LeftAlignedLabeledRadioButtonList<T>(this Listing_Standard listingStandard, string labelText,
			IDictionary<string, T> labeledOptions, ref T settingsValue)
		{
			listingStandard.Label(labelText);
			foreach (var pair in labeledOptions)
			{
				var optionLabelText = pair.Key;
				var optionValue = pair.Value;
				if (listingStandard.LeftAlignedLabeledRadioButton(optionLabelText, settingsValue.Equals(optionValue)))
					settingsValue = optionValue;
			}
		}

		public static bool LeftAlignedLabeledRadioButton(this Listing_Standard listingStandard, string labelText, bool chosen)
		{
			// Annoyingly, Widgets.RadioButton has a mouseover sound effect that Widgets.RadioButtonLabeled lacks,
			// and Widgets.RadioButtonLabeled right-aligns the radio buttons, so we can't use either method.
			var rect = listingStandard.GetRect(Text.LineHeight);
			RadioButtonDraw(rect.x + Widgets.RadioButtonSize * 0.25f, rect.y + (rect.height - Widgets.RadioButtonSize) / 2f, chosen);
			var origAnchor = Text.Anchor;
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect.RightPartPixels(rect.width - Widgets.RadioButtonSize * 1.5f), labelText);
			Text.Anchor = origAnchor;
			var selected = Widgets.ButtonInvisible(rect);
			if (selected && !chosen)
			{
				SoundDefOf.RadioButtonClicked.PlayOneShotOnCamera();
			}
			listingStandard.Gap(listingStandard.verticalSpacing);
			return selected;
		}

		static readonly Action<float, float, bool> RadioButtonDraw =
			(Action<float, float, bool>)Delegate.CreateDelegate(typeof(Action<float, float, bool>),
				typeof(Widgets).GetMethod("RadioButtonDraw", AccessTools.all));
	}
}
