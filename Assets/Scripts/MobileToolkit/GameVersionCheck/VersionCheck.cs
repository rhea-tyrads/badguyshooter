using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
public class VersionCheck : MobileTool
{
    public string versionCheckUrl;
    public string googlePlayUrl;
    public string currentVersion;
    public SpreadsheetData data;
    public InternetConnection internetAccess;

    public Button downloadButton;



    void Start()
    {
        mobileUI.Hide();
        currentVersion = Application.version;
        if (Time.time < 0.1f)
            StartCoroutine(CheckGameVersion());
    }

    IEnumerator CheckGameVersion()
    {
        while (!internetAccess.hasConnection)
            yield return null;

        UnityWebRequest request = UnityWebRequest.Get(versionCheckUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching data: " + request.error);
        }
        else
        {
            string rawData = request.downloadHandler.text;
            // Debug.LogError("Rawdata: "+rawData);

            string[] rows = rawData.Split('\n');
            if (rows.Length > 1) // Assuming the first row could be headers
            {
                string[] cells = rows[1].Split(',');
                string latestVersion = cells[0].Trim();
                string latestVersionNumber = cells[1].Trim();
                data.Name = latestVersion;
                data.value = latestVersionNumber;
            }

            PromptForUpdate();
        }
    }

    void PromptForUpdate()
    {
        var current = float.Parse(currentVersion);
        var last = float.Parse(data.value);
        if (current >= last) return;
        //  if (currentVersion.Equals(data.value)) return;
        mobileUI.Show();
        downloadButton.onClick.AddListener(OpenGameStore);
    }

    void OpenGameStore()
    {
        Application.OpenURL(googlePlayUrl);
    }

    [Serializable]
    public class SpreadsheetData
    {
        public string Name;
        public string value;
    }
}
