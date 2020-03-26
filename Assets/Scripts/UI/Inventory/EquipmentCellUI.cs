/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using MULTIPLAYER_GAME.Entities;
using MULTIPLAYER_GAME.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MULTIPLAYER_GAME.Inventory.UI
{
    public class EquipmentCellUI : MonoBehaviour, IPointerClickHandler
    {
        public Systems.EquipmentSlot equipmentSlot;

        public void OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case (PointerEventData.InputButton.Left):
                    Player.localPlayer.CmdTakeOffEquipment(equipmentSlot);
                    break;
                case (PointerEventData.InputButton.Middle):
                    Debug.Log("ON CLICK Middle");
                    break;
                case (PointerEventData.InputButton.Right):
                    Debug.Log("ON CLICK Right");
                    break;
            }
        }
    }
}
