using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace FpAnalyzer.Core
{

    using Minutiaes = Dictionary<MinutiaeType, List<(int X, int Y)>>;

    public enum MinutiaeType
    {
        Ending,
        Bifurcation,
        Crossing,
    }

    static class MinutiaeExtractor
    {

        public static unsafe Bitmap GetMinutiaeExtracted(this Bitmap sourceBitmap)
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

            var minutiaes = new Minutiaes();

            int stride = data.Stride;
            int height = data.Height;

            for (int i = 1; i < height - 1; i++)
                for (int j = 3; j < stride - 3; j += 3)
                {
                    int offset = i * stride + j;

                    if (p[offset] == 255)
                        continue;

                    int sum = 0;

                    if (IsValid(p[offset + 3]))
                        ++sum;

                    if (IsValid(p[offset - 3]))
                        ++sum;

                    for (int k = -1; k < 2; k++)
                    {
                        if (IsValid(p[offset + stride + k * 3]))
                            ++sum;

                        if (IsValid(p[offset - stride + k * 3]))
                            ++sum;
                    }

                    switch (sum)
                    {
                        case 1:
                            Extract(minutiaes, p, i, j, offset, stride, MinutiaeType.Ending);
                            break;

                        case 3:
                            Extract(minutiaes, p, i, j, offset, stride, MinutiaeType.Bifurcation);
                            break;

                        case 4:
                            Extract(minutiaes, p, i, j, offset, stride, MinutiaeType.Crossing);
                            break;
                    }
                }

            

            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
            BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
            Marshal.Copy(p, 0, resultData.Scan0, p.Length);
            resultBitmap.UnlockBits(resultData);

            for (int k = 0; k < minutiaes[MinutiaeType.Ending].Count; k++)
            {
                for (int m = -2; m < 3; m++)
                {
                    for (int n = -2; n < 3; n++)
                    {
                        if (minutiaes[MinutiaeType.Ending][k].X + m >= 0 && minutiaes[MinutiaeType.Ending][k].X + m < resultBitmap.Width && minutiaes[MinutiaeType.Ending][k].Y + n >= 0 && minutiaes[MinutiaeType.Ending][k].Y + n < resultBitmap.Height && (m == -2 || m == 2 || n ==-2 || n==2)) {
                            resultBitmap.SetPixel(minutiaes[MinutiaeType.Ending][k].X + m, minutiaes[MinutiaeType.Ending][k].Y + n, Color.Red);
                        }
                    }
                }
            }

            for (int k = 0; k < minutiaes[MinutiaeType.Bifurcation].Count; k++)
            {
                for (int m = -2; m < 3; m++)
                {
                    for (int n = -2; n < 3; n++)
                    {
                        if (minutiaes[MinutiaeType.Bifurcation][k].X + m >= 0 && minutiaes[MinutiaeType.Bifurcation][k].X + m < resultBitmap.Width && minutiaes[MinutiaeType.Bifurcation][k].Y + n >= 0 && minutiaes[MinutiaeType.Bifurcation][k].Y + n < resultBitmap.Height && (m == -2 || m == 2 || n == -2 || n == 2))
                        {
                            resultBitmap.SetPixel(minutiaes[MinutiaeType.Bifurcation][k].X + m, minutiaes[MinutiaeType.Bifurcation][k].Y + n, Color.Blue);
                        }
                    }
                }
            }

            for (int k = 0; k < minutiaes[MinutiaeType.Crossing].Count; k++)
            {
                for (int m = -2; m < 3; m++)
                {
                    for (int n = -2; n < 3; n++)
                    {
                        if (minutiaes[MinutiaeType.Crossing][k].X + m >= 0 && minutiaes[MinutiaeType.Crossing][k].X + m < resultBitmap.Width && minutiaes[MinutiaeType.Crossing][k].Y + n >= 0 && minutiaes[MinutiaeType.Crossing][k].Y + n < resultBitmap.Height && (m == -2 || m == 2 || n == -2 || n == 2))
                        {
                            resultBitmap.SetPixel(minutiaes[MinutiaeType.Crossing][k].X + m, minutiaes[MinutiaeType.Crossing][k].Y + n, Color.Green);
                        }
                    }
                }
            }

            return resultBitmap;
        }

        private static void Extract(
            Minutiaes minutiaes,
            byte[] p,
            int i,
            int j,
            int offset,
            int stride,
            MinutiaeType type
        )
        {
            //p[offset + 0] = 250;
            //p[offset + 1] = 120;
            //p[offset + 2] = 0;

            if (minutiaes.ContainsKey(type))
                minutiaes[type].Add((j / 3, i));
            else
                minutiaes[type] = new List<(int X, int Y)>() { (j / 3, i) };
        }

        public static bool IsValid(byte b) => b != 255;

        public static int GetDifferences(Minutiaes first, Minutiaes second)
        {
            var values = (MinutiaeType[])Enum.GetValues(typeof(MinutiaeType));
            var count = new int[values.Length];

            for (int i = 0; i < first.Count; i++)
                if (first.TryGetValue(values[i], out var list))
                    count[i] = list.Count;
            for (int i = 0; i < first.Count; i++)
                if (second.TryGetValue(values[i], out var list))
                    count[i] -= list.Count;

            int sum = 0;
            foreach (int s in count)
                sum += s > 0 ? s : -s;
            return sum;
        }
    }
}
