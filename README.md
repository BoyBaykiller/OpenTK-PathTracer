# C# OpenGL OpenTK Path Tracer

I am presenting a noisy, yet fully [Path Traced](https://de.wikipedia.org/wiki/Path_Tracing) renderer written in C#.  


The calculations and rendering are done in real time using OpenGl.  
I upload the whole Scene (only consisting out of Cuboids and Spheres for now) to a UBO which is then accessed in a Compute Shader where all the Path Tracing happens.
Due to the realistic nature of Path Tracers various effects like Soft Shadows, Reflections or Ambient Occlusion emerge automatically without explicitly adding code for any of these effects like you would have to do in a traditional rasterizer.

The renderer also features [Depth of Field](https://en.wikipedia.org/wiki/Depth_of_field), which can be controlled with two variables at runtime through [ImGui](https://github.com/ocornut/imgui).
`FocalLength` is the distance a object appears in focus.
`ApertureRadius` controlls how strongly objects out of focus are blured.

If a ray does not hit any object the color is retrieved from a precomputed cubemap.
The atmospheric scattering in this cubemap is calculated in yet an other Compute Shader at startup.

Screenshots taken via the screenshot feature are saved in the local execution folder `Screenshots`.

---

## **Controls**

### **KeyBoard:**
* W, A, S, D => Movment
* E => make visisble / hide mouse cursor for [ImGui](https://github.com/ocornut/imgui)
* F11 => Go into / out of fullscreen
* Esc => Close

### **Mouse:**
* LButton => Select object if cursor is visible
* LShift => Faster movment speed
* LControl => Slower movment speed

---

## **Render Samples**

![img1](https://github.com/JulianStambuk/OpenTK-PathTracer/blob/main/Screenshots/img1.png?raw=true)

![img2](https://github.com/JulianStambuk/OpenTK-PathTracer/blob/main/Screenshots/img2.png?raw=true)

![img3](https://github.com/JulianStambuk/OpenTK-PathTracer/blob/main/Screenshots/img3.png?raw=true)
