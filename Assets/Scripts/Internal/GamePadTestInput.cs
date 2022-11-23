using UnityEngine;

public class GamePadTestInput : MonoBehaviour
{
    public float LeftStickX;
    public float LeftStickY;
    public float DPadX;
    public float DPadY;
    private void Update()
    {
        LeftStickX = Input.GetAxis("LeftStickX");
        LeftStickY = Input.GetAxis("LeftStickY");
        DPadX = Input.GetAxis("DPadX");
        DPadY = Input.GetAxis("DPadY");
    }
}
