﻿using Unity.Services.Vivox;
using UnityEngine;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Manages Vivox session
    /// </summary>
    public class VivoxSession : MonoBehaviour
    {
        public bool IsConnecting { get; private set; }
        public bool IsLogged => VivoxService.Instance.IsLoggedIn;
        

        public void Login(string displayName)
        {
            if (IsConnecting)
                return;

            if (string.IsNullOrEmpty(displayName))
            {
                Debug.LogWarning("[VIVOX] The user's DisplayName is missing or invalid. Vivox will utilize the default value as a fallback.");
                displayName = "Player";
            }

            if (VivoxService.Instance == null)
            {
                Debug.Log($"VivoxService.Instance is null: {VivoxService.Instance}");
                return;
            }

            LoginToVivoxAsync(displayName);
        }
        
        private async void LoginToVivoxAsync(string displayName)
        {
            IsConnecting = true;
            LoginOptions options = new LoginOptions();
            options.DisplayName = displayName;
            options.ParticipantUpdateFrequency = ParticipantPropertyUpdateFrequency.FivePerSecond;
            Debug.Log($"[VIVOX] Starting login user {displayName}");
            await VivoxService.Instance.LoginAsync(options);
            IsConnecting = false;
        }

        public async void ClosingClientConnection()
        {
            if(IsLogged)
                await VivoxService.Instance.LogoutAsync();
        }
    }
}