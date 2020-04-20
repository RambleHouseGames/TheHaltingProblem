using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlacementHandler : MonoBehaviour
{
    public static PlacementHandler Inst;

    public bool PlacementIsInProgress { get { return (activeGhost != null); } }

    private ItemGhost activeGhost = null;

    private List<Coord> adjacentCoords;
    private float currentMouseX;
    private float currentMouseZ;

    private bool mouseDownCounts = false;

    void Awake()
    {
        Inst = this;
    }

    void Start()
    {
        GameSceneSignalManager.Inst.AddListenner<ItemIconClickedSignal>(onItemIconClicked);
    }

    void Update()
    {
        if(activeGhost != null)
        {
            if (Input.GetMouseButtonUp(0))
                mouseDownCounts = true;

            updateRaycast();
            placeGhost();
            if(Input.GetMouseButtonDown(0) && mouseDownCounts)
            {
                foreach (Coord coord in adjacentCoords)
                {
                    if (activeGhost.CanBePlacedAt(coord.X, coord.Z))
                    {
                        ItemLocationSelectedSignal itemLocationSelectedSignal = new ItemLocationSelectedSignal(activeGhost, coord.X, coord.Z);
                        GameSceneSignalManager.Inst.FireSignal(itemLocationSelectedSignal);
                        Destroy(activeGhost.gameObject);
                        activeGhost = null;
                        adjacentCoords = null;
                        return;
                    }
                }
            }
        }
    }

    private void onItemIconClicked(Signal signal)
    {
        mouseDownCounts = false;
        ItemIconClickedSignal itemIconClickedSignal = (ItemIconClickedSignal)signal;
        updateRaycast();
        activeGhost = Instantiate<ItemGhost>(itemIconClickedSignal.ItemIcon.ItemGhost);
        placeGhost();
    }

    private void updateRaycast()
    {
        RaycastHit raycastHit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool didHit = Physics.Raycast(ray, out raycastHit);
        Debug.Assert(didHit, "Raycast didn't hit anything.  This might mean the player reached the edge of the level.");
        adjacentCoords = new List<Coord>();
        adjacentCoords.Add(new Coord(Mathf.FloorToInt(raycastHit.point.x), Mathf.FloorToInt(raycastHit.point.z), raycastHit.point.x, raycastHit.point.z));
        adjacentCoords.Add(new Coord(Mathf.FloorToInt(raycastHit.point.x), Mathf.CeilToInt(raycastHit.point.z), raycastHit.point.x, raycastHit.point.z));
        adjacentCoords.Add(new Coord(Mathf.CeilToInt(raycastHit.point.x), Mathf.FloorToInt(raycastHit.point.z), raycastHit.point.x, raycastHit.point.z));
        adjacentCoords.Add(new Coord(Mathf.CeilToInt(raycastHit.point.x), Mathf.CeilToInt(raycastHit.point.z), raycastHit.point.x, raycastHit.point.z));

        adjacentCoords = adjacentCoords.OrderBy(x => x.DistanceToMouse).ToList();
        currentMouseX = raycastHit.point.x;
        currentMouseZ = raycastHit.point.z;
    }

    private void placeGhost()
    {
        foreach (Coord coord in adjacentCoords)
        {
            if (activeGhost.CanBePlacedAt(coord.X, coord.Z))
            {
                placeHappyGhost(coord);
                return;
            }
        }
        placeSadGhost();
    }

    private void placeHappyGhost(Coord coord)
    {
        activeGhost.transform.position = new Vector3((float)coord.X, 0f, (float)coord.Z);
        activeGhost.HappyGhost.active = true;
        activeGhost.SadGhost.active = false;
    }

    private void placeSadGhost()
    {
        activeGhost.transform.position = new Vector3(currentMouseX, 0f, currentMouseZ);
        activeGhost.SadGhost.active = true;
        activeGhost.HappyGhost.active = false;
    }
}

public class Coord
{
    public int X;
    public int Z;
    public float DistanceToMouse;

    public Coord (int x, int z, float mouseX, float mouseZ)
    {
        this.X = x;
        this.Z = z;
        this.DistanceToMouse = Vector2.Distance(new Vector2((float)x, (float)z), new Vector2(mouseX, mouseZ));
    }
}