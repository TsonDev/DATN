using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    [Header("UI references")]
    public RectTransform mapParrent;
    public GameObject areaPrefab;
    public RectTransform playerIcon;
    [Header("Colours")]
    //public Color defaultColor = Color.gray;
    public Color defaultColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    //public Color currentAreaColor = Color.green;
    public Color currentAreaColor = Color.white;
    [Header("Map setting")]
    public GameObject MapBounds;
    public PolygonCollider2D startArea;
    public float mapScale = 10f;

    private PolygonCollider2D[] AreaMaps;
    private Dictionary<string, RectTransform> uiAreas = new Dictionary<string, RectTransform>();

    public static MapController Instance { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        AreaMaps = MapBounds.GetComponentsInChildren<PolygonCollider2D>();
    }
    public void GenerateMap(PolygonCollider2D newCurrentArea = null)
    {
        PolygonCollider2D currentArea = newCurrentArea != null ? newCurrentArea : startArea;
        ClearMap();
        foreach (PolygonCollider2D area in AreaMaps)
        {
            //CreateUiArea
             CreateAreaUi(area, area == currentArea);
        }
        MovePlayerIcon(currentArea.name);

    }
    //clear map
    private void ClearMap()
    {
        foreach (Transform child in mapParrent)
        {
            Destroy(child.gameObject);
        }
        uiAreas.Clear();
    }
    private void CreateAreaUi(PolygonCollider2D area, bool isCurrent)
    {
        //Tao prefab for image
        GameObject areaImage = Instantiate(areaPrefab, mapParrent);
        RectTransform rect = areaImage.GetComponent<RectTransform>();
        //get bounds
        Bounds bounds = area.bounds;
        //Scale ui fit with map and bounds
        rect.sizeDelta = new Vector2(bounds.size.x * mapScale, bounds.size.y * mapScale);
        rect.anchoredPosition = bounds.center * mapScale;

        Image img = areaImage.GetComponent<Image>();

        // lấy sprite từ AreaData
        AreaData data = area.GetComponent<AreaData>();
        if (data != null)
        {
            img.sprite = data.mapSprite;
        }

        img.color = isCurrent ? currentAreaColor : defaultColor;
        if (isCurrent)
        {
            rect.SetAsLastSibling(); // luôn vẽ trên cùng
        }


        ////Set color for base or not
        //areaImage.GetComponent<Image>().color = isCurrent ? currentAreaColor : defaultColor;
        //Add to dictionary
        uiAreas[area.name] = rect;
    } 
    //Update current Area
    public void UpdateCurrentArea(string newCurrentArea)
    {
        //update color
        foreach(KeyValuePair<string, RectTransform> area in uiAreas)
        {
            area.Value.GetComponent<Image>().color = area.Key == newCurrentArea ? currentAreaColor : defaultColor;
        }
        //Move player icon
        MovePlayerIcon(newCurrentArea);
    }
    private void MovePlayerIcon(string newCurrentArea)
    {
        if (uiAreas.TryGetValue(newCurrentArea, out RectTransform areaUi))
        {
            playerIcon.anchoredPosition = areaUi.anchoredPosition;
        }
    }

}
