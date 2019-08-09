using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smile : MonoBehaviour
{
    private bool smile = false;
    private SkinnedMeshRenderer mesh_renderer;

    private float blendWeight = 50f;
    private float blendSpeed = 1f;

    private void Start()
    {
        mesh_renderer = GetComponent<SkinnedMeshRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (smile == true)
        {
            if (blendWeight < 100f)
            {
                mesh_renderer.SetBlendShapeWeight(30, blendWeight);
                mesh_renderer.SetBlendShapeWeight(31, blendWeight);
                //renderer.updateWhenOffscreen = true;
                blendWeight += blendSpeed;
            }
        }
        else
        {
            if (blendWeight >= 30f)
            {
                mesh_renderer.SetBlendShapeWeight(30, blendWeight);
                mesh_renderer.SetBlendShapeWeight(31, blendWeight);
                blendWeight -= 2*blendSpeed;
            }
        }
    }

    public void OpenSmile()
    {
        smile = true;
    }

    public void CloseSmile()
    {
        smile = false;
    }
}
