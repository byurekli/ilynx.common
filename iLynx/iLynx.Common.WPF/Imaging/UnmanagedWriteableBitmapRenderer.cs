using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using iLynx.Common.Threading;

namespace iLynx.Common.WPF.Imaging
{
    /// <summary>
    /// WriteableBitmapRenderer
    /// </summary>
    public class UnmanagedBitmapRenderer : RendererBase
    {
        private readonly IDispatcher dispatcher;

        /// <summary>
        /// Used for rendering, the backBuffer parameter will be a pointer to the writeablebitmap's backbuffer.
        /// </summary>
        /// <param name="backBuffer">The back buffer.</param>
        /// <param name="width">The width of the backbuffer.</param>
        /// <param name="height">The height of the backbuffer.</param>
        /// <param name="stride">The stride (row length in bytes) of the backbuffer.</param>
        public delegate void RenderCallback(IntPtr backBuffer, int width, int height, int stride);
        private readonly SortedList<int, RenderCallback> renderCallbacks = new SortedList<int, RenderCallback>();
        private readonly IRenderProxy proxy;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnmanagedBitmapRenderer" /> class.
        /// </summary>
        /// <param name="threadManager">The thread manager.</param>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <param name="pixelWidth">Width of the pixel.</param>
        /// <param name="pixelHeight">Height of the pixel.</param>
        /// <param name="stride">The stride.</param>
        public UnmanagedBitmapRenderer(IThreadManager threadManager, IDispatcher dispatcher, int pixelWidth, int pixelHeight, int stride)
            : base(threadManager)
        {
            threadManager.Guard("threadManager");
            dispatcher.Guard("dispatcher");
            this.dispatcher = dispatcher;
            proxy = new RenderProxy(pixelHeight, pixelWidth, stride);
        }

        /// <summary>
        /// Renders the loop.
        /// </summary>
        protected override void RenderLoop()
        {
            lock (proxy)
            {
                var src = CreateWriteable();
                var ptr = IntPtr.Zero;
                dispatcher.Invoke(() => ptr = src.BackBuffer);
                while (ptr == IntPtr.Zero) Thread.CurrentThread.Join(1);
                var dirty = new Int32Rect(0, 0, proxy.Width, proxy.Height);

                // Clear first pass regardless of ClearEachPass (Otherwise we'd risk giving out an image filled with random data).
                NativeMethods.MemSet(ptr, 0x00, proxy.Height * proxy.BackBufferStride);
                OnSourceCreated(src);
                while (Render)
                {
                    Thread.CurrentThread.Join(RenderInterval);
                    var cnt = renderCallbacks.Count;
                    try { dispatcher.Invoke(src.Lock); }
                    catch (TaskCanceledException) { return; }
                    if (ClearEachPass)
                        NativeMethods.MemSet(ptr, 0x00, proxy.Height * proxy.BackBufferStride);
                    while (cnt-- > 0)
                        renderCallbacks.Values[cnt](ptr, proxy.Width, proxy.Height,
                                                    proxy.BackBufferStride);
                    try
                    {
                        dispatcher.Invoke(() =>
                            {
                                src.AddDirtyRect(dirty);
                                src.Unlock();
                            });
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }
                }
            }
        }

        private WriteableBitmap CreateWriteable()
        {
            WriteableBitmap src = null;
            dispatcher.Invoke(() => { src = new WriteableBitmap(proxy.Width, proxy.Height, 96, 96, PixelFormats.Pbgra32, null); });
            while (null == src) Thread.CurrentThread.Join(1);
            return src;
        }

        private void OnSourceCreated(BitmapSource source)
        {
            if (null == SourceCreated) return;
            //source.Freeze();
            SourceCreated(source);
        }

        public event Action<BitmapSource> SourceCreated;

        /// <summary>
        /// Registers the render callback.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="tieBreaker">if set to <c>true</c> [tie breaker].</param>
        public void RegisterRenderCallback(RenderCallback callback, int priority, bool tieBreaker = false)
        {
            var prio = priority * 100;
            lock (renderCallbacks)
            {
                AdjustPriority(ref prio, tieBreaker);
                renderCallbacks.Add(prio, callback);
            }
        }

        /// <summary>
        /// Adjusts the priority.
        /// </summary>
        /// <param name="priority">The priority.</param>
        /// <param name="breaker">if set to <c>true</c> [breaker].</param>
        protected virtual void AdjustPriority(ref int priority, bool breaker)
        {
            while (renderCallbacks.ContainsKey(priority))
                priority += (breaker ? 1 : -1);
            // Invert priority to allow the renderthread to go through the callbacks in reverse order
            priority *= -1;
        }

        /// <summary>
        /// Removes the render callback.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public void RemoveRenderCallback(RenderCallback callback)
        {
            lock (renderCallbacks)
            {
                var exists = renderCallbacks.Values.Contains(callback);
                if (!exists) return;
                var kvp = renderCallbacks.FirstOrDefault(k => k.Value == callback);
                renderCallbacks.Remove(kvp.Key);
            }
        }
    }
}
