using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    public static Minimap Instance;

    public RectTransform MinimapContents;

    [SerializeField]
    Vector2 realMapDimensions;

    [SerializeField]
    RectTransform scrollViewRectTransform;

    [SerializeField]
    RectTransform contentRectTransform;

    [SerializeField]
    MinimapIcon minimapIconPrefab;

    [SerializeField]
    Matrix4x4 transformationMaxtrix;

    [SerializeField]
    Vector2 currentPlayerIconPosition;
    public Dictionary<MinimapWorldObject, MinimapIcon> minimapWorldObjectDictrionary = new Dictionary<MinimapWorldObject, MinimapIcon>();

    public Dictionary<MinimapWorldObject, MinimapIcon> minimapOFFScreenObjectDictionary = new Dictionary<MinimapWorldObject, MinimapIcon>();

    [SerializeField]
    public Vector2 OutOfBoundsDimension;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CalculateTransformationMatrix();
    }

    private void Update()
    {
        UpdateMinimapIcons();
    }

    public void RegisterMinimapWorldObject(MinimapWorldObject minimapWorldObject, Color offScreenColor)
    {
        var minimapIcon = Instantiate(minimapIconPrefab);
        minimapIcon.transform.SetParent(contentRectTransform);

        Random.InitState((int)System.DateTime.Now.Ticks * 256);
        Color c = Random.ColorHSV();
        c = new Color(
            Mathf.Min(255f / 255f, c.r) + 150f / 255f,
            Mathf.Min(100f / 255f, c.g) + 50f / 255f,
            Mathf.Min(100f / 255f, c.b) + 50f / 255f,
            1f);
        

        if (!minimapWorldObjectDictrionary.ContainsKey(minimapWorldObject))
        {
            minimapIcon.SetIcon(minimapWorldObject.Icon);
            if (minimapWorldObject.pv.viewID != GameManager.Instance.MyPlayerID)
                minimapIcon.SetColor(new Color(c.r, c.g, c.b, c.a));
            else
            {
                minimapIcon.SetColor(new Color(184f / 255f, 255 / 255f, 100 / 255f, 1f));
                minimapIcon.IconText.color = minimapIcon.IconImage.color;
            }
            minimapIcon.SetText(minimapWorldObject.Text);
            minimapIcon.SetTextSize(minimapWorldObject.TextSize);
            minimapIcon.PrefabRectTransform.localScale = Vector3.one;
            minimapIcon.SetIsLocalMinimapPlayer(minimapWorldObject.isLocalMinimapPlayer);

            minimapWorldObjectDictrionary.Add(minimapWorldObject, minimapIcon);
        }
        else
        {
            Debug.Log("Icon: " + minimapWorldObject.pv.viewID + " is already in the dictioary");
        }
        if(minimapWorldObject.pv.viewID == GameManager.Instance.MyPlayerID)
        {
            LocalPlayerIconPosltion = -WorldPositionToMapPosition(minimapWorldObject.transform.position);
            tempMinimapIcon.SetText("#" + StatManager.Instance.GetPlayerStats(minimapWorldObject.pv.viewID, StatType.Rank));
            Debug.Log("Labeling Player Icon Text: " + "#" + StatManager.Instance.GetPlayerStats(minimapWorldObject.pv.viewID, StatType.Rank));
        }
        else
        {
            tempMinimapIcon = Instantiate(tempMinimapIconPrefab);
            tempMinimapIcon.transform.SetParent(contentRectTransform);
            tempMinimapIcon.SetIcon(minimapSpriteOffScreen);
            Random.InitState((int)Time.time);
            tempMinimapIcon.SetColor(new Color(c.r, c.g, c.b, c.a));

            //tempMinimapIcon.SetText("P" + minimapWorldObject.pv.viewID + ": " + StatManager.Instance.GetPlayerStats(minimapWorldObject.pv.viewID, StatType.Rank));
            tempMinimapIcon.SetText("#" + StatManager.Instance.GetPlayerStats(minimapWorldObject.pv.viewID, StatType.Rank));
            tempMinimapIcon.SetTextSize(minimapWorldObject.TextSize);
            tempMinimapIcon.PrefabRectTransform.localScale = Vector3.one;
            
            minimapOFFScreenObjectDictionary.Add(minimapWorldObject, tempMinimapIcon);
        }
    }

    Vector2 mapCenterDistance;
    public MinimapIcon tempMinimapIconPrefab;
    MinimapIcon tempMinimapIcon;
    public Sprite minimapSpriteOffScreen;

    Vector2 tempPos;
    float tempX;
    float tempY;

    private Vector2 LocalPlayerIconPosltion;
    private void UpdateMinimapIcons()
    {
        foreach(var keyValuePair in minimapWorldObjectDictrionary)
        {
            var minimapWorldObject = keyValuePair.Key;
            var minimapIcon = keyValuePair.Value;

            var mapPosition = Vector2.zero;
            if (minimapWorldObject)
            {
                mapPosition = -WorldPositionToMapPosition(minimapWorldObject.transform.position);

                if (minimapWorldObject.pv.viewID == GameManager.Instance.MyPlayerID)
                {
                    currentPlayerIconPosition = mapPosition;
                    mapCenterDistance = CalculateLocalPlayerCenterDistance(mapPosition);
                    contentRectTransform.anchoredPosition = mapCenterDistance;
                }

                minimapIcon.PrefabRectTransform.anchoredPosition = mapPosition;

                if (minimapWorldObject.pv.viewID != GameManager.Instance.MyPlayerID)
                {
                    //Not Local Player, so if otherPlayers distX > dx, and/or distY > dy shift it to the outside border on edge of minimap to be visible still.
                    //var DistanceToPlayer = CalculateDistanceToLocalPlayer(mapPosition);
                    if (currentPlayerIconPosition.x - mapPosition.x > OutOfBoundsDimension.x && mapPosition.y - currentPlayerIconPosition.y > OutOfBoundsDimension.y)
                    {
                        //Icon set to TOP LEFT, as it is to the Left of the player
                        if (!minimapOFFScreenObjectDictionary[minimapWorldObject].gameObject.activeSelf) minimapOFFScreenObjectDictionary[minimapWorldObject].gameObject.SetActive(true);
                        tempX = currentPlayerIconPosition.x - OutOfBoundsDimension.x; 
                        tempY = currentPlayerIconPosition.y + OutOfBoundsDimension.y;
                    }
                    else if (mapPosition.x - currentPlayerIconPosition.x > OutOfBoundsDimension.x && mapPosition.y - currentPlayerIconPosition.y > OutOfBoundsDimension.y)
                    {
                        //Icon set to TOP RIGHT
                        if (!minimapOFFScreenObjectDictionary[minimapWorldObject].gameObject.activeSelf) minimapOFFScreenObjectDictionary[minimapWorldObject].gameObject.SetActive(true);
                        tempX = currentPlayerIconPosition.x + OutOfBoundsDimension.x;
                        tempY = currentPlayerIconPosition.y + OutOfBoundsDimension.y;
                    }
                    else if (mapPosition.x - currentPlayerIconPosition.x > OutOfBoundsDimension.x && currentPlayerIconPosition.y - mapPosition.y > OutOfBoundsDimension.y)
                    {
                        //Icon set to BOTTOM RIGHT (bottom as it is below player)
                        if (!minimapOFFScreenObjectDictionary[minimapWorldObject].gameObject.activeSelf) minimapOFFScreenObjectDictionary[minimapWorldObject].gameObject.SetActive(true);
                        tempX = currentPlayerIconPosition.x + OutOfBoundsDimension.x;
                        tempY = currentPlayerIconPosition.y - OutOfBoundsDimension.y;
                    }
                    else if (currentPlayerIconPosition.x - mapPosition.x > OutOfBoundsDimension.x && currentPlayerIconPosition.y - mapPosition.y > OutOfBoundsDimension.y)
                    {
                        //Icon set to BOTTOM LEFT
                        if (!minimapOFFScreenObjectDictionary[minimapWorldObject].gameObject.activeSelf) minimapOFFScreenObjectDictionary[minimapWorldObject].gameObject.SetActive(true);
                        tempX = currentPlayerIconPosition.x - OutOfBoundsDimension.x;
                        tempY = currentPlayerIconPosition.y - OutOfBoundsDimension.y;
                    }
                    else if (currentPlayerIconPosition.x - mapPosition.x > OutOfBoundsDimension.x && Mathf.Abs(currentPlayerIconPosition.y - mapPosition.y) < OutOfBoundsDimension.y)
                    {
                        //Icon set to LEFT
                        if (!minimapOFFScreenObjectDictionary[minimapWorldObject].gameObject.activeSelf) minimapOFFScreenObjectDictionary[minimapWorldObject].gameObject.SetActive(true);
                        tempX = currentPlayerIconPosition.x - OutOfBoundsDimension.x;
                        tempY = mapPosition.y;
                    }
                    else if (mapPosition.x - currentPlayerIconPosition.x > OutOfBoundsDimension.x && Mathf.Abs(currentPlayerIconPosition.y - mapPosition.y) < OutOfBoundsDimension.y)
                    {
                        //Icon set to RIGHT
                        if (!minimapOFFScreenObjectDictionary[minimapWorldObject].gameObject.activeSelf) minimapOFFScreenObjectDictionary[minimapWorldObject].gameObject.SetActive(true);
                        tempX = currentPlayerIconPosition.x + OutOfBoundsDimension.x;
                        tempY = mapPosition.y;
                    }
                    else if (mapPosition.y - currentPlayerIconPosition.y > OutOfBoundsDimension.y && Mathf.Abs(currentPlayerIconPosition.x - mapPosition.x) < OutOfBoundsDimension.x)
                    {
                        //Icon set to TOP
                        if (!minimapOFFScreenObjectDictionary[minimapWorldObject].gameObject.activeSelf) minimapOFFScreenObjectDictionary[minimapWorldObject].gameObject.SetActive(true);
                        tempX = mapPosition.x;
                        tempY = currentPlayerIconPosition.y + OutOfBoundsDimension.y;
                    }
                    else if (currentPlayerIconPosition.y - mapPosition.y > OutOfBoundsDimension.y && Mathf.Abs(currentPlayerIconPosition.x - mapPosition.x) < OutOfBoundsDimension.x)
                    {
                        //Icon set to BOTTOM
                        if (!minimapOFFScreenObjectDictionary[minimapWorldObject].gameObject.activeSelf) minimapOFFScreenObjectDictionary[minimapWorldObject].gameObject.SetActive(true);
                        tempX = mapPosition.x;
                        tempY = currentPlayerIconPosition.y - OutOfBoundsDimension.y;
                    }
                    else
                    {
                        if (minimapOFFScreenObjectDictionary[minimapWorldObject].gameObject.activeSelf) minimapOFFScreenObjectDictionary[minimapWorldObject].gameObject.SetActive(false);
                    }

                    tempPos = new Vector2(tempX, tempY);
                    if (minimapOFFScreenObjectDictionary[minimapWorldObject].gameObject.activeSelf)
                        minimapOFFScreenObjectDictionary[minimapWorldObject].PrefabRectTransform.anchoredPosition = Vector2.Lerp(minimapOFFScreenObjectDictionary[minimapWorldObject].PrefabRectTransform.anchoredPosition, tempPos, 0.5f);
                    else
                        minimapOFFScreenObjectDictionary[minimapWorldObject].PrefabRectTransform.anchoredPosition = tempPos;
                }

                var rotation = minimapWorldObject.GetComponent<PlayerContainer>().HeadingTransform.rotation.eulerAngles;
                //Rotating the play occurs along Y axis (Negative because of Unity's Left Hand Coordinate System. Rotating the minimap Icon occurs along the Z axis.

                minimapIcon.IconRectTransform.localRotation = Quaternion.AngleAxis(-rotation.y, Vector3.forward);
            }
        }

    }

    private Vector2 WorldPositionToMapPosition(Vector3 worldPos)
    {
        var pos = new Vector2(worldPos.x, worldPos.z);
        return transformationMaxtrix.MultiplyPoint3x4(pos);
    }

    public bool matrixCalculated;
    //Call this if your Mini Map dimensions or Real Map Dimension changes.
    private void CalculateTransformationMatrix()
    {
        var minimapDimensions = contentRectTransform.rect.size;

        var terrainDimensions = realMapDimensions;

        var scaleRatio = minimapDimensions / terrainDimensions;

        //IF Map is not at origin, we need to do a translation that requires the transformation matrix.
        //If not, multiplying by scale is all that's needed.
        //But for purpose of learning, below is the example if the Map's Center is not at origin.

        //If square minimap is 200 px wide, and at bottom left (-100 units, south), your're basically out of the map. So 200/-100
        var translation = -minimapDimensions / 2 + new Vector2(minimapDimensions.x / 2 - 30 + 5, minimapDimensions.y / 2 + 5);

        transformationMaxtrix = Matrix4x4.TRS(translation, Quaternion.identity, scaleRatio);

        /*
        * [    scaleRatio.x    ,  0            ,  0    , translation.x ]
        * [    0               ,  scaleRatio.y ,  0    , translation.x ]
        * [    0               ,  0            ,  0    , 0             ]
        * [    0               ,  0            ,  0    , 0             ]
        */
        matrixCalculated = true;
    }

    //Finds out how far the local player icon is from the Center of the 'Content's rect transform. Use this distance to move the entire 'Content's accordingly.
    private Vector2 CalculateLocalPlayerCenterDistance(Vector3 minimapIconPos)
    {
        var distX = -minimapIconPos.x;
        var distY = -minimapIconPos.y;

        return new Vector2(distX, distY);
    }

    //Finds out how far each other Player icon is from the Local Player icon rectTransform. Use this distance to shift the other players' icons accordingly.
    private Vector2 CalculateDistanceToLocalPlayer(Vector2 otherPlayerPos)
    {
        var distX = LocalPlayerIconPosltion.x - mapCenterDistance.x;
        var distY = LocalPlayerIconPosltion.y - mapCenterDistance.y;
        Debug.DrawRay(otherPlayerPos, otherPlayerPos - LocalPlayerIconPosltion, Color.red, 100f);

        return new Vector2(distX, distY);
    }

}
