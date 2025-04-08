using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Animation Clips")]
    public PlayerAnimationSet animationSet; // Назначаем клипы в инспекторе

    [Header("Animation State Names")]
    public AnimationStates animationStates; // Назначаем имена состояний в инспекторе

    [HideInInspector]
    public Animator animator; // Чтобы к ним можно было обращаться из других скриптов

    private AnimatorOverrideController overrideController;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator не найден ни на объекте, ни в его детях: " + gameObject.name);
            return;
        }
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("Animator не имеет контроллера анимаций!");
            return;
        }
        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;
        OverrideAnimations();
    }


    private void OverrideAnimations()
    {
        if (animationSet.Idle != null)
            overrideController[animationStates.Idle] = animationSet.Idle;
        if (animationSet.WalkForward != null)
            overrideController[animationStates.WalkForward] = animationSet.WalkForward;
        if (animationSet.WalkBackward != null)
            overrideController[animationStates.WalkBackward] = animationSet.WalkBackward;
        if (animationSet.WalkLeft != null)
            overrideController[animationStates.WalkLeft] = animationSet.WalkLeft;
        if (animationSet.WalkRight != null)
            overrideController[animationStates.WalkRight] = animationSet.WalkRight;
        if (animationSet.RunForward != null)
            overrideController[animationStates.RunForward] = animationSet.RunForward;
        if (animationSet.RunBackward != null)
            overrideController[animationStates.RunBackward] = animationSet.RunBackward;
        if (animationSet.RunLeft != null)
            overrideController[animationStates.RunLeft] = animationSet.RunLeft;
        if (animationSet.RunRight != null)
            overrideController[animationStates.RunRight] = animationSet.RunRight;
        if (animationSet.Jump != null)
            overrideController[animationStates.Jump] = animationSet.Jump;
        if (animationSet.Attack1 != null)
            overrideController[animationStates.Attack1] = animationSet.Attack1;
        if (animationSet.Attack2 != null)
            overrideController[animationStates.Attack2] = animationSet.Attack2;
        if (animationSet.Die != null)
            overrideController[animationStates.Die] = animationSet.Die;
    }

    // Метод для проигрывания нужной анимации по ключу (имени состояния)
    public void PlayAnimation(string animationKey)
    {
        animator.Play(animationKey);
    }

    // Дополнительные методы для атак и смерти
    public void PlayAttack1()
    {
        animator.Play(animationStates.Attack1);
    }

    public void PlayAttack2()
    {
        animator.Play(animationStates.Attack2);
    }

    public void PlayDie()
    {
        animator.Play(animationStates.Die);
    }
}

[System.Serializable]
public class PlayerAnimationSet
{
    public AnimationClip Idle;
    public AnimationClip WalkForward;
    public AnimationClip WalkBackward;
    public AnimationClip WalkLeft;
    public AnimationClip WalkRight;
    public AnimationClip RunForward;
    public AnimationClip RunBackward;
    public AnimationClip RunLeft;
    public AnimationClip RunRight;
    public AnimationClip Jump;
    public AnimationClip Attack1;
    public AnimationClip Attack2;
    public AnimationClip Die;
}

[System.Serializable]
public class AnimationStates
{
    public string Idle = "Idle";
    public string WalkForward = "WalkForward";
    public string WalkBackward = "WalkBackward";
    public string WalkLeft = "WalkLeft";
    public string WalkRight = "WalkRight";
    public string RunForward = "RunForward";
    public string RunBackward = "RunBackward";
    public string RunLeft = "RunLeft";
    public string RunRight = "RunRight";
    public string Jump = "Jump";
    public string Attack1 = "Attack1";
    public string Attack2 = "Attack2";
    public string Die = "Die";
}
