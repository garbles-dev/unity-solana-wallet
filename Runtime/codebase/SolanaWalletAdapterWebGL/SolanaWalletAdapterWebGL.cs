﻿using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AOT;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Wallet;
using UnityEngine;

// ReSharper disable once CheckNamespace

namespace Solana.Unity.SDK
{
    [Serializable]
    public class SolanaWalletAdapterWebGLOptions
    {
        public GameObject walletAdapterButtonPrefab;

        public GameObject walletAdapterUIPrefab;
    }
    public class SolanaWalletAdapterWebGL: WalletBase
    {
        private static SolanaWalletAdapterWebGLOptions _walletOptions;
        private static TaskCompletionSource<Account> _loginTaskCompletionSource;
        private static TaskCompletionSource<string> _getWalletsTaskCompletionSource;
        private static TaskCompletionSource<bool> _loadedScriptTaskCompletionSource;
        private static TaskCompletionSource<Transaction> _signedTransactionTaskCompletionSource;
        private static TaskCompletionSource<byte[]> _signedMessageTaskCompletionSource;
        private static Transaction _currentTransaction;
        private static Account _account;
        public static GameObject WalletAdapterUI { get; private set; }

        [Serializable]
        public class WalletSpecs
        {
            public string name;
            public bool installed;
            public string icon;
            public override string ToString()
            {
                return $"{name}: installed? {installed}";
            }
        }
        
        [Serializable]
        public class WalletSpecsObject
        {
            public WalletSpecs[] wallets;
        }

        
        public static WalletSpecs[] Wallets { get; private set; }

        private static WalletSpecs _currentWallet;
            

        public SolanaWalletAdapterWebGL(
            SolanaWalletAdapterWebGLOptions solanaWalletOptions,
            RpcCluster rpcCluster = RpcCluster.DevNet,
            string customRpcUri = null,
            string customStreamingRpcUri = null,
            bool autoConnectOnStartup = false) 
            : base(rpcCluster, customRpcUri, customStreamingRpcUri, autoConnectOnStartup)
        {
            _walletOptions = solanaWalletOptions;
        }
        
        private static async Task InitWallets() {
            # if UNITY_WEBGL && !UNITY_EDITOR
            if (Wallets == null){
                _loadedScriptTaskCompletionSource = new TaskCompletionSource<bool>();
                InitWalletAdapter(OnScriptLoaded);
                await _loadedScriptTaskCompletionSource.Task;
            }
            _getWalletsTaskCompletionSource = new TaskCompletionSource<string>();
            ExternGetWallets(OnWalletsLoaded);
            var walletsData = await _getWalletsTaskCompletionSource.Task;
            # else
            var walletsData = "{\"wallets\":[{\"name\":\"Phantom\",\"installed\":true},{\"name\":\"Solflare\",\"installed\":true},{\"name\":\"Sollet\",\"installed\":true},{\"name\":\"Sollet.io\",\"installed\":true},{\"name\":\"Ledger Wallet\",\"installed\":true},{\"name\":\"Token Pocket\",\"installed\":true}]}\n";
            # endif
            Wallets = JsonUtility.FromJson<WalletSpecsObject>(walletsData).wallets;
           
        }



        protected override async Task<Account> _Login(string password = null)
        {
            await SetCurrentWallet();
            _loginTaskCompletionSource = new TaskCompletionSource<Account>();
            try
            {
                ExternConnectWallet(_currentWallet.name, OnWalletConnected);
            }
            catch (Exception e)
            {
                Debug.LogError("WalletAdapter _Login -> Exception: " + e);
                _loginTaskCompletionSource.SetResult(null);
            }
            WalletAdapterUI.SetActive(false);
            return await _loginTaskCompletionSource.Task;
        }
        
        private static async Task SetCurrentWallet()
        {
            await InitWallets();
            if (WalletAdapterUI == null)
            {
                WalletAdapterUI = GameObject.Instantiate(_walletOptions.walletAdapterUIPrefab);
            }
           
            var waitForWalletSelectionTask = new TaskCompletionSource<string>();
            var walletAdapterScreen = WalletAdapterUI.transform.GetChild(0).gameObject.GetComponent<WalletAdapterScreen>();
            walletAdapterScreen.viewPortContent = WalletAdapterUI.transform.GetChild(0).Find("Scroll View").Find("Viewport").Find("Content").GetComponent<RectTransform>();
            walletAdapterScreen.buttonPrefab = _walletOptions.walletAdapterButtonPrefab;
            walletAdapterScreen.OnSelectedAction = walletName =>
            {
                waitForWalletSelectionTask.SetResult(walletName);
            };
            WalletAdapterUI.SetActive(true);
            var walletName = await waitForWalletSelectionTask.Task;
            _currentWallet = Array.Find(Wallets, wallet => wallet.name == walletName);
        }

        protected override Task<Transaction> _SignTransaction(Transaction transaction)
        {
            _signedTransactionTaskCompletionSource = new TaskCompletionSource<Transaction>();
            _currentTransaction = transaction;
            var base64TransactionStr = Convert.ToBase64String(transaction.Serialize()) ;
            ExternSignTransactionWallet(_currentWallet.name,base64TransactionStr, OnTransactionSigned);
            return _signedTransactionTaskCompletionSource.Task;
        }

        public override Task<byte[]> SignMessage(byte[] message)
        {
            _signedMessageTaskCompletionSource = new TaskCompletionSource<byte[]>();
            var base64MessageStr = Convert.ToBase64String(message) ;
            ExternSignMessageWallet(_currentWallet.name,base64MessageStr, OnMessageSigned);
            return _signedMessageTaskCompletionSource.Task;
        }
        
        protected override Task<Account> _CreateAccount(string mnemonic = null, string password = null)
        {
            throw new NotImplementedException();
        }
        
        #region WebGL Callbacks
        
        /// <summary>
        /// Called from javascript when the wallet adapter approves the connection
        /// </summary>
        [MonoPInvokeCallback(typeof(Action<string>))]
        private static void OnWalletConnected(string walletPubKey)
        {
            Debug.Log($"Wallet {walletPubKey} connected!");
            _account = new Account("", walletPubKey);
            _loginTaskCompletionSource.SetResult(_account);
        }

        /// <summary>
        /// Called from javascript when the wallet signed the transaction and return the signature
        /// that we then need to put into the transaction before we send it out.
        /// </summary>
        [MonoPInvokeCallback(typeof(Action<string>))]
        public static void OnTransactionSigned(string signature)
        {
            _currentTransaction.Signatures.Add(new SignaturePubKeyPair()
            {
                PublicKey = _account.PublicKey,
                Signature = Convert.FromBase64String(signature)
            });
            _signedTransactionTaskCompletionSource.SetResult(_currentTransaction);
        }
        
        /// <summary>
        /// Called from javascript when the wallet adapter signed the message and return the signature.
        /// </summary>
        [MonoPInvokeCallback(typeof(Action<string>))]
        public static void OnMessageSigned(string signature)
        {
            _signedMessageTaskCompletionSource.SetResult(Convert.FromBase64String(signature));
        }
        
        /// <summary>
        /// Called from javascript when the wallet adapter script is loaded
        /// </summary>
        [MonoPInvokeCallback(typeof(Action<bool>))]
        private static void OnScriptLoaded(bool success)
        {
            _loadedScriptTaskCompletionSource.SetResult(success);
        }
        
        /// <summary>
        /// Called from javascript when the wallets are loaded
        /// </summary>
        [MonoPInvokeCallback(typeof(Action<string>))]
        private static void OnWalletsLoaded(string walletsData)
        {
            _getWalletsTaskCompletionSource.SetResult(walletsData);
        }

        #endregion

        #if UNITY_WEBGL
                
                [DllImport("__Internal")]
                private static extern void ExternConnectWallet(string walletName,Action<string> callback);

                [DllImport("__Internal")]
                private static extern void ExternSignTransactionWallet(string walletName, string transaction, Action<string> callback);
                
                [DllImport("__Internal")]
                private static extern void ExternSignMessageWallet(string walletName, string messageBase64, Action<string> callback);
        
                [DllImport("__Internal")]
                private static extern string  ExternGetWallets(Action<string> callback);

                [DllImport("__Internal")]
                private static extern void InitWalletAdapter(Action<bool> callback);
                
                
        #else
                private static void ExternConnectWallet(string walletName, Action<string> callback){}
                private static void ExternSignTransactionWallet(string walletName, string transaction, Action<string> callback){}
                private static void ExternSignMessageWallet(string walletName, string messageBase64, Action<string> callback){}
                private static string ExternGetWallets(Action<bool> callback){return null;}
                private static void InitWalletAdapter(Action<bool> callback){}
                
        #endif
    }
}
