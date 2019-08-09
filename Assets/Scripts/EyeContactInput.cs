using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeContactInput : MonoBehaviour
{
    private bool sendMessage = false;

    public Transform mainCamera;
    public float maxRayDistance = 25;
    public LayerMask activeLayers;

    void Start()
    {
        mainCamera = GameObject.Find("Main Camera").transform;

        StartCoroutine("UpdateEveryOneSecond");
    }

    IEnumerator UpdateEveryOneSecond()
    {
        while (true)
        {
            // callback
            if (sendMessage == true)
            {
                PresentationUnitError pu = new PresentationUnitError("noeyecontact", Config.VerbalPriorityMapping["noeyecontact"]);

                //Debug.Log("Sending message.");
                gameObject.SendMessage("NoEyeContactPresentationUnitError", pu);
            }

            //
            yield return new WaitForSeconds(1.0f);
        }
    }

    private void FixedUpdate()
    {
        Ray ray = new Ray(mainCamera.position, mainCamera.forward);
        RaycastHit hit;

        Debug.DrawLine(mainCamera.position, mainCamera.position + mainCamera.forward * maxRayDistance, Color.red);
        if (!Physics.Raycast(ray, out hit, maxRayDistance, activeLayers))
        {
            sendMessage = true;
        }
        else
        {
            sendMessage = false;
            Debug.DrawLine(hit.point, hit.point + Vector3.up * 5, Color.green);
            //Debug.Log("You hit a ray.");
        }


        //RaycastHit[] hits = Physics.RaycastAll(ray, maxRayDistance, activeLayers);

        //foreach(RaycastHit hit in hits)
        //{
        //    //Debug.DrawLine(hit.point, hit.point + Vector3.up * 5, Color.green);
        //}
    }
}
