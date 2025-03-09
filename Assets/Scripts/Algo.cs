using UnityEngine;

public class Algo : MonoBehaviour
{
    public GameObject cube;
    public Material transparent;
    public Material moving;
    public Material settled;
    public Material start;
    public int width = 5;
    public int height = 5;
    public int depth = 5;
    public GameObject[,,] gameObjects;
    private bool[,,] mapp = { { { true, true, true, true },
        { true, true, true, true },
        { true, true, true, true }},
    { { true, true, true, true },
        { true, true, true, true },
        { true, true, true, true }},
    { { true, true, true, true },
        { true, true, true, true },
        { true, true, true, true }},
    { { true, true, true, true },
        { true, true, true, true },
        { true, true, true, true }},};

    private int[,,] reach;
    private int[,,] finish;

    int counter_dfs = 0;
    public int ticks_per_update = 50;

    public int time = 0;
    public int ticks_since_last_update = 0;

    public int start_x = 2, start_y = 2, start_z = 2;

    void dfs(int x, int y, int z, int dir1, int dir2, int dir3)
    {
        if (x >= 0 && y >= 0 && z >= 0 &&
           x < height && y < width && z < depth &&
           mapp[x, y, z] && reach[x, y, z] == 0)
        {
            ++counter_dfs;
            reach[x, y, z] = counter_dfs;
            if (dir1 != -1)
                dfs(x + 1, y, z, 1, dir2, dir3);
            if (dir2 != -1)
                dfs(x, y + 1, z, dir1, 1, dir3);
            if (dir3 != -1)
                dfs(x, y, z + 1, dir1, dir2, 1);
            if (dir1 != 1)
                dfs(x - 1, y, z, -1, dir2, dir3);
            if (dir2 != 1)
                dfs(x, y - 1, z, dir1, -1, dir3);
            if (dir3 != 1)
                dfs(x, y, z - 1, dir1, dir2, -1);
            ++counter_dfs;
            finish[x, y, z] = counter_dfs;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        height = mapp.GetLength(0);
        width = mapp.GetLength(1);
        depth = mapp.GetLength(2);
        gameObjects = new GameObject[height, width, depth];
        reach = new int[height, width, depth];
        finish = new int[height, width, depth];

        for (int x = 0; x < height; ++x)
        {
            for (int y = 0; y < width; ++y)
            {
                for (int z = 0; z < depth; ++z)
                {
                    if (mapp[x, y, z])
                    {
                        gameObjects[x, y, z] = Instantiate(cube, new Vector3(x, y, z), Quaternion.identity);
                        gameObjects[x, y, z].transform.parent = transform;
                    }
                }
            }
        }

        dfs(start_x, start_y, start_z, 0, 0, 0);

    }

    // Update is called once per frame
    void Update()
    {

    }
    void FixedUpdate()
    {
        if (ticks_since_last_update >= ticks_per_update)
        {
            ticks_since_last_update = 0;
            ++time;

            for (int x = 0; x < height; ++x)
            {
                for (int y = 0; y < width; ++y)
                {
                    for (int z = 0; z < depth; ++z)
                    {
                        if (mapp[x, y, z])
                        {
                            if (finish[x, y, z] + 5 <= time)
                            {
                                gameObjects[x, y, z].SetActive(false);
                            }
                            else if (finish[x, y, z] <= time)
                            {
                                gameObjects[x, y, z].GetComponent<MeshRenderer>().material = settled;
                            }
                            else if (reach[x, y, z] <= time && (time - reach[x, y, z]) % 2 == 0)
                            {
                                gameObjects[x, y, z].GetComponent<MeshRenderer>().material = moving;
                            }
                            else
                            {
                                if (start_x == x && start_y == y && start_z == z)
                                {
                                    gameObjects[x, y, z].GetComponent<MeshRenderer>().material = start;
                                }
                                else
                                {
                                    gameObjects[x, y, z].GetComponent<MeshRenderer>().material = transparent;
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
