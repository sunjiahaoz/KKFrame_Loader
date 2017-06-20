using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using KK.Frame.Loader;

public class Scene1 : MonoBehaviour {

    public Image img;

    void Awake()
    {
        Loader.Instance.Init(null);
    }

    void OnGUI()
    {
        if (GUILayout.Button("Click", GUILayout.Width(100), GUILayout.Height(100)))
        {
            Loader.Instance.LoadPrefab("KKTestGame", "Prefab1", (go) => 
            {
                go.transform.position = Vector3.zero;
                go.transform.localScale = Vector3.one;
            }, true);
        }

        if (GUILayout.Button("Img", GUILayout.Width(100), GUILayout.Height(100)))
        {
            Loader.Instance.LoadMultipleTexture("KKTestGame", "majiang2D_newCard", (sps) => 
            {
                img.sprite = sps[1] as Sprite;
            });
        }

        if (GUILayout.Button("TOScene2", GUILayout.Width(100), GUILayout.Height(100)))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("scene2");
        }
    }
}
