using System.Collections.Generic;
using UnityEngine;

public class VignetteController : Singleton
{
    public static VignetteController Instance { get { return Instance<VignetteController>(); } }

    private VignetteGlow prefab_glow;

    private List<VignetteGlow> glows = new List<VignetteGlow>();
    private List<ScreenTransform> screen_transforms = new List<ScreenTransform>();

    private class ScreenTransform
    {
        public Transform transform;
        public Vector2 normalized_position;

        public void UpdatePosition()
        {
            var w = CameraController.Instance.Width;
            var h = CameraController.Instance.Height;
            var c = CameraController.Instance.Camera.transform.position.SetZ(0) - new Vector3(w * 0.5f, h * 0.5f);
            var x = normalized_position.x;
            var y = normalized_position.y;
            transform.position = c + new Vector3(w * x, h * y);
        }
    }

    protected override void Initialize()
    {
        base.Initialize();
        prefab_glow = Resources.Load<VignetteGlow>("Prefabs/Vignette/VignetteGlow");

        AreaController.Instance.onNextArea += OnNextArea;

        CreateGlows();
        FadeIn();
    }

    private void Update()
    {
        foreach(var trans in screen_transforms)
        {
            trans.UpdatePosition();
        }
    }

    private void OnNextArea(Area area)
    {
        SetArea(area);
    }

    public void SetArea(Area area)
    {
        foreach (var glow in glows)
        {
            var color = area.vignette_gradient.Evaluate(Random.Range(0f, 1f));
            glow.FadeColor(color);
        }
    }

    private void CreateGlows()
    {
        var w = CameraController.Instance.Width;
        var h = CameraController.Instance.Height;
        var glows_per_w = 0;
        var units_per_glow = w / glows_per_w;
        var glows_per_h = (int)(h / units_per_glow);

        for (int i = 0; i < glows_per_w; i++)
        {
            var x = (float)i / (glows_per_w - 1);
            CreateGlowTransform(new Vector2(x, 0));
            CreateGlowTransform(new Vector2(x, 1));
        }

        for (int i = 0; i < glows_per_h; i++)
        {
            var y = (float)i / (glows_per_h - 1);
            CreateGlowTransform(new Vector2(0, y));
            CreateGlowTransform(new Vector2(1, y));
        }

        void CreateGlowTransform(Vector2 normalized_position)
        {
            var glow = CreateGlow();
            var trans = new ScreenTransform
            {
                transform = glow.transform,
                normalized_position = normalized_position,
            };
            screen_transforms.Add(trans);
        }
    }

    private VignetteGlow CreateGlow()
    {
        var inst = Instantiate(prefab_glow, GameController.Instance.world);
        glows.Add(inst);
        return inst;
    }

    private void FadeIn()
    {
        foreach(var glow in glows)
        {
            glow.FadeAlpha(4f, 1f);
        }
    }
}