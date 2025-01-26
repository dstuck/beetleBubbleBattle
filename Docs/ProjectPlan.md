# Project Plan

v0.1 character movement
- [x] Control scheme
- [x] Charging burst
- [x] Tapping bops
- [x] Size change
v0.2 environment 
- [x] Bounce off walls
- [x] Pop on spikes
- [x] Bounce into other bubble
v0.3 multiplayer
- [x] Other players can spawn in by pressing start
- [ ] Player 2 starts out as a bot that just does basic movement and doesn’t die
v0.4 Items
- [x] Create item blocks
- [ ] Power charge - charge up fast
- [ ] Shield - no knockback, no spikes
- [ ] ice - other bubbles lose drag and decreased bursts
- [ ] Sliding - joystick actually moves you
v0.5 level design
- [x] Overall layout of walls and spikes
- [x] Item spawn points
- [ ] Maybe falling rocks
- [ ] Maybe floating bubbles
- [x] Player spawn points
v0.7 Game management 
- [ ] Add win condition and scene
- [ ] Possibly add lives
- [ ] Respawn with short shield
v0.8 menu
- [ ] Add player select scene. Set number players and let people hit button to join
v1.1 Character select
- [ ] Rhino beetle - high weight
- [ ] Ant - high bop
- [ ] Lady bug - high charge


# Controls
Joystick and one button
- support arrow keys and space for laptop testing

#Core gameplay
Players inflate and deflate their bubble, trading off the risk of exposure by being a larger target with maybe less control for being a more agile character but more at risk of being pushed around. Inflating and deflating are controlled by the core movement of the game, with the charged bursts increasing your size and short bops decreasing it.

Core stats are charge speed, burst baseline, and base weight. As you charge you build up size which increases weight as well as single use burst.

As the button is held, the bubble will grow in size and a burst will build up. When released the bubble will blast off in that direction losing a small amount of size. There will be a small baseline burst though so tapping the button will allow consistent small movement

Movement should be force based so that players float across the screen and continue moving until slowed down or redirected. Big bubbles should be more floaty with less drag while little bubbles should feel more like they’re hopping around

When a player is hit with a burst they should have a short span where their own burst is disabled. Battling should be context dependent rather than just jamming into each other. Trying to position yourself away from spikes, trying to get items first

Maybe there are coins or something to collect that

# Level design
Items should spawn in risky locations like near spikes or in tight quarters


Control ideas:
introduce a hit state so that you can modify drag and potentially disable charging

Add arrow showing direction