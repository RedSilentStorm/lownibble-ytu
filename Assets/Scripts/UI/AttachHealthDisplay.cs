using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AttachHealthDisplay : MonoBehaviour
{
    [Tooltip("Prefab of the Health Display (should contain a HealthBarUI component)")]
    public GameObject healthDisplayPrefab;
    public Vector3 offset = new Vector3(0f, 2f, 0f);
    public string instanceName = "_HealthDisplayInstance";

    private GameObject instance;
    private HealthBarUI healthUI;
    private BattleEntity entity;

    void Start()
    {
        // Determine context: are we on the entity itself or on an instantiated health-display child?
        entity = GetComponent<BattleEntity>();
        BattleEntity parentEntity = GetComponentInParent<BattleEntity>();

        // If this AttachHealthDisplay is part of a prefab instance that was instantiated as a child
        // of the entity (i.e., it has a BattleEntity in parents but not on the same GameObject),
        // treat this component as the display instance initializer and do not instantiate again.
        if (entity == null && parentEntity != null)
        {
            entity = parentEntity;
            instance = this.gameObject;
            healthUI = instance.GetComponentInChildren<HealthBarUI>();

            // Ensure canvases are WorldSpace
            var canvases = instance.GetComponentsInChildren<Canvas>(true);
            foreach (var c in canvases)
            {
                c.renderMode = RenderMode.WorldSpace;
                if (c.worldCamera == null) c.worldCamera = GetCamera();
                c.transform.localScale = new Vector3(0.003f, 0.005f, 0.005f);
            }

            // Always ensure UI graphics are under a Canvas
            EnsureCanvasForUI(instance);

            if (healthUI == null)
            {
                // try to build minimal UI under this instance
                BuildRuntimeUI(instance, out healthUI);
            }

            if (healthUI != null && entity != null && entity.data != null)
                healthUI.Setup(Mathf.CeilToInt(entity.data.maxHealth));

            if (entity != null)
                entity.OnHealthChanged += OnEntityHealthChanged;

            return;
        }

        // Otherwise this component is attached to the entity itself and should instantiate the prefab (or build runtime UI)
        entity = entity ?? parentEntity;
        if (healthDisplayPrefab == null)
        {
            // No prefab: create a runtime instance under this object
            instance = new GameObject(instanceName);
            instance.transform.SetParent(transform, false);
            instance.transform.localPosition = offset;
            BuildRuntimeUI(instance, out healthUI);
            EnsureCanvasForUI(instance);
        }
        else
        {
            // Avoid creating multiple instances
            Transform existing = transform.Find(instanceName);
            if (existing != null)
            {
                instance = existing.gameObject;
                healthUI = instance.GetComponentInChildren<HealthBarUI>();
                if (healthUI == null) BuildRuntimeUI(instance, out healthUI);
                EnsureCanvasForUI(instance);
            }
            else
            {
                instance = Instantiate(healthDisplayPrefab, transform);
                instance.name = instanceName;
                instance.transform.localPosition = offset;
                // If the prefab itself contains AttachHealthDisplay, its Start() will run and handle setup.
                // Otherwise, perform post-instantiation setup now.
                if (instance.GetComponent<AttachHealthDisplay>() == null)
                {
                    // ensure canvases are worldspace
                    var canvases = instance.GetComponentsInChildren<Canvas>(true);
                    if (canvases == null || canvases.Length == 0)
                    {
                        GameObject canvasGO = new GameObject("HealthCanvas");
                        canvasGO.transform.SetParent(instance.transform, false);
                        Canvas c = canvasGO.AddComponent<Canvas>();
                        c.renderMode = RenderMode.WorldSpace;
                        c.worldCamera = GetCamera();
                        canvasGO.AddComponent<GraphicRaycaster>();
                        var rt = canvasGO.GetComponent<RectTransform>();
                        rt.sizeDelta = new Vector2(200, 50);
                        canvasGO.transform.localScale = new Vector3(0.003f, 0.005f, 0.005f);
                    }
                    else
                    {
                        foreach (var c in canvases)
                        {
                            if (c.renderMode != RenderMode.WorldSpace)
                                c.renderMode = RenderMode.WorldSpace;
                            if (c.worldCamera == null) c.worldCamera = GetCamera();
                            c.transform.localScale = new Vector3(0.003f, 0.005f, 0.005f);
                        }
                    }

                    healthUI = instance.GetComponentInChildren<HealthBarUI>();
                    if (healthUI == null)
                    {
                        BuildRuntimeUI(instance, out healthUI);
                        EnsureCanvasForUI(instance);
                    }
                }
            }
        }

        if (healthUI != null && entity != null && entity.data != null)
            healthUI.Setup(Mathf.CeilToInt(entity.data.maxHealth));

        if (entity != null)
            entity.OnHealthChanged += OnEntityHealthChanged;
    }

    void OnDestroy()
    {
        if (entity != null)
            entity.OnHealthChanged -= OnEntityHealthChanged;
    }

    private void OnEntityHealthChanged(int current, int max)
    {
        if (healthUI != null)
        {
            healthUI.UpdateHealth(current);
        }
    }

    void LateUpdate()
    {
        if (instance == null) return;
        if (Camera.main == null) return;

        // Make the health display face the camera
        instance.transform.rotation = Quaternion.LookRotation(instance.transform.position - Camera.main.transform.position);
    }

    private void BuildRuntimeUI(GameObject parent, out HealthBarUI outHealthUI)
    {
        outHealthUI = null;

        // find or create a Canvas under parent
        Canvas canvas = parent.GetComponentInChildren<Canvas>();
        Transform canvasT;
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("HealthCanvas");
            canvasGO.transform.SetParent(parent.transform, false);
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = GetCamera();
            canvasGO.AddComponent<GraphicRaycaster>();
            var rt = canvasGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200, 50);
            // Make worldspace canvas small so UI elements have reasonable size in world units
            canvasGO.transform.localScale = new Vector3(0.003f, 0.005f, 0.005f);
            canvasT = canvasGO.transform;
        }
        else canvasT = canvas.transform;

        // Create background
        GameObject bg = new GameObject("HB_Background");
        bg.transform.SetParent(canvasT, false);
        var bgRt = bg.AddComponent<RectTransform>();
        bgRt.sizeDelta = new Vector2(160, 24);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.6f);

        // Create filler
        GameObject filler = new GameObject("HB_Filler");
        filler.transform.SetParent(bg.transform, false);
        var fillRt = filler.AddComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0f, 0f);
        fillRt.anchorMax = new Vector2(1f, 1f);
        fillRt.anchoredPosition = Vector2.zero;
        var fillImg = filler.AddComponent<Image>();
        fillImg.color = new Color(1f, 0.2f, 0.2f, 0.9f);
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;

        // Create text
        GameObject txt = new GameObject("HB_Text");
        txt.transform.SetParent(bg.transform, false);
        var txtRt = txt.AddComponent<RectTransform>();
        txtRt.sizeDelta = new Vector2(160, 24);
        var tmp = txt.AddComponent<TMP_Text>();
        tmp.text = "0 / 0";
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.fontSize = 18;

        // Add HealthBarUI and wire references
        var hbar = parent.GetComponent<HealthBarUI>();
        if (hbar == null) hbar = parent.AddComponent<HealthBarUI>();
        hbar.fillImage = fillImg;
        hbar.healthText = tmp as TMP_Text;
        outHealthUI = hbar;
    }

    // Ensure any existing UI graphics under 'instance' have a WorldSpace Canvas as ancestor.
    // If none exists, create one and reparent graphics under it so they become visible.
    private void EnsureCanvasForUI(GameObject instance)
    {
        if (instance == null) return;
        // find any UI graphics (Image, RawImage, TMP_Text)
        var imgs = instance.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
        var tmps = instance.GetComponentsInChildren<TMPro.TMP_Text>(true);

        bool hasGraphics = (imgs != null && imgs.Length > 0) || (tmps != null && tmps.Length > 0);
        if (!hasGraphics) return;

        // If there's already a Canvas ancestor for the instance's graphics, ensure it's WorldSpace
        Canvas existingCanvas = instance.GetComponentInChildren<Canvas>(true);
        if (existingCanvas == null)
        {
            GameObject canvasGO = new GameObject("HealthCanvas");
            canvasGO.transform.SetParent(instance.transform, false);
            Canvas c = canvasGO.AddComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;
            c.worldCamera = GetCamera();
            canvasGO.AddComponent<GraphicRaycaster>();
            var rt = canvasGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200, 50);
            canvasGO.transform.localScale = new Vector3(0.003f, 0.005f, 0.005f);

            // Reparent top-level graphic-containing children under the canvas to ensure they render
            foreach (var g in imgs)
            {
                if (g == null) continue;
                // move the GameObject to be child of canvas if it isn't already under it
                if (g.transform.IsChildOf(canvasGO.transform)) continue;
                g.transform.SetParent(canvasGO.transform, false);
            }
            foreach (var t in tmps)
            {
                if (t == null) continue;
                if (t.transform.IsChildOf(canvasGO.transform)) continue;
                t.transform.SetParent(canvasGO.transform, false);
            }
        }
        else
        {
            // ensure existing canvas is worldspace and scaled properly
            var canvases = instance.GetComponentsInChildren<Canvas>(true);
            foreach (var c in canvases)
            {
                c.renderMode = RenderMode.WorldSpace;
                if (c.worldCamera == null) c.worldCamera = GetCamera();
                c.transform.localScale = new Vector3(0.003f, 0.005f, 0.005f);
            }
        }
    }

    private Camera GetCamera()
    {
        if (Camera.main != null) return Camera.main;
        if (Camera.current != null) return Camera.current;
        var any = FindObjectOfType<Camera>();
        return any;
    }
}
