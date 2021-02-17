using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    /* STATIC */

    public static CameraController Instance { private set; get; }
    public static Camera Camera { private set; get; }
    public static Plane BasePlane { private set; get; }
    public static Vector3 Cursor { private set; get; }

    private static float yaw, pitch, zoom;
    public static float Yaw
    {
        private set
        {
            yaw = value % 360;
        }
        get { return yaw; }
    }
    public static float Pitch
    {
        private set
        {
            pitch = Mathf.Clamp (value, -80, 80);
        }
        get { return pitch; }
    }
    public static float Zoom
    {
        private set
        {
            zoom = Mathf.Clamp (value, 5, 50);
        }
        get { return zoom; }
    }



    // https://answers.unity.com/questions/967170/detect-if-pointer-is-over-any-ui-element.html
    public static bool IsPointerOverUIObject ()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData (EventSystem.current);
        eventDataCurrentPosition.position = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult> ();
        EventSystem.current.RaycastAll (eventDataCurrentPosition, results);
        return results.Count > 0;
    }



    /* INSTANCE */

    private void Awake ()
    {
        // Check for duplicate instances
        if (Instance != null)
            Destroy (gameObject);

        // Set up
        Instance = this;
        Camera = transform.Find ("Camera").GetComponent<Camera> ();
        BasePlane = new Plane (Vector3.up, Vector3.zero);
    }

    private void Start ()
    {
        Yaw = transform.rotation.eulerAngles.y;
        Pitch = Camera.transform.localRotation.eulerAngles.x;

        Camera.transform.localPosition = new Vector3 (0, 2, -1) * Zoom;
    }

    private void Update ()
    {
        // Rotation
        if (Input.GetMouseButton (1))
        {
            Pitch -= Input.GetAxis ("Mouse Y") * 400 * Time.deltaTime;
            Yaw   += Input.GetAxis ("Mouse X") * 400 * Time.deltaTime;

            transform.rotation = Quaternion.Euler (0, Yaw, 0);
            Camera.transform.localRotation = Quaternion.Euler (Pitch, 0, 0);
        }



        // Zoom
        if (Input.GetMouseButton (2))
        {
            Zoom -= Input.GetAxis ("Mouse Y") * 400 * Time.deltaTime;
        }
        else
        {
            Zoom -= Input.GetAxis ("Mouse ScrollWheel") * 1000 * Time.deltaTime;
        }

        Camera.transform.localPosition = new Vector3 (0, 2, -1) * Zoom;



        // Movement
        Vector3 movement = new Vector3 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Elevation"), Input.GetAxis ("Vertical"));
        //movement = Quaternion.Euler (0, 45, 0) * movement;
        transform.Translate (movement * Time.deltaTime * 40);



        // Cursor
        Ray ray = Camera.ScreenPointToRay (Input.mousePosition);
        if (BasePlane.Raycast (ray, out float distance))
        {
            Cursor = ray.GetPoint (distance);
        }
    }



    /* DEBUG */
    private void OnGUI ()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle ();

        Rect rect = new Rect (0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = new Color (0.0f, 0.0f, 0.5f, 1.0f);
        string text = CameraController.Cursor.ToString ();
        GUI.Label (rect, text, style);
    }
}
