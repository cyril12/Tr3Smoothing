using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace SoSmooth.Rendering
{
    /// <summary>
    /// A surface that can render any number of vertices
    /// by automatically utilizing multiple <see cref="VertexBuffer{TVertexData}"/> if needed.
    /// </summary>
    /// <typeparam name="TVertexData">The <see cref="IVertexData" /> used.</typeparam>
    public class ExpandingVertexSurface<TVertexData> : Surface where TVertexData : struct, IVertexData
    {
        private readonly List<VertexBuffer<TVertexData>> m_vertexBuffers;
        private readonly List<VertexArray<TVertexData>> m_vertexArrays;

        private readonly PrimitiveType m_primitiveType;

        private int m_activeBufferIndex;
        private VertexBuffer<TVertexData> m_activeVertexBuffer;

        /// <summary>
        /// Wether to clear vertex buffer after drawing.
        /// </summary>
        public bool ClearOnRender { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpandingVertexSurface{TVertexData}"/> class.
        /// </summary>
        /// <param name="primitiveType">Type of the primitives to draw</param>
        public ExpandingVertexSurface(PrimitiveType primitiveType = PrimitiveType.Triangles)
        {
            m_primitiveType = primitiveType;
            m_vertexBuffers = new List<VertexBuffer<TVertexData>>
                { (m_activeVertexBuffer = new VertexBuffer<TVertexData>()) };

            m_vertexArrays = new List<VertexArray<TVertexData>>
                { new VertexArray<TVertexData>(m_activeVertexBuffer) };

            ClearOnRender = true;
        }

        /// <summary>
        /// Handles setting up (new) shader program with this surface.
        /// </summary>
        protected override void OnNewShaderProgram()
        {
            foreach (var vertexArray in m_vertexArrays)
            {
                vertexArray.SetShaderProgram(Program);
            }
        }

        /// <summary>
        /// Whether the surface has any vertices to render.
        /// If this is false, calling <see cref="Surface.Render"/> has no visual effect.
        /// </summary>
        public bool HasVerticesToRender
        {
            get { return m_vertexBuffers[0].Count > 0; }
        }

        protected override void OnRender()
        {
            if (m_vertexBuffers[0].Count == 0)
            {
                return;
            }

            for (int i = 0; i < m_vertexBuffers.Count; i++)
            {
                var vertexBuffer = m_vertexBuffers[i];
                if (vertexBuffer.Count == 0)
                {
                    break;
                }

                var vertexArray = m_vertexArrays[i];

                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);

                vertexArray.SetVertexData();
                vertexBuffer.BufferData();

                GL.DrawArrays(m_primitiveType, 0, vertexBuffer.Count);

                vertexArray.UnSetVertexData();

            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            if (ClearOnRender)
            {
                Clear();
            }
        }

        /// <summary>
        /// Clears the surface of all vertices.
        /// </summary>
        public void Clear()
        {
            m_activeBufferIndex = 0;
            m_activeVertexBuffer = m_vertexBuffers[0];

            foreach (var vertexBuffer in m_vertexBuffers)
            {
                vertexBuffer.Clear();
            }
        }
        
        /// <summary>
        /// Adds a vertex to be drawn.
        /// </summary>
        public void AddVertex(TVertexData vertex)
        {
            MakeBufferSpaceFor(1);
            m_activeVertexBuffer.AddVertex(vertex);
        }

        /// <summary>
        /// Adds two vertices to be drawn.
        /// </summary>
        public void AddVertices(TVertexData v0, TVertexData v1)
        {
            MakeBufferSpaceFor(2);
            m_activeVertexBuffer.AddVertices(v0, v1);
        }

        /// <summary>
        /// Adds three vertices to be drawn.
        /// </summary>
        public void AddVertices(TVertexData v0, TVertexData v1, TVertexData v2)
        {
            MakeBufferSpaceFor(3);
            m_activeVertexBuffer.AddVertices(v0, v1, v2);
        }

        /// <summary>
        /// Adds four vertices to be drawn.
        /// </summary>
        public void AddVertices(TVertexData v0, TVertexData v1, TVertexData v2, TVertexData v3)
        {
            MakeBufferSpaceFor(4);
            m_activeVertexBuffer.AddVertices(v0, v1, v2, v3);
        }

        /// <summary>
        /// Adds vertices to be drawn.
        /// </summary>
        public void AddVertices(params TVertexData[] vertices)
        {
            MakeBufferSpaceFor(vertices.Length);
            m_activeVertexBuffer.AddVertices(vertices);
        }

        /// <summary>
        /// Exposes the underlying array of the vertex buffer directly,
        /// to allow for faster vertex creation.
        /// </summary>
        /// <param name="count">The amount of vertices to write.
        /// The returned array is guaranteed to have this much space.
        /// Do not write more than this number of vertices.
        /// Note also that writing less vertices than specified may result in undefined behaviour.</param>
        /// <param name="offset">The offset of the first vertex to write to.</param>
        /// <remarks>
        /// <para>Make sure to write vertices to the array in the full range [offset, offset + count[.
        /// Writing more or less or outside that range may result in undefined behaviour.</para>
        /// <para>Do not write more than 2^16 vertices at the same time this way.
        /// Doing so may result in undefined behaviour.</para>
        /// </remarks>
        /// <returns>The underlying array of vertices to write to. This array is only valid for this single call.
        /// To copy more vertices, call this method again and use the new return value.</returns>
        public TVertexData[] WriteVerticesDirectly(int count, out ushort offset)
        {
            MakeBufferSpaceFor(count);
            return m_activeVertexBuffer.WriteVerticesDirectly(count, out offset);
        }

        /// <summary>
        /// Makes sure the active vertex buffer can handle the given number of new vertices
        /// or switches to/creates a new one of needed.
        /// </summary>
        /// <remarks>If more than 2^16 vertices are added at the same time, this will still result in
        /// undefined behaviour.</remarks>
        private void MakeBufferSpaceFor(int i)
        {
            ushort c = m_activeVertexBuffer.Count;
            if (c + i <= ushort.MaxValue + 1 || c == 0)
            {
                return;
            }

            m_activeBufferIndex++;

            if (m_vertexBuffers.Count > m_activeBufferIndex)
            {
                m_activeVertexBuffer = m_vertexBuffers[m_activeBufferIndex];
                return;
            }

            m_vertexBuffers.Add(m_activeVertexBuffer = new VertexBuffer<TVertexData>(i));
            m_vertexArrays.Add(new VertexArray<TVertexData>(m_activeVertexBuffer));
        }
    }
}
