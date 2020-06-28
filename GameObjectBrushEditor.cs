using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameObjectBrush))]
public class GameObjectBrushEditor : Editor
{
    GameObjectBrush brush;
    
    GameObject actualObject;
    GameObject sampleObject;

    Vector3 lastHitPoint;

    private static GUIStyle ToggleButtonStyleNormal = null;

    private string destructionModeStr = "Destruction Mode";
    private string InstantiationModeStr = "Instantiation Mode";

    private string displayString;

    private bool isInstantiationMode = true;

    private float destructionSphereTransparency = 0.8f;

    private void OnEnable()
    {
        brush = target as GameObjectBrush;

        displayString = InstantiationModeStr;
        isInstantiationMode = true;
    }

    public override void OnInspectorGUI()
    {
        if (ToggleButtonStyleNormal == null)
        {
            ToggleButtonStyleNormal = "Button";
        }

        GUI.backgroundColor = isInstantiationMode ? Color.green : Color.red;

        if(GUILayout.Button(displayString, ToggleButtonStyleNormal))
        {
            isInstantiationMode = !isInstantiationMode;

            if(displayString == InstantiationModeStr)
            {
                displayString = destructionModeStr;
            }
            else
            {
                displayString = InstantiationModeStr;
            }
        }




        GUI.backgroundColor = Color.white;

        DrawDefaultInspector();

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Delete Previously Placed Objects", ToggleButtonStyleNormal))
        {
            brush.DestroyRecentlyPlacedObjects();
        }
    }


    private void OnSceneGUI()
    {
        if (!brush.isActiveAndEnabled) { return; }

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit = new RaycastHit();

        if(isInstantiationMode)
        {
            HandleCreationModeGUI(ray, hit);
        }
        else
        {
            HandleDestructionModeGUI(ray, hit);
        }

        SceneView.RepaintAll();
    }

    private void HandleDestructionModeGUI(Ray ray, RaycastHit hit)
    {
        if (Physics.Raycast(ray, out hit))
        {
            lastHitPoint = hit.point;
            DrawDestructionHandles(hit);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                brush.DeleteObjectsInRange(hit.point);
            }
        }
    }

    private void HandleCreationModeGUI(Ray ray,RaycastHit hit)
    {
        

        if (Physics.Raycast(ray, out hit))
        {
            lastHitPoint = hit.point;
            DrawBrushHandles(hit);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                brush.PlaceObjectsAt(hit.point);
            }

        }


        PlacePreviewObject();
    }

    private void PlacePreviewObject()
    {
        if (brush.objectToPlace != null)
        {
            if (sampleObject != brush.objectToPlace)
            {
                Debug.Log("sampleObject != brush.objectToPlace");
                if (!NullCheck(actualObject))
                {
                    DestroyImmediate(actualObject);

                }

                foreach (Transform child in brush.transform)
                {
                    DestroyImmediate(child.gameObject);
                }

                sampleObject = brush.objectToPlace;
                actualObject = Instantiate(brush.objectToPlace, lastHitPoint, Quaternion.identity);
                actualObject.name = "actualObject";
                actualObject.transform.SetParent(brush.transform);
            }
        }

        if (actualObject != null)
        {
            actualObject.transform.position = lastHitPoint;
        }
    }

    private void DrawBrushHandles(RaycastHit hit)
    {
        Quaternion circleRotation = Quaternion.LookRotation(hit.normal);

        Handles.color = Color.white;
        Handles.CircleHandleCap(0, hit.point, circleRotation,
            brush.Radius, EventType.Repaint);

        Handles.color = Color.red;
        Handles.CircleHandleCap(0, hit.point, circleRotation,
            brush.radiusFromOtherObjects, EventType.Repaint);
    }

    private void DrawDestructionHandles(RaycastHit hit)
    {
        Quaternion circleRotation = Quaternion.LookRotation(hit.normal);

        Color destructionSphereColor = Color.red;

        destructionSphereColor.a = destructionSphereTransparency;
        Handles.color = destructionSphereColor;

        Handles.CircleHandleCap(0, hit.point, circleRotation,
            brush.Radius * 0.1f, EventType.Repaint);

        Handles.color = Color.red;
        Handles.CircleHandleCap(0, hit.point, circleRotation,
            brush.Radius, EventType.Repaint);

        Handles.CircleHandleCap(0, hit.point, circleRotation,
            brush.Radius * 0.95f, EventType.Repaint);
    }

    private bool NullCheck(GameObject obj)
    {
        return obj == null;
    }
    
    

}
