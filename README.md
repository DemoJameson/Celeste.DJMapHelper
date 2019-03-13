# Usage
1. **Put the .zip in the Mods folder. If you're just playing with other maps that require this Helper, do only this.**

2. **If you want to include it in your map, add a dependency to the DJMapHelper like so:**

    ~~~yaml
    - Name: YourModName
      Version: 1.0.0 (Your mod's version)
      Dependencies:
        - Name: Everest
          Version: 1.808.0 (Everest version your mod runs on, at least 808)
        - Name: DJMapHelper
          Version: 1.0.0
    ~~~

3. **Restart Ahorn for the entities and triggers list to update**

    - Colorful Feather
    - Colorful Feather Barrier - Only go through if you're flying in the same color
    - Colorful Refill: 
        - black: +dash -stamina or -dash +stamina. If no stamina and no dash, die.
        - blue: only +stamina
        - red: only +dash
    - FinalBossReversed - Badeline boss which won't lock camera.
    - Start Level Spawn Point - Used in cases where the start room has multiple spawn points. Search for a spawn point near this entity when the chapter begins.
    - Play Sprite Entity - Play the sprite in Sprites.xml (Madeline's hair is not yet supported).
    - Temple Gate Reversed - Used for going from right to left.
    - Theo Crystal Barrier - Like Seeker Barrier except for theo.  
    - Climb Blocker Trigger - When the player is in the trigger completely banning interaction with walls means you can't wall jump and climb
    - Color Grade Trigger - Change the color grade.
    - Kill Box Trigger - Die instantly when enters.
    - MaxDashes Trigger - Change the max dashes.
    - Snowballs Left Trigger - SnowBalls going to the right.
    - Teleport Trigger - Teleport player to assigned room.

# Thanks
- Artain - Provide pictures of the feathers.
- [Exudias](https://gamebanana.com/members/1651705) - His mods taught me a lot about writing new entities ~~and stolen this README from him~~.
- Hyper ツ - Source of the idea for feather barrier.
- [meiyou](https://gamebanana.com/members/1650353) - Source of the idea for climb blocker trigger.
- [HigashiD](https://gamebanana.com/members/1661237) - Coop-Coder.
