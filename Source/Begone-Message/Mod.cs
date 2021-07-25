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
			base.ExposeData();
			Scribe_Values.Look(ref rightClickDismissAll, nameof(rightClickDismissAll), false);
			Scribe_Values.Look(ref shiftRightClickDismissAll, nameof(shiftRightClickDismissAll), true);
		}
	}

	public class BegoneMessageMod : Mod
	{
		public static BegoneMessageModSettings Settings { get; private set; }

		public BegoneMessageMod(ModContentPack content) : base(content)
		{
			Settings = GetSettings<BegoneMessageModSettings>();
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			var listing = new ListingExtended
			{
				verticalSpacing = 12f,
			};
			listing.Begin(inRect);
			// All calls to Translate must be done after the game is initialized, so this can't be put into a static or instance initializer.
			var dismissOptions = new RadioButtonOption<bool>[]
			{
				RadioButtonOption.New("BegoneMessage_dismissSingleMessage".Translate(), false),
				RadioButtonOption.New("BegoneMessage_dismissAllMessages".Translate(), true),
			};
			listing.Label("BegoneMessage_rightClick".Translate());
			listing.LeftAlignedLabeledRadioButtonList(ref Settings.rightClickDismissAll, dismissOptions);
			listing.Label("BegoneMessage_shiftRightClick".Translate());
			listing.LeftAlignedLabeledRadioButtonList(ref Settings.shiftRightClickDismissAll, dismissOptions);
			listing.End();
		}

		public override string SettingsCategory() => "BegoneMessage_Name".Translate();
	}
}
