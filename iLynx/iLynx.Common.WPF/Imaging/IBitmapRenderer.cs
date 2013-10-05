using System;
using System.Windows.Media.Imaging;

namespace iLynx.Common.WPF.Imaging
{
    public interface IBitmapRenderer : IRenderer
    {
        event Action<BitmapSource> SourceCreated;

        /// <summary>
        /// Registers the render callback.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="tieBreaker">if set to <c>true</c> [tie breaker].</param>
        void RegisterRenderCallback(RenderCallback callback, int priority, bool tieBreaker = false);

        /// <summary>
        /// Removes the render callback.
        /// </summary>
        /// <param name="callback">The callback.</param>
        void RemoveRenderCallback(RenderCallback callback);
    }
}