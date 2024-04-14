using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;
using System.CodeDom;
using System.Collections.Specialized;
using System.Data.Odbc;
using System.Drawing.Text;
using System.Dynamic;



//using Quintessential.Settings;
//using SDL2;
//using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;


//OVerall structure of the code and a whole bunch of helper methods are from Reductive Metallurgy, yay for open source

namespace TrueAnimismus;

using PartType = class_139;
using PartTypes = class_191;
using Texture = class_256;

public static class Glyphs
{
    public static PartType Disproportion, DisproportionR, DispoJack, LeftHand, Infusion;
    //const string ProliferationPrevStateField = "ReductiveMetallurgy_ProliferationPrevState";
    //const string ProliferationPrevCycleField = "ReductiveMetallurgy_ProliferationPrevCycle";

    public static Sound disproportionSound, lefthandSound, infusionSound;
    public static string disproportionSoundName = "glyph_disproportion";
    public static string lefthandSoundName = "glyph_lefthand";
    public static string infusionSoundName = "glyph_infusion";

    public static Texture disproportion_base_L, disproportion_base_R, disproportion_connectors_L, disproportion_connectors_R,
        disproportion_gloss_L, disproportion_gloss_R, disproportion_glossMask_L, disproportion_glossMask_R,
        animismus_outputUnderIris, animismus_outputAboveIris, bonderShadow,
        indeterminate_symbol, animismus_input, animismus_symbol;
    public static Texture[] irisFullArray = new Texture[16];

    public static Texture[] lettersFullArray = new Texture[16];
    public static Texture dispojack_base = new Texture();
    public static Texture dispojack_face = new Texture();
    public static Texture dispojack_details = new Texture();

    public static Texture circleglow, circlestroke, singleglow, singlestroke;

    private static PartType makeGlyph(
        string id,
        string name,
        string desc,
        int cost,
        HexIndex[] footprint,
        Texture icon,
        Texture hover,
        Texture glow,
        Texture stroke,
        string permission,
        bool onlyOne = false)
    {
        PartType ret = new PartType()
        {
            /*ID*/
            field_1528 = id,
            /*Name*/
            field_1529 = class_134.method_253(name, string.Empty),
            /*Desc*/
            field_1530 = class_134.method_253(desc, string.Empty),
            /*Cost*/
            field_1531 = cost,
            /*Is a Glyph?*/
            field_1539 = true,
            /*Hex Footprint*/
            field_1540 = footprint,
            /*Icon*/
            field_1547 = icon,
            /*Hover Icon*/
            field_1548 = hover,
            /*Glow (Shadow)*/
            field_1549 = glow,
            /*Stroke (Outline)*/
            field_1550 = stroke,
            /*Only One Allowed?*/
            field_1552 = onlyOne,
            CustomPermissionCheck = perms => perms.Contains(permission)
        };
        return ret;
    }

    #region drawingHelpers
    private static Vector2 hexGraphicalOffset(HexIndex hex) => MainClass.hexGraphicalOffset(hex);
    private static Vector2 textureDimensions(Texture tex) => tex.field_2056.ToVector2();
    private static Vector2 textureCenter(Texture tex) => (textureDimensions(tex) / 2).Rounded();
    private static void drawPartGraphic(class_195 renderer, Texture tex, Vector2 graphicPivot, float graphicAngle, Vector2 graphicTranslation, Vector2 screenTranslation)
    {
        drawPartGraphicScaled(renderer, tex, graphicPivot, graphicAngle, graphicTranslation, screenTranslation, new Vector2(1f, 1f));
    }

    private static void drawPartGraphicScaled(class_195 renderer, Texture tex, Vector2 graphicPivot, float graphicAngle, Vector2 graphicTranslation, Vector2 screenTranslation, Vector2 scaling)
    {
        //for graphicPivot and graphicTranslation, rightwards is the positive-x direction and upwards is the positive-y direction
        //graphicPivot is an absolute position, with (0,0) denoting the bottom-left corner of the texture
        //graphicTranslation is a translation, so (5,-3) means "translate 5 pixels right and 3 pixels down"
        //graphicAngle is measured in radians, and counterclockwise is the positive-angle direction
        //screenTranslation is the final translation applied, so it is not affected by rotations
        Matrix4 matrixScreenPosition = Matrix4.method_1070(renderer.field_1797.ToVector3(0f));
        Matrix4 matrixTranslateOnScreen = Matrix4.method_1070(screenTranslation.ToVector3(0f));
        Matrix4 matrixRotatePart = Matrix4.method_1073(renderer.field_1798);
        Matrix4 matrixTranslateGraphic = Matrix4.method_1070(graphicTranslation.ToVector3(0f));
        Matrix4 matrixRotateGraphic = Matrix4.method_1073(graphicAngle);
        Matrix4 matrixPivotOffset = Matrix4.method_1070(-graphicPivot.ToVector3(0f));
        Matrix4 matrixScaling = Matrix4.method_1074(scaling.ToVector3(0f));
        Matrix4 matrixTextureSize = Matrix4.method_1074(tex.field_2056.ToVector3(0f));

        Matrix4 matrix4 = matrixScreenPosition * matrixTranslateOnScreen * matrixRotatePart * matrixTranslateGraphic * matrixRotateGraphic * matrixPivotOffset * matrixScaling * matrixTextureSize;
        class_135.method_262(tex, Color.White, matrix4);
    }

    public static void LoadAllCustomSounds()
    {
        foreach (var dir in QuintessentialLoader.ModContentDirectories)
        {
            string disproPath = Path.Combine(dir, "Content/sounds/" + disproportionSoundName + ".wav");
            if (File.Exists(disproPath))
            {
                disproportionSound = new Sound()
                {
                    field_4060 = Path.GetFileNameWithoutExtension(disproPath),
                    field_4061 = class_158.method_375(disproPath)
                };
                string lefthandPath = Path.Combine(dir, "Content/sounds/" + lefthandSoundName + ".wav");
                lefthandSound = new Sound()
                {
                    field_4060 = Path.GetFileNameWithoutExtension(lefthandPath),
                    field_4061 = class_158.method_375(lefthandPath)
                };
                string infusionPath = Path.Combine(dir, "Content/sounds/" + infusionSoundName + ".wav");
                infusionSound = new Sound()
                {
                    field_4060 = Path.GetFileNameWithoutExtension(infusionPath),
                    field_4061 = class_158.method_375(infusionPath)
                };
                break;
            }
        }
        // add entry to the volume dictionary
        var field = typeof(class_11).GetField("field_52", BindingFlags.Static | BindingFlags.NonPublic);
        var dictionary = (Dictionary<string, float>)field.GetValue(null);
        dictionary.Add(disproportionSoundName, 0.2f);
        dictionary.Add(lefthandSoundName, 0.4f);
        dictionary.Add(infusionSoundName, 0.2f);

        // modify the method that reenables sounds after they are triggered
        void Method_540(On.class_201.orig_method_540 orig, class_201 class201_self)
        {
            orig(class201_self);
            disproportionSound.field_4062 = false;
            lefthandSound.field_4062 = false;
            infusionSound.field_4062 = false;
        }
        On.class_201.method_540 += Method_540;
    }

    private static void drawPartGraphicSpecular(class_195 renderer, Texture tex, Vector2 graphicPivot, float graphicAngle, Vector2 graphicTranslation, Vector2 screenTranslation)
    {
        float specularAngle = (renderer.field_1799 - (renderer.field_1797 + graphicTranslation.Rotated(renderer.field_1798))).Angle() - 1.570796f - renderer.field_1798;
        drawPartGraphic(renderer, tex, graphicPivot, graphicAngle + specularAngle, graphicTranslation, screenTranslation);
    }

    private static void drawPartGloss(class_195 renderer, Texture gloss, Texture glossMask, Vector2 offset)
    {
        drawPartGloss(renderer, gloss, glossMask, offset, new HexIndex(0, 0), 0f);
    }
    private static void drawPartGloss(class_195 renderer, Texture gloss, Texture glossMask, Vector2 offset, HexIndex hexOffset, float angle)
    {
        class_135.method_257().field_1692 = class_238.field_1995.field_1757; // MaskedGlossPS shader
        class_135.method_257().field_1693[1] = gloss;
        var hex = new HexIndex(0, 0);
        Vector2 method2001 = 0.0001f * (renderer.field_1797 + hexGraphicalOffset(hex).Rotated(renderer.field_1798) - 0.5f * class_115.field_1433);
        class_135.method_257().field_1695 = method2001;
        drawPartGraphic(renderer, glossMask, offset, angle, hexGraphicalOffset(hexOffset), Vector2.Zero);
        class_135.method_257().field_1692 = class_135.method_257().field_1696; // previous shader
        class_135.method_257().field_1693[1] = class_238.field_1989.field_71;
        class_135.method_257().field_1695 = Vector2.Zero;
    }
    public static void drawAtomIO(class_195 renderer, AtomType atomType, HexIndex hex, float num)
    {
        Molecule molecule = Molecule.method_1121(atomType);
        Vector2 method1999 = renderer.field_1797 + hexGraphicalOffset(hex).Rotated(renderer.field_1798);
        Editor.method_925(molecule, method1999, new HexIndex(0, 0), 0f, 1f, num, 1f, false, null);
    }
    #endregion

    // private static bool AtopAnotherPart(Part part, Solution soln)
    // {
    // 	HashSet<HexIndex> hashSet = part.method_1187(soln, (enum_137)0, part.method_1161(), part.method_1163()); /*Hex footprint of the one passed here*/
    // 	HashSet<HexIndex> other = soln.method_1947(part, (enum_137)0); /*Hex footprints of every part that's not the one passed here*/
    // 	return hashSet.Overlaps(other); //Atop another part if Overlaps() returns true
    // }
    private static bool GlyphBelowIsFiring(Part part, SolutionEditorBase SEB)
    {
        //Jank code because I don't actually understand the right way to use HexIndexes. 
        HexIndex h0 = new HexIndex(0, 0);
        HashSet<HexIndex> hashSet = new HashSet<HexIndex> { h0.Rotated(part.method_1163()) + part.method_1161() };
        foreach (Part item in SEB.method_502().field_3919.Where(x => x.method_1159().field_1539 && (x != part) /*Only checking glyphs and excluding the Dispojack itself*/))
        {
            if (SEB.method_507().method_481(item).field_2743
            &&
            hashSet.Overlaps(item.method_1187(SEB.method_502(), (enum_137)0, item.method_1161(), item.method_1163()))
            )
            { return true; /*Glyph is firing and it's on the same hex as dispojack*/}
        }
        return false;
    }
    // private static bool PartsIntersect(Part part1, Part part2, Solution soln)
    // {
    // 	HashSet<HexIndex> part1footprint = part1.method_1187(soln, (enum_137)0, part1.method_1161(), part1.method_1163()); /*Hex footprint*/
    // 	HashSet<HexIndex> part2footprint = part2.method_1187(soln, (enum_137)0, part2.method_1161(), part2.method_1163()); /*Hex footprint*/

    // 	return part1footprint.Overlaps(part2footprint); //Atop another part if Overlaps() returns true
    // }

    public static bool AtopIrisHoverHex(Part irispart, HexIndex hoverhex, Solution soln)
    {
        HashSet<HexIndex> hh = new HashSet<HexIndex> { hoverhex };
        HashSet<HexIndex> irisfootprint = new HashSet<HexIndex>();

        if (irispart.method_1159() == PartTypes.field_1780)
        {
            // Animismus irises: 0,1 and  1,-1
            HexIndex topiris = new HexIndex(0, 1);
            HexIndex bottomiris = new HexIndex(1, -1);

            irisfootprint.Add(topiris.Rotated(irispart.method_1163()) + irispart.method_1161());
            irisfootprint.Add(bottomiris.Rotated(irispart.method_1163()) + irispart.method_1161());
            return hh.Overlaps(irisfootprint);
        };
        if (irispart.method_1159() == Disproportion || irispart.method_1159() == DisproportionR)
        {
            // Disproportion irises: -1,0 and 1,0
            // Same for both chiralities
            HexIndex leftiris = new HexIndex(-1, 0);
            HexIndex rightiris = new HexIndex(1, 0);

            irisfootprint.Add(leftiris.Rotated(irispart.method_1163()) + irispart.method_1161());
            irisfootprint.Add(rightiris.Rotated(irispart.method_1163()) + irispart.method_1161());
            return hh.Overlaps(irisfootprint);
        };
        if (irispart.method_1159() == LeftHand)
        {
            // Left Hand iris: 1,0
            HexIndex rightiris = new HexIndex(1, 0);
            irisfootprint.Add(rightiris.Rotated(irispart.method_1163()) + irispart.method_1161());
            return hh.Overlaps(irisfootprint);
        };
        return false; //Should only get here if something is wrong. Just don't let the dispojack overlap the whatever in that case.
    }

    public static bool AtopIris(Part irispart, Part dispojack, Solution soln)
    {
        HashSet<HexIndex> dispojackfootprint = dispojack.method_1187(soln, (enum_137)0, dispojack.method_1161(), dispojack.method_1163()); /*Hex footprint*/
        HashSet<HexIndex> irisfootprint = new HashSet<HexIndex>();

        if (irispart.method_1159() == PartTypes.field_1780)
        {
            // Animismus irises: 0,1 and  1,-1
            HexIndex topiris = new HexIndex(0, 1);
            HexIndex bottomiris = new HexIndex(1, -1);

            irisfootprint.Add(topiris.Rotated(irispart.method_1163()) + irispart.method_1161());
            irisfootprint.Add(bottomiris.Rotated(irispart.method_1163()) + irispart.method_1161());
            return dispojackfootprint.Overlaps(irisfootprint);
        };
        if (irispart.method_1159() == Disproportion || irispart.method_1159() == DisproportionR)
        {
            // Disproportion irises: -1,0 and 1,0
            // Same for both chiralities
            HexIndex leftiris = new HexIndex(-1, 0);
            HexIndex rightiris = new HexIndex(1, 0);

            irisfootprint.Add(leftiris.Rotated(irispart.method_1163()) + irispart.method_1161());
            irisfootprint.Add(rightiris.Rotated(irispart.method_1163()) + irispart.method_1161());
            return dispojackfootprint.Overlaps(irisfootprint);
        };
        if (irispart.method_1159() == LeftHand)
        {
            // Left Hand iris: 1,0
            HexIndex rightiris = new HexIndex(1, 0);

            irisfootprint.Add(rightiris.Rotated(irispart.method_1163()) + irispart.method_1161());
            return dispojackfootprint.Overlaps(irisfootprint);
        };
        return false; //Should only get here if something is wrong. Just don't let the dispojack overlap the whatever in that case.
    }



    private static bool ContentLoaded = false;

    // Helper from main class

    public static void LoadContent()
    {
        if (ContentLoaded) return;
        ContentLoaded = true;

        API.addDisproportionRule(API.vitaeAtomType, ModdedAtoms.RedVitae, API.saltAtomType);
        API.addDisproportionRule(ModdedAtoms.RedVitae, ModdedAtoms.TrueVitae, API.vitaeAtomType);
        API.addDisproportionRule(API.morsAtomType, ModdedAtoms.GreyMors, API.saltAtomType);
        API.addDisproportionRule(ModdedAtoms.GreyMors, ModdedAtoms.TrueMors, API.morsAtomType);

        API.addLeftHandRule(API.vitaeAtomType, API.morsAtomType);
        API.addLeftHandRule(ModdedAtoms.RedVitae, ModdedAtoms.GreyMors);
        API.addLeftHandRule(ModdedAtoms.TrueVitae, ModdedAtoms.TrueMors);
        API.addLeftHandRule(API.morsAtomType, API.vitaeAtomType);
        API.addLeftHandRule(ModdedAtoms.GreyMors, ModdedAtoms.RedVitae);
        API.addLeftHandRule(ModdedAtoms.TrueMors, ModdedAtoms.TrueVitae);
        API.addLeftHandRule(API.saltAtomType, API.saltAtomType); //0 x -1 = 0
        Logger.Log("Adding Glyph Rules for the higher grades of animismus!");

        LoadAllCustomSounds();

        // create parts
        string path, iconpath, selectpath;
        path = "textures/";
        iconpath = path + "parts/icons/";
        selectpath = path + "select/";

        Disproportion = makeGlyph(
            "true-animismus-disproportion",
            "Glyph of Disproportion",
            "The glyph of disproportion converts two identical atoms of animismus into one of higher grade and one of lower grade.",
            20, new HexIndex[4] { new HexIndex(0, -1), new HexIndex(1, -1), new HexIndex(-1, 0), new HexIndex(1, 0) },
            class_235.method_615(iconpath + "disproportion"),
            class_235.method_615(iconpath + "disproportion_hover"),
            class_235.method_615(selectpath + "disproportion_glow"),
            class_235.method_615(selectpath + "disproportion_stroke"),
            "TrueAnimismus:disproportion"
        );

        DisproportionR = makeGlyph(
            "true-animismus-disproportion-r",
            "Glyph of Disproportion (Right-Handed)",
            "The glyph of disproportion converts two identical atoms of animismus into one of higher grade and one of lower grade.",
            20, new HexIndex[4] { new HexIndex(0, -1), new HexIndex(1, -1), new HexIndex(-1, 0), new HexIndex(1, 0) },
            class_235.method_615(iconpath + "disproportion"),
            class_235.method_615(iconpath + "disproportion_hover"),
            class_235.method_615(selectpath + "disproportion_glow"),
            class_235.method_615(selectpath + "disproportion_stroke"),
            "TrueAnimismus:disproportion"
        );

        //Going to be switching between glows and strokes a lot for DispoJack, so, loading them in advance:
        singleglow = class_238.field_1989.field_97.field_382;
        singlestroke = class_238.field_1989.field_97.field_383;
        circleglow = class_235.method_615(selectpath + "circle_glow");
        circlestroke = class_235.method_615(selectpath + "circle_stroke");

        DispoJack = makeGlyph(
            "true-animismus-dispojack",
            "Disposal Jack",
            "When placed over the iris of an animismus-producing glyph, the Disposal Jack disposes atoms emerging from that iris.",
            20, new HexIndex[1] { new HexIndex(0, 0) },
            class_235.method_615(iconpath + "dispojack"),
            class_235.method_615(iconpath + "dispojack_hover"),
            singleglow,
            singlestroke,
            "TrueAnimismus:dispojack",
            true // Only one dispojack per solution
        );

        LeftHand = makeGlyph(
             "true-animismus-lefthand",
             "Glyph of the Left Hand",
             "The glyph of the left hand inverts an atom of animismus.",
             20, new HexIndex[3] { new HexIndex(0, 0), new HexIndex(1, 0), new HexIndex(-1, 0) },
             class_235.method_615(iconpath + "lefthand"),
             class_235.method_615(iconpath + "lefthand_hover"),
             class_235.method_615(selectpath + "line_glow"),
             class_235.method_615(selectpath + "line_stroke"),
             "TrueAnimismus:lefthand"
        );

        Infusion = makeGlyph(
            "true-animismus-infusion",
            "Glyph of Infusion",
            "The glyph of infusion transfers one unit of animismus from one atom to another, provided they are not in opposition.",
            30, new HexIndex[2] { new HexIndex(0, 0), new HexIndex(1, 0) },
            class_235.method_615(iconpath + "infusion"),
            class_235.method_615(iconpath + "infusion_hover"),
            class_238.field_1989.field_97.field_374, // double_glow
            class_238.field_1989.field_97.field_375, // double_stroke
            "TrueAnimismus:infusion"
        );

        var projector = PartTypes.field_1778;
        var purifier = PartTypes.field_1779;
        var animismer = PartTypes.field_1780;
        var disposer = PartTypes.field_1781;
        QApi.AddPartTypeToPanel(Disproportion, animismer); //Don't add right-handed Disproportion; you get that by pressing C while dragging left-handed Disproportion
        QApi.AddPartTypeToPanel(DispoJack, disposer);
        QApi.AddPartTypeToPanel(LeftHand, animismer);
        QApi.AddPartTypeToPanel(Infusion, animismer);

        //Textures used by multiple glyphs
        animismus_input = class_235.method_615("textures/parts/input");
        animismus_symbol = class_235.method_615("textures/parts/animismus_symbol");

        path = "textures/parts/disproportion/";
        disproportion_base_L = class_235.method_615(path + "base_L");
        disproportion_connectors_L = class_235.method_615(path + "connectors");
        disproportion_gloss_L = class_235.method_615(path + "gloss");
        disproportion_glossMask_L = class_235.method_615(path + "gloss_mask");
        // Uses animismus_input
        // Uses animismus_symbol

        disproportion_base_R = class_235.method_615(path + "base_R");
        disproportion_connectors_R = class_235.method_615(path + "connectors");
        disproportion_gloss_R = class_235.method_615(path + "gloss"); // todo: reference vanilla texture
        disproportion_glossMask_R = class_235.method_615(path + "gloss_mask");
        indeterminate_symbol = class_235.method_615(path + "indeterminate_symbol");
        // Uses animismus_input
        // Uses animismus_symbol

        path = "textures/parts/disposal_jack/";
        dispojack_base = class_238.field_1989.field_90.field_169; //vanilla calcinator base
        dispojack_face = class_235.method_615(path + "face");
        dispojack_details = class_235.method_615(path + "details");

        path = "textures/parts/lefthand/";
        Texture lefthand_base = class_235.method_615(path + "base");
        Texture lefthand_gloss = class_235.method_615(path + "gloss");
        Texture lefthand_glossMask = class_235.method_615(path + "gloss_mask");
        // Uses animismus_input
        // Uses animismus_symbol
        Texture lefthand_markerDetails = class_235.method_615(path + "marker_details");
        Texture lefthand_markerLighting = class_235.method_615(path + "marker_lighting");

        path = "textures/parts/infusion/";
        Texture infusion_base = class_235.method_615(path + "base");
        Texture infusion_inputBowl = class_235.method_615(path + "big_bowl");
        Texture infusion_gloss = class_235.method_615(path + "gloss");
        Texture infusion_glossMask = class_235.method_615(path + "gloss_mask");
        Texture infusion_connectors = class_235.method_615(path + "connectors");
        Texture infusion_outputBowl = class_235.method_615(path + "small_bowl");
        Texture infusion_animismusSymbolDown = class_235.method_615(path + "animismus_symbol_down");



        // fetch vanilla textures
        bonderShadow = class_238.field_1989.field_90.field_164;

        animismus_outputAboveIris = class_238.field_1989.field_90.field_228.field_271;
        animismus_outputUnderIris = class_238.field_1989.field_90.field_228.field_272;
        Texture animismus_ringShadow = class_238.field_1989.field_90.field_228.field_273;





        Texture projectionGlyph_base = class_238.field_1989.field_90.field_255.field_288;
        Texture projectionGlyph_bond = class_238.field_1989.field_90.field_255.field_289;
        Texture projectionGlyph_quicksilverInput = class_238.field_1989.field_90.field_255.field_293;

        irisFullArray = class_238.field_1989.field_90.field_246;
        lettersFullArray = MainClass.fetchTextureArray(12, "animations/dispojack_flash.array/dispojack_flash_");
        Texture[] discoFullArray = MainClass.fetchTextureArray(16, "animations/disco.array/disco_");

        QApi.AddPartType(Disproportion, (part, pos, editor, renderer) =>
        {
            DrawChiralDisproportion(part, editor, renderer, true); //true means lefty
        });

        QApi.AddPartType(DisproportionR, (part, pos, editor, renderer) =>
        {
            DrawChiralDisproportion(part, editor, renderer, false); //false means righty
        });

        QApi.AddPartType(DispoJack, (part, pos, editor, renderer) =>
        {
            // This space intentionally left blank
            // Disposal Jack has to be drawn in a special way--after every other glyph
            // This is done 'manually' elsewhere 
        });

        QApi.AddPartType(LeftHand, (part, pos, editor, renderer) =>
        {
            PartSimState partSimState = editor.method_507().method_481(part);
            var simTime = editor.method_504();

            var originHex = new HexIndex(0, 0);
            var leftHex = new HexIndex(-1, 0);
            var inputHex = leftHex;
            var rightHex = new HexIndex(1, 0);

            float partAngle = renderer.field_1798;
            Vector2 base_offset = new Vector2(123f, 48f);

            int index = irisFullArray.Length - 1;
            float num = 0f;
            bool flag = false;

            if (partSimState.field_2743) // Which iris frame is being drawn?
            {
                index = class_162.method_404((int)(class_162.method_411(1f, -1f, simTime) * irisFullArray.Length), 0, irisFullArray.Length - 1);
                num = simTime;
                flag = (double)simTime > 0.5;
            }

            bool hasdispojack = false;
            foreach (Part dispojack in editor.method_502().field_3919.Where(x => x.method_1159() == DispoJack))
            {   //Did you put a dispojack on me, istg
                if (dispojack.method_1161() == rightHex.Rotated(part.method_1163()) + part.method_1161())
                { hasdispojack = true; }
            }

            drawPartGraphic(renderer, lefthand_base, base_offset, 0f, Vector2.Zero, new Vector2(-1f, -1f));
            drawPartGraphic(renderer, bonderShadow, textureCenter(bonderShadow), 0f, hexGraphicalOffset(inputHex), new Vector2(0f, -3f));
            foreach (var hex in new HexIndex[1] { rightHex })
            {
                drawPartGraphic(renderer, bonderShadow, textureCenter(bonderShadow), 0f, hexGraphicalOffset(hex), new Vector2(0f, -3f));
                drawPartGraphicSpecular(renderer, animismus_outputUnderIris, textureCenter(animismus_outputUnderIris), 0f, hexGraphicalOffset(hex), Vector2.Zero);
                if (partSimState.field_2743 && !flag)
                {
                    if (!hasdispojack) { drawAtomIO(renderer, partSimState.field_2744[0], hex, num); }
                }
                drawPartGraphic(renderer, irisFullArray[index], textureCenter(irisFullArray[index]), -partAngle, hexGraphicalOffset(hex), Vector2.Zero);
                drawPartGraphicSpecular(renderer, animismus_outputAboveIris, textureCenter(animismus_outputAboveIris), 0f, hexGraphicalOffset(hex), Vector2.Zero);
                if (flag)
                {
                    if (!hasdispojack) { drawAtomIO(renderer, partSimState.field_2744[0], hex, num); }
                }
            }

            drawPartGraphicSpecular(renderer, animismus_input, textureCenter(animismus_input), 0f, hexGraphicalOffset(inputHex), Vector2.Zero);
            drawPartGraphic(renderer, animismus_symbol, textureCenter(animismus_symbol), -partAngle, hexGraphicalOffset(inputHex), Vector2.Zero);

            // Random-looking but deterministic values from -2 to 2, chosen so they're 0 if iris-opening index is max.
            // Not cryptographically secure randomness or anything, just does the shakey shakey
            int shakeX = ((((index + 1) * 3) ^ ((index + 1) >> 2)) % 5) - 2;
            int shakeY = (((index * 5) ^ (index >> 2)) % 5) - 2;

            drawPartGraphicSpecular(renderer, lefthand_markerLighting, textureCenter(lefthand_markerLighting), 0f, hexGraphicalOffset(originHex), Vector2.Zero);
            drawPartGraphic(renderer, lefthand_markerDetails, textureCenter(lefthand_markerDetails), -partAngle, hexGraphicalOffset(originHex), new Vector2(shakeX, shakeY));
            drawPartGloss(renderer, lefthand_gloss, lefthand_glossMask, base_offset + new Vector2(0f, -1f));

            if (partSimState.field_2743)
            { // Quintessence disco; some code swiped from RM's Glyph of Proliferation
                Atom atom;
                HexIndex key = part.method_1184(originHex);
                foreach (Molecule molecule in editor.method_507().method_483().Where(x => x.method_1100().Count == 1)) // foreach one-atom molecule
                {
                    if (molecule.method_1100().TryGetValue(key, out atom) && atom.field_2275 == API.quintessenceAtomType)
                    {
                        drawPartGraphic(renderer, discoFullArray[index], textureCenter(discoFullArray[index]), -partAngle, hexGraphicalOffset(originHex), Vector2.Zero);
                        break;
                    }
                }
            }

        });

        QApi.AddPartType(Infusion, (part, pos, editor, renderer) =>
        {
            PartSimState partSimState = editor.method_507().method_481(part);
            var simTime = editor.method_504();

            var inputHex = new HexIndex(0, 0);
            var outputHex = new HexIndex(1, 0);

            float partAngle = renderer.field_1798;
            Vector2 base_offset = new Vector2(41f, 48f);
            drawPartGraphic(renderer, infusion_base, base_offset, 0.0f, Vector2.Zero, new Vector2(-1f, -1f));

            // bowl connectors
            drawPartGraphic(renderer, infusion_connectors, base_offset, 0f, new Vector2(-1f, 0f), Vector2.Zero);
            drawPartGloss(renderer, infusion_gloss, infusion_glossMask, base_offset + new Vector2(-1f, 0f));

            //draw donor bowl
            drawPartGraphic(renderer, bonderShadow, textureDimensions(bonderShadow) / 2, 0f, hexGraphicalOffset(inputHex), new Vector2(0.0f, -3f));
            drawPartGraphicSpecular(renderer, infusion_inputBowl, textureCenter(infusion_inputBowl), 0f, hexGraphicalOffset(inputHex), Vector2.Zero);
            drawPartGraphic(renderer, infusion_animismusSymbolDown, textureCenter(infusion_animismusSymbolDown), -partAngle, hexGraphicalOffset(inputHex), Vector2.Zero);

            //draw reciever bowl
            drawPartGraphicSpecular(renderer, infusion_outputBowl, textureCenter(infusion_outputBowl), 0f, hexGraphicalOffset(outputHex), Vector2.Zero);
        });
    }
    public static void DrawChiralDisproportion(Part part, SolutionEditorBase editor, class_195 renderer, bool lefty)
    {
        HexIndex originHex, leftinputHex, rightinputHex, leftoutputHex, rightoutputHex;
        if (lefty)
        {
            originHex = new HexIndex(0, 0);
            leftinputHex = new HexIndex(0, -1);
            rightinputHex = new HexIndex(1, -1);
            leftoutputHex = new HexIndex(-1, 0);
            rightoutputHex = new HexIndex(1, 0);
        }
        else
        { //mirrored. The 'left' hexes are on the right for DisproportionR.  
            originHex = new HexIndex(0, 0);
            leftinputHex = new HexIndex(1, -1);
            rightinputHex = new HexIndex(0, -1);
            leftoutputHex = new HexIndex(1, 0);
            rightoutputHex = new HexIndex(-1, 0);
        }

        PartSimState partSimState = editor.method_507().method_481(part);
        var simTime = editor.method_504();

        float partAngle = renderer.field_1798;
        Vector2 base_offset = new Vector2(lefty ? 132f : 133f, 173f); //X-offsets slightly different from the right-handed version due to image editing mistake

        int index = irisFullArray.Length - 1;

        float num = 0f;
        bool flag = false;
        if (partSimState.field_2743) // Which iris frame to draw. If they're not opening, irises stay closed. 
        {
            index = class_162.method_404((int)(class_162.method_411(1f, -1f, simTime) * irisFullArray.Length), 0, irisFullArray.Length - 1);
            num = simTime;
            flag = (double)simTime > 0.5;
        }

        bool Ldispojack = false;
        bool Rdispojack = false;
        foreach (Part dispojack in editor.method_502().field_3919.Where(x => x.method_1159() == DispoJack))
        {   //Did you put a dispojack on me, istg
            if (dispojack.method_1161() == leftoutputHex.Rotated(part.method_1163()) + part.method_1161())
            { Ldispojack = true; }
            if (dispojack.method_1161() == rightoutputHex.Rotated(part.method_1163()) + part.method_1161())
            { Rdispojack = true; }
        }

        Texture disproportion_base = lefty ? disproportion_base_L : disproportion_base_R;
        Texture disproportion_connectors = lefty ? disproportion_connectors_L : disproportion_connectors_R;
        Texture disproportion_gloss = lefty ? disproportion_gloss_L : disproportion_gloss_R;
        Texture disproportion_glossMask = lefty ? disproportion_glossMask_L : disproportion_glossMask_R;

        drawPartGraphic(renderer, disproportion_base, base_offset, 0f, Vector2.Zero, new Vector2(-1f, -1f));
        drawPartGraphic(renderer, bonderShadow, textureCenter(bonderShadow), 0f, hexGraphicalOffset(leftinputHex), new Vector2(0f, -3f));
        drawPartGraphic(renderer, bonderShadow, textureCenter(bonderShadow), 0f, hexGraphicalOffset(rightinputHex), new Vector2(0f, -3f));
        foreach (var hex in new HexIndex[2] { leftoutputHex, rightoutputHex }) // Tweaking Deposition's code from RM
        {
            var i = hex == leftoutputHex ? 0 : 1;
            bool drawshadow = !((Ldispojack && hex == leftoutputHex) || (Rdispojack && hex == rightoutputHex));
            //^We can skip drawing an emerging atom if there's a Dispojack over the iris
            //You can otherwise see an emerging atom's shadow coming out from under it and it's unsightly
            //I will eventually figure out how to skip the atom shadows for the Glyph of Animismus too
            drawPartGraphic(renderer, bonderShadow, textureCenter(bonderShadow), 0f, hexGraphicalOffset(hex), new Vector2(0f, -3f));
            drawPartGraphicSpecular(renderer, animismus_outputUnderIris, textureCenter(animismus_outputUnderIris), 0f, hexGraphicalOffset(hex), Vector2.Zero);
            if (partSimState.field_2743 && !flag && drawshadow)
            {
                drawAtomIO(renderer, partSimState.field_2744[i], hex, num);
            }
            drawPartGraphic(renderer, irisFullArray[index], textureCenter(irisFullArray[index]), -partAngle, hexGraphicalOffset(hex), Vector2.Zero);
            drawPartGraphicSpecular(renderer, animismus_outputAboveIris, textureCenter(animismus_outputAboveIris), 0f, hexGraphicalOffset(hex), Vector2.Zero);
            if (flag && drawshadow)
            {
                drawAtomIO(renderer, partSimState.field_2744[i], hex, num);
            }
        }

        //more actual glyph components
        if (index == irisFullArray.Length - 1) /*irises closed or almost-closed*/
        {
            drawPartGraphic(renderer, indeterminate_symbol, textureCenter(indeterminate_symbol), -partAngle, hexGraphicalOffset(leftoutputHex), new Vector2(-1f, -1f));
        }
        drawPartGraphicSpecular(renderer, animismus_input, textureCenter(animismus_input), 0f, hexGraphicalOffset(leftinputHex), Vector2.Zero);
        drawPartGraphicSpecular(renderer, animismus_input, textureCenter(animismus_input), 0f, hexGraphicalOffset(rightinputHex), Vector2.Zero);
        drawPartGraphic(renderer, animismus_symbol, textureCenter(animismus_symbol), -partAngle, hexGraphicalOffset(leftinputHex), Vector2.Zero);
        drawPartGraphic(renderer, animismus_symbol, textureCenter(animismus_symbol), -partAngle, hexGraphicalOffset(rightinputHex), Vector2.Zero);
        drawPartGraphic(renderer, disproportion_connectors, base_offset, 0f, lefty ? new Vector2(-1f, 0f) : Vector2.Zero, Vector2.Zero); //X-offset issue
        drawPartGloss(renderer, disproportion_gloss, disproportion_glossMask, base_offset + new Vector2(-1f, 0f));
    }

    public static bool mirrorDispro(SolutionEditorScreen ses, Part part, bool mirrorVert, HexIndex pivot)
    {
        //Left-handed Dispro becomes right-handed Dispro and vice versa; thanks FTSIGCTU for proving examples of how to DynamicData
        new DynamicData(part).Set("field_2691", part.method_1159() == Disproportion ? DisproportionR : Disproportion);
        //And then mirror it normally
        FTSIGCTU.MirrorTool.mirrorOrigin(part, mirrorVert, pivot);
        FTSIGCTU.MirrorTool.mirrorRotation(part, !mirrorVert); /*For some reason I have to negate mirrorVert or the flipped part comes out 180 degrees rotated; your guess is as good as mine*/
        return true;
    }

    public static void LoadMirrorRules()
    {
        FTSIGCTU.MirrorTool.addRule(DispoJack, FTSIGCTU.MirrorTool.mirrorSingleton);
        //FTSICTU hasn't heard of chirality-flipping parts, so we roll our own mirror rule
        FTSIGCTU.MirrorTool.addRule(Disproportion, mirrorDispro);
        FTSIGCTU.MirrorTool.addRule(DisproportionR, mirrorDispro);
        FTSIGCTU.MirrorTool.addRule(Infusion, FTSIGCTU.MirrorTool.mirrorSimplePart);
        FTSIGCTU.MirrorTool.addRule(LeftHand, FTSIGCTU.MirrorTool.mirrorHorizontalPart0_0);
        FTSIGCTU.MirrorTool.addRule(Wheel.Herriman, FTSIGCTU.MirrorTool.mirrorVanBerlo);
    }

    public static ILHook dispodrawhook, dontdrawhook;

    public static void DispoDrawHook()
    {
        dispodrawhook = new ILHook(
            typeof(SolutionEditorBase).GetMethod("method_1984", BindingFlags.Public | BindingFlags.Instance),
            DispoDraw);
    }

    public static void DontDrawHook()
    {
        dontdrawhook = new ILHook(
            typeof(SolutionEditorBase).GetMethod("orig_method_1996", BindingFlags.NonPublic | BindingFlags.Instance),
            DontDrawAtomIOUnderDispo);
    }

    public static void DontDrawAtomIOUnderDispo(ILContext il)
    {
        //There are so many things I have to dig around in for the glyph of animismus to make the Dispojack work on it.
        //This one is for drawing emerging atoms.
        //However, this code doesn't work right, and I'm not actually sure why
        //Ergo it's quarantined and never being called, but I leave it here as an example
        //This is one of the big things to work on for an update to True Animismus
        var gremlin = new ILCursor(il);
        gremlin.Goto(1800); //somewhere shortly before the right place in the code

        //Exact spot
        //If you're reading this code and trying to follow along, I'm sorry.
        if (gremlin.TryGotoNext(MoveType.Before,
        x => x.MatchLdloc(105),
        x => x.MatchBrfalse(out _),
        x => x.MatchLdsfld(out _),
        x => x.MatchBr(out _),
        x => x.MatchLdsfld(out _),
        x => x.MatchCall(out _),
        x => x.MatchLdloc(0),
        x => x.MatchLdloc(106)
            ))
            //Get rid of the code so we can reimplement it wrapped in an if-statement
            gremlin.RemoveRange(21);

        //So many local variables in the code that I have to fetch
        //SEB
        gremlin.Emit(OpCodes.Ldarg_0);
        //param_5584
        gremlin.Emit(OpCodes.Ldloc_S, (byte)96);
        //num17
        gremlin.Emit(OpCodes.Ldloc_S, (byte)105);
        //hexindex6
        gremlin.Emit(OpCodes.Ldloc_S, (byte)106);
        //class_
        gremlin.Emit(OpCodes.Ldloc_0);
        //Part
        gremlin.Emit(OpCodes.Ldarg_1);

        //Use them to do this
        //Logger.Log("gremlin.EmitDelegate<Action<SolutionEditorBase,float,int,HexIndex,class_236,Part>>((SEB,param_5584,num17,hexIndex6,class_,part) => ");
        gremlin.EmitDelegate<Action<SolutionEditorBase, float, int, HexIndex, class_236, Part>>((SEB, param_5584, num17, hexIndex6, class_, part) =>
            {
                foreach (var dispojack in SEB.method_502().field_3919.Where(x => x.method_1159() == DispoJack))
                {
                    Logger.Log("h6: " + hexIndex6);
                    Logger.Log("dj: " + dispojack.method_1161());
                    Logger.Log("hi+: " + (hexIndex6.Rotated(part.method_1163()) + part.method_1161()));
                    Logger.Log("Equal?: " + (dispojack.method_1161() == (hexIndex6.Rotated(part.method_1163()) + part.method_1161())));
                    Logger.Log("---------");
                    if (dispojack.method_1161() == (hexIndex6.Rotated(part.method_1163()) + part.method_1161())) { return; } /*if dispojack is where you're drawing the emerging atom, don't draw it!*/
                }
                //otherwise yeah resume doing the thing
                Molecule param_5585 = Molecule.method_1121((num17 == 0) ? class_175.field_1687 : class_175.field_1688);
                Vector2 param_5586 = class_.field_1984 + class_187.field_1742.method_492(hexIndex6).Rotated(class_.field_1985); //method_1999 would go here, but it's private so I just write out what it does the long way
                Editor.method_925(param_5585, param_5586, new HexIndex(0, 0), 0f, 1f, param_5584, 1f, false, null);

                //Future me is going to hate this part of the code; I'm layers deep in the sauce
            });

        //BUT WAIT THERE'S MORE
        //Atom IO drawings are handled in two different spots depending on sim time so we do this again

        if (gremlin.TryGotoNext(MoveType.Before,
        x => x.MatchLdloc(108),
        x => x.MatchBrfalse(out _),
        x => x.MatchLdcI4(1),
        x => x.MatchLdcI4(-1),
        x => x.MatchNewobj<HexIndex>()
            ))
            Logger.Log("Second shadow-drawing injection: " + gremlin.Index);
        gremlin.RemoveRange(31);
        //Why do you even HAVE all of these
        //SEB
        gremlin.Emit(OpCodes.Ldarg_0);
        //param_5584
        gremlin.Emit(OpCodes.Ldloc_S, (byte)96);
        //num18
        gremlin.Emit(OpCodes.Ldloc_S, (byte)108);
        //class_
        gremlin.Emit(OpCodes.Ldloc_0);
        //Part
        gremlin.Emit(OpCodes.Ldarg_1);

        Logger.Log("gremlin.EmitDelegate<Action<SolutionEditorBase,float,int,class_236,Part>>((SEB,param_5584,num17,class_,part) => ");
        gremlin.EmitDelegate<Action<SolutionEditorBase, float, int, class_236, Part>>((SEB, param_5584, num18, class_, part) =>
            {
                HexIndex param_5587 = (num18 == 0) ? new HexIndex(0, 1) : new HexIndex(1, -1);
                foreach (var dispojack in SEB.method_502().field_3919.Where(x => x.method_1159() == DispoJack))
                {
                    Logger.Log("p5587: " + param_5587);
                    Logger.Log("dj: " + dispojack.method_1161());
                    Logger.Log("hi+: " + (param_5587.Rotated(part.method_1163()) + part.method_1161()));
                    Logger.Log("Equal?: " + (dispojack.method_1161() == (param_5587.Rotated(part.method_1163()) + part.method_1161())));
                    if (dispojack.method_1161() == (param_5587.Rotated(part.method_1163()) + part.method_1161())) { return; }  /*if dispojack is where you're drawing the emerging atom, don't draw it!*/
                }
                Molecule param_5588 = Molecule.method_1121((num18 == 0) ? class_175.field_1687 : class_175.field_1688);
                Vector2 param_5589 = class_.field_1984 + class_187.field_1742.method_492(param_5587).Rotated(class_.field_1985);
                Editor.method_925(param_5588, param_5589, new HexIndex(0, 0), 0f, 1f, param_5584, 1f, false, null);

                //Future me is going to hate this part of the code; I'm layers deep in the sauce
            });


    }

    private static void DispoDraw(ILContext il)
    {
        // The Disposal Jack has to be drawn on top of every other glyph.
        // Normally the game draws each glyph in order, so I can't use QApi in the same way as with the rest of the custom glyphs
        // So instead, I'm going into method_1984, the one responsible for drawing everything on the board
        // And inserting 'draw the disposal jack' code right after the 'draw all the glyphs' code

        var gremlin = new ILCursor(il);
        gremlin.Goto(350); //somewhere shortly before the right place in the code


        //Go to the right spot in the code; this is what the opcodes look like just before it
        if (gremlin.TryGotoNext(MoveType.Before,
        x => x.MatchLdarg(0),
        x => x.MatchCallvirt(out _),
        x => x.MatchLdarg(0),
        x => x.MatchCallvirt(out _),
        x => x.MatchCallvirt(out _),
        x => x.MatchLdsfld(out _),
        x => x.MatchDup(),
        x => x.MatchBrtrue(out _),
        x => x.MatchPop(),
        x => x.MatchLdsfld(out _)
            ))
            //Gonna need the list of glyphs
            gremlin.Emit(OpCodes.Ldloc_3);
        //And SolutionEditorBase
        gremlin.Emit(OpCodes.Ldarg_0);
        //And also that first argument for method_1984--Vector2 param_5533
        //I don't know what it does, but later methods want it 
        gremlin.Emit(OpCodes.Ldarg_1);

        //Use them to do this
        //Logger.Log("gremlin.EmitDelegate<Action<Part[], SolutionEditorBase, Vector2>>((glyphlist, SEB, param_5533) => ");
        gremlin.EmitDelegate<Action<Part[], SolutionEditorBase, Vector2>>((glyphlist, SEB, param_5533) =>
            {
                foreach (var dispojack in glyphlist.Where(x => x.method_1159() == DispoJack))
                {
                    //Roll our own rendering helper, the ones used in the usual QApi syntax
                    class_236 class_292 = SEB.method_1989(dispojack, param_5533);
                    class_195 renderer = new class_195(class_292.field_1984, class_292.field_1985, Editor.method_922());
                    DispoDrawInner(dispojack, SEB, renderer, false);
                }
            });
    }

    private static void DispoDrawInner(Part part, SolutionEditorBase editor, class_195 renderer, bool dragging)
    {
        // Functionality of the DispoJack is coded in the glyphs of disproportion, left hand, and animismus, but DispoJack handles its own drawing
        PartSimState partSimState = editor.method_507().method_481(part);
        var simTime = editor.method_504();
        var originHex = new HexIndex(0, 0);

        float partAngle = renderer.field_1798;
        Vector2 base_offset = textureCenter(dispojack_base);
        int lettersFlashIndex = lettersFullArray.Length - 1;

        // Draw DispoJack base only if it's not currently being a cap.
        // Possible DispoJack placements: any iris of an animismus-produciton glyph
        bool nobase = false;
        foreach (Part cappablepart in editor.method_502().field_3919.Where(x =>
            x.method_1159() == PartTypes.field_1780/*Glyph of Animismus*/ ||
            x.method_1159() == Disproportion ||
            x.method_1159() == DisproportionR ||
            x.method_1159() == LeftHand))
        {
            if (AtopIris(cappablepart, part, editor.method_502())) { nobase = true; }
            /*I am sorry about having to pass editor.method_502(),
			I do not like having to say the magic words before it opens sesame either,
			I wish Solution and SolutionEditorBase etc were global so I could just
			use them
			whenever I wanted
			instead of having to do this
			for every function that does anything interesting
			but I know there's a good reason I can't do that
			e.g. There is a whole second instance of SolutionEditorBase when a tip is open*/
        }

        if (!nobase && !dragging)
        {
            drawPartGraphic(renderer, dispojack_base, base_offset, -partAngle, Vector2.Zero, Vector2.Zero);
            if (DispoJack.field_1549 != singleglow) { new DynamicData(DispoJack /*The entire part type*/).Set("field_1549"/*glow (shadow)*/, singleglow); }
            if (DispoJack.field_1550 != singlestroke) { new DynamicData(DispoJack /*The entire part type*/).Set("field_1550"/*stroke (outline)*/, singlestroke); }
            //If you hack in an additional Disposal Jack, all of their glows and strokes and bases and stuff might sync up,
            //Because I'm changing the data for the entire part type
            //There be dragons
        }
        else
        {
            if (DispoJack.field_1549 != circleglow) { new DynamicData(DispoJack /*The entire part type*/).Set("field_1549"/*stroke (outline)*/, circleglow); }
            if (DispoJack.field_1550 != circlestroke) { new DynamicData(DispoJack /*The entire part type*/).Set("field_1550"/*stroke (outline)*/, circlestroke); }
            //Why the if-statements, if the variable has to end up as circlewhatever anyway? Because I don't wanna write a whole texture to the variable every frame
        }

        drawPartGraphicSpecular(renderer, dispojack_face, textureCenter(dispojack_face), -partAngle, Vector2.Zero, Vector2.Zero);
        drawPartGraphic(renderer, dispojack_details, textureCenter(dispojack_details), -partAngle, Vector2.Zero, Vector2.Zero);
        // No gloss

        //Letter Flash
        if (GlyphBelowIsFiring(part, editor))
        {
            lettersFlashIndex = class_162.method_404((int)(class_162.method_411(1f, -1f, simTime) * lettersFullArray.Length), 0, lettersFullArray.Length - 1);
            drawPartGraphic(renderer, lettersFullArray[lettersFlashIndex], textureCenter(lettersFullArray[lettersFlashIndex]), -partAngle, hexGraphicalOffset(originHex), Vector2.Zero);
        }
    }
    public static void dispojackToStartOfList(On.SolutionEditorBase.orig_method_1984 orig, SolutionEditorBase SEB, Vector2 param_5533, Bounds2 param_5534, Bounds2 param_5535, bool param_5536, Maybe<List<Molecule>> param_5537, bool param_5538)
    {
        //Make the disposal jack always able to be picked up by clamping it to the start of the part list
        //THIS IS NOT HOW I'M SUPPOSED TO DO IT
        //Other code that messes with the part list may end up allergic to this

        List<Part> partList = SEB.method_502().field_3919;

        int dispojackIndex = partList.FindIndex(x => x.method_1159() == DispoJack);
        if (dispojackIndex >= 0) //neither null (-1) nor already at start of list (0)
        {
            Part dispojackElement = partList[dispojackIndex];
            partList.RemoveAt(dispojackIndex);
            partList.Insert(0, dispojackElement);
        }
        orig(SEB, param_5533, param_5534, param_5535, param_5536, param_5537, param_5538);
    }

    public static void DispoDrawDragged(On.PartDraggingInputMode.orig_method_1 orig, PartDraggingInputMode PDIM, SolutionEditorScreen SES)
    {
        //There are two ways that the game renders a glyph
        //When it's on the board, it goes through SolutionEditorBase.method_1984 (we'll call SolutionEditorBase "SEB"), then through SEB.method_1993 and finally SEB.method_1996
        //When it's being dragged around, it goes through PartDraggingInputMode.method_1, then to SEB.method_1993 and finally SEB.method_1996
        //Changing the order of when the Disposal Jack needs to be drawn--last--has to be done in the outer methods, since those are the ones that know that more than one part exists
        //So DispoDrawInner() has to be called here too, otherwise the Disposal Jack does not render while it's being dragged

        //Nice of the princess to invite of over for a picnic, eh, Luigi?
        //I hope she made lotsa spaghetti!
        orig(PDIM, SES);

        Type PDIMtype = typeof(PartDraggingInputMode);
        FieldInfo reflected_field_2711 = PDIMtype.GetField("field_2711", BindingFlags.NonPublic | BindingFlags.Instance);
        Vector2 vector = class_115.method_202() - (Vector2)reflected_field_2711.GetValue(PDIM);
        FieldInfo reflected_field_2715 = PDIMtype.GetField("field_2715", BindingFlags.NonPublic | BindingFlags.Instance);
        SES.field_4019 = class_187.field_1742.method_491((HexIndex)reflected_field_2715.GetValue(PDIM), vector);

        var current_interface = SES.field_4010;
        var interfaceDyn = new DynamicData(current_interface);
        var draggedParts = interfaceDyn.Get<List<PartDraggingInputMode.DraggedPart>>("field_2712");
        foreach (PartDraggingInputMode.DraggedPart draggedpart in draggedParts)
        {   //All the parts being dragged
            if (draggedpart.field_2722.method_1159() != DispoJack) { continue; } //Just the disposal jacks

            Part dispojack = draggedpart.field_2722;
            class_236 class_292 = SES.method_1989(dispojack, vector);
            class_195 renderer = new class_195(class_292.field_1984, class_292.field_1985, Editor.method_922());
            DispoDrawInner(dispojack, SES, renderer, true /*no base*/);
            //SolutionEditorScreen inherits from SolutionEditorBase, so you can apparently juse use a SES anywhere you would use a SEB
            //You can tell I'm not formally educated in C# because that feels like it would lead to SO MUCH CONFUSION
        }
    }
}
