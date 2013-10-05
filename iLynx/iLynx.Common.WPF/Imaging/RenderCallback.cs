using System;

namespace iLynx.Common.WPF.Imaging
{
    /// <summary>
    /// Used for rendering, the backBuffer parameter will be a pointer to the writeablebitmap's backbuffer.
    /// </summary>
    /// <param name="backBuffer">The back buffer.</param>
    /// <param name="width">The width of the backbuffer.</param>
    /// <param name="height">The height of the backbuffer.</param>
    /// <param name="stride">The stride (row length in bytes) of the backbuffer.</param>
    public delegate void RenderCallback(IntPtr backBuffer, int width, int height, int stride);
}