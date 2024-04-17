//using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;
//using Quintessential.Settings;
//using SDL2;
using System;
//using System.IO;
using System.Linq;
using System.Collections.Generic;
//using System.Reflection;

namespace TrueAnimismus;

using PartType = class_139;
using PartTypes = class_191;
using Texture = class_256;

public static class Wheel
{
    public static PartType Herriman;

    const float sixtyDegrees = 60f * (float)Math.PI / 180f;
    const string HerrimanWheelAtomsField = "TrueAnimismus_HerrimanWheelAtoms";

    static Texture[] HerrimanFlashAnimation;

    static class_126 atomCageLighting => class_238.field_1989.field_90.field_232;
    static PartType Berlo => PartTypes.field_1771;
    static HexRotation[] HexArmRotations => PartTypes.field_1767.field_1534;
    public static Molecule HerrimanMolecule()
    {
        Molecule molecule = new Molecule();
        molecule.method_1105(new Atom(ModdedAtoms.TrueVitae), new HexIndex(0, 1));
        molecule.method_1105(new Atom(ModdedAtoms.RedVitae), new HexIndex(1, 0));
        molecule.method_1105(new Atom(API.vitaeAtomType), new HexIndex(1, -1));
        molecule.method_1105(new Atom(API.morsAtomType), new HexIndex(0, -1));
        molecule.method_1105(new Atom(ModdedAtoms.GreyMors), new HexIndex(-1, 0));
        molecule.method_1105(new Atom(ModdedAtoms.TrueMors), new HexIndex(-1, 1));
        return molecule;
    }

    // ============================= //
    // public methods called by main
    //public static void LoadMirrorRules() => FTSIGCTU.MirrorTool.addRule(Herriman, FTSIGCTU.MirrorTool.mirrorVanBerlo);
    public static void DrawHerrimanFlash(SolutionEditorBase SEB, Part part, HexIndex hex)
    {
        SEB.field_3935.Add(new class_228(SEB, (enum_7)1, MainClass.hexGraphicalOffset(hex.Rotated(part.method_1163()) + part.method_1161()), HerrimanFlashAnimation, 30f, Vector2.Zero, 0f));
    }

    public static void drawSelectionGlow(SolutionEditorBase seb_self, Part part, Vector2 pos, float alpha)
    {
        var cageSelectGlowTexture = class_238.field_1989.field_97.field_367;
        int armLength = 1; // part.method_1165()
        class_236 class236 = seb_self.method_1989(part, pos);
        Color color = Color.White.WithAlpha(alpha);

        API.PrivateMethod<SolutionEditorBase>("method_2006").Invoke(seb_self, new object[] { armLength, HexArmRotations, class236, color });
        for (int index = 0; index < 6; ++index)
        {
            float num = index * sixtyDegrees;
            API.PrivateMethod<SolutionEditorBase>("method_2016").Invoke(seb_self, new object[] { cageSelectGlowTexture, color, class236.field_1984, class236.field_1985 + num });
        }
    }

    public static void drawHerrimanAtoms(SolutionEditorBase seb_self, Part part, Vector2 pos)
    {
        if (part.method_1159() != Herriman) return;
        PartSimState partSimState = seb_self.method_507().method_481(part);

        class_236 class236 = seb_self.method_1989(part, pos);
        Editor.method_925(GetHerrimanWheelAtoms(partSimState), class236.field_1984, new HexIndex(0, 0), class236.field_1985, 1f, 1f, 1f, false, seb_self);
    }

    public static Maybe<AtomReference> maybeFindHerrimanWheelAtom(Sim sim_self, Part part, HexIndex offset)
    {
        var SEB = sim_self.field_3818;
        var solution = SEB.method_502();
        var partList = solution.field_3919;
        var partSimStates = sim_self.field_3821;

        HexIndex key = part.method_1184(offset);
        foreach (var Herriman in partList.Where(x => x.method_1159() == Herriman))
        {
            var partSimState = partSimStates[Herriman];
            Molecule HerrimanAtoms = GetHerrimanWheelAtoms(partSimState);
            var hexIndex = partSimState.field_2724;
            var rotation = partSimState.field_2726;
            var hexKey = (key - hexIndex).Rotated(rotation.Negative());

            Atom atom;
            if (HerrimanAtoms.method_1100().TryGetValue(hexKey, out atom))
            {
                return (Maybe<AtomReference>)new AtomReference(HerrimanAtoms, hexKey, atom.field_2275, atom, true);
            }
        }
        return (Maybe<AtomReference>)struct_18.field_1431;
    }

    public static Maybe<AtomReference> maybeFindHerrimanWheelAtomSEB(Sim sim_self, Part part, HexIndex offset)
    {
        var SEB = sim_self.field_3818;
        var solution = SEB.method_502();
        var partList = solution.field_3919;
        var partSimStates = sim_self.field_3821;

        HexIndex key = part.method_1184(offset);
        foreach (var Herriman in partList.Where(x => x.method_1159() == Herriman))
        {
            var partSimState = partSimStates[Herriman];
            Molecule HerrimanAtoms = GetHerrimanWheelAtoms(partSimState);
            var hexIndex = partSimState.field_2724;
            var rotation = partSimState.field_2726;
            var hexKey = (key - hexIndex).Rotated(rotation.Negative());

            Atom atom;
            if (HerrimanAtoms.method_1100().TryGetValue(hexKey, out atom))
            {
                return (Maybe<AtomReference>)new AtomReference(HerrimanAtoms, hexKey, atom.field_2275, atom, true);
            }
        }
        return (Maybe<AtomReference>)struct_18.field_1431;
    }

    private static bool ContentLoaded = false;
    public static void LoadContent()
    {
        if (ContentLoaded) return;
        ContentLoaded = true;
        LoadTextureResources();
        //=========================//
        string iconpath = "textures/parts/icons/herriman";
        Herriman = new PartType()
        {
            /*ID*/
            field_1528 = "true-animismus-Herriman",
            /*Name*/
            field_1529 = class_134.method_253("Herriman's Wheel", string.Empty),
            /*Desc*/
            field_1530 = class_134.method_253("Herriman's Wheel can supply or receive from the Glyph of Infusion, or mediate half of the Glyphs of Animismus and Disproportion.", string.Empty),
            /*Cost*/
            field_1531 = 30,
            /*Type*/
            field_1532 = (enum_2)1,
            /*Programmable?*/
            field_1533 = true,
            /*Force-rotatable*/
            field_1536 = true,
            /*Berlo Atoms*/
            field_1544 = new Dictionary<HexIndex, AtomType>(),
            /*Icon*/
            field_1547 = class_235.method_615(iconpath),
            /*Hover Icon*/
            field_1548 = class_235.method_615(iconpath + "_hover"),
            /*Only One Allowed?*/
            field_1552 = true,
            CustomPermissionCheck = perms => perms.Contains(API.HerrimanPermission)
        };
        foreach (var hex in HexIndex.AdjacentOffsets) Herriman.field_1544.Add(hex, API.saltAtomType); //I don't know what this line does.

        QApi.AddPartTypeToPanel(Herriman, Berlo);
        QApi.AddPartType(Herriman, DrawHerrimanPart);
    }

    // private methods
    private static void SetHerrimanWheelData<T>(PartSimState state, string field, T data) => new DynamicData(state).Set(field, data);
    private static T GetHerrimanWheelData<T>(PartSimState state, string field, T initial)
    {
        var data = new DynamicData(state).Get(field);
        if (data == null)
        {
            SetHerrimanWheelData(state, field, initial);
            return initial;
        }
        else
        {
            return (T)data;
        }
    }
    private static Molecule GetHerrimanWheelAtoms(PartSimState state) => GetHerrimanWheelData(state, HerrimanWheelAtomsField, HerrimanMolecule());

    private static void LoadTextureResources()
    {
        HerrimanFlashAnimation = MainClass.fetchTextureArray(10, "textures/parts/herriman/herriman_flash.array/flash_");
    }

    static void DrawHerrimanPart(Part part, Vector2 pos, SolutionEditorBase editor, class_195 renderer)
    {
        // draw atoms, if the simulation is stopped - otherwise, the running simulation will draw them
        if (editor.method_503() == enum_128.Stopped)
        {
            drawHerrimanAtoms(editor, part, pos);
        }

        // draw arm stubs
        class_236 class236 = editor.method_1989(part, pos);
        API.PrivateMethod<SolutionEditorBase>("method_2005").Invoke(editor, new object[] { part.method_1165(), HexArmRotations, class236 });

        // draw cages
        PartSimState partSimState = editor.method_507().method_481(part);
        for (int i = 0; i < 6; i++)
        {
            float radians = renderer.field_1798 + (i * sixtyDegrees);
            Vector2 vector2_9 = renderer.field_1797 + MainClass.hexGraphicalOffset(new HexIndex(1, 0)).Rotated(radians);
            API.PrivateMethod<SolutionEditorBase>("method_2003").Invoke(editor, new object[] { atomCageLighting, vector2_9, new Vector2(39f, 33f), radians });
        }
    }
}
