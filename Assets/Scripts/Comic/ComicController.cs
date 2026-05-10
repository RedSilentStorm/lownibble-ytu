using System.Collections;
using UnityEngine;

public class ComicController : MonoBehaviour
{
    public Transform[] panels;
    public AudioSource audioSource;

    public float moveSpeed = 2f;
    public float zoomSize = 2f;

    private int index = 0;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        Focus(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Next();
        }
    }

    void Next()
    {
        index++;

        if (index < panels.Length)
        {
            Focus(index);
            audioSource.Play();
        }
    }

    void Focus(int i)
    {
        StopAllCoroutines();
        StartCoroutine(Move(panels[i]));
    }

    IEnumerator Move(Transform target)
    {
        Vector3 startPos = cam.transform.position;
        float startZoom = cam.orthographicSize;

        Vector3 targetPos = new Vector3(
            target.position.x,
            target.position.y,
            -10
        );

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * moveSpeed;

            cam.transform.position = Vector3.Lerp(startPos, targetPos, t);
            cam.orthographicSize = Mathf.Lerp(startZoom, zoomSize, t);

            yield return null;
        }
    }
}