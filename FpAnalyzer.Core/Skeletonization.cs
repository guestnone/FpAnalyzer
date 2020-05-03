﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace FpAnalyzer.Core
{
    static class Skeletonization
    {
        public static unsafe Bitmap GetSkeletonization(this Bitmap sourceBitmap)
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

            int bpp = length / sourceBitmap.Width / sourceBitmap.Height;

            var tmpOnes = new List<int>();
            for (int i = data.Stride + bpp; i < data.Stride * sourceBitmap.Height - data.Stride - bpp; i += bpp)
                if (p[i] == One)
                    tmpOnes.Add(i);
            int[] ones = tmpOnes.ToArray();

            bool hasDeleted = true;
            while (hasDeleted)
            {
                hasDeleted = false;

                foreach (int i in ones)
                    if (p[i + 3] == Zero || p[i + data.Stride] == Zero ||
                        p[i - 3] == Zero || p[i - data.Stride] == Zero)
                    {
                        p[i] = p[i + 1] = p[i + 2] = Two;
                    }

                foreach (int i in ones)
                    if (p[i] != Two && (
                        p[i + data.Stride + 3] == Zero || p[i + data.Stride - 3] == Zero ||
                        p[i - data.Stride + 3] == Zero || p[i - data.Stride - 3] == Zero))
                    {
                        p[i] = p[i + 1] = p[i + 2] = Three;
                    }

                foreach (int i in ones)
                    if (Fours.Contains(ComputeSum(i)))
                        p[i] = p[i + 1] = p[i + 2] = Four;

                foreach (int i in ones)
                    if (p[i] == Four && Deletion.Contains(ComputeSum(i)))
                        p[i] = p[i + 1] = p[i + 2] = Zero;

                foreach (int i in ones)
                    if (p[i] == Two && Deletion.Contains(ComputeSum(i)))
                    {
                        p[i] = p[i + 1] = p[i + 2] = Zero;
                        hasDeleted = true;
                    }

                foreach (int i in ones)
                    if (p[i] == Three && Deletion.Contains(ComputeSum(i)))
                    {
                        p[i] = p[i + 1] = p[i + 2] = Zero;
                        hasDeleted = true;
                    }

                var tmp = new List<int>(ones.Length >> 1);
                for (int i = 0; i < ones.Length; i++)
                    if (p[ones[i]] != Zero)
                        tmp.Add(ones[i]);
                ones = tmp.ToArray();
            }

            int ComputeSum(int sumOffset)
            {
                int sum = 0;

                if (p[sumOffset + bpp] != Zero) sum += Matrix[5];
                if (p[sumOffset - bpp] != Zero) sum += Matrix[3];

                sumOffset += data.Stride;

                if (p[sumOffset + bpp] != Zero) sum += Matrix[2];
                if (p[sumOffset] != Zero) sum += Matrix[1];
                if (p[sumOffset - bpp] != Zero) sum += Matrix[0];

                sumOffset -= data.Stride + data.Stride;

                if (p[sumOffset + bpp] != Zero) sum += Matrix[8];
                if (p[sumOffset] != Zero) sum += Matrix[7];
                if (p[sumOffset - bpp] != Zero) sum += Matrix[6];

                return sum;
            }


        Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
        BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height),
            ImageLockMode.WriteOnly,
            PixelFormat.Format24bppRgb);
        Marshal.Copy(p, 0, resultData.Scan0, p.Length);
            resultBitmap.UnlockBits(resultData);
            return resultBitmap;
        }

        private static readonly int[] Matrix =
        {
                128,  1, 2,
                 64,  0, 4,
                 32, 16, 8
            };

        private const byte Zero = 255;
        private const byte One = 0;
        private const byte Two = 2;
        private const byte Three = 3;
        private const byte Four = 4;

        private static readonly HashSet<int> Deletion = new HashSet<int>(new int[] { 3, 5, 7, 12, 13, 14, 15, 20, 21, 22, 23, 28, 29, 30, 31, 48, 52, 53, 54, 55, 56, 60, 61, 62, 63, 65, 67, 69, 71, 77, 79, 80, 81, 83, 84, 85, 86, 87, 88, 89, 91, 92, 93, 94, 95, 97, 99, 101, 103, 109, 111, 112, 113, 115, 116, 117, 118, 119, 120, 121, 123, 124, 125, 126, 127, 131, 133, 135, 141, 143, 149, 151, 157, 159, 181, 183, 189, 191, 192, 193, 195, 197, 199, 205, 207, 208, 209, 211, 212, 213, 214, 215, 216, 217, 219, 220, 221, 222, 223, 224, 225, 227, 229, 231, 237, 239, 240, 241, 243, 244, 245, 246, 247, 248, 249, 251, 252, 253, 254, 255 });
        private static readonly HashSet<int> Fours = new HashSet<int>(new int[] { 3, 6, 12, 24, 48, 96, 192, 129, 7, 14, 28, 56, 112, 224, 193, 131, 15, 30, 60, 120, 240, 225, 195, 135 });

    }
}
