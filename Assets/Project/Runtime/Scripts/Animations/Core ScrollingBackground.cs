using UnityEngine;
using UnityEngine.UI;

public class ScrollingBackground : MonoBehaviour
{
    [SerializeField] private Image imgScroll;
    [SerializeField] private float x, y;

    private Material runtimeMaterial;

    void Start()
    {
        runtimeMaterial = Instantiate(imgScroll.material);
        imgScroll.material = runtimeMaterial;
    }
    
    void Update()
    {
        ScrollImage();
    }

    void ScrollImage()
    {
        runtimeMaterial.mainTextureOffset += new Vector2(x, y) * Time.deltaTime;
    }
}
