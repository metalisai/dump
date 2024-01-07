using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NumberField : ManagedMonoBehaviour {

    public InputField InputField;
    public int minValue = 0;
    public int maxValue = 999;

    public void Increment()
    {
        int val = int.Parse(InputField.text) + 1;
        val = Mathf.Min(maxValue, val);
        InputField.text = val.ToString();
        InputField.onEndEdit.Invoke(InputField.text);
    }

    public void Decrement()
    {
        int val = int.Parse(InputField.text) - 1;
        val = Mathf.Max(minValue, val);
        InputField.text = val.ToString();
        InputField.onEndEdit.Invoke(InputField.text);
    }
}
