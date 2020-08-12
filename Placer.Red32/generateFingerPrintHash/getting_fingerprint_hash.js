(function() {
    if (window.requestIdleCallback) {
        requestIdleCallback(function () {
            Fingerprint2.get(function (components) {
                let values = components.map(function(component) { return component.value});
                console.log(components) // an array of components: {key: ..., value: ...}
                let fingerPrintHash = Fingerprint2.x64hash128(values.join(''), 31)
                download("fingerPrintHash.txt", fingerPrintHash);
            })
        })
    } else {
        setTimeout(function () {
            Fingerprint2.get(function (components) {
                let values = components.map(function(component) { return component.value});
                console.log(components) // an array of components: {key: ..., value: ...}
                let fingerPrintHash = Fingerprint2.x64hash128(values.join(''), 31)
                download("fingerPrintHash.txt", fingerPrintHash);
            })  
        }, 500)
    }
})();

function download(filename, text) {
    var element = document.createElement('a');
    element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
    element.setAttribute('download', filename);
  
    element.style.display = 'none';
    document.body.appendChild(element);
  
    element.click();
  
    document.body.removeChild(element);
  }

/*
0{key: "userAgent", value: "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWeb…ML, like Gecko) Chrome/79.0.3945.88 Safari/537.36"}
1: {key: "language", value: "ru-RU"}
2: {key: "colorDepth", value: 24}
3: {key: "deviceMemory", value: 8}
4: {key: "hardwareConcurrency", value: 12}
5: {key: "screenResolution", value: Array(2)}
6: {key: "availableScreenResolution", value: Array(2)}
7: {key: "timezoneOffset", value: -540}
8: {key: "timezone", value: "Asia/Chita"}
9: {key: "sessionStorage", value: true}
10: {key: "localStorage", value: true}
11: {key: "indexedDb", value: true}
12: {key: "addBehavior", value: false}
13: {key: "openDatabase", value: true}
14: {key: "cpuClass", value: "not available"}
15: {key: "platform", value: "Win32"}
16: {key: "plugins", value: Array(3)}
17: {key: "canvas", value: Array(2)}
18: {key: "webgl", value: Array(65)}
19: {key: "webglVendorAndRenderer", value: "Google Inc.~ANGLE (NVIDIA GeForce GT 1030 Direct3D11 vs_5_0 ps_5_0)"}
20: {key: "adBlock", value: false}
21: {key: "hasLiedLanguages", value: false}
22: {key: "hasLiedResolution", value: false}23: {key: "hasLiedOs", value: false}
24: {key: "hasLiedBrowser", value: false}
25: {key: "touchSupport", value: Array(3)}
26: {key: "fonts", value: Array(40)}
27: {key: "audio", value: "124.04344884395687"}
length: 28__proto__: Array(0) */