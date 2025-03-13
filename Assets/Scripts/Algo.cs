using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text.Json;
using UnityEngine.Assertions;
using System;
using UnityEngine.UI;
using TMPro;

public enum CellState
{
    WALL,
    OCCUPIED,
    FREE
}

public class Robot
{
    public int x, y, z;
    private Vector3Int dir = new Vector3Int(1, 0, 0);
    bool ever_moved = false;
    private Vector3Int last_move;

    private Vector3Int[] all_directions()
    {
        return new Vector3Int[] {
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(0, 0, -1),
            new Vector3Int(1, 0, 0)
        };
    }

    private Vector3Int sucDir(Vector3Int dir)
    {
        if (dir.x == 1 && dir.y == 0 && dir.z == 0) return new Vector3Int(0, 1, 0);
        if (dir.x == 0 && dir.y == 1 && dir.z == 0) return new Vector3Int(0, 0, 1);
        if (dir.x == 0 && dir.y == 0 && dir.z == 1) return new Vector3Int(-1, 0, 0);
        if (dir.x == -1 && dir.y == 0 && dir.z == 0) return new Vector3Int(0, -1, 0);
        if (dir.x == 0 && dir.y == -1 && dir.z == 0) return new Vector3Int(0, 0, -1);
        if (dir.x == 0 && dir.y == 0 && dir.z == -1) return new Vector3Int(1, 0, 0);
        return new Vector3Int(0, 0, 0);
    }


    public Robot(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        x_target = x;
        y_target = y;
        z_target = z;
    }

    List<List<List<CellState>>> neighbours_tmp;

    private CellState getRelative(Vector3Int rel_coords)
    {
        return neighbours_tmp[rel_coords.x + 1][rel_coords.y + 1][rel_coords.z + 1];
    }

    private void setNextMoveDir(Vector3Int rel_coords)
    {
        x_target = x + rel_coords.x;
        y_target = y + rel_coords.y;
        z_target = z + rel_coords.z;
        last_move = rel_coords;
    }

    public List<Vector3Int> GetCompatibleDirs(Vector3Int dir) {
        List<Vector3Int> resi = new List<Vector3Int>();
        resi.Add(sucDir(dir));
        resi.Add(sucDir(sucDir(dir)));
        resi.Add(sucDir(sucDir(sucDir(sucDir(dir)))));
        resi.Add(sucDir(sucDir(sucDir(sucDir(sucDir(dir))))));
        return resi;
    }

    private bool reachable(int x1, int y1, int z1, int x2, int y2, int z2, List<List<List<CellState>>> neigh)
    {
        if (neigh[x1][y1][z1] == CellState.WALL || neigh[x2][y2][z2] == CellState.WALL) return false;
        bool[,,] reach = new bool[neigh.Count, neigh[0].Count, neigh[0][0].Count];
        reach[x1, y1, z1] = true;
        bool change = false;
        do
        {
            change = false;
            for(int i = 0; i < reach.GetLength(0); i++)
            {
                for (int j = 0; j < reach.GetLength(1); j++)
                {
                    for (int k = 0; k < reach.GetLength(2); k++)
                    {
                        for (int i2 = 0; i2 < reach.GetLength(0); i2++)
                        {
                            for (int j2 = 0; j2 < reach.GetLength(1); j2++)
                            {
                                for (int k2 = 0; k2 < reach.GetLength(2); k2++)
                                {
                                    if (Mathf.Abs(i - i2) <= 1 && Mathf.Abs(j - j2) <= 1 && Mathf.Abs(k - k2) <= 1 &&
                                        Mathf.Abs(i - i2)+ Mathf.Abs(j - j2)+ Mathf.Abs(k - k2) == 1)
                                    {
                                        if (reach[i,j,k] && !reach[i2,j2,k2] && neigh[i2][j2][k2] != CellState.WALL)
                                        {
                                            reach[i2, j2, k2] = true;
                                            change = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        } while (change);
        return reach[x2, y2, z2];
    }

    public void LookCompute(List<List<List<CellState>>> neighbours)
    {
        neighbours_tmp = neighbours;
        bool block_all = true;
        foreach (var dir in all_directions())
        {
            if (getRelative(dir) != CellState.WALL)
            {
                block_all = false;
            }
        }
        if (block_all)
        {
            active = false;
            return;
        }
        if (!ever_moved)
        {
            for (int i=0;i<6; i++)
            {
                if (getRelative(dir) == CellState.FREE)
                {
                    setNextMoveDir(dir);
                    ever_moved = true;
                    return;
                }
                dir = sucDir(dir);
            }
        }
        int sarok_count_neighbs = (getRelative(new Vector3Int(1, 0, 0)) == CellState.WALL || getRelative(new Vector3Int(-1, 0, 0)) == CellState.WALL ? 1 : 0) +
            (getRelative(new Vector3Int(0, 1, 0)) == CellState.WALL || getRelative(new Vector3Int(0, -1, 0)) == CellState.WALL ? 1 : 0) +
            (getRelative(new Vector3Int(0, 0, 1)) == CellState.WALL || getRelative(new Vector3Int(0, 0, -1)) == CellState.WALL ? 1 : 0);
        //Debug.Log($"sarok_count_neighbs for {x} {y} {z}: {sarok_count_neighbs}");
        if (sarok_count_neighbs == 3)
        {
            bool can_settle = true;
            //Trying to settle
            for(int i = 0; i <= 2; i++)
            {
                for (int j = 0; j <= 2; j++)
                {
                    for (int k = 0; k <= 2; k++)
                    {
                        for (int i2 = 0; i2 <= 2; i2++)
                        {
                            for (int j2 = 0; j2 <= 2; j2++)
                            {
                                for (int k2 = 0; k2 <= 2; k2++)
                                {
                                    if (i == 1 && j == 1 && k == 1) continue;
                                    if (i2 == 1 && j2 == 1 && k2 == 1) continue;
                                    bool can_traverse_now = reachable(i, j, k, i2, j2, k2, neighbours);
                                    neighbours[1][1][1] = CellState.WALL;
                                    bool can_traverse_later = reachable(i, j, k, i2, j2, k2, neighbours);
                                    neighbours[1][1][1] = CellState.OCCUPIED;
                                    if(can_traverse_now && ! can_traverse_later)
                                    {
                                        //Debug.Log($"Cannot settle at {x} {y} {z} because it would remove path between {i} {j} {k} and {i2} {j2} {k2}");
                                        can_settle = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            for(int i = -1; i <= 1; i += 2)
            {
                for (int j = -1; j <= 1; j += 2)
                {
                    for (int k = -1; k <= 1; k += 2)
                    {
                        if(getRelative(new Vector3Int(i, 0, 0)) != CellState.WALL &&
                            getRelative(new Vector3Int(0, j, 0)) != CellState.WALL &&
                            getRelative(new Vector3Int(0, 0, k)) != CellState.WALL &&
                            getRelative(new Vector3Int(i, j, 0)) != CellState.WALL &&
                            getRelative(new Vector3Int(0, j, k)) != CellState.WALL &&
                            getRelative(new Vector3Int(i, 0, k)) != CellState.WALL &&
                            getRelative(new Vector3Int(i,j,k)) == CellState.WALL)
                        {
                            can_settle = false;
                        }
                    }
                }
            }
            if (can_settle)
            {
                active = false;
                return;
            }
        }
        if (getRelative(dir) == CellState.FREE)
        {
            setNextMoveDir(dir);
            return;
        }
        foreach (var d in GetCompatibleDirs(last_move))
        {
            if (getRelative(d) == CellState.FREE)
            {
                dir = d;
                setNextMoveDir(dir);
                return;
            }
        }
        Debug.LogError($"Could not decide on a correct new direction Last move: {last_move} Compatbile dirs {string.Join(",", GetCompatibleDirs(last_move))}");
        Assert.IsTrue(false);
    }

    public void Move()
    {
        x = x_target;
        y = y_target;
        z = z_target;
    }

    public int x_target, y_target, z_target;

    public int settled_for = 0;

    public bool active = true;
}

public class Algo : MonoBehaviour
{
    private CellState GetCellState(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0 || x >= height || y >= width || z >= depth)
        {
            return CellState.WALL;
        }
        if (!map[x,y,z] || (current_robot_field[x, y, z] != null && !current_robot_field[x, y, z].active))
        {
            return CellState.WALL;
        }
        if (current_robot_field[x, y, z] != null)
        {
            return CellState.OCCUPIED;
        }
        return CellState.FREE;
    }
    private List<List<List<CellState>>> generateNeighbours(int x, int y, int z)
    {
        List<List<List<CellState>>> resi = new List<List<List<CellState>>>();
        for (int i = x - 1; i <= x + 1; i++)
        {
            List<List<CellState>> resi2 = new List<List<CellState>>();
            for (int j = y - 1; j <= y + 1; j++)
            {
                List<CellState> resi3 = new List<CellState>();
                for (int k = z - 1; k <= z + 1; k++)
                {
                    resi3.Add(GetCellState(i, j, k));
                }
                resi2.Add(resi3);
            }
            resi.Add(resi2);
        }
        return resi;
    }

    List<Robot> r;

    Robot[,,] current_robot_field;

    void GenerateRobotField()
    {
        current_robot_field = new Robot[height, width, depth];
        foreach (Robot robot in r)
        {
            if (current_robot_field[robot.x, robot.y, robot.z] == null)
            {
                if (map[robot.x, robot.y, robot.z])
                {
                    current_robot_field[robot.x, robot.y, robot.z] = robot;
                }
                else
                {
                    Debug.LogError("Robot went out of field");
                }
            }
            else
            {
                Debug.LogError("Robots collided");
            }
        }
    }

    public GameObject cube;
    public Material transparent;
    public Material moving;
    public Material settled;
    public Material start;
    int width = 5;
    int height = 5;
    int depth = 5;
    GameObject[,,] gameObjects;
    public string json_file_path;

    private bool[,,] map;

    int counter_dfs = 0;
    public int ticks_per_update = 50;

    int time = 0;
    int ticks_since_last_update = 0;

    private int start_x = 2, start_y = 2, start_z = 2;
    JsonElement re;

    void DropdownValueChanged(TMPro.TMP_Dropdown dropdown)
    {
        for (int x = 0; x < height; ++x)
        {
            for (int y = 0; y < width; ++y)
            {
                for (int z = 0; z < depth; ++z)
                {
                    if (map[x, y, z])
                    {
                        Destroy(gameObjects[x, y, z]);
                    }
                }
            }
        }
        Debug.Log(dropdown.value.ToString());
        string json = "";
        switch (dropdown.value)
        {
            case 0:
                json = PreppedMaps.mapOne();
                break;
            case 1:
                using (StreamReader r = new StreamReader(json_file_path))
                {
                    json = r.ReadToEnd();
                }
                break;
            case 2:
                json = PreppedMaps.mapTwo();
                break;
        }
        loadJson(json);
    }

    void loadJson(string json)
    {
        JsonDocument jd = JsonDocument.Parse(json);
        re = jd.RootElement;
        height = re.GetProperty("map").GetArrayLength();
        width = re.GetProperty("map")[0].GetArrayLength();
        depth = re.GetProperty("map")[0][0].GetArrayLength();
        gameObjects = new GameObject[height, width, depth];
        map = new bool[height, width, depth];

        int minx = int.MaxValue;
        int maxx = int.MinValue;
        int miny = int.MaxValue;
        int maxy = int.MinValue;
        int minz = int.MaxValue;
        int maxz = int.MinValue;
        for (int x = 0; x < height; ++x)
        {
            for (int y = 0; y < width; ++y)
            {
                for (int z = 0; z < depth; ++z)
                {
                    if (re.GetProperty("map")[x][y][z].GetBoolean())
                    {
                        minx = Math.Min(minx, x);
                        maxx = Math.Max(maxx, x);
                        miny = Math.Min(miny, y);
                        maxy = Math.Max(maxy, y);
                        minz = Math.Min(minz, z);
                        maxz = Math.Max(maxz, z);
                        map[x, y, z] = true;
                        gameObjects[x, y, z] = Instantiate(cube);
                        gameObjects[x, y, z].transform.parent = transform;
                        gameObjects[x, y, z].transform.SetLocalPositionAndRotation(new Vector3(x, y, z), Quaternion.identity);
                    }
                }
            }
        }
        Vector3Int egyik = new Vector3Int(minx, miny, minz);
        Vector3Int masik = new Vector3Int(maxx, maxy, maxz);
        transform.SetLocalPositionAndRotation(-egyik - masik / 2 + egyik / 2, Quaternion.identity);
        start_x = re.GetProperty("start").GetProperty("x").GetInt32();
        start_y = re.GetProperty("start").GetProperty("y").GetInt32();
        start_z = re.GetProperty("start").GetProperty("z").GetInt32();
        r = new List<Robot>();
        GenerateRobotField();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        GameObject dropdown = GameObject.Find("MapSelection");
        Debug.Log(dropdown);

        TMPro.TMP_Dropdown dropdown2 = dropdown.GetComponent<TMPro.TMP_Dropdown>();
        
        Debug.Log(dropdown2);
        dropdown2.onValueChanged.AddListener(delegate {
            DropdownValueChanged(dropdown2);
        });

        try
        {
            string json;
            using (StreamReader r = new StreamReader(json_file_path))
            {
                json = r.ReadToEnd();
            }
            loadJson(json);
        } catch(Exception ex)
        {
            height = 0;
            width = 0;
            depth = 0;
            r = new List<Robot>();
            gameObjects = new GameObject[height, width, depth];
            map = new bool[height, width, depth];
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    void FixedUpdate()
    {
        if (ticks_since_last_update >= GameObject.Find("Speed slider").GetComponent<Slider>().value)
        {
            foreach (Robot robot in r)
            {
                if(robot.active)
                    robot.LookCompute(generateNeighbours(robot.x, robot.y, robot.z));
            }

            if (current_robot_field[start_x, start_y, start_z] == null)
            {
                r.Add(new Robot(start_x, start_y, start_z));
            }

            foreach (Robot robot in r)
            {
                if (robot.active)
                {
                    robot.Move();
                } else
                {
                    ++robot.settled_for;
                }
            }

            GenerateRobotField();

            ticks_since_last_update = 0;
            ++time;

            for (int x = 0; x < height; ++x)
            {
                for (int y = 0; y < width; ++y)
                {
                    for (int z = 0; z < depth; ++z)
                    {
                        if (map[x, y, z])
                        {
                            if (current_robot_field[x, y, z] == null)
                            {
                                if (x == start_x && y == start_y && z == start_z)
                                {
                                    gameObjects[x, y, z].GetComponent<MeshRenderer>().material = start;
                                }
                                else
                                {
                                    gameObjects[x, y, z].GetComponent<MeshRenderer>().material = transparent;
                                }
                            }
                            else
                            {
                                if (current_robot_field[x, y, z].active)
                                {
                                    gameObjects[x, y, z].GetComponent<MeshRenderer>().material = moving;
                                }
                                else
                                {
                                    if (current_robot_field[x, y, z].settled_for <= 5)
                                    {
                                        gameObjects[x, y, z].GetComponent<MeshRenderer>().material = settled;
                                    }
                                    else
                                    {
                                        gameObjects[x, y, z].SetActive(false);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            ++ticks_since_last_update;
        }
    }
}
