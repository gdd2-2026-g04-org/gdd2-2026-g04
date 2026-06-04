using GameAssets.Battle;
using UnityEngine;

public class BossAnimation : MonoBehaviour
{
    private Animator _animator;
    void Start()
    {
        _animator = GetComponent<Animator>();
        TurnManager.Instance.OnBossTurnStart += BossSwipeAnim;
    }

    private void BossSwipeAnim()
    {
        _animator.SetTrigger("Swipe");
    }
    
    
}
