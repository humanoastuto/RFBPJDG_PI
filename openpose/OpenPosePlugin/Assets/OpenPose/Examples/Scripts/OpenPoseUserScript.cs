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

        //Created for us
        [SerializeField] LineRenderer humanTorso;
        [SerializeField] LineRenderer humanArms;
        [SerializeField] LineRenderer humanLegs;
        [SerializeField] LineRenderer humanGlasses;
        [SerializeField] LineRenderer copyTorso;
        [SerializeField] LineRenderer copyArms;
        [SerializeField] LineRenderer copyLegs;
        [SerializeField] LineRenderer copyGlasses;
        [SerializeField] Text scoreText;
        [SerializeField] Text timerText;
        [SerializeField] Button start;
        private List<(float, float)> humancopy = new List<(float, float)>();
        private string[] poses = null;
        private int npose;
        private float timer;
        private float timeToStart;
        private float timeStarted;
        private float timeToSkip;
        private float timeNextSkip;
        
        private List<bool> points;

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
        public bool renderBgImg = false; //Start by default
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

        private void LoadMap() {
            poses = System.IO.File.ReadAllLines(@".\Assets\Levels\prueba.txt");
            npose = 0;
            timer = 0;
            timeNextSkip = float.MaxValue;
            timeToStart = 3;
            timeToSkip = 1;
            timeStarted = 0;
            points = new List<bool>();
            for (int i = 0; i < poses.Length; i++) {
                points.Add(false);
            }
        }

        private void EnableCameraRender()
        {
            ToggleRenderBgImg();
            ToggleRenderBgImg();
        }

        private void CustomInitialConfig()
        {
            EnableCameraRender();
        }


        private void Start() {
            //StartCoroutine(ExampleCoroutine());

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

            CustomInitialConfig();

            // Start OpenPose
            OPWrapper.OPRun();
           
            LoadMap();
     
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

        private float Distance(Vector2 p1, Vector2 p2)
        {
            return (float)System.Math.Sqrt(System.Math.Pow((p2.x - p1.x), 2) + System.Math.Pow((p2.y - p1.y), 2));
        }

        private float Distance(float x1, float y1, float x2, float y2) {
            return (float)System.Math.Sqrt(System.Math.Pow((x1 - x2), 2) + System.Math.Pow((y1 - y2), 2));
        }

        public void ChangePose() {
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

        private void Compare(List<RectTransform> human1, List<(float, float)> human2)
        {
            float dif = 0;
            if (human1.Count > 0)
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
                if (dif < 1000) {
                    points[npose] = true;
                    if (dif < 600) {
                        scoreText.text = "perfecto";
                    } else {
                        scoreText.text = "bien";
                    }
                } else {
                    scoreText.text = "mal";
                }
                //scoreText.text = CountPoints();
                
            }
            else
            {
                scoreText.text = "NADA QUE COMPARAR";
            }
        }

        private void DrawCopy() {
            float x = 700;
            float y = 50;
            float z = 3;

            copyTorso.SetPosition(0, new Vector3(x - humancopy[0].Item1 / z, y - humancopy[0].Item2 / z, -1));
            copyTorso.SetPosition(1, new Vector3(x - humancopy[1].Item1 / z, y - humancopy[1].Item2 / z, -1));
            copyTorso.SetPosition(2, new Vector3(x - humancopy[8].Item1 / z, y - humancopy[8].Item2 / z, -1));

            copyArms.SetPosition(0, new Vector3(x - humancopy[4].Item1 / z, y - humancopy[4].Item2 / z, -1));
            copyArms.SetPosition(1, new Vector3(x - humancopy[3].Item1 / z, y - humancopy[3].Item2 / z, -1));
            copyArms.SetPosition(2, new Vector3(x - humancopy[2].Item1 / z, y - humancopy[2].Item2 / z, -1));
            copyArms.SetPosition(3, new Vector3(x - humancopy[1].Item1 / z, y - humancopy[1].Item2 / z, -1));
            copyArms.SetPosition(4, new Vector3(x - humancopy[5].Item1 / z, y - humancopy[5].Item2 / z, -1));
            copyArms.SetPosition(5, new Vector3(x - humancopy[6].Item1 / z, y - humancopy[6].Item2 / z, -1));
            copyArms.SetPosition(6, new Vector3(x - humancopy[7].Item1 / z, y - humancopy[7].Item2 / z, -1));
            
            copyLegs.SetPosition(0, new Vector3(x - humancopy[23].Item1 / z, y - humancopy[23].Item2 / z, -1));
            copyLegs.SetPosition(1, new Vector3(x - humancopy[11].Item1 / z, y - humancopy[11].Item2 / z, -1));
            copyLegs.SetPosition(2, new Vector3(x - humancopy[10].Item1 / z, y - humancopy[10].Item2 / z, -1));
            copyLegs.SetPosition(3, new Vector3(x - humancopy[9].Item1 / z, y - humancopy[9].Item2 / z, -1));
            copyLegs.SetPosition(4, new Vector3(x - humancopy[8].Item1 / z, y - humancopy[8].Item2 / z, -1));
            copyLegs.SetPosition(5, new Vector3(x - humancopy[12].Item1 / z, y - humancopy[12].Item2 / z, -1));
            copyLegs.SetPosition(6, new Vector3(x - humancopy[13].Item1 / z, y - humancopy[13].Item2 / z, -1));
            copyLegs.SetPosition(7, new Vector3(x - humancopy[14].Item1 / z, y - humancopy[14].Item2 / z, -1));
            copyLegs.SetPosition(8, new Vector3(x - humancopy[20].Item1 / z, y - humancopy[20].Item2 / z, -1));
            
            copyGlasses.SetPosition(0, new Vector3(x - humancopy[17].Item1 / z, y - humancopy[17].Item2 / z, -1));
            copyGlasses.SetPosition(1, new Vector3(x - humancopy[15].Item1 / z, y - humancopy[15].Item2 / z, -1));
            copyGlasses.SetPosition(2, new Vector3(x - humancopy[0].Item1 / z, y - humancopy[0].Item2 / z, -1));
            copyGlasses.SetPosition(3, new Vector3(x - humancopy[16].Item1 / z, y - humancopy[16].Item2 / z, -1));
            copyGlasses.SetPosition(4, new Vector3(x - humancopy[18].Item1 / z, y - humancopy[18].Item2 / z, -1));
        }

        public void StartGame() {
            timeStarted = timer;
            timeNextSkip = timer + timeToStart;
        }

        private string CountPoints() {
            int n = 0;
            for (int i = 0; i < poses.Length; i++) {
                if(points[i]) {
                    n++;
                }
            }
            if(n * 3 < poses.Length) {
                return "das asco" ;
            } else if (n * 3 / 2 < poses.Length) {
                return "bien, sigue adelante";
            } else {
                return "eres un pvto dios";
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
                Vector2 screenSize = Camera.main.pixelRect.size;
                float scale = Mathf.Min(outputSize.x / (screenSize.x * 2), outputSize.y / (screenSize.y * 2));
                //float scale = Mathf.Min(screenSize.x / outputSize.x, screenSize.y / outputSize.y);
                outputTransform.localScale = new Vector3(scale, scale, scale);

                // Update number of people in UI
                if (datum.poseKeypoints == null || datum.poseKeypoints.Empty()) numberPeople = 0;
                else numberPeople = datum.poseKeypoints.GetSize(0);
                peopleText.text = "People: " + numberPeople;

                // Draw human
                while (humanContainer.childCount < numberPeople) { // Make sure no. of HumanControllers no less than numberPeople
                    Instantiate(humanPrefab, humanContainer);
                }
                int i = 0;
                foreach (var human in humanContainer.GetComponentsInChildren<HumanController2D>()) {
                    // When i >= no. of human, the human will be hidden
                    human.DrawHuman(ref datum, i++, renderThreshold);
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

            timer += Time.deltaTime;
            if (timer > timeNextSkip) {
                timeNextSkip += timeToSkip;
                ChangePose();
            }
            if (timeStarted > 0 ) {
                if (timer - timeStarted - timeToStart > 0) {
                    
                    if (humanContainer.GetComponentsInChildren<HumanController2D>().Length > 0) {
                        Compare(humanContainer.GetComponentsInChildren<HumanController2D>()[0].getPoseJoints(), humancopy);
                    }
                }
            }
            if (humancopy.Count == 25)
            {
                DrawCopy();
            }
            
            if (npose == poses.Length) {
                timeStarted = 0;
                scoreText.text = CountPoints();
            }
        }
    }
}

//Debug.Log("X1 " + screenSize.x + " Y1 " + screenSize.y);
//Debug.Log("X2 " + outputSize.x + " Y2 " + outputSize.y);
//float scale = Mathf.Min(720 / outputSize.x, 480/ outputSize.y);
//float scalex = 368 / outputSize.x;
//float scaley = 207 / outputSize.y;
//outputTransform.localScale = new Vector3(scalex, scaley, scale);

                    //if (human.getPoseJoints().Count > 0) {
                        //DrawHuman(human.getPoseJoints());
                    //}
                    //Compare(human.getPoseJoints(), humancopy);

/*        private void DrawHuman(List<RectTransform> poseJoints) {
            float x = -100;
            float y = 50;
            float z = (float) 0.1;

            humanTorso.SetPosition(0, new Vector3(x - poseJoints[0].anchoredPosition.x / z, y - poseJoints[0].anchoredPosition.y / z, -1));
            humanTorso.SetPosition(1, new Vector3(x - poseJoints[1].anchoredPosition.x / z, y - poseJoints[1].anchoredPosition.y / z, -1));
            humanTorso.SetPosition(2, new Vector3(x - poseJoints[8].anchoredPosition.x / z, y - poseJoints[8].anchoredPosition.y / z, -1));

            humanArms.SetPosition(0, new Vector3(x - poseJoints[4].anchoredPosition.x / z, y - poseJoints[4].anchoredPosition.y / z, -1));
            humanArms.SetPosition(1, new Vector3(x - poseJoints[3].anchoredPosition.x / z, y - poseJoints[3].anchoredPosition.y / z, -1));
            humanArms.SetPosition(2, new Vector3(x - poseJoints[2].anchoredPosition.x / z, y - poseJoints[2].anchoredPosition.y / z, -1));
            humanArms.SetPosition(3, new Vector3(x - poseJoints[1].anchoredPosition.x / z, y - poseJoints[1].anchoredPosition.y / z, -1));
            humanArms.SetPosition(4, new Vector3(x - poseJoints[5].anchoredPosition.x / z, y - poseJoints[5].anchoredPosition.y / z, -1));
            humanArms.SetPosition(5, new Vector3(x - poseJoints[6].anchoredPosition.x / z, y - poseJoints[6].anchoredPosition.y / z, -1));
            humanArms.SetPosition(6, new Vector3(x - poseJoints[7].anchoredPosition.x / z, y - poseJoints[7].anchoredPosition.y / z, -1));
            
            humanLegs.SetPosition(0, new Vector3(x - poseJoints[23].anchoredPosition.x / z, y - poseJoints[23].anchoredPosition.y / z, -1));
            humanLegs.SetPosition(1, new Vector3(x - poseJoints[11].anchoredPosition.x / z, y - poseJoints[11].anchoredPosition.y / z, -1));
            humanLegs.SetPosition(2, new Vector3(x - poseJoints[10].anchoredPosition.x / z, y - poseJoints[10].anchoredPosition.y / z, -1));
            humanLegs.SetPosition(3, new Vector3(x - poseJoints[9].anchoredPosition.x / z, y - poseJoints[9].anchoredPosition.y / z, -1));
            humanLegs.SetPosition(4, new Vector3(x - poseJoints[8].anchoredPosition.x / z, y - poseJoints[8].anchoredPosition.y / z, -1));
            humanLegs.SetPosition(5, new Vector3(x - poseJoints[12].anchoredPosition.x / z, y - poseJoints[12].anchoredPosition.y / z, -1));
            humanLegs.SetPosition(6, new Vector3(x - poseJoints[13].anchoredPosition.x / z, y - poseJoints[13].anchoredPosition.y / z, -1));
            humanLegs.SetPosition(7, new Vector3(x - poseJoints[14].anchoredPosition.x / z, y - poseJoints[14].anchoredPosition.y / z, -1));
            humanLegs.SetPosition(8, new Vector3(x - poseJoints[20].anchoredPosition.x / z, y - poseJoints[20].anchoredPosition.y / z, -1));
            
            humanGlasses.SetPosition(0, new Vector3(x - poseJoints[17].anchoredPosition.x / z, y - poseJoints[17].anchoredPosition.y / z, -1));
            humanGlasses.SetPosition(1, new Vector3(x - poseJoints[15].anchoredPosition.x / z, y - poseJoints[15].anchoredPosition.y / z, -1));
            humanGlasses.SetPosition(2, new Vector3(x - poseJoints[0].anchoredPosition.x / z, y - poseJoints[0].anchoredPosition.y / z, -1));
            humanGlasses.SetPosition(3, new Vector3(x - poseJoints[16].anchoredPosition.x / z, y - poseJoints[16].anchoredPosition.y / z, -1));
            humanGlasses.SetPosition(4, new Vector3(x - poseJoints[18].anchoredPosition.x / z, y - poseJoints[18].anchoredPosition.y / z, -1));
        }*/