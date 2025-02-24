namespace MAGES.Editor.VSAttribution
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Analytics;

    /// <summary>
    /// Attribution event for Verified Solutions Partners.
    /// </summary>
    [InitializeOnLoad]
    public static class VSAttribution
    {
        private const int KVersionId = 4;
        private const int KMaxEventsPerHour = 10;
        private const int KMaxNumberOfElements = 1000;

        private const string KVendorKey = "unity.vsp-attribution";
        private const string KEventName = "vspAttribution";

        static VSAttribution()
        {
            DeveloperAuthentication.LoggedIn.AddListener(() =>
            {
                DeveloperAuthentication.GetUserInfo((RequestResult res, User? info) =>
                {
                    if (res.ResponseCode == 200)
                    {
                        SendAttributionEvent("Login", "ORamaVR", info.Value.Username);
                    }
                });
            });
        }

        /// <summary>
        /// Registers and attempts to send a Verified Solutions Attribution event.
        /// </summary>
        /// <param name="actionName">Name of the action, identifying a place this event was called from.</param>
        /// <param name="partnerName">Identifiable Verified Solutions Partner's name.</param>
        /// <param name="customerUid">Unique identifier of the customer using Partner's Verified Solution.</param>
        /// <returns>The Analytics result.</returns>
        public static AnalyticsResult SendAttributionEvent(string actionName, string partnerName, string customerUid)
        {
            try
            {
                // Are Editor Analytics enabled ? (Preferences)
                if (!EditorAnalytics.enabled)
                {
                    return AnalyticsResult.AnalyticsDisabled;
                }

#if !UNITY_2023_2_OR_NEWER
                if (!RegisterEvent())
                {
                    return AnalyticsResult.InvalidData;
                }
#endif

                // Create an expected data object
                var eventData = new VSAttributionData
                {
                    ActionName = actionName,
                    PartnerName = partnerName,
                    CustomerUid = customerUid,
                    Extra = "{}",
                };
#if UNITY_2023_2_OR_NEWER
                VSAttributionAnalytic analytic = new VSAttributionAnalytic(eventData);
                return EditorAnalytics.SendAnalytic(analytic);
#else
                return EditorAnalytics.SendEventWithLimit(KEventName, eventData, KVersionId);
#endif
            }
            catch
            {
                // Fail silently
                return AnalyticsResult.AnalyticsDisabled;
            }
        }

#if !UNITY_2023_2_OR_NEWER
        private static bool RegisterEvent()
        {
            AnalyticsResult result = EditorAnalytics.RegisterEventWithLimit(KEventName, KMaxEventsPerHour, KMaxNumberOfElements, KVendorKey, KVersionId);

            var isResultOk = result == AnalyticsResult.Ok;
            return isResultOk;
        }
#endif

        /// <summary>
        /// VSAttributionData is the data object that will be sent to the Analytics server.
        /// </summary>
        [Serializable]
        private struct VSAttributionData
#if UNITY_2023_2_OR_NEWER
            : IAnalytic.IData
#endif
        {
            public string ActionName;
            public string PartnerName;
            public string CustomerUid;
            public string Extra;
        }

#if UNITY_2023_2_OR_NEWER
        [AnalyticInfo(eventName: KEventName, vendorKey: KVendorKey, maxEventsPerHour: KMaxEventsPerHour, maxNumberOfElements: KMaxNumberOfElements, version: KVersionId)]
        private class VSAttributionAnalytic : IAnalytic
        {
            private VSAttributionData dataVS;

            public VSAttributionAnalytic(VSAttributionData data)
            {
                data = dataVS;
            }

            public bool TryGatherData(out IAnalytic.IData data, out Exception error)
            {
                error = null;
                data = dataVS;
                return data != null;
            }
        }
#endif
    }
}