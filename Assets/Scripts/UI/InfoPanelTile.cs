using TMPro;
using UnityEngine;

namespace UI
{
    public class InfoPanelTile : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text ping;
        [SerializeField]
        private TMP_Text nickname;
        [SerializeField]
        private TMP_Text deathCount;
        [SerializeField]
        private TMP_Text score;

        public void SetValues(InfoTileData data)
        {
            ping.text = "" + data.Ping;
            nickname.text = data.Nickname;
            deathCount.text = "" + data.DeathCount;
            score.text = "" + data.Score;
        }

        public struct InfoTileData
        {
            public int Ping;
            public string Nickname;
            public int DeathCount;
            public int Score;
        }
    }
}
