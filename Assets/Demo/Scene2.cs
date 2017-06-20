using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Scene2 : MonoBehaviour {
    public Image img;
    void OnGUI()
    {
        if (GUILayout.Button("Click", GUILayout.Width(100), GUILayout.Height(100)))
        {
            Loader.Instance.LoadMultipleTexture("KKTestGame", "majiang2D_newCard", (sp) => 
            {
                img.sprite = sp[2] as Sprite;
            });
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Home))
        {
            Debug.Log("<color=green>[log]</color>---" + "Unload");
            Resources.UnloadUnusedAssets();
        }
    }
}
