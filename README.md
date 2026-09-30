unity game project Iconoclast/Kastic from 2026 (abandoned due to overambitious design). 2d melee fighting game, full physics body with psuedo-active ragdoll
would fight enemies with customisable, physics based weapons with the ability to knock weapons out of an enemies hands or throw them off balance.

requirements :
  - OS that can run unity editor.
  - unity installed on machine to run the editor.
  - a unity version to run the project in.

install instructions :
    - download repository and unzip.
    - open unity hub and click add then add project from disk.
    - navigate and select unzipped repository.
    - you will be prompted for a version to open it in, selecting missing version or latest LTS version will work best.
    - wait for editor to load and enjoy.

gameplay notes :

as in AGILE the player can move left and right(AD) and sprint(shift).

the player could equip weapons to their left and right hand(QE) with 2 handed weapons taking up both slots, players could also de-equip weapons(QE) and could kick(R).

by players could point the weapon equipped to their left and right hand(LMB/RMB) this would allow the player to swing the weapon by moveing their mouse,
furthermore if a weapon was used in 2 hands it would swap which end would point to the mouses position.

the player could also flip their weapon along its axis to utilise different aspects of its geomotry(ZX) and could project the weapon forward to block(space).

overall this gave the player the technical capacity to do many things with the weapon at the cost of their being too many controls.

controller : 
  - the body used in the game is the same as AGILE only retooled for melee weapons and made simpler to impliment by compartmentalising each limb into a
    self governing system. the legs step through the world adgusting the hight of the body based on the enviroment, the body can rotate freely and seeks to
    right itself, if the body is knocked too far off centre it will enter a ragdoll state. each limb can individually ragdoll and de-ragdoll to fit gameplay
    needs.

  - as in AGILE the user has a number of weapons to equip, these sit at specific positions on the players body, when equipped they can be swung around to
    attack enemies, if the weapon strays too far from the hand then the hand lets go allowing for a strong strike to disarm an opponent.

there were future plans of the player going from level to level around a map killing enemies to gain resources, using these they could upgrade their stats,
make new weapons out of different materials and create spells, the spell editor would have been dynamic with players placing and connecting different magic
nodes, depending on the connection, spacing and alignment of the nodes the spells would gain different effects/costs.

please direct all inquiries, questions and problems to ruled.dev@gmail.com
