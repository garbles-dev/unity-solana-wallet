﻿mergeInto(LibraryManager.library, {
    InitWalletAdapter: async function (callback) {
        // Add UnityWalletAdapter from CDN
        if(window.walletAdapterLib == undefined){
            console.log("Adding WalletAdapterLib")
            var script = document.createElement("script");
            script.src = "https://cdn.jsdelivr.net/gh/magicblock-labs/unity-js-wallet-adapter@main/dist/wallet-adapter-lib.js";
            document.head.appendChild(script);
            script.onload = function() {
                console.log("WalletAdapterLib loaded");
                Module.dynCall_vi(callback);
            };
        }
        console.log(window.walletAdapterLib);
    },
     ExternGetWallets: async function(callback) {
        try {
            const wallets = await window.walletAdapterLib.getWallets();
            var bufferSize = lengthBytesUTF8(wallets) + 1;
            var walletsPtr = _malloc(bufferSize);
            stringToUTF8(wallets, walletsPtr, bufferSize);
            Module.dynCall_vi(callback, walletsPtr);
        } catch (err) {
            console.error(err.message);
        }
    },
    ExternConnectWallet: async function (walletNamePtr, callback) {
         try {
                const walletName = UTF8ToString(walletNamePtr)
                var pubKey = await window.walletAdapterLib.connectWallet(walletName);
                var bufferSize = lengthBytesUTF8(pubKey) + 1;
                var pubKeyPtr = _malloc(bufferSize);
                stringToUTF8(pubKey, pubKeyPtr, bufferSize);
                Module.dynCall_vi(callback, pubKeyPtr);
         } catch (err) {
            console.error(err.message);
         }
    },
    ExternSignTransactionWallet: async function (walletNamePtr, transactionPtr, callback) {
         try {
                const walletName = UTF8ToString(walletNamePtr)
                var base64transaction = UTF8ToString(transactionPtr)
                var signedTransaction = await window.walletAdapterLib.signTransaction(walletName, base64transaction);
                var signature = signedTransaction.signature.toString('base64');
                var bufferSize = lengthBytesUTF8(signature) + 1;
                var signaturePtr = _malloc(bufferSize);
                stringToUTF8(signature, signaturePtr, bufferSize);
                Module.dynCall_vi(callback, signaturePtr);          
         } catch (err) {
            console.error(err.message);
         }
    },
    ExternSignMessageWallet: async function (walletNamePtr, messagePtr, callback) {
         try {
                const walletName = UTF8ToString(walletNamePtr)
                var base64Message = UTF8ToString(messagePtr)
                var signature = await window.walletAdapterLib.signMessage(walletName, base64Message);
                var signatureStr =  signature.toString('base64');
                var bufferSize = lengthBytesUTF8(signatureStr) + 1;
                var signaturePtr = _malloc(bufferSize);
                stringToUTF8(signatureStr, signaturePtr, bufferSize);
                Module.dynCall_vi(callback, signaturePtr);          
         } catch (err) {
            console.error(err.message);
         }
    },
     ExternSignAllTransactionsWallet: async function (walletNamePtr, transactionsPtr, callback) {
         try {
                const walletName = UTF8ToString(walletNamePtr)
                var base64transactionsStr = UTF8ToString(transactionsPtr)
                var base64transactions = base64transactionsStr.split(',');
                var signedTransactions = await window.walletAdapterLib.signAllTransactions(walletName, base64transactions);
                var signatures = [];
                for (var i = 0; i < signedTransactions.length; i++) {
                    var signedTransaction = signedTransactions[i];
                    var signature = signedTransaction.signature.toString('base64');
                    signatures.push(signature);
                }
                var signaturesStr = signatures.join(',');
                var bufferSize = lengthBytesUTF8(signaturesStr) + 1;
                var signaturesPtr = _malloc(bufferSize);
                stringToUTF8(signaturesStr, signaturesPtr, bufferSize);
                Module.dynCall_vi(callback, signaturesPtr);
        } catch (err) {
            console.error(err.message);
        }
    },
} );



 