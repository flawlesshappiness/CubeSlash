using System.Collections;
using UnityEngine;

public class PlayerInputController : Singleton
{
    public static PlayerInputController Instance { get { return Instance<PlayerInputController>(); } }

    protected override void Initialize()
    {
        base.Initialize();
        MouseVibilityUpdate();
    }

    private Coroutine MouseVibilityUpdate()
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            var mouse_time = Time.time;
            while (true)
            {
                if (PlayerInput.Controls.Player.MouseDelta.ReadValue<float>() > 0)
                {
                    Cursor.visible = true;
                    mouse_time = Time.time + 3;
                }

                if (Time.time < mouse_time)
                {
                    yield return null;
                    continue;
                }

                Cursor.visible = false;
                yield return null;
            }
        }
    }
}