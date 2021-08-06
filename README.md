# C# OpenGL OpenTK Path Tracer

I am presenting a noisy, yet fully [Path Traced](https://de.wikipedia.org/wiki/Path_Tracing) renderer written in C#.  


The calculations and rendering are done in real-time using OpenGL.  
I upload the whole Scene (only consisting out of Cuboids and Spheres for now) to a UBO which is then accessed in a Compute Shader where all the Path Tracing happens.
Due to the realistic nature of Path Tracers - tracing a ray over many bounces - various effects like soft shadows, reflections or ambient occlusion emerge automatically, without explicitly adding any code like you would have to do in a rasterizer.

If a ray does not hit any objects the color is retrieved from a precomputed cubemap.
The atmospheric scattering in this cubemap is calculated in yet an other Compute Shader at startup.


## **Controls**

### **KeyBoard:**
* W, A, S, D => Movment
* E => make visisble / hide mouse cursor for ImGUI
* F11 => Go into / out of fullscreen
* Esc => Close

### **Mouse:**
* LButton => Select Object


## **Render Samples**

![img1](https://github.com/JulianStambuk/OpenTK-PathTracer/blob/main/Screenshots/img1.png?raw=true)

![img2](https://github.com/JulianStambuk/OpenTK-PathTracer/blob/main/Screenshots/img2.png?raw=true)

![img3](https://github.com/JulianStambuk/OpenTK-PathTracer/blob/main/Screenshots/img3.png?raw=true)
