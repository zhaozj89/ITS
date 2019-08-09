using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

using Crosstales.RTVoice;
using CrazyMinnow.SALSA;
using System.Linq;
using System.Collections.Generic;

public class Main : MonoBehaviour
{
    // components of the model
    private enum VoiceModel { Woman, Man };
    private VoiceModel m_voiceModel;
    private Smile m_smileController; // smile
    private AudioSource m_audioSrc;
    private Animator m_animator;
    private ElevatorSpeechMonitor m_elevatorSpeechMonitor;
    private string m_speakingText; // speaking text
    private ThreadSafeQueue<PresentationUnitError> m_queuePresentationUnitErrors;

    // character model
    public enum CharacterModel { Woman0, Woman1, Woman2, Man0, Man1, Man2 };
    public GameObject woman0;
    public GameObject woman1;
    public GameObject woman2;
    public GameObject man0;
    public GameObject man1;
    public GameObject man2;
    private GameObject m_model;

    private void ChooseModel()
    {
        switch (m_characterModel)
        {
            case CharacterModel.Woman0:
                m_model = woman0;
                m_voiceModel = VoiceModel.Woman;
                break;
            case CharacterModel.Woman1:
                m_model = woman1;
                m_voiceModel = VoiceModel.Woman;
                break;
            case CharacterModel.Woman2:
                m_model = woman2;
                m_voiceModel = VoiceModel.Woman;
                break;
            case CharacterModel.Man0:
                m_model = man0;
                m_voiceModel = VoiceModel.Man;
                break;
            case CharacterModel.Man1:
                m_model = man1;
                m_voiceModel = VoiceModel.Man;
                break;
            case CharacterModel.Man2:
                m_model = man2;
                m_voiceModel = VoiceModel.Man;
                break;
        }
    }

    public CharacterModel m_characterModel = CharacterModel.Woman0;

    // objects in the scene
    public Transform m_mainCamera;

    // states
    private bool m_startWork = false;
    private bool m_userFinished = false;

    // GUI control
    private int m_toolbarIntFeedbackStrategy = 0;
    private string[] m_toolbarStringsFeedbackStrategy = { "immediate", "combination", "after" };

    // misc
    private float m_timer;
    private Logger m_logger;
    private System.Random rnd;
    private float m_timeElapse;
    private int m_log_counter = 0;
    private bool m_speakerMutedLock = false;

    private float m_rotationProgress = 0f;

    private Quaternion m_startRotation;
    private Quaternion m_endRotation;


    void Awake()
    {
        rnd = new System.Random();
        m_timer = 0f;

        m_mainCamera = GameObject.Find("Main Camera").transform;

        ChooseModel();
        m_smileController = m_model.GetComponentInChildren<Smile>();
        m_audioSrc = m_model.GetComponent<AudioSource>();
        m_animator = m_model.GetComponent<Animator>();
        m_elevatorSpeechMonitor = new ElevatorSpeechMonitor();
        m_model.GetComponent<Eyes>().lookTarget = m_mainCamera;
    }

    // Testing
    //[Obsolete]
    private void OnGUI()
    {
        // choose feedback strategy
        GUI.BeginGroup(new Rect(20, 20, 400, 70));
        GUI.Box(new Rect(0, 0, 400, 60), "Feedback Strategy");
        m_toolbarIntFeedbackStrategy = GUI.Toolbar(new Rect(0, 30, 400, 30), m_toolbarIntFeedbackStrategy, m_toolbarStringsFeedbackStrategy);
        switch (m_toolbarIntFeedbackStrategy)
        {
            case 0:
                m_elevatorSpeechMonitor.FeedbackStrategy = FeedbackStrategyType.IMMEDIATE;
                break;
            case 1:
                m_elevatorSpeechMonitor.FeedbackStrategy = FeedbackStrategyType.COMBINATION;
                break;
            case 2:
                m_elevatorSpeechMonitor.FeedbackStrategy = FeedbackStrategyType.AFTER_ACTION;
                break;
        }
        GUI.EndGroup();

        // internal information vis
        GUI.BeginGroup(new Rect(20, 120, 400, 500));
        GUI.Box(new Rect(0, 0, 400, 500), "Detected Presentation Errors");

        GUI.Box(new Rect(0, 30, 100, 25), "Name");
        GUI.Box(new Rect(100, 30, 100, 25), "Priority");
        GUI.Box(new Rect(200, 30, 100, 25), "Number");
        GUI.Box(new Rect(300, 30, 100, 25), "Time");

        int k = 0;
        GUIStyle myStyle = new GUIStyle(GUI.skin.textField);
        myStyle.alignment = TextAnchor.MiddleCenter;

        Hashtable hashtablePUEStatistics = m_elevatorSpeechMonitor.PublicLazyIteratePUErrorStatistics();
        foreach (var name in Config.VerbalPriorityMapping.Keys)
        {
            if (hashtablePUEStatistics != null)
            {
                if (hashtablePUEStatistics.ContainsKey(name))
                {
                    PresentationUnitErrorStatistics pueStatistics = (PresentationUnitErrorStatistics)hashtablePUEStatistics[name];
                    GUI.TextField(new Rect(0, 60 + k * 25, 100, 25), pueStatistics.name, myStyle);
                    GUI.TextField(new Rect(100, 60 + k * 25, 100, 25), Enum.GetName(typeof(PriorityType), pueStatistics.priority), myStyle);
                    GUI.TextField(new Rect(200, 60 + k * 25, 100, 25), pueStatistics.count.ToString(), myStyle);
                    GUI.TextField(new Rect(300, 60 + k * 25, 100, 25), pueStatistics.elapse.ToString(), myStyle);
                }
                else
                {
                    GUI.TextField(new Rect(0, 60 + k * 25, 100, 25), name, myStyle);
                    GUI.TextField(new Rect(100, 60 + k * 25, 100, 25), "null", myStyle);
                    GUI.TextField(new Rect(200, 60 + k * 25, 100, 25), "null", myStyle);
                    GUI.TextField(new Rect(300, 60 + k * 25, 100, 25), "null", myStyle);
                }
                ++k;
            }
            else
            {
                GUI.TextField(new Rect(0, 60 + k * 25, 100, 25), name, myStyle);
                GUI.TextField(new Rect(100, 60 + k * 25, 100, 25), "null", myStyle);
                GUI.TextField(new Rect(200, 60 + k * 25, 100, 25), "null", myStyle);
                GUI.TextField(new Rect(300, 60 + k * 25, 100, 25), "null", myStyle);
                ++k;
            }
        }
        GUI.EndGroup();

        // start and end
        GUI.BeginGroup(new Rect(Screen.width - 320, 20, 300, 125));
        GUI.Box(new Rect(0, 0, 300, 125), "Switch");

        if (m_startWork == true)
        {
            if (m_userFinished == true)
            {
                m_timer = 0f;
                GUI.Box(new Rect(0, 30, 300, 30), (0.0f).ToString("0"));
            }
            else
            {
                GUI.Box(new Rect(0, 30, 300, 30), m_timer.ToString("0"));
            }
        }
        else
            GUI.Box(new Rect(0, 30, 300, 30), (0.0f).ToString("0"));

        if (GUI.Button(new Rect(0, 60, 300, 30), "Start"))
        {
            m_logger = new Logger("D://its_log_" + m_log_counter + ".txt");
            m_logger.Log("start logging");
            ++m_log_counter;

            m_elevatorSpeechMonitor.CleanState();

            this.StartAnimation();
        }

        if (GUI.Button(new Rect(0, 90, 300, 30), "Finish"))
        {
            m_timeElapse = Time.time - m_timeElapse;

            //m_startWork = false;
            m_userFinished = true;

            for (int i = 0; i < m_queuePresentationUnitErrors.Count; ++i)
            {
                PresentationUnitError pue = m_queuePresentationUnitErrors.Dequeue();
                m_elevatorSpeechMonitor.Add(pue);
            }

            m_logger.Log("end logging");
            m_logger.Close();
        }
        GUI.EndGroup();

        // behaviors
        int immediateActionNum = m_elevatorSpeechMonitor.immediateVerbalMapping.Keys.Count;
        GUI.BeginGroup(new Rect(Screen.width - 320, 160, 300, immediateActionNum * 40 + 20));
        GUI.Box(new Rect(0, 0, 300, immediateActionNum * 30 + 30), "Feedback Behaviors");
        for (int i = 0; i < immediateActionNum; ++i)
        {
            string val = m_elevatorSpeechMonitor.immediateVerbalMapping.ElementAt(i).Value;

            string label = val + "  [" + m_elevatorSpeechMonitor.immediateVerbalMapping.ElementAt(i).Key + "]";

            if (GUI.Button(new Rect(0, 30 + 30 * i, 300, 30), label))
            {
                // log
                string errorName = m_elevatorSpeechMonitor.immediateVerbalMapping.ElementAt(i).Key;
                PriorityType erroPriority = Config.VerbalPriorityMapping[errorName];
                m_logger.Log(errorName);

                if ((m_elevatorSpeechMonitor.FeedbackStrategy == FeedbackStrategyType.IMMEDIATE) || (m_elevatorSpeechMonitor.FeedbackStrategy == FeedbackStrategyType.COMBINATION && erroPriority == PriorityType.HIGH))
                {
                    if (m_speakerMutedLock == false)
                    {
                        this.AttentionAnimation();
                        this.TextToSpeech(val);
                    }
                }
                else
                {
                    PresentationUnitError pu = new PresentationUnitError(errorName, erroPriority);
                    m_queuePresentationUnitErrors.Enqueue(pu);
                }
            }
        }
        GUI.EndGroup();


        int listeningActionNum = m_elevatorSpeechMonitor.listeningVerbalMapping.Keys.Count;
        GUI.BeginGroup(new Rect(Screen.width - 320, 420, 300, listeningActionNum * 40 + 20 + 10));
        GUI.Box(new Rect(0, 0, 300, listeningActionNum * 40 + 20 + 50), "Listening Behaviors");

        if (GUI.Button(new Rect(0, 30, 300, 30), "Normal"))
        {
            this.ListeningAnimation();
            m_smileController.CloseSmile();
        }
        if (GUI.Button(new Rect(0, 60, 300, 30), "Smile"))
        {
            this.ListeningAnimation();
            m_smileController.OpenSmile();
        }
        for (int i = 0; i < listeningActionNum; ++i)
        {
            string val = m_elevatorSpeechMonitor.listeningVerbalMapping.ElementAt(i).Value;
            if (GUI.Button(new Rect(0, 90 + 30 * i, 300, 30), val))
            {
                if (m_speakerMutedLock == false)
                {
                    this.ListeningAnimation();
                    this.TextToSpeech(val);
                }
            }
        }

        GUI.EndGroup();

        if (GUI.Button(new Rect(Screen.width - 320, Screen.height - 30, 300, 30), "Log Fail"))
        {
            m_logger.Log("fail logging");
        }
    }

    private void Update()
    {
        if (m_startWork == true)
        {
            if (m_rotationProgress < 1 && m_rotationProgress >= 0)
            {
                m_rotationProgress += Time.deltaTime * 5;
                m_model.transform.rotation = Quaternion.Lerp(m_startRotation, m_endRotation, m_rotationProgress);
            }
        }
    }

    IEnumerator TimerCoroutine()
    {
        while (true)
        {
            if (m_startWork == true)
            {
                m_timer += 1.0f;
                yield return new WaitForSeconds(1.0f);
            }
            else
            {
                yield break;
            }
        }
    }

    // Coroutine 
    IEnumerator UpdateMainCoroutine()
    {
        while (true)
        {
            Debug.Log("The number of presentation unit error: " + m_elevatorSpeechMonitor.GetCount());

            if (m_startWork == true)
            {
                if (m_userFinished == false) // immediate + combination
                {
                    if (m_queuePresentationUnitErrors.Count > 0)
                    {
                        PresentationUnitError pu = m_queuePresentationUnitErrors.Dequeue();
                        m_elevatorSpeechMonitor.Add(pu);
                    }

                    if (m_elevatorSpeechMonitor.FeedbackStrategy != FeedbackStrategyType.AFTER_ACTION)
                    {
                        if (m_elevatorSpeechMonitor.GetCount() <= 0) yield return new WaitForSeconds(Config.ReactionTimeStep);

                        if (m_speakerMutedLock == false)
                        {
                            BehaviorDNA behaviorDNA = m_elevatorSpeechMonitor.BehaviorGeneratorImmediate();

                            if (behaviorDNA.surfaceText != null)
                            {
                                this.TextToSpeech(behaviorDNA.surfaceText);
                            }
                        }
                    }
                }
                else // summarize
                {
                    if (m_elevatorSpeechMonitor.FeedbackStrategy != FeedbackStrategyType.IMMEDIATE)
                    {
                        if (m_speakerMutedLock == false)
                        {
                            BehaviorDNA behaviorDNA = m_elevatorSpeechMonitor.BehaviorGeneratorSummarize();

                            if (m_timeElapse > Config.ExpectedElevatorSpeechTime)
                            {
                                behaviorDNA.surfaceText += " <break time='0.5s'/> Also, you use too long time. You should finish the speech in about " + Config.ExpectedElevatorSpeechTime.ToString() + " seconds";
                            }
                            else if (m_timeElapse < Config.ExpectedElevatorSpeechTime - 50)
                            {
                                behaviorDNA.surfaceText += " <break time='0.5s'/> Also, you speak too fast. You should organize the speech in about " + Config.ExpectedElevatorSpeechTime.ToString() + " seconds";
                            }

                            behaviorDNA.surfaceText += "<break time='1s'/> OK. That is all. <break time='0.5s'/> Good luck.";

                            this.TalkingAnimation();
                            this.TextToSpeech(behaviorDNA.surfaceText);

                            m_startWork = false;
                        }

                    }
                    else
                    {
                        if (m_speakerMutedLock == false)
                        {
                            string surfaceText = "Well. <break time='1s'/> I have some otherthings to do now. <break time='0.5s'/> Then that's all for today? Bye bye.";

                            //this.TalkingAnimation();
                            this.TextToSpeech(surfaceText);

                            m_startWork = false;
                        }
                    }
                    m_startWork = false;
                }
                yield return new WaitForSeconds(Config.ReactionTimeStep);
            }
            else
            {
                yield break;
            }
        }
    }

    // handle messages
    void NoHandGesturePresentationUnitError(PresentationUnitError pu)
    {
        if (m_startWork)
        {
            m_queuePresentationUnitErrors.Enqueue(pu);
            m_logger.Log("nohandgesture");
        }
    }

    void NoEyeContactPresentationUnitError(PresentationUnitError pu)
    {
        if (m_startWork)
        {
            m_queuePresentationUnitErrors.Enqueue(pu);
            m_logger.Log("noeyecontact");
        }
    }


    // rhythm
    void NoSpeechPresentationUnitError(PresentationUnitError pu)
    {
        if (m_startWork)
        {
            m_queuePresentationUnitErrors.Enqueue(pu);
            m_logger.Log("nospeech");
        }
    }

    void TooFastPresentationUnitError(PresentationUnitError pu)
    {
        if (m_startWork)
        {
            m_queuePresentationUnitErrors.Enqueue(pu);
            m_logger.Log("toofast");
        }
    }

    void TooSlowPresentationUnitError(PresentationUnitError pu)
    {
        if (m_startWork)
        {
            m_queuePresentationUnitErrors.Enqueue(pu);
            m_logger.Log("tooslow");
        }
    }

    // volume
    void TooLoudPresentationUnitError(PresentationUnitError pu)
    {
        if (m_startWork)
        {
            m_queuePresentationUnitErrors.Enqueue(pu);
            m_logger.Log("tooloud");
        }
    }

    void TooSilentPresentationUnitError(PresentationUnitError pu)
    {
        if (m_startWork)
        {
            m_queuePresentationUnitErrors.Enqueue(pu);
            m_logger.Log("toosilent");
        }
    }

    // Let us start
    void StartCharacter()
    {
        if (m_speakerMutedLock == false)
        {
            //m_model.transform.rotation = Quaternion.Euler(0, m_mainCamera.transform.eulerAngles.y, 0);

            m_startRotation = m_model.transform.rotation;
            m_endRotation = Quaternion.Euler(0, m_mainCamera.rotation.eulerAngles.y+180, 0);


            if (this.m_voiceModel == VoiceModel.Woman)
            {
                m_speakingText = "Hello! <break time='0.5s'/> My name is Jane. I am your elevator speech coach. <break time='0.5s'/> You can start now.";
            }
            else
            {
                m_speakingText = "Hello! <break time='0.5s'/> My name is David. I am your elevator speech coach. <break time='0.5s'/> You can start now.";
            }

            this.TextToSpeech(m_speakingText);

            m_queuePresentationUnitErrors = new ThreadSafeQueue<PresentationUnitError>();

            m_startWork = true;
            m_userFinished = false;

            m_timeElapse = Time.time;

            GetComponent<SpeechRecognizer>().enabled = true;

            StartCoroutine("TimerCoroutine");
            StartCoroutine("UpdateMainCoroutine");
        }
    }

    private void ResetTriggers()
    {
        m_animator.ResetTrigger("Talking");
        m_animator.ResetTrigger("Listening");
        m_animator.ResetTrigger("Start");
        m_animator.ResetTrigger("Attention1");
        m_animator.ResetTrigger("Attention2");
    }

    void StartAnimation()
    {
        this.ResetTriggers();
        m_animator.SetTrigger("Start");
    }

    void TalkingAnimation()
    {
        this.ResetTriggers();
        m_animator.SetTrigger("Talking");
    }

    void ListeningAnimation()
    {
        this.ResetTriggers();
        m_animator.SetTrigger("Listening");
    }

    void AttentionAnimation()
    {
        this.ResetTriggers();
        int dice = rnd.Next(2)+1;
        m_animator.SetTrigger("Attention" + dice);
    }

    // dirty jobs
    private void TextToSpeech(string text)
    {
        if (this.m_voiceModel == VoiceModel.Woman)
        {
            Speaker.Speak(text, m_audioSrc, Speaker.VoiceForName("Microsoft Zira Desktop"));
        }
        else
        {
            Speaker.Speak(text, m_audioSrc, Speaker.VoiceForName("Microsoft David Desktop"));
        }
    }

    private void speakStartMethod(Crosstales.RTVoice.Model.Wrapper wrapper)
    {
        m_speakerMutedLock = true;
    }
    private void speakCompleteMethod(Crosstales.RTVoice.Model.Wrapper wrapper)
    {
        m_speakerMutedLock = false;
    }

    private void OnEnable()
    {
        // Subscribe event listeners
        Speaker.OnSpeakStart += speakStartMethod;
        Speaker.OnSpeakComplete += speakCompleteMethod;
    }

    private void OnDisable()
    {
        // Unsubscribe event listeners
        Speaker.OnSpeakStart -= speakStartMethod;
        Speaker.OnSpeakComplete -= speakCompleteMethod;
    }
}
