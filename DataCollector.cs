using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Xml;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;


public class DataCollector : MonoBehaviour
{
    //Les régles à surtout respecter:
    //1. Pas de charactère spéciaux, pas d'espace, que ABCDEFGHIJKLMNOPQRSTUVW 0123456789 _- dans les variables Game et version
    //2. Essayer de limiter le nombre de données envoyé pour économisé l'espace (une frame sur deux ou trois par exemple)
    //3. Faite attention a envoyé que des données de type String ou Int
    //4. Cassez pas tout
    //5. J'ai fait les régles en français et les commentaires en anglais me demandez pas pourquoi

    [Header("Setup")]
    public string Game = "GameName";
    [Tooltip("Used to distinguished differents builds ")]
    public string Version = "GameVersion";
    public KeyCode triggerKey = KeyCode.None; // CHANGE THE KEYCODE TO MAKE IT WORK <=====================================================

    const string url = "Demander moi URL";

    [Header("Data")]
    //Those lists will be stored in an excel file
    public List<int> _Time;
    public List<string> _Data1;
    public List<int> _Data2;


    // Start is called before the first frame update
    void Start()
    {
        _Time = new List<int>();
        _Data1 = new List<string>();
        _Data2 = new List<int>();
    }


    int frameSkip = 2;
    // Update is called once per frame
    void Update()
    {
        //Skipping 2 frame between each sample to reduce file size
        frameSkip--;
        if (frameSkip < 0)
        {
            frameSkip = 2;
        }
        else
        {
            return;
        }

        //Adding time since startup for each frame
        _Time.Add((int)Time.realtimeSinceStartup);
        _Data1.Add("MyData");
        //Added current framerate for each frame
        _Data2.Add((int)(1.0f / Time.deltaTime));


        //Triggering the upload /!\ PLS USE GETKEYDOWN no spam pls /!\
        if (Input.GetKeyDown(triggerKey))
        {
            StartCoroutine(SendData());
        }
    }

    IEnumerator SendData()
    {
        //Creating a form to store data
        WWWForm form = new WWWForm();

        //Creating a random filename
        string fileName = Path.GetRandomFileName();
        fileName = fileName.Substring(0, 6);
        fileName = fileName.ToUpper();
        fileName = Game+"_"+Version+"_"+fileName + ".csv";


        //Adding data send to my website for ordering purpose
        form.AddField("Game", Regex.Replace(Game, "[^\\w\\._]", ""));
        form.AddField("Version", Regex.Replace(Version, "[^\\w\\._]", ""));
        form.AddField("Pcname", Regex.Replace(SystemInfo.deviceName, "[^\\w\\._]", ""));
        form.AddField("Filename", fileName);

        byte[] excel = generateExcel();
        //Adding the excel file to the web form. Syntax:
        // "file" = the name of the variable in the webserver
        // "excel" = the actual file in bytes
        // "filename" = the filename
        // "text/csv" = The Mime type for the webserver
        form.AddBinaryData("file", excel, fileName, "text/csv");

        //Creating the webrequest
        using (var w = UnityWebRequest.Post(url, form))
        {
            yield return w.SendWebRequest();
            if (w.result != UnityWebRequest.Result.Success)
            {
                //Request failed
                Debug.LogError(w.error);
            }
            else
            {
                //Data uploaded !
                Debug.Log(w.downloadHandler.text);
                Debug.Log("Data Uploaded");
            }
        }
    }

    byte[] generateExcel()
    {
        //Creating excel headers, each collumn is seperated by ";"
        string cols = "Time; Data1; FPS;" + SystemInfo.processorType + ";" + SystemInfo.graphicsDeviceName;

        //Creating our data buffer, each line is seperated by "+ System.Environment.NewLine"
        string dataCSV = cols + System.Environment.NewLine;

        for (int i = 0; i < _Time.Count; i++)
        {
            dataCSV += _Time[i] + ";" + _Data1[i] + ";" + _Data2[i] + System.Environment.NewLine;
        }

        //CSV is done, converting string to bytes for transfer
        return Encoding.UTF8.GetBytes(dataCSV);

    }
}
