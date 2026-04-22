using Project.Runtime.Scripts.Data;

namespace Project.Runtime.Scripts.Interaction.Interactables.Base
{
    public interface IInteractable
    {
        InteractionAction Action { get; }
        void Focus();
        void Unfocus();
        void Interact(PlayerInteractionController interactor);
    }
}