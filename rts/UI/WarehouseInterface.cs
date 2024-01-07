using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

// Expected transform layout
/* WarehouseWindow
 *  StoreContent (Has GridLayout)
 *      Children with a dropdown and input box
 *  StoredContent (Has GridLayout)
 *      Children
 *          Amount
 *          ResourceName
*/

public class WarehouseInterface : ManagedMonoBehaviour
{
    List<Dropdown.OptionData> _dropdownOptions = new List<Dropdown.OptionData>();
    Warehouse current = null;

    void Start()
    {
        _dropdownOptions.Add(new Dropdown.OptionData() { text = "Anything" });
        foreach (var rt in Enum.GetNames(typeof(ResourceType)))
        {
            _dropdownOptions.Add(new Dropdown.OptionData() { text = rt });
        }
        var storeTrans = transform.FindChild("StoreContent");
        for (int i = 0; i < Warehouse.MaxConstraints; i++)
        {
            var dd = storeTrans.GetChild(i).GetComponent<Dropdown>();
            dd.options = _dropdownOptions;
        }
        /*GameObject reference = transform.FindChild("StoredContent").GetChild(0).gameObject;
        for (int i = 1; i < Warehouse.MaxStoredTypes; i++)
        {
            var newo = GameObject.Instantiate(reference);
            newo.transform.SetParent(reference.transform.parent);
        }*/
    }

    public void UpdateForWarehouse(Warehouse warehouse)
    {
        if(current == null)
        {
            warehouse.ContentsChanged += UpdateData;
            warehouse.ConstraintsChanged += UpdateData;
        }
        current = warehouse;

        var storedTrans = transform.FindChild("StoredContent");
        var storedResources = warehouse.GetStoredResources();
        for(int i = 0; i < Warehouse.MaxStoredTypes; i++)
        {
            if (i < storedResources.Length)
            {
                var trans = storedTrans.GetChild(i);
                var amountText = trans.Find("Amount").GetComponentInChildren<Text>();
                var nameText = trans.Find("ResourceName").GetComponentInChildren<Text>();
                amountText.text = storedResources[i].resourceAmount.ToString();
                nameText.text = storedResources[i].resourceName;
            }
            else
            {
                var trans = storedTrans.GetChild(i);
                var amountText = trans.Find("Amount").GetComponentInChildren<Text>();
                var nameText = trans.Find("ResourceName").GetComponentInChildren<Text>();
                amountText.text = "Empty";
                nameText.text = "";
            }
        }

        var storeTrans = transform.FindChild("StoreContent");
        var constraints = warehouse.GetConstraints();
        for(int i =0; i < Warehouse.MaxConstraints; i++)
        {
            int localI = i; // copy for the delegate
            var dd = storeTrans.GetChild(i).GetComponent<Dropdown>();
            var ifd = storeTrans.GetChild(i).GetComponentInChildren<InputField>();
            UnityAction<string> ifdvc = (string value) =>
            {
                var val = int.Parse(value);
                warehouse.SetConstraint(localI, dd.value == 0 ? null : (ResourceType?)(dd.value - 1), val);
            };
            ifd.onEndEdit.RemoveAllListeners();
            ifd.text = constraints[i].resourceType == null ? 0.ToString() : constraints[i].resourceAmount.ToString();
            dd.onValueChanged.RemoveAllListeners();
            int svalue = constraints[i].resourceType == null ? 0 : (int)constraints[i].resourceType + 1;
            dd.value = svalue;
            UnityAction<int> vc = (int value) =>
            {
                if (warehouse == null)
                    return;
                var val = int.Parse(ifd.text);
                warehouse.SetConstraint(localI, value == 0 ? null : (ResourceType?)(value - 1), val);
            };
            dd.onValueChanged.AddListener(vc);
            ifd.onEndEdit.AddListener(ifdvc);
            dd.RefreshShownValue();
        }
    }

    void UpdateData()
    {
        UpdateForWarehouse(current);
    }

    new void OnDisable()
    {
        if (current != null)
        {
            current.ContentsChanged -= UpdateData;
            current.ConstraintsChanged -= UpdateData;
            current = null;
        }
        base.OnDisable();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
