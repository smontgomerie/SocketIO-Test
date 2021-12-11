using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[HelpURL("https://tibcosoftware.github.io/Augmented-Reality/3DCharts/")]
public class BarChartManager : MonoBehaviour
{
    [Header("Bar Prefab")] [Tooltip("store here your Ground Plane Prefab to be used.")]
    public GameObject GroundPrefab;

    [Tooltip("this is how each Bar of the Barchart should look like.")]
    public GameObject BarPrefab;

    [Tooltip("store here a simple TextMesh to be used.")]
    public GameObject LabelPrefab;

    [Tooltip("general Chart Label, below the BarChart.")]
    public string ChartLabel;

    [Tooltip("define if all bars should be rendered in centered mode.")]
    public bool centered;

    [Tooltip("displayed after each Scaling Variable.")]
    public string postFix;

    [Header("Bar Label")] [Tooltip("a Label Value shown at each Bar.")]
    public string[] BarLabel;

    [Header("Bar Scaling")] [Tooltip("the scale Value define size for each Bar.")]
    public float[] BarSize;

    [Header("Bar Color")] [Tooltip("Color to display for each Bar.")]
    public Color[] BarColor;

    private Dictionary<string, GameObject> bars = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> labels = new Dictionary<string, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        var scale = transform.localScale.x;
        var scale_y = transform.localScale.y;
        var scale_z = transform.localScale.z;

        this.transform.localScale = new Vector3(scale * BarSize.Length / 2, scale_y, scale_z);
        this.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        var GroundObj = Instantiate(GroundPrefab,
            new Vector3(transform.position.x, transform.position.y - scale_y / 2, transform.position.z),
            Quaternion.identity, transform);

        Renderer rend = GroundObj.GetComponent<Renderer>();
        rend.material.SetColor("_SpecColor", new Color32(222, 222, 222, 80));

        GroundObj.transform.localScale = new Vector3(1, 0.01f, scale_z / 2);
        GroundObj.GetComponent<Rigidbody>().mass = 1;
        GroundObj.transform.SetParent(this.transform, false);
        GroundObj.transform.localRotation = Quaternion.identity;

        //Chart Bars
        for (int i = 0; i < BarSize.Length; i++)
        {
            // Bar Rendering
            // var BarObj = Instantiate(BarPrefab, new Vector3(transform.position.x - transform.localScale.x / 2 + transform.localScale.x/BarSize.Length*i + 0.04f, transform.position.y, transform.position.z), Quaternion.identity);
            // var BarObj = Instantiate(BarPrefab, new Vector3(0 - transform.localScale.x / 2 + transform.localScale.x/BarSize.Length*i + 0.04f, 0, 0), Quaternion.identity, transform);
            var BarObj = Instantiate(BarPrefab, transform);
            // BarObj.transform.parent = this.transform;
            BarObj.transform.localRotation = Quaternion.identity;
            BarObj.transform.localPosition =
                new Vector3(1 / (float) BarSize.Length * i - 0.5f + 1 / ((float) BarSize.Length * 2), 0, 0);


            rend = BarObj.GetComponent<Renderer>();
            rend.material.SetColor("_SpecColor", BarColor[i]);
            bars[BarLabel[i].ToLower()] = BarObj;

            var scaledSize = (1f / 100) * BarSize[i];
            BarObj.GetComponent<Bar>().Size = scaledSize;
            BarObj.GetComponent<Bar>().numItems = BarSize.Length;

            BarObj.GetComponent<Bar>().scale = scale;
            BarObj.GetComponent<Bar>().centered = centered;
            BarObj.GetComponent<Bar>().displayColor = BarColor[i];
            BarObj.GetComponent<Rigidbody>().mass = 1;

            // Value Rendering
            // var ValueObj = Instantiate(LabelPrefab, new Vector3(transform.position.x - transform.localScale.x / 2 + transform.localScale.x / BarSize.Length * i + 0.04f, transform.position.y + scaledSize*scale/2, transform.position.z - transform.localScale.z/4), Quaternion.identity);
            var ValueObj = Instantiate(LabelPrefab);
            ValueObj.transform.localScale = new Vector3(0.09f * scale, 0.09f * scale, 0.09f * scale);
            ValueObj.GetComponent<TextMesh>().text = BarSize[i].ToString() + postFix;
            ValueObj.transform.parent = this.transform;
            ValueObj.transform.localRotation = Quaternion.identity;
            ValueObj.transform.localPosition =
                new Vector3(1 / (float) BarSize.Length * i - 0.5f + 1 / ((float) BarSize.Length * 2), 0, -0.5f);
            labels[BarLabel[i].ToLower()] = ValueObj;

            // Label Rendering
            // var BarLabelObj = Instantiate(LabelPrefab, new Vector3(transform.position.x - transform.localScale.x / 2 + transform.localScale.x / BarSize.Length * i + 0.04f, transform.position.y - scale_y / 2 + 0.1f * scale, transform.position.z - transform.localScale.z/4), Quaternion.identity);
            var BarLabelObj = Instantiate(LabelPrefab);
            BarLabelObj.transform.localScale = new Vector3(0.09f * scale, 0.09f * scale, 0.09f * scale);
            BarLabelObj.GetComponent<TextMesh>().text = BarLabel[i];
            BarLabelObj.transform.parent = this.transform;
            BarLabelObj.transform.localPosition =
                new Vector3(1 / (float) BarSize.Length * i - 0.5f + 1 / ((float) BarSize.Length * 2), -0.4f, -0.5f);
        }

        //Chart Label
        var LabelObj = Instantiate(LabelPrefab,
            new Vector3(transform.position.x, transform.position.y - scale_y / 2 - 0.1f * scale,
                transform.position.z - transform.localScale.z / 2), Quaternion.identity);
        LabelObj.transform.localScale = new Vector3(0.1f * scale, 0.1f * scale, 0.1f * scale);
        LabelObj.GetComponent<TextMesh>().text = ChartLabel;
        LabelObj.transform.parent = this.transform;
    }

    public void SetBarSize(string label, float value)
    {
        GameObject barObj, labelObj;
        if (!bars.TryGetValue(label, out barObj))
        {
            bars.TryGetValue(label.ToLower(), out barObj);
        }

        if (!labels.TryGetValue(label, out labelObj))
        {
            labels.TryGetValue(label.ToLower(), out labelObj);
        }

        if (!(barObj is null)) barObj.GetComponent<Bar>().Size = (1f / 100) * value;
        if (!(labelObj is null)) labelObj.GetComponent<TextMesh>().text = value + postFix;
    }
}