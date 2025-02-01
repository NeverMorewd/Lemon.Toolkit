using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.GLControl;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering;
using System;
namespace Lemon.Toolkit.Controls;
public class OpenGLControl : Control
{
    private GLControl _glControl;
    private int _vao, _vbo;

    public OpenGLControl()
    {
        _glControl = new GLControl(GLControlSettings.Default);
        _glControl.Load += OnLoad;
        _glControl.Paint += OnRender;
        _glControl.Resize += OnResize;
    }

    private void OnLoad(object sender, EventArgs e)
    {
        // OpenGL 初始化
        GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        CreateSphere();
    }

    private void CreateSphere()
    {
        // 使用 OpenGL 创建球体的顶点数据
        var vertices = new float[]
        {
            1,2,3,4,5,6
        };

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
    }

    private void OnRender(object sender, EventArgs e)
    {
        // 渲染循环
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 36); // 假设我们有36个顶点

        _glControl.SwapBuffers();
    }

    private void OnResize(object sender, EventArgs e)
    {
        GL.Viewport(0, 0, _glControl.Width, _glControl.Height);
    }
}
