using System;

namespace iLynx.Common.WPF.Imaging
{
    public static class Extensions
    {
        public static unsafe void DrawLine(this IntPtr target,
                                           int width,
                                           int height,
                                           int x1,
                                           int y1,
                                           int x2,
                                           int y2,
                                           int colour)
        {
            // Use refs for faster access (really important!) speeds up a lot!
            var w = width;
            var h = height;
            var pixels = (int*) target;

            // Distance start and end point
            var dx = x2 - x1;
            var dy = y2 - y1;

            // Determine sign for direction x
            var incx = 0;
            if (dx < 0)
            {
                dx = -dx;
                incx = -1;
            }
            else if (dx > 0)
            {
                incx = 1;
            }

            // Determine sign for direction y
            var incy = 0;
            if (dy < 0)
            {
                dy = -dy;
                incy = -1;
            }
            else if (dy > 0)
            {
                incy = 1;
            }

            // Which gradient is larger
            int pdx, pdy, odx, ody, es, el;
            if (dx > dy)
            {
                pdx = incx;
                pdy = 0;
                odx = incx;
                ody = incy;
                es = dy;
                el = dx;
            }
            else
            {
                pdx = 0;
                pdy = incy;
                odx = incx;
                ody = incy;
                es = dx;
                el = dy;
            }

            // Init start
            var x = x1;
            var y = y1;
            var error = el >> 1;
            if (y < h && y >= 0 && x < w && x >= 0)
            {
                pixels[y*w + x] = colour;
            }

            // Walk the line!
            for (var i = 0; i < el; i++)
            {
                // Update error term
                error -= es;

                // Decide which coord to use
                if (error < 0)
                {
                    error += el;
                    x += odx;
                    y += ody;
                }
                else
                {
                    x += pdx;
                    y += pdy;
                }

                // Set pixel
                if (y < h && y >= 0 && x < w && x >= 0)
                {
                    pixels[y*w + x] = colour;
                }
            }
        }
    }
}