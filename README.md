# C# OpenGL OpenTK Path Tracer

I am presenting a noisy, yet fully [Path Traced](https://de.wikipedia.org/wiki/Path_Tracing) renderer written in C#.

The calculations and rendering are done in real time using OpenGL.  
I upload the whole Scene (only consisting out of Cuboids and Spheres for now) to a UBO which is then accessed in a Compute Shader where all the Path Tracing happens.
Due to the realistic nature of Path Tracers various effects like Soft Shadows, Reflections or Ambient Occlusion emerge automatically without explicitly adding code for any of these effects like you would have to do in a traditional rasterizer.

The renderer also features [Depth of Field](https://en.wikipedia.org/wiki/Depth_of_field), which can be controlled with two variables at runtime through [ImGui](https://github.com/ocornut/imgui).
`FocalLength` is the distance an object appears in focus.
`ApertureRadius` controlls how strongly objects out of focus are blured.

If a ray does not hit any object the color is retrieved from a precomputed cubemap.
The atmospheric scattering in this cubemap is calculated in yet an other Compute Shader at startup.

Screenshots taken via the screenshot feature are saved in the local execution folder `Screenshots`.

---

## **Controls**

### **KeyBoard:**
* W, A, S, D => Movment
* E => Toggle cursor visibility
* V => Toggle VSync
* F11 => Toggle fullscreen
* LShift => Faster movment speed
* LControl => Slower movment speed
* Esc => Close

### **Mouse:**
* LButton => Select object if cursor is visible

---

## **Render Samples**

![img1](https://github.com/JulianStambuk/OpenTK-PathTracer/blob/master/Screenshots/img1.png?raw=true)

![img2](https://github.com/JulianStambuk/OpenTK-PathTracer/blob/master/Screenshots/img2.png?raw=true)

![img3](https://github.com/JulianStambuk/OpenTK-PathTracer/blob/master/Screenshots/img3.png?raw=true)


## How to contribute

Contributations can be made through the following procedure

1. Do `git clone https://github.com/JulianStambuk/OpenTK-PathTracer.git` to download the project files into your local directory

2. Type `git switch -c <new-branch>` to switch to a new local working branch. To rename a branch run `git branch -m <old> <new>`

3. Make changes to the branch and commit them with `git add .` followed by `git commit -m "Commit Message"` 

4. Do `git push --set-upstream origin <new-branch>` to finally push the new branch including your commits to the repo. It will give you a GitHub link to open a Pull Request for merging into a different branch. You can create one at any time via the GitHub page. If you don't the new branch will still continue to exist. 
