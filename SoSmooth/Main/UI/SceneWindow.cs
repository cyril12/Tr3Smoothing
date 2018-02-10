﻿using System;
using Gtk;
using GLib;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SoSmooth
{
    /// <summary>
    /// Manages the 3D scene view in the application
    /// </summary>
    public class SceneWindow : IDisposable
    {
        private const int OPENGL_VERSION_MAJOR = 3;
        private const int OPENGL_VERSION_MINOR = 0;

        private const int DEPTH_BUFFER_SIZE     = 24;
        private const int AA_SAMPLES            = 4;

        private int m_glWidth;
        private int m_glHeight;
        private bool m_viewportDirty;

        private GLWidget m_glWidget;
        public Widget Widget { get { return m_glWidget; } }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public SceneWindow()
        {
            GraphicsMode mode = new GraphicsMode(DisplayDevice.Default.BitsPerPixel, DEPTH_BUFFER_SIZE, 0, AA_SAMPLES, 0, 0, false);
            m_glWidget = new GLWidget(mode, OPENGL_VERSION_MAJOR, OPENGL_VERSION_MINOR, GraphicsContextFlags.ForwardCompatible);
            m_glWidget.Name = "glWindow";
            m_glWidget.Initialized += GLWidgetInitialize;
            m_glWidget.SizeAllocated += OnResize;

            m_viewportDirty = true;
            
            m_glWidget.KeyPressEvent += KeyPressEvent;
            m_glWidget.KeyReleaseEvent += KeyReleaseEvent;
        }

        [ConnectBefore]
        private void KeyPressEvent(object o, Gtk.KeyPressEventArgs args)
        {
            Logger.Debug(args.Event.Key.ToString());
        }

        [ConnectBefore]
        private void KeyReleaseEvent(object o, Gtk.KeyReleaseEventArgs args)
        {
            Logger.Debug(args.Event.Key.ToString());
        }

        /// <summary>
        /// Frees managed resources.
        /// </summary>
        public void Dispose()
        {
            if (m_glWidget != null)
            {
                m_glWidget.Dispose();
                m_glWidget = null;
            }
        }

        /// <summary>
        /// Called when the context is initialized.
        /// </summary>
        private void GLWidgetInitialize(object sender, EventArgs e)
        {
            GL.ClearColor(Color4.DarkSlateBlue);
            GL.Enable(EnableCap.DepthTest);

            // Add idle event handler to process rendering whenever and as long as time is availble.
            Idle.Add(new IdleHandler(OnIdleProcessMain));
        }

        /// <summary>
        /// Called when the widget has been resized. We get the new size and mark that we need
        /// to update the window size next time the scene is rendered.
        /// </summary>
        private void OnResize(object o, SizeAllocatedArgs args)
        {
            m_glWidth = args.Allocation.Width;
            m_glHeight = args.Allocation.Height;
            m_viewportDirty = true;
        }

        /// <summary>
        /// Render the scene whenever the main applicatoin is not busy.
        /// </summary>
        private bool OnIdleProcessMain()
        {
            if (m_glWidget == null)
            {
                return false;
            }
            else
            {
                RenderFrame();
                return true;
            }
        }

        private Scene.Scene m_scene;
        private Scene.Camera m_camera;
        private Scene.Entity m_camChild;
        private Scene.Entity m_entity1;
        private Scene.Entity m_entity2;

        /// <summary>
        /// Renders a frame.
        /// </summary>
        private void RenderFrame()
        {
            Time.FrameStart();

            if (m_scene == null)
            {
                m_scene = new Scene.Scene();

                Scene.Entity cam = new Scene.Entity(m_scene, "Camera");
                m_camera = new Scene.Camera(cam);
                m_camera.Transform.LocalPosition = new Vector3(0, -5, 0);
                m_camera.Transform.LocalRotation = Quaternion.FromEulerAngles(0, 0, MathHelper.PiOver2);

                m_camChild = new Scene.Entity(m_scene, "CamChildCube");
                m_camChild.Transform.LocalPosition = new Vector3(0, 0, -8);
                m_camChild.Transform.SetParent(cam.Transform, false);
                Logger.Debug(m_camChild.Transform.LocalToWorldMatix);
                Scene.MeshRenderer camChildRenderer = new Scene.MeshRenderer(m_camChild);
                camChildRenderer.SetMesh(Renderer.Meshes.Mesh<Renderer.Meshes.VertexNC>.CreateCube());
                camChildRenderer.SetProgram(Renderer.ShaderManager.Instance.GetProgram("unlit"));

                m_entity1 = new Scene.Entity(m_scene, "Cube");
                Scene.MeshRenderer renderer = new Scene.MeshRenderer(m_entity1);
                renderer.SetMesh(Renderer.Meshes.Mesh<Renderer.Meshes.VertexNC>.CreateDirectionThing());
                renderer.SetProgram(Renderer.ShaderManager.Instance.GetProgram("unlit"));

                m_entity2 = new Scene.Entity(m_scene, "Cube2");
                m_entity2.Transform.Parent = m_entity1.Transform;
                m_entity2.Transform.LocalPosition = new Vector3(-1, 1, 1);
                Scene.MeshRenderer renderer2 = new Scene.MeshRenderer(m_entity2);
                renderer2.SetMesh(Renderer.Meshes.Mesh<Renderer.Meshes.VertexNC>.CreateDirectionThing());
                renderer2.SetProgram(Renderer.ShaderManager.Instance.GetProgram("unlit"));

                Logger.Debug(m_entity1.GetComponentsInChildren<Scene.Transform>().Count);
            }

            // Resize the view if it has been changed.
            if (m_viewportDirty)
            {
                ResizeGLContext();
            }
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //m_camera.Transform.LocalPosition = new Vector3(0, -5 + (float)Math.Sin(Time.time), 0);
            //m_camera.Transform.LocalRotation = Quaternion.FromEulerAngles(0, 0, MathHelper.PiOver2 * (float)Math.Sin(Time.time));

            //m_entity1.Transform.LocalScale = new Vector3((float)Math.Sin(Time.time) + 2, 1, 1);
            m_entity1.Transform.LocalPosition = new Vector3((float)Math.Sin(Time.time), 0, 0);
            m_entity1.Transform.LocalRotation = Quaternion.FromAxisAngle(new Vector3(1, 1, 1), Time.time);

            m_entity2.Transform.LocalScale = new Vector3((float)Math.Sin(Time.time) + 2, 1, 1);
            m_entity2.Transform.LocalRotation = Quaternion.FromAxisAngle(new Vector3(1, 1, 1), Time.time).Inverted();

            m_camera.Transform.SetParent((Time.time % 3 > 1.5f) ? m_entity1.Transform : null, true);

            m_entity1.GetComponent<Scene.MeshRenderer>().Render(m_camera);
            m_entity2.GetComponent<Scene.MeshRenderer>().Render(m_camera);
            m_camChild.GetComponent<Scene.MeshRenderer>().Render(m_camera);

            Logger.Debug(m_camChild.Transform.Root);

            GraphicsContext.CurrentContext.SwapBuffers();
        }

        /// <summary>
        /// Handles resizing the framebuffers.
        /// </summary>
        private void ResizeGLContext()
        {
            GL.Viewport(0, 0, m_glWidth, m_glHeight);

            float aspectRatio = ((float)m_glWidth) / m_glHeight;
            
            m_viewportDirty = false;
        }
    }
}
