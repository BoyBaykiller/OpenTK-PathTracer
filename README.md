# C# OpenGL OpenTK Path Tracer

---

This is a working fork of [this repo main branch](https://github.com/JulianStambuk/OpenTK-PathTracer/tree/main) with some differences:
- It doesn't depend on ARB_bindless_texture, ARB_direct_state_access, GL_ARB_seamless_cubemap_per_texture and some other extensions used in the mentioned repo.
- Compatible with OpenGL 4.3 and earlier.
- ImGui not implemented yet.
- Minor changes.

---

[Path Traced](https://en.wikipedia.org/wiki/Path_Tracing) renderer written in C#.

The calculations and rendering are done in real time using OpenGL. 
The whole Scene (only consisting out of Cuboids and Spheres for now) is loaded to a UBO which is then accessed in a Compute Shader where the Path Tracing is done.
Due to the realistic nature of Path Tracers various effects like Soft Shadows, Reflections or Ambient Occlusion emerge automatically without explicitly adding code for any of these effects like you would have to do in a traditional rasterizer.

If a ray does not hit any object the color is retrieved from a precomputed cubemap.
The atmospheric scattering in this cubemap is calculated in yet an other Compute Shader at startup.

---

## **Controls**

### **KeyBoard:**
* E => Toggle cursor visibility.
* F11 => Toggle fullscreen.
* V => Toggle VSync.
* Esc => Close.

* W, A, S, D => Movement.
* LShift => Faster movement speed
* LControl => Slower movement speed

---

## **Render Samples**

![img1](https://github.com/JulianStambuk/OpenTK-PathTracer/blob/main/Screenshots/img1.png?raw=true)

![img2](https://github.com/JulianStambuk/OpenTK-PathTracer/blob/main/Screenshots/img2.png?raw=true)

![img3](https://github.com/JulianStambuk/OpenTK-PathTracer/blob/main/Screenshots/img3.png?raw=true)
