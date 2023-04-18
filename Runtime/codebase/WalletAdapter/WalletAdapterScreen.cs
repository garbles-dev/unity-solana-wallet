﻿using UnityEngine;
using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace

namespace Solana.Unity.SDK
{
   
    public class WalletAdapterScreen: MonoBehaviour
    {
        [SerializeField]
        private GameObject walletButtonPrefab;
        [SerializeField]
        private RectTransform walletListScrollTransform;
        [SerializeField]
        public Action<string> OnSelectedAction;
        [SerializeField]
        private HashSet<string> addedWallets;

        
         private void Start()
        {
            Debug.Log("WalletAdapterScreen Start");
            addedWallets = new HashSet<string>();
            Debug.Log("WalletAdapterScreen Start Done");
        }
         
        private void OnEnable()
        {
            Debug.Log("WalletAdapterScreen OnEnable");
            AddWalletAdapterButtons();
            Debug.Log("WalletAdapterScreen OnEnable Done");
        }
        
        
         
         private async void AddWalletAdapterButtons()
         {
             Debug.Log("Adding Wallet Adapter Buttons");
            
             
             Debug.Log($"Len: {WalletAdapter.Wallets.Length}");
             
             
             
             foreach (var wallet in WalletAdapter.Wallets)
             {
                 if (addedWallets.Contains(wallet.name))
                 {
                     continue;  
                 }
                 addedWallets.Add(wallet.name);
                Debug.Log($"Wallet: {wallet.name}");
                var g = Instantiate(walletButtonPrefab, walletListScrollTransform);
                 var walletView = g.GetComponent<WalletAdapterButton>();
                 walletView.WalletNameLabel.text = wallet.name;
                 walletView.Name = wallet.name;
                 walletView.DetectedLabel.SetActive(wallet.installed);
                 
                 walletView.OnSelectedAction = walletName =>
                 {
                     Debug.Log($"Selected Wallet: {walletName} - {wallet.name}");
                     Debug.Log("Calling OnSelectedAction");
                     OnSelectedAction?.Invoke(walletName);
                     Debug.Log("Calling OnSelectedAction Done");
                 };
                 
             }
         }
         
         public void OnClose()
         {
             gameObject.SetActive(false);
         }
         
         
        
    }
}