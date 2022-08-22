﻿using System;
using Solana.Unity.Rpc.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Solana.Unity.SDK.Example
{
    [RequireComponent(typeof(TxtLoader))]
    public class WalletScreen : SimpleScreen
    {
        public TextMeshProUGUI lamports;
        public Button refresh_btn;
        public Button send_btn;
        public Button receive_btn;
        public Button logout_btn;
        public Button save_mnemonics_btn;
        public Button save_private_key_btn;
        
        public GameObject tokenItem;
        public Transform tokenContainer;

        public KnownTokens knownTokens;
        public SimpleScreenManager parentManager;

        private TxtLoader _txtLoader;
        private CancellationTokenSource stopTask;

        private const string _mnemonicsFileTitle = "Mnemonics";
        private const string _privateKeyFileTitle = "PrivateKey";
        private List<GameObject> _instantiatedTokens;

        void Start()
        {
            _txtLoader = GetComponent<TxtLoader>();
            _instantiatedTokens = new List<GameObject>();
            WebSocketActions.WebSocketAccountSubscriptionAction += (bool istrue) => 
            {
                MainThreadDispatcher.Instance().Enqueue(() => { UpdateWalletBalanceDisplay(); });
            };
            WebSocketActions.CloseWebSocketConnectionAction += DisconnectToWebSocket;
            refresh_btn?.onClick.AddListener(() =>
            {
                UpdateWalletBalanceDisplay();
                GetOwnedTokenAccounts();
            });

            send_btn?.onClick.AddListener(() =>
            {
                TransitionToTransfer();
            });

            receive_btn?.onClick.AddListener(() =>
            {
                manager.ShowScreen(this, "receive_screen");
            });

            logout_btn.onClick.AddListener(() =>
            {
                SimpleWallet.Instance.Wallet.Logout();
                manager.ShowScreen(this, "login_screen");
                if(parentManager != null)
                    parentManager.ShowScreen(this, "[Connect_Wallet_Screen]");
            });

            save_private_key_btn.onClick.AddListener(() => 
            {
                _txtLoader.SaveTxt(_privateKeyFileTitle, SimpleWallet.Instance.Wallet.Account.PrivateKey, false);
            });

            save_mnemonics_btn.onClick.AddListener(() =>
            {
                _txtLoader.SaveTxt(_mnemonicsFileTitle, SimpleWallet.Instance.Wallet.Mnemonic.ToString(), false);
            });

            _txtLoader.TxtSavedAction += SaveMnemonicsOnClick;
            _txtLoader.TxtSavedAction += SavePrivateKeyOnClick;

            stopTask = new CancellationTokenSource();
        }

        private void SavePrivateKeyOnClick(string path, string key, string fileTitle)
        {
            if (!this.gameObject.activeSelf) return;
            if (fileTitle != _privateKeyFileTitle) return;

            //List<string> list = new List<string>();
            //foreach (byte item in key)
            //{
            //    list.Add(item.ToString());
            //}

            if (path != string.Empty)
            {
                File.WriteAllText(path, key);
            }
            else
            {
                //string result = string.Join(Environment.NewLine, list);
                var bytes = Encoding.UTF8.GetBytes(key);
                DownloadFile(gameObject.name, "OnFileDownload", _privateKeyFileTitle + ".txt", bytes, bytes.Length);
            }
        }

        private void SaveMnemonicsOnClick(string path, string mnemonics, string fileTitle)
        {
            if (!gameObject.activeSelf) return;
            if (fileTitle != _mnemonicsFileTitle) return;

            if (SimpleWallet.Instance.StorageMethodReference == StorageMethod.JSON)
            {
                List<string> mnemonicsList = new List<string>();

                string[] splittedStringArray = mnemonics.Split(' ');
                foreach (string stringInArray in splittedStringArray)
                {
                    mnemonicsList.Add(stringInArray);
                }
                MnemonicsModel mnemonicsModel = new MnemonicsModel
                {
                    Mnemonics = mnemonicsList
                };

                if(path != string.Empty)
                    File.WriteAllText(path, JsonConvert.SerializeObject(mnemonicsModel));
                else
                {
                    var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(mnemonicsModel));
                    DownloadFile(gameObject.name, "OnFileDownload", _mnemonicsFileTitle + ".txt", bytes, bytes.Length);
                }
            }
            else if (SimpleWallet.Instance.StorageMethodReference == StorageMethod.SimpleTxt)
            {
                if(path != string.Empty)
                    File.WriteAllText(path, mnemonics);
                else
                {
                    var bytes = Encoding.UTF8.GetBytes(mnemonics);
                    DownloadFile(gameObject.name, "OnFileDownload", _mnemonicsFileTitle + ".txt", bytes, bytes.Length);
                }
            }
        }

        private void TransitionToTransfer(object data = null)
        {
            manager.ShowScreen(this, "transfer_screen", data);
        }

        private async void UpdateWalletBalanceDisplay()
        {
            if (SimpleWallet.Instance.Wallet.Account is null) return;

            double sol = await SimpleWallet.Instance.Wallet.GetBalance();
            if (SimpleWallet.Instance.Wallet.Account is null) return;
            MainThreadDispatcher.Instance().Enqueue(() => { lamports.text = $"{sol}"; });
        }

        private void DisconnectToWebSocket()
        {
            MainThreadDispatcher.Instance().Enqueue(() => { manager.ShowScreen(this, "login_screen"); });
            MainThreadDispatcher.Instance().Enqueue(() => { SimpleWallet.Instance.Wallet.Logout(); });
        }

        public override void ShowScreen(object data = null)
        {
            base.ShowScreen();
            gameObject.SetActive(true);
            UpdateWalletBalanceDisplay();
            GetOwnedTokenAccounts();
        }

        public override void HideScreen()
        {
            base.HideScreen();
            gameObject.SetActive(false);
        }

        private async Task GetOwnedTokenAccounts()
        {
            DisableTokenItems();
            var result = await SimpleWallet.Instance.Wallet.GetTokenAccounts();

            if (result is {Length: > 0})
            {
                foreach (var item in result)
                {
                    if (!(float.Parse(item.Account.Data.Parsed.Info.TokenAmount.Amount) > 0)) continue;
                    var nft = await Nft.Nft.TryGetNftData(item.Account.Data.Parsed.Info.Mint, SimpleWallet.Instance.Wallet.ActiveRpcClient);

                    var tk = Instantiate(tokenItem, tokenContainer, true);
                    tk.transform.localScale = Vector3.one;
                    _instantiatedTokens.Add(tk);
                    tk.SetActive(true);
                    tk.GetComponent<TokenItem>().InitializeData(item, this, nft);
                }
            }
        }


        private void OnDestroy()
        {
            if (stopTask is null) return;
            stopTask.Cancel();
        }

        void DisableTokenItems()
        {
            if(_instantiatedTokens == null) return;
            foreach (GameObject token in _instantiatedTokens)
            {
                Destroy(token);
            }
            _instantiatedTokens.Clear();
        }

        //
        // WebGL
        //
        [DllImport("__Internal")]
        private static extern void DownloadFile(string gameObjectName, string methodName, string filename, byte[] byteArray, int byteArraySize);

        // Called from browser
        public void OnFileDownload()
        {
            
        }
    }
}
