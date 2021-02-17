using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Util;

public class EditorMenuController : MonoBehaviour
{
    /* STATIC */

    public static EditorMenuController Instance { private set; get; }



    /* INSTANCE */

    [SerializeField] private Button buttonUndo, buttonRedo;
    [SerializeField] private Button buttonSizeUp, buttonSizeDown;
    [SerializeField] private Button buttonWeightUp, buttonWeightDown;
    [SerializeField] private Button buttonReset;

    [SerializeField] private RectTransform textPanelSize, textPanelWeight;
    
    [SerializeField] private Scrollbar scrollbarSize, scrollbarWeight;

    [SerializeField] private RectTransform terrainBrushButtonsContainer, objectButtonsContainer;

    [SerializeField] private Object buttonTerrainBrushPrefab, buttonObjectPrefab;

    private TooltipController tooltip;
    private ButtonController undo, redo, sizeUp, sizeDown, weightUp, weightDown, reset;
    private ScrollbarController sizeScroll, weightScroll;
    private ButtonController[] brushButtons, objectButtons;

    private TextPanelController size, weight;



    private void Awake ()
    {
        // Check for duplicate instances
        if (Instance != null)
            Destroy (gameObject);
        else
            Instance = this;

        // Tooltip
        RectTransform tooltipTransform = transform.Find ("Tooltip") as RectTransform;
        tooltip = new TooltipController (tooltipTransform, tooltipTransform.Find ("Text").GetComponent<Text> ());
        tooltip.Hide ();

        // Create a button for each terrain brush
        GetTerrainBrushButtons ();

        // Create a button for each object
        GetObjectButtons ();

        // Text panels
        Text sizeText = textPanelSize.Find ("Text").GetComponent<Text> ();
        Text weightText = textPanelWeight.Find ("Text").GetComponent<Text> ();
        size = new TextPanelController (textPanelSize.transform, sizeText, () => { sizeText.text = ((int)TerrainController.CursorSize).ToString (); });
        weight = new TextPanelController (textPanelWeight.transform, weightText, () => { weightText.text = ((int)TerrainController.CursorWeight).ToString (); });

        // Static buttons
        undo = new ButtonController (buttonUndo.transform, buttonUndo, "Undo last move", () => { TerrainController.Undo (); });
        redo = new ButtonController (buttonRedo.transform, buttonRedo, "Redo last undo'd move", () => { TerrainController.Redo (); });
        reset = new ButtonController (buttonReset.transform, buttonReset, "Reset terrain to flat plane", () => { TerrainController.GenerateDefault (); TerrainController.UpdateMesh (); });
        sizeUp = new ButtonController (buttonSizeUp.transform, buttonSizeUp, "Increase the brush's range", () => { TerrainController.ChangeSize (1); size.OnValue (); sizeScroll.OnValue (); });
        sizeDown = new ButtonController (buttonSizeDown.transform, buttonSizeDown, "Decrease the brush's range", () => { TerrainController.ChangeSize (-1); size.OnValue (); sizeScroll.OnValue (); });
        weightUp = new ButtonController (buttonWeightUp.transform, buttonWeightUp, "Increase the brush's strength", () => { TerrainController.ChangeWeight (1); weight.OnValue (); weightScroll.OnValue (); });
        weightDown = new ButtonController (buttonWeightDown.transform, buttonWeightDown, "Decrease the brush's strength", () => { TerrainController.ChangeWeight (-1); weight.OnValue (); weightScroll.OnValue (); });

        // Scrollbars
        sizeScroll = new ScrollbarController (
            scrollbarSize.transform, scrollbarSize, "Change the brush's range",
            (value) => { TerrainController.SetSize (sizeScroll.Remap (value)); size.OnValue (); },
            () => { sizeScroll.scrollbar.value = sizeScroll.InverseRemap (TerrainController.CursorSize); },
            new Vector4 (0, 1, 1, 50)
        );
        weightScroll = new ScrollbarController (
            scrollbarWeight.transform, scrollbarWeight, "Change the brush's strength",
            (value) => { TerrainController.SetWeight (weightScroll.Remap (value)); weight.OnValue (); },
            () => { weightScroll.scrollbar.value = weightScroll.InverseRemap (TerrainController.CursorWeight); },
            new Vector4 (0, 1, 1, 50)
        );

        // Set the initial value of the text panels and scrollbars
        size.OnValue ();
        sizeScroll.OnValue ();
        weight.OnValue ();
        weightScroll.OnValue ();
    }

    private void GetTerrainBrushButtons ()
    {
        // Load all unique descendants of TerrainBrush and load them in as usable terrain brushes
        TerrainBrush[] terrainBrushes = System.Reflection.Assembly.GetAssembly (typeof (TerrainBrush))
            .GetTypes ()
            .Where (t => t.IsSubclassOf (typeof (TerrainBrush)))
            .Select (t => (TerrainBrush)System.Activator.CreateInstance (t))
            .OrderBy (brush => brush.Order)
            .ToArray ();

        // Create a button for each
        brushButtons = new ButtonController[terrainBrushes.Length];
        terrainBrushButtonsContainer.sizeDelta = new Vector2 (2 * 30 + brushButtons.Length * 60, terrainBrushButtonsContainer.sizeDelta.y);
        //(terrainBrushesContainer.parent.parent as RectTransform).sizeDelta = new Vector2 (Mathf.Clamp (terrainBrushesContainer.sizeDelta.x, 0, 500), (terrainBrushesContainer.parent.parent as RectTransform).sizeDelta.y);
        for (int i = 0; i < brushButtons.Length; i++)
        {
            // Get the brush
            TerrainBrush brush = terrainBrushes[i];

            // Create the button
            Transform button = (Instantiate (buttonTerrainBrushPrefab, Vector3.zero, Quaternion.identity, terrainBrushButtonsContainer) as GameObject).transform;
            button.localPosition = new Vector3 (30 + i * 60, -25, 0);
            button.Find ("Text").GetComponent<Text> ().text = brush.Name;

            // Create the button controller and define its functionality
            brushButtons[i] = new ButtonController (button, button.GetComponent<Button> (), brush.Tooltip, () => { TerrainController.TerrainBrush = brush; });
        }
    }

    private void GetObjectButtons ()
    {
        //// Load all objects from the resouces directory
        //ObjectContainer[] objects = Resources.LoadAll ("objects")
        //    .Select (o => new ObjectContainer (o.name, o))
        //    .ToArray ();

        //// Create a button for each
        //objectButtons = new ButtonController[objects.Length];
        //objectButtonsContainer.sizeDelta = new Vector2 (2 * 30 + objectButtons.Length * 60, objectButtonsContainer.sizeDelta.y);
        //for (int i = 0; i < objectButtons.Length; i++)
        //{
        //    // Get the brush
        //    ObjectContainer oc = objects[i];

        //    // Create the button
        //    Transform button = Instantiate (buttonObjectPrefab, Vector3.zero, Quaternion.identity, objectButtonsContainer) as Transform;
        //    button.localPosition = new Vector3 (30 + i * 60, -25, 0);
        //    button.Find ("Text").GetComponent<Text> ().text = oc.Name;

        //    // Create the button controller and define its functionality
        //    objectButtons[i] = new ButtonController (button, button.GetComponent<Button> (), brush.Tooltip, () => { TerrainController.TerrainBrush = brush; });
        //}
    }



    /* UI CONTROLLERS */

    private class TooltipController
    {
        public RectTransform transform { private set; get; }
        public Text text { private set; get; }

        public TooltipController (RectTransform transform, Text text)
        {
            this.transform = transform;
            this.text = text;
        }

        public void Hide ()
        {
            transform.gameObject.SetActive (false);
        }

        public void Show (Vector2 position, string text)
        {
            transform.gameObject.SetActive (true);

            float x = position.x < Screen.width / 2 ? 0 : 1;
            float y = position.y < Screen.height / 2 ? 0 : 1;

            transform.pivot = new Vector2 (x, 0.5f);
            transform.position = position;
            this.text.text = text;
        }
    }



    private class UIController
    {
        public Transform transform { protected set; get; }
        public string tooltip { protected set; get; }
    }       
    private class ClickableController : UIController
    {
        public System.Action OnClick { protected set; get; }
        public System.Action OnDown { protected set; get; }
        public System.Action OnUp { protected set; get; }
        public float timeLastPressed;
        public System.Func<IEnumerator> WhilePressed;
        public IEnumerator whilePressedCoroutine;
    }


    private class ButtonController : ClickableController
    {
        
        public Button button { private set; get; }

        public ButtonController (Transform transform, Button button, string tooltip = null, System.Action onClick = null, System.Action onDown = null, System.Action onUp = null, System.Func<IEnumerator> whilePressed = null)
        {
            this.transform = transform;
            this.button = button;
            OnClick = onClick;

            if (tooltip == null)
            {
                tooltip = transform.Find ("Text").GetComponent<Text> ().text;
            }
            this.tooltip = tooltip;

            EventTrigger trigger = button.gameObject.AddComponent<EventTrigger> ();
            EventTrigger.Entry entry;

            // Mouse enter
            entry = new EventTrigger.Entry ();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener ((data) => { OnPointerEnterDelegate ((PointerEventData) data, this); });
            trigger.triggers.Add (entry);

            // Mouse exit
            entry = new EventTrigger.Entry ();
            entry.eventID = EventTriggerType.PointerExit;
            entry.callback.AddListener ((data) => { OnPointerExitDelegate ((PointerEventData) data, this); });
            trigger.triggers.Add (entry);

            // On click
            entry = new EventTrigger.Entry ();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener ((data) => { OnPointerClickDelegate ((PointerEventData) data, this); });
            trigger.triggers.Add (entry);

            //// On down
            //entry = new EventTrigger.Entry ();
            //entry.eventID = EventTriggerType.PointerDown;
            //entry.callback.AddListener ((data) => { OnPointerDownDelegate ((PointerEventData) data, this); });
            //trigger.triggers.Add (entry);

            //// On up
            //entry = new EventTrigger.Entry ();
            //entry.eventID = EventTriggerType.PointerUp;
            //entry.callback.AddListener ((data) => { OnPointerUpDelegate ((PointerEventData) data, this); });
            //trigger.triggers.Add (entry);

            // While pressed
            WhilePressed = whilePressed;
        }
    }



    private class TextPanelController : UIController
    {
        public Text text { private set; get; }

        public System.Action OnValue { private set; get; }

        public TextPanelController (Transform transform, Text text, System.Action onValue = null)
        {
            this.transform = transform;
            this.text = text;
            OnValue = onValue;
        }
    }



    private class ScrollbarController : UIController
    {
        public Scrollbar scrollbar { protected set; get; }
        public System.Action<float> OnValueChanged { protected set; get; }
        public System.Action OnValue { protected set; get; }
        private Vector4 remapBounds;

        public ScrollbarController (Transform transform, Scrollbar scrollbar, string tooltip = null, System.Action<float> onValueChanged = null, System.Action onValue = null, Vector4 remapBounds = default)
        {
            this.transform = transform;
            this.scrollbar = scrollbar;
            OnValueChanged = onValueChanged;
            OnValue = onValue;
            this.remapBounds = remapBounds;

            if (tooltip == null)
            {
                tooltip = transform.Find ("Text").GetComponent<Text> ().text;
            }
            this.tooltip = tooltip;

            EventTrigger trigger = scrollbar.gameObject.AddComponent<EventTrigger> ();
            EventTrigger.Entry entry;

            // Mouse enter
            entry = new EventTrigger.Entry ();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener ((data) => { OnPointerEnterDelegate ((PointerEventData) data, this); });
            trigger.triggers.Add (entry);

            // Mouse exit
            entry = new EventTrigger.Entry ();
            entry.eventID = EventTriggerType.PointerExit;
            entry.callback.AddListener ((data) => { OnPointerExitDelegate ((PointerEventData) data, this); });
            trigger.triggers.Add (entry);

            // On value update
            scrollbar.onValueChanged.AddListener ((value) => { OnScrollDelegate (value, this); });
        }

        public float Remap (float value)
        {
            return MathfExt.Remap (value, remapBounds.x, remapBounds.y, remapBounds.z, remapBounds.w);
        }
        public float InverseRemap (float value)
        {
            return MathfExt.Remap (value, remapBounds.z, remapBounds.w, remapBounds.x, remapBounds.y);
        }
    }



    /* DELEGATES */

    // Tooltip
    private static void OnPointerEnterDelegate (PointerEventData data, UIController ui)
    {
        Instance.tooltip.Show (ui.transform.position, ui.tooltip);
    }
    private static void OnPointerExitDelegate (PointerEventData data, UIController ui)
    {
        Instance.tooltip.Hide ();
    }

    // Button click
    private static void OnPointerClickDelegate (PointerEventData data, ClickableController cc)
    {
        cc.timeLastPressed = Time.timeSinceLevelLoad;
        cc.OnClick ();
    }

    // Button up/down
    //private static void OnPointerDownDelegate (PointerEventData data, ClickableController cc)
    //{
    //    cc.timeLastPressed = Time.timeSinceLevelLoad;
    //    cc.OnDown ();

    //    if (cc.whilePressedCoroutine != null)
    //        Instance.StopCoroutine (cc.whilePressedCoroutine);

    //    //cc.whilePressedCoroutine = Instance.StartCoroutine (cc.WhilePressed ());
    //}
    //private static void OnPointerUpDelegate (PointerEventData data, ClickableController cc)
    //{
    //    cc.OnUp ();
    //}

    // Scrollbar changed
    private static void OnScrollDelegate (float value, ScrollbarController sc)
    {
        sc.OnValueChanged (value);
    }



    /* OBJECT CONTAINER */
    
    private class ObjectContainer
    {
        public string Name { private set; get; }
        private Object prefab;

        public ObjectContainer (string name, Object prefab)
        {
            Name = name;
            this.prefab = prefab;
        }

        public Transform CreateInstance (Vector3 position, Quaternion rotation, Transform parent = null)
        {
            Transform transform = Instantiate (prefab, position, rotation, parent) as Transform;
            return transform;
        }
    }
}
