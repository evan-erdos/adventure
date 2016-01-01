
using UnityEngine;
using ui=UnityEngine.UI;
using PathwaysEngine;
using inv=PathwaysEngine.Inventory;
using System.Collections;


[RequireComponent(typeof(ui::Button))]
[RequireComponent(typeof(ui::Image))]
class Slot : MonoBehaviour {
//    where T : inv::Item {
    ui::Image image;

    public inv::Item item {
        get { return _item; }
        set { _item = value;
            image.sprite = (_item)?value.Icon:null;
            image.color = (image.sprite!=null)
                ? new Color(255,255,255,255)
                : new Color(255,255,255,0);
        }
    } inv::Item _item;


    void Awake() {
        image = GetComponent<ui::Image>();
        image.sprite = null;
        image.color = new Color(255,255,255,0);
    }
}