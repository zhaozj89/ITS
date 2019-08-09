using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayQueuePlayable : PlayableBehaviour
{
    private int m_CurrentClipIndex = -1;

    private float m_TimeToNextClip;

    private Playable mixer;

    public void Initialize(AnimationClip[] clipsToPlay, Playable owner, PlayableGraph graph)
    {
        owner.SetInputCount(1);

        mixer = AnimationMixerPlayable.Create(graph, clipsToPlay.Length);

        graph.Connect(mixer, 0, owner, 0);

        owner.SetInputWeight(0, 1);

        for (int clipIndex = 0; clipIndex < mixer.GetInputCount(); ++clipIndex)

        {

            graph.Connect(AnimationClipPlayable.Create(graph, clipsToPlay[clipIndex]), 0, mixer, clipIndex);

            mixer.SetInputWeight(clipIndex, 1.0f);

        }

    }

    override public void PrepareFrame(Playable owner, FrameData info)
    {

        if (mixer.GetInputCount() == 0)

            return;

        // Advance to next clip if necessary

        m_TimeToNextClip -= (float)info.deltaTime;

        if (m_TimeToNextClip <= 0.0f)

        {

            m_CurrentClipIndex++;

            if (m_CurrentClipIndex >= mixer.GetInputCount())

                m_CurrentClipIndex = 0;

            var currentClip = (AnimationClipPlayable)mixer.GetInput(m_CurrentClipIndex);

            // Reset the time so that the next clip starts at the correct position

            currentClip.SetTime(0);

            m_TimeToNextClip = currentClip.GetAnimationClip().length;

        }

        // Adjust the weight of the inputs

        for (int clipIndex = 0; clipIndex < mixer.GetInputCount(); ++clipIndex)

        {

            if (clipIndex == m_CurrentClipIndex)

                mixer.SetInputWeight(clipIndex, 1.0f);

            else

                mixer.SetInputWeight(clipIndex, 0.0f);

        }

    }

}

public static class ZAnimationPlayableUtilities
{
    public static ScriptPlayable<PlayQueuePlayable> PlayClipArray(Animator animator, AnimationClip[] clips_to_play, out PlayableGraph graph)
    {
        graph = PlayableGraph.Create();

        var play_queue_playable = ScriptPlayable<PlayQueuePlayable>.Create(graph);
        var play_queue = play_queue_playable.GetBehaviour();
        play_queue.Initialize(clips_to_play, play_queue_playable, graph);

        var playable_output = AnimationPlayableOutput.Create(graph, "Animation", animator);
        playable_output.SetSourcePlayable(play_queue_playable);

        playable_output.SetSourceOutputPort(0);

        graph.Play();

        return play_queue_playable;
    }
}


