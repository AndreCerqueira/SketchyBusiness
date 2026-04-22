using DG.Tweening;
using FMOD.Studio;
using Project.Runtime.Scripts.Data;
using Project.Runtime.Scripts.Interaction.Interactables.Base;
using System.Collections;
using UnityEngine;

namespace Project.Runtime.Scripts.Interaction.Interactables
{ 
    public class HallwayDoorInteractable : BaseInteractable
    {
        [SerializeField] private float moveAmount;
        [SerializeField] private float moveTime;

        private bool _isOpen;

        public override InteractionAction Action => InteractionAction.Open;

        //public override InteractionAction Action => _isOpen ? InteractionAction.Close : InteractionAction.Open;

        protected override void Awake()
        {
            base.Awake();
        }

        //protected override void ConfigureInteractionSound(EventInstance instance)
        //{
        //    var stateLabel = _isOpen ? STATE_OPEN : STATE_CLOSE;
        //    instance.setParameterByNameWithLabel(STATE_PARAMETER, stateLabel);
        //}

        protected override void ExecuteInteraction(PlayerInteractionController interactor)
        {
            if (interactor != null)
                interactor.ForceUpdateAction();

            StartCoroutine(MoveDoor());
        }

        private IEnumerator MoveDoor()
        {
            float currentTime = 0.0f;
            Vector3 initialPos = transform.position;
            Vector3 endPos = new Vector3(transform.position.x , transform.position.y , transform.position.z - moveAmount); 

            while (currentTime < moveTime)
            {
                currentTime += Time.deltaTime;

                float lerpAmountZ = Mathf.Lerp(initialPos.z, endPos.z, (currentTime / moveTime));

                transform.position = new Vector3(transform.position.x, transform.position.y, lerpAmountZ);

                yield return null;
            }

        }
    }
}
