using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace FpAnalyzer.Core
{

    public enum FingerPrintProcessingStage
    {
        GrayScale,
        Thresholding,
        MedianFiltering,
        Skeletonization,
        MinutaeFiltering,
        FinalOutput
    }

    public class FingerPrintProcessor
    {

        public bool Process(bool grayScale)
        {
            if (mSourceBitmap == null)
                return false;
            mPostGrayScalingBitmap = ToGrayScale(mSourceBitmap);
            mPostThresholdBitmap = mPostGrayScalingBitmap.GetOtsuThresholded2();
            mPostMedianFilterBitmap = mPostThresholdBitmap.GetMedianFiltered(mMedianFilterKernelSize, mMedianFilterBias, true);
            // TODO: Implement Skeletonization and minutae filtering
            mPostSkeletonizationBitmap = mPostMedianFilterBitmap.GetSkeletonization();
            mPostMinutaeFilteringBitmap = mPostSkeletonizationBitmap.GetMinutiaeExtracted();
            return true;
        }

        public Bitmap GetProcessingStageBitmap(FingerPrintProcessingStage stage)
        {
            switch (stage)
            {
                case FingerPrintProcessingStage.GrayScale:
                    return mPostGrayScalingBitmap;
                case FingerPrintProcessingStage.Thresholding:
                    return mPostThresholdBitmap;
                case FingerPrintProcessingStage.MedianFiltering:
                    return mPostMedianFilterBitmap;
                case FingerPrintProcessingStage.Skeletonization:
                    return mPostSkeletonizationBitmap;
                case FingerPrintProcessingStage.MinutaeFiltering:
                    return mPostMinutaeFilteringBitmap;
                case FingerPrintProcessingStage.FinalOutput:
                    return mPostMinutaeFilteringBitmap;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stage), stage, null);
            }
        }

        public Bitmap SourceBitmap
        {
            set => mSourceBitmap = value;
        }

        public int MedianFilterKernelSize
        {
            get => mMedianFilterKernelSize;
            set => mMedianFilterKernelSize = value;
        }

        public int MedianFilterBias
        {
            get => mMedianFilterBias;
            set => mMedianFilterBias = value;
        }

        // Settings
        private int mMedianFilterKernelSize = 3;
        private int mMedianFilterBias = 0;

        // Processing stages
        private Bitmap mSourceBitmap = null;
        private Bitmap mPostGrayScalingBitmap;
        private Bitmap mPostThresholdBitmap;
        private Bitmap mPostMedianFilterBitmap;
        private Bitmap mPostSkeletonizationBitmap;
        private Bitmap mPostMinutaeFilteringBitmap;
        // private Bitmap mFinalImage
        //
        public static Bitmap ToGrayScale(Bitmap Bmp)
        {
            int rgb;
            Color c;
            Bitmap output = DeepClone<Bitmap>(Bmp);

            for (int y = 0; y < Bmp.Height; y++)
            for (int x = 0; x < Bmp.Width; x++)
            {
                c = output.GetPixel(x, y);
                rgb = (int)Math.Round(.299 * c.R + .587 * c.G + .114 * c.B);
                output.SetPixel(x, y, Color.FromArgb(rgb, rgb, rgb));
            }

            return output;
        }

        public static T DeepClone<T>(T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
