# IGME689Midterm

My midterm was to use a custom flood layer that I created in a 3D model of the city of Rochester.
I used the esri 3rd person template instead of a car to drive because I wanted the unit to switch from walking to swimming as needed. 
I found that while I could get this to work after many hours, the logic for the mesh collider for the flood elevation model does not operate the same way as a fixed elevation box collider.  
Within the full bounds of the flood elevation collider, the person follows the rules of swimming, and when they leave the flood elevation bounds, reverts to walking. 
I have spent many hours attempting to remedy this, and the only fix would negate the reason for using the custom elevation model for the flood.  
