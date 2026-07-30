using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

public static class AnimationUtils
{
    public static IEnumerator OnAnimationFinish(Animator animator, string stateName, Action onFinish = null, int layer = -1, Action<float> onUpdate = null, Action onRestart = null)
    {
        AnimatorStateInfo prevStateInfo = animator.GetCurrentAnimatorStateInfo(layer >= 0 ? layer : 0);
        // if(prevStateInfo.IsName(stateName) && prevStateInfo.normalizedTime < 1.0f) onRestart?.Invoke();
        if(prevStateInfo.normalizedTime < 1.0f) onRestart?.Invoke();

        animator.Play(stateName, layer, 0f);
        if(onFinish == null)
            yield break;
        
        yield return null;
        // 3. Keep looping as long as we are in the state and it hasn't reached 1.0 (100% completion)
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layer >= 0 ? layer : 0);
        while (stateInfo.IsName(stateName) && stateInfo.normalizedTime < 1.0f)
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(layer >= 0 ? layer : 0);
            onUpdate?.Invoke(stateInfo.normalizedTime);
            yield return null;
        }

        // 4. Animation is finished! Run your code here
        onFinish?.Invoke();
    }

    public static List<AnimationClip> GetClipsFromLayer(Animator anim, int layerIndex)
    {
        List<AnimationClip> clips = new List<AnimationClip>();

        #if UNITY_EDITOR
        if (anim == null || anim.runtimeAnimatorController == null) return clips;

        // Cast runtime controller to the Editor AnimatorController
        AnimatorController ac = anim.runtimeAnimatorController as AnimatorController;
        if (ac == null || layerIndex >= ac.layers.Length) return clips;

        // Get the state machine for the target layer
        AnimatorStateMachine stateMachine = ac.layers[layerIndex].stateMachine;
        
        // Loop through all states in this specific state machine
        foreach (ChildAnimatorState childState in stateMachine.states)
        {
            Motion motion = childState.state.motion;
            
            if (motion is AnimationClip clip)
            {
                if (!clips.Contains(clip)) clips.Add(clip);
            }
            else if (motion is BlendTree blendTree)
            {
                // Extract clips inside blend trees if they exist
                ExtractClipsFromBlendTree(blendTree, clips);
            }
        }
        #else
        Debug.LogWarning("Cannot extract all layer clips at runtime. Use Editor mode.");
        #endif

        return clips;
    }

    #if UNITY_EDITOR
    private static void ExtractClipsFromBlendTree(BlendTree blendTree, List<AnimationClip> clips)
    {
        foreach (ChildMotion childMotion in blendTree.children)
        {
            if (childMotion.motion is AnimationClip clip)
            {
                if (!clips.Contains(clip)) clips.Add(clip);
            }
            else if (childMotion.motion is BlendTree subBlendTree)
            {
                ExtractClipsFromBlendTree(subBlendTree, clips);
            }
        }
    }
    #endif
}