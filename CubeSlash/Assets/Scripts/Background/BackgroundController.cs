using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Flawliz.Lerp;

public class BackgroundController : Singleton
{
    public static BackgroundController Instance { get { return Instance<BackgroundController>(); } }

    private List<FogLayer> fogs = new List<FogLayer>();
    private List<BackgroundObject> objects = new List<BackgroundObject>();

    private Area current_area;

    public const float OBJECT_FADE_TIME = 5f;

    protected override void Initialize()
    {
        base.Initialize();
        AreaController.Instance.onNextArea += OnNextArea;
        GameController.Instance.onMainMenu += OnMainMenu;
    }

    private void Update()
    {
        ParallaxUpdate();
    }

    private void OnMainMenu()
    {
        ClearObjectsImmediate();
    }

    private void OnNextArea(Area area)
    {
        FadeToArea(area);
    }

    public void FadeToArea(Area area)
    {
        if (current_area == area) return;
        current_area = area;

        ClearObjects();
        CreateSprites(area);
        CreateParticles(area);
        FadeBackground(area);
        FadeFog(area);
    }

    private void ParallaxUpdate()
    {
        var cam_pos = GetCameraPosition();
        foreach(var parallax in objects)
        {
            parallax.UpdateParallax(cam_pos);
        }
    }

    private Vector3 GetCameraPosition() => CameraController.Instance.Camera.transform.position.SetZ(0);

    private void ClearObjects()
    {
        foreach(var o in objects)
        {
            o.Destroy();
        }
        objects.Clear();
    }

    private void ClearObjectsImmediate()
    {
        foreach(var o in objects)
        {
            o.DestroyImmediate();
        }
        objects.Clear();
    }

    private void CreateSprites(Area area)
    {
        // Create
        var layers = area.background_layers;
        for (int i_layer = 0; i_layer < layers.Count; i_layer++)
        {
            var layer = layers[i_layer];
            for (int i = 0; i < layer.count_sprite; i++)
            {
                var bg = Instantiate(Resources.Load<BackgroundSprite>("Prefabs/Background/BackgroundSprite"));
                objects.Add(bg);
                bg.Layer = i_layer;
                bg.transform.parent = GameController.Instance.world;

                bg.Sprite = layer.sprites.Random();
                bg.spr.color = bg.spr.color.SetA(0);
                bg.AnimateAppear(OBJECT_FADE_TIME);

                var r = Random.insideUnitCircle;
                bg.transform.localPosition = new Vector3(r.x, r.y) * CameraController.Instance.Width + Vector3.forward * (2 + 10 + 10 * i_layer);
                bg.transform.localScale = Vector3.one * Random.Range(layer.size_sprite_min, layer.size_sprite_min);
                bg.transform.rotation = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward);
                bg.Initialize(area);
            }
        }
    }

    private void CreateParticles(Area area)
    {
        // Create
        var layers = area.background_layers;
        for (int i_layer = 0; i_layer < layers.Count; i_layer++)
        {
            var layer = layers[i_layer];
            foreach (var ps_prefab in layer.particles)
            {
                var bg = Instantiate(Resources.Load<BackgroundParticleSystem>("Prefabs/Background/BackgroundParticleSystem"));
                objects.Add(bg);
                bg.Layer = i_layer;
                bg.transform.parent = GameController.Instance.world;

                bg.transform.position = Player.Instance.transform.position;
                bg.Initialize(area);
                bg.SetPrefab(ps_prefab);
            }
        }
    }

    #region FOG
    private void FadeFog(Area area)
    {
        var layers = area.background_layers;
        var count = Mathf.Max(fogs.Count, layers.Count);
        for (int i = 0; i < count; i++)
        {
            var t = Mathf.Clamp(i / (float)(count + 1), 0, 1);
            var alpha = area.ac_alpha_fog.Evaluate(t);
            if(i >= fogs.Count) // Create new fog
            {
                var fog = CreateFog(i);
                fog.spr.color = area.color_fog.SetA(0);
                Lerp.Alpha(fog.spr, OBJECT_FADE_TIME, alpha);
                fogs.Add(fog);
            }
            else if(i >= layers.Count) // Destroy fog
            {
                var fog = fogs[i];
                fog.Destroy(OBJECT_FADE_TIME);
                fogs.RemoveAt(i);
            }
            else // Fade fog
            {
                Lerp.Color(fogs[i].spr, OBJECT_FADE_TIME, area.color_fog.SetA(alpha));
            }
        }
    }

    private FogLayer CreateFog(int layer)
    {
        var fog = Instantiate(Resources.Load<FogLayer>("Prefabs/Background/Fog"));
        fog.transform.SetParent(CameraController.Instance.Camera.transform);
        fog.transform.localPosition = new Vector3(0, 0, 20 + 10 * layer);
        return fog;
    }

    private void FadeBackground(Area area)
    {
        var cam = CameraController.Instance.Camera;
        var start = cam.backgroundColor;
        Lerp.Value("background_color_" + GetInstanceID(), OBJECT_FADE_TIME, f =>
        {
            CameraController.Instance.Camera.backgroundColor = Color.Lerp(start, area.color_bg, f);
        });
    }
    #endregion
}