using System;
using System.Collections.Generic;
using System.Text;

namespace Image
{
    public static class RidgeDetector
    {
        public static IImage<double> Detect(IImage<double> imageLx, IImage<double> imageLy, IImage<double> imageLxx, IImage<double> imageLyy, IImage<double> imageLxy)
        {
            // Calculate the orientation of the local (p, q) co-ordinate system
            // aligned with the principle curvature directions
            IImage<double> imageBeta = PrincipleCurvatureDirection(imageLxx, imageLyy, imageLxy);

            // TODO: Remove this from the release build
            // Compute the cross derivative in the local co-ordinate system should be zero everywhere
            IImage<double> imageLpq = LocalLpq(imageLxx, imageLyy, imageLxy, imageBeta);

            // Compute the gradient in the local co-ordinate system
            IImage<double> imageLp = LocalLp(imageLx, imageLy, imageBeta);
            IImage<double> imageLq = LocalLq(imageLx, imageLy, imageBeta);

            IImage<double> imageLpp = LocalLpp(imageLxx, imageLyy, imageLxy, imageBeta);
            IImage<double> imageLqq = LocalLqq(imageLxx, imageLyy, imageLxy, imageBeta);

            IImage<double> imageRidge = (IImage<double>) imageLp.Clone();
            imageRidge.Clear();
            const double dThreshold = 0.11;
            const double ddThreshold = -0.08;
            // TODO: Interchange loop ordering for performance
            for (int j = 0; j < imageRidge.Height; ++j)
            {
                for (int i = 0; i < imageRidge.Width; ++i)
                {
                    double p = imageLp[i, j];
                    double q = imageLq[i, j];
                    double pp = imageLpp[i, j];
                    double qq = imageLqq[i, j];
                    double ridge;
                    if ( (Math.Abs(p) < dThreshold) && (pp < ddThreshold) && (Math.Abs(pp) >= Math.Abs(qq))
                      || (Math.Abs(q) < dThreshold) && (qq < ddThreshold) && (Math.Abs(qq) >= Math.Abs(pp)))
                    {
                        ridge = 1.0;
                    }
                    else
                    {
                        ridge = 0.0;
                    }
                    imageRidge[i, j] = ridge;
                }
            }
            return imageRidge;
        }

        public static IImage<double> LocalLpp(IImage<double> imageLxx, IImage<double> imageLyy, IImage<double> imageLxy, IImage<double> imageBeta)
        {
            // Compute the second derivative in the local co-ordinate system
            IImage<double> imageLpp = (IImage<double>) imageLxx.Clone();
            imageLpp.Clear();

            // TODO: Interchange loop ordering for performance
            for (int j = 0; j < imageLpp.Height; ++j)
            {
                for (int i = 0; i < imageLpp.Width; ++i)
                {
                    double xx = imageLxx[i, j];
                    double yy = imageLyy[i, j];
                    double xy = imageLxy[i, j];
                    double beta = imageBeta[i, j];

                     // A unit vector parallel to p axis
                    double uPx =  Math.Sin(beta);
                    double uPy = -Math.Cos(beta);

                    double pp = (xx * uPx * uPx) + (2 * xy * uPx * uPy) + (yy * uPy * uPy);
                    imageLpp[i, j] = pp;
                }
            }
            return imageLpp;
        }

        public static IImage<double> LocalLqq(IImage<double> imageLxx, IImage<double> imageLyy, IImage<double> imageLxy, IImage<double> imageBeta)
        {
            // Compute the second derivative in the local co-ordinate system
            IImage<double> imageLqq = (IImage<double>) imageLxx.Clone();
            imageLqq.Clear();
            // TODO: Interchange loop ordering for performance
            for (int j = 0; j < imageLqq.Height; ++j)
            {
                for (int i = 0; i < imageLqq.Width; ++i)
                {
                    double xx = imageLxx[i, j];
                    double yy = imageLyy[i, j];
                    double xy = imageLxy[i, j];
                    double beta = imageBeta[i, j];

                    // A unit vector parallel to q axis
                    double uQx = Math.Cos(beta);
                    double uQy = Math.Sin(beta);

                    double qq = (xx * uQx * uQx) + (2 * xy * uQx * uQy) + (yy * uQy * uQy);
                    imageLqq[i, j] = qq;
                }
            }
            return imageLqq;
        }

        public static IImage<double> LocalLp(IImage<double> imageLx, IImage<double> imageLy, IImage<double> imageBeta)
        {
            // Compute the gradient in the local co-ordinate system
            IImage<double> imageLp = (IImage<double>) imageLx.Clone();
            imageLp.Clear();
            // TODO: Interchange loop ordering for performance
            for (int j = 0; j < imageLp.Height; ++j)
            {
                for (int i = 0; i < imageLp.Width; ++i)
                {
                    double x = imageLx[i, j];
                    double y = imageLy[i, j];
                    double beta = imageBeta[i, j];
                    double p = Math.Sin(beta) * x - Math.Cos(beta) * y;
                    imageLp[i, j] = p;
                }
            }
            return imageLp;
        }

        public static IImage<double> LocalLq(IImage<double> imageLx, IImage<double> imageLy, IImage<double> imageBeta)
        {
            // Compute the gradient in the local co-ordinate system
            IImage<double> imageLq = (IImage<double>) imageLx.Clone();
            imageLq.Clear();

            // TODO: Interchange loop ordering for performance
            for (int j = 0; j < imageLq.Height; ++j)
            {
                for (int i = 0; i < imageLq.Width; ++i)
                {
                    double x = imageLx[i, j];
                    double y = imageLy[i, j];
                    double beta = imageBeta[i, j];
                    double q = Math.Cos(beta) * x + Math.Sin(beta) * y;
                    imageLq[i, j] = q;
                }
            }
            return imageLq;
        }

        public static IImage<double> LocalLpq(IImage<double> imageLxx, IImage<double> imageLyy, IImage<double> imageLxy, IImage<double> imageBeta)
        {
            // Compute cross derivative in a local(p,q) co-ordinate system,
            // with the axis of PQ defined by imageBeta. This grid should be zero
            // if imageBeta is computed based upon Lxx, Lyy and Lxy.
            IImage<double> imageLpq = (IImage<double>) imageLxx.Clone();
            imageLpq.Clear();

            // TODO: Interchange loop ordering for performance
            for (int j = 0; j < imageLpq.Height; ++j)
            {
                for (int i = 0; i < imageLpq.Width; ++i)
                {
                    double xx = imageLxx[i, j];
                    double yy = imageLyy[i, j];
                    double xy = imageLxy[i, j];
                    double beta = imageBeta[i, j];

                    double pq = Math.Cos(beta) * Math.Sin(beta) * (xx - yy) - (Math.Cos(beta) * Math.Cos(beta) - Math.Sin(beta) * Math.Sin(beta)) * xy;
                    imageLpq[i, j] = pq;
                }
            }
            return imageLpq;
        }

        public static IImage<double> PrincipleCurvatureDirection(IImage<double> imageLxx, IImage<double> imageLyy, IImage<double> imageLxy)
        {
            // Determine beta, find the rotation angle of a co-ordinate system
            // (p, q) for which the rotated mixed second-order derivative Lpq == 0

            IImage<double> imageBeta = (IImage<double>) imageLxx.Clone();
            imageBeta.Clear();

            // TODO: Interchange loop ordering for performance
            for (int j = 0; j < imageBeta.Height; ++j)
            {
                for (int i = 0; i < imageBeta.Width; ++i)
                {
                    double xx = imageLxx[i, j];
                    double yy = imageLyy[i, j];
                    double xy = imageLxy[i, j];

                    double v = ((xx - yy) / Math.Sqrt((xx - yy) * (xx - yy) + 4 * xy * xy));
                    double cosBeta = Math.Sqrt(0.5 * (1 + v));
                    double sinBeta = Math.Sign(xy) * Math.Sqrt(0.5 * (1 - v));
                    double beta = Math.Atan2(sinBeta, cosBeta);
                    imageBeta[i, j] = beta;
                }
            }
            return imageBeta;
        }
    }
}
