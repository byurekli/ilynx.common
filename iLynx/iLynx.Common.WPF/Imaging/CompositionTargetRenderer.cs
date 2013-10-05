using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using iLynx.Common.Threading;

namespace iLynx.Common.WPF.Imaging
{
    public class CompositionTargetRenderer : RendererBase, IBitmapRenderer
    {
        private readonly SortedList<int, RenderCallback> renderCallbacks = new SortedList<int, RenderCallback>();
        private readonly IntPtr backBuffer;
        private readonly int width;
        private readonly int height;
        private readonly int backBufferStride;
        private readonly AutoResetEvent frameProc;
        private readonly AutoResetEvent renderProc;
        private readonly Int32Rect dirty;
        /// <summary>
        /// Initializes a new instance of the <see cref="RendererBase" /> class.
        /// </summary>
        public CompositionTargetRenderer(int width, int height, IThreadManager threadManager)
            : base(threadManager)
        {
            if (1 > width) throw new ArgumentOutOfRangeException("width");
            if (1 > height) throw new ArgumentOutOfRangeException("height");
            bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
            backBuffer = bitmap.BackBuffer;
            backBufferStride = bitmap.BackBufferStride;
            this.width = bitmap.PixelWidth;
            this.height = bitmap.PixelHeight;
            dirty = new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);
            frameProc = new AutoResetEvent(true);
            renderProc = new AutoResetEvent(false);
            NativeMethods.MemSet(backBuffer, 0x00, bitmap.PixelHeight * bitmap.BackBufferStride);
        }

        #region Implementation of IRenderer

        private readonly WriteableBitmap bitmap;

        private void OnSourceCreated(BitmapSource source)
        {
            if (null == SourceCreated) return;
            SourceCreated(source);
        }

        /// <summary>
        /// Renders the loop.
        /// </summary>
        protected override void RenderLoop()
        {
            while (Render)
            {
                renderProc.WaitOne();
                if (!Render) return;
                Thread.CurrentThread.Join(RenderInterval);
                var cnt = renderCallbacks.Count;
                if (ClearEachPass)
                    NativeMethods.MemSet(backBuffer, 0x00, height * backBufferStride);
                while (cnt-- > 0)
                    renderCallbacks.Values[cnt](backBuffer, width, height,
                                                backBufferStride);
                frameProc.Set();
            }
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public override void Start()
        {
            base.Start();
            OnSourceCreated(bitmap);
            CompositionTarget.Rendering += CompositionTargetOnRendering;
        }

        private bool locked;
        private void CompositionTargetOnRendering(object sender, EventArgs eventArgs)
        {
            if (locked)
            {
                if (!frameProc.WaitOne(1)) return;
                bitmap.AddDirtyRect(dirty);
                bitmap.Unlock();
                locked = false;
            }
            else
            {
                bitmap.Lock();
                locked = true;
                renderProc.Set();
            }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public override void Stop()
        {
            CompositionTarget.Rendering -= CompositionTargetOnRendering;
            Render = false;
            renderProc.Set();
            base.Stop();
        }

        #endregion

        #region Implementation of IBitmapRenderer

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
        #endregion
    }
}