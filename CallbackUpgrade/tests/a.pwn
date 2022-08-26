#if !defined PLAYER_STATE
	#define PLAYER_STATE: _:
#endif
public OnPlayerStateChange(playerid, PLAYER_STATE:newstate, PLAYER_STATE:oldstate)
{
	return My_OnPlayerStateChange(playerid, newstate, oldstate);
}

#if !defined PLAYER_STATE
	#define PLAYER_STATE: _:
#endif
forward My_OnPlayerStateChange(playerid, PLAYER_STATE:newstate, PLAYER_STATE:oldstate);

