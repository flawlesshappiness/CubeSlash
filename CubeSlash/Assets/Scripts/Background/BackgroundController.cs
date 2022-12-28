using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Flawliz.Lerp;

public class BackgroundController : Singleton
{
    public static BackgroundController Instance { get { return Instance<BackgroundController>(); } }

    private List<FogLayer> fogs = new List<FogLayer>();
    private List<ParticleSystem> particles = new List<ParticleSystem>();
    private List<BackgroundSprite> sprites = new List<BackgroundSprite>();

    private LevelAsset currentLevel;

    protected override void Initialize()
    {
        base.Initialize();
        GameController.Instance.OnNextLevel += OnNextLevel;
    }

    private void Update()
    {
        ParallaxUpdate();
    }

    private void OnNextLevel()
    {
        FadeToLevel(Level.Current, 5f);
    }

    public void FadeToLevel(LevelAsset level, float time)
    {
        if (currentLevel == level) return;
        currentLevel = level;

        FadeBackground(level, time);
        FadeFog(level, time);
        CreateSprites(level.area.background_layers, time);
        CreateParticles(level.area.background_layers);
    }

    #region SPRITES & PARALLAX
    private void CreateSprites(List<BackgroundLayer> layers, float time)
    {
        // Clear
        foreach(var sprite in sprites)
        {
            sprite.Destroy(time);
        }
        sprites.Clear();

        // Create
        for (int i_layer = 0; i_layer < layers.Count; i_layer++)
        {
            var layer = layers[i_layer];
            for (int i = 0; i < layer.count_sprite; i++)
            {
                var bg = Instantiate(Resources.Load<BackgroundSprite>("Prefabs/Background/BackgroundSprite"));
                bg.Sprite = layer.sprites.Random();
                bg.spr.color = bg.spr.color.SetA(0);
                bg.AnimateAppear(time);
                sprites.Add(bg);

                bg.transform.parent = GameController.Instance.world;
                var r = Random.insideUnitCircle;
                bg.transform.localPosition = new Vector3(r.x, r.y) * CameraController.Instance.Width + Vector3.forward * (2 + 10 + 10 * i_layer);
                bg.StartPosition = bg.transform.localPosition;
                bg.Layer = i_layer;
                bg.transform.localScale = Vector3.one * Random.Range(layer.size_sprite_min, layer.size_sprite_min);
                bg.transform.rotation = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward);
            }
        }
    }

    private void ParallaxUpdate()
    {
        var w = CameraController.Instance.Width;
        var w2 = w * 2;
        var h = CameraController.Instance.Height;
        var h2 = h * 2;
        var pos_cam = CameraController.Instance.Camera.transform.position.SetZ(0);
        var layers = Level.Current.area.background_layers.Count;
        var pmin = Level.Current.area.parallax_min;
        var pmax = Level.Current.area.parallax_max;
        foreach(var sprite in sprites)
        {
            var scale = sprite.transform.localScale.x;

            var t = 1f - Mathf.Clamp(sprite.Layer / (float)(layers-1), 0, 1);
            var tp = Mathf.Lerp(pmin, pmax, t);
            var pp = sprite.StartPosition - pos_cam * tp;
            sprite.transform.localPosition = pos_cam + new Vector3(pp.x, pp.y, sprite.StartPosition.z) + sprite.Offset;

            var dir_from_cam = sprite.transform.position - pos_cam;
            var w_max = scale + w;
            var h_max = scale + h;
            if (dir_from_cam.x < -w_max) sprite.Offset += new Vector3(w_max * 2, 0);
            if (dir_from_cam.x > w_max) sprite.Offset -= new Vector3(w_max * 2, 0);
            if (dir_from_cam.y < -h_max) sprite.Offset += new Vector3(0, h_max * 2);
            if (dir_from_cam.y > h_max) sprite.Offset -= new Vector3(0, h_max * 2);
        }
    }
    #endregion
    #region PARTICLES
    private void CreateParticles(List<BackgroundLayer> layers)
    {
        // Clear
        foreach(var ps in particles)
        {
            StartCoroutine(StopParticleCr(ps));
        }
        particles.Clear();

        // Create
        var cam = CameraController.Instance.Camera;
        for (int i = 0; i < layers.Count; i++)
        {
            var layer = layers[i];
            foreach (var ps_prefab in layer.particles)
            {
                var ps = Instantiate(ps_prefab, cam.transform);
                particles.Add(ps);
                ps.transform.position = cam.transform.position + Vector3.forward * (1 + 20 + 10 * i);
            }
        }

        IEnumerator StopParticleCr(ParticleSystem ps)
        {
            var psm = ps.main;
            var pse = ps.emission;
            pse.enabled = false;
            var lifetime = Mathf.Max(psm.startLifetime.constant, psm.startLifetime.constantMax);
            yield return new WaitForSeconds(lifetime);
            Destroy(ps.gameObject);
        }
    }
    #endregion
    #region FOG
    private void FadeFog(LevelAsset level, float time)
    {
        var layers = level.area.background_layers;
        var count = Mathf.Max(fogs.Count, layers.Count);
        for (int i = 0; i < count; i++)
        {
            var t = Mathf.Clamp(i / (float)(count + 1), 0, 1);
            var alpha = level.area.ac_alpha_fog.Evaluate(t);
            if(i >= fogs.Count) // Create new fog
            {
                var fog = CreateFog(i);
                fog.spr.color = level.area.color_fog.SetA(0);
                Lerp.Alpha(fog.spr, time, alpha);
                fogs.Add(fog);
            }
            else if(i >= layers.Count) // Destroy fog
            {
                var fog = fogs[i];
                fog.Destroy(time);
                fogs.RemoveAt(i);
            }
            else // Fade fog
            {
                Lerp.Color(fogs[i].spr, time, level.area.color_fog.SetA(alpha));
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

    private void FadeBackground(LevelAsset level, float time)
    {
        var cam = CameraController.Instance.Camera;
        var start = cam.backgroundColor;
        Lerp.Value("background_color_" + GetInstanceID(), time, f =>
        {
            CameraController.Instance.Camera.backgroundColor = Color.Lerp(start, level.area.color_bg, f);
        });
    }
    #endregion
}