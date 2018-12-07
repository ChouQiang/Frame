Version：1.1

Update Content:
Added the function of action blend.
Users can choose whether to use blend or not. Blend will increase the performance consumption, but it can greatly smooth the motion switching process. 
For details, see Scene Animation Blend.

===========================================================================================

Version：1.0

Quick Start
Click “GpuInstancingAnim->AnimMapBaker” to open the window for baking animation map. 

Configure the desired parameter value in the window. Including: 

Target Prefab – The prefab we need bake.
Target Animation FBX – The animation file to be baked (if the animation file and the model are not separated, it is the model file).
RootBone GameObject – The root skeleton of the model. If the model has multiple root bones, then just put the root of the model here.
Output path – File output path, default and prefab a directory.
After the configuration is complete, click bake and we can generate an animated map.

See Doc/GpuInstancing Animation.pdf for more detail.

You can get more by looking at the video address below:
https://www.youtube.com/watch?v=JwrsT0wDBaY&feature=youtu.be