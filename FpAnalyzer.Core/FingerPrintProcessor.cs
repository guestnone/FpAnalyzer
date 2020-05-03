using System;
using System.Drawing;

namespace FpAnalyzer.Core
{


    public enum FingerPrintProcessingStage
    {
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
            mPostThresholdBitmap = mSourceBitmap.GetOtsuThresholded2();
            mPostMedianFilterBitmap = mPostThresholdBitmap.GetMedianFiltered(mMedianFilterKernelSize, mMedianFilterBias, true);
            // TODO: Implement Skeletonization and minutae filtering
            mPostSkeletonizationBitmap = mPostMedianFilterBitmap.GetSkeletonization();
            mPostMinutaeFilteringBitmap = mPostSkeletonizationBitmap;
            return true;
        }

        public Bitmap GetProcessingStageBitmap(FingerPrintProcessingStage stage)
        {
            switch (stage)
            {
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
        private Bitmap mPostThresholdBitmap;
        private Bitmap mPostMedianFilterBitmap;
        private Bitmap mPostSkeletonizationBitmap;
        private Bitmap mPostMinutaeFilteringBitmap;
        // private Bitmap mFinalImage
        //


    }
}
