﻿using System;
using System.Collections.Generic;
using System.Linq;
using SoSmooth.Meshes;

namespace SoSmooth
{
    /// <summary>
    /// Manages the meshes loaded in the scene.
    /// </summary>
    public class MeshManager : Singleton<MeshManager>
    {
        public delegate void MeshesAddedHandler(IEnumerable<MeshInfo> meshes);
        public delegate void MeshRemovedHandler(MeshInfo mesh);
        public delegate void MeshesChangedHandler(IEnumerable<MeshInfo> meshes);

        /// <summary>
        /// Triggered after a single mesh has been added.
        /// </summary>
        public event MeshesAddedHandler MeshesAdded;

        /// <summary>
        /// Triggered after a single mesh has been removed.
        /// </summary>
        public event MeshRemovedHandler MeshRemoved;

        /// <summary>
        /// Triggered after meshes have been added or removed.
        /// </summary>
        public event MeshesChangedHandler MeshesChanged;

        private readonly List<MeshInfo> m_meshes = new List<MeshInfo>();
        
        /// <summary>
        /// The loaded meshes.
        /// </summary>
        public List<MeshInfo> Meshes => new List<MeshInfo>(m_meshes);

        /// <summary>
        /// The selected meshes.
        /// </summary>
        public List<MeshInfo> SelectedMeshes => m_meshes.Where(m => m.IsSelected).ToList();

        /// <summary>
        /// The visible meshes.
        /// </summary>
        public List<MeshInfo> VisibleMeshes => m_meshes.Where(m => m.IsVisible).ToList();

        /// <summary>
        /// The active mesh.
        /// </summary>
        public MeshInfo ActiveMesh => m_meshes.FirstOrDefault(m => m.IsActive);

        /// <summary>
        /// Adds meshes to the loaded mesh list.
        /// </summary>
        /// <param name="meshes">The meshes to add.</param>
        public void AddMeshes(IEnumerable<Mesh> meshes)
        {
            m_meshes.ForEach(m => m.IsSelected = false);

            List<MeshInfo> newMeshes = new List<MeshInfo>();
            foreach (Mesh mesh in meshes)
            {
                newMeshes.Add(new MeshInfo(mesh));
            }
            MeshesAdded?.Invoke(newMeshes);

            m_meshes.AddRange(newMeshes);
            MeshesChanged?.Invoke(m_meshes);
        }

        /// <summary>
        /// Removes a mesh from the loaded mesh list.
        /// </summary>
        /// <param name="meshes">The meshes to remove.</param>
        public void RemoveMeshes(IEnumerable<MeshInfo> meshes)
        {
            // remove meshes from selection
            foreach (MeshInfo mesh in meshes)
            {
                mesh.IsActive = false;
                mesh.IsSelected = false;
                m_meshes.Remove(mesh);
            }
            
            // notify that there are new meshes
            MeshesChanged?.Invoke(m_meshes);

            // notify each mesh's removal
            foreach (MeshInfo mesh in meshes)
            {
                MeshRemoved?.Invoke(mesh);
                // dispose old meshes to prevent memory leak of unmanaged mesh data
                mesh.Mesh.Dispose();
            }
        }

        /// <summary>
        /// Removes all selected meshes.
        /// </summary>
        public void DeleteSelected()
        {
            RemoveMeshes(SelectedMeshes);
        }

        /// <summary>
        /// Makes every mesh visible.
        /// </summary>
        public void ShowAll()
        {
            ModifyMeshInfosOperation op = new ModifyMeshInfosOperation();

            m_meshes.ForEach(m => m.IsSelected = false);
            foreach (MeshInfo mesh in m_meshes.Where(m => !m.IsVisible))
            {
                mesh.IsVisible = true;
                mesh.IsSelected = true;
            }

            op.Complete();
        }

        /// <summary>
        /// Makes every mesh visible.
        /// </summary>
        public void HideSelected()
        {
            ModifyMeshInfosOperation op = new ModifyMeshInfosOperation();

            foreach (MeshInfo mesh in m_meshes)
            {
                if (mesh.IsSelected)
                {
                    mesh.IsVisible = false;
                }
            }

            op.Complete();
        }

        /// <summary>
        /// Delects all meshes if any are selected and selects all meshes is none are selected.
        /// </summary>
        public void ToggleSelected()
        {
            ModifyMeshInfosOperation op = new ModifyMeshInfosOperation();

            if (m_meshes.Any(m => m.IsSelected))
            {
                m_meshes.ForEach(m => m.IsSelected = false);
            }
            else
            {
                m_meshes.ForEach(m => m.IsSelected = m.IsVisible);
            }

            op.Complete();
        }
    }
}
