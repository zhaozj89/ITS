using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class PythonEngine
{
    public string data;
    public string pythonFile;

    private Process process;
    private Action<object, DataReceivedEventArgs> receivedHandler;
    private bool exitFlag;

    public PythonEngine(string pythonFile)
    {
        this.data = "";
        this.pythonFile = pythonFile;
        process = new Process();
    }

    // passive callback
    public void addPassiveListener(Action<object, DataReceivedEventArgs> receivedHandler)
    {
        process.OutputDataReceived += new DataReceivedEventHandler(receivedHandler);
    }

    public void runInBackGround()
    {
        Task.Factory.StartNew(() => runCmd(pythonFile));
    }

    public void runCmd(string args)
    {
        process.StartInfo.FileName = Config.PYTHON_ENGINE_LOCATION;
        process.StartInfo.Arguments = string.Format("{0}", args);
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;  // Any error in standard output will be redirected back (for example exceptions)
        process.StartInfo.RedirectStandardInput = true;
        try
        {
            process.Start();

            if(Config.PYTHON_DEBUG_FLAG)
            {
                //UnityEngine.Debug.Log(process.StandardOutput.ReadToEnd());
                //UnityEngine.Debug.Log(process.StandardError.ReadToEnd());
            }

            process.BeginOutputReadLine();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log(ex);
        }
    }

    public void exit()
    {
        if (!process.HasExited)
        {
            process.Kill();
        }
    }
}

