// OpenPose Unity Plugin v1.0.0alpha-1.5.0
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenPose.Example {
    /*
     * User example of using OPWrapper
     */
    public class OpenPoseUserScript : MonoBehaviour {

        // HumanController2D prefab
        [SerializeField] GameObject humanPrefab;

        // UI elements
        [SerializeField] RectTransform outputTransform;
        [SerializeField] ImageRenderer bgImageRenderer;
        [SerializeField] Transform humanContainer;
        [SerializeField] Text fpsText;
        [SerializeField] Text peopleText;
        [SerializeField] Text stateText;
        [SerializeField] Text difText;
        [SerializeField] Button captureButton;

        // Output
        private OPDatum datum;

        // OpenPose settings
        public ProducerType inputType = ProducerType.Webcam;
        public string producerString = "-1";
        public int maxPeople = -1;
        public float renderThreshold = 0.05f;
        public bool
            handEnabled = false,
            faceEnabled = false;
        public Vector2Int
            netResolution = new Vector2Int(-1, 368),
            handResolution = new Vector2Int(368, 368),
            faceResolution = new Vector2Int(368, 368);
        public void SetHandEnabled(bool enabled) { handEnabled = enabled; }
        public void SetFaceEnabled(bool enabled) { faceEnabled = enabled; }
        public void SetRenderThreshold(string s){float res; if (float.TryParse(s, out res)){renderThreshold = res;};}
        public void SetMaxPeople(string s){int res; if (int.TryParse(s, out res)){maxPeople = res;};}
        public void SetPoseResX(string s){int res; if (int.TryParse(s, out res)){netResolution.x = res;};}
        public void SetPoseResY(string s){int res; if (int.TryParse(s, out res)){netResolution.y = res;};}
        public void SetHandResX(string s){int res; if (int.TryParse(s, out res)){handResolution.x = res;};}
        public void SetHandResY(string s){int res; if (int.TryParse(s, out res)){handResolution.y = res;};}
        public void SetFaceResX(string s){int res; if (int.TryParse(s, out res)){faceResolution.x = res;};}
        public void SetFaceResY(string s){int res; if (int.TryParse(s, out res)){faceResolution.y = res;};}

        //I don't know what is this
        //HumanController2D humancopy = null;
        //List<RectTransform> humancopy = null;
        List<(float, float)> humancopy = new List<(float, float)>();

        public void ApplyChanges(){
            // Restart OpenPose
            StartCoroutine(UserRebootOpenPoseCoroutine());
        }

        // Bg image
        public bool renderBgImg = false;
        public void ToggleRenderBgImg(){
            renderBgImg = !renderBgImg;
            bgImageRenderer.FadeInOut(renderBgImg);
        }

        // Number of people
        int numberPeople = 0;

        // Frame rate calculation
        private int queueMaxCount = 20; 
        private Queue<float> frameTimeQueue = new Queue<float>();
        private float avgFrameRate = 0f;
        private int frameCounter = 0;

        private void Start() {
            // Register callbacks
            OPWrapper.OPRegisterCallbacks();
            // Enable OpenPose log to unity (default true)
            OPWrapper.OPEnableDebug(true);
            // Enable OpenPose output to unity (default true)
            OPWrapper.OPEnableOutput(true);
            // Enable receiving image (default false)
            OPWrapper.OPEnableImageOutput(true);

            // Configure OpenPose with default value, or using specific configuration for each
            /* OPWrapper.OPConfigureAllInDefault(); */
            UserConfigureOpenPose();

            // Start OpenPose
            OPWrapper.OPRun();
        }

        // Parameters can be set here
        private void UserConfigureOpenPose(){
            OPWrapper.OPConfigurePose(
                /* poseMode */ PoseMode.Enabled, /* netInputSize */ netResolution, /* outputSize */ null,
                /* keypointScaleMode */ ScaleMode.InputResolution,
                /* gpuNumber */ -1, /* gpuNumberStart */ 0, /* scalesNumber */ 1, /* scaleGap */ 0.25f,
                /* renderMode */ RenderMode.Auto, /* poseModel */ PoseModel.BODY_25,
                /* blendOriginalFrame */ true, /* alphaKeypoint */ 0.6f, /* alphaHeatMap */ 0.7f,
                /* defaultPartToRender */ 0, /* modelFolder */ null,
                /* heatMapTypes */ HeatMapType.None, /* heatMapScaleMode */ ScaleMode.ZeroToOne,
                /* addPartCandidates */ false, /* renderThreshold */ renderThreshold, /* numberPeopleMax */ maxPeople,
                /* maximizePositives */ false, /* fpsMax fps_max */ -1.0,
                /* protoTxtPath */ "", /* caffeModelPath */ "", /* upsamplingRatio */ 0f);

            OPWrapper.OPConfigureHand(
                /* enable */ handEnabled, /* detector */ Detector.Body, /* netInputSize */ handResolution,
                /* scalesNumber */ 1, /* scaleRange */ 0.4f, /* renderMode */ RenderMode.Auto,
                /* alphaKeypoint */ 0.6f, /* alphaHeatMap */ 0.7f, /* renderThreshold */ 0.2f);

            OPWrapper.OPConfigureFace(
                /* enable */ faceEnabled, /* detector */ Detector.Body, 
                /* netInputSize */ faceResolution, /* renderMode */ RenderMode.Auto,
                /* alphaKeypoint */ 0.6f, /* alphaHeatMap */ 0.7f, /* renderThreshold */ 0.4f);

            OPWrapper.OPConfigureExtra(
                /* reconstruct3d */ false, /* minViews3d */ -1, /* identification */ false, /* tracking */ -1,
                /* ikThreads */ 0);

            OPWrapper.OPConfigureInput(
                /* producerType */ inputType, /* producerString */ producerString,
                /* frameFirst */ 0, /* frameStep */ 1, /* frameLast */ ulong.MaxValue,
                /* realTimeProcessing */ false, /* frameFlip */ false,
                /* frameRotate */ 0, /* framesRepeat */ false,
                /* cameraResolution */ null, /* cameraParameterPath */ null,
                /* undistortImage */ false, /* numberViews */ -1);

            OPWrapper.OPConfigureOutput(
                /* verbose */ -1.0, /* writeKeypoint */ "", /* writeKeypointFormat */ DataFormat.Xml,
                /* writeJson */ "", /* writeCocoJson */ "", /* writeCocoJsonVariants */ 1,
                /* writeCocoJsonVariant */ 1, /* writeImages */ "", /* writeImagesFormat */ "png",
                /* writeVideo */ "", /* writeVideoFps */ -1.0, /* writeVideoWithAudio */ false,
                /* writeHeatMaps */ "", /* writeHeatMapsFormat */ "png", /* writeVideo3D */ "",
                /* writeVideoAdam */ "", /* writeBvh */ "", /* udpHost */ "", /* udpPort */ "8051");

            OPWrapper.OPConfigureGui(
                /* displayMode */ DisplayMode.NoDisplay, /* guiVerbose */ false, /* fullScreen */ false);
            
            OPWrapper.OPConfigureDebugging(
                /* loggingLevel */ Priority.High, /* disableMultiThread */ false, /* profileSpeed */ 1000);
        }

        private IEnumerator UserRebootOpenPoseCoroutine() {
            if (OPWrapper.state == OPState.None) yield break;
            // Shutdown if running
            if (OPWrapper.state == OPState.Running) {
                OPWrapper.OPShutdown();
                // Reset framerate calculator
                frameTimeQueue.Clear();
                frameCounter = 0;
            }
            // Wait until fully stopped
            yield return new WaitUntil( ()=>{ return OPWrapper.state == OPState.Ready; } );
            // Configure and start
            UserConfigureOpenPose();
            OPWrapper.OPRun();
        }

        public void Capture()
        {
            humancopy.Clear();
            foreach (RectTransform rectTransform in humanContainer.GetComponentsInChildren<HumanController2D>()[0].poseJoints)
            {
                Debug.Log("P00P - " + rectTransform.anchoredPosition.x);
                humancopy.Add((rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y));
            }
        }

        private float Distance(Vector2 p1, Vector2 p2)
        {
            return (float)System.Math.Sqrt(System.Math.Pow((p2.x - p1.x), 2) + System.Math.Pow((p2.y - p1.y), 2));
        }

        private string Difference(List<RectTransform> human1, List<(float, float)> human2)
        {
            float dif = 0;
            if (human2.Count > 0)
            {
                float difx = human1[1].anchoredPosition.x - human2[1].Item1;
                float dify = human1[1].anchoredPosition.y - human2[1].Item2;
                float col1 = Distance(human1[1].anchoredPosition, human1[8].anchoredPosition);
                float col2 = Distance(new Vector2(human2[1].Item1, human2[1].Item2), new Vector2(human2[8].Item1, human2[8].Item2));
                float tam = col2 / col1;
                if (col1 > 0)
                {
                    for (int i = 0; i < 25; i++)
                    {
                        if (human2[i].Item1 + human2[i].Item2 > 0)
                        {
                            dif += System.Math.Abs(human1[i].anchoredPosition.x * tam - human2[i].Item1 + difx);
                            dif += System.Math.Abs(human1[i].anchoredPosition.y * tam - human2[i].Item2 + dify);
                        }
                    }
                }
            }
            return dif + " DIF";
        }

        private void Update() {
            // Update state in UI
            stateText.text = OPWrapper.state.ToString();

            // Try getting new frame
            if (OPWrapper.OPGetOutput(out datum)){ // true: has new frame data

                // Update background image
                bgImageRenderer.UpdateImage(datum.cvInputData);

                // Rescale output UI
                Vector2 outputSize = outputTransform.sizeDelta;
                Vector2 screenSize = Camera.main.pixelRect.size;
                //Debug.Log("X1 " + screenSize.x + " Y1 " + screenSize.y);
                //Debug.Log("X2 " + outputSize.x + " Y2 " + outputSize.y);
                //float scale = Mathf.Min(screenSize.x / outputSize.x, screenSize.y / outputSize.y);
                float scale = Mathf.Min(720 / outputSize.x, 480/ outputSize.y);
                float scalex = 480 / outputSize.x;
                float scaley = 270 / outputSize.y;
                outputTransform.localScale = new Vector3(scalex, scaley, scale);

                // Update number of people in UI
                if (datum.poseKeypoints == null || datum.poseKeypoints.Empty()) numberPeople = 0;
                else numberPeople = datum.poseKeypoints.GetSize(0);
                peopleText.text = "People: " + numberPeople;

                // Draw human
                while (humanContainer.childCount < numberPeople) { // Make sure no. of HumanControllers no less than numberPeople
                    Instantiate(humanPrefab, humanContainer);
                }
                int i = 0;
                //float j = 0;
                foreach (var human in humanContainer.GetComponentsInChildren<HumanController2D>()) {
                    // When i >= no. of human, the human will be hidden
                    human.DrawHuman(ref datum, i++, renderThreshold);
                    //difText.text = human.poseJoints[4].anchoredPosition.x + " - " + human.poseJoints[4].anchoredPosition.y;
                    difText.text = Difference(human.poseJoints, humancopy);
                }

                // Update framerate in UI
                frameTimeQueue.Enqueue(Time.time);
                frameCounter++;
                if (frameTimeQueue.Count > queueMaxCount){ // overflow
                    frameTimeQueue.Dequeue();
                }
                if (frameCounter >= queueMaxCount || frameTimeQueue.Count <= 5){ // update frame rate
                    frameCounter = 0;
                    avgFrameRate = frameTimeQueue.Count / (Time.time - frameTimeQueue.Peek());
                    fpsText.text = avgFrameRate.ToString("F1") + " FPS";
                }
            }
        }
    }
}

/*private string Difference(List<RectTransform> human1, List<RectTransform> human2)
{
    float dif = 0;
    if (human2 != null)
    {
        float difx = human1[1].anchoredPosition.x - human2[1].anchoredPosition.x;
        float dify = human1[1].anchoredPosition.y - human2[1].anchoredPosition.y;
        float col1 = Distance(human1[1].anchoredPosition, human1[8].anchoredPosition);
        float col2 = Distance(human2[1].anchoredPosition, human2[8].anchoredPosition);
        float tam = col2 / col1;
        if (col1 > 0)
        {
            for (int i = 0; i < 25; i++)
            {
                if (human1[i].anchoredPosition.x + human1[i].anchoredPosition.y + human2[i].anchoredPosition.x + human2[i].anchoredPosition.y != 0)
                {
                    dif += System.Math.Abs(human1[i].anchoredPosition.x * tam - human2[i].anchoredPosition.x + difx);
                    dif += System.Math.Abs(human1[i].anchoredPosition.y * tam - human2[i].anchoredPosition.y + dify);
                }
            }
        }
    }
    return dif + " DIF";
}*/


//j += human.poseJoints[5].anchoredPosition.y;
//j += human.poseJoints[6].anchoredPosition.y;
//j += System.Math.Abs(human.poseJoints[6].anchoredPosition.y - human.poseJoints[5].anchoredPosition.y);
//j += System.Math.Abs(human.poseJoints[7].anchoredPosition.x - human.poseJoints[6].anchoredPosition.x);
//j += System.Math.Abs(human.poseJoints[3].anchoredPosition.y - human.poseJoints[2].anchoredPosition.y);
//j += System.Math.Abs(human.poseJoints[4].anchoredPosition.x - human.poseJoints[3].anchoredPosition.x);
//j += System.Math.Abs(human.poseJoints[13].anchoredPosition.y - human.poseJoints[12].anchoredPosition.y);
//difText.text = j.ToString();
//difText.text = human.poseJoints[4].anchoredPosition.x.ToString() + "-" + human.poseJoints[4].anchoredPosition.y.ToString();
//difText.text = Difference(human, humancopy);
//difText.text = Difference(human.poseJoints, humancopy);
//if (!humancopy[0].Equals(null))
//{
//    difText.text = humancopy[0].Item1 + " - " + humancopy[0].Item2;
//}

/*private RectTransform Clone(RectTransform rectTransform)
{
    RectTransform newRectTransform = new RectTransform();
    newRectTransform.anchoredPosition = rectTransform.anchoredPosition;
    return newRectTransform;
}*/