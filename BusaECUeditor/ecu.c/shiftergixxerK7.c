/*

   	shiftergixxerK7.c
	
    This file is part of ecueditor.com

    ecueditor.com is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ecueditor.com is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hayabusa ECUeditor.  If not, see <http://www.gnu.org/licenses/>.

    Notice: Please note that under GPL if you use this program or parts of it
    you are obliged to distribute your software including source code
    under this same license for free. For more information see paragraph 5
    of the GNU licence.

	v1.00 first release for Gixxer K7,K8    - WORK IN PROGRESS
	
*/

/*
	These are the RAM variable addresses that are internal to this subroutine
*/
#define ECU_AD_GPS *(volatile unsigned short *)  		0x00804318 // Gixxer
#define ECU_GPS	*(volatile unsigned char *)  			  0x008050FC // Gixxer
#define ECU_RPM *(volatile unsigned short *)  			0x0080507A // Gixxer
#define ECU_KillFlag *(volatile unsigned char *)  	0x0080657F // Gixxer
#define IGN_KillFlag *(volatile unsigned char *)  	0x008063E2 // Gixxer
#define PORT3   *(volatile unsigned char *)  				0x00804686 // Gixxer bit0=DSM2, bit1=DSM1
/*
	Internal variables for this subroutine only, these are borrowed from the ecu ram area using addresses
	that are considered not having been assigned for any use.
*/
#define ramaddr 										0x00806900 // Gixxer - this is the starting address for free ram area needed for this program
#define killcount *(volatile unsigned short *)    		(ramaddr)
#define killswitch *(volatile unsigned short *)   		(ramaddr + 4)
#define killcountactive *(volatile unsigned short *) 	(ramaddr + 8)
#define initialized *(volatile unsigned short *)  		(ramaddr + 12)
#define killflag *(volatile unsigned short *)  			(ramaddr + 16)

/* +0x18 - 0x60 reserved for boostfuel and nitrous control */

#define	previousgear *(volatile unsigned short *)  		(ramaddr + 0x64)
#define	overboost *(volatile unsigned char *)  			(ramaddr + 0x64 + 4) /* this is a joint variable with turbofuel module */
#define	duration_kill *(volatile unsigned short *)  	(ramaddr + 0x64 + 8)
#define	minimum_killrpm *(volatile unsigned short *)  		(ramaddr + 0x64 + 12)

#define P8bit	*(volatile unsigned char *)					0x20
#define ModeA						0x0
#define ModeB						0x1
#define ModeC						0x2
#define DSM2						0x1
#define DSM1						0x2
#define PAIR						0x20

/*
	The shift kill variables are defined here, e.g kill times. These are adjusted using ecueditor.
*/

#pragma SECTION C PARAMS //0x5D900
const short const_pgmid = 				100;			// 0 program id, must match to ecueditor version to be able to load this code to ecu
const short const_killtime_std = 		80;			// 2 kill times, these are egnie revolutions for the rpm range
const short const_killtime_gear2 = 		80;			// 4
const short const_killtime_gear3 = 		80;			// 6
const short const_killtime_gear4 = 		80;			// 8
const short const_killtime_gear5 = 		80;			// 10
const short const_killtime_gear6 = 		80;			// 12
const short const_shiftrpm	 = 			0x1400;			// 14
const short const_shiftrpm2 = 			0x1400;			// 16
const short const_shiftrpm3 = 			0x1400;			// 18
const short const_shiftrpm456 = 		0x1400;			// 20
const short minkillactive = 			5;   			// 22. hysteresis for how many revolutions the gps must be active before a kill is initiated
const short killcountdelay = 			300;			// 24. delay of revolutions how many times must pass from last kill before a kill can be initiated again
const short fuelkillactive = 			1;				// 26. igncut will be used, this can be changed by ecueditor to 0 to disable fuelcut
const short ignkillactive = 			1;				// 28. fuelcut will be used, this can be changed by ecueditor to 0 to disable igncut
const short killtimedivider = 			2560;
const unsigned char	 DSMSELECTED =				0;		// 0x55420 Set this to 0x1 for DSM2 or 0x2 for DSM1 or use directly DSM1 / DSM2
/*
	Constants 
*/ 
#define ACTIVE 							1			/* internal const for the program */
#define DEACTIVE 						0			/* internal const for the program */
#define CUTACTIVE 						0x3			/* Bitflag for setting the cut active for ecu fuelcut variable, both soft and harduct */
#define SHIFTERACTIVE 					0x40		/* This is the internal voltage limit for 200Ohm resistor that is used to detect that shifter is active */


/*
	The fuelcut code, the program enters here each fuel calculation loop after calculating the limiters but before calculating the amount of fuel
*/
#pragma SECTION P SHIFTERCODE //0x5DA00 Gixxer
void shiftermain(void)
{

	/*
		First time only initialization of internal variables. Ram is inizialized to zero values after hw boot. assumed as its in gen1 so.
	*/		
 	if (initialized != 1)
  		{
   			initialized= 1;
			killcount = killcountdelay;
  		}


	/*
		As the each cycle this program is run is twice on every revolution we need to adjust the kill time
		to be multiplied by rpm. ECU_RPM / 2.56 is the real rpm.
	*/		
    if (((ECU_AD_GPS >> 2)  <=  SHIFTERACTIVE ) || ((PORT3 & DSMSELECTED) != 0))
 		{

						/*
							gear specific kill times active only with DSM activation
						*/
			 			if (DSMSELECTED != 0)
						{
							if (previousgear == 1)
								{
									duration_kill = (const_killtime_std * (ECU_RPM>>2)) / killtimedivider;
									minimum_killrpm = const_shiftrpm;
								}
							else if (previousgear == 2)
								{
									duration_kill = (const_killtime_gear2 * (ECU_RPM>>2)) / killtimedivider;
									minimum_killrpm = const_shiftrpm2;
								}
							else if (previousgear == 3)
								{
									duration_kill = (const_killtime_gear3 * (ECU_RPM>>2)) / killtimedivider;
									minimum_killrpm = const_shiftrpm3;
								}
							else if (previousgear == 4)
								{
									duration_kill = (const_killtime_gear4 * (ECU_RPM>>2)) / killtimedivider;
									minimum_killrpm = const_shiftrpm456;
								}
							else if (previousgear == 5)
								{
									duration_kill = (const_killtime_gear5 * (ECU_RPM>>2)) / killtimedivider;
									minimum_killrpm = const_shiftrpm456;
								}
							else	// gear can not be used, then use default killtime 
								{
									duration_kill = (const_killtime_gear6 * (ECU_RPM>>2)) / killtimedivider;
									minimum_killrpm = const_shiftrpm456;
								}
						}
						else
						{
						/*
							if DSM activation is not used then use
						*/
						duration_kill = (const_killtime_std * (ECU_RPM>>2)) / killtimedivider;
						minimum_killrpm = const_shiftrpm;
						}

				if (ECU_RPM > minimum_killrpm)
				{
					killswitch= ACTIVE;
				}
 		}
 	else
 		{
  			killswitch = DEACTIVE;
		}

	/*
		Main logic for processing fuel/ignitionkill function.
	*/
 	if( killflag == ACTIVE ) 
 		{
			/*
				Hold the killflag active until minimum duration of kill time is reached.
				When reaching the duration of kill time set kill deactive and set killcount
				to delay calculation mode that must be reached before next kill initialization. 
			*/
     		killcount += 1;          
     		if( killcount  >= duration_kill)
     			{
         			killflag = DEACTIVE;
         			killcount = killcountdelay; 
     			}
 		}
 	else                          
 		{
     		if( killcount > 0 )       
     			{
					/*
						Killswitch delay calculation mode, count from killcountdelay backwards
						waiting for the next killswitch cycle. The delay is calculated only 
						when killswitch is not depressed.
					*/
         			if( killswitch != ACTIVE )  
         				{
             				killcount -= 1 ;
         				}
     			}
     		else                   
     				{
					/*
						Killcount is zero, a new kill cycle can be initialized.
					*/
         			if( killswitch == ACTIVE )
         				{
             				killcountactive += 1;
             				if( killcountactive >= minkillactive )
								/* 
									If minimum killflag active time reached then activate kill
									and reset the killswitch counter back to zero.
								*/
			 					{
									killflag = ACTIVE;
           							killcount = 0;
	          					}
         				}
         			else
         				{
             				killcountactive = DEACTIVE;  
							/*
								Only set previousgear when you know that the GPS has stabilized enough after previous kill cycle
							*/
							previousgear = ECU_GPS;
         				}
     				}
 		}

	/*
		Use fuelcut only if parametrized to do so.
		Set the fuelkill on if KillFlag indicates that kill is active.
	*/		
	if (fuelkillactive == ACTIVE)
		if ( killflag == ACTIVE) 
			{ 
				ECU_KillFlag = ECU_KillFlag | CUTACTIVE; 
			}

	/*
		This is the overboost limit that is checked if boostfuel module is loaded.
	*/			
	if ((BOOSTACTIVE != 0xFF ) && (overboost != 0))
			{ 
				ECU_KillFlag = ECU_KillFlag | CUTACTIVE; 
			}


/*
	This is inline assembly put at the end of the fuelcut code that returns control back to main loop in the ecu code.
*/
#pragma keyword asm on
	asm(
	" addi sp, #16 \n"
	" jmp R14 \n"
	);

}


/*
		Use igncut only if parametrized to do so.
		Set the ignitionkill on if KillFlag indicates that kill is active.
*/		
#pragma SECTION P IGNCODE //0x5DE00 Gixxer Check this after compiling the code
void ignmain(void)
{
	if (ignkillactive == ACTIVE)
			if ( killflag == ACTIVE) 
			{ 
				IGN_KillFlag = IGN_KillFlag | CUTACTIVE; 
			}


#pragma keyword asm on
	asm(
	" addi sp, #16 \n"
	" jmp R14 \n"
	);
}
	
