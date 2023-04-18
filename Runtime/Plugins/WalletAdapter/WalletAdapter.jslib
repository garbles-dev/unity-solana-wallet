﻿mergeInto(LibraryManager.library, {
    InitWalletAdapter: async function () {
        // Add UnityWalletAdapter from CDN
        if(window.walletAdapterLib == undefined){
            console.log("Adding JS")
            var script = document.createElement("script");
            script.src = "https://cdn.jsdelivr.net/gh/nicoeft/unity-wallet-adapter@main/dist/wallet-adapter-lib.js";
            document.head.appendChild(script);
        }
        console.log(window.walletAdapterLib);
    },
     ExternGetWallets: function() {
        console.log("ExternGetWallets called");
        try {
            const wallets = window.walletAdapterLib.getWallets();
            var bufferSize = lengthBytesUTF8(wallets) + 1;
            var walletsPtr = _malloc(bufferSize);
            stringToUTF8(wallets, walletsPtr, bufferSize);
            console.log("walletsPtr: " + walletsPtr);
            return walletsPtr;
        } catch (err) {
            console.error(err.message);
        }
    },
    ExternConnectWallet: async function (walletNamePtr, callback) {
        console.log("ExternConnectWallet called");
         try {
                console.log("window.walletAdapterLib: ", window.walletAdapterLib);
                const walletName = UTF8ToString(walletNamePtr)
                var pubKey = await window.walletAdapterLib.connectWallet(walletName);
                console.log("pubKey: " + pubKey);                
                var bufferSize = lengthBytesUTF8(pubKey) + 1;
                var pubKeyPtr = _malloc(bufferSize);
                stringToUTF8(pubKey, pubKeyPtr, bufferSize);
                console.log("pubKeyPtr: " + pubKeyPtr);
                Module.dynCall_vi(callback, pubKeyPtr);
         } catch (err) {
            console.error(err.message);
         }
    },
    ExternSignTransactionWallet: async function (walletNamePtr, transactionPtr, callback) {
        console.log("ExternSignTransactionWallet called");
         try {
                console.log("window.walletAdapterLib: ", window.walletAdapterLib);
                const walletName = UTF8ToString(walletNamePtr)
                var base64transaction = UTF8ToString(transactionPtr)
                console.log("base64transaction: " + base64transaction);
                var signedTransaction = await window.walletAdapterLib.signTransaction(walletName, base64transaction);
                var signature = signedTransaction.signature.toString('base64');
                console.log("signature: " + signature);
                var bufferSize = lengthBytesUTF8(signature) + 1;
                var signaturePtr = _malloc(bufferSize);
                stringToUTF8(signature, signaturePtr, bufferSize);
                console.log("signaturePtr: " + signaturePtr);
                Module.dynCall_vi(callback, signaturePtr);          
         } catch (err) {
            console.error(err.message);
         }
    },
} );



 