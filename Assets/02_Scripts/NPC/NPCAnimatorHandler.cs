using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _02_Scripts.NPC
{
    [RequireComponent(typeof(Animator))]
    public class NPCAnimatorHandler : MonoBehaviour
    {
        private Animator animator;
        private void Start()
        {
            animator = GetComponent<Animator>();
            // animator.SetBool("UseIdle1", false);
            // animator.SetBool("UseIdle2", false);
            // animator.SetBool("UseIdle4", false);
            // animator.SetBool("UseIdle3", true);
            animator.SetInteger("DirIndex", 2);
            animator.SetFloat("Direction", 2);
        }
    }
}