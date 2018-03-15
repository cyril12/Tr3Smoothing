﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Gtk;
using SoSmooth.Meshes;
using SoSmooth.Scenes;
using Gdk;

namespace SoSmooth
{
    /// <summary>
    /// Manages the visuals of and interactions with meshes in the scene.
    /// </summary>
    public class SceneMeshes
    {
        private readonly Dictionary<Mesh, Entity> m_meshToEntity = new Dictionary<Mesh, Entity>();
        private readonly Entity m_meshesRoot;
        private readonly SceneWindow m_window;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="window">The scene window to add meshes to.</param>
        public SceneMeshes(SceneWindow window)
        {
            m_window = window;
            m_window.Update += OnUpdate;

            m_meshesRoot = new Entity(window.Scene, "MeshesRoot");
            
            MeshManager.Instance.MeshAdded += OnMeshAdded;
            MeshManager.Instance.MeshRemoved += OnMeshRemoved;
            MeshManager.Instance.SelectionChanged += OnSelectionChanged;
        }

        /// <summary>
        /// Called when a mesh is added to the scene.
        /// </summary>
        private void OnMeshAdded(Mesh mesh)
        {
            Entity entity = new Entity(m_meshesRoot, mesh.Name);
            m_meshToEntity[mesh] = entity;

            Color4 color = Random.GetColor();

            MeshRenderer r = new MeshRenderer(entity, mesh);
            r.ShaderProgram = ShaderManager.SHADER_LIT;
            r.BackFaceMode = PolygonMode.Line;
            r.Color = color;

            BoundsRenderer b = new BoundsRenderer(entity, mesh);
            b.Enabled = false;
            b.Color = color;
        }

        /// <summary>
        /// Called when a mesh is removed from the scene.
        /// </summary>
        private void OnMeshRemoved(Mesh mesh)
        {
            Entity entity;
            if (m_meshToEntity.TryGetValue(mesh, out entity))
            {
                m_meshToEntity.Remove(mesh);
                entity.Dispose();
            }
        }

        /// <summary>
        /// Called when the selected meshes have changed.
        /// Enables drawing bounding boxes around selected meshes.
        /// </summary>
        private void OnSelectionChanged(IReadOnlyList<Mesh> selected)
        {
            foreach (BoundsRenderer renderer in m_meshesRoot.GetComponentsInChildren<BoundsRenderer>())
            {
                renderer.Enabled = false;
            }
            foreach (Mesh mesh in selected)
            {
                m_meshToEntity[mesh].GetComponent<BoundsRenderer>().Enabled = true;
            }
        }

        /// <summary>
        /// Called when a mouse button was pressed.
        /// </summary>
        public void OnButtonPress(ButtonPressEventArgs args)
        {
            Vector2 mousePos = new Vector2((float)args.Event.X, (float)args.Event.Y);

            ModifierType modifierMask = Accelerator.DefaultModMask;
            bool shift = (args.Event.State & modifierMask) == ModifierType.ShiftMask;

            if (args.Event.Button == 1) // left click
            {
                // find the nearest mesh under the cursor and select it
                Camera cam = m_window.Camera.Camera;
                Vector3 rayDir = cam.ScreenPointToWorldDirection(mousePos);
                Vector3 rayOrigin = cam.Transform.Position;
                
                Mesh clicked = null;
                float minDistance = float.MaxValue;

                foreach (Mesh mesh in MeshManager.Instance.Meshes)
                {
                    Entity entity = m_meshToEntity[mesh];
                    
                    Vertex[] verts = mesh.Vertices;
                    Triangle[] tris = mesh.Triangles;

                    Matrix4 toLocal = entity.Transform.WorldToLocalMatrix;
                    Matrix4 toWorld = entity.Transform.LocalToWorldMatix;

                    Vector3 dir = Vector3.TransformVector(rayDir, toLocal);
                    Vector3 orig = Vector3.TransformPosition(rayOrigin, toLocal);
                    
                    for (int i = 0; i < tris.Length; i++)
                    {
                        Vector3 localIntersect;

                        if (Utils.RaycastTriangle(
                            verts[tris[i].index0].position,
                            verts[tris[i].index1].position,
                            verts[tris[i].index2].position,
                            orig, dir, out localIntersect))
                        {
                            Vector3 intersect = Vector3.TransformPosition(localIntersect, toWorld);
                            float dist = (intersect - rayOrigin).Length;

                            if (dist < minDistance)
                            {
                                minDistance = dist;
                                clicked = mesh;
                            }
                        }
                    }
                }
                MeshManager.Instance.SetActiveMesh(clicked, !shift);
            }
        }

        /// <summary>
        /// Gets the entity for a mesh in this scene.
        /// </summary>
        /// <param name="mesh">A mesh that is in the scene.</param>
        /// <returns>The entity corresponding to the mesh.</returns>
        public Entity GetEntity(Mesh mesh)
        {
            return m_meshToEntity[mesh];
        }

        /// <summary>
        /// Called prior to rendering.
        /// </summary>
        private void OnUpdate()
        {
        }
    }
}