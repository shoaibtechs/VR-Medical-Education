namespace MAGES.Networking
{
    using System;
    using System.Collections;
    using MAGES;
    using MAGES.Networking;
    using TMPro;
    using UnityEngine;

    /// <summary>
    /// Logic handler for session progress UI in multiplayer.
    /// </summary>
    public class SessionProgressUI : MonoBehaviour
    {
        private GameObject screenContent;
        [SerializeField]
        private TextMeshProUGUI identifierUI;
        [SerializeField]
        private TextMeshProUGUI timerUI;
        [SerializeField]
        private TextMeshProUGUI participantsUI;
        [SerializeField]
        private TextMeshProUGUI serverPingUI;

        private NetworkingModule networkingModule;
        private AnalyticsModule analyticsModule;
        private IMAGESNetworkIntegration integration;

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            screenContent = GameObject.Find("ΟRamaVR_Pictogram_Screen");
            if (screenContent != null)
            {
                screenContent.SetActive(false);
            }

            networkingModule = Hub.Instance.Get<NetworkingModule>();
            analyticsModule = Hub.Instance.Get<AnalyticsModule>();
            integration = networkingModule.Integration;
            StartCoroutine(UpdateUI());
        }

        private IEnumerator UpdateUI()
        {
            while (networkingModule.IsInitialized)
            {
                TimeSpan t = TimeSpan.FromSeconds(analyticsModule.GetTimeSinceStartUp());
                if (t.Hours <= 0)
                {
                    timerUI.text = "Session time: " + $"{t.Minutes:D2}:{t.Seconds:D2}";
                }
                else
                {
                    timerUI.text = "Session time: " + $"{t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";
                }

                identifierUI.text = "Session in progress \n ID: " + networkingModule.GetCurrentConnectedRoom();
                var numberOfUsers = integration.GetConnectedUsersToCurrentRoom();
                if (numberOfUsers > 1)
                {
                    participantsUI.text = "Participants: " + integration.GetConnectedUsersToCurrentRoom();
                }
                else
                {
                    participantsUI.text = "Participant: " + integration.GetConnectedUsersToCurrentRoom();
                }

                serverPingUI.text = CheckPing();
                yield return new WaitForSeconds(1f);
            }
        }

        private string CheckPing()
        {
            var ping = integration.GetPing();
            return ping switch
            {
                < 150 => "Network Quality: Excellent",
                < 300 => "Network Quality: Good",
                _ => "Network Quality: Poor",
            };
        }
    }
}