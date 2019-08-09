using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class ElevatorSpeechMonitor
{
    private StageType stage;
    private FeedbackStrategyType feedbackStrategy;
    private System.Random rnd;

    public Hashtable hashTableNamePUEList;
    public Dictionary<string, string> immediateVerbalMapping;
    public Dictionary<string, string> listeningVerbalMapping;
    public NamePriority[] lazyNameListInPriorityOrder = null;

    // property jobs
    public StageType Stage
    {
        get { return stage; }
        set { stage = value; }
    }
    public FeedbackStrategyType FeedbackStrategy
    {
        get { return feedbackStrategy; }
        set { feedbackStrategy = value; }
    }

    public ElevatorSpeechMonitor()
    {
        stage = StageType.Opening;
        feedbackStrategy = FeedbackStrategyType.IMMEDIATE;

        hashTableNamePUEList = new Hashtable();

        rnd = new System.Random();

        // immediate verbal mapping
        immediateVerbalMapping = new Dictionary<string, string>();
        immediateVerbalMapping.Add("noeyecontact", "Look at me!");
        immediateVerbalMapping.Add("nohandgesture", "Hand gesture!");
        immediateVerbalMapping.Add("nospeech", "Come on!");
        immediateVerbalMapping.Add("toofast", "Slow down!");
        immediateVerbalMapping.Add("tooslow", "Faster!");
        immediateVerbalMapping.Add("tooloud", "Keep your voice!");
        immediateVerbalMapping.Add("toosilent", "Louder!");

        // listening verbal mapping
        listeningVerbalMapping = new Dictionary<string, string>();
        listeningVerbalMapping.Add("2", "Cool!");
        listeningVerbalMapping.Add("1", "I see.");
        listeningVerbalMapping.Add("4", "Uhm...");
        //listeningVerbalMapping.Add("5", "Well...");
        listeningVerbalMapping.Add("7", "OK...");
        listeningVerbalMapping.Add("3", "Yes.");
        listeningVerbalMapping.Add("6", "Really?");
    }

    public BehaviorDNA BehaviorGeneratorImmediate()
    {
        BehaviorDNA res = new BehaviorDNA(null, null);

        PresentationUnitErrorStatistics pueStatistics = GetHighestPUErrorStatistics();
        if (pueStatistics != null)
        {
            // IMPORTANT! Current policy!
            if (pueStatistics.count > Config.PUESTATISTICS_COUNT && pueStatistics.elapse > Config.PUESTATISTICS_ELAPSE)
            {
                if (feedbackStrategy == FeedbackStrategyType.COMBINATION && pueStatistics.priority < PriorityType.HIGH)
                {
                    return res;
                }
                else
                {
                    res.surfaceText = this.PUE2SurfaceTextMappingImmediate(pueStatistics);
                    res.animationDescription = "neutral";

                    hashTableNamePUEList.Remove(pueStatistics.name);

                    return res;
                }
            }
            else return res;
        }

        return res;
    }

    public BehaviorDNA BehaviorGeneratorSummarize()
    {
        BehaviorDNA res = new BehaviorDNA("", "");
        res.animationDescription = "neutral";

        if (this.GetCount() == 0)
        {
            res.surfaceText += "Well. <break time='0.2s'/> Thanks for participanting the experiment. You are good! But,  <break time='0.5s'/>";
            return res;
        }
        else
        {
            res.surfaceText += "Well.<break time='0.2s'/> Thanks for participanting the experiment. There are some problems of your performance. In particular, ";
        }

        foreach (PresentationUnitErrorStatistics pueStatistics in this.IteratePUErrorStatistics())
        {
            if (pueStatistics != null)
            {
                string surfaceText = this.PUE2SurfaceTextMappingSummarize(pueStatistics);

                res.surfaceText += surfaceText;

                //hashTableNamePUEList.Remove(pueStatistics.name);
            }
        }


        return res;
    }

    // iterate current presentation unit error statistics
    public Hashtable PublicLazyIteratePUErrorStatistics()
    {
        Hashtable res = new Hashtable();

        int N = hashTableNamePUEList.Count;

        if (N == 0) return null;
        //if (lazyNameListInPriorityOrder == null)
        //{
        //    lazyNameListInPriorityOrder = RankPresentationErrorByPriorityAndTime();
        //}
        lazyNameListInPriorityOrder = RankPresentationErrorByPriorityAndTime();

        if (lazyNameListInPriorityOrder.Length > 0)
        {
            foreach (var namePriority in lazyNameListInPriorityOrder)
            {
                string name = ((NamePriority)namePriority).name;

                //Debug.Log(name);

                if (hashTableNamePUEList.ContainsKey(name))
                {
                    SortedList list = (SortedList)hashTableNamePUEList[name];

                    // do statistics
                    PresentationUnitErrorStatistics pueStatistics = new PresentationUnitErrorStatistics();
                    pueStatistics.name = namePriority.name;
                    pueStatistics.priority = ((PresentationUnitError)list.GetByIndex(0)).priority;
                    pueStatistics.elapse = ((PresentationUnitError)list.GetByIndex(list.Count - 1)).timeStamp - ((PresentationUnitError)list.GetByIndex(0)).timeStamp;
                    pueStatistics.count = list.Count;

                    res.Add(name, pueStatistics);
                }
            }
        }

        return res;
    }

    public void CleanState()
    {
        hashTableNamePUEList = new Hashtable();
        lazyNameListInPriorityOrder = null;
    }

    public int GetCount()
    {
        return hashTableNamePUEList.Keys.Count;
    }

    public void Add(PresentationUnitError pu)
    {
        if (hashTableNamePUEList.ContainsKey(pu.name))
        {
            SortedList list = (SortedList)hashTableNamePUEList[pu.name];
            list.Add(pu.timeStamp, pu);
        }
        else
        {
            SortedList list = new SortedList();
            list.Add(pu.timeStamp, pu);
            hashTableNamePUEList.Add(pu.name, list);
        }

        //lazyNameListInPriorityOrder = RankPresentationErrorByPriorityAndTime();
    }

    private string PUE2SurfaceTextMappingImmediate(PresentationUnitErrorStatistics pueStatistics)
    {
        if(this.immediateVerbalMapping.ContainsKey(pueStatistics.name)){
            return this.immediateVerbalMapping[pueStatistics.name];
        }
        else
        {
            return null;
        }
    }

    private string PUE2SurfaceTextMappingSummarize(PresentationUnitErrorStatistics pueStatistics)
    {
        switch (pueStatistics.name)
        {
            case "noeyecontact":
                return " There are some problems of your eye contact. You should look at me more frequently.";
            case "nohandgesture":
                return " There are some problems of your hand gesture. You should use your hand gesture more frequently.";
            case "nospeech":
                return " You did say anything! Please do not be shy!";
            case "toofast":
                return " There are some problems of your rhythm. Sometimes you spoke too fast.";
            case "tooslow":
                return " There are some problems of your rhythm. Sometimes you spoke too slow.";
            case "tooloud":
                return " There are some problems of your volume. Sometimes you spoke too loud.";
            case "toosilent":
                return " There are some problems of your volume. Sometimes you spoke too silent.";
            default:
                return null;
        }
    }

    private void Remove(string name)
    {
        if (hashTableNamePUEList.ContainsKey(name))
        {
            hashTableNamePUEList.Remove(name);
        }
    }

    private bool SortedListLeftHigerThanRight(SortedList lhs, SortedList rhs)
    {
        float lhsTime = ((PresentationUnitError)lhs.GetByIndex(lhs.Count - 1)).timeStamp;
        float rhsTime = ((PresentationUnitError)rhs.GetByIndex(rhs.Count - 1)).timeStamp;
        PriorityType lhsPriority = ((PresentationUnitError)lhs.GetByIndex(0)).priority;
        PriorityType rhsPriority = ((PresentationUnitError)rhs.GetByIndex(0)).priority;

        if (lhs.Count > rhs.Count && lhsTime > rhsTime && lhsPriority > rhsPriority) return true;
        else
        {
            if (lhs.Count > rhs.Count && lhsTime > rhsTime && lhsPriority == rhsPriority) return true;
            else
            {
                int dice = rnd.Next(10);
                if (dice >= 5) return true;
                else return false;
            }
        }
    }

    private NamePriority[] RankPresentationErrorByPriorityAndTime()
    {
        int cursor = 0;
        int N = hashTableNamePUEList.Count;

        NamePriority[] nameListInPriorityOrder = new NamePriority[N];

        // insert sort
        foreach (var name in hashTableNamePUEList.Keys)
        {
            SortedList list = hashTableNamePUEList[name] as SortedList;
            PresentationUnitError one = (PresentationUnitError)list.GetByIndex(0);

            if (cursor == 0)
            {
                nameListInPriorityOrder[0] = new NamePriority(one.name, list);
                ++cursor;
                continue;
            }

            int j = cursor - 1;
            for (; j >= 0 && !SortedListLeftHigerThanRight(nameListInPriorityOrder[j].list, list);)
            {
                nameListInPriorityOrder[j + 1] = nameListInPriorityOrder[j];
                --j;
            }
            nameListInPriorityOrder[j + 1] = new NamePriority(one.name, list);

            ++cursor;
        }

        return nameListInPriorityOrder;
    }

    private PresentationUnitErrorStatistics GetHighestPUErrorStatistics()
    {
        int N = hashTableNamePUEList.Count;

        if (N == 0) return null;

        NamePriority[] nameListInPriorityOrder = RankPresentationErrorByPriorityAndTime();
        
        {
            NamePriority namePriority = nameListInPriorityOrder[0];
            string name = ((NamePriority)namePriority).name;
            SortedList list = (SortedList)hashTableNamePUEList[name];

            // do statistics
            PresentationUnitErrorStatistics pueStatistics = new PresentationUnitErrorStatistics();
            pueStatistics.name = namePriority.name;
            pueStatistics.priority = ((PresentationUnitError)list.GetByIndex(0)).priority;
            pueStatistics.elapse = ((PresentationUnitError)list.GetByIndex(list.Count - 1)).timeStamp - ((PresentationUnitError)list.GetByIndex(0)).timeStamp;
            pueStatistics.count = list.Count;

            return pueStatistics;
        }
    }

    // iterate current presentation unit error statistics
    private IEnumerable<PresentationUnitErrorStatistics> IteratePUErrorStatistics()
    {
        int N = hashTableNamePUEList.Count;

        if (N == 0) yield return null;

        NamePriority[] nameListInPriorityOrder = RankPresentationErrorByPriorityAndTime();
        //lazyNameListInPriorityOrder = nameListInPriorityOrder;

        foreach (var namePriority in nameListInPriorityOrder)
        {
            string name = ((NamePriority)namePriority).name;
            if (hashTableNamePUEList.ContainsKey(name))
            {
                SortedList list = (SortedList)hashTableNamePUEList[name];

                // do statistics
                PresentationUnitErrorStatistics pueStatistics = new PresentationUnitErrorStatistics();
                pueStatistics.name = namePriority.name;
                pueStatistics.priority = ((PresentationUnitError)list.GetByIndex(0)).priority;
                pueStatistics.elapse = ((PresentationUnitError)list.GetByIndex(list.Count - 1)).timeStamp - ((PresentationUnitError)list.GetByIndex(0)).timeStamp;
                pueStatistics.count = list.Count;

                yield return pueStatistics;
            }
            else
            {
                yield return null;
            }
        }
    }
}
