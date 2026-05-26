//using MonoMod.RuntimeDetour;
//using MonoMod.Utils;
using Quintessential;
//using Quintessential.Settings;
//using SDL2;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TrueAnimismus;

using AtomTypes = class_175;

public static class API
{
    //This is called an API, but it's not really; it's a bunch of helper methods

    //I didn't bother to make it right while I was learning how to mod

    public const string DisproportionPermission = "TrueAnimismus:disproportion";
    public const string DispoJackPermission = "TrueAnimismus:dispojack";
    public const string LeftHandPermission = "TrueAnimismus:lefthand";
    public const string InfusionPermission = "TrueAnimismus:infusion";
    public const string HerrimanPermission = "TrueAnimismus:herriman";

    public static MethodInfo PrivateMethod<T>(string method) => typeof(T).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

    #region atomtype getters
    public static AtomType vitaeAtomType => AtomTypes.field_1687; // vitae
    public static AtomType morsAtomType => AtomTypes.field_1688; // mors
    public static AtomType saltAtomType => AtomTypes.field_1675; // salt
    public static AtomType quintessenceAtomType => AtomTypes.field_1690;
    //Definitions for higher grades of animismus are in TrueAnimismusCore.cs
    #endregion

    #region ruleDictionaries and methods

    //Disproportion Dictionary
    private static Dictionary<AtomType, Pair<AtomType, AtomType>> disproportionDict = new();

    //Left Hand Dictionary
    private static Dictionary<AtomType, AtomType> lefthandDict = new();

    //Struct of atom for the rating system.
    public struct RegisteredAtom
    {
        public RegisteredAtom(AtomType atomtype, int rating, string tag)
        {
            // The AtomType of the rating atom.
            this.atomtype = atomtype;
            // The Charge of the rating atom.
            this.rating = rating;
            // A tag, to differentiate shared ratings. (i.e. Animismus -3 to 3 is a separate scale from a different set of animismus-adjacent atoms that also scale from -3 to 3.)
            this.tag = tag;
        }
        public AtomType atomtype;
        public int rating;
        public string tag;
    }
    public static List<RegisteredAtom> AtomsForRating = new();


    public static int? AnimeRating(AtomType atomToBeRated, out string tag)
    {
        foreach (API.RegisteredAtom check in API.AtomsForRating)
        {
            if ((check.atomtype == atomToBeRated))
            {
                tag = check.tag;
                return check.rating;
            }
        }
        // Invalid results are a normal case now, as the rating check can happen on any atom.
        tag = null;
        return 0;
    }

    public static AtomType RatingToAtom(int an, string tag)
    {
        //Check every registered rating for a matching rating and tag
        foreach (API.RegisteredAtom check in API.AtomsForRating)
        {
            if ((check.rating == an) && (check.tag == tag))
            {
                return check.atomtype;
            }
        }
        // Invalid results are a normal case now, as the rating check can happen on any atom.
        return null;
    }

    public static bool applyLeftHandRule(AtomType input, out AtomType output) => applyTRule(input, lefthandDict, out output);
    public static bool applyDisproportionRule(AtomType input, out AtomType outputHi, out AtomType outputLo)
    {
        Pair<AtomType, AtomType> output;
        bool ret = applyTRule(input, disproportionDict, out output);
        outputHi = ret ? output.Left : default(AtomType);
        outputLo = ret ? output.Right : default(AtomType);
        return ret;
    }
    public static void addDisproportionRule(AtomType input, AtomType outputHi, AtomType outputLo) => addTRule("disproportion", input, new Pair<AtomType, AtomType>(outputHi, outputLo), disproportionDict, new List<AtomType> { saltAtomType }); //Can't disproportionate salt
    public static void addLeftHandRule(AtomType input, AtomType output) => addTRule("lefthand", input, output, lefthandDict, new List<AtomType> { }); //No explicitly banned atoms for Left Hand yet

    private static bool applyTRule<T>(AtomType hi, Dictionary<AtomType, T> dict, out T lo)
    {
        bool ret = dict.ContainsKey(hi);
        lo = ret ? dict[hi] : default(T);
        return ret;
    }
    private static string ToString(AtomType A) => A.field_2284;

    private static string ruleToString<T>(AtomType hi, T lo) //This function just helps with debugging, I think
    {
        if (typeof(T) == typeof(AtomType))
        {
            return ToString(hi) + " => " + ToString((AtomType)(object)lo);
        }
        else if (typeof(T) == typeof(Pair<AtomType, AtomType>))
        {
            return ToString(hi) + " => ( " + ToString(((Pair<AtomType, AtomType>)(object)lo).Left) + ", " + ToString(((Pair<AtomType, AtomType>)(object)lo).Right) + " )";
        }
        return "";
    }
    private static bool TEquality<T>(T A, T B)
    {
        if (typeof(T) == typeof(AtomType))
        {
            return (AtomType)(object)A == (AtomType)(object)B;
        }
        else if (typeof(T) == typeof(Pair<AtomType, AtomType>))
        {
            return (Pair<AtomType, AtomType>)(object)A == (Pair<AtomType, AtomType>)(object)B;
        }
        return false;
    }
    private static void addTRule<T>(string Tname, AtomType input, T output, Dictionary<AtomType, T> dict, List<AtomType> forbiddenInputs)
    {
        string TNAME = Tname.First().ToString().ToUpper() + Tname.Substring(1);
        //check if rule is forbidden
        if (forbiddenInputs.Contains(input))
        {
            Logger.Log("[TrueAnimismus] ERROR: A " + Tname + " rule for " + ToString(input) + " is not permitted.");
            throw new Exception("add" + TNAME + "Rule: Cannot add rule '" + ruleToString(input, output) + "'.");
        }
        //try to add rule
        bool flag = dict.ContainsKey(input);
        if (flag && !TEquality(dict[input], output))
        {
            //throw an error
            string msg = "[TrueAnimismus] ERROR: Preparing debug dump.";
            msg += "\n  Current list of " + TNAME + " Rules:";
            foreach (var kvp in dict) msg += "\n\t" + ruleToString(kvp.Key, kvp.Value);
            msg += "\n\n  AtomType '" + ToString(input) + "' already has a " + Tname + " rule: '" + ruleToString(input, dict[input]) + "'.";
            Logger.Log(msg);
            throw new Exception("add" + TNAME + "Rule: Cannot add rule '" + ruleToString(input, output) + "'.");
        }
        else if (!flag)
        {
            dict.Add(input, output);
        }
    }
    #endregion
}
