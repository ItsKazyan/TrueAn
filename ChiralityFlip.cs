using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;
using SDL2;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Schema;
//using System.Reflection;

namespace TrueAnimismus;
using PartType = class_139;

public static class ChiralityFlip
{
    public static Part cloneandflipPart(Part orig)
	{
		//based off method_1175
		class_162.method_403(!orig.method_1171(), "Fixed parts cannot be cloned.");
        Part part;
        if (orig.method_1159() == Glyphs.Disproportion)
            {part = new Part(Glyphs.DisproportionR, false);} //Disp left becomes disp right
        else
            {part = new Part(Glyphs.Disproportion, false);}; //Disp right becomes disp left
		var partDyn = new DynamicData(part);
		partDyn.Set("field_2692", orig.method_1161());
		partDyn.Set("field_2693", orig.method_1163());
		partDyn.Set("field_2694", orig.method_1165());
		part.field_2695 = (Maybe<Part>)struct_18.field_1431;
		part.field_2697 = orig.field_2697.method_897();
		partDyn.Set("field_2698", orig.method_1167());
		partDyn.Set("field_2699", orig.method_1169());
		part.field_2702 = orig.field_2702;
		if (orig.method_1159().field_1542)
		{
			part.method_1194();
			foreach (HexIndex hexIndex in new DynamicData(orig).Get<List<HexIndex>>("field_2700"))
				part.method_1192(hexIndex);
		}
		if (orig.method_1159().field_1543)
		{
			part.field_2703 = orig.field_2703;
			part.method_1204(new List<HexIndex>(orig.method_1173()));
		}
		//part.method_1200();
		{
			for (int index = 0; index < part.field_2696.Length; ++index)
			{
				var subPart = part.field_2696[index];
				new DynamicData(subPart).Set("field_2692", Sim.class_284.method_230(part, index));
			}
		}
		return part;
	}

    private static PartType getPartType(Part part) => part.method_1159();
	private static PartType getDraggedPartType(PartDraggingInputMode.DraggedPart draggedPart) => getPartType(draggedPart.field_2722);
    public static void SolutionEditorScreen_method_50(SolutionEditorScreen SES_self)
        {   
            var current_interface = SES_self.field_4010;
            bool inDraggingMode = current_interface.GetType() == new PartDraggingInputMode().GetType();
            bool flipItGood = Input.IsSdlKeyPressed(SDL.enum_160.SDLK_c); // Press C to Flip

            // exit early if wrong mode
            if (!inDraggingMode) return;
            // exit early if not trying to flip
            if (!flipItGood) return;

            // time to flip chiralities!
            var interfaceDyn = new DynamicData(current_interface);
            var draggedParts = interfaceDyn.Get<List<PartDraggingInputMode.DraggedPart>>("field_2712");
            if (draggedParts.Count != 1) {MainClass.playSoundWithVolume(class_238.field_1991.field_1872, 0.5f); return;} // Only flip if we're dragging a single part; FTSIGCTU beep if multiple parts
            var cursorHex = interfaceDyn.Get<HexIndex>("field_2715");

            var ChiralityFlippedParts = new List<PartDraggingInputMode.DraggedPart>();

            foreach (var draggedPart in draggedParts) // There should be only one draggedPart in draggedParts by this point in the code; I may or may not fix this later.
            {
                PartType draggedPartType = getDraggedPartType(draggedPart);
                var partType = getDraggedPartType(draggedPart);

                if (partType != Glyphs.Disproportion && partType != Glyphs.DisproportionR ) // or however I'm support to check this
                {
                    //nothing to do; that's not a Glyph of Disproportion to flip
                    return;
                }
                else
                {
                    var part = draggedPart.field_2722;
                    var flippedPart = cloneandflipPart(part);
                    //MainClass.playSound(sim_self, class_238.field_1991.field_1855);  // 'sounds/piece_modify'
                    var ChiralityFlippedPart = new PartDraggingInputMode.DraggedPart()
                    {
                        field_2722 = flippedPart,
                        field_2723 = draggedPart.field_2723,
                        field_2207 = draggedPart.field_2207,
                    };
                    ChiralityFlippedParts.Add(ChiralityFlippedPart);
                }
            }
            interfaceDyn.Set("field_2712", ChiralityFlippedParts);
            MainClass.playSoundWithVolume(class_238.field_1991.field_1877, 0.2f);  // 'sounds/ui_transition_back'
        }
}