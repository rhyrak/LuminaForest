using TMPro;
using UnityEngine;

namespace UI
{
    public class PlayerTile : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text playerName;
        [SerializeField]
        private TMP_Text playerType;
    
        public void SetValues(string nickname, string type)
        {
            playerName.text = nickname;
            playerType.text = type;
        }
    }
}
