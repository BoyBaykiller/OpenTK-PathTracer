# C# OpenGL OpenTK Path Tracer

I am presenting a noisy, yet fully [Path Traced](https://de.wikipedia.org/wiki/Path_Tracing) renderer written in C#.

The calculations and rendering are done in real time using OpenGL.  
I upload the whole Scene to a UBO which is then accessed in a Compute Shader where all the Path Tracing happens.
Due to the realistic nature of Path Tracers various effects like Soft Shadows, Reflections or Ambient Occlusion emerge automatically without explicitly adding code for any of these effects like you would have to do in a traditional rasterizer.

The renderer also features [Depth of Field](https://en.wikipedia.org/wiki/Depth_of_field), which can be controlled with two variables at runtime through [ImGui](https://github.com/mellinoe/ImGui.NET).
`FocalLength` is the distance an object appears in focus.
`ApertureDiameter` controlls how strongly objects out of focus are blured.

If a ray does not hit any object the color is retrieved from a cubemap which can either be 6 images inside the `Res` folder or a precomputed skybox.
The atmospheric scattering in this skybox gets calculated in yet an other Compute Shader at startup.

Screenshots taken via the screenshot feature are saved in the local execution folder `Screenshots`.

Also see https://youtu.be/XcIToi0fh5c.

---

## **Controls**

### **KeyBoard:**
* W, A, S, D => Movment
* E => Toggle cursor visibility
* R => Reset scene
* V => Toggle VSync
* F11 => Toggle fullscreen
* LShift => Faster movment speed
* LControl => Slower movment speed
* Esc => Close

### **Mouse:**
* LButton => Select object if cursor is visible

---

## **Render Samples**

![img1](Screenshots/img1.png?raw=true)

![img2](Screenshots/img2.png?raw=true)

![img3](Screenshots/img3.png?raw=true)