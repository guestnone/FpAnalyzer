using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FpAnalyzer.Core;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace FpAnalyzer.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Bitmap mSourceBitmap;
        private bool isLoaded = false;
        private FingerPrintProcessor mFingerPrintProcessor = new FingerPrintProcessor();
        private string mFileName = "";
        private bool mGrayScale = false;

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog
            {
                Title = "Select image file",
                InitialDirectory = Directory.GetCurrentDirectory(),
                Filter = "PNG (*.png)|*.png|JPEG (*.jpeg)|*.jpeg|All files (*.*)|*.*"
            };

            if (open.ShowDialog() == true)
            {
                mFileName = open.SafeFileName;
                CurrStageViewSizeComboBox.SelectedItem = CSVSourceImage;
                mSourceBitmap = new Bitmap(open.FileName);

                if (System.Drawing.Image.GetPixelFormatSize(mSourceBitmap.PixelFormat) != 24)
                {
                    mGrayScale = true;
                    var bmp = new Bitmap(mSourceBitmap.Width, mSourceBitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    using (var g = Graphics.FromImage(bmp))
                        g.DrawImage(mSourceBitmap, PointF.Empty);
                    mSourceBitmap = bmp;
                }
                mFingerPrintProcessor.SourceBitmap = mSourceBitmap;
                isLoaded = true;
                ProcessButton.IsEnabled = true;
                Image.Source = BitmapToImageSource(mSourceBitmap);
                SaveAllStageButton.IsEnabled = false;
                SaveFinalImageButton.IsEnabled = false;
                CurrStageViewSizeComboBox.IsEnabled = false;
            }
        }

        private void ProcessButton_Click(object sender, RoutedEventArgs e)
        {
            CurrStageViewSizeComboBox.SelectedItem = CSVSourceImage;
            if (BiasIntegerUpDown.Value != null) mFingerPrintProcessor.MedianFilterBias = (int) BiasIntegerUpDown.Value;
            if (mFingerPrintProcessor.Process(mGrayScale))
            {
                CurrStageViewSizeComboBox.IsEnabled = true;
                SaveAllStageButton.IsEnabled = true;
                SaveFinalImageButton.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("Something bad happened!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MedianFilterKernelSizeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Equals(MedianFilterKernelSizeComboBox.SelectedItem, KeDisable))
            {
                mFingerPrintProcessor.MedianFilterKernelSize = 1;
            }
            else if (Equals(MedianFilterKernelSizeComboBox.SelectedItem, Ke3x3))
            {
                mFingerPrintProcessor.MedianFilterKernelSize = 3;
            }
            else if (Equals(MedianFilterKernelSizeComboBox.SelectedItem, Ke5x5))
            {
                mFingerPrintProcessor.MedianFilterKernelSize = 5;
            }
            else if (Equals(MedianFilterKernelSizeComboBox.SelectedItem, Ke7x7))
            {
                mFingerPrintProcessor.MedianFilterKernelSize = 7;
            }
            else if (Equals(MedianFilterKernelSizeComboBox.SelectedItem, Ke9x9))
            {
                mFingerPrintProcessor.MedianFilterKernelSize = 9;
            }
            else if (Equals(MedianFilterKernelSizeComboBox.SelectedItem, Ke11x11))
            {
                mFingerPrintProcessor.MedianFilterKernelSize = 11;
            }
            else if (Equals(MedianFilterKernelSizeComboBox.SelectedItem, Ke13x13))
            {
                mFingerPrintProcessor.MedianFilterKernelSize = 13;
            }
            else
            {
                
            }
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        private void CurrStageViewSizeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mSourceBitmap == null)
                return;
            if (Equals(CurrStageViewSizeComboBox.SelectedItem, CSVSourceImage))
            {
                Image.Source = BitmapToImageSource(mSourceBitmap);
            }
            else if (Equals(CurrStageViewSizeComboBox.SelectedItem, CSVPostGrayScale))
            {
                Image.Source = BitmapToImageSource(mFingerPrintProcessor.GetProcessingStageBitmap(FingerPrintProcessingStage.GrayScale));
            }
            else if (Equals(CurrStageViewSizeComboBox.SelectedItem, CSVPostThresholding))
            {
                Image.Source = BitmapToImageSource(mFingerPrintProcessor.GetProcessingStageBitmap(FingerPrintProcessingStage.Thresholding));
            }
            else if (Equals(CurrStageViewSizeComboBox.SelectedItem, CSVPostMedianFilter))
            {
                Image.Source = BitmapToImageSource(mFingerPrintProcessor.GetProcessingStageBitmap(FingerPrintProcessingStage.MedianFiltering));
            }
            else if (Equals(CurrStageViewSizeComboBox.SelectedItem, CSVPostSkeletonization))
            {
                Image.Source = BitmapToImageSource(mFingerPrintProcessor.GetProcessingStageBitmap(FingerPrintProcessingStage.Skeletonization));
            }
            else if (Equals(CurrStageViewSizeComboBox.SelectedItem, CSVPostMinuaeFiltering))
            {
                Image.Source = BitmapToImageSource(mFingerPrintProcessor.GetProcessingStageBitmap(FingerPrintProcessingStage.MinutaeFiltering));
            }
            else if (Equals(CurrStageViewSizeComboBox.SelectedItem, CSVFinalImage))
            {
                Image.Source = BitmapToImageSource(mFingerPrintProcessor.GetProcessingStageBitmap(FingerPrintProcessingStage.FinalOutput));
            }
            else
            {
                //throw new IndexOutOfRangeException("Unknown Stage");
            }
        }

        private void SaveFinalImageButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog
            {
                Title = "SaveFinalImage",
                InitialDirectory = Directory.GetCurrentDirectory(),
                DefaultExt = "png",
                Filter = "PNG (*.png)|*.png|All files (*.*)|*.*"
            };
            if (save.ShowDialog() == true)
            {
                mFingerPrintProcessor.GetProcessingStageBitmap(FingerPrintProcessingStage.FinalOutput).Save(save.FileName);
            }
        }

        private void SaveAllStageButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string fileNameLoc = dialog.FileName + "\\" + mFileName;
                mFingerPrintProcessor.GetProcessingStageBitmap(FingerPrintProcessingStage.GrayScale).Save(fileNameLoc + "_gray.png");
                mFingerPrintProcessor.GetProcessingStageBitmap(FingerPrintProcessingStage.Thresholding).Save(fileNameLoc + "_thresholded.png");
                mFingerPrintProcessor.GetProcessingStageBitmap(FingerPrintProcessingStage.MedianFiltering).Save(fileNameLoc + "_medianFiltered.png");
                mFingerPrintProcessor.GetProcessingStageBitmap(FingerPrintProcessingStage.Skeletonization).Save(fileNameLoc + "_skeletonized.png");
                mFingerPrintProcessor.GetProcessingStageBitmap(FingerPrintProcessingStage.MinutaeFiltering).Save(fileNameLoc + "_minutaeFiltered.png");
                mFingerPrintProcessor.GetProcessingStageBitmap(FingerPrintProcessingStage.FinalOutput).Save(fileNameLoc + "_final.png");
            }
        }
    }
}
