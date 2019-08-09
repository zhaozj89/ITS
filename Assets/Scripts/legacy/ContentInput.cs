//using UnityEngine;
//using System.Collections;
//using System.Diagnostics;

//public class ContentInput : MonoBehaviour
//{
//    /* Content evaluation, includings:
//     * 1. Lexicial relevance. (passive listening, return bonus count)
//     * 2. TODO: Grammatical accuracy
//     */
//    public SpeechRecognizer speechRecognizer;

//    void StartContentInput()
//    {
//        StartCoroutine("BindspeechRecognizerCallback");
//    }

//    IEnumerator BindspeechRecognizerCallback()  // passive listening
//    {
//        while (true)
//        {
//            speechRecognizer = gameObject.GetComponent<SpeechRecognizer>();
//            if (speechRecognizer != null && speechRecognizer.py != null) 
//            {
//                speechRecognizer.py.addPassiveListener(EvaluateContent);
//                yield break;
//            }
//            yield return new WaitForSeconds(Config.DURATION_BINDER);
//        }
//    }

//    private void EvaluateContent(object sender, DataReceivedEventArgs e)
//    {
//        Evaluator.EvaluateLexicalRelevance(e.Data);
//    }
//}
