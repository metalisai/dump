using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[ExecuteInEditMode]
public class EditorServices : MonoBehaviour
{
    bool _createdInEditor = false;

    public void Start()
    {
        _createdInEditor = !Application.isPlaying;
    }

    public void Update()
    {
        if (_createdInEditor)
            MonoBehaviourManager.Update();
    }

    public void OnDestroy()
    {
        if (_createdInEditor)
            MonoBehaviourManager.Clear();
    }
}
