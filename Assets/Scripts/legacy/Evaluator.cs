//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;

//public class Evaluator : MonoBehaviour
//{
//    private static GameObject messageAddress;
//    private static Dictionary<string, int> vocabulary = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(Config.EVALUATE_CONTENT_CORPUS_JSON_LOCATION, System.Text.Encoding.UTF8));

    

//    public static void sendMessage(String tag, object message)
//    {
//        if(messageAddress==null)
//            messageAddress = GameObject.Find(Config.TAG_EVALUATOR);
//        messageAddress.SendMessage(tag, message);
//    }

//    private static void print(object message)
//    {
//        if (!Config.SILENCE_MODE) MonoBehaviour.print(message);
//    }

//    // Check if the word of text are in the corpus
//    public static void EvaluateLexicalRelevance(string text)
//    {
//        foreach (var word in text.Split(' '))
//        {
//            if (vocabulary.ContainsKey(word))
//            {
//                print(string.Format("{0} is lexicial relevant.", word));
//                sendMessage(Config.TAG_CONTENT_LEXICAL, new PresentationUnitError(Config.TAG_CONTENT_LEXICAL, PriorityType.HIGH));
//            }
//        }
//        print("{0} is not inrelevant.");
//    }




//    public static void EvaluateVolume(string db)
//    {
//        if (db != null)
//        {
//            print(db);
//            var volume = Convert.ToDouble(db);
//            if (volume > Config.THRESHOLD_RHYTHM_MAX_VOLUME)
//                sendMessage(Config.TAG_RHYTHM_VOLUME, new PresentationUnitError(Config.TAG_RHYTHM_VOLUME, PriorityType.HIGH));
//            else if (volume < Config.THRESHOLD_RHYTHM_MIN_VOLUME)
//                sendMessage(Config.TAG_RHYTHM_VOLUME, new PresentationUnitError(Config.TAG_RHYTHM_VOLUME, PriorityType.LOW));
//        }
//    }
//}
