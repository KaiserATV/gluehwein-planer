# Gluehwein-planer
A simple tool that allows the planning, simulation and saving of structrures placed in an city-enviroment.

# Features
The Tool allows the placing of structures, as well as removing them. You can toggle a Simulation in which the movement 
of customers is simulated. You can move, remove and add structures as you like. 
It is possible to toggle a Heatmap, in which visualizes the utilization of a cell in different states.
The navigation of the scene is possible in two ways, either through teleporation or in the so called godmode.

# How to use
To use this projekt, you'll need to habe a few objects in the scene. Firstly you will need an empty object named "SceneManager"
with the scene manager script attached to it. Then you'll need to have a floor on which the game will occure and add this floor in the
visual setup for the scene manager. Then you'll need to have an heatmap in the scene. The heamap should also be a plane. It need the Heatmap script 
attached to it as well as the heatmap material. 
Furthermore you'll need on Container for every Bude calld "BudenContainer", in which you'll have to place all Buden in the scene. You can adjust the name 
you prefere in the scene manager. You'll also need a container for every exit, "ExitContainer" and a container for every Spawner "SpawnerContainer".
If you are using an agent model which has anything other then a "Meshrenderer" the renderer needs to be changed in the "Bude" script at the start of the function "Start".
You'll also need to add the agent model you want to use to every Bude in the editor. 
The agent itself should have the script "NPC" on it on which you can change wether or not it should use animations. Currently the animations trigger on a boolean name. If 
you have a diffrent name for the booleans, you should change it in the NPC script.
The Spawner aswell as the Exits can be abetrary Plates or cubs, but should have the corresponding script attached to it. 
The floor of the scene should be at height 0 as the current spawning position is there. 

# Current State
Currently the tool only allows the simulating of events in the centre of Leipzig. 
Also only 1 model for an structure, a point to which agents seek, is in use. 
Also the number of agents in the scene are limited to about 1800 agents due to the Navmesh plugin.

# Contact 
If you want to improve the project, add features or want to talk about it and contact the creators,
feel free to send an email to: maxmalong@duck.com
I will be happy to share our learnings and insights with you.
