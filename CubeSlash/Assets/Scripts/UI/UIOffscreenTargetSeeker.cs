using UnityEngine;
using UnityEngine.UI;

public class UIOffscreenTargetSeeker : MonoBehaviour
{
    [SerializeField] private Image img_glow;

    private const float ALPHA = 0.2f;
    private const float SPEED_FADE_IN = 5f;
    private const float SPEED_FADE_OUT = 10f;

    public Transform Target { get; set; }

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        img_glow.color = img_glow.color.SetA(0);
    }

    private void Update()
    {
        if (Target == null)
        {
            gameObject.SetActive(false);
        }

        var wh = Screen.width * 0.5f; // Half screen width
        var hh = Screen.height * 0.5f; // Half screen height
        var d = wh; // Max distance outside screen
        var p = cam.WorldToScreenPoint(Target.position).AddX(-wh).AddY(-hh); // Target screen position
        var xa = p.x.Abs(); // Absolute x coord
        var ya = p.y.Abs(); // Absolute y coord
        if((xa > wh) || (ya > hh))
        {
            var abs = xa > ya ? xa : ya; // Biggest absolute coord
            var h = xa > ya ? wh : hh; // Width or height, based on biggest coord
            var t = Mathf.Clamp(((abs - h) / d), 0, 1); // t value outside screen
            var a = t * ALPHA;
            img_glow.color = Color.Lerp(img_glow.color, img_glow.color.SetA(a), Time.deltaTime * SPEED_FADE_IN);
        }
        else
        {
            img_glow.color = img_glow.color = Color.Lerp(img_glow.color, img_glow.color.SetA(0), Time.deltaTime * SPEED_FADE_OUT);
        }

        transform.position = new Vector2(Mathf.Clamp(p.x, -wh, wh) + wh, Mathf.Clamp(p.y, -hh, hh) + hh);
    }
}