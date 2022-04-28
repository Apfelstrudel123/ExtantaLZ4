using UnityEngine;
using UnityEngine.InputSystem;
public class Map : MonoBehaviour
{
    public static Map instance;

    [SerializeField] private Transform mapTransform;
    [SerializeField] private Transform icons;
    public Transform playerIcon = null;

    [SerializeField] private int size = 515;
    [SerializeField] private float minZoom = 1;
    [SerializeField] private float maxZoom = 5;
    [SerializeField] private float panSpeed = 250f;

    private bool mouseOnPanel = false;
    [SerializeField] private RectTransform waypointImg;
    [SerializeField] private GameObject waypoint3D;


    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        float zoom = Mouse.current.scroll.y.ReadValue();
        if (zoom > 0 && mapTransform.localScale.x < maxZoom)
        {
            mapTransform.localScale += new Vector3(1, 1, 0);
            mapTransform.localPosition *= mapTransform.localScale.x/(mapTransform.localScale.x-1);
            
            Vector3 v = new Vector3(1 / mapTransform.localScale.x, 1 / mapTransform.localScale.y, 1);
            for (int i = 0; i < icons.childCount; i++)
            {
                icons.GetChild(i).localScale = v;
            }
        }
        else if (zoom < 0 && mapTransform.localScale.x > minZoom)
        {
            mapTransform.localScale += new Vector3(-1, -1, 0);
            if (mapTransform.localScale.x > 1)
            {
                mapTransform.localPosition *= mapTransform.localScale.x / (mapTransform.localScale.x + 1);
            }
            else
            {
                mapTransform.localPosition *= 0;
            }
            Vector3 v = new Vector3(1 / mapTransform.localScale.x, 1 / mapTransform.localScale.y, 1);
            for (int i = 0; i < icons.childCount; i++)
            {
                icons.GetChild(i).localScale = v;
            }
        }

        bool w = Keyboard.current.wKey.isPressed;
        bool s = Keyboard.current.sKey.isPressed;
        bool a = Keyboard.current.aKey.isPressed;
        bool d = Keyboard.current.dKey.isPressed;

        float ver = 0f;
        float hor = 0f;

        if (w && !s)
        {
            ver = -panSpeed * Time.unscaledDeltaTime;
        }
        else if (!w && s)
        {
            ver = panSpeed * Time.unscaledDeltaTime;
        }

        if (d && !a)
        {
            hor = -panSpeed * Time.unscaledDeltaTime;
        }
        else if (!d && a)
        {
            hor = panSpeed * Time.unscaledDeltaTime;
        }

        if (ver != 0 || hor != 0 || zoom != 0)
        {
            mapTransform.Translate(hor, ver, 0f);

            float bound = (mapTransform.localScale.x - 1) * size;

            float x = mapTransform.localPosition.x;
            float y = mapTransform.localPosition.y;

            x = Mathf.Clamp(x, -bound, bound);
            y = Mathf.Clamp(y, -bound, bound);

            mapTransform.localPosition = new Vector3(x, y, 0f);
        }

        if(Mouse.current.rightButton.wasPressedThisFrame)
        { PlaceWaypoint(); }
    }

    private void OnEnable()
    {
        mapTransform.localScale = new Vector3(1, 1, 1);
        mapTransform.localPosition = Vector2.zero;
        playerIcon.transform.position = WorldToMap(Gameplay.Player.Transform.position);
    }

    private void PlaceWaypoint()
    {
        if (mouseOnPanel)
        {
            Vector2 mouse = Mouse.current.position.ReadValue();

            waypointImg.position = new Vector3(mouse.x, mouse.y, 0);
            waypointImg.gameObject.SetActive(true);

            //Vector3 worldPos = new Vector3((mouse.x - 965f + mapTransform.localPosition.x / size) * 4f, 200f, (mouse.y - 565f + mapTransform.localPosition.y / size) * 4f);

            waypoint3D.transform.position = MapToWorld(mouse);
            waypoint3D.SetActive(true);
        }
    }

    public void DeactivateWaypoint()
    {
        waypointImg.gameObject.SetActive(false);
    }

    public void OnMouseOnPanel()
    {
        mouseOnPanel = !mouseOnPanel;
    }

    private Vector2 WorldToMap(Vector3 pos)
    {
        return new Vector2((pos.x + 2048) / 4f + 450f, (-pos.y + 2048) / 4f + 50f);
    }

    private Vector3 MapToWorld(Vector2 pos)
    {
        return new Vector3((pos.x - 450f) * 4f - 2048f, 200f, (pos.y - 50f) * 4f - 2048f);
    }
}