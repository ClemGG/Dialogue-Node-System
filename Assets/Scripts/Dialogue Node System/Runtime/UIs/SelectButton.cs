using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Ce script corrige le bug qui empêche le bouton de choix n°1 d'être sélectionné.

public class SelectButton : MonoBehaviour
{

    [SerializeField] private Button _btnToSelect;

    // Start is called before the first frame update
    void OnEnable()
    {
        _btnToSelect.Select();
        _btnToSelect.OnSelect(new BaseEventData(EventSystem.current));
    }

}
