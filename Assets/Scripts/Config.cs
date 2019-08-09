using System.Collections.Generic;

public static class Config
{
    // Print evaluation detail or not
    public static bool SILENCE_MODE = true;

    // mine
    public static float ReactionTimeStep = 3f;
    public static float ExpectedElevatorSpeechTime = 120f;
    public static Dictionary<string, PriorityType> VerbalPriorityMapping = new Dictionary<string, PriorityType>
    {
        { "noeyecontact", PriorityType.HIGH},
        { "nohandgesture", PriorityType.LOW},
        { "nospeech", PriorityType.HIGH},
        { "toofast", PriorityType.HIGH},
        { "tooslow", PriorityType.HIGH},
        { "tooloud", PriorityType.LOW},
        { "toosilent", PriorityType.LOW}
    };

    // Python stuff
    public static string PYTHON_ENGINE_LOCATION = @"C:\Anaconda3\pythonw.exe";
    public static string PYTHON_SCRIPT_SPEECH_RECOGNIZE = @"D:\zzj\ITS\Assets\Scripts\speech_recognition.py";
    public static string PYTHON_SCRIPT_VOLUME_EVALUATION = @"D:\zzj\ITS\Assets\Scripts\evaluate_rhythm.py";

    public static bool PYTHON_DEBUG_FLAG = true;

    // Content evaluation
    public static string EVALUATE_CONTENT_CORPUS_JSON_LOCATION = @"D:\zzj\ITS\Assets\Data\vocabulary.json";

    // Rhythm evaluation
    public static float THRESHOLD_RHYTHM_MIN_VOLUME = 0.0001f;//23f;
    public static float THRESHOLD_RHYTHM_MAX_VOLUME = 1f;//75f;
    public static float THRESHOLD_RHYTHM_MIN_SPEED = 0.5f;
    public static float THRESHOLD_RHYTHM_MAX_SPEED = 5f;

    // Motion evaluation
    public static int THRESHOLD_MOTION_COUNT_PER_EVALUATION = 1;

    // Tag
    public static string TAG_EVALUATOR = "RTVoice";
    public static string TAG_CONTENT_LEXICAL = "[Content-Lexical]";

    public static string TAG_RHYTHM_SPEED = "[Rhythm-Speed]";
    public static string TAG_RHYTHM_VOLUME = "[Rhythm-Volume]";

    public static string TAG_MOTION_UPPERLIMB = "[Motion-UpperLimb]";

    // Time duration (s)
    public static float DURATION_BINDER = 0.05f;
    public static float DURATION_SPEED = 1.50f;
    public static float DURATION_MOTION_DETECTION = 1f;
    public static float DURATION_MOTION_EVALUATETION = 2f;

    // presention unit error statistics tolerance
    public static int PUESTATISTICS_COUNT = 3;
    public static float PUESTATISTICS_ELAPSE = 2f;
}
