using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Random = Unity.Mathematics.Random;
using System.Collections;
using System;
using System.Linq;
using UnityEngine.UIElements;
using UnityEngine.Events;
using Unity.AI.Navigation;
using Unity.VisualScripting;

public class DungeonGenerator : MonoBehaviour
{
    public static DungeonGenerator instance;
    private void Awake() => instance = this;

    [Header("General")]
    public int seed;
    Random rng;

    public Vector2Int size = new Vector2Int(100, 50);
    public Vector2Int maxRoomSize = new Vector2Int(10, 10);

    public UnityEvent OnGenerationDone;

    [Header("Generation")]
    public List<RectInt> generatedRooms = new List<RectInt>();
    public float maxSplitOffset;
    bool generationFinished = false;

    [Header("Graph")]
    public Graph<RectInt> graph = new();

    [Header("Room Removal")]
    public float removePercentage;
    bool removalFinished = false;

    [Header("Path Removal")]
    [Tooltip("Switches between DFS and BFS")]
    public bool useDFS;
    bool removedCyclicPaths = false;

    [Header("Doors")]
    public int doorArea;
    public float hallwayChance;
    public List<Door> doors = new();

    [Header("Assets")]
    public GameObject floorPrefab;
    public List<GameObject> wallPrefabs = new();
    public GameObject doorHighlightPrefab;
    List<RoomManager> roomManagers = new List<RoomManager>();

    GameObject assetParent;

    [Header("Decorations")]
    public int maxDecorationsPerRoom;
    public GameObject[] decorationObjects;
    List<Vector3> spawnedDecorationPositions = new();

    [HorizontalLine]
    [Header("Navigation")]
    public Graph<Vector3> navigationGraph = new();

    [HorizontalLine]
    [Header("Animation")]
    public bool doAnimation;
    public float waitTime;

    Vector2Int[] orthogonalDirections = new Vector2Int[4]
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };
    Vector2Int[] diagonalDirections = new Vector2Int[4]
    {
        new Vector2Int(1, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, -1)
    };

    IEnumerator Start()
    {
        Initilize();

        yield return StartCoroutine(CreateRooms());

        if (keyPressContinue) yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        yield return StartCoroutine(RemoveRooms());

        if (keyPressContinue) yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        yield return StartCoroutine(RemoveCyclicPaths());

        if (keyPressContinue) yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        yield return StartCoroutine(SpawnDoors());

        if (keyPressContinue) yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        yield return StartCoroutine(SpawnAssets());

        if (keyPressContinue) yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        yield return StartCoroutine(CreateNavigationGraph());

        HideAllRooms();

        OnGenerationDone.Invoke();
    }

    void Initilize()
    {
        rng = new Random(Convert.ToUInt32(seed));
    }

    List<RectInt> roomsToCheck = new List<RectInt>();
    // Complexity O(n) extreme cases O(n^2) because of intgrated graph generation where n is the area of the entire dungeon / min room size?
    IEnumerator CreateRooms()
    {
        RectInt startRoom = new RectInt(0, 0, size.x, size.y);
        roomsToCheck.Add(startRoom);
        graph.AddNode(startRoom);

        while (roomsToCheck.Count > 0)
        {
            List<RectInt> newRooms = new List<RectInt>();

            foreach (RectInt room in roomsToCheck)
            {
                (RectInt, RectInt[])[] createdRooms = new (RectInt, RectInt[])[0];

                if (room.width > maxRoomSize.x && room.height > maxRoomSize.y) // both dimensions are too big - choose a random one
                {
                    bool splitHorizontal = Mathf.Round(rng.NextFloat()) == 1;

                    if (splitHorizontal) createdRooms = SplitHorizontally(room, graph.GetNeighbors(room).ToArray());
                    else createdRooms = SplitVertically(room, graph.GetNeighbors(room).ToArray());

                    graph.RemoveNode(room);
                }
                else if (room.width > maxRoomSize.x) // width is too big
                {
                    createdRooms = SplitVertically(room, graph.GetNeighbors(room).ToArray());

                    graph.RemoveNode(room);
                }
                else if (room.height > maxRoomSize.y) // height is too big
                {
                    createdRooms = SplitHorizontally(room, graph.GetNeighbors(room).ToArray());

                    graph.RemoveNode(room);
                }
                else // room is finished - move to generatedRooms
                {
                    generatedRooms.Add(room);
                }

                foreach((RectInt, RectInt[]) newRoom in createdRooms)
                {
                    newRooms.Add(newRoom.Item1);

                    // add the node to the graph
                    graph.AddNode(newRoom.Item1);
                    foreach (RectInt adjacentRoom in newRoom.Item2) graph.AddEdge(newRoom.Item1, adjacentRoom);
                }

                if (showRooms)
                {
                    foreach (RectInt room2 in roomsToCheck) { AlgorithmsUtils.DebugRectInt(room2, Color.yellow, waitTime); }

                    foreach (RectInt createdRoom in newRooms) { AlgorithmsUtils.DebugRectInt(createdRoom, Color.cyan, waitTime); }
                }

                if(doAnimation) yield return new WaitForSeconds(waitTime);
            }
            roomsToCheck = newRooms;
        }

        generationFinished = true;
    }
    (RectInt, RectInt[])[] SplitVertically(RectInt room, RectInt[] adjacentRooms)
    {
        (RectInt, RectInt[])[] newRooms = new (RectInt, RectInt[])[2];

        float splitRatio = 2 + rng.NextFloat(-maxSplitOffset, maxSplitOffset);

        RectInt newRoom1 = new RectInt(room.x, room.y, Mathf.RoundToInt((float)(room.width / splitRatio)), room.height);
        RectInt newRoom2 = new RectInt(room.x + newRoom1.width - 1, room.y, room.width - newRoom1.width + 1, room.height);

        List<RectInt> room1Connections = new();
        room1Connections.Add(newRoom2);
        foreach (RectInt adjacentRoom in adjacentRooms)
        {
            RectInt intersection = AlgorithmsUtils.Intersect(newRoom1, adjacentRoom);
            if (intersection.width * intersection.height >= doorArea)
            {
                room1Connections.Add(adjacentRoom);
            }
        }

        List<RectInt> room2Connections = new();
        room2Connections.Add(newRoom1);
        foreach (RectInt adjacentRoom in adjacentRooms)
        {
            RectInt intersection = AlgorithmsUtils.Intersect(newRoom2, adjacentRoom);
            if (intersection.width * intersection.height >= doorArea)
            {
                room2Connections.Add(adjacentRoom);
            }
        }

        newRooms[0] = (newRoom1, room1Connections.ToArray());
        newRooms[1] = (newRoom2, room2Connections.ToArray());

        return newRooms;
    }
    (RectInt, RectInt[])[] SplitHorizontally(RectInt room, RectInt[] adjacentRooms)
    {
        (RectInt, RectInt[])[] newRooms = new (RectInt, RectInt[])[2];

        float splitRatio = 2 + rng.NextFloat(-maxSplitOffset, maxSplitOffset);

        RectInt newRoom1 = new RectInt(room.x, room.y, room.width, Mathf.RoundToInt((float)(room.height / splitRatio)));
        RectInt newRoom2 = new RectInt(room.x, room.y + newRoom1.height - 1, room.width, room.height - newRoom1.height + 1);
        
        List<RectInt> room1Connections = new();
        room1Connections.Add(newRoom2);
        foreach (RectInt adjacentRoom in adjacentRooms)
        {
            RectInt intersection = AlgorithmsUtils.Intersect(newRoom1, adjacentRoom);
            if (intersection.width * intersection.height >= doorArea)
            {
                room1Connections.Add(adjacentRoom);
            }
        }

        List<RectInt> room2Connections = new();
        room2Connections.Add(newRoom1);
        foreach (RectInt adjacentRoom in adjacentRooms)
        {
            RectInt intersection = AlgorithmsUtils.Intersect(newRoom2, adjacentRoom);
            if (intersection.width * intersection.height >= doorArea)
            {
                room2Connections.Add(adjacentRoom);
            }
        }

        newRooms[0] = (newRoom1, room1Connections.ToArray());
        newRooms[1] = (newRoom2, room2Connections.ToArray());

        return newRooms;
    }

    // Complexity O(n^2) where n is the number of rooms
    [Obsolete("deprecated, included in room creation")] IEnumerator GenerateGraph()
    {
        foreach(RectInt room1 in generatedRooms)
        {
            graph.AddNode(room1);
            foreach(RectInt room2 in generatedRooms)
            {
                if (room1 == room2) continue;

                RectInt intersection = AlgorithmsUtils.Intersect(room1, room2);

                if (intersection.width * intersection.height >= doorArea) // the rooms are connectable
                {
                    graph.AddEdge(room1, room2);
                }

                AlgorithmsUtils.DebugRectInt(room1, Color.cyan, waitTime, false, 1);
                //AlgorithmsUtils.DebugRectInt(room2, Color.cyan, waitTime, false, 1);

                //if (doAnimation) yield return new WaitForSeconds(waitTime);
            }
            if (doAnimation) yield return new WaitForSeconds(waitTime);
        }
    }

    // Complexity O(n) where n is the number of rooms
    IEnumerator RemoveRooms()
    {
        int AmountToRemove = Mathf.RoundToInt(generatedRooms.Count * (removePercentage / 100f));
        List<RectInt> orderedRooms = new(graph.GetNodes());
        orderedRooms = orderedRooms.OrderBy(x => x.width * x.height).ToList();

        int indexToRemove = 0;
        for (int i = 0; i < AmountToRemove - 1; i++)
        {
            if (indexToRemove >= orderedRooms.Count) break;

            RectInt roomToRemove = orderedRooms[indexToRemove];

            RectInt[] connectedRooms = graph.GetNeighbors(roomToRemove).ToArray();

            graph.RemoveNode(roomToRemove);

            if(!graph.BFS(graph.GetNodes()[0])) // if the graph is no longer fully connected
            {
                // add the node and it's connections back to the graph
                graph.AddNode(roomToRemove);
                foreach (RectInt room in connectedRooms)
                {
                    graph.AddEdge(roomToRemove, room);
                    graph.AddEdge(room, roomToRemove);
                }

                i--; //retry
            }

            indexToRemove++;

            if (doAnimation) yield return new WaitForSeconds(waitTime);
        }

        removalFinished = true;
    }

    // Complexity O(n) where n is the number of vertecis and edges of the graph
    IEnumerator RemoveCyclicPaths()
    {
        Graph<RectInt> newGraph = new Graph<RectInt>();

        // generate new edges/paths
        if(useDFS) // use DFS
        {
            Stack<RectInt> stack = new();

            HashSet<RectInt> discovered = new();

            RectInt startNode = graph.GetNodes()[0];

            stack.Push(startNode);

            discovered.Add(startNode);

            while (stack.Count > 0)
            {
                RectInt node = stack.Pop();

                foreach (RectInt connectedNode in graph.GetNeighbors(node))
                {
                    if (!discovered.Contains(connectedNode)) // found a new room
                    {
                        newGraph.AddEdge(node, connectedNode); // add the connection

                        stack.Push(connectedNode);
                        discovered.Add(connectedNode);
                    }

                    if (showGraph) DrawGraph(newGraph, waitTime);
                    if (doAnimation) yield return new WaitForSeconds(waitTime);
                }

                if (showGraph) DrawGraph(newGraph, waitTime);
                if (doAnimation) yield return new WaitForSeconds(waitTime);
            }
        }
        else // use BFS
        {
            Queue<RectInt> queue = new();

            HashSet<RectInt> discovered = new();

            RectInt startNode = graph.GetNodes()[0];

            queue.Enqueue(startNode);
            discovered.Add(startNode);

            while (queue.Count > 0)
            {
                RectInt node = queue.Dequeue();

                foreach (RectInt connectedNode in graph.GetNeighbors(node))
                {
                    if (!discovered.Contains(connectedNode)) // found a new room
                    {
                        newGraph.AddEdge(node, connectedNode); // add the connection

                        queue.Enqueue(connectedNode);
                        discovered.Add(connectedNode);
                    }

                    DrawGraph(newGraph, waitTime);
                    if (doAnimation) yield return new WaitForSeconds(waitTime);
                }

                DrawGraph(newGraph, waitTime);
                if (doAnimation) yield return new WaitForSeconds(waitTime);
            }
        }

        // replace the old graph
        graph = newGraph;

        removedCyclicPaths = true;
    }

    // Complexity O(n) where n is the number of edges in the graph, note that this changed after removing the cyclic paths
    IEnumerator SpawnDoors()
    {
        foreach(RectInt node in graph.GetNodes())
        {
            foreach(RectInt connectedNode in graph.GetNeighbors(node))
            {
                Door newDoor = new();

                newDoor.room1 = node;
                newDoor.room2 = connectedNode;

                bool isDuplicate = doors.Any(door =>
                    (newDoor.room1 == door.room1 && newDoor.room2 == door.room2)
                    ||
                    (newDoor.room1 == door.room2 && newDoor.room2 == door.room1));

                if (isDuplicate) continue;

                RectInt intersection = AlgorithmsUtils.Intersect(node, connectedNode);

                bool isHallwayDoor = rng.NextFloat() <= hallwayChance;

                if (isHallwayDoor)
                {
                    newDoor.rect = intersection.height == 1 ? // check orientation
                        new RectInt(intersection.x + 1, intersection.y, intersection.width - 2, 1) // horizontal
                        :
                        new RectInt(intersection.x, intersection.y + 1, 1, intersection.height - 2); // vertical
                }
                else
                {
                    newDoor.rect = intersection.height == 1 ? // check orientation
                        new RectInt(rng.NextInt(intersection.xMin + 1, intersection.xMax - 3), intersection.y, 2, 1) // door is horizontal
                        :
                        new RectInt(intersection.x, rng.NextInt(intersection.yMin + 1, intersection.yMax - 3), 1, 2); // door is vertical 
                }

                doors.Add(newDoor);

                if (doAnimation) yield return new WaitForSeconds(waitTime);
            }

            if (doAnimation) yield return new WaitForSeconds(waitTime);
        }
    }

    // Complexity O(n) where n is the area of the entire dungeon
    IEnumerator SpawnAssets()
    {
        // create parent
        assetParent = new GameObject("Dungeon");

        Dictionary<RoomManager, List<Vector2>> perRoomFloorMap = new(); 
        Dictionary<Vector2, GameObject> globalFloorMap = new(); // contains spawned object by position in order to share references between different rooms

        int roomIndex = 0;
        foreach (RectInt room in graph.GetNodes())
        {
            Transform roomParent = new GameObject($"Room{roomIndex}").transform;
            roomParent.parent = assetParent.transform;
            RoomManager roomManager = roomParent.AddComponent<RoomManager>();
            roomManager.rect = room;

            Transform floorParent = new GameObject("Floor").transform;
            floorParent.parent = roomParent.transform;
            roomManager.floorParent = floorParent;

            Transform wallParent = new GameObject("Walls").transform;
            wallParent.parent = roomParent.transform;
            roomManager.wallParent = wallParent;

            perRoomFloorMap.Add(roomManager, new());
            roomManagers.Add(roomManager);

            // Map Floors
            foreach (Vector2Int position in room.allPositionsWithin)
            {
                perRoomFloorMap[roomManager].Add(new Vector2(position.x + .5f, position.y + .5f));
            }

            roomIndex++;
        }

        // spawn floors and walls
        foreach (KeyValuePair<RoomManager, List<Vector2>> kvp in perRoomFloorMap)
        {
            RoomManager roomManager = kvp.Key;

            foreach (Vector2 position in kvp.Value)
            {
                if (!globalFloorMap.ContainsKey(position)) // instantiate only non-duplicate floors
                    globalFloorMap.Add(position, Instantiate(floorPrefab, new Vector3(position.x, 0, position.y), Quaternion.identity, kvp.Key.floorParent));

                roomManager.floors.Add(globalFloorMap[position]); // add reference to roomManager

                if(doAnimation) yield return new WaitForSeconds(waitTime);
            }

            RectInt roomBounds = roomManager.rect;

            foreach (Vector2Int position in roomBounds.allPositionsWithin)
            {
                if (doors.Any(door => door.rect.Contains(position))) continue;

                Vector2 Apos = position + new Vector2(-.5f, .5f);
                Vector2 Bpos = position + new Vector2(.5f, .5f);
                Vector2 Cpos = position + new Vector2(-.5f, -.5f);
                Vector2 Dpos = position + new Vector2(.5f, -.5f);

                // invalidate all top and right positions to treat them as empty
                bool A = kvp.Value.Contains(Apos) && Apos.x < roomBounds.xMax - 1 && Apos.y < roomBounds.yMax - 1;
                bool B = kvp.Value.Contains(Bpos) && Bpos.x < roomBounds.xMax - 1 && Bpos.y < roomBounds.yMax - 1;
                bool C = kvp.Value.Contains(Cpos) && Cpos.x < roomBounds.xMax - 1 && Cpos.y < roomBounds.yMax - 1;
                bool D = kvp.Value.Contains(Dpos) && Dpos.x < roomBounds.xMax - 1 && Dpos.y < roomBounds.yMax - 1;

                int index = (A ? 8 : 0) | (B ? 4 : 0) | (C ? 2 : 0) | (D ? 1 : 0);

                if (wallPrefabs[index] == null) 
                {
                    continue;
                }

                GameObject wall = Instantiate(wallPrefabs[index], new Vector3(position.x + .5f, 0, position.y + .5f), Quaternion.identity, roomManager.wallParent);
                roomManager.walls.Add(wall);

                if (doAnimation) yield return new WaitForSeconds(waitTime);
            }

            Transform doorParent = new GameObject("Doors").transform;
            doorParent.transform.parent = roomManager.transform;
            roomManager.doorParent = doorParent;

            foreach (Door door in doors.Where(d => d.room1 == roomManager.rect || d.room2 == roomManager.rect))
            {
                GameObject doorHighlight = Instantiate(doorHighlightPrefab, doorParent);
                doorHighlight.transform.localScale = new Vector3(door.rect.width, 1, door.rect.height);
                doorHighlight.transform.position = new Vector3(door.rect.x + door.rect.width / 2f, 0, door.rect.y + door.rect.height / 2f);
                roomManager.doors.Add(doorHighlight);
            }

            roomManager.Initialize();
        }

        

        foreach (RoomManager roomManager in roomManagers)
            roomManager.neighbours = roomManagers.Where(rm => graph.GetNeighbors(roomManager.rect).Contains(rm.rect)).ToList();
    }

    // Complexity O(n) where n is the area of the entire dungeon
    IEnumerator CreateNavigationGraph()
    {
        // map all detailed positions
        foreach(RectInt room in graph.GetNodes())
        {
            foreach(Vector2Int position in room.allPositionsWithin)
            {
                Vector3 adjustedPosition = new Vector3(position.x + .5f, 0, position.y + .5f);

                if (spawnedDecorationPositions.Contains(adjustedPosition)) continue;

                if (!(position.x == room.x || position.x == room.xMax - 1 || position.y == room.y || position.y == room.yMax - 1))
                {
                    navigationGraph.AddNode(new Vector3(position.x + .5f, 0, position.y + .5f));
                }

                if(doAnimation) yield return new WaitForSeconds(waitTime);
            }
        }

        Vector3[] positions = navigationGraph.GetNodes().ToArray();

        // connect all neighbours
        foreach (Vector3 position in positions)
        {
            foreach(Vector2Int direction in orthogonalDirections)
            {
                if (positions.Contains(position + new Vector3(direction.x, 0, direction.y)))
                {
                    navigationGraph.AddEdge(position, position + new Vector3(direction.x, 0, direction.y));
                }
            }

            //foreach(Vector2Int direction in diagonalDirections)
            //{
            //    if (positions.Contains(position + new Vector3(direction.x, 0, direction.y)))
            //    {
            //        navigationGraph.AddEdge(position, position + new Vector3(direction.x, 0, direction.y));
            //    }
            //}

            if (doAnimation) yield return new WaitForSeconds(waitTime);
        }

        foreach (Door door in doors)
        {
            // add door positions
            foreach (Vector2Int position in door.rect.allPositionsWithin)
            {
                Vector3 adjustedPosition = new Vector3(position.x + .5f, 0, position.y + .5f);

                navigationGraph.AddNode(adjustedPosition);

                if (doAnimation) yield return new WaitForSeconds(waitTime);
            }
        }

        foreach(Door door in doors)
        {
            // connect door positions to other nodes
            foreach (Vector2Int position in door.rect.allPositionsWithin)
            {
                Vector3 adjustedPosition = new Vector3(position.x + .5f, 0, position.y + .5f);

                foreach (Vector2Int direction in orthogonalDirections)
                {
                    if (positions.Contains(adjustedPosition + new Vector3(direction.x, 0, direction.y)))
                    {
                        navigationGraph.AddEdge(adjustedPosition, adjustedPosition + new Vector3(direction.x, 0, direction.y));
                    }
                }

                // don't include diagonal connections for doors in order to stay clear of walls
                //foreach (Vector2Int direction in diagonalDirections)
                //{
                //    if (positions.Contains(adjustedPosition + new Vector3(direction.x, 0, direction.y)))
                //    {
                //        navigationGraph.AddEdge(adjustedPosition, adjustedPosition + new Vector3(direction.x, 0, direction.y));
                //    }
                //}

                if(doAnimation) yield return new WaitForSeconds(waitTime);
            }
        }
    }

    void HideAllRooms()
    {
        foreach (RoomManager room in roomManagers)
            room.HideAssets(true);
    }

    [HorizontalLine]
    [Header("Debugging")]
    public Transform cursor;
    public bool showRooms;
    public bool showGraph;
    public bool showNavigationGraph;
    public bool keyPressContinue;
    public bool showDoors;
    private void Update()
    {
        ToggleBools();

        if (showRooms)
            DrawRooms();

        if (showGraph)
            DrawGraph(graph, Time.deltaTime);

        if (showDoors)
            DrawDoors();

        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log(FindRoomAtPosition(cursor.position));
        }

        if (showNavigationGraph)
        {
            foreach (Vector3 node in navigationGraph.GetNodes())
            {
                DebugExtension.DebugCircle(node, Vector3.up, Color.magenta, .2f);
                foreach (Vector3 connection in navigationGraph.GetNeighbors(node))
                {
                    Debug.DrawLine(node, connection, Color.magenta);
                }
            } 
        }
    }
    void ToggleBools()
    {
        if (Input.GetKeyDown(KeyCode.R)) showRooms = !showRooms; 
        if (Input.GetKeyDown(KeyCode.G)) showGraph = !showGraph;
        if (Input.GetKeyDown(KeyCode.N)) showNavigationGraph = !showNavigationGraph;
        if (Input.GetKeyDown(KeyCode.A)) doAnimation = !doAnimation;
        if (Input.GetKeyDown(KeyCode.K)) keyPressContinue = !keyPressContinue;
        if (Input.GetKeyDown(KeyCode.D)) showDoors = !showDoors;
    }
    void DrawRooms()
    {
        foreach (RectInt room in generatedRooms) { AlgorithmsUtils.DebugRectInt(room, Color.green, Time.deltaTime); }
    }
    void DrawGraph(Graph<RectInt> graph, float time)
    {
        foreach(RectInt node in graph.GetNodes()) 
        {
            DebugExtension.DebugCircle(GetMiddle(node), Color.magenta, Mathf.Min(node.width, node.height) / 4, time); 
            foreach(RectInt connection in graph.GetNeighbors(node))
            {
                Debug.DrawLine(GetMiddle(node), GetMiddle(connection), Color.magenta, time);
            }
        }
    }
    void DrawDoors()
    {
        foreach (Door door in doors)
        {
            AlgorithmsUtils.DebugRectInt(door.rect, Color.cyan, Time.deltaTime, false, .1f);
        }
    }

    public UnityEvent onRedraw;
    [Button] void Redraw()
    {
        StopAllCoroutines();
        roomsToCheck.Clear();
        generatedRooms.Clear();
        generationFinished = false;
        graph.Clear();
        removalFinished = false;
        removedCyclicPaths = false;
        doors.Clear();
        Destroy(assetParent);
        spawnedDecorationPositions.Clear();
        navigationGraph.Clear();
        onRedraw.Invoke();

        StartCoroutine(Start());
    }
    [Button] void CheckGraphBFS()
    {
        graph.BFS(graph.GetNodes()[0], true);
    }
    [Button] void CheckGraphDFS()
    {
        graph.DFS(graph.GetNodes()[0], true);
    }

    // helper functions
    Vector3 GetMiddle(RectInt rect)
    {
        return new Vector3((float)rect.x + (float)rect.width / 2f, 0f, (float)rect.y + (float)rect.height / 2f);
    }
    RectInt FindRoomAtPosition(Vector3 position)
    {
        foreach (RectInt room in graph.GetNodes())
        {
            if (room.x + room.width < position.x) continue;
            if (room.y + room.height < position.z) continue;
            if (room.x > position.x) continue;
            if (room.y > position.z) continue;

            return room;
        }
        return default;
    }
}

[Serializable] public struct Door
{
    public RectInt rect;
    public RectInt room1;
    public RectInt room2;
}