using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class GameObjectBrush : MonoBehaviour
{
    [Range(0.0f, 20.0f),SerializeField,Tooltip("The radius of the placement circle")]
    private float radius;

    [Range(0.0f, 10.0f), SerializeField, 
        Tooltip(" The minimum allowed distance for each placed object to another placed object")]
    public float radiusFromOtherObjects;

    public GameObject objectToPlace;
    public Transform parentToPlaceIn;

    [Range(0.0f, 1.0f)]
    public float density;

    private List<GameObject> lastPlacedGameObjects = new List<GameObject>();

    public Terrain Terrain;

    public bool alwaysPlaceAtLeastOne = true;

    public bool randomlyRotateObjects = true;

    

    public float Radius
    {
        get
        {
            return radius;
        }
        set
        {
            Debug.Log("set");
            radius = Mathf.Clamp(value,0.05f,20.0f);
        }
    }


    private void Start()
    {
        Radius = 1.0f;
    }
    

    public void PlaceObjectsAt(Vector3 position)
    {
        lastPlacedGameObjects.Clear();

        //ensure radius > distanceFromOtherObjects

        if (radiusFromOtherObjects > radius)
        {
            Debug.LogError("radiusFromOtherObjects CANNOT be bigger than radius");
            return;
        }

        float spaceForOtherCircles = radius - radiusFromOtherObjects;
        float diameterFromOtherObjects = radiusFromOtherObjects * 2;

        // number of circles that can be placed excluding center
        int circlePlacementCount = (int)(spaceForOtherCircles/diameterFromOtherObjects);
        Debug.Log(" circlePlacementCount " + circlePlacementCount);

        //get absolute value of radius - radiusFromOtherObjects + (2*radiusFromOtherObjects)
        float leftoverSpace = spaceForOtherCircles - circlePlacementCount * diameterFromOtherObjects;

        //Instantiate Object on center
        if(alwaysPlaceAtLeastOne)
        {
            PlaceObject(position);
        }
        else
        {
            if(DoCheckIfShouldPlace()) { PlaceObject(position); }
        }
        

        // get minimum chord length to avoid collision
        float minimumChordLength = diameterFromOtherObjects;

        float placementCircleDiameter = diameterFromOtherObjects*2;

        //for CirclePlacementCount
        for (int i = 0; i < circlePlacementCount; i++)
        {
            Vector3 nextSpawnPoint = Vector3.forward * diameterFromOtherObjects * (i+1);

            float minimumAngleToAvoidCollision =
                2 * Mathf.Asin(minimumChordLength / (placementCircleDiameter)) * Mathf.Rad2Deg;

            int numberOfObjectsPlacableInCircle = (int)(361.0f / minimumAngleToAvoidCollision);
            Debug.Log(" numberOfObjectsPlacableInCircle " + numberOfObjectsPlacableInCircle);

            float leftoverAngle = Mathf.Abs(360.0f - numberOfObjectsPlacableInCircle * minimumAngleToAvoidCollision);
            Debug.Log("leftoverAngle  " + leftoverAngle);

            for (int j = 0; j < numberOfObjectsPlacableInCircle; j++)
            {
                float addedAngleInIteration = Random.Range(0.0f, leftoverAngle); 
                leftoverAngle -= addedAngleInIteration;

                nextSpawnPoint = Quaternion.AngleAxis(minimumAngleToAvoidCollision + addedAngleInIteration, Vector3.up) * nextSpawnPoint;

                Vector3 curentSpawnPoint = position + nextSpawnPoint;

                if(DoCheckIfShouldPlace())
                {
                    PlaceObject(curentSpawnPoint);
                }
            }

            placementCircleDiameter += diameterFromOtherObjects;
        }
    }



    private bool DoCheckIfShouldPlace()
    {
        return Random.Range(0, 1.0f) < density;
    }


    private void PlaceObject(Vector3 position)
    {
        GameObject placedObject = Instantiate(objectToPlace, position, Quaternion.identity);
        lastPlacedGameObjects.Add(placedObject);

        if (Terrain != null)
        {
            ModifyYBasedOnTerrain(placedObject);
            ModifyLookRotationBasedOnTerrain(placedObject);
        }

        if(parentToPlaceIn != null)
        {
            placedObject.transform.SetParent(parentToPlaceIn);
        }

        if(randomlyRotateObjects)
        {
            placedObject.transform.rotation *= Quaternion.AngleAxis(Random.Range(0, 360), placedObject.transform.up);
        }
    }

    public void DeleteObjectsInRange(Vector3 position)
    {
        if(parentToPlaceIn ==null)
        {
            Debug.LogError("This feature only works if ParentToPlaceIn is set!");
            return;
        }

        foreach(Transform child in parentToPlaceIn)
        {
            float distanceSquared = Vector3.SqrMagnitude(child.transform.position - position);
            if(distanceSquared < Mathf.Pow(radius,2))
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    private void ModifyYBasedOnTerrain(GameObject obj)
    {
        float newY = Terrain.SampleHeight(obj.transform.position) + Terrain.GetPosition().y;
        obj.transform.position = new Vector3(obj.transform.position.x, newY, obj.transform.position.z);
    }


    private void ModifyLookRotationBasedOnTerrain(GameObject obj)
    {
        Vector3 objectPosition = obj.transform.position;

        Vector3 nextOnForward = objectPosition + Vector3.forward * 0.01f;
        float nextOnForwardY = Terrain.SampleHeight(nextOnForward) + Terrain.GetPosition().y;
        nextOnForward = new Vector3(nextOnForward.x, nextOnForwardY, nextOnForward.z);

        Vector3 nextOnRight = objectPosition + Vector3.right * 0.01f;
        float nextOnRightY = Terrain.SampleHeight(nextOnRight) + Terrain.GetPosition().y;
        nextOnRight = new Vector3(nextOnRight.x, nextOnRightY, nextOnRight.z);

        Vector3 forward = nextOnForward - objectPosition;
        Vector3 right = nextOnRight - objectPosition;

        obj.transform.LookAt(nextOnForward, Vector3.Cross(forward, right).normalized);
        

    }

    public void DestroyRecentlyPlacedObjects()
    {
        foreach(var obj in lastPlacedGameObjects)
        {
            DestroyImmediate(obj);
        }
    }

}
