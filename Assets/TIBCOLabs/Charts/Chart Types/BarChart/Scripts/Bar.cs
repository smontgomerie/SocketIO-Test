using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bar : MonoBehaviour
{
    public GameObject LabelPrefab;

    public float scale;
    static float defaultSize = 0.07f;
    private float size = defaultSize;
    public bool centered;
    public Color32 displayColor;

    private float currSize;
    public int numItems;

    public float Size
    {
        get => size;
        set
        {
            size = value;
            SetScale(size);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        currSize = 0.007f;
    }

    // Update is called once per frame
    void Update()
    {
        //Scale 
        // currSize += 2f * Time.deltaTime;

        currSize += Mathf.Lerp(defaultSize, Size, Time.deltaTime);
        
        //max Scale
        if (currSize  <= Size)
        {
            Debug.Log($"currSize: {currSize} size: {Size}");
            SetScale(currSize);

            if (!centered)
            {
                transform.position = new Vector3(transform.position.x, transform.parent.position.y - transform.parent.localScale.y/2 + transform.parent.localScale.y * currSize /2, transform.position.z);
            }
        }
    }

    private void SetScale(float size)
    {
        var scaleX = 1/ (float) (numItems * 2);
        transform.localScale = new Vector3(scaleX, size, scale);
        
        if (!centered)
        {
            transform.position = new Vector3(transform.position.x, transform.parent.position.y - transform.parent.localScale.y/2 + transform.parent.localScale.y * size /2, transform.position.z);
        }
    }

    private void OnMouseEnter()
    {
        //mouse over Bar
        Renderer rend = GetComponent<Renderer>();
        rend.material.SetColor("_SpecColor", new Color32(100,100,100,100));
    }

    private void OnMouseExit()
    {
        //mouse left Bar
        Renderer rend = GetComponent<Renderer>();
        rend.material.SetColor("_SpecColor", displayColor);
    }
}
