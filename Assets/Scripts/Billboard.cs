using UnityEngine;

public class Billboard : MonoBehaviour
{
    public bool keepUpright = true;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null) { cam = Camera.main; if (cam == null) return; }

        Vector3 dir = transform.position - cam.transform.position;
        if (keepUpright) dir.y = 0;

        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }
}