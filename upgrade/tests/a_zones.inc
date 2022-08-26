/*  SA:MP Functions
 *
 *  ZONES Functions By ~Slayer101~
 *  ZONES Functions Aided By Betamaster (locations), Mabako (locations), Simon (finetuning), and Cueball.
 *
 *  (c) Copyright 2005-2014, SA:MP Team
 *
 */

#if defined _zones_included
	#endinput
#endif
#define _zones_included
#pragma library samp
#include <a_samp>

#define MAX_ZONE_NAME 28

//---------------------------------------------------------------------------------------------------

//NATIVES: KEEP THESE COMMENTED! THEY WILL STILL SHOW IN THE RIGHT HAND SIDE OF PAWN!

//native GetPlayer2DZone(playerid, zone[], len);
//native GetPlayer3DZone(playerid, zone[], len);
//native IsPlayerInZone(playerid, zone[]);

//---------------------------------------------------------------------------------------------------

//OUR MAJOR ARRAY: (GLOBAL)

enum SAZONE_MAIN { //Betamaster
		SAZONE_NAME[28],
		Float:SAZONE_AREA[6]
};

static const gSAZones[][SAZONE_MAIN] = {  // Majority of names and area coordinates adopted from Mabako's 'Zones Script' v0.2
	//	NAME                            AREA (Xmin,Ymin,Zmin,Xmax,Ymax,Zmax)

// LC-Zones:
{"Francis Intl. Airport", {-2310.71, 990.272, 0.0, -2023.55, 1402.69, 155.802}},
{"Francis Intl. Airport", {-2976.69, 776.426, 0.0, -2316.82, 1448.51, 244.396}},
{"Francis Intl. Airport", {-2982.97, 455.29, -45.9404, -1818.63, 1531.56, 254.696}},
{"Cochrane Dam", {-2744.5, 1893.44, -46.7412, -2217.52, 2504.54, 253.344}},
{"Chinatown", {-604.579, 891.711, -21.203, -284.58, 1336.31, 191.101}},
{"Newport", {-1149.89, 740.81, -0.000144958, -734.893, 1387.81, 198.864}},
{"Fort Staunton", {-1110.12, 1388.38, 7.62939e-005, -735.678, 1738.38, 173.897}},
{"Portland Beach", {39.37, 1186.53, -29.883, 447.6, 1999.63, 120.117}},
{"Harwood", {-604.021, 1721.82, -48.5832, 38.98, 2122.68, 101.417}},
{"Shoreside Lift Bridge", {-1909.72, 1128.27, 0.213943, -1348.6, 1210.21, 173.004}},
{"Rockford", {-1232.76, 1738.89, -17.071, -734.764, 2068.89, 83.754}},
{"Saint Mark's", {-284.1, 1287.68, -14.296, 38.9, 1721.68, 135.704}},
{"Belleville Park", {-1471.57, 796.93, -46.7463, -1150.73, 1386.93, 224.163}},
{"Portland Harbor", {13.6801, 730.35, -18.8643, 465.68, 1186.35, 131.136}},
{"Trenton", {-284.12, 730.15, 1.49868, 13.38, 1057.95, 151.499}},
{"Portland View", {-284.508, 1058.91, -5.78586, 38.4922, 1286.91, 126.214}},
{"Atlantic Quays", {-284.12, 548.45, -13.5049, 151.88, 730.07, 136.495}},
{"Callahan Point", {-598.32, 621.78, -13.8723, -284.32, 841.275, 136.128}},
{"Wichita Gardens", {-2161.83, 1531.93, -45.8745, -1721.04, 1892.73, 254.241}},
{"Red Light District", {-604.622, 1336.38, -22.6676, -284.62, 1517.38, 147.332}},
{"Callahan Bridge", {-732.558, 841.653, 6.26081, -284.56, 891.653, 206.261}},
{"Callahan Bridge", {-905.232, 841.702, 30.7441, -735.122, 891.702, 180.744}},
{"Bedford Point", {-1574.44, 127.95, -61.3183, -1150.44, 795.55, 432.352}},
{"Aspatria", {-1575.76, 1387.4, -8.01086e-005, -1233.76, 1960.5, 120.271}},
{"Pike Creek", {-2757.57, 1532.03, -49.6792, -2162.31, 1892.76, 250.437}},
{"Cedar Grove", {-2217.23, 1893.39, -50.1134, -1616.91, 2450.06, 250.426}},
{"Hepburn Heights", {-604.579, 1517.6, -13.4117, -284.58, 1721.23, 136.588}},
{"Liberty Campus", {-1232.73, 1388.38, 0.000190735, -1110.73, 1738.38, 166.36}},
{"Torrington", {-1150.23, 127.58, -61.7588, -772.234, 740.07, 473.656}},

// VC-Zones:
{"Leaf Links", {1786.27, -1191.43, 0.0, 2163.66, -152.395, 300.0}},
{"Escobar International", {92.1001, -1895.2, 0.0, 792.1, -1265.2, 300.0}},
{"Escobar International", {77.7263, -2752.63, 0.0, 792.246, -1895.2, 300.0}},
{"National Guard Base", {112.466, -1265.46, 0.0, 792.466, -905.464, 300.0}},
{"Ocean Beach", {1746.79, -2948.1, 0.0, 3254.9, -1848.1, 300.0}},
{"Washington Beach", {1895.18, -1849.14, 0.0, 3245.18, -1191.14, 300.0}},
{"Starfish Island", {1251.79, -1671.72, 0.0, 1895.49, -1191.72, 300.0}},
{"Viceport", {791.79, -2729.61, 0.0, 1746.79, -1848.74, 300.0}},
{"Washington Mall", {1926.84, -1984.67, 0.0, 2051.84, -1859.67, 100.0}},
{"Prawn Island", {1786.27, -152.395, 0.0, 2163.66, 293.47, 300.0}},
{"Little Haiti", {791.79, -1191.47, 0.0, 1421.71, -537.34, 300.0}},
{"Little Havana", {791.79, -1848.74, 0.0, 1251.79, -1191.47, 300.0}},
{"Junk Yard", {640.441, -992.911, 0.0, 792.441, -719.61, 250.0}},
{"Terminal", {423.404, -1892.41, 0.0, 723.404, -1705.41, 250.0}},
{"Vice Point", {2163.66, -1190.35, 0.0, 3246.03, 439.646, 300.0}},
{"Vice City Downtown", {386.97, -536.872, 0.0, 1786.27, 727.32, 300.0}},
{"Hyman Memorial Stadium", {413.973, 189.007, 0.0, 1062.48, 643.753, 290.0}},
// SHOP2", {2324.31, 27.1423, 0.0, 2484.31, 327.142, 250.0}}, VCMALL2 #North Point Mall // The mall is interior now, so this is also Vice Point
{"Vice Point", {2324.31, 27.1423, 0.0, 2484.31, 327.142, 250.0}},
{"VCBRID", {1421.38, -1191.47, 0.0, 1783.81, -537.34, 300.0}},

{"Liberty City", {-3000.0, 0.0, -100.0, 400.0, 3000.0, 800.0}},
{"Liberty City", {400.0, 2300.0, -100.0, 700.0, 3000.0, 800.0}},
{"Vice City", {400.0, -3000.0, -100.0, 3000.0, 700.0, 800.0}},
{"Vice City", {100.0, -3000.0, -100.0, 400.0, -800.0, 800.0}},

// LC-Islands:
{"Staunton Island", {-1615.48, 80.03, -114.769, -734.48, 2167.26, 485.231}},
{"Portland", {-732.849, 470.28, -117.535, 552.66, 2234.11, 482.465}},
{"Shoreside Vale", {-2994.64, 448.62, -117.0, -1616.9, 3006.35, 483.0}},
{"Shoreside Vale", {-1615.44, 1961.11, -41.7094, -1471.29, 2167.04, 358.291}},
{"Shoreside Vale", {-1615.43, 1879.09, -45.8201, -1576.33, 1961.06, 354.18}}

};

//---------------------------------------------------------------------------------------------------

/* GetPlayer2DZone(playerid, zone[], len)
 * @Info:   Used to retrieve the players zone/area name. DOES NOT COMPARE HEIGHTS!
 *
 * @params: playerid      - The id of the player whose zone you will retrieve.
 * @params: zone[]        - The string with which the zone name will be formatted into.
 * @params: len		  - The maximum length you wish to format the zone name into. RECOMMENDED 28!
 */

stock GetPlayer2DZone(playerid, zone[], len) //Credits to Cueball, Betamaster, Mabako, and Simon (for finetuning).
{
	new Float:x, Float:y, Float:z;
	GetPlayerPos(playerid, x, y, z);
 	for(new i = 0; i != sizeof(gSAZones); i++ )
 	{
		if(x >= gSAZones[i][SAZONE_AREA][0] && x <= gSAZones[i][SAZONE_AREA][3] && y >= gSAZones[i][SAZONE_AREA][1] && y <= gSAZones[i][SAZONE_AREA][4])
		{
		    return format(zone, len, gSAZones[i][SAZONE_NAME], 0);
		}
	}
	return 0;
}

//---------------------------------------------------------------------------------------------------

/* GetPlayer3DZone(playerid, zone[], len)
 * @Info:   Used to retrieve the players zone/area name and comparing with heights.
 *
 * @params: playerid      - The id of the player whose zone you will retrieve.
 * @params: zone[]        - The string with which the zone name will be formatted into.
 * @params: len		  - The maximum length you wish to format the zone name into. RECOMMENDED 28!
 */

stock GetPlayer3DZone(playerid, zone[], len) //Credits to Cueball, Betamaster, Mabako, and Simon (for finetuning).
{
	new Float:x, Float:y, Float:z;
	GetPlayerPos(playerid, x, y, z);
 	for(new i = 0; i != sizeof(gSAZones); i++ )
 	{
		if(x >= gSAZones[i][SAZONE_AREA][0] && x <= gSAZones[i][SAZONE_AREA][3] && y >= gSAZones[i][SAZONE_AREA][1] && y <= gSAZones[i][SAZONE_AREA][4] && z >= gSAZones[i][SAZONE_AREA][2] && z <= gSAZones[i][SAZONE_AREA][5])
		{
		    return format(zone, len, gSAZones[i][SAZONE_NAME], 0);
		}
	}
	return 0;
}

//---------------------------------------------------------------------------------------------------

/* IsPlayerInZone(playerid, zone[])
 * @Info:   Used to check if the player is inside the zone[] parameter.
 *
 * @params: playerid      - The id of the player whose zone you will check.
 * @params: zone[]        - The string with which we will compare the players zone name to.
 */

stock IsPlayerInZone(playerid, zone[]) //Credits to Cueball, Betamaster, Mabako, and Simon (for finetuning).
{
	new TmpZone[MAX_ZONE_NAME];
	GetPlayer3DZone(playerid, TmpZone, sizeof(TmpZone));
	for(new i = 0; i != sizeof(gSAZones); i++)
	{
		if(strfind(TmpZone, zone, true) != -1)
			return 1;
	}
	return 0;
}

