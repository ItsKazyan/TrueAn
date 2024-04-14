using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;
using Quintessential.Settings;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace TrueAnimismus;
public class ModdedAtoms{
public static AtomType RedVitae, TrueVitae, GreyMors, TrueMors, Dummy;
  public static void Load(){
		RedVitae = new AtomType()
		{
			/*ID, byte*/field_2283 = 83, //I just like 83
			/*Non-local Name*/field_2284 = class_134.method_254("Red Vitae"),
			/*Atomic Name*/field_2285 = class_134.method_253("Elemental Red Vitae", string.Empty),
			/*Local name*/field_2286 = class_134.method_253("Red Vitae", string.Empty),
			/*Symbol*/field_2287 = class_235.method_615("textures/atoms/redvitae_symbol"),
			/*Shadow*/field_2288 = class_238.field_1989.field_81.field_599,
			/*Default Graphics struct*/field_2290 = new class_106()
			{
					field_994 = class_235.method_615("textures/atoms/redvitae_diffuse"),
					field_995 = class_235.method_615("textures/atoms/redvitae_shade")
			},
			/*is Purifijectable?*/field_2294 = false,//do not use field_2291 for drawing -- checked by purifier (consumption) and projector (promotion)
			/*is Cardinal element?*/field_2293 = false,//use field_2289 for drawing -- also checked by duplicator (copy essence) and calcinator (calcify)
			/*is Projector fuel?*/field_2295 = false,//checked by projector (consumption)
			/*is drawn like Quintessence?*/field_2296 = false, //use field_2292 for drawing
			/*field_2298 is unused, it seems*/
			QuintAtomType = "Red Vitae:redvitae"
		};

		TrueVitae = new AtomType()
		{
			/*ID, byte*/field_2283 = 84,
			/*Non-local Name*/field_2284 = class_134.method_254("True Vitae"),
			/*Atomic Name*/field_2285 = class_134.method_253("Elemental True Vitae", string.Empty),
			/*Local name*/field_2286 = class_134.method_253("True Vitae", string.Empty),
			/*Symbol*/field_2287 = class_235.method_615("textures/atoms/truevitae_symbol"),
			/*Shadow*/field_2288 = class_238.field_1989.field_81.field_599,
			/*Default Graphics struct*/field_2290 = new class_106()
			{
					field_994 = class_235.method_615("textures/atoms/truevitae_diffuse"),
					field_995 = class_235.method_615("textures/atoms/truevitae_shade")
			},
			/*is Purifijectable?*/field_2294 = false,//do not use field_2291 for drawing -- checked by purifier (consumption) and projector (promotion)
			/*is Cardinal element?*/field_2293 = false,//use field_2289 for drawing -- also checked by duplicator (copy essence) and calcinator (calcify)
			/*is Projector fuel?*/field_2295 = false,//checked by projector (consumption)
			/*is drawn like Quintessence?*/field_2296 = false, //use field_2292 for drawing
			/*field_2298 is unused, it seems*/
			QuintAtomType = "True Vitae:truevitae"
		};

		GreyMors = new AtomType()
		{
			/*ID, byte*/field_2283 = 85,
			/*Non-local Name*/field_2284 = class_134.method_254("Grey Mors"),
			/*Atomic Name*/field_2285 = class_134.method_253("Elemental Grey Mors", string.Empty),
			/*Local name*/field_2286 = class_134.method_253("Grey Mors", string.Empty),
			/*Symbol*/field_2287 = class_235.method_615("textures/atoms/greymors_symbol"),
			/*Shadow*/field_2288 = class_238.field_1989.field_81.field_599,
			/*Default Graphics struct*/field_2290 = new class_106()
			{
					field_994 = class_235.method_615("textures/atoms/greymors_diffuse"),
					field_995 = class_235.method_615("textures/atoms/greymors_shade")
			},
			/*is Purifijectable?*/field_2294 = false,//do not use field_2291 for drawing -- checked by purifier (consumption) and projector (promotion)
			/*is Cardinal element?*/field_2293 = false,//use field_2289 for drawing -- also checked by duplicator (copy essence) and calcinator (calcify)
			/*is Projector fuel?*/field_2295 = false,//checked by projector (consumption)
			/*is drawn like Quintessence?*/field_2296 = false, //use field_2292 for drawing
			/*field_2298 is unused, it seems*/
			QuintAtomType = "Grey Mors:greymors"
		};

		TrueMors = new AtomType()
		{
			/*ID, byte*/field_2283 = 86,
			/*Non-local Name*/field_2284 = class_134.method_254("True Mors"),
			/*Atomic Name*/field_2285 = class_134.method_253("Elemental True Mors", string.Empty),
			/*Local name*/field_2286 = class_134.method_253("RTrue Mors", string.Empty),
			/*Symbol*/field_2287 = class_235.method_615("textures/atoms/truemors_symbol"),
			/*Shadow*/field_2288 = class_238.field_1989.field_81.field_599,
			/*Default Graphics struct*/field_2290 = new class_106()
			{
					field_994 = class_235.method_615("textures/atoms/truemors_diffuse"),
					field_995 = class_235.method_615("textures/atoms/truemors_shade")
			},
			/*is Purifijectable?*/field_2294 = false,//do not use field_2291 for drawing -- checked by purifier (consumption) and projector (promotion)
			/*is Cardinal element?*/field_2293 = false,//use field_2289 for drawing -- also checked by duplicator (copy essence) and calcinator (calcify)
			/*is Projector fuel?*/field_2295 = false,//checked by projector (consumption)
			/*is drawn like Quintessence?*/field_2296 = false, //use field_2292 for drawing
			/*field_2298 is unused, it seems*/
			QuintAtomType = "True Mors:truemors"
		};

		Dummy = new AtomType()
		{
			/*ID, byte*/field_2283 = 87,
			/*Non-local Name*/field_2284 = class_134.method_254("Dummy Atom"),
			/*Atomic Name*/field_2285 = class_134.method_253("Elemental Dummy Atom", string.Empty),
			/*Local name*/field_2286 = class_134.method_253("Dummy Atom", string.Empty),
			/*Symbol*/field_2287 = class_235.method_615("textures/atoms/greymors_symbol"),
			/*Shadow*/field_2288 = class_238.field_1989.field_81.field_599,
			/*Default Graphics struct*/field_2290 = new class_106()
			{
					field_994 = class_235.method_615("textures/atoms/redvitae_diffuse"),
					field_995 = class_235.method_615("textures/atoms/redvitae_shade")
			},
			/*is Purifijectable?*/field_2294 = false,//do not use field_2291 for drawing -- checked by purifier (consumption) and projector (promotion)
			/*is Cardinal element?*/field_2293 = false,//use field_2289 for drawing -- also checked by duplicator (copy essence) and calcinator (calcify)
			/*is Projector fuel?*/field_2295 = false,//checked by projector (consumption)
			/*is drawn like Quintessence?*/field_2296 = false, //use field_2292 for drawing
			QuintAtomType = "TA-Dummy:ta-dummy"
		};
    // ... 
  }
}