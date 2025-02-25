        using System;
        using System.Collections.Generic;
        using System.Drawing;
        using System.Linq;
        using System.Text;
        using System.Threading.Tasks;
        using System.Windows.Forms;
        using static ImageSimilarity.ImageOperations;

namespace ImageSimilarity
{
    public struct ChannelStats
    {
        public int[] Hist;
        public int Min;
        public int Max;
        public int Med;
        public double Mean;
        public double StdDev;
    }
    public struct ImageInfo
    {
        public string Path;
        public int Width;
        public int Height;
        public ChannelStats RedStats;
        public ChannelStats GreenStats;
        public ChannelStats BlueStats;
    }

    public struct MatchInfo
    {
        public string MatchedImgPath;
        public double MatchScore;
    }
    public class ImageHistSimilarity
    {
        /// <summary>
        /// Calculate the image stats (Max, Min, Med, Mean, StdDev & Histogram) of each color
        /// </summary>
        /// <param name="imgPath">Image path</param>
        /// <returns>Calculated stats of the given image</returns>
        public static ImageInfo CalculateImageStats(string imgPath)
        {
            ImageInfo info = new ImageInfo();
            info.Path = imgPath;
            RGBPixel[,] rGBPixel = OpenImage(imgPath);
            info.Height = ImageOperations.GetHeight(rGBPixel);//0
            info.Width = ImageOperations.GetWidth(rGBPixel);//1
            int size = info.Height * info.Width;

            int[] histogramR = new int[256];
            int[] histogramG = new int[256];
            int[] histogramB = new int[256];
            int maxr = 0, maxg = 0, maxb = 0;
            int minr = 256, ming = 0, minb = 0;
            double meanr = 0, meang = 0, meanb = 0;

            List<int> pixelsr = new List<int>();
            List<int> pixelsg = new List<int>();
            List<int> pixelsb = new List<int>();

            for (int i = 0; i < info.Height; i++)
            {
                for (int j = 0; j < info.Width; j++)
                {

                    int pixelR = 0;
                    int pixelG = 0;
                    int pixelB = 0;

                    pixelR = rGBPixel[i, j].red;
                    pixelsr.Add(pixelR);
                    pixelG = rGBPixel[i, j].green;
                    pixelsg.Add(pixelG);
                    pixelB = rGBPixel[i, j].blue;
                    pixelsb.Add(pixelB);

                    histogramR[pixelR]++;
                    histogramG[pixelG]++;
                    histogramB[pixelB]++;

                    //pixelValues.Add(pixelValue);
                    //histogram[pixelValue]++; // Update histogram
                    if (pixelR < minr) minr = pixelR;
                    if (pixelR > maxr) maxr = pixelR;

                    if (pixelG < ming) ming = pixelG;
                    if (pixelG > maxg) maxg = pixelG;

                    if (pixelB < minb) minb = pixelB;
                    if (pixelB > maxb) maxb = pixelB;

                    meanr += pixelR;
                    meang += pixelG;
                    meanb += pixelB;


                }
            }
            meanr = meanr / size;
            meang = meang / size;
            meanb = meanb / size;

            //median red
            // Step 3: Compute Median using Histogram
            int medianR = -1, medianB = -1, medianG = -1;
            int countR = 0;
            int countB = 0;
            int countG = 0;

            int median = -1;
            bool isEven = (size % 2 == 0);
            int midIndex = size / 2;

            for (int i = 0; i < 256; i++)
            {
                countR += histogramR[i];
                countB += histogramB[i];
                countG += histogramG[i];


                if (medianR == -1 && countR >= midIndex)
                {
                    if (isEven && countR == midIndex)
                    {
                        medianR = (i + i++) / 2;
                    }
                    else
                    {
                        medianR = i;
                    }
                }

                if (medianG == -1 && countG >= midIndex)
                {
                    if (isEven && countG == midIndex)
                    {
                        medianG = (i + i++) / 2;
                    }
                    else
                    {
                        medianG = i;
                    }
                }

                if (medianB == -1 && countB >= midIndex)
                {
                    if (isEven && countB == midIndex)
                    {
                        medianB = (i + i++) / 2;
                    }
                    else
                    {
                        medianB = i;
                    }
                }

                if (medianR != -1 && medianG != -1 && medianB != -1) break;
            }




            /// step for std 
            double redSquaredDiffs = 0; // For std
            double greenSquaredDiffs = 0; // For std
            double blueSquaredDiffs = 0; // For std

            for (int i = 0; i < 256; i++)
            {
                redSquaredDiffs += histogramR[i] * Math.Pow(i - meanr, 2);
                greenSquaredDiffs += histogramG[i] * Math.Pow(i - meang, 2);
                blueSquaredDiffs += histogramB[i] * Math.Pow(i - meanb, 2);
            }
            double stdDevR = Math.Sqrt(redSquaredDiffs / size);
            double stdDevG = Math.Sqrt(greenSquaredDiffs / size);
            double stdDevB = Math.Sqrt(blueSquaredDiffs / size);


            //////////////////////////////////red
            ChannelStats red = new ChannelStats();
            RGBPixelD rgbPixel= new RGBPixelD();
            double x = rgbPixel.red = meanr;
            red.Mean = x;
            red.Hist = histogramR;
            red.Max = maxr;
            red.Min = minr;
            red.Med = medianR;
            red.StdDev = stdDevR;

            ///////////////////////////////////////////////green
            ChannelStats green = new ChannelStats();
            green.Mean = meang;
            green.Hist = histogramG;
            green.Med = medianG;
            green.Max = maxg;
            green.Min = ming;
            green.StdDev = stdDevG;
            /////////////////////////blue
            ChannelStats blue = new ChannelStats();
            blue.Mean = meanb;
            blue.Hist = histogramB;
            blue.Med = medianB;
            blue.Max = maxb;
            blue.Min = minb;
            blue.StdDev = stdDevB;
            ///////////////////////////////////////////////////////////////
            info.RedStats = red;
            info.GreenStats = green;
            info.BlueStats = blue;

            return info;
        }


        /// <summary>
        /// Load all target images and calculate their stats
        /// </summary>
        /// <param name="targetPaths">Path of each target image</param>
        /// <returns>Calculated stats of each target image</returns>
        public static ImageInfo[] LoadAllImages(string[] targetPaths)
        {
            int size = targetPaths.Length;
            ImageInfo[] info_image = new ImageInfo[size];
            for (int i = 0; i < size; i++)
            {
                info_image[i] = CalculateImageStats(targetPaths[i]);
            }
            return info_image;

        }

        /// <summary>
        /// Match the given query image with the given target images and return the TOP matches as specified
        /// </summary>
        /// <param name="queryPath">Path of the query image</param>
        /// <param name="targetImgStats">Calculated stats of each target image</param>
        /// <param name="numOfTopMatches">Desired number of TOP matches to be returned</param>
        /// <returns>Top matches (image path & distance score) </returns>
        public static MatchInfo[] FindTopMatches(string queryPath, ImageInfo[] targetImgStats, int numOfTopMatches)
        {
    ImageInfo info_quary = CalculateImageStats(queryPath);

        List<MatchInfo> matches = new List<MatchInfo>();

        // Initialize similarity score array with correct size
        double[] similarityScore = new double[targetImgStats.Length];

        for (int i = 0; i < targetImgStats.Length; i++)
        {
            similarityScore[i] = ComputeCosineSimilarity(info_quary, targetImgStats[i]);
            matches.Add(new MatchInfo { MatchedImgPath = targetImgStats[i].Path, MatchScore = similarityScore[i] });
        }

        // Sort matches based on similarity score (lower score = more similar)
        matches.Sort((a, b) => a.MatchScore.CompareTo(b.MatchScore));

        // Return top N matches
        return matches.Take(numOfTopMatches).ToArray();
        
        }
        private static double ComputeCosineSimilarity(ImageInfo info_quary, ImageInfo targetImgStats)
        {

            int sizequary = info_quary.Height * info_quary.Width;
            double[] PtargetHistr = new double[256];
            double[] PtargetHistg = new double[256];
            double[] PtargetHistb = new double[256];

            double[] PHistr = new double[256];
            double[] PHistg = new double[256];
            double[] PHistb = new double[256];
            int sizetarget = targetImgStats.Width * targetImgStats.Height;
            sizetarget = Math.Max(1, sizetarget);
            sizequary = Math.Max(1, sizequary);

            for (int i = 0; i < 256; i++)
            {
                PtargetHistr[i] = targetImgStats.RedStats.Hist[i] / (double)sizetarget;
                PtargetHistg[i] = targetImgStats.GreenStats.Hist[i] / (double)sizetarget;
                PtargetHistb[i] = targetImgStats.BlueStats.Hist[i] / (double)sizetarget;
                 PHistr[i] = info_quary.RedStats.Hist[i] / (double)sizequary;
                 PHistg[i] = info_quary.GreenStats.Hist[i] / (double)sizequary;
                 PHistb[i] = info_quary.BlueStats.Hist[i] / (double)sizequary;
            }
         
            double dotProductR = 0, magnitudeRED1 = 0, magnitudeRED2 = 0;
            double dotProductG = 0, magnitudeGREEN1 = 0, magnitudeGREEN2 = 0;
            double dotProductB = 0, magnitudeBLUE1 = 0, magnitudeBLUE2 = 0;
            for (int i = 0; i < 256; i++)
            {
                dotProductR += PHistr[i] * PtargetHistr[i];
                dotProductG += PHistg[i] * PtargetHistg[i];
                dotProductB += PHistb[i] * PtargetHistb[i];
                magnitudeRED1 += PHistr[i] * PHistr[i];
                magnitudeGREEN1 += PHistg[i] * PHistg[i];
                magnitudeBLUE1 += PHistb[i] * PHistb[i];
                magnitudeRED2 += PtargetHistr[i] * PtargetHistr[i];
                magnitudeGREEN2 += PtargetHistg[i] * PtargetHistg[i];
                magnitudeBLUE2 += PtargetHistb[i] * PtargetHistb[i];

            }

            magnitudeRED1 = Math.Sqrt(magnitudeRED1);
            magnitudeRED2 = Math.Sqrt(magnitudeRED2);
            magnitudeGREEN1 = Math.Sqrt(magnitudeGREEN1);
            magnitudeGREEN2 = Math.Sqrt(magnitudeGREEN2);
            magnitudeBLUE1 = Math.Sqrt(magnitudeBLUE1);
            magnitudeBLUE2 = Math.Sqrt(magnitudeBLUE2);
            double CosineREDSimilarity, CosineGREENSimilarity, CosineBLUESimilarity;
         /* Console.WriteLine($"DotProduct R: {dotProductR}, G: {dotProductG}, B: {dotProductB}");
            Console.WriteLine($"Magnitude R1: {magnitudeRED1}, R2: {magnitudeRED2}");
            Console.WriteLine($"Magnitude G1: {magnitudeGREEN1}, G2: {magnitudeGREEN2}");
            Console.WriteLine($"Magnitude B1: {magnitudeBLUE1}, B2: {magnitudeBLUE2}");*/

            if (magnitudeRED1 == 0 || magnitudeRED2 == 0) CosineREDSimilarity = 0;
            else CosineREDSimilarity = dotProductR / (magnitudeRED1 * magnitudeRED2);

            if (magnitudeGREEN1 == 0 || magnitudeGREEN2 == 0) CosineGREENSimilarity = 0;
            else CosineGREENSimilarity = dotProductG / (magnitudeGREEN1 * magnitudeGREEN2);

            if (magnitudeBLUE1 == 0 || magnitudeBLUE2 == 0) CosineBLUESimilarity = 0;
            else CosineBLUESimilarity = dotProductB / (magnitudeBLUE1 * magnitudeBLUE2);
          //Console.WriteLine($"Cosine R: {CosineREDSimilarity}, G: {CosineGREENSimilarity}, B: {CosineBLUESimilarity}");

            // Clamp values to prevent NaN
            CosineREDSimilarity = Math.Max(-1, Math.Min(1, CosineREDSimilarity));
            CosineGREENSimilarity = Math.Max(-1, Math.Min(1, CosineGREENSimilarity));
            CosineBLUESimilarity = Math.Max(-1, Math.Min(1, CosineBLUESimilarity));

            double CosineREDDistance = Math.Acos(CosineREDSimilarity) * (180 / Math.PI);
            double CosineGREENDistance = Math.Acos(CosineGREENSimilarity) * (180 / Math.PI);
            double CosineBLUEDistance = Math.Acos(CosineBLUESimilarity) * (180 / Math.PI);
            double SCORE = (CosineREDDistance + CosineGREENDistance + CosineBLUEDistance) / 3;
            return SCORE;
        }

    }
}

         
