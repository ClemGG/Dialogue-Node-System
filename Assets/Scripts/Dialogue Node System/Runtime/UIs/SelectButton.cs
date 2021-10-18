using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Fixes the bug preventing the choices slot #1 to be selected

public class SelectButton : MonoBehaviour
{

    [SerializeField] private Button _btnToSelect;

    private bool ControllerIsConnected
    {
        get
        {
            return Input.GetJoystickNames().Length > 0;
        }
    }


    // Start is called before the first frame update
    void OnEnable()
    {
        //if controller is connected, select the button
        if (ControllerIsConnected)
        {
            _btnToSelect.Select();
            _btnToSelect.OnSelect(new BaseEventData(EventSystem.current));
        }
    }

}
