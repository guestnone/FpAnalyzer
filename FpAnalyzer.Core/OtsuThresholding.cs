using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace FpAnalyzer.Core
{
    public static class OtsuThresholding
    {

        public static unsafe Bitmap GetOtsuThresholded2(this Bitmap sourceBitmap)
        {
            BitmapData data = sourceBitmap.LockBits(
                new System.Drawing.Rectangle(System.Drawing.Point.Empty, sourceBitmap.Size),
                ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb
            );

            byte[] p = new byte[data.Stride * data.Height];
            Marshal.Copy(data.Scan0, p, 0, p.Length);
            sourceBitmap.UnlockBits(data);

            int length = data.Stride * sourceBitmap.Height;
            byte[] resultBuffer = new byte[data.Stride *
                                           data.Height];

			var histData = new int[256];
            float sum = 0;
            for (int i = 0; i < length; ++i)
                histData[p[i]]++;
            for (int i = 0; i < 256; i++)
                sum += i * histData[i];

            float sumB = 0;
            int back = 0;
            int threshold = 0;
            float varMax = 0;

            for (int i = 0; i < 256; i++)
            {
                back += histData[i];
                if (back == 0)
                    continue;

                int fore = length - back;
                if (fore == 0)
                    break;

                sumB += i * histData[i];

                float backMean = sumB / back;
                float foreMean = (sum - sumB) / fore;

                float varBetween = (float)back * fore * (backMean - foreMean) * (backMean - foreMean);

                if (varBetween > varMax)
                {
                    varMax = varBetween;
                    threshold = i;
                }
            }


            for (int i = 0; i < length; i++)
                resultBuffer[i] = p[i] > threshold ? byte.MaxValue : byte.MinValue;

            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
            BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
			Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
			resultBitmap.UnlockBits(resultData);
            return resultBitmap;
		}

	}
}