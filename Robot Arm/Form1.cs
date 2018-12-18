using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO.Ports;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Robot_Arm
{
    public partial class ShapeSorter : Form
    {
        struct Shape //this struct is used to faciliate easy storage of a found shape
        {
            public PointF position;
            public int type;
            public enum Type {Triangle, Square};
        }

        Shape[] shapes;

        private Thread _visionThread;
        private Thread _armInterfaceThread;

        private VideoCapture _capture;

        private Size capturePictureBoxSize;
        private Size processPictureBoxSize;
        private Size workspacePictureBoxSize;

        //parameters for the paper corner averaging function
        Queue<Point[]> workspaceCornersSets = new Queue<Point[]>();
        int workspaceCornerAverageCount = 5;
        public ShapeSorter()
        {
            AutoSize = false;
            AutoScaleMode = 0;
            InitializeComponent();
        }

        private void ShapeSorter_Load(object sender, EventArgs e)
        {
            workspacePictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            capturePictureBoxSize   = new Size(capturePictureBox.Size.Width, capturePictureBox.Size.Height);
            processPictureBoxSize   = new Size(processPictureBox.Size.Width, processPictureBox.Size.Height);
            workspacePictureBoxSize = new Size(workspacePictureBox.Size.Width, workspacePictureBox.Size.Height);

            _capture = new VideoCapture(0);
            _visionThread = new Thread(GenerateWorkspace);
            _armInterfaceThread = new Thread(armInterface);
            _visionThread.Start();
        }


        private void armInterface() // This function is responsible for managing the serial interface with the arduino
        {
            //==== Function Variables ====
            SerialPort arduino = new SerialPort("COM5",9600);
            arduino.Open();
            Shape[] localShapes = shapes;
            
            //==== Serial Interface ====
            //after retrieving the global byte array, dispense shape/location codes to the arduino one by one
            // (by grabbing the global byte array (updated every frame) and creating a local copy to work from, the sorting is not affected
            // by the physical arm obsurcing the image.)
            for(int i = 0; i < localShapes.Length; i++)
            {
                byte[] msg = { 0, 0, 0 };

                if (localShapes[i].type == (int)Shape.Type.Triangle)
                    msg[0] = (byte)'t';
                else
                    msg[0] = (byte)'s';

                msg[1] = (byte)localShapes[i].position.X;
                msg[2] = (byte)localShapes[i].position.Y;

                arduino.Write(msg, 0, 3);

                //display last sent message to the form
                Invoke(new Action(() =>
                {
                    label1.Text = msg.ToString();
                }));

                //do not continue until the arduino responds
                if (arduino.ReadBufferSize == 0)
                    Thread.Sleep(50);
                else
                    arduino.DiscardInBuffer();
            }
        }
        
        private void GenerateWorkspace()
        {
            //========== Objects and Variables ==========
            Mat               captureFrame;
            Image<Gray, byte> processFrame;
            Image<Gray, byte> processContourFrame;
            Image<Gray, byte> workspaceFrame;
            MKeyPoint[] squareBlobs;
            MKeyPoint[] triBlobs;

            VectorOfVectorOfPoint processFrameContours = new VectorOfVectorOfPoint();
            byte processFrameThreshold = 150;

            //========== Square and Triangle Blob Detector Config ==========

            SimpleBlobDetectorParams squareBlobDetectorParams = new SimpleBlobDetectorParams();
            SimpleBlobDetectorParams triBlobDetectorParams = new SimpleBlobDetectorParams();
            squareBlobDetectorParams.FilterByArea = true;
            squareBlobDetectorParams.FilterByCircularity = false;
            squareBlobDetectorParams.FilterByColor = false;
            squareBlobDetectorParams.FilterByInertia = false;
            squareBlobDetectorParams.FilterByConvexity = false;
            squareBlobDetectorParams.MinArea = 10000;
            squareBlobDetectorParams.MaxArea = 100000;
            squareBlobDetectorParams.MaxCircularity = 1;
            squareBlobDetectorParams.MinCircularity = 0.67f;
            squareBlobDetectorParams.blobColor = 255;
            SimpleBlobDetector squareBlobDetector = new SimpleBlobDetector(squareBlobDetectorParams);
            triBlobDetectorParams.FilterByArea = true;
            triBlobDetectorParams.FilterByCircularity = false;
            triBlobDetectorParams.FilterByColor = false;
            triBlobDetectorParams.FilterByInertia = false;
            triBlobDetectorParams.FilterByConvexity = false;
            triBlobDetectorParams.MinArea = 2000;
            triBlobDetectorParams.MaxArea = 9999;
            triBlobDetectorParams.MaxCircularity = 0.66f;
            triBlobDetectorParams.MinCircularity = 0.01f;
            triBlobDetectorParams.blobColor = 255;
            SimpleBlobDetector triBlobDetector = new SimpleBlobDetector(triBlobDetectorParams);

            //========== Begin Shape Detection Algorithm ==========

            while(_capture.IsOpened)
            {
                //==== Pull Image from the Webcam ====
                captureFrame = _capture.QueryFrame();
                
                //==== Scrub Captured Frame ====
                //this optomizes the image for paper edge detection
                Image<Bgr, byte> processFrameBGR = captureFrame.ToImage<Bgr,byte>();
                Image<Hsv, byte> processFrameHSV = processFrameBGR.Convert<Hsv,byte>();
                processFrameHSV = processFrameHSV.SmoothMedian(9);
                Image<Gray,byte>[] procssFrameHSVBreakouts = processFrameHSV.Split();
                procssFrameHSVBreakouts[2]._EqualizeHist();
                //darken any colors in the image
                procssFrameHSVBreakouts[2] -= procssFrameHSVBreakouts[1]*1.5*(procssFrameHSVBreakouts[2].GetAverage().Intensity/255.0);
                processFrame = procssFrameHSVBreakouts[2];
                //convert to a binary image
                processFrame._ThresholdBinary(new Gray(processFrameThreshold), new Gray(255));
                
                //==== Detect Paper Border ====
                CvInvoke.FindContours(processFrame,processFrameContours,null, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
                int largestContourIndex = 0;
                double largestContour = 0;
                double currentContour = 0;
                for (int i = 0; i < processFrameContours.Size; i++)
                {
                    currentContour = CvInvoke.ContourArea(processFrameContours[i], false);
                    if(currentContour > largestContour)
                    {
                        largestContour = currentContour;
                        largestContourIndex = i;
                    }
                }
                processContourFrame = new Image<Gray, byte>(new Size(captureFrame.Size.Width, captureFrame.Size.Height));
                CvInvoke.DrawContours(captureFrame, processFrameContours, largestContourIndex, new MCvScalar(255, 0, 0), 3);
                VectorOfPoint contourPoints = new VectorOfPoint();
                CvInvoke.ApproxPolyDP(processFrameContours[largestContourIndex],contourPoints,CvInvoke.ArcLength(processFrameContours[largestContourIndex],true)*0.06,true);
                Point[] workspaceCorners = contourPoints.ToArray();

                if (workspaceCorners.Length == 4)
                {
                    Point[] temp = new Point[workspaceCorners.Length];
                    temp[0] = workspaceCorners[2];
                    temp[1] = workspaceCorners[3];
                    temp[2] = workspaceCorners[0];
                    temp[3] = workspaceCorners[1];
                    workspaceCorners = temp;

                    //averages the paper corners over the last five frames
                    workspaceCorners = workspaceCornersAverage(workspaceCorners);
                    for (int i = 0; i < workspaceCorners.Length; i++)
                    {
                        CvInvoke.Circle(captureFrame, workspaceCorners[i], 6, new MCvScalar(0, 255, 0), -1);
                        CvInvoke.PutText(captureFrame, i.ToString(), workspaceCorners[i], Emgu.CV.CvEnum.FontFace.HersheyPlain, 3, new MCvScalar(0, 0, 255), 2);
                    }

                    //==== Warp Paper Perspective ====
                    workspaceFrame = processFrame;
                    IEnumerable<Point> query = workspaceCorners.OrderBy(point => point.Y).ThenBy(point => point.X);
                    PointF[] ptsSrc = new PointF[4];
                    PointF[] ptsDst = new PointF[] { new PointF(0, 0), new PointF(workspaceFrame.Width-1, 0), new PointF(0, workspaceFrame.Height-1), new PointF(workspaceFrame.Width-1, workspaceFrame.Height-1) };
                    for (int i = 0; i < 4; i++)
                    {
                        ptsSrc[i] = new PointF(query.ElementAt(i).X, query.ElementAt(i).Y);
                    }

                    using (var matrix = CvInvoke.GetPerspectiveTransform(ptsSrc, ptsDst))
                    {
                        using (var cutImagePortion = new Mat())
                        {
                            CvInvoke.WarpPerspective(workspaceFrame, cutImagePortion, matrix, new Size(workspaceFrame.Width, workspaceFrame.Height), Inter.Cubic);
                            workspaceFrame = cutImagePortion.ToImage<Gray, Byte>().Flip(FlipType.Vertical).Flip(FlipType.Horizontal)/*.Rotate(180, new Gray(0),false)*/;
                            
                        }
                    }

                    //==== Detect Blobs on Warped Image ====
                    squareBlobs = squareBlobDetector.Detect(workspaceFrame);
                    triBlobs = triBlobDetector.Detect(workspaceFrame);


                    //==== Transfer Blobs To Shape Object Array ====
                    Shape[] foundShapes = new Shape[squareBlobs.Length + triBlobs.Length];

                    for (int i = 0; i<squareBlobs.Length; i++)
                    {
                        foundShapes[i].position.X = (85*squareBlobs[i].Point.X/workspaceFrame.Width);
                        foundShapes[i].position.Y = (110*squareBlobs[i].Point.Y/workspaceFrame.Height);
                        foundShapes[i].type = (int)Shape.Type.Square;
                        Point Keypoint = new Point((int)squareBlobs[i].Point.X, (int)squareBlobs[i].Point.Y);
                        CvInvoke.Circle(workspaceFrame, Keypoint, 6, new MCvScalar(150, 150, 0), -1);
                        CvInvoke.PutText(workspaceFrame, "Sq", Keypoint, Emgu.CV.CvEnum.FontFace.HersheyPlain, 2, new MCvScalar(150, 150, 0), 2);
                    }

                    for (int i = 0; i < triBlobs.Length; i++)
                    {
                        foundShapes[i+squareBlobs.Length].position.X = (85 * triBlobs[i].Point.X / workspaceFrame.Width);
                        foundShapes[i+squareBlobs.Length].position.X = (110 * triBlobs[i].Point.X / workspaceFrame.Height);
                        foundShapes[i+squareBlobs.Length].type = (int)Shape.Type.Triangle;
                        Point Keypoint = new Point((int)triBlobs[i].Point.X, (int)triBlobs[i].Point.Y);
                        CvInvoke.Circle(workspaceFrame, Keypoint, 6, new MCvScalar(150, 150, 0), -1);

                        CvInvoke.PutText(workspaceFrame, "Tri", Keypoint, Emgu.CV.CvEnum.FontFace.HersheyPlain, 2, new MCvScalar(150, 150, 0), 2);
                    }

                    //==== Sort Shapes by Order (Front to Back on Paper) ====
                    IEnumerable <Shape>  shapee = foundShapes.OrderBy(position => position.position.Y);
                    foundShapes = shapee.ToArray();

                    String output = "Shapes: ";

                    for(int i = 0; i < foundShapes.Length; i++)
                    {
                        if (foundShapes[i].type == (int)Shape.Type.Triangle)
                            output += "T";
                        else
                            output += "S";

                        output += foundShapes[i].position.Y;
                        output += " ";
                    }

                    Invoke(new Action(() =>
                    {
                        shapes = foundShapes; //this is the global shape array
                    }));

                    //==== Display the Important Images ====
                    DisplayFrames(captureFrame, processFrame, workspaceFrame);
                }
                else
                {
                    DisplayFrames(captureFrame, processFrame, processFrame);
                }

            }   
        }

        Point[] workspaceCornersAverage(Point[] workspaceCorners) //utilizes a queue to average the last 5 corners found
        {
            Point[] temp;
            Point[] average = new Point[workspaceCorners.Length];

            workspaceCornersSets.Enqueue(workspaceCorners);
            if (workspaceCornersSets.Count > workspaceCornerAverageCount)
            {
                workspaceCornersSets.Dequeue();
            }

            for(int i = 0; i < workspaceCornersSets.Count; i++)
            {
                temp = workspaceCornersSets.ElementAt(i);
                for (int j = 0; j < temp.Length; j++)
                {
                    average[j].X = average[j].X + temp[j].X;
                    average[j].Y = average[j].Y + temp[j].Y;
                }
            }
            
            for(int i = 0; i < average.Length; i++)
            {
                average[i].X /= workspaceCornerAverageCount;
                average[i].Y /= workspaceCornerAverageCount;
            }
            return average;
        }

        private void DisplayFrames(Mat captureFrame, Image<Gray,byte> processFrame, Image<Gray,byte> workspaceFrame) //displays frames to the form
        {
            CvInvoke.Resize(captureFrame,captureFrame,capturePictureBoxSize);
            capturePictureBox.Image   = captureFrame.Bitmap;
            processPictureBox.Image   = processFrame.Resize(processPictureBoxSize.Width, processPictureBoxSize.Height, Emgu.CV.CvEnum.Inter.Area).Bitmap;
            workspacePictureBox.Image = workspaceFrame.Resize(workspaceFrame.Width, workspaceFrame.Height, Emgu.CV.CvEnum.Inter.Area).Bitmap;
        }

        private void ShapeSorter_FormClosing(object sender, FormClosingEventArgs e)
        {
            _visionThread.Abort();
            _armInterfaceThread.Abort();
        }

        private void SORT_Click(object sender, EventArgs e) //when the button is clicked, begin sorting the found shapes
        {
            _armInterfaceThread.Start();
        }
    }
}
