using System.Diagnostics;
using System.Net.Mime;
namespace OpenCvSharp.Demo
{
    using UnityEngine;
    using System.Collections;
    using OpenCvSharp;
    using UnityEngine.UI;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    public class RemoveBackground : MonoBehaviour
    {
        public Texture2D m_texture;
        public RawImage m_image_origin;
        public RawImage m_image_gray;
        public RawImage m_Image_binarization;
        public RawImage m_image_mask;
        public RawImage m_image_backgroundTransparent;
        public double v_thresh = 180;
        public double v_maxval = 255;
        public RemoveBackground()
        {
            
        }
        public Texture2D Remove(Texture2D texture)
        {
            Mat origin = Unity.TextureToMat(texture);
            Mat grayMat = new Mat();
            Cv2.CvtColor(origin, grayMat, ColorConversionCodes.BGR2GRAY);
            Mat thresh = new Mat();
            Cv2.Threshold(grayMat, thresh, v_thresh, v_maxval, ThresholdTypes.BinaryInv);
            Mat Mask = Unity.TextureToMat(Unity.MatToTexture(grayMat));
            Point[][] contours; HierarchyIndex[] hierarchy;
            Cv2.FindContours(thresh, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxNone, null);
            for (int i = 0; i < contours.Length; i++)
            {
                Cv2.DrawContours(Mask, new Point[][] { contours[i] }, 0, new Scalar(0, 0, 0), -1);
            }
            Mask = Mask.CvtColor(ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(Mask, Mask, v_thresh, v_maxval, ThresholdTypes.Binary);
            Mat transparent = origin.CvtColor(ColorConversionCodes.BGR2BGRA);
            unsafe
            {
                byte* b_transparent = transparent.DataPointer;
                byte* b_mask = Mask.DataPointer;
                float pixelCount = transparent.Height * transparent.Width;
                for (int i = 0; i < pixelCount; i++)
                {
                    if (b_mask[0] == 255)
                    {
                        b_transparent[0] = 0;
                        b_transparent[1] = 0;
                        b_transparent[2] = 0;
                        b_transparent[3] = 0;
                    }
                    b_transparent = b_transparent + 4;
                    b_mask = b_mask + 1;
                }
            }
            Texture2D resultTex = Unity.MatToTexture(transparent);
            return resultTex;
        }
    }
}