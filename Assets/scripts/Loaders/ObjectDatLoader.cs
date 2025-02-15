﻿using UnityEngine;
using System.Collections;
using System.IO;

public class ObjectDatLoader : Loader
{

    public struct MeleeData
    {
        //0000   Int8   damage modifier for Slash attack
        //0001   Int8   damage modifier for Bash attack
        //0002   Int8   damage modifier for Stab attack
        //0003   int8   used in attack charge calculations  (min charge?)
        //0004   int8   attackSpeed - how quickly an attack charge builds up.
        //0005   int8   used in attack charge calculations  (max charge?)
        //0006   Int8   skill type (3: sword, 4: axe, 5: mace, 6: unarmed)
        //0007   Int8   durability	
        public short Slash;
        public short Bash;
        public short Stab;
        public short MinCharge;
        public short WeaponSpeed;
        public short MaxCharge;
        public short Skill;
        public short Durability;
    };

    public struct RangedData
    {
        public int ammo;
        public int damage;
    }

    public struct ArmourData
    {
        /*0000   Int8   protection
        0001   Int8   durability
        0002   Int8   unknown
        0003   Int8   category:
        00: shield
        01: body armour
        03: leggings
        04: gloves
        05: boots
        08: hat
        09: ring*/
        public short protection;
        public short durability;
        public short category;
    }

    public struct ContainerData
    {
        /** Containers table (0x0080-0x008f)

        0000   Int8   capacity in 0.1 stones
        0001   Int8   objects accepted; 0: runes, 1: arrows, 2: scrolls, 3: edibles, 0xFF: any
        0002   Int8   number of slots available?; 2: , -1: any
        */

        public int capacity;
        public int objectsMask;
        public int slots;
    }

    public struct LightSourceData
    {
        //* Light source table (0x0090-0x009f)
        public int brightness;
        public int duration;
        //0000   Int8   light brightness (max. is 4; 0 means unlit)
        //0001   Int8   duration (00: doesn't go out, e.g. taper of sacrifice)
    }


    public struct CritterData
    {
        /*
00h 	1 	uint8 	Level 	Level of the creature.
01h 	3 	 ?? 	 ?? 	 ??
04h 	1 	uint8 	HitPoints 	Average hit points. <is this meant to be uint8
05h     1    uint8 Strength  - for the player these values are copied at save load into the table.
06h 	1 	uint8 	Dexterity
07h 	1 	 uint8	 Intelligence)
08h 	1 	uint8 	FluidAndRemains 	A combination of remains after death and the type of blood splatters this produces.
Mask 0x0F is the splatter type, 0 for dust, 8 for red blood. 
Mask 0xF0 is the remains; Nothing = 0x00, RotwormCorpse = 0x20, Rubble = 0x40, WoodChips = 0x60, Bones = 0x80, GreenBloodPool = 0xA0, RedBloodPool = 0xC0, RedBloodPoolGiantSpider = 0xE0.
09h 	1 	uint8 	GeneralType 	An index into the strings on page 8, offset 370. This string is the generic name for the creature, like "a creature" for "a goblin" or "a rat" for "a giant rat".
0Ah 	1 	uint8 	Passiveness 	Relative passiveness. 255 will never take a swing at you, even if you kill them.
0Bh 	1 	magic related to the critter having extra/specific spells.   ?? 	 ?? 	 ??
0Ch 	1 	uint8 	MovementSpeed 	Speed of movement; 0 is immobile, maxes out at 12 for vampire bat.
0Dh 	2 	 ?? 	 ?? 	 ??
0Fh 	1 	uint8 	PoisonDamage 	Amount of poison damage this is capable of on attack.
10h 	1 	uint8 	Category 	Ethereal = 0x00 (Ethereal critters like ghosts, wisps, and shadow beasts), Humanoid = 0x01 (Humanlike non-thinking forms like lizardmen, trolls, ghouls, and mages), Flying = 0x02 (Flying critters like bats and imps), Swimming = 0x03 (Swimming critters like lurkers), Creeping = 0x04 (Creeping critters like rats and spiders), Crawling = 0x05 (Crawling critters like slugs, worms, reapers (!), and fire elementals (!!)), EarthGolem = 0x11 (Only used for the earth golem), Human = 0x51 (Humanlike thinking forms like goblins, skeletons, mountainmen, fighters, outcasts, and stone and metal golems).
11h 	1 	uint8 	EquipmentDamage 	Amount of equipment damage this is capable of on attack.
12h 	1 	 ?? 	 ?? 	 ??
13h 	9 	Probability[3] 	Probabilities 	Each has the form (uint16 value, uint8 percent). What this means is unknown.
1Ch 	12 	 ?? 	 ?? 	 ??
28h 	2 	uint16 	Experience 	Experience provided when killed.
2Ah 	5 	 ?? 	 ?? 	 ?? list of speels. Looks like 3 values?
2Dh     Some sort of value (magic users related)
2Fh 	1 	uint8 	 ?? 	Always 73.
*/
        public int Level;
        public short AvgHit;//Is this defence?????
        public int Strength;
        public int Dexterity;
        public int Intelligence;
        public int Remains;
        public int Blood;
        public int Race;
        public int Passive;
        public int Defence;
        public int Speed;
        public int Poison;
        public int Category;
        public int EquipDamage;
        //public int ProbValue1;
        public int[] AttackChanceToHit;//What defense rolls against to save against this attack  = new int[3];
        public int[] AttackDamage; //the damage of the choose attack.
        public int[] AttackProbability; //Probability of which attack/animation to execute

        public int[] Loot;  //A list of item ids that the Npc drops on death or uses in bartering

        public int Exp;

        //Spells this critter can cast. -1 is no spell.
        // public sbyte Spell0;
        //public sbyte Spell1;
        //public sbyte Spell2;
        public sbyte[] Spells;

        public byte Unk2D;
        public byte Unk2E;
    };

    public struct NutritionData
    {
        public int FoodValue;
    }


    public MeleeData[] weaponStats = new MeleeData[16];
    public RangedData[] rangedStats = new RangedData[8];
    public ArmourData[] armourStats = new ArmourData[32];
    public ContainerData[] containerStats = new ContainerData[16];
    public LightSourceData[] lightSourceStats = new LightSourceData[8];
    public CritterData[] critterStats = new CritterData[64];

    public NutritionData[] nutritionStats = new NutritionData[16];

    public ObjectDatLoader()
    {
        char[] obj_dat;
        int add_ptr;
        if (DataLoader.ReadStreamFile(Path.Combine(BasePath,"DATA" , "OBJECTS.DAT"), out obj_dat))
        {
            add_ptr = 2;
            int j = 0;
            for (int i = 0; i < 16; i++)
            {
                weaponStats[j].Slash = (short)DataLoader.getValAtAddress(obj_dat, add_ptr, 8);
                weaponStats[j].Bash = (short)DataLoader.getValAtAddress(obj_dat, add_ptr + 1, 8);
                weaponStats[j].Stab = (short)DataLoader.getValAtAddress(obj_dat, add_ptr + 2, 8);
                weaponStats[j].MinCharge = (short)DataLoader.getValAtAddress(obj_dat, add_ptr + 3, 8);
                weaponStats[j].WeaponSpeed = (short)DataLoader.getValAtAddress(obj_dat, add_ptr + 4, 8);
                weaponStats[j].MaxCharge = (short)DataLoader.getValAtAddress(obj_dat, add_ptr + 5, 8);
                weaponStats[j].Skill = (short)DataLoader.getValAtAddress(obj_dat, add_ptr + 6, 8);
                weaponStats[j].Durability = (short)DataLoader.getValAtAddress(obj_dat, add_ptr + 7, 8);
                add_ptr = add_ptr + 8;
                j++;
            }


            add_ptr = 0x82;
            j = 0;
            for (int i = 0; i < 8; i++)
            {//ranged weapon damage stats.
                rangedStats[j].damage = (int)((DataLoader.getValAtAddress(obj_dat, add_ptr, 16) >> 9) & 0x7f);
                add_ptr = add_ptr + 3;
                j++;
            }
            j = 0;
            for (int i = 0; i < 8; i++)
            {//Ranged weapons
             //This is probably wrong!!
             //rangedStats[j].ammo=0x10 + ((((int)DataLoader.getValAtAddress(obj_dat,  add_ptr, 16) >> 9) & 0x7F));
             //rangedStats[j].ammo= (int)DataLoader.getValAtAddress(obj_dat,  add_ptr, 16) ;
             //rangedStats[j].durability= (int)DataLoader.getValAtAddress(obj_dat,  add_ptr + 2, 8);
                rangedStats[j].damage = (int)DataLoader.getValAtAddress(obj_dat, add_ptr, 8);
                rangedStats[j].ammo = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 2, 8) + 16;//an index into the ranged table+16;
                add_ptr = add_ptr + 3;
                j++;
            }

            add_ptr = 0xb2;
            j = 0;
            for (int i = 0; i < 32; i++)
            {
                armourStats[j].protection = (short)DataLoader.getValAtAddress(obj_dat, add_ptr, 8);
                armourStats[j].durability = (short)DataLoader.getValAtAddress(obj_dat, add_ptr + 1, 8);
                armourStats[j].category = (short)DataLoader.getValAtAddress(obj_dat, add_ptr + 3, 8);
                add_ptr = add_ptr + 4;
                j++;
            }


            add_ptr = 0xd32;
            j = 0;
            for (int i = 0; i < 16; i++)
            {
                containerStats[i].capacity = (int)DataLoader.getValAtAddress(obj_dat, add_ptr, 8);
                containerStats[i].objectsMask = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 1, 8);
                containerStats[i].slots = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 2, 8);
                add_ptr = add_ptr + 3;
                j++;
            }


            add_ptr = 0xd62;
            j = 0;
            for (int i = 0; i < 8; i++)
            {//Light sources
                lightSourceStats[j].duration = (int)DataLoader.getValAtAddress(obj_dat, add_ptr, 8);
                lightSourceStats[j].brightness = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 1, 8);
                add_ptr = add_ptr + 2;
                j++;
            }

            add_ptr = 0xd82;
            j = 0;
            for (int i = 0; i < 16; i++)
            {//Nutrition stats
                nutritionStats[j].FoodValue = (int)(sbyte)DataLoader.getValAtAddress(obj_dat, add_ptr, 8);
                add_ptr = add_ptr + 1;
                j++;
            }


            add_ptr = 0x132;
            j = 0;
            for (int i = 0; i < 64; i++)
            {//Critters
                critterStats[j].Level = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 0, 8);//Level
                critterStats[j].AvgHit = (short)DataLoader.getValAtAddress(obj_dat, add_ptr + 4, 8);//Average Hitpoints - changed from uint16 to uint8

                critterStats[j].Strength = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 5, 8); //Base damage calculations
                critterStats[j].Dexterity = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 6, 8);// attackscore calculations
                critterStats[j].Intelligence = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 7, 8); //need to id usages. probably magic spell attacks

                critterStats[j].Remains = (int)((DataLoader.getValAtAddress(obj_dat, add_ptr + 8, 8) & 0xF0) >> 4);//Remains body
                critterStats[j].Blood = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 8, 8) & 0x0F;//Remains blood

                critterStats[j].Race = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 9, 8);//Uwformats calls this General Type

                critterStats[j].Passive = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 0xA, 8);//Passiveness
                critterStats[j].Defence = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 0xB, 8);//Defence
                critterStats[j].Speed = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 0xC, 8);//Speed
                critterStats[j].Poison = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 0xF, 8) & 0xF;//Poison Damage
                critterStats[j].Category = (int)(DataLoader.getValAtAddress(obj_dat, add_ptr + 0x10, 8));//& 0x1F);//Category
                critterStats[j].EquipDamage = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 0x11, 8);//Equipment damage

                critterStats[j].AttackChanceToHit = new int[3];
                critterStats[j].AttackDamage = new int[3];
                critterStats[j].AttackProbability = new int[3];

                critterStats[j].AttackChanceToHit[0] = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 0x13, 8);
                critterStats[j].AttackDamage[0] = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 0x14, 8);
                critterStats[j].AttackProbability[0] = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 0x15, 8);

                critterStats[j].AttackChanceToHit[1] = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 0x16, 8);
                critterStats[j].AttackDamage[1] = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 0x17, 8);
                critterStats[j].AttackProbability[1] = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 0x18, 8);

                critterStats[j].AttackChanceToHit[2] = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 0x19, 8);
                critterStats[j].AttackDamage[2] = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 0x1A, 8);
                critterStats[j].AttackProbability[2] = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 0x1B, 8);

                critterStats[j].Exp = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 0x28, 16);//Exp

                critterStats[j].Loot = new int[4];
                critterStats[j].Loot[0] = -1; critterStats[j].Loot[1] = -1; critterStats[j].Loot[2] = -1; critterStats[j].Loot[3] = -1;

                int byte1 = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 0x20, 8);
                if ((byte1 & 0x1) == 1)
                {
                    critterStats[j].Loot[0] = byte1 >> 1;
                }

                byte1 = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 0x20 + 1, 8);
                if ((byte1 & 0x1) == 1)
                {
                    critterStats[j].Loot[1] = byte1 >> 1;
                }

                byte1 = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 0x20 + 2, 16);
                if (byte1 != 0)
                {
                    critterStats[j].Loot[2] = (byte1 >> 4);
                }

                byte1 = (int)DataLoader.getValAtAddress(obj_dat, add_ptr + 0x20 + 4, 16);
                if (byte1 != 0)
                {
                    critterStats[j].Loot[3] = (byte1 >> 4);
                }




                // critterStats[j].Spell0 = (sbyte)DataLoader.getValAtAddress(obj_dat, add_ptr + 0x2a, 8);
                // critterStats[j].Spell1 = (sbyte)DataLoader.getValAtAddress(obj_dat, add_ptr + 0x2b, 8);
                // critterStats[j].Spell2 = (sbyte)DataLoader.getValAtAddress(obj_dat, add_ptr + 0x2c, 8);
                critterStats[j].Unk2D = (byte)(DataLoader.getValAtAddress(obj_dat, add_ptr + 0x2d, 8));
                critterStats[j].Unk2E = (byte)(DataLoader.getValAtAddress(obj_dat, add_ptr + 0x2e, 8));



                critterStats[j].Spells = new sbyte[8];
                // for (int k =0; k<= critterStats[j].Spells.GetUpperBound(0); k++)
                for (int k = 0; k < 3; k++)
                {//First 3 spells are stored in the critter data.
                    critterStats[j].Spells[k] = (sbyte)DataLoader.getValAtAddress(obj_dat, add_ptr + 0x2a + k, 8);
                }
                //if (_RES == GAME_UW2)
                //{//The remainder are retrieved when needded
                //    int ctr = 3;
                //    if (critterStats[j].Race == 0x17)//Liche with extra magic
                //    {
                //        if (((DataLoader.getValAtAddress(obj_dat, add_ptr + 0xB, 8) >> 7) & 0x1) == 1)
                //        {
                //            critterStats[j].Spells[ctr++] = 0x39;
                //        }
                //        if(critterStats[j].Unk2E<=0x2D)
                //        {
                //            critterStats[j].Spells[ctr++] = 0x23;
                //        }
                //        if(ObjectInteraction.ScaleDamage(j+64,1,8)==0)
                //        {
                //            critterStats[j].Spells[ctr++] = 0x1C;
                //        }
                //        if (((critterStats[j].Unk2D>>1) & 0x7f)< 0x19)
                //        {
                //            if (critterStats[j].Level>=9)
                //            {
                //                critterStats[j].Spells[ctr++] = 0x3b;
                //            }
                //        }
                //    }
                //}


                add_ptr = add_ptr + 48;
                j++;
            }
        }
    }
}
