using System.Diagnostics;
using UnityEngine;

public class SpeechRecognizer : MonoBehaviour
{
    public PythonEngine py;
    public string lastSentence = "";

    void Start()
    {
        py = new PythonEngine(Config.PYTHON_SCRIPT_SPEECH_RECOGNIZE);
        py.addPassiveListener(saveData);
        py.runInBackGround();
    }


    private void saveData(object sender, DataReceivedEventArgs e)
    {
        if(e!=null && e.Data!=null && e.Data.Trim()!="")
        {
            
            var text = e.Data.Trim();
            if (text.EndsWith("."))  //  isfinal = true
            {
                lastSentence += text;
            }
            py.data = lastSentence +" "+ text;
            if(!Config.SILENCE_MODE)
            {
                print("!!");
                print(py.data);
            }
        }
    }

    public string GetRealTimeSpeechText()
    {
        return py == null ? "" : py.data;
    }

    
    private void OnApplicationQuit()
    {
        if(py!=null)
            py.exit();
    }
}

