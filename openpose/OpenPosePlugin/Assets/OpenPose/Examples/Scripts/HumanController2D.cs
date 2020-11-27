// OpenPose Unity Plugin v1.0.0alpha-1.5.0
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenPose.Example
{
    /*
     * HumanController2D translate the output data into 2D transforms
     * The Joints child gameObject contains all keypoints info
     * Transform (x, y): the x and y coordinates on the frame
     * Active: whether the score of that keypoint is larger than ScoreThres
     */
    public class HumanController2D : MonoBehaviour
    {

        public int PoseKeypointsCount = 25;
        public int HandKeypointsCount = 21;
        public int FaceKeypointsCount = 70;

        [SerializeField] RectTransform PoseParent;
        [SerializeField] RectTransform LHandParent;
        [SerializeField] RectTransform RHandParent;
        [SerializeField] RectTransform FaceParent;
        [SerializeField] RectTransform LHandRectangle;
        [SerializeField] RectTransform RHandRectangle;
        [SerializeField] RectTransform FaceRectangle;
        private List<RectTransform> poseJoints = new List<RectTransform>();
        private List<RectTransform> lHandJoints = new List<RectTransform>();
        private List<RectTransform> rHandJoints = new List<RectTransform>();
        private List<RectTransform> faceJoints = new List<RectTransform>();
        private double[] arrayx = { 713.549, 717.589, 650.932, 560.829, 474.593, 789.967, 878.214, 964.378, 713.711, 676.377, 678.369, 668.569, 758.596, 743.08, 746.985, 698.038, 727.333, 682.286, 745.001, 792.074, 793.91, 739.089, 621.554, 621.554, 680.334 };
        private double[] arrayy = { 94.5737, 165.122, 165.16, 178.768, 180.826, 165.069, 165.17, 167.014, 378.593, 374.707, 511.872, 647.009, 380.551, 515.78, 646.956, 84.7163, 84.786, 96.5211, 100.351, 674.425, 666.537, 64.823, 668.531, 664.564, 656.804 };

        public void DrawHuman(ref OPDatum datum, int bodyIndex, float scoreThres = 0)
        {
            DrawBody(ref datum, bodyIndex, scoreThres);
            DrawHand(ref datum, bodyIndex, scoreThres);
            DrawFace(ref datum, bodyIndex, scoreThres);
            DrawRectangles(ref datum, bodyIndex);
            GetScore(ref datum, bodyIndex);
        }

        public void GetScore(ref OPDatum datum, int bodyIndex)
        {
            double promBody = 0;
            double expectedProm = 0;
            float score = 0;
            if (datum.poseKeypoints == null || bodyIndex >= datum.poseKeypoints.GetSize(0))
            {
                return;
            }
            else
            {
                double normalizedX = arrayx[7] - datum.poseKeypoints.Get(bodyIndex, 8, 0); //NormalizeTheMiddle
                double normalizedY = arrayy[7] - datum.poseKeypoints.Get(bodyIndex, 8, 1);

                for (int part = 0; part < poseJoints.Count; part++)
                {
                    promBody+= (double)(datum.poseKeypoints.Get(bodyIndex, part, 0)) + normalizedX;
                    promBody+= (double)(datum.poseKeypoints.Get(bodyIndex, part, 1)) + normalizedY;
                    expectedProm += arrayx[part] + arrayy[part];
                }
                score =  Mathf.Abs((float) (100 - ((promBody * 100) / expectedProm)));
                Debug.Log("You got " + score + " Accuracy");
            }
        }



        private void DrawBody(ref OPDatum datum, int bodyIndex, float scoreThres)
        {
            if (datum.poseKeypoints == null || bodyIndex >= datum.poseKeypoints.GetSize(0))
            {
                PoseParent.gameObject.SetActive(false);
                return;
            }
            else
            {
                PoseParent.gameObject.SetActive(true);
            }
            // Pose
            for (int part = 0; part < poseJoints.Count; part++)
            {
                // Joints overflow
                if (part >= datum.poseKeypoints.GetSize(1))
                {
                    poseJoints[part].gameObject.SetActive(false);
                    continue;
                }
                // Compare score
                if (datum.poseKeypoints.Get(bodyIndex, part, 2) <= scoreThres)
                {
                    poseJoints[part].gameObject.SetActive(false);
                }
                else
                {
                    poseJoints[part].gameObject.SetActive(true);
                    Vector3 pos = new Vector3(datum.poseKeypoints.Get(bodyIndex, part, 0), datum.poseKeypoints.Get(bodyIndex, part, 1), 0f);
                    poseJoints[part].localPosition = pos;
                }
            }
        }

        private void DrawHand(ref OPDatum datum, int bodyIndex, float scoreThres)
        {
            // Left
            if (datum.handKeypoints == null || bodyIndex >= datum.handKeypoints.left.GetSize(0))
            {
                LHandParent.gameObject.SetActive(false);
            }
            else
            {
                LHandParent.gameObject.SetActive(true);
                for (int part = 0; part < lHandJoints.Count; part++)
                {
                    // Joints overflow
                    if (part >= datum.handKeypoints.left.GetSize(1))
                    {
                        lHandJoints[part].gameObject.SetActive(false);
                        continue;
                    }
                    // Compare score
                    if (datum.handKeypoints.left.Get(bodyIndex, part, 2) <= scoreThres)
                    {
                        lHandJoints[part].gameObject.SetActive(false);
                    }
                    else
                    {
                        lHandJoints[part].gameObject.SetActive(true);
                        Vector3 pos = new Vector3(datum.handKeypoints.left.Get(bodyIndex, part, 0), datum.handKeypoints.left.Get(bodyIndex, part, 1), 0f);
                        lHandJoints[part].localPosition = pos;
                    }
                }
            }
            // Right
            if (datum.handKeypoints == null || bodyIndex >= datum.handKeypoints.right.GetSize(0))
            {
                RHandParent.gameObject.SetActive(false);
            }
            else
            {
                RHandParent.gameObject.SetActive(true);
                for (int part = 0; part < rHandJoints.Count; part++)
                {
                    // Joints overflow
                    if (part >= datum.handKeypoints.right.GetSize(1))
                    {
                        rHandJoints[part].gameObject.SetActive(false);
                        continue;
                    }
                    // Compare score
                    if (datum.handKeypoints.right.Get(bodyIndex, part, 2) <= scoreThres)
                    {
                        rHandJoints[part].gameObject.SetActive(false);
                    }
                    else
                    {
                        rHandJoints[part].gameObject.SetActive(true);
                        Vector3 pos = new Vector3(datum.handKeypoints.right.Get(bodyIndex, part, 0), datum.handKeypoints.right.Get(bodyIndex, part, 1), 0f);
                        rHandJoints[part].localPosition = pos;
                    }
                }
            }
        }

        private void DrawFace(ref OPDatum datum, int bodyIndex, float scoreThres)
        {
            // Face
            if (datum.faceKeypoints == null || bodyIndex >= datum.faceKeypoints.GetSize(0))
            {
                FaceParent.gameObject.SetActive(false);
                return;
            }
            else
            {
                FaceParent.gameObject.SetActive(true);

                for (int part = 0; part < faceJoints.Count; part++)
                {
                    // Joints overflow
                    if (part >= datum.faceKeypoints.GetSize(1))
                    {
                        faceJoints[part].gameObject.SetActive(false);
                        continue;
                    }
                    // Compare score
                    if (datum.faceKeypoints.Get(bodyIndex, part, 2) <= scoreThres)
                    {
                        faceJoints[part].gameObject.SetActive(false);
                    }
                    else
                    {
                        faceJoints[part].gameObject.SetActive(true);
                        Vector3 pos = new Vector3(datum.faceKeypoints.Get(bodyIndex, part, 0), datum.faceKeypoints.Get(bodyIndex, part, 1), 0f);
                        faceJoints[part].localPosition = pos;
                    }
                }
            }
        }

        private void DrawRectangles(ref OPDatum datum, int bodyIndex)
        {
            // Hand rect
            if (datum.handRectangles == null || bodyIndex >= datum.handRectangles.Count)
            {
                LHandRectangle.gameObject.SetActive(false);
                RHandRectangle.gameObject.SetActive(false);
            }
            else
            {
                var rects = datum.handRectangles[bodyIndex];
                // Left
                LHandRectangle.gameObject.SetActive(true);
                LHandRectangle.localPosition = rects.left.center;
                LHandRectangle.sizeDelta = rects.left.size;
                // Right
                RHandRectangle.gameObject.SetActive(true);
                RHandRectangle.localPosition = rects.right.center;
                RHandRectangle.sizeDelta = rects.right.size;
            }

            // Face rect
            if (datum.faceRectangles == null || bodyIndex >= datum.faceRectangles.Count)
            {
                FaceRectangle.gameObject.SetActive(false);
            }
            else
            {
                FaceRectangle.gameObject.SetActive(true);
                FaceRectangle.localPosition = datum.faceRectangles[bodyIndex].center;
                FaceRectangle.sizeDelta = datum.faceRectangles[bodyIndex].size;
            }
        }

        // Use this for initialization
        void Start()
        {
            InitJoints();
        }

        private void InitJoints()
        {
            // Pose
            if (PoseParent)
            {
                Debug.Assert(PoseParent.childCount == PoseKeypointsCount, "Pose joint count not match");
                for (int i = 0; i < PoseKeypointsCount; i++)
                {
                    poseJoints.Add(PoseParent.GetChild(i) as RectTransform);
                }
            }
            // LHand
            if (LHandParent)
            {
                Debug.Assert(LHandParent.childCount == HandKeypointsCount, "LHand joint count not match");
                //LHandRectangle = LHandParent.GetChild(0) as RectTransform;
                for (int i = 0; i < HandKeypointsCount; i++)
                {
                    lHandJoints.Add(LHandParent.GetChild(i) as RectTransform);
                }
            }
            // RHand
            if (RHandParent)
            {
                Debug.Assert(RHandParent.childCount == HandKeypointsCount, "RHand joint count not match");
                //RHandRectangle = RHandParent.GetChild(0) as RectTransform;
                for (int i = 0; i < HandKeypointsCount; i++)
                {
                    rHandJoints.Add(RHandParent.GetChild(i) as RectTransform);
                }
            }
            // Face
            if (FaceParent)
            {
                Debug.Assert(FaceParent.childCount == FaceKeypointsCount, "Face joint count not match");
                //FaceRectangle = FaceParent.GetChild(0) as RectTransform;
                for (int i = 0; i < FaceKeypointsCount; i++)
                {
                    faceJoints.Add(FaceParent.GetChild(i) as RectTransform);
                }
            }
        }
    }
}

