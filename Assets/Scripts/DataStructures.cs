using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public enum FeedbackStrategyType { IMMEDIATE, AFTER_ACTION, COMBINATION };
public enum PriorityType { LOW, MIDDLE, HIGH };
public enum StageType { Opening, Body, Closing};

struct BehaviorDNA
{
    public string surfaceText;
    public string animationDescription;

    public BehaviorDNA(string _surfaceText, string _animationDescription)
    {
        surfaceText = _surfaceText;
        animationDescription = _animationDescription;
    }
}

struct NamePriority
{
    public string name;
    public SortedList list;

    public NamePriority(string _name, SortedList _list)
    {
        name = _name;
        list = _list;
    }
}

struct PresentationUnitError
{
    public string name;
    public PriorityType priority;
    //public DateTime timeStamp;
    public float timeStamp;

    // TODO: intensity

    public PresentationUnitError(string _name, PriorityType _priority)
    {
        name = _name;
        priority = _priority;
        timeStamp = Time.time;
    }
}

class PresentationUnitErrorStatistics
{
    public string name;
    public int count;
    public float elapse;
    public PriorityType priority;

    public PresentationUnitErrorStatistics() { }
}

public class ThreadSafeQueue<T>
{
    private readonly Queue<T> _queue = new Queue<T>();

    public int Count{get {return _queue.Count;}}

    public void Enqueue(T item)
    {
        lock (_queue)
        {
            _queue.Enqueue(item);
            if (_queue.Count == 1)
                Monitor.PulseAll(_queue);
        }
    }

    public T Dequeue()
    {
        lock (_queue)
        {
            while (_queue.Count == 0)
                Monitor.Wait(_queue);

            return _queue.Dequeue();
        }
    }
}

//class PresentationUnitErrorHashList
//{
//    private Hashtable keyList;
//    private Queue<PresentationUnitError> puq;


//    public int Count
//    {
//        get
//        {
//            int res = 0;
//            foreach(var key in keyList.Keys)
//            {
//                res += ((SortedList)keyList[key]).Count;
//            }
//            return res;
//        }
//    }

//    // constructor
//    public PresentationUnitErrorHashList()
//    {
//        keyList = new Hashtable();
//        puq = new Queue<PresentationUnitError>();
//    }

//    public void Add(PresentationUnitError pu)
//    {
//        if (keyList.ContainsKey(pu.name))
//        {
//            SortedList list = (SortedList)keyList[pu.name];
//            list.Add(pu.timeStamp, pu);
//        }
//        else
//        {
//            SortedList list = new SortedList();
//            list.Add(pu.timeStamp, pu);
//            keyList.Add(pu.name, list);
//        }
//    }

//    public void Remove(PresentationUnitError pu)
//    {
//        if (keyList.ContainsKey(pu.name))
//        {
//            SortedList list = (SortedList)keyList[pu.name];
//            list.Remove(pu.timeStamp);

//            if (list.Count == 0)
//            {
//                keyList.Remove(pu.name);
//            }
//        }
//    }

//    public IEnumerable<string> TraverseNames()
//    {
//        foreach(string name in keyList.Keys)
//        {
//            yield return name;
//        }
//    }

//    public IEnumerable<PresentationUnitError> TraversePUs(string _name)
//    {
//        SortedList list = (SortedList)keyList[_name];
//        foreach (float timeStamp in list.Keys)
//        {
//            yield return (PresentationUnitError)list[timeStamp];
//        }
//    }


//    public PresentationUnitError Pop()
//    {
//        if (puq.Count == 0)
//        {
//            List<PresentationUnitError> tmpl = new List<PresentationUnitError>();
//            foreach (var name in TraverseNames())
//            {
//                foreach (var pu in TraversePUs(name))
//                {
//                    puq.Enqueue(pu);
//                    tmpl.Add(pu);
//                }
//            }

//            foreach(var pu in tmpl)
//            {
//                keyList.Remove(pu.name);
//            }

//            if (puq.Count == 0) return null;
//        }
//        return puq.Dequeue();
//    }
//}

//class PresentationUnitErrorDatabase
//{
//    public PresentationUnitErrorHashList puhlHigh;
//    public PresentationUnitErrorHashList puhlMiddle;
//    public PresentationUnitErrorHashList puhlLow;

//    public PresentationUnitErrorDatabase()
//    {
//        puhlHigh = new PresentationUnitErrorHashList();
//        puhlMiddle = new PresentationUnitErrorHashList();
//        puhlLow = new PresentationUnitErrorHashList();
//    }

//    //public void Add(PresentationUnitError pu)
//    //{
//    //    switch (pu.priority)
//    //    {
//    //        case PresentationUnitError.Priority.HIGH:
//    //            pudbHigh.Add(pu);
//    //            break;
//    //        case PresentationUnitError.Priority.MIDDLE:
//    //            pudbMiddle.Add(pu);
//    //            break;
//    //        case PresentationUnitError.Priority.LOW:
//    //            pudbLow.Add(pu);
//    //            break;
//    //    }
//    //}

//    //public void Remove(PresentationUnitError pu)
//    //{
//    //    switch (pu.priority)
//    //    {
//    //        case PresentationUnitError.Priority.HIGH:
//    //            pudbHigh.Remove(pu);
//    //            break;
//    //        case PresentationUnitError.Priority.MIDDLE:
//    //            pudbMiddle.Remove(pu);
//    //            break;
//    //        case PresentationUnitError.Priority.LOW:
//    //            pudbLow.Remove(pu);
//    //            break;
//    //    }
//    //}
//}