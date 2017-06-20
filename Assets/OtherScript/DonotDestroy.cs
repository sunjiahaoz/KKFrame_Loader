using UnityEngine;
using System.Collections;

public class DonotDestroy : MonoBehaviour {    
    void Awake()
    {   
        DontDestroyOnLoad(gameObject);        
    }    
}
