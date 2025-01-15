using Shared.Interfaces.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BullGimmick : MonoBehaviour
{
    [SerializeField] float waaitSec;
    AudioSource audioSource;
    Animator animator;
    Coroutine coroutine;

    private void OnEnable()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        coroutine = StartCoroutine(PlayAnimalGimmick());
    }

    private void OnDisable()
    {
        StopCoroutine(coroutine);
        coroutine = null;
    }

    public void PlayEatAnim()
    {
        audioSource.Play();
        animator.Play("Eat", 0, 0);
    }

    IEnumerator PlayAnimalGimmick()
    {
        while (true)
        {
            yield return new WaitForSeconds(waaitSec);
            if (RoomModel.Instance.userState == RoomModel.USER_STATE.joined)
            {
                if (RoomModel.Instance.JoinedUsers[RoomModel.Instance.ConnectionId].IsMasterClient) 
                    this.PlayAnimalGimmickAsynk(this.transform.name, null);
            }
            else
            {
                PlayEatAnim();
            }
        }
    }

    async void PlayAnimalGimmickAsynk(string name, Vector3[] option)
    {
        await RoomModel.Instance.PlayAnimalGimmickAsynk(EnumManager.ANIMAL_GIMMICK_ID.Bull, name, option);
    }
}
