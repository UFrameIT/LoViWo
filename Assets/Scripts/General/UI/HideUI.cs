using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CommunicationEvents;

public class HideUI : MonoBehaviour
{

    public KeyCode Key = KeyCode.LeftShift;
    //public KeyCode ScreenshotKey = KeyCode.F2;
    
    public bool LockOnly = true;
    public PlayerMovement PlayerControl;
    public MouseLook CamControl;
    public Canvas UICanvas;

    //TODO: Remove this bool
    private bool generatorOn = false;

    void Start()
    {

        if (!LockOnly)
        {
            if(UICanvas==null)
                UICanvas = GetComponentInChildren<Canvas>();
            bool camActive  = !UICanvas.enabled;
            CamControl.enabled = camActive;
            PlayerControl.enabled = camActive;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(Key))
        {

            if (LockOnly)
            {
                CamControl.enabled = !CamControl.enabled;
                PlayerControl.enabled = CamControl.enabled;
            }
            else
            {
                bool camActive = UICanvas.enabled;
                UICanvas.enabled = !UICanvas.enabled;
                CamControl.enabled = camActive;
                PlayerControl.enabled = camActive;
                if (!UICanvas.enabled)
                    CommunicationEvents.closeUIEvent.Invoke();
            }


        }
        /*
        //Todo before capturing: Make directories "UFrameIT-Screenshots/Unity_ScreenCapture" in project folder
        else if (Input.GetKeyDown(ScreenshotKey)) {
            ScreenCapture.CaptureScreenshot("UFrameIT-Screenshots\\Unity_ScreenCapture\\Capture.png");
        }
        */
        //TODO: Remove this Code, remove the generatorOn/Off events, build a menu and invoke Activate-/Stop-Method
        //Of Generator-Script
        else if (Input.GetKeyDown(KeyCode.LeftControl)) {
            if (!generatorOn)
            {
                generatorOn = true;
                generatorOnEvent.Invoke(false, null, 10.0f);
            }
            else {
                generatorOn = false;
                generatorOffEvent.Invoke();
            }
        }
    }
}
