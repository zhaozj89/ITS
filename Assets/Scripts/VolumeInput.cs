using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class VolumeInput : MonoBehaviour
{

    public float m_loudness;
    public string m_mic_name;

    public bool m_detected_sound;
    public float threshold = 0.00002f; //0.002f;

    public float mean = 0;
    public float sd = 0.00000025f;
    public int n = 1;

    public float m_talking_time = 0;
    public float m_silent_time = 0;

    public AudioClip m_clip_record;
    public int m_sample_window = 128;
    bool m_is_initialized;

    private enum LexLevel { Non, Low, Normal, High };
    private LexLevel lexLevel;

    void Start()
    {
        StartCoroutine("UpdateEveryOneSecond");
    }

    IEnumerator UpdateEveryOneSecond()
    {
        while (true)
        {
            // callback
            if (lexLevel != LexLevel.Non)
            {
                if (lexLevel == LexLevel.High) gameObject.SendMessage("TooLoudPresentationUnitError", new PresentationUnitError("tooloud", Config.VerbalPriorityMapping["tooloud"]));

                if (lexLevel == LexLevel.Low) gameObject.SendMessage("TooSilentPresentationUnitError", new PresentationUnitError("toosilent", Config.VerbalPriorityMapping["toosilent"]));

            }

            //
            yield return new WaitForSeconds(1.0f);
        }
    }

    //mic initialization
    void InitMic()
    {
        for (int i = 0; i < Microphone.devices.Length; i++)
        {
            //Debug.Log("Microphone name***********************");
            //Debug.Log(Microphone.devices[i]);
            //Debug.Log("Microphone name***********************");
        }
        if (m_mic_name == null || m_mic_name == "")
        {
            if (Microphone.devices.Length == 1)
            {
                m_mic_name = Microphone.devices[0];
            }
            else
            {

                foreach (var device in Microphone.devices)
                {
                    if (device.ToLower().Contains("rift"))
                        m_mic_name = device;
                    else
                    {
                        m_mic_name = Microphone.devices[1];
                    }
                }
            }
        }

        m_clip_record = Microphone.Start(m_mic_name, true, 2, 44100);

        m_detected_sound = false;
        m_is_initialized = true;
        // threshold = mean + 2 * (Mathf.Sqrt(sd));
    }

    void StopMicrophone()
    {
        Microphone.End(m_mic_name);
    }

    //get data from microphone into audioclip
    float LevelMax()
    {
        float level_max = 0;
        float[] wave_data = new float[m_sample_window];
        int micPosition = Microphone.GetPosition(m_mic_name) - (m_sample_window + 1); // null means the first microphone

        if (micPosition < 0) return 0;
        m_clip_record.GetData(wave_data, micPosition);

        // rmse
        //float sum = 0;
        //for (int i = 0; i < m_sample_window; i++)
        //{
        //    sum += wave_data[i];
        //}
        //var RmsValue = Mathf.Sqrt(sum / m_sample_window); // rms = square root of average
        //if (RmsValue == 0) RmsValue = 0.00001f;
        //var DbValue = -20 * Mathf.Log10(RmsValue / 0.1f); // calculate dB
        //return DbValue;

        // Getting a peak on the last 128 samples
        for (int i = 0; i < m_sample_window; i++)
        {
            float wave_peak = wave_data[i] * wave_data[i];
            if (level_max < wave_peak)
            {
                level_max = wave_peak;
            }
        }
        return level_max;
    }

    void Update()
    {
        if (!m_is_initialized)
        {
            InitMic();
        }

        m_loudness = LevelMax();

        lexLevel = LexLevel.Non;

        if (!Config.SILENCE_MODE)
        {
            print("[loudness]" + m_loudness);
        }
        if (Time.time > 5.0f && m_loudness > threshold)
        {
            m_detected_sound = true;
            m_silent_time = 0;
            m_talking_time += Time.deltaTime;

            if (m_loudness >= Config.THRESHOLD_RHYTHM_MAX_VOLUME) lexLevel = LexLevel.High;
            else if (m_loudness >= Config.THRESHOLD_RHYTHM_MIN_VOLUME) lexLevel = LexLevel.Normal;
            else lexLevel = LexLevel.Low;
        }
        else
        {
            m_silent_time += Time.deltaTime;
            m_talking_time = 0;
            if (m_detected_sound)
            {
                StopMicrophone();
                InitMic();
            }
            else
            {
                n++;
                float new_mean = mean + (m_loudness - mean) / n;
                sd = sd + (m_loudness - mean) * (m_loudness - new_mean);
                mean = new_mean;
                //threshold = mean + 2 * (Mathf.Sqrt(sd / (n - 1)));
            }
            m_detected_sound = false;
        }
    }

    // start mic when scene starts
    void OnEnable()
    {
        m_talking_time = 0;
        m_silent_time = 0;
    }

    //stop mic when loading a new level or quit application
    void OnDisable()
    {
        StopMicrophone();
    }

    void OnDestroy()
    {
        StopMicrophone();
    }


    // make sure the mic gets started & stopped when application gets focused
    void OnApplicationFocus(bool focus)
    {

        if (focus && enabled)
        {
            if (!m_is_initialized)
            {
                InitMic();
                m_is_initialized = true;
            }
        }
        if (!focus)
        {
            StopMicrophone();
            m_is_initialized = false;
        }
    }
}