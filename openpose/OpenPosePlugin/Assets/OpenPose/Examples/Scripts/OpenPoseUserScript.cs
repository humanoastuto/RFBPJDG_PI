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

        //I don't know what is this
        //HumanController2D humancopy = null;
        //List<RectTransform> humancopy = null;
        [SerializeField] RectTransform outputTransform2;
        [SerializeField] LineRenderer linea1;
        [SerializeField] LineRenderer linea2;
        [SerializeField] LineRenderer linea3;
        [SerializeField] LineRenderer linea4;
        List<(float, float)> humancopy = new List<(float, float)>();
        string[] poses = null;
        int npose;
        float timer;
        float capturado;

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

            poses = System.IO.File.ReadAllLines(@"C:\Users\bramo\Desktop\RFBPJDG_PI\openpose\OpenPosePlugin\Assets\Media\prueba.txt");
            npose = 0;
            //Capture();
            timer = 0;
            capturado = (float) 0.5;
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
            //humancopy.Clear();
            //foreach (RectTransform rectTransform in humanContainer.GetComponentsInChildren<HumanController2D>()[0].poseJoints)
            //{
            //    humancopy.Add((rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y));
            //}
            //string pose = "";
            //float column = Distance(humancopy[1].Item1, humancopy[1].Item2, humancopy[8].Item1, humancopy[8].Item2);
            //float column2 = 300 / column;
            //for (int i = 0; i < 25; i++)
            //{
            //    pose += (humancopy[i].Item1 * column2).ToString() + " " + (humancopy[i].Item2 * column2).ToString() + " ";
            //}
            //using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\bramo\Desktop\RFBPJDG_PI\openpose\OpenPosePlugin\Assets\Media\prueba.txt", true)) {
            //    file.WriteLine(pose);
            //}
            
            if (npose < poses.Length)
            {
                humancopy.Clear();
                string[] pose = poses[npose].Split(' ');
                for (int i = 0; i < 25; i++)
                {
                    humancopy.Add((float.Parse(pose[i * 2]), float.Parse(pose[i * 2 + 1])));
                }
                npose++;
            }
        }       
        private float Distance(Vector2 p1, Vector2 p2)
        {
            return (float)System.Math.Sqrt(System.Math.Pow((p2.x - p1.x), 2) + System.Math.Pow((p2.y - p1.y), 2));
        }

        private float Distance(float x1, float y1, float x2, float y2)
        {
            return (float)System.Math.Sqrt(System.Math.Pow((x1 - x2), 2) + System.Math.Pow((y1 - y2), 2));
        }

        private string Difference(List<RectTransform> human1, List<(float, float)> human2)
        {
            float dif = 0;
            if (human2.Count > 0)
            {
                float col1 = 1;
                float col2 = 1;
                if (human1[1].anchoredPosition.x > 0 && human1[8].anchoredPosition.y > 0 && human2[1].Item1 > 0 && human2[8].Item1 > 0)
                {
                    col1 = Distance(human1[1].anchoredPosition, human1[8].anchoredPosition);
                    col2 = Distance(human2[1].Item1, human2[1].Item2, human2[8].Item1, human2[8].Item2);
                }
                float tam = col2 / col1;
                float difx = human1[1].anchoredPosition.x * tam - human2[1].Item1;
                float dify = human1[1].anchoredPosition.y * tam - human2[1].Item2;
                for (int i = 0; i < 25; i++)
                {
                    if (human2[i].Item1 > 0 && human2[i].Item2 > 0)
                    {
                        dif += System.Math.Abs(human1[i].anchoredPosition.x * tam - human2[i].Item1 - difx);
                        dif += System.Math.Abs(human1[i].anchoredPosition.y * tam - human2[i].Item2 - dify);
                    }
                }
                if (dif < 1000)
                {
                    Capture();
                }
                return (int)dif + " DIF";
            }
            else
            {
                return "NADA QUE COMPARAR";
            }
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
                Vector2 outputSize2 = outputTransform2.sizeDelta;
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
                    Debug.Log(linea1.positionCount);

                    float x = 225;
                    float y = 75;
                    float z = 7;
                    //This draws the esquelet to copy
                    if (humancopy.Count > 24)
                    {
                        linea1.SetPosition(0, new Vector3(x - humancopy[0].Item1 / z, y - humancopy[0].Item2 / z, -1));
                        linea1.SetPosition(1, new Vector3(x - humancopy[1].Item1 / z, y - humancopy[1].Item2 / z, -1));
                        linea1.SetPosition(2, new Vector3(x - humancopy[8].Item1 / z, y - humancopy[8].Item2 / z, -1));
                        linea2.SetPosition(0, new Vector3(x - humancopy[4].Item1 / z, y - humancopy[4].Item2 / z, -1));
                        linea2.SetPosition(1, new Vector3(x - humancopy[3].Item1 / z, y - humancopy[3].Item2 / z, -1));
                        linea2.SetPosition(2, new Vector3(x - humancopy[2].Item1 / z, y - humancopy[2].Item2 / z, -1));
                        linea2.SetPosition(3, new Vector3(x - humancopy[1].Item1 / z, y - humancopy[1].Item2 / z, -1));
                        linea2.SetPosition(4, new Vector3(x - humancopy[5].Item1 / z, y - humancopy[5].Item2 / z, -1));
                        linea2.SetPosition(5, new Vector3(x - humancopy[6].Item1 / z, y - humancopy[6].Item2 / z, -1));
                        linea2.SetPosition(6, new Vector3(x - humancopy[7].Item1 / z, y - humancopy[7].Item2 / z, -1));
                        linea3.SetPosition(0, new Vector3(x - humancopy[23].Item1 / z, y - humancopy[23].Item2 / z, -1));
                        linea3.SetPosition(1, new Vector3(x - humancopy[11].Item1 / z, y - humancopy[11].Item2 / z, -1));
                        linea3.SetPosition(2, new Vector3(x - humancopy[10].Item1 / z, y - humancopy[10].Item2 / z, -1));
                        linea3.SetPosition(3, new Vector3(x - humancopy[9].Item1 / z, y - humancopy[9].Item2 / z, -1));
                        linea3.SetPosition(4, new Vector3(x - humancopy[8].Item1 / z, y - humancopy[8].Item2 / z, -1));
                        linea3.SetPosition(5, new Vector3(x - humancopy[12].Item1 / z, y - humancopy[12].Item2 / z, -1));
                        linea3.SetPosition(6, new Vector3(x - humancopy[13].Item1 / z, y - humancopy[13].Item2 / z, -1));
                        linea3.SetPosition(7, new Vector3(x - humancopy[14].Item1 / z, y - humancopy[14].Item2 / z, -1));
                        linea3.SetPosition(8, new Vector3(x - humancopy[20].Item1 / z, y - humancopy[20].Item2 / z, -1));
                        linea4.SetPosition(0, new Vector3(x - humancopy[17].Item1 / z, y - humancopy[17].Item2 / z, -1));
                        linea4.SetPosition(1, new Vector3(x - humancopy[15].Item1 / z, y - humancopy[15].Item2 / z, -1));
                        linea4.SetPosition(2, new Vector3(x - humancopy[0].Item1 / z, y - humancopy[0].Item2 / z, -1));
                        linea4.SetPosition(3, new Vector3(x - humancopy[16].Item1 / z, y - humancopy[16].Item2 / z, -1));
                        linea4.SetPosition(4, new Vector3(x - humancopy[18].Item1 / z, y - humancopy[18].Item2 / z, -1));
                    }
                    difText.text = Difference(human.poseJoints, humancopy);
                    //timer += Time.deltaTime;
                    //difText.text = timer.ToString();
                    //if (0.5 < timer && timer < 2 && timer > capturado) {
                    //    Capture();
                    //    capturado += (float) 0.05;
                    //}
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
