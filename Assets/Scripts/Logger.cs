using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Logger
{
    public System.IO.StreamWriter file=null;

    public Logger(string fileName)
    {
        file = new System.IO.StreamWriter(fileName, true);
    }

    public void Log(string lines)
    {
        try
        {
            if(file!=null)
            {
                file.WriteLine("[***" + DateTime.Now + "***]" + lines);
            }
        }
        catch
        {
        }
    }

    public void Close()
    {
        if (file != null)
        {
            file.Close();
            file = null;
        }
    }
}