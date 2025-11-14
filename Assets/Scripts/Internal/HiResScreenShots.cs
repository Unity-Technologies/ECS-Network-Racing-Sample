using UnityEngine;
using System.Collections;
using System.IO;

public class HiResScreenShots : MonoBehaviour
{
    public int resWidth = 2550;
    public int resHeight = 3300;

    public GameObject[] prefabs;
    private GameObject m_Instance;
    private bool m_TakeHiResShot = false;

    public void TakeHiResShot()
    {
        m_TakeHiResShot = true;
    }

    void LateUpdate()
    {
        if (m_TakeHiResShot)
            return;

        m_TakeHiResShot |= Input.GetKeyDown("k");
        if (m_TakeHiResShot)
        {
            StartCoroutine(TakeScreenshot());
        }
    }

    private IEnumerator TakeScreenshot() 
    {
        for (int i = 0; i < prefabs.Length; i++)
        {
            yield return m_Instance == null;
            m_Instance = Instantiate(prefabs[i]);
            m_Instance.transform.position = Vector3.zero;

            var rt = new RenderTexture(resWidth, resHeight, 24);
            GetComponent<Camera>().targetTexture = rt;
            var screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            GetComponent<Camera>().Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            GetComponent<Camera>().targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);

            var bytes = screenShot.EncodeToPNG();
            var filename = ScreenShotName(i);
            File.WriteAllBytes(filename, bytes);
            
            Debug.Log(string.Format("Took screenshot to: {0}", filename));
            yield return new WaitForEndOfFrame();
            Destroy(m_Instance);
            yield return new WaitForEndOfFrame();
        }

        m_TakeHiResShot = false;
    }

    public static string ScreenShotName(int index)
    {
        var namingFormat = "{0}/Art/Textures/UI/CarSelection/car-{1}.jpg";
        return string.Format(namingFormat, Application.dataPath, index + 1);
    }
}