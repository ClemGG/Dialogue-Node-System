using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Fixes the bug preventing the choices slot #1 to be selected

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
