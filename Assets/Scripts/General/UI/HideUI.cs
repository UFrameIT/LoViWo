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
                    CommunicationEvents.closeWindowEvent.Invoke();
            }


        }
        /*
        //Todo before capturing: Make directories "UFrameIT-Screenshots/Unity_ScreenCapture" in project folder
        else if (Input.GetKeyDown(ScreenshotKey)) {
            ScreenCapture.CaptureScreenshot("UFrameIT-Screenshots\\Unity_ScreenCapture\\Capture.png");
        }
        */
    }
}
