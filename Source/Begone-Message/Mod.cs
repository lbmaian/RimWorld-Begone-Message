using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BegoneMessage
{
	public class BegoneMessageModSettings : ModSettings
	{
		public bool rightClickDismissAll = false;
		public bool shiftRightClickDismissAll = true;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref rightClickDismissAll, nameof(rightClickDismissAll), false);
			Scribe_Values.Look(ref shiftRightClickDismissAll, nameof(shiftRightClickDismissAll), true);
			base.ExposeData();
		}
	}

	public class BegoneMessageMod : Mod
	{
		public static BegoneMessageModSettings Settings;

		public BegoneMessageMod(ModContentPack content) : base(content)
		{
			Settings = GetSettings<BegoneMessageModSettings>();
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			var listingStandard = new Listing_Standard
			{
				verticalSpacing = 12f
			};
			listingStandard.Begin(inRect);
			// All calls to Translate must be done after the game is initialized, so this can't be put into a static or instance initializer.
			var dismissOptions = new Dictionary<string, bool>()
					   {
							   { "BegoneMessage_dismissSingleMessage".Translate(), false },
							   { "BegoneMessage_dismissAllMessages".Translate(), true },
					   };
			listingStandard.LeftAlignedLabeledRadioButtonList("BegoneMessage_rightClick".Translate(), dismissOptions,
					ref Settings.rightClickDismissAll);
			listingStandard.LeftAlignedLabeledRadioButtonList("BegoneMessage_shiftRightClick".Translate(), dismissOptions,
					ref Settings.shiftRightClickDismissAll);
			listingStandard.End();
			base.DoSettingsWindowContents(inRect);
		}

		public override string SettingsCategory() => "BegoneMessage_Name".Translate();
	}
}
