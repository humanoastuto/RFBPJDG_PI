// OpenPose Unity Plugin v1.0.0alpha-1.5.0
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace OpenPose.Example
{
    /*
     * User example of using OPWrapper
     */
    public class OpenPoseUserScript2 : MonoBehaviour
    {

        // HumanController2D prefab
        [SerializeField] GameObject humanPrefab;

        [SerializeField] Canvas canvas;

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
        [SerializeField] Toggle typeCapture;
        [SerializeField] Dropdown videos;
        [SerializeField] Dropdown timer2;
        [SerializeField] Button exitbttn;
        [SerializeField] Button startbttn;
        [SerializeField] InputField enterTimeToRedord;
        [SerializeField] InputField enterTimeToStart;
        private List<(float, float)> humancopy = new List<(float, float)>();
        private string[] poses = null;
        private int npose;
        private float timer;
        private float timeNextCapture;
        private float timeToStart;
        private float timeToRecord;
        private float timeToCapture;
        private float timeStarted;
        private string[] options;
        private bool flipScreen;
        private string uname;

        private string chartname,artistname,movename,chartername = "";
        private bool saveCustomIcon = true;

        // Output
        private OPDatum datum;

        // OpenPose settings
        private ProducerType inputType = ProducerType.Webcam;
        private string producerString = "-1";
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
        public void SetRenderThreshold(string s) { float res; if (float.TryParse(s, out res)) { renderThreshold = res; }; }
        public void SetMaxPeople(string s) { int res; if (int.TryParse(s, out res)) { maxPeople = res; }; }
        public void SetPoseResX(string s) { int res; if (int.TryParse(s, out res)) { netResolution.x = res; }; }
        public void SetPoseResY(string s) { int res; if (int.TryParse(s, out res)) { netResolution.y = res; }; }
        public void SetHandResX(string s) { int res; if (int.TryParse(s, out res)) { handResolution.x = res; }; }
        public void SetHandResY(string s) { int res; if (int.TryParse(s, out res)) { handResolution.y = res; }; }
        public void SetFaceResX(string s) { int res; if (int.TryParse(s, out res)) { faceResolution.x = res; }; }
        public void SetFaceResY(string s) { int res; if (int.TryParse(s, out res)) { faceResolution.y = res; }; }

        public void ApplyChanges()
        {
            // Restart OpenPose
            StartCoroutine(UserRebootOpenPoseCoroutine());
        }

        // Bg image
        public bool renderBgImg = false;
        public void ToggleRenderBgImg()
        {
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

        private void LoadParameters()
        {
            enterTimeToRedord.characterValidation = InputField.CharacterValidation.Integer;
            timer = 0;
            timeToStart = 3;
            timeToRecord = 10;
            timeToCapture = (float)0.25;
            timeNextCapture = float.MaxValue;
            timeStarted = float.MinValue;
            options = System.IO.Directory.GetFiles("./Media/");
            List<string> listoptions = new List<string>();
            for (int i = 0; i < options.Length; i++)
            {
                if (!options[i].Contains(".meta"))
                {
                    listoptions.Add(options[i].Replace("./Media/", ""));
                }
            }
            videos.ClearOptions();
            videos.AddOptions(listoptions);
            videos.enabled = false;
            flipScreen = false;
            uname = "";

        }
        public void SetInputType(bool isCamera)
        {
            if (isCamera)
            {
                inputType = ProducerType.Webcam;
                videos.enabled = false;
                flipScreen = false;
                Debug.Log("Enabled by default");
            }
            else
            {
                inputType = ProducerType.Video;
                videos.enabled = true;
                Debug.Log("----" + producerString);
                flipScreen = true;
            }
        }

        public void SetCustomIcon(bool value)
        {
            saveCustomIcon = value;
        }
    
        public void SetChartName(string value)
        {
            chartname = value;
        }
        public void SetArtistName(string value)
        {
            artistname = value;
        }
        public void SetMoveName(string value)
        {
            movename = value;
        }
        public void SetCharterName(string value)
        {
            chartername = value;
        }
        private void Start()
        {
            // Register callbacks
            OPWrapper.OPRegisterCallbacks();
            // Enable OpenPose log to unity (default true)
            OPWrapper.OPEnableDebug(true);
            // Enable OpenPose output to unity (default true)S
            OPWrapper.OPEnableOutput(true);
            // Enable receiving image (default false)
            OPWrapper.OPEnableImageOutput(true);

            // Configure OpenPose with default value, or using specific configuration for each
            /* OPWrapper.OPConfigureAllInDefault(); */
            //UserConfigureOpenPose();

            // Start OpenPose
            //OPWrapper.OPRun();

            LoadParameters();

            ToggleRenderBgImg();
            //ToggleRenderBgImg();
        }

        // Parameters can be set here
        private void UserConfigureOpenPose()
        {
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
                /* realTimeProcessing */ true, /* frameFlip */ flipScreen,
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

        private IEnumerator UserRebootOpenPoseCoroutine()
        {
            if (OPWrapper.state == OPState.None) yield break;
            // Shutdown if running
            if (OPWrapper.state == OPState.Running)
            {
                OPWrapper.OPShutdown();
                // Reset framerate calculator
                frameTimeQueue.Clear();
                frameCounter = 0;
            }
            // Wait until fully stopped
            yield return new WaitUntil(() => { return OPWrapper.state == OPState.Ready; });
            // Configure and start
            UserConfigureOpenPose();
            OPWrapper.OPRun();
        }

        private float Distance(Vector2 p1, Vector2 p2)
        {
            return (float)System.Math.Sqrt(System.Math.Pow((p2.x - p1.x), 2) + System.Math.Pow((p2.y - p1.y), 2));
        }

        private float Distance(float x1, float y1, float x2, float y2)
        {
            return (float)System.Math.Sqrt(System.Math.Pow((x1 - x2), 2) + System.Math.Pow((y1 - y2), 2));
        }

        public void AddPose()
        {
            humancopy.Clear();
            foreach (RectTransform rectTransform in humanContainer.GetComponentsInChildren<HumanController2D>()[0].getPoseJoints())
            {
                humancopy.Add((rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y));
            }
            if (humancopy.Count == 25)
            {
                string pose = "";
                if (uname == "")
                {
                    if (producerString == "-1")
                    {
                        uname = "user";
                        int i = 1;
                        while (File.Exists("./Custom/" + uname + i + "/data.json"))
                        {
                            i++;
                        }
                        uname += i;
                    }
                    else
                    {
                        uname = chartname;
                    }
                }
                string filename = "./Custom/" + uname + "/movement.txt";
                if (!File.Exists("./Custom/" + uname + "/data.json"))
                {
                    string datafile = "./Custom/" + uname + "/data.json";
                    string[] data = {
                        "{",          
                        "   \"artist\": \"" + artistname + "\",",
                        "   \"name\": \"" + chartname + "\",",
                        "   \"movement\": \"" + movename + "\",",
                        "   \"charter\": \"" + chartername + "\",",
                        "   \"timer\": \"" + timeToCapture + "\"",
                        "}"
                    };
                    System.IO.Directory.CreateDirectory("./Custom/" + uname);
                    if (producerString == "-1")
                    {
                        //System.IO.File.Copy(producerString, filename.Replace(".txt", ".mp4"), true);
                    }
                    else
                    {
                        System.IO.File.Copy(producerString, "./Custom/" + uname + "/video.mp4", true);
                        if (saveCustomIcon)
                        {
                            StartCoroutine(takeScreenShot("Custom/" + uname + "/icon.png"));
                        }
                    }
                    for (int i = 0; i < data.Length; i++)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@datafile, true))
                        {
                            file.WriteLine(data[i]);
                        }
                    }
                }
                float column = Distance(humancopy[1].Item1, humancopy[1].Item2, humancopy[8].Item1, humancopy[8].Item2);
                float column2 = 300 / column;
                for (int i = 0; i < 25; i++)
                {
                    pose += (humancopy[i].Item1 * column2).ToString() + " " + (humancopy[i].Item2 * column2).ToString() + " ";
                }
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filename, true))
                {
                    file.WriteLine(pose);
                }
                DrawCopy();
            }
        }

        public IEnumerator takeScreenShot(string path)
        {
            yield return new WaitForEndOfFrame();
            Rect rect = outputTransform.rect;
            int textWidth = (int)(System.Convert.ToInt32(rect.width) * 0.3631944); // width of the object to capture
            int textHeight = (int)(System.Convert.ToInt32(rect.height) * 0.3631944); // height of the object to capture

            var startX = System.Convert.ToInt32(-370) + Screen.width / 2; // offset X
            var startY = System.Convert.ToInt32(-125) + Screen.height / 2; // offset Y

            var tex = new Texture2D(250, 250, TextureFormat.RGB24, false);

            tex.ReadPixels(new Rect(startX, startY, 250, 250), 0, 0);
            Debug.Log(startX + " : " + startY);
            tex.Apply();

            // Encode texture into PNG
            var bytes = tex.EncodeToPNG();
            Destroy(tex);

            File.WriteAllBytes(path, bytes);
        }

        private void DrawCopy()
        {
            float x = 550;
            float y = 30;
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

        public void StartRecord()
        {
            if (enterTimeToRedord.text.Length > 0)
            {
                timeToRecord = float.Parse(enterTimeToRedord.text);
            }
            if (enterTimeToStart.text.Length > 0)
            {
                timeToStart = float.Parse(enterTimeToStart.text);
            }
            switch (timer2.value)
            {
                case 0:
                    timeToCapture = (float)0.5;
                    break;
                case 1:
                    timeToCapture = 1;
                    break;
                case 2:
                    timeToCapture = (float)1.5;
                    break;
                case 3:
                    timeToCapture = 2;
                    break;
                default:
                    timeToCapture = 1;
                    break;
            }
            if (inputType == ProducerType.Video)
            {
                producerString = "./Media/" + videos.options[videos.value].text;
            }
            else
            {
                producerString = "-1";
            }
            uname = "";
            UserConfigureOpenPose();
            OPWrapper.OPRun();
            timeStarted = timer;
            timeNextCapture = timeToStart + timer;
            typeCapture.enabled = false;
            videos.enabled = false;
            enterTimeToRedord.enabled = false;
            enterTimeToStart.enabled = false;
            timer2.enabled = false;
            // exitbttn.enabled = false;
            //  startbttn.enabled = false;
        }

        private void Update()
        {

            // Update state in UI
            stateText.text = OPWrapper.state.ToString();

            // Try getting new frame
            if (OPWrapper.OPGetOutput(out datum))
            { // true: has new frame data

                // Update background image
                bgImageRenderer.UpdateImage(datum.cvInputData);

                // Rescale output UI
                Vector2 outputSize = outputTransform.sizeDelta;
                Vector2 screenSize = Camera.main.pixelRect.size;
                //float scale = Mathf.Min(screenSize.x / outputSize.x, screenSize.y / outputSize.y);
                float scale = Mathf.Min(screenSize.x / (outputSize.x * 2), screenSize.y / (outputSize.y * 2));
                outputTransform.localScale = new Vector3(scale, scale, scale);

                // Update number of people in UI
                if (datum.poseKeypoints == null || datum.poseKeypoints.Empty()) numberPeople = 0;
                else numberPeople = datum.poseKeypoints.GetSize(0);
                peopleText.text = "People: " + numberPeople;

                // Draw human
                while (humanContainer.childCount < numberPeople)
                { // Make sure no. of HumanControllers no less than numberPeople
                    Instantiate(humanPrefab, humanContainer);
                }
                int i = 0;
                foreach (var human in humanContainer.GetComponentsInChildren<HumanController2D>())
                {
                    // When i >= no. of human, the human will be hidden
                    human.DrawHuman(ref datum, i++, renderThreshold);
                }

                // Update framerate in UI
                frameTimeQueue.Enqueue(Time.time);
                frameCounter++;
                if (frameTimeQueue.Count > queueMaxCount)
                { // overflow
                    frameTimeQueue.Dequeue();
                }
                if (frameCounter >= queueMaxCount || frameTimeQueue.Count <= 5)
                { // update frame rate
                    frameCounter = 0;
                    avgFrameRate = frameTimeQueue.Count / (Time.time - frameTimeQueue.Peek());
                    fpsText.text = avgFrameRate.ToString("F1") + " FPS";
                }
            }
            Debug.Log(timeToCapture);
            timer += Time.deltaTime;
            if (humanContainer.GetComponentsInChildren<HumanController2D>().Length > 0 && timeStarted > 0)
            {
                if (timer - timeStarted - timeToStart < 0)
                {
                    timerText.text = "-" + ((int)(timeStarted + timeToStart - timer) / 60) + ":" + ((int)(timeStarted + timeToStart - timer) % 60);
                }
                else
                {
                    timerText.text = ((int)(timer - timeStarted - timeToStart) / 60) + ":" + ((int)(timer - timeStarted - timeToStart) % 60);
                }

                if (timer > timeNextCapture && timer < timeStarted + timeToStart + timeToRecord)
                {
                    timeNextCapture += timeToCapture;
                    AddPose();
                }
            }

            if(!(string.IsNullOrEmpty(chartname) && string.IsNullOrEmpty(artistname) &&string.IsNullOrEmpty(movename)&&string.IsNullOrEmpty(chartername))){
                startbttn.enabled = true;
            }else{
                startbttn.enabled = false;
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