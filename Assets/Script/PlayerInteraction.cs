using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    [Range(0, 255)]
    float highlightIntensity = 20;
    MouseSystem mouseSystem;
    public bool Running { get; private set; } = true;
    GameObject tmp_obj = null;
    float animValue = 0;
    float raycastDistance = 7f;

    //Dictionary<MeshRenderer, Color> colorStats = new Dictionary<MeshRenderer, Color>();
    Dictionary<GameObject, Dictionary<MeshRenderer, Color>> tmp_objs =
        new Dictionary<GameObject, Dictionary<MeshRenderer, Color>>();

    [SerializeField]
    LayerMask layerMask;

    void Awake()
    {
        mouseSystem = FindAnyObjectByType<MouseSystem>();
    }

    private void Update()
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (
            Physics.Raycast(
                transform.position,
                transform.TransformDirection(Vector3.forward),
                out hit,
                raycastDistance,
                layerMask
            )
        )
        {
            Debug.DrawRay(
                transform.position,
                transform.TransformDirection(Vector3.forward) * hit.distance,
                Color.yellow
            );

            tmp_obj = hit.transform.gameObject;

            if (!Cache_ObjectHasCache(tmp_obj))
                Cache_CreateObjectCache(tmp_obj);
            if (!mouseSystem.Interacting)
                mouseSystem.StartInteraction();
            ChangeFamilyColor(tmp_obj.transform, true, animValue);

            if (Input.GetMouseButton((int)MouseButton.Left))
            {
                CameraFix cameraFix = tmp_obj.GetComponent<CameraFix>();
                if (cameraFix)
                {
                    cameraFix.LockCamera();
                    Debug.Log("Camera locked on puzzle");
                }
            }
        }
        else
        {
            if (mouseSystem.Interacting)
                mouseSystem.FinishInteraction();

            if (tmp_obj != null)
            {
                StartCoroutine(ResetToDefaultColor(tmp_obj, animValue));
                tmp_obj = null;
            }
            Debug.DrawRay(
                transform.position,
                transform.TransformDirection(Vector3.forward) * raycastDistance,
                Color.white
            );
        }

        if (tmp_obj != null)
        {
            animValue = Mathf.Clamp01(animValue + Time.deltaTime * 10);
        }
        else if (animValue != 0)
            animValue = 0;

        return;
    }

    bool Cache_ObjectHasCache(GameObject parent)
    {
        if (parent == null)
        {
            Debug.LogError("Invalid parent");
            return false;
        }

        foreach (KeyValuePair<GameObject, Dictionary<MeshRenderer, Color>> stats in tmp_objs)
        {
            if (stats.Key == parent)
                return true;
        }
        return false;
    }

    void Cache_CreateObjectCache(GameObject parent)
    {
        List<MeshRenderer> renderers = parent.GetComponentsInChildren<MeshRenderer>().ToList();

        Dictionary<MeshRenderer, Color> colorStats = new Dictionary<MeshRenderer, Color>();

        foreach (MeshRenderer render in renderers)
        {
            if (render.material == null)
            {
                Debug.LogError("CACHE : Object " + render.gameObject.name + " don't has material");
                continue;
            }
            colorStats.Add(render, render.material.GetColor("_Color"));
        }
        tmp_objs.Add(parent, colorStats);
        Debug.Log("CACHE : Object " + parent.name + " cache created");
        return;
    }

    void ChangeFamilyColor(Transform parent, bool active, float animV)
    {
        List<MeshRenderer> renderers = Cache_GetRenderersInObject(parent.gameObject);
        if (active)
        {
            foreach (MeshRenderer render in renderers)
            {
                Color color =
                    Cache_GetRenderDefaultColor(parent.gameObject, render)
                    + new Color(
                        Mathf.Lerp(0, highlightIntensity / 100, animV),
                        Mathf.Lerp(0, highlightIntensity / 100, animV),
                        Mathf.Lerp(0, highlightIntensity / 100, animV),
                        0
                    );
                try
                {
                    render.material.SetColor("_Color", color);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    return;
                }
            }
        }
        else
        {
            foreach (MeshRenderer render in renderers)
            {
                Color color =
                    Cache_GetRenderDefaultColor(parent.gameObject, render)
                    + new Color(
                        Mathf.Lerp(0, (float)20 / 100, animV),
                        Mathf.Lerp(0, (float)20 / 100, animV),
                        Mathf.Lerp(0, (float)20 / 100, animV),
                        0
                    );
                try
                {
                    render.material.SetColor("_Color", color);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    return;
                }
                //Color color = tmp_objs.Select(obj => obj.Select(o => o.Key == parent.GameObject).KeyValuePair.Select(o => o.Value));
            }
        }
    }

    IEnumerator ResetToDefaultColor(GameObject parent, float animV)
    {
        float currentAnimValue = animV;
        while (currentAnimValue > 0)
        {
            currentAnimValue = Mathf.Clamp01(currentAnimValue - Time.deltaTime * 10);
            ChangeFamilyColor(parent.transform, false, currentAnimValue);
            yield return null; // wait for next frame
        }
        yield break;
    }

    List<MeshRenderer> Cache_GetRenderersInObject(GameObject obj)
    {
        foreach (KeyValuePair<GameObject, Dictionary<MeshRenderer, Color>> objStats in tmp_objs)
        {
            List<MeshRenderer> mList = obj.GetComponentsInChildren<MeshRenderer>().ToList();
            if (objStats.Key == obj)
            {
                MeshRenderer thisRender = obj.GetComponent<MeshRenderer>();

                if (thisRender)
                    mList.Add(thisRender);
            }
            return mList;
        }
        Debug.Log("CACHE : Object not found in cache");
        return new List<MeshRenderer>();
    }

    Color Cache_GetRenderDefaultColor(GameObject parent, MeshRenderer render)
    {
        foreach (KeyValuePair<GameObject, Dictionary<MeshRenderer, Color>> objStats in tmp_objs)
        {
            if (objStats.Key == parent.gameObject)
            {
                foreach (KeyValuePair<MeshRenderer, Color> cStats in objStats.Value)
                {
                    if (cStats.Key == render)
                    {
                        return cStats.Value;
                    }
                }
            }
        }
        return Color.white;
    }
}
