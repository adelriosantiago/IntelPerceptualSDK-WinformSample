using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

/*
 * Important notice: When you load up the project on Visual Studio the reference to libpxcclr may not be there.
 * To solve this simply go to Project->Add reference and then search for the library (normally in C:\Program Files\Intel\PCSDK\bin\win32\libpxcclr.dll)
*/

namespace facereco_test
{
    public partial class Form1 : Form
    {
        MyPipeline pipeline;
        bool isRunning = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isRunning)
            {
                isRunning = false;
                pipeline.PauseFaceLocation(true);
                pipeline.Close();
            }
            else
            {
                isRunning = true;
                pipeline.LoopFrames();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pipeline = new MyPipeline(this, pictureBox1);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }
    }

    class MyPipeline : UtilMPipeline
    {
        //Core variables
        ulong timeStamp;
        int faceId;
        uint fidx = 0; //Unknown variable, what does this does?

        //Statuses
        pxcmStatus locationStatus;
        pxcmStatus landmarkStatus;
        pxcmStatus attributeStatus;
        public bool takeRecoSnapshot = false;

        //Form variables
        Form1 parent;
        string detectionConfidence;
        Bitmap lastProcessedBitmap;

        //Attribute array
        uint[] age_group = new uint[5];
        uint[] gender = new uint[2];
        uint[] blink = new uint[2];
        uint[] smile = new uint[1];
        
        //PXCM variables
        PXCMFaceAnalysis faceAnalysis;
        PXCMSession session;
        PXCMFaceAnalysis.Detection faceLocation;
        PXCMFaceAnalysis.Landmark faceLandmark;
        PXCMFaceAnalysis.Attribute faceAttributes;
        PXCMFaceAnalysis.Detection.Data faceLocationData;
        PXCMFaceAnalysis.Landmark.LandmarkData[] faceLandmarkData;
        PXCMFaceAnalysis.Landmark.ProfileInfo landmarkProfile;
        PXCMFaceAnalysis.Attribute.ProfileInfo attributeProfile;

        //Face data
        PictureBox recipient; //Where the image will be drawn

        public MyPipeline(Form1 parent, PictureBox recipient)
        {
            lastProcessedBitmap = new Bitmap(640, 480);            

            this.recipient = recipient;
            this.parent = parent;
                        
            attributeProfile = new PXCMFaceAnalysis.Attribute.ProfileInfo();

            EnableImage(PXCMImage.ColorFormat.COLOR_FORMAT_RGB24);
            EnableFaceLocation();
            EnableFaceLandmark();            
        }

        public override bool OnNewFrame()
        {
            faceAnalysis = QueryFace();
            faceAnalysis.QueryFace(fidx, out faceId, out timeStamp);
            
            //Get face location
            faceLocation = (PXCMFaceAnalysis.Detection)faceAnalysis.DynamicCast(PXCMFaceAnalysis.Detection.CUID);
            locationStatus = faceLocation.QueryData(faceId, out faceLocationData);
            detectionConfidence = faceLocationData.confidence.ToString();
            parent.label1.Text = "Confidence: " + detectionConfidence;

            //Get face landmarks (eye, mouth, nose position, etc)
            faceLandmark = (PXCMFaceAnalysis.Landmark)faceAnalysis.DynamicCast(PXCMFaceAnalysis.Landmark.CUID);
            faceLandmark.QueryProfile(1, out landmarkProfile);
            faceLandmark.SetProfile(ref landmarkProfile);
            faceLandmarkData = new PXCMFaceAnalysis.Landmark.LandmarkData[7];
            landmarkStatus = faceLandmark.QueryLandmarkData(faceId, PXCMFaceAnalysis.Landmark.Label.LABEL_7POINTS, faceLandmarkData);

            //Get face attributes (smile, age group, gender, eye blink, etc)
            faceAttributes = (PXCMFaceAnalysis.Attribute)faceAnalysis.DynamicCast(PXCMFaceAnalysis.Attribute.CUID);
            faceAttributes.QueryProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_EMOTION, 0, out attributeProfile);
            faceAttributes.SetProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_EMOTION, ref attributeProfile);
            attributeStatus = faceAttributes.QueryData(PXCMFaceAnalysis.Attribute.Label.LABEL_EMOTION, faceId, out smile);

            faceAttributes.QueryProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_EYE_CLOSED, 0, out attributeProfile);
            attributeProfile.threshold = 50; //Must be here!
            faceAttributes.SetProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_EYE_CLOSED, ref attributeProfile);
            attributeStatus = faceAttributes.QueryData(PXCMFaceAnalysis.Attribute.Label.LABEL_EYE_CLOSED, faceId, out blink);

            faceAttributes.QueryProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_GENDER, 0, out attributeProfile);
            faceAttributes.SetProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_GENDER, ref attributeProfile);
            attributeStatus = faceAttributes.QueryData(PXCMFaceAnalysis.Attribute.Label.LABEL_GENDER, faceId, out gender);

            faceAttributes.QueryProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_AGE_GROUP, 0, out attributeProfile);
            faceAttributes.SetProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_AGE_GROUP, ref attributeProfile);
            attributeStatus = faceAttributes.QueryData(PXCMFaceAnalysis.Attribute.Label.LABEL_AGE_GROUP, faceId, out age_group);

            ShowAttributesOnForm();
            
            //Do the application events
            try
            {
                Application.DoEvents(); //TODO: This should be avoided using a different thread, but how?
            }
            catch (AccessViolationException e)
            {
                //TODO: Handle exception!
            }
            return true;
        }

        public override void OnImage(PXCMImage image)
        {
            session = QuerySession();
            image.QueryBitmap(session, out lastProcessedBitmap);
            using (Graphics drawer = Graphics.FromImage(lastProcessedBitmap))
            {
                if (locationStatus != pxcmStatus.PXCM_STATUS_ITEM_UNAVAILABLE)
                {
                    drawer.DrawRectangle(new Pen(new SolidBrush(Color.Red), 1), new Rectangle(new Point((int)faceLocationData.rectangle.x, (int)faceLocationData.rectangle.y), new Size((int)faceLocationData.rectangle.w, (int)faceLocationData.rectangle.h)));
                }

                if (landmarkStatus != pxcmStatus.PXCM_STATUS_ITEM_UNAVAILABLE && faceLandmarkData != null)
                {
                    drawer.DrawLine(new Pen(new SolidBrush(Color.Red), 1), new Point((int)faceLandmarkData[0].position.x, (int)faceLandmarkData[0].position.y), new Point((int)faceLandmarkData[1].position.x, (int)faceLandmarkData[1].position.y));
                    drawer.DrawLine(new Pen(new SolidBrush(Color.Red), 1), new Point((int)faceLandmarkData[2].position.x, (int)faceLandmarkData[2].position.y), new Point((int)faceLandmarkData[3].position.x, (int)faceLandmarkData[3].position.y));
                    drawer.DrawLine(new Pen(new SolidBrush(Color.Red), 1), new Point((int)faceLandmarkData[4].position.x, (int)faceLandmarkData[4].position.y), new Point((int)faceLandmarkData[5].position.x, (int)faceLandmarkData[5].position.y));
                    drawer.DrawRectangle(new Pen(new SolidBrush(Color.Red), 1), new Rectangle((int)faceLandmarkData[6].position.x - 2, (int)faceLandmarkData[6].position.y - 2, 4, 4));
                }
            }

            //Show main image
            recipient.Image = lastProcessedBitmap;
        }

        private void ShowAttributesOnForm()
        {
            if (blink[0] == 100)
            {
                parent.checkBox1.Checked = true;
            }
            else
            {
                parent.checkBox1.Checked = false;
            }

            if (smile[0] == 100)
            {
                parent.checkBox2.Checked = true;
            }
            else
            {
                parent.checkBox2.Checked = false;
            }

            if (gender[0] == 100)
            {
                parent.label2.Text = "Male";
            }
            else
            {
                parent.label2.Text = "Female";
            }

            parent.trackBar1.Value = (int)age_group[0];
            parent.trackBar2.Value = (int)age_group[1];
            parent.trackBar3.Value = (int)age_group[2];
            parent.trackBar4.Value = (int)age_group[3];
            parent.trackBar5.Value = (int)age_group[4];
        }
    }
}
