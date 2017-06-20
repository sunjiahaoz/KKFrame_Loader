using System;
using UnityEngine;
using KK.Frame.Loader;
public class Loader
{
    private static ILoader instance;
    public static ILoader Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Loader_ABS();
            }
            return instance;
        }
    }
}    

