﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;
using Quintessential.Settings;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Security.Cryptography;

namespace TrueAnimismus;

using PartType = class_139;
using Permissions = enum_149;
using BondType = enum_126;
using BondSite = class_222;
using AtomTypes = class_175;
using PartTypes = class_191;
using Texture = class_256;
public class MainClass : QuintessentialMod
	{
	// resources
	static Texture[] projectAtomAnimation => class_238.field_1989.field_81.field_614;
	static Texture[] donorAnimation, receiverAnimation;

	static Sound animismusActivate => class_238.field_1991.field_1838;
	static Sound projectionActivate => class_238.field_1991.field_1844;
	static Sound duplicationActivate => class_238.field_1991.field_1843;
	static Sound purificationActivate => class_238.field_1991.field_1845;


	// helper functions
	private static bool glyphIsFiring(PartSimState partSimState) => partSimState.field_2743;
	private static void glyphNeedsToFire(PartSimState partSimState) => partSimState.field_2743 = true;
	public static void playSound(Sim sim_self, Sound sound) => API.PrivateMethod<Sim>("method_1856").Invoke(sim_self, new object[] { sound });

	public static void playSoundWithVolume(Sound SOUND, float VOLUME = 1f)
	{
		SOUND.method_28(VOLUME);
	}
	
	//drawing helpers
	public static Vector2 hexGraphicalOffset(HexIndex hex) => class_187.field_1742.method_492(hex);

	public static Texture[] fetchTextureArray(int length, string path)
	{
		var ret = new Texture[length];
		for (int i = 0; i < ret.Length; i++)
		{
			ret[i] = class_235.method_615(path + (i + 1).ToString("0000"));
		}
		return ret;
	}

	public static void LoadAnimations()
	{
		donorAnimation = MainClass.fetchTextureArray(12, "animations/donor_effect.array/donor_");
		receiverAnimation = MainClass.fetchTextureArray(12, "animations/receiver_effect.array/receiver_");
		//Logger.Log(donorAnimation.Length);
		//Logger.Log(receiverAnimation.Length);
	}

	// List<AtomType> atomsToAdd;
	public override void Load() { 
	}
	public override void LoadPuzzleContent()
	{
		//Adding the new atoms
		ModdedAtoms.Load();	
		QApi.AddAtomType(ModdedAtoms.RedVitae);
		QApi.AddAtomType(ModdedAtoms.TrueVitae);
		QApi.AddAtomType(ModdedAtoms.GreyMors);
		QApi.AddAtomType(ModdedAtoms.TrueMors);

		LoadAnimations();

		Glyphs.LoadContent();
		Wheel.LoadContent();

		QApi.AddPuzzlePermission(API.DisproportionPermission, "Glyph of Disproportion", "True Animismus");
		QApi.AddPuzzlePermission(API.DispoJackPermission, "Disposal Jack", "True Animismus");
		QApi.AddPuzzlePermission(API.LeftHandPermission, "Glyph of the Left Hand", "True Animismus");
		QApi.AddPuzzlePermission(API.InfusionPermission, "Glyph of Infusion", "True Animismus");
		QApi.AddPuzzlePermission(API.HerrimanPermission, "Herriman's Wheel", "True Animismus");
		

		//------------------------- HOOKING -------------------------//
		AnimismusFiringHook();
		QApi.RunAfterCycle(My_Method_1832);
		IL.SolutionEditorBase.method_1984 += drawHerrimanWheelAtoms;
		On.SolutionEditorBase.method_1984 += Glyphs.dispojackToEndOfList;
		//Glyphs.DispoDrawHook();
	}

	private static void drawHerrimanWheelAtoms(ILContext il)
	{
		//I don't fully understand mr_puzzle's code here but it works; directly copied from Ravari's wheel like everything else about Herriman's wheel but COPIED EXTRA HARD
		ILCursor cursor = new ILCursor(il);
		// skip ahead to roughly where method_2015 is called
		cursor.Goto(658);

		// jump ahead to just after the method_2015 for-loop
		if (!cursor.TryGotoNext(MoveType.After, instr => instr.Match(OpCodes.Ldarga_S))) return;

		// load the SolutionEditorBase self and the class423 local onto the stack so we can use it
		cursor.Emit(OpCodes.Ldarg_0);
		cursor.Emit(OpCodes.Ldloc_0);
		// then run the new code
		cursor.EmitDelegate<Action<SolutionEditorBase, SolutionEditorBase.class_423>>((seb_self, class423) =>
		{
			if (seb_self.method_503() != enum_128.Stopped)
			{
				var partList = seb_self.method_502().field_3919;
				foreach (var herriman in partList.Where(x => x.method_1159() == Wheel.Herriman))
				{
					Wheel.drawHerrimanAtoms(seb_self, herriman, class423.field_3959);
				}
			}
		});
	}

	private static void My_Method_1832(Sim sim_self, bool isConsumptionHalfstep)
	{
		var SEB = sim_self.field_3818;
		var solution = SEB.method_502();
		var partList = solution.field_3919;
		var partSimStates = sim_self.field_3821;
		var struct122List = sim_self.field_3826;
		var moleculeList = sim_self.field_3823;
		var gripperList = sim_self.HeldGrippers;

		//define some helpers

		Maybe<AtomReference> maybeFindAtom(Part part, HexIndex hex, List<Part> list, bool checkWheels = false)
		{
			return (Maybe<AtomReference>)API.PrivateMethod<Sim>("method_1850").Invoke(sim_self, new object[] { part, hex, list, checkWheels });
		}

		void addColliderAtHex(Part part, HexIndex hex)
		{
			struct122List.Add(new Sim.struct_122()
			{
				field_3850 = (Sim.enum_190)0,
				field_3851 = hexGraphicalOffset(part.method_1184(hex)),
				field_3852 = 15f // Sim.field_3832;
			});
		}

		void spawnAtomAtHex(Part part, HexIndex hex, AtomType atom)
		{
			Molecule molecule = new Molecule();
			molecule.method_1105(new Atom(atom), part.method_1184(hex));
			moleculeList.Add(molecule);
		}

		void consumeAtomReference(AtomReference atomRef)
		{
			// delete the input atom
			atomRef.field_2277.method_1107(atomRef.field_2278);
			// draw input getting consumed
			SEB.field_3937.Add(new class_286(SEB, atomRef.field_2278, atomRef.field_2280));
		}

		// void changeAtomTypeWithFlash(AtomReference atomReference, AtomType newAtomType)
		// {
		// 	// change atom type
		// 	var molecule = atomReference.field_2277;
		// 	molecule.method_1106(newAtomType, atomReference.field_2278);
		// 	// draw projection/flash animation
		// 	atomReference.field_2279.field_2276 = (Maybe<class_168>)new class_168(SEB, (enum_7)0, (enum_132)1, atomReference.field_2280, projectAtomAnimation, 30f);
		// }

		void changeAtomTypeDonorAnimation(AtomReference atomReference, AtomType newAtomType)
		{
			// change atom type
			var molecule = atomReference.field_2277;
			molecule.method_1106(newAtomType, atomReference.field_2278);
			// draw donor animation
			atomReference.field_2279.field_2276 = (Maybe<class_168>)new class_168(SEB, (enum_7)0, (enum_132)1, atomReference.field_2280, donorAnimation, 30f);
			//Logger.Log(donorAnimation.Length);
		}

		void changeAtomTypeReceiverAnimation(AtomReference atomReference, AtomType newAtomType)
		{
			// change atom type
			var molecule = atomReference.field_2277;
			molecule.method_1106(newAtomType, atomReference.field_2278);
			// draw receiver animation
			atomReference.field_2279.field_2276 = (Maybe<class_168>)new class_168(SEB, (enum_7)0, (enum_132)1, atomReference.field_2280, receiverAnimation, 30f);
			//Logger.Log(receiverAnimation.Length);
		}

		AtomType DetermineHerrimanOutputResult(AtomType OutMediator, bool UpOrDown)
		{	// Don't do it like this. real programmers use dicts. I tried to use a switch statement, but the compiler didn't like it.
			if (OutMediator == ModdedAtoms.TrueMors)
				{
					if (!UpOrDown) {Logger.Log("[TrueAnimismus] Tried to add mors to already True Mors in Herriman's Wheel. This is a bug. Report it!");};
					return UpOrDown ? ModdedAtoms.GreyMors : ModdedAtoms.TrueMors;
				};
			if (OutMediator == ModdedAtoms.GreyMors)
				{return UpOrDown ? API.morsAtomType : ModdedAtoms.TrueMors;};
			if (OutMediator == API.morsAtomType)
				{return UpOrDown ? API.saltAtomType : ModdedAtoms.GreyMors;};
			if (OutMediator == API.saltAtomType)
				{return UpOrDown ?  API.vitaeAtomType : API.morsAtomType;};
			if (OutMediator == API.vitaeAtomType)
				{return UpOrDown ? ModdedAtoms.RedVitae : API.saltAtomType;};
			if (OutMediator == ModdedAtoms.RedVitae)
				{return UpOrDown ? ModdedAtoms.TrueVitae : API.vitaeAtomType;};
			if (OutMediator == ModdedAtoms.TrueVitae)
				{
					if (UpOrDown) {Logger.Log("[TrueAnimismus] Tried to add vitae to already True Vitae in Herriman's Wheel. This is a bug. Report it!");};
					return UpOrDown ? ModdedAtoms.TrueVitae : ModdedAtoms.RedVitae;
				};
			Logger.Log("[TrueAnimismus] Couldn't determine how to change the output-mediator in Herriman's Wheel; defaulted to salt. This is a bug. Report it!");
			return API.saltAtomType;
		}
		
		bool MediationMath(ref AtomType HerrimanOut, ref AtomType HerrimanIn, ref AtomType FreeAtom, int sign /*1 for mediating the hi side, -1 for mediating the lo side*/)
		{
			bool withinBounds;
			int mediatedHerrimanOut, mediatedHerrimanIn, freeAtomOut;
			
			int hOutRating = API.AnimeRating(HerrimanOut);
			int hInRating = API.AnimeRating(HerrimanIn);
			int fInRating = API.AnimeRating(FreeAtom);
			int VorM = (fInRating > 0) ? 1 : -1;

			//Logger.Log("hOut:" + hOutRating + ", hIn: " + hInRating  + ", fIn: " + fInRating + ", VorM: " + VorM);

			mediatedHerrimanOut = hOutRating + fInRating + sign*VorM;
			mediatedHerrimanIn = hInRating - fInRating;
			freeAtomOut = fInRating - sign*VorM;
			
			//Logger.Log("mhOut:" + mediatedHerrimanOut + ", mhIn: " + mediatedHerrimanIn  + ", mfOut: " + freeAtomOut);

			withinBounds = 
				mediatedHerrimanOut >= -3 &&
				mediatedHerrimanOut <= 3 &&
				mediatedHerrimanIn >= -3 &&
				mediatedHerrimanIn <= 3 &&
				freeAtomOut >= -3 &&
				freeAtomOut <= 3;

			if (!withinBounds) {return false;}
			else
			{
				HerrimanOut = API.RatingToAtom(mediatedHerrimanOut);
				HerrimanIn = API.RatingToAtom(mediatedHerrimanIn);
				FreeAtom = API.RatingToAtom(freeAtomOut);
				return true;
			}
		}

		bool InfusionMath(ref AtomType Donor, ref AtomType Reciever, bool OppositionPermitted)
		{
			int donorRating = API.AnimeRating(Donor);
			int receiverRating = API.AnimeRating(Reciever);
			//Can't concentrate animismus with this glyph, so I don't check for exceeding true vitae or mors
			if (!OppositionPermitted && (donorRating ^ receiverRating) > 0 || ((receiverRating >= 0 && donorRating >= 0) || (receiverRating < 0 && donorRating < 0) ? Math.Abs(receiverRating) >= Math.Abs(donorRating) : false))
				{/*If atoms are in opposition but opposition isn't allowed, infusion fails
				If donor isn't more concentrated of the same sign of animismus than the reciever, also fails*/
				return false;
				}
			int VorM = (donorRating > 0) ? 1 : -1;
			donorRating -= VorM;
			receiverRating += VorM;
			Donor = API.RatingToAtom(donorRating);
			Reciever = API.RatingToAtom(receiverRating);
			return true;
		}

		// fire the glyphs!
		var GlyphAnimismus = PartTypes.field_1780;
		foreach (Part part in partList)
		{
			PartSimState partSimState = partSimStates[part];
			var partType = part.method_1159();

			if (partType == GlyphAnimismus)
			{
				
				// check if Herriman's Wheel is in place to mediate half of the glyph

				HexIndex hexInputLeft = new HexIndex(0, 0);
				HexIndex hexInputRight = new HexIndex(1, 0);
				HexIndex hexOutputUp = new HexIndex(0, 1);
				HexIndex hexOutputDown = new HexIndex(1, -1);
				AtomReference atomSaltLeft = default(AtomReference);
				AtomReference atomSaltRight = default(AtomReference);
				AtomReference atomSaltToConsume = default(AtomReference);
				AtomReference atomInputHerriman = default(AtomReference);
				AtomReference atomInputHerrimanLeft = default(AtomReference);
				AtomReference atomInputHerrimanRight = default(AtomReference);
				AtomReference atomOutputHerriman = default(AtomReference);
				AtomReference atomOutputHerrimanUp = default(AtomReference);
				AtomReference atomOutputHerrimanDown = default(AtomReference);
				AtomType HerrimanOutputResult = default(AtomType);

				// if(SEB.method_507().method_481(part).field_2744[0] == ModdedAtoms.Dummy)
				// {/*Trying to "fire" the glyph because Herriman's Wheel is mediating over the vitae port (that's what the Dummy means)*/
				// 	spawnAtomAtHex(part, hexOutputDown, API.morsAtomType);
					
				// }
				// else if(SEB.method_507().method_481(part).field_2744[1] == ModdedAtoms.Dummy)
				// {/*Trying to "fire" the glyph because Herriman's Wheel is mediating over the mors port (that's what the Dummy means)*/
				// 	spawnAtomAtHex(part, hexOutputUp, API.vitaeAtomType);
				// }

				bool foundSaltInputLeft =
					maybeFindAtom(part, hexInputLeft, gripperList).method_99(out atomSaltLeft)
					&& atomSaltLeft.field_2280 == API.saltAtomType // salt atom
					&& !atomSaltLeft.field_2281 // a single atom
					&& !atomSaltLeft.field_2282 // not held by a gripper
				;
				bool foundSaltInputRight =
					maybeFindAtom(part, hexInputRight, gripperList).method_99(out atomSaltRight)
					&& atomSaltRight.field_2280 == API.saltAtomType // salt atom
					&& !atomSaltRight.field_2281 // a single atom
					&& !atomSaltRight.field_2282 // not held by a gripper
				;
				atomSaltToConsume = atomSaltLeft ?? atomSaltRight;

				bool foundMediationInputLeft = 
					Wheel.maybeFindHerrimanWheelAtom(sim_self, part, hexInputLeft).method_99(out atomInputHerrimanLeft);
				;
				bool foundMediationInputRight = 
					Wheel.maybeFindHerrimanWheelAtom(sim_self, part, hexInputRight).method_99(out atomInputHerrimanRight);
				;

				//Logger.Log(atomInputHerrimanLeft + " " + atomInputHerrimanLeft);
				atomInputHerriman = atomInputHerrimanLeft ?? atomInputHerrimanRight;

				bool foundMediationOutputUp =
					Wheel.maybeFindHerrimanWheelAtom(sim_self, part, hexOutputUp).method_99(out atomOutputHerrimanUp)
					&& atomOutputHerrimanUp.field_2280 != (ModdedAtoms.TrueVitae) // Can't mediate if more vitae would be added to true vitae
				;
				bool foundMediationOutputDown =
					Wheel.maybeFindHerrimanWheelAtom(sim_self, part, hexOutputDown).method_99(out atomOutputHerrimanDown)
					&& atomOutputHerrimanDown.field_2280 != (ModdedAtoms.TrueMors) // Can't mediate if more mors would be added to true mors
				;
				atomOutputHerriman = atomOutputHerrimanUp ?? atomOutputHerrimanDown;

				bool UpBlocked = maybeFindAtom(part, hexOutputUp, new List<Part>(),true).method_99(out _);
				bool DownBlocked = maybeFindAtom(part, hexOutputDown, new List<Part>(),true).method_99(out _);

				bool MediationPossible = /*big ugly conditional to check for a bunch of stuff; blocked output, salt, wheel in right place, etc*/
					(foundMediationInputLeft && foundSaltInputRight && ((foundMediationOutputUp && !DownBlocked) || (foundMediationOutputDown && !UpBlocked))) ||
					(foundMediationInputRight && foundSaltInputLeft && ((foundMediationOutputUp && !DownBlocked) || (foundMediationOutputDown && !UpBlocked)))
				;

				

				if (MediationPossible)
				{	
					// sounds and animation for firing the glyph
					playSound(sim_self, animismusActivate);
					// partSimState.field_2743 = true;
					// partSimState.field_2744[0] = foundMediationOutputDown ? API.vitaeAtomType : ModdedAtoms.Dummy;
					// partSimState.field_2744[1] = foundMediationOutputUp ? API.morsAtomType : ModdedAtoms.Dummy;
					// /* I don't actually know if I can just call glyphNeedstoFire() on animismus without borking something.
					//    Using the "what atoms do I spawn?" fields as free variable real estate
					//    without having to otherwise hack the glyph of animismus
					//    The dummy is for the blocked iris*/


					//TODO: Open the irises here!
					
					//Vector2 hexPosition = hexGraphicalOffset(part.method_1161() + hexProject.Rotated(part.method_1163()));
					//Texture[] projectionGlyphFlashAnimation = class_238.field_1989.field_90.field_256;
					//SEB.field_3935.Add(new class_228(SEB, (enum_7)1, hexPosition, projectionGlyphFlashAnimation, 30f, Vector2.Zero, part.method_1163().ToRadians()));

					if (foundMediationOutputUp) //Ugly, need to refctor this, especially the 'UpOrDown' part in DetermineHerrimanOutputResult
					{	//+1 vitaeness
					HerrimanOutputResult = API.RatingToAtom(API.AnimeRating(atomOutputHerriman.field_2280) + 1);
					}
					else
					{	//+1 morsosity
					HerrimanOutputResult = API.RatingToAtom(API.AnimeRating(atomOutputHerriman.field_2280) - 1);
					}
					// eat salt
					consumeAtomReference(atomSaltToConsume);
					// herriman input
					if (foundMediationInputLeft)
						{Wheel.DrawHerrimanFlash(SEB, part, hexInputLeft);}
					else
						{Wheel.DrawHerrimanFlash(SEB, part, hexInputRight);}
					 //mediating is happening beep boop
					// herriman output
					changeAtomTypeReceiverAnimation(atomOutputHerriman, HerrimanOutputResult);
					changeAtomTypeDonorAnimation(atomInputHerriman, atomInputHerriman.field_2280); /*No actual change to the atom's identity, just makes it flash nicely*/
					
					glyphNeedsToFire(partSimState); //When animismus gets this signal, it will do the atom-spawning

					bool blockvitae = false;
					bool blockmors = false;
					foreach (Part dispojack in SEB.method_502().field_3919.Where(x => x.method_1159() == Glyphs.DispoJack))
					{
						if (dispojack.method_1161() == hexOutputUp.Rotated(part.method_1163()) + part.method_1161())
						{blockvitae = true;}
						if (dispojack.method_1161() == hexOutputDown.Rotated(part.method_1163()) + part.method_1161())
						{blockmors = true;}
					}
					//Spawn the half-sized colliders that atoms have when emerging from outputs, skipping if there's a disposal jack involved
					if(foundMediationOutputDown && !blockvitae) {addColliderAtHex(part,hexOutputUp);}
					if(foundMediationOutputUp && !blockmors) {addColliderAtHex(part,hexOutputDown);}


					//Glyphs.drawAtomIO(/*PartRenderer, somewhere???*/, partSimState.field_2744[0], foundMediationOutputDown ? hexOutputUp : hexOutputDown, SEB.method_504());
						/* ^^^ I SURE HOPE I DON'T HAVE TO TOUCH THAT LINE AGAIN ^^^ */
				}			
			}

			if (partType == Glyphs.LeftHand)
			{
				HexIndex hexInput = new HexIndex(-1, 0);
				HexIndex hexMarker = new HexIndex(0, 0);
				HexIndex hexRight = new HexIndex(1, 0); //output

				AtomReference atomToInvert;
				AtomType atomInverse;
				
				bool hasdispojack = false;
				foreach (Part dispojack in SEB.method_502().field_3919.Where(x => x.method_1159() == Glyphs.DispoJack))
				{	//Did you put a dispojack on me, istg
					if (dispojack.method_1161() == hexRight.Rotated(part.method_1163()) + part.method_1161())
					{hasdispojack = true;}
				}

				if (glyphIsFiring(partSimState))
				{
					if(!hasdispojack){spawnAtomAtHex(part, hexRight, partSimState.field_2744[0]);} //output, or blunder your atom if there's a dispojack there
				}
				else if (isConsumptionHalfstep
					&& !maybeFindAtom(part, hexRight, new List<Part>(),true).method_99(out _) // output not blocked; extra "true" means that wheels can block outputs
					&& maybeFindAtom(part, hexInput, gripperList).method_99(out atomToInvert) // invertible atom exists
					&& !atomToInvert.field_2281 // a single atom
					&& !atomToInvert.field_2282 // not held by a gripper
					&& API.applyLeftHandRule(atomToInvert.field_2280, out atomInverse) // is invertible; this line finds what the inverse of the input is.
				)
				{
					glyphNeedsToFire(partSimState);
					playSound(sim_self, Glyphs.lefthandSound);
					consumeAtomReference(atomToInvert);
					// take care of output
					partSimState.field_2744 = new AtomType[1] {atomInverse};
					if(!hasdispojack){addColliderAtHex(part, hexRight);}
				}
			}

			if (partType == Glyphs.Disproportion) 
			{
				HexIndex originHex = new HexIndex(0, 0);
				HexIndex hexInputLeft = new HexIndex(0, -1);
				HexIndex hexInputRight = new HexIndex(1, -1);
				HexIndex hexOutputHi = new HexIndex(-1, 0); // Higher grade output
				HexIndex hexOutputLo = new HexIndex(1, 0); // Lower grade output

				AtomReference atomLeft, atomRight, atomHi, atomLo;
				AtomType outputAtomHi, outputAtomLo;

				bool hidispojack = false;
				bool lodispojack = false;
				foreach (Part dispojack in SEB.method_502().field_3919.Where(x => x.method_1159() == Glyphs.DispoJack))
				{	//Did you put a dispojack on me, istg
					if (dispojack.method_1161() == hexOutputHi.Rotated(part.method_1163()) + part.method_1161())
					{hidispojack = true;}
					if (dispojack.method_1161() == hexOutputLo.Rotated(part.method_1163()) + part.method_1161())
					{lodispojack = true;}
				}

				if (glyphIsFiring(partSimState))
				{
					//Herrmian's wheel mediation will pass a dummy atom into partSimState.field_2744 just to avoid breaking stuff; output everything except dummy atoms.
					if (partSimState.field_2744[0] != ModdedAtoms.Dummy){spawnAtomAtHex(part, hexOutputHi, partSimState.field_2744[0]);};
					if (partSimState.field_2744[1] != ModdedAtoms.Dummy){spawnAtomAtHex(part, hexOutputLo, partSimState.field_2744[1]);};
				}
				else
				{
					if (isConsumptionHalfstep
						&& !maybeFindAtom(part, hexOutputHi, new List<Part>(),true).method_99(out _) // high output not blocked; extra true means that wheels can block outputs
						&& !maybeFindAtom(part, hexOutputLo, new List<Part>(),true).method_99(out _) // low output not blocked; extra true means that wheels can block outputs
						&& maybeFindAtom(part, hexInputLeft, gripperList).method_99(out atomLeft) // left input exists
						&& maybeFindAtom(part, hexInputRight, gripperList).method_99(out atomRight) // right input exists
						&& atomLeft.field_2280 == atomRight.field_2280 // identical input atoms
						&& !atomLeft.field_2281 // a single atom
						&& !atomLeft.field_2282 // not held by a gripper
						&& !atomRight.field_2281 // a single atom
						&& !atomRight.field_2282 // not held by a gripper
						&& API.applyDisproportionRule(atomLeft.field_2280, out outputAtomHi, out outputAtomLo) // apply disproportion rule
					)
					{
						glyphNeedsToFire(partSimState);
						playSound(sim_self, Glyphs.disproportionSound);
						consumeAtomReference(atomLeft);
						consumeAtomReference(atomRight);
						// take care of outputs
						outputAtomLo = !lodispojack ? outputAtomLo : ModdedAtoms.Dummy;
						outputAtomHi = !hidispojack ? outputAtomHi : ModdedAtoms.Dummy;
						partSimState.field_2744 = new AtomType[2] { outputAtomHi, outputAtomLo };
						if (outputAtomLo != ModdedAtoms.Dummy) {addColliderAtHex(part, hexOutputLo);};
						if (outputAtomHi != ModdedAtoms.Dummy) {addColliderAtHex(part, hexOutputHi);};





					} else if (//Now check for left mediation. hexOutputHi is on the left for this chirality.
							isConsumptionHalfstep
							&& !maybeFindAtom(part, hexOutputLo, new List<Part>(),true).method_99(out _) // low output not blocked; extra true means that wheels can block outputs
							// If you use glitches to make duplicate wheels, two Herriman wheels on opposite sides of the same glyph won't work. Known issue, not going to fix it. 
							&& maybeFindAtom(part, hexInputRight, gripperList).method_99(out atomRight) // right input exists
							&& !atomRight.field_2281 // a single atom
							&& !atomRight.field_2282 // not held by a gripper
							&& Wheel.maybeFindHerrimanWheelAtom(sim_self, part, hexInputLeft).method_99(out atomLeft)
							&& Wheel.maybeFindHerrimanWheelAtom(sim_self, part, hexOutputHi).method_99(out atomHi)
							&& API.applyDisproportionRule(atomRight.field_2280, out outputAtomHi, out outputAtomLo)
					)
					{
						outputAtomHi = atomHi.field_2280;
						AtomType inputAtomLeft  = atomLeft.field_2280;
						outputAtomLo = atomRight.field_2280; 
						if (MediationMath(ref outputAtomHi,ref inputAtomLeft,ref outputAtomLo,1))
							/* outputAtomLo is only atomright's type briefly and becomes the free output atom's type
								also only faux-fire the glyph if the mediation math works out.
								This code sucks */ 
							{
							glyphNeedsToFire(partSimState);
							playSound(sim_self, Glyphs.disproportionSound);
							consumeAtomReference(atomRight);
							// take care of outputs
							//Disposal Jack
							outputAtomLo = !lodispojack ? outputAtomLo : ModdedAtoms.Dummy;
							partSimState.field_2744 = new AtomType[2] {ModdedAtoms.Dummy, outputAtomLo}; 
							if (outputAtomLo != ModdedAtoms.Dummy) {addColliderAtHex(part, hexOutputLo);};
							// Change herriman atoms
							//Herriman mediates left atoms:
							Wheel.DrawHerrimanFlash(SEB, part, hexInputLeft);
							Wheel.DrawHerrimanFlash(SEB, part, hexOutputHi);
							changeAtomTypeDonorAnimation(atomLeft, inputAtomLeft); /* inputAtomLeft was changed by MediationMath */
							changeAtomTypeReceiverAnimation(atomHi, outputAtomHi); /*same deal*/
							}
					}
					else if (//Now check for right mediation. hexOutputHi is on the left for this chirality.
							isConsumptionHalfstep // top output not blocked; extra true means that wheels can block outputs
							&& !maybeFindAtom(part, hexOutputHi, new List<Part>(),true).method_99(out _) // low output not blocked; extra true means that wheels can block outputs
								// If you use glitches to make duplicate wheels, two Herriman wheels on opposite sides of the same glyph won't work. Known issue, not going to fix it. 
							&& maybeFindAtom(part, hexInputLeft, gripperList).method_99(out atomLeft) // right input exists
							&& !atomLeft.field_2281 // a single atom
							&& !atomLeft.field_2282 // not held by a gripper
							&& Wheel.maybeFindHerrimanWheelAtom(sim_self, part, hexInputRight).method_99(out atomRight)
							&& Wheel.maybeFindHerrimanWheelAtom(sim_self, part, hexOutputLo).method_99(out atomLo)
							&& API.applyDisproportionRule(atomLeft.field_2280, out outputAtomHi, out outputAtomLo)
					)
					{
						outputAtomLo = atomLo.field_2280;
						AtomType inputAtomRight  = atomRight.field_2280;
						outputAtomHi = atomLeft.field_2280;
						if( MediationMath(ref outputAtomLo,ref inputAtomRight,ref outputAtomHi,-1)) 
						{
							/* outputAtomHi is only atomright's type briefly and becomes the free output atom's type
								also only faux-fire the glyph if the mediation math works out.
								This code sucks */  
						glyphNeedsToFire(partSimState);
						playSound(sim_self, Glyphs.disproportionSound);
						consumeAtomReference(atomLeft);
						// take care of outputs
						outputAtomHi = !hidispojack ? outputAtomHi : ModdedAtoms.Dummy;
						partSimState.field_2744 = new AtomType[2] {outputAtomHi, ModdedAtoms.Dummy};
						if (outputAtomHi != ModdedAtoms.Dummy) {addColliderAtHex(part, hexOutputHi);}
						// Change herriman atoms
						//Herriman mediates Right atoms:
						Wheel.DrawHerrimanFlash(SEB, part, hexInputRight);
						Wheel.DrawHerrimanFlash(SEB, part, hexOutputLo);
						changeAtomTypeDonorAnimation(atomRight, inputAtomRight); /* inputAtomRight was changed by MediationMath */
						changeAtomTypeReceiverAnimation(atomLo, outputAtomLo); /*same deal*/
						}
					}
				}
			
			}

			if (partType == Glyphs.DisproportionR) // Nearly identical to nonflipped version
			{
				HexIndex originHex = new HexIndex(0, 0);
				HexIndex hexInputLeft = new HexIndex(0, -1);
				HexIndex hexInputRight = new HexIndex(1, -1);
				HexIndex hexOutputHi = new HexIndex(1, 0); // Higher grade output
				HexIndex hexOutputLo = new HexIndex(-1, 0); // Lower grade output

				AtomReference atomLeft, atomRight, atomHi, atomLo;
				AtomType outputAtomHi, outputAtomLo;

				bool hidispojack = false;
				bool lodispojack = false;
				foreach (Part dispojack in SEB.method_502().field_3919.Where(x => x.method_1159() == Glyphs.DispoJack))
				{	//Did you put a dispojack on me, istg
					if (dispojack.method_1161() == hexOutputHi.Rotated(part.method_1163()) + part.method_1161())
					{hidispojack = true;}
					if (dispojack.method_1161() == hexOutputLo.Rotated(part.method_1163()) + part.method_1161())
					{lodispojack = true;}
				}

				if (glyphIsFiring(partSimState))
				{
					if (partSimState.field_2744[0] != ModdedAtoms.Dummy){spawnAtomAtHex(part, hexOutputHi, partSimState.field_2744[0]);};
					if (partSimState.field_2744[1] != ModdedAtoms.Dummy){spawnAtomAtHex(part, hexOutputLo, partSimState.field_2744[1]);};
				}
				else if (isConsumptionHalfstep
					&& !maybeFindAtom(part, hexOutputHi, new List<Part>(),true).method_99(out _) // top output not blocked; extra true means that wheels can block outputs
					&& !maybeFindAtom(part, hexOutputLo, new List<Part>(),true).method_99(out _) // bottom output not blocked; extra true means that wheels can block outputs
					//Not checking this anymore due to integrated disposal feature
					&& maybeFindAtom(part, hexInputLeft, gripperList).method_99(out atomLeft) // left input exists
					&& maybeFindAtom(part, hexInputRight, gripperList).method_99(out atomRight) // right input exists
					&& atomLeft.field_2280 == atomRight.field_2280 // identical input atoms
					&& !atomLeft.field_2281 // a single atom
					&& !atomLeft.field_2282 // not held by a gripper
					&& !atomRight.field_2281 // a single atom
					&& !atomRight.field_2282 // not held by a gripper
					&& API.applyDisproportionRule(atomLeft.field_2280, out outputAtomHi, out outputAtomLo) // apply disproportion rule
				)
				{
					glyphNeedsToFire(partSimState);
					playSound(sim_self, Glyphs.disproportionSound);
					consumeAtomReference(atomLeft);
					consumeAtomReference(atomRight);
					// take care of outputs
					outputAtomLo = !lodispojack ? outputAtomLo : ModdedAtoms.Dummy;
					outputAtomHi = !hidispojack ? outputAtomHi : ModdedAtoms.Dummy;
					partSimState.field_2744 = new AtomType[2] { outputAtomHi, outputAtomLo };
					if (outputAtomLo != ModdedAtoms.Dummy) {addColliderAtHex(part, hexOutputLo);};
					if (outputAtomHi != ModdedAtoms.Dummy) {addColliderAtHex(part, hexOutputHi);};
				}
				else if (//Now check for right mediation. hexOutputHi is on the right for this chirality.
						isConsumptionHalfstep // top output not blocked; extra true means that wheels can block outputs
						&& !maybeFindAtom(part, hexOutputLo, new List<Part>(),true).method_99(out _) // low output not blocked; extra true means that wheels can block outputs
						// If you use glitches to make duplicate wheels, two Herriman wheels on opposite sides of the same glyph won't work. Known issue, not going to fix it. 
						&& maybeFindAtom(part, hexInputLeft, gripperList).method_99(out atomLeft) // right input exists
						&& !atomLeft.field_2281 // a single atom
						&& !atomLeft.field_2282 // not held by a gripper
						&& Wheel.maybeFindHerrimanWheelAtom(sim_self, part, hexInputRight).method_99(out atomRight)
						&& Wheel.maybeFindHerrimanWheelAtom(sim_self, part, hexOutputHi).method_99(out atomHi)
						&& API.applyDisproportionRule(atomLeft.field_2280, out outputAtomHi, out outputAtomLo)
				)
				{	outputAtomHi = atomHi.field_2280;
					AtomType inputAtomRight = atomRight.field_2280;
					outputAtomLo = atomLeft.field_2280; 
					if (MediationMath(ref outputAtomHi,ref inputAtomRight,ref outputAtomLo,1))
						{ /* outputAtomLo is only atomright's type briefly. This code sucks */ 
						glyphNeedsToFire(partSimState);
						playSound(sim_self, Glyphs.disproportionSound);
						consumeAtomReference(atomLeft);
						// take care of outputs
						// Disposal Jack
						outputAtomLo = !lodispojack ? outputAtomLo : ModdedAtoms.Dummy;
						partSimState.field_2744 = new AtomType[2] {ModdedAtoms.Dummy, outputAtomLo};
						if (outputAtomLo != ModdedAtoms.Dummy) {addColliderAtHex(part, hexOutputLo);};
						// Change herriman atoms
						//Herriman mediates Right atoms:
						Wheel.DrawHerrimanFlash(SEB, part, hexInputRight);
						Wheel.DrawHerrimanFlash(SEB, part, hexOutputHi);
						changeAtomTypeDonorAnimation(atomRight, inputAtomRight); /* inputAtomRight was changed by MediationMath */
						changeAtomTypeReceiverAnimation(atomHi, outputAtomHi); /*same deal*/
						}
					}
				else if (//Now check for left mediation. hexOutputHi is on the left for this chirality.
						isConsumptionHalfstep // top output not blocked; extra true means that wheels can block outputs
						&& !maybeFindAtom(part, hexOutputHi, new List<Part>(),true).method_99(out _) // low output not blocked; extra true means that wheels can block outputs
							// If you use glitches to make duplicate wheels, two Herriman wheels on opposite sides of the same glyph won't work. Known issue, not going to fix it. 
						&& maybeFindAtom(part, hexInputRight, gripperList).method_99(out atomRight) // right input exists
						&& !atomRight.field_2281 // a single atom
						&& !atomRight.field_2282 // not held by a gripper
						&& Wheel.maybeFindHerrimanWheelAtom(sim_self, part, hexInputLeft).method_99(out atomLeft)
						&& Wheel.maybeFindHerrimanWheelAtom(sim_self, part, hexOutputLo).method_99(out atomLo)
						&& API.applyDisproportionRule(atomRight.field_2280, out outputAtomHi, out outputAtomLo)
				)
				{		outputAtomLo = atomLo.field_2280;
						AtomType inputAtomLeft  = atomLeft.field_2280;
						outputAtomHi = atomRight.field_2280; 
						if (MediationMath(ref outputAtomLo,ref inputAtomLeft,ref outputAtomHi,-1)) /* This code sucks */ 
							{
							glyphNeedsToFire(partSimState);
							playSound(sim_self, Glyphs.disproportionSound);
							consumeAtomReference(atomRight);
							// take care of outputs
							// Disposal Jack
							outputAtomHi = !hidispojack ? outputAtomHi : ModdedAtoms.Dummy;
							partSimState.field_2744 = new AtomType[2] {outputAtomHi, ModdedAtoms.Dummy};
							if (outputAtomHi != ModdedAtoms.Dummy) {addColliderAtHex(part, hexOutputHi);};
							// Change herriman atoms
							//Herriman mediates left atoms:
							Wheel.DrawHerrimanFlash(SEB, part, hexInputLeft);
							Wheel.DrawHerrimanFlash(SEB, part, hexOutputLo);
							changeAtomTypeDonorAnimation(atomLeft, inputAtomLeft); /* inputAtomLeft was changed by MediationMath */
							changeAtomTypeReceiverAnimation(atomLo, outputAtomLo); /*same deal*/
							}
				}
			
			}

			if (partType == Glyphs.Infusion)
			{
				//Hardcoding the functionality of the Glyph of Infusion. It doesn't have an API for custom rules yet.
				HexIndex hexInputLeft = new HexIndex(0, 0);
				HexIndex hexInputRight = new HexIndex(1, 0);

				AtomReference atomLeft, atomRight;

				//case 1: Herriman's wheel can recieve infusion.
				if (!isConsumptionHalfstep /*Should fire when you drag the atom over the glyph, but not at the start of the cycle*/
					&& 
					(// left input exists; checking for Herriman's wheel also
					maybeFindAtom(part, hexInputLeft, gripperList).method_99(out atomLeft) || Wheel.maybeFindHerrimanWheelAtom(sim_self, part, hexInputLeft).method_99(out atomLeft)
					)
					&& Wheel.maybeFindHerrimanWheelAtom(sim_self, part, hexInputRight).method_99(out atomRight) // right input exists
					//Don't need to check for grippers or single atoms or anything here; infusion works even on held molecules
				)
					{if (
						/*do infusion if left atom is actually animismus; we skip salt because it can't infuse anything*/
						atomLeft.field_2280 == ModdedAtoms.TrueMors ||
						atomLeft.field_2280 == ModdedAtoms.GreyMors ||
						atomLeft.field_2280 == API.morsAtomType ||
						atomLeft.field_2280 == API.vitaeAtomType ||
						atomLeft.field_2280 == ModdedAtoms.RedVitae ||
						atomLeft.field_2280 == ModdedAtoms.TrueVitae)
						{	AtomType transleft = atomLeft.field_2280, transright = atomRight.field_2280;
							if(InfusionMath(ref transleft, ref transright, true)) // opposition is permitted since the wheel is receiving
							{
								changeAtomTypeDonorAnimation(atomLeft, transleft);
								changeAtomTypeReceiverAnimation(atomRight, transright);
								playSound(sim_self, Glyphs.infusionSound);
							}
						}


				//case 2: The atom that recieves infusion is not Herriman's wheel.
				} else if (!isConsumptionHalfstep /*Should fire when you drag the atom over the glyph, but not at the start of the cycle*/
					&& 
					(// left input exists; checking for Herriman's wheel also
					maybeFindAtom(part, hexInputLeft, gripperList).method_99(out atomLeft) || Wheel.maybeFindHerrimanWheelAtom(sim_self, part, hexInputLeft).method_99(out atomLeft)
					)
					&& maybeFindAtom(part, hexInputRight, gripperList).method_99(out atomRight) // right input exists
					//Don't need to check for grippers or single atoms or anything here; infusion works even on held molecules
				)
				{
					AtomType transleft = default, transright = default;
					bool DoInfusion = false;
					// Don't code like this. Use a dict or something instead. Hardcode to account for opposition and "are these atoms actually animismus?"

					if (atomLeft.field_2280 == API.vitaeAtomType && atomRight.field_2280 == API.saltAtomType) { transleft = API.saltAtomType; transright = API.vitaeAtomType; DoInfusion = true;}
					if (atomLeft.field_2280 == ModdedAtoms.RedVitae && atomRight.field_2280 == API.saltAtomType) { transleft = API.vitaeAtomType; transright = API.vitaeAtomType; DoInfusion = true;}
					if (atomLeft.field_2280 == ModdedAtoms.RedVitae && atomRight.field_2280 == API.vitaeAtomType) { transleft = API.vitaeAtomType; transright = ModdedAtoms.RedVitae; DoInfusion = true;}
					if (atomLeft.field_2280 == ModdedAtoms.TrueVitae && atomRight.field_2280 == API.saltAtomType) { transleft = ModdedAtoms.RedVitae; transright = API.vitaeAtomType; DoInfusion = true;}
					if (atomLeft.field_2280 == ModdedAtoms.TrueVitae && atomRight.field_2280 == API.vitaeAtomType) { transleft = ModdedAtoms.RedVitae; transright = ModdedAtoms.RedVitae; DoInfusion = true;}
					if (atomLeft.field_2280 == ModdedAtoms.TrueVitae && atomRight.field_2280 == ModdedAtoms.RedVitae) { transleft = ModdedAtoms.RedVitae; transright = ModdedAtoms.TrueVitae; DoInfusion = true;}

					if (atomLeft.field_2280 == API.morsAtomType && atomRight.field_2280 == API.saltAtomType) { transleft = API.saltAtomType; transright = API.morsAtomType; DoInfusion = true;}
					if (atomLeft.field_2280 == ModdedAtoms.GreyMors && atomRight.field_2280 == API.saltAtomType) { transleft = API.morsAtomType; transright = API.morsAtomType; DoInfusion = true;}
					if (atomLeft.field_2280 == ModdedAtoms.GreyMors && atomRight.field_2280 == API.morsAtomType) { transleft = API.morsAtomType; transright = ModdedAtoms.GreyMors; DoInfusion = true;}
					if (atomLeft.field_2280 == ModdedAtoms.TrueMors && atomRight.field_2280 == API.saltAtomType) { transleft = ModdedAtoms.GreyMors; transright = API.morsAtomType; DoInfusion = true;}
					if (atomLeft.field_2280 == ModdedAtoms.TrueMors && atomRight.field_2280 == API.morsAtomType) { transleft = ModdedAtoms.GreyMors; transright = ModdedAtoms.GreyMors; DoInfusion = true;}
					if (atomLeft.field_2280 == ModdedAtoms.TrueMors && atomRight.field_2280 == ModdedAtoms.GreyMors) { transleft = ModdedAtoms.GreyMors; transright = ModdedAtoms.TrueMors; DoInfusion = true;}

					if (DoInfusion) // If the two atoms are valid for an infusion to happen...
					{
						changeAtomTypeDonorAnimation(atomLeft, transleft);
						changeAtomTypeReceiverAnimation(atomRight, transright);
						playSound(sim_self, Glyphs.infusionSound);
					}	
				}
			}
		}
	}


	public override void Unload(){
		caowhook?.Dispose();
	}

	public override void PostLoad()
	{
		On.SolutionEditorScreen.method_50 += SES_Method_50;
		On.SolutionEditorBase.method_1997 += DrawPartSelectionGlows;
		On.Solution.method_1948 += Solution_method_1948;
		
		//optional dependencies
		if (QuintessentialLoader.CodeMods.Any(mod => mod.Meta.Name == "FTSIGCTU"))
		{
			Logger.Log("[TrueAnimismus] Detected optional dependency 'FTSIGCTU' - adding mirror rules for parts.");
			Glyphs.LoadMirrorRules();
		}
		else
		{
			Logger.Log("[TrueAnimismus] Did not detect optional dependency 'FTSIGCTU'.");
		}
	}

	public void DrawPartSelectionGlows(On.SolutionEditorBase.orig_method_1997 orig, SolutionEditorBase seb_self, Part part, Vector2 pos, float alpha)
	{
		if (part.method_1159() == Wheel.Herriman) Wheel.drawSelectionGlow(seb_self, part, pos, alpha);
		orig(seb_self, part, pos, alpha);
	}
	public void SES_Method_50(On.SolutionEditorScreen.orig_method_50 orig, SolutionEditorScreen SES_self, float param_5703)
	{	
		ChiralityFlip.SolutionEditorScreen_method_50(SES_self);
		orig(SES_self, param_5703);
	}

	// Making the Disposal Jack able to be a cap.
	static string errStr(string str) => (string)class_134.method_253(str, string.Empty);
	public static bool Solution_method_1948(On.Solution.orig_method_1948 orig,
	Solution solution_self,
	Part part,
	HexIndex hex1,
	HexIndex hex2,
	HexRotation rot,
	out string errorMessageOut)
	{
		string errorMessage;
		bool ret = orig(solution_self, part, hex1, hex2, rot, out errorMessage);

		// if (errorMessage == errStr("There is already another part here."))
		// {
		// 	Logger.Log(part.method_1159().field_1529);
		// }

		if (errorMessage == errStr("There is already another part here.") && (part.method_1159() == Glyphs.DispoJack))
		{
			//Go check if the Dispojack is being held over a compatible iris.
			//If so, let it be placed (nulling the error message and ret is true)
			foreach (Part cappablepart in solution_self.field_3919.Where(
				x => 
				x.method_1159() == PartTypes.field_1780/*Glyph of Animismus*/ || 
				x.method_1159() == Glyphs.Disproportion || 
				x.method_1159() == Glyphs.DisproportionR || 
				x.method_1159() == Glyphs.LeftHand))
			{
				if (Glyphs.AtopIrisHoverHex(cappablepart,hex2,solution_self)) 
					{errorMessage = null;
					ret = true;}
			}
			
		}

		errorMessageOut = errorMessage;
		return ret;
	}

	// WIP WIP WIP

	private static ILHook caowhook;
	public static void AnimismusFiringHook()
	{
		caowhook = new ILHook
			(
				typeof(Sim).GetMethod("orig_method_1832", BindingFlags.NonPublic | BindingFlags.Instance),
				ConditionalAnimismusOutputtingWrapper
			);
	}

	private static void ConditionalAnimismusOutputtingWrapper(ILContext il)
	{
	var gremlin = new ILCursor(il);
	// Send code-modifying gremlin to roughly where the glyph of animismus's native code is
	// not specifying an exact instruction number because that apparently changes if some other mod roots around in method_1832
	// including Quintessential itself 
	// oh no
	
	gremlin.Goto(640);
	
	// And THIS syntax goes to the start of a block of instructions that dnSpy says is what the "spawn vitae" part looks like under the hood.
	// The 35 is the 35th local variable, molecule2, which animismus will turn into elemental vitae
	// Whatever you say, Mr. President.
	if (gremlin.TryGotoNext(MoveType.Before,
	x => x.MatchNewobj<Molecule>(),
	x => x.MatchStloc(35),
	x => x.MatchLdloc(35),
	x => x.MatchLdsfld(out _)
		))

		//Skip over the first line of the "spawn vitae and mors" code because it has an IL label attached and OM crashes if you remove those.
		//The skipped line gets to execute normally, but it just declares the existence of a molecule object and loads it into a new variable.
		//If that new molecule doesn't get loaded with an atom or added to the molecule list or anything, there's no consequence to ignoring it.

		gremlin.GotoNext(); //new molecule object
		gremlin.GotoNext(); //loaded into a new variable, which we proceed to ignore

		//Remove the rest of the "spawn vitae and mors" code 
		gremlin.RemoveRange(30);
		//Grab the current Sim so we can reference anything on the board we want by hitting it with methods until the info falls out
		Logger.Log("gremlin.Emit(OpCodes.Ldarg_0)");
		gremlin.Emit(OpCodes.Ldarg_0);
		//Grab local variable #6; it's a class that's keeping track of which glyph we're messing with
		//If you want to mess with the deets of any other vanilla glyph, you will probably end up in orig_method_1832 and grabbing local variable #6, too
		Logger.Log("gremlin.Emit(OpCodes.Ldloc_S, 6);");
		gremlin.Emit(OpCodes.Ldloc_S, (byte)6);
		//Use them to do this
		Logger.Log("gremlin.EmitDelegate<Action<Sim, Sim.class_402>>((sim_self,tracker) => ");
		gremlin.EmitDelegate<Action<Sim, Sim.class_402>>((sim_self,tracker) => 
			{	
				//	Logger.Log(sim_self);
				// 	Logger.Log(tracker.field_3841.method_1159());

				// Spawn vitae and mors atoms as normal, UNLESS there's a dispojack on an iris or a herriman wheel atom covering it.

				Logger.Log(sim_self);
				Logger.Log(tracker);
				Part part = tracker.field_3841;
				Logger.Log(part.method_1159());
				SolutionEditorBase SEB = sim_self.field_3818;

				bool blockvitae = false;
				bool blockmors = false;
				HexIndex hexOutputHi = new HexIndex(0,1);
				HexIndex hexOutputLo = new HexIndex(1,-1);


				foreach (Part whatisthis in SEB.method_502().field_3919)
				{Logger.Log(whatisthis.method_1159());}

				foreach (Part dispojack in SEB.method_502().field_3919.Where(x => x.method_1159() == Glyphs.DispoJack))
				{
					Logger.Log(dispojack.method_1161());
					Logger.Log(hexOutputHi.Rotated(part.method_1163()) + part.method_1161());
					Logger.Log(hexOutputLo.Rotated(part.method_1163()) + part.method_1161());
					if (dispojack.method_1161() == hexOutputHi.Rotated(part.method_1163()) + part.method_1161())
					{blockvitae = true;}
					if (dispojack.method_1161() == hexOutputLo.Rotated(part.method_1163()) + part.method_1161())
					{blockmors = true;}
				}

				if (Wheel.maybeFindHerrimanWheelAtom(sim_self, part, hexOutputHi).method_99(out _)){blockvitae = true;}
				if (Wheel.maybeFindHerrimanWheelAtom(sim_self, part, hexOutputLo).method_99(out _)){blockmors = true;}

				// Recreation of the vite and mors spawning code; now to add conditionality
				if(!blockvitae)
				{Molecule vitmolecule = new Molecule();
				vitmolecule.method_1105(new Atom(API.vitaeAtomType), part.method_1184(new HexIndex(0,1)));
				sim_self.field_3823.Add(vitmolecule);
				}
				if(!blockmors)
				{
				Molecule morsmolecule = new Molecule();
				morsmolecule.method_1105(new Atom(API.morsAtomType), part.method_1184(new HexIndex(1,-1)));
				sim_self.field_3823.Add(morsmolecule);
				}
			});
	}
}