using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class PlayerTransform : MonoBehaviour
{
    [SerializeField] SpriteLibrary spriteLibrary;
    [SerializeField] SpriteLibraryAsset[] forms; // 0: normal, 1: formA, 2: formB...
    int index = 0;

    void Awake()
    {
        if (!spriteLibrary) spriteLibrary = GetComponent<SpriteLibrary>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) // nút biến hình
        {
            index = (index + 1) % forms.Length;
            spriteLibrary.spriteLibraryAsset = forms[index];
        }
    }
}
