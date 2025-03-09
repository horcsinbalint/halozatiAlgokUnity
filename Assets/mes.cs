using UnityEngine;

public class mes : MonoBehaviour
{
    public float speed = 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(0, Time.fixedTime * speed, 0);
    }
}
