using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace BegoneMessage
{
	public class RadioButtonOption<T>
	{
		public readonly string labelText;
		public readonly T value;
		public readonly string tooltipText;

		public RadioButtonOption(string labelText, T value, string tooltipText = null)
		{
			this.labelText = labelText;
			this.value = value;
			this.tooltipText = tooltipText;
		}
	}

	// This class only exists for type inference convenience.
	public static class RadioButtonOption
	{
		public static RadioButtonOption<T> New<T>(string labelText, T value, string tooltipText = null) =>
			new RadioButtonOption<T>(labelText, value, tooltipText);
	}

	public class ListingExtended : Listing_Standard
	{
		public ListingExtended()
		{
		}

		public void SetupColumns(int numColumns)
		{
			curX = 0;
			if (numColumns < 1)
				throw new ArgumentOutOfRangeException(nameof(numColumns));
			ColumnWidth = (listingRect.width - (numColumns - 1) * ColumnSpacing) / numColumns;
		}

		// Returns true if settingsValue was changed.
		public void LeftAlignedLabeledRadioButtonList<T>(ref T settingsValue, params RadioButtonOption<T>[] options)
		{
			foreach (var option in options)
			{
				if (LeftAlignedLabeledRadioButton(option.labelText, Equals(settingsValue, option.value), option.tooltipText))
					settingsValue = option.value;
			}
		}

		public bool LeftAlignedLabeledRadioButton(string labelText, bool chosen,
			string tooltipText = null)
		{
			// Annoyingly, Widgets.RadioButton/RadioButtonLabeled has an unwanted mouseover sound effect,
			// and Widgets.RadioButtonLabeled also right-aligns the radio buttons, so we can't use either method.
			var rect = GetRect(Text.LineHeight);
			if (!tooltipText.NullOrEmpty())
			{
				TooltipHandler.TipRegion(rect, tooltipText);
			}
			RadioButtonDraw(rect.x + Widgets.RadioButtonSize * 0.25f, rect.y + (rect.height - Widgets.RadioButtonSize) / 2f, chosen);
			var origAnchor = Text.Anchor;
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect.RightPartPixels(rect.width - Widgets.RadioButtonSize * 1.5f), labelText);
			Text.Anchor = origAnchor;
			var selected = Widgets.ButtonInvisible(rect);
			if (selected && !chosen)
			{
				SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
			}
			Gap(verticalSpacing);
			return selected;
		}

		static readonly Action<float, float, bool> RadioButtonDraw =
			(Action<float, float, bool>)Delegate.CreateDelegate(typeof(Action<float, float, bool>),
				typeof(Widgets).GetMethod("RadioButtonDraw", AccessTools.all));
	}
}
