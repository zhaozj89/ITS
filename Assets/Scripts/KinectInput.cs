using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using UnityEngine;

public class KinectInput : MonoBehaviour
{
    private KinectManager manager = null;
    private long userID = 0;

    private List<int> indexes = new List<int>();
    private List<Vector3> savedPositions;
    private List<Vector3> savedDirections;
    private List<Vector3> savedAngles;
    private List<bool> isdetecteds;

    private int counter = 0;


    private bool firstEvaluate = true;

    void Start()
    {

        // data initialization
        manager = KinectManager.Instance;  // initialize KinectManager

        indexes.Add((int)KinectInterop.JointType.WristLeft);
        indexes.Add((int)KinectInterop.JointType.WristRight);
        savedPositions = new List<Vector3>(new Vector3[indexes.Count]);
        savedDirections = new List<Vector3>(new Vector3[indexes.Count]);
        savedAngles = new List<Vector3>(new Vector3[indexes.Count]);
        isdetecteds = new List<bool>(new bool[indexes.Count]);

        StartCoroutine("Detect");
        StartCoroutine("Evaluate");
    }


    private bool Determine_by_z_distance(int joint, Vector3 previousPos, Vector3 currentPos)
    {
        // determine whether the gesture reachs the predefined threshold
        var distance = Mathf.Abs(previousPos.z - currentPos.z);

        //print(string.Format("Joint {0} distance: {1:0.00}", joint, distance));
        if (distance > 0.1f)
            return true;
        return false;
    }

    private bool Determine_by_angle(int joint, Vector3 previousDirection, Vector3 currentDirection)
    {
        var currentAngle = Vector3.Angle(previousDirection, currentDirection);
        //print(string.Format("The {0} joint angle {1} ", joint, currentAngle));
        if (currentAngle > 50f)
            return true;
        return false;
    }

    IEnumerator Detect()
    {
        while (true)
        {
            //GetPrimaryUserID()：获取主用户的UserID（第一个或最接近的），如果没有检测到用户，则为0。     
            userID = manager.GetPrimaryUserID(); //获取用户的userID
            for (int i = 0; i < isdetecteds.Count; i++) { isdetecteds[i] = false; }

            if (userID != 0 && manager.IsInitialized())
            {
                // Index: the key of saved variables.
                // Value: the key of `indexes`
                foreach (var joint in indexes.Select((Value, Index) => new { Value, Index }))
                {
                    if (manager.IsJointTracked(userID, joint.Value))  // if joint is tracked,
                    {
                        // The very beginning of the detection,
                        // here should set initial value of previous data
                        if (savedPositions[joint.Index] == Vector3.zero || savedDirections[joint.Index] == Vector3.zero)
                        {
                            //if (joint.Index == 0)
                            //    StartCoroutine("Evaluate");
                            //evaluateTimer.Enabled = true;
                            savedPositions[joint.Index] = manager.GetJointPosition(userID, joint.Value);
                            savedDirections[joint.Index] = manager.GetJointDirection(userID, joint.Value, true, true);
                            continue;  // wait for the next timer
                        }

                        var currentPosition = manager.GetJointPosition(userID, joint.Value);
                        var currentDirection = manager.GetJointDirection(userID, joint.Value, true, true);

                        if (Determine_by_angle(joint.Value, savedDirections[joint.Index], currentDirection) || Determine_by_z_distance(joint.Index, savedPositions[joint.Index], currentPosition))
                        {

                            isdetecteds[joint.Index] = true;
                            savedDirections[joint.Index] = currentDirection;
                            savedPositions[joint.Index] = currentPosition;  // save new position
                            break;  // skip other detection
                        }

                    }
                }
            }
            if (isdetecteds.Contains(true))
            {
                counter++;
                //  UnityEngine.Debug.Log(string.Format("Total movement: {0}", counter));
            }

            yield return new WaitForSeconds(Config.DURATION_MOTION_DETECTION);
        }
    }

    IEnumerator Evaluate()
    {
        while (true)
        {
            if (firstEvaluate == true)
            {
                firstEvaluate = false;
                yield return new WaitForSeconds(Config.DURATION_MOTION_EVALUATETION);
            }

            //Debug.Log("The number of using hand gesture: " + counter);

            if (counter == 0)
            {
                PresentationUnitError pu = new PresentationUnitError("nohandgesture", Config.VerbalPriorityMapping["nohandgesture"]);
                gameObject.SendMessage("NoHandGesturePresentationUnitError", pu);
            }
            else
            {
                counter = 0;
            }

            yield return new WaitForSeconds(Config.DURATION_MOTION_EVALUATETION);
        }
    }
}
