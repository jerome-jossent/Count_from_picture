using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour
{

    [SerializeField] KeyCode toucheEXIT;

    private void Start()
    {
        if (toucheEXIT == KeyCode.None)
            toucheEXIT = KeyCode.Escape;
    }

    void Update()
    {
        if (Input.GetKey(toucheEXIT))
            Quit();
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }
}
