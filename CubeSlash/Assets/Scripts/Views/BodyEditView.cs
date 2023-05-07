using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BodyEditView : View
{
    private Player Player { get { return Player.Instance; } }
    private Body Body { get { return Player.Body; } }

    private void Start()
    {
        InitializePlayer();
    }

    private void InitializePlayer()
    {
        Player.gameObject.SetActive(true);
        Player.Rigidbody.isKinematic = true;
    }
}