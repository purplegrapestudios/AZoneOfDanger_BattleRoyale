using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class Minimap : SimulationBehaviour
{
    public static Minimap Instance;

    [SerializeField]
    Vector2 realMapDimensions;

    //[SerializeField]
    //RectTransform scrollViewRectTransform;

    [SerializeField]
    RectTransform contentRectTransform;

    [SerializeField]
    MinimapIcon minimapIconPrefab;

    [SerializeField]
    Matrix4x4 transformationMaxtrix;

    [SerializeField]
    Vector2 currentPlayerIconPosition;
    public Dictionary<MinimapWorldObject, MinimapIcon> minimapWorldObjectDictrionary = new Dictionary<MinimapWorldObject, MinimapIcon>();

    public Dictionary<MinimapWorldObject, MinimapIcon> offScreenMinimapObjectDictionary = new Dictionary<MinimapWorldObject, MinimapIcon>();

    private List<MinimapIcon> m_minimapIconsToDestroy = new List<MinimapIcon>();
    private List<MinimapIcon> m_offScreenMinimapIconsToDestroy = new List<MinimapIcon>();
    private List<MinimapWorldObject> m_worldObjToRemove = new List<MinimapWorldObject>();
    private List<MinimapWorldObject> m_offScreenWorldObjToRemove = new List<MinimapWorldObject>();

    [SerializeField]
    public Vector2 OutOfBoundsDimension;

    [SerializeField] private App m_app;
    [SerializeField] private bool m_initialized = false;
    private void Awake()
    {
        Instance = this;
        m_app = App.FindInstance();
    }

    public void Init()
    {
        CalculateTransformationMatrix();
        m_initialized = true;
    }

    private void OnEnable()
    {
        if (m_initialized) CalculateTransformationMatrix();
    }

    private void Update()
    {
        if (!m_app)
        {
            m_app = App.FindInstance();
            return;
        }
        if (m_app.AllowInput)
        {
            if (!m_initialized)
            {
                Init();
            }
        }
        else
        {
            return;
        }

        if (!m_initialized) return;
        UpdateMinimapIcons();
    }

    //For each game instance, All Players will call this. 
    public void RegisterMinimapWorldObject(MinimapWorldObject minimapWorldObject, Color offScreenColor)
    {
        if (!minimapWorldObjectDictrionary.ContainsKey(minimapWorldObject))
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
        
            minimapIcon.SetIcon(minimapWorldObject.Icon);
            //if (minimapWorldObject.m_character.Object.InputAuthority != Object.Runner.LocalPlayer)
            if (minimapWorldObject.isLocalMinimapPlayer)
            {
                //Minimap Local Player Set
                Debug.Log($"Minimap Local Player Set");
                minimapIcon.SetColor(new Color(c.r, c.g, c.b, c.a));
            }
            else
            {
                minimapIcon.SetColor(new Color(184f / 255f, 255 / 255f, 100 / 255f, 1f));
                minimapIcon.IconText.color = minimapIcon.IconImage.color;
            }
            minimapIcon.SetText(minimapWorldObject.Text);
            minimapIcon.SetTextSize(minimapWorldObject.TextSize);
            minimapIcon.m_minimapRT.localScale = Vector3.one;
            minimapIcon.SetIsLocalMinimapPlayer(minimapWorldObject.isLocalMinimapPlayer);

            minimapWorldObjectDictrionary.Add(minimapWorldObject, minimapIcon);
        }
        else
        {
            Debug.Log("Icon: " + minimapWorldObject.PlayerID + " is already in the dictioary");
        }
        if(minimapWorldObject.m_character.Object.InputAuthority == Object.Runner.LocalPlayer)
        {
            LocalPlayerIconPosltion = -WorldPositionToMapPosition(minimapWorldObject.transform.position);
        }
        else
        {
            tempMinimapIcon = Instantiate(tempMinimapIconPrefab);
            tempMinimapIcon.transform.SetParent(contentRectTransform);
            tempMinimapIcon.SetIcon(minimapSpriteOffScreen);
            Random.InitState((int)System.DateTime.Now.Ticks * 256);
            Color c = Random.ColorHSV();
            c = new Color(
                Mathf.Min(255f / 255f, c.r) + 150f / 255f,
                Mathf.Min(100f / 255f, c.g) + 50f / 255f,
                Mathf.Min(100f / 255f, c.b) + 50f / 255f,
                1f);
            tempMinimapIcon.SetColor(new Color(c.r, c.g, c.b, c.a));

            //tempMinimapIcon.SetText("P" + minimapWorldObject.pv.viewID + ": " + StatManager.Instance.GetPlayerStats(minimapWorldObject.pv.viewID, StatType.Rank));
            tempMinimapIcon.SetText($"PLR# {minimapWorldObject.PlayerID}");
            tempMinimapIcon.SetTextSize(minimapWorldObject.TextSize);
            tempMinimapIcon.m_minimapRT.localScale = Vector3.one;
            
            offScreenMinimapObjectDictionary.Add(minimapWorldObject, tempMinimapIcon);
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
        if (!m_app.AllowInput) return;

        RemoveOldWorldObjectAndIcons(m_minimapIconsToDestroy, m_offScreenMinimapIconsToDestroy, m_worldObjToRemove, m_offScreenWorldObjToRemove);

        foreach (var keyValuePair in minimapWorldObjectDictrionary)
        {
            var minimapWorldObject = keyValuePair.Key;
            var minimapIcon = keyValuePair.Value;

            var mapPosition = Vector2.zero;
            if (minimapWorldObject)
            {
                if (minimapWorldObject.m_character?.Object?.InputAuthority.IsNone ?? true)
                {
                    //This player is no longer in the game. Let's remove the kvp and destroy the player's object
                    m_minimapIconsToDestroy.Add(minimapIcon);
                    m_worldObjToRemove.Add(minimapWorldObject);

                    m_offScreenMinimapIconsToDestroy.Add(offScreenMinimapObjectDictionary[minimapWorldObject]);
                    m_offScreenWorldObjToRemove.Add(minimapWorldObject);
                    continue;
                }

                mapPosition = -WorldPositionToMapPosition(minimapWorldObject.transform.position);

                if (minimapWorldObject.m_character.Object.InputAuthority == Object.Runner.LocalPlayer)
                {
                    currentPlayerIconPosition = mapPosition;
                    mapCenterDistance = CalculateLocalPlayerCenterDistance(mapPosition);
                    contentRectTransform.anchoredPosition = mapCenterDistance;
                }

                minimapIcon.m_minimapRT.anchoredPosition = mapPosition;

                if (minimapWorldObject.m_character.Object.InputAuthority != Object.Runner.LocalPlayer)
                {
                    //Not Local Player, so if otherPlayers distX > dx, and/or distY > dy shift it to the outside border on edge of minimap to be visible still.
                    //var DistanceToPlayer = CalculateDistanceToLocalPlayer(mapPosition);
                    if (currentPlayerIconPosition.x - mapPosition.x > OutOfBoundsDimension.x && mapPosition.y - currentPlayerIconPosition.y > OutOfBoundsDimension.y)
                    {
                        //Icon set to TOP LEFT, as it is to the Left of the player
                        if (!offScreenMinimapObjectDictionary[minimapWorldObject].gameObject.activeSelf) offScreenMinimapObjectDictionary[minimapWorldObject].gameObject.SetActive(true);
                        tempX = currentPlayerIconPosition.x - OutOfBoundsDimension.x; 
                        tempY = currentPlayerIconPosition.y + OutOfBoundsDimension.y;
                    }
                    else if (mapPosition.x - currentPlayerIconPosition.x > OutOfBoundsDimension.x && mapPosition.y - currentPlayerIconPosition.y > OutOfBoundsDimension.y)
                    {
                        //Icon set to TOP RIGHT
                        if (!offScreenMinimapObjectDictionary[minimapWorldObject].gameObject.activeSelf) offScreenMinimapObjectDictionary[minimapWorldObject].gameObject.SetActive(true);
                        tempX = currentPlayerIconPosition.x + OutOfBoundsDimension.x;
                        tempY = currentPlayerIconPosition.y + OutOfBoundsDimension.y;
                    }
                    else if (mapPosition.x - currentPlayerIconPosition.x > OutOfBoundsDimension.x && currentPlayerIconPosition.y - mapPosition.y > OutOfBoundsDimension.y)
                    {
                        //Icon set to BOTTOM RIGHT (bottom as it is below player)
                        if (!offScreenMinimapObjectDictionary[minimapWorldObject].gameObject.activeSelf) offScreenMinimapObjectDictionary[minimapWorldObject].gameObject.SetActive(true);
                        tempX = currentPlayerIconPosition.x + OutOfBoundsDimension.x;
                        tempY = currentPlayerIconPosition.y - OutOfBoundsDimension.y;
                    }
                    else if (currentPlayerIconPosition.x - mapPosition.x > OutOfBoundsDimension.x && currentPlayerIconPosition.y - mapPosition.y > OutOfBoundsDimension.y)
                    {
                        //Icon set to BOTTOM LEFT
                        if (!offScreenMinimapObjectDictionary[minimapWorldObject].gameObject.activeSelf) offScreenMinimapObjectDictionary[minimapWorldObject].gameObject.SetActive(true);
                        tempX = currentPlayerIconPosition.x - OutOfBoundsDimension.x;
                        tempY = currentPlayerIconPosition.y - OutOfBoundsDimension.y;
                    }
                    else if (currentPlayerIconPosition.x - mapPosition.x > OutOfBoundsDimension.x && Mathf.Abs(currentPlayerIconPosition.y - mapPosition.y) < OutOfBoundsDimension.y)
                    {
                        //Icon set to LEFT
                        if (!offScreenMinimapObjectDictionary[minimapWorldObject].gameObject.activeSelf) offScreenMinimapObjectDictionary[minimapWorldObject].gameObject.SetActive(true);
                        tempX = currentPlayerIconPosition.x - OutOfBoundsDimension.x;
                        tempY = mapPosition.y;
                    }
                    else if (mapPosition.x - currentPlayerIconPosition.x > OutOfBoundsDimension.x && Mathf.Abs(currentPlayerIconPosition.y - mapPosition.y) < OutOfBoundsDimension.y)
                    {
                        //Icon set to RIGHT
                        if (!offScreenMinimapObjectDictionary[minimapWorldObject].gameObject.activeSelf) offScreenMinimapObjectDictionary[minimapWorldObject].gameObject.SetActive(true);
                        tempX = currentPlayerIconPosition.x + OutOfBoundsDimension.x;
                        tempY = mapPosition.y;
                    }
                    else if (mapPosition.y - currentPlayerIconPosition.y > OutOfBoundsDimension.y && Mathf.Abs(currentPlayerIconPosition.x - mapPosition.x) < OutOfBoundsDimension.x)
                    {
                        //Icon set to TOP
                        if (!offScreenMinimapObjectDictionary[minimapWorldObject].gameObject.activeSelf) offScreenMinimapObjectDictionary[minimapWorldObject].gameObject.SetActive(true);
                        tempX = mapPosition.x;
                        tempY = currentPlayerIconPosition.y + OutOfBoundsDimension.y;
                    }
                    else if (currentPlayerIconPosition.y - mapPosition.y > OutOfBoundsDimension.y && Mathf.Abs(currentPlayerIconPosition.x - mapPosition.x) < OutOfBoundsDimension.x)
                    {
                        //Icon set to BOTTOM
                        if (!offScreenMinimapObjectDictionary[minimapWorldObject].gameObject.activeSelf) offScreenMinimapObjectDictionary[minimapWorldObject].gameObject.SetActive(true);
                        tempX = mapPosition.x;
                        tempY = currentPlayerIconPosition.y - OutOfBoundsDimension.y;
                    }
                    else
                    {
                        if (offScreenMinimapObjectDictionary[minimapWorldObject].gameObject.activeSelf) offScreenMinimapObjectDictionary[minimapWorldObject].gameObject.SetActive(false);
                    }

                    tempPos = new Vector2(tempX, tempY);
                    if (offScreenMinimapObjectDictionary[minimapWorldObject].gameObject.activeSelf)
                        offScreenMinimapObjectDictionary[minimapWorldObject].m_minimapRT.anchoredPosition = Vector2.Lerp(offScreenMinimapObjectDictionary[minimapWorldObject].m_minimapRT.anchoredPosition, tempPos, 0.5f);
                    else
                        offScreenMinimapObjectDictionary[minimapWorldObject].m_minimapRT.anchoredPosition = tempPos;
                }

                var rotation = minimapWorldObject.m_character.GetComponent<NetworkRigidbody>().ReadRotation().eulerAngles;
                //Rotating the play occurs along Y axis (Negative because of Unity's Left Hand Coordinate System. Rotating the minimap Icon occurs along the Z axis.

                minimapIcon.m_iconRT.localRotation = Quaternion.AngleAxis(-rotation.y, Vector3.forward);
            }
        }

    }

    private Vector2 WorldPositionToMapPosition(Vector3 worldPos)
    {
        var pos = new Vector2(worldPos.x, worldPos.z);
        return transformationMaxtrix.MultiplyPoint3x4(pos);
    }

    private void RemoveOldWorldObjectAndIcons(List<MinimapIcon> minimapIconsToDestroy, List<MinimapIcon> offScreenMinimapIconsToDestroy, List<MinimapWorldObject> worldObjsToRemove, List<MinimapWorldObject> offScreenWorldObjsToRemove)
    {
        if (minimapIconsToDestroy.Count > 0)
        {
            foreach (var icon in minimapIconsToDestroy) if (icon) Destroy(icon.gameObject);
        }
        if (offScreenMinimapIconsToDestroy.Count > 0)
        {
            foreach (var icon in offScreenMinimapIconsToDestroy) if (icon) Destroy(icon.gameObject);
        }
        if (worldObjsToRemove.Count > 0)
        {
            foreach (var worldObj in worldObjsToRemove) if (worldObj) minimapWorldObjectDictrionary.Remove(worldObj);
        }
        if (offScreenWorldObjsToRemove.Count > 0)
        {
            foreach (var worldObj in offScreenWorldObjsToRemove) if (worldObj) offScreenMinimapObjectDictionary.Remove(worldObj);
        }
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
