using UnityEngine;
using UnityEngine.UIElements;

public class Rotator : MonoBehaviour
{
    [SerializeField]
    private float speed = 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(0, Time.fixedTime * speed, 0);
    }
    Bounds GetMaxBounds(MonoBehaviour g)
    {
        var b = new Bounds(g.transform.position, Vector3.zero);
        foreach (Renderer r in g.GetComponentsInChildren<Renderer>())
        {
            b.Encapsulate(r.bounds);
        }
        return b;
    }
}
