      Soroush Falahati's HybridBridge Library
------------------------------------------------------
This package is the UWP platform package of the HybridBridge library, expanding its  functionalities
to the UWP environment.
Using this library you can simply connect the WebView control with your controller class:

new WebViewHybridBridge(webView).Add(new ClassBridge<MethodSamplesController>());

However, because of limitations of WebView control, some of the HybridBridge functionalities are not
available. For full support consider using the `WebViewHybridServer` class which opens a  local port
and handles the HTTP requests. For this to works, you need to have Server capacities.

new WebViewHybridServer(webView).Add(new ClassBridge<MethodSamplesController>());

For more information about this project and usage samples, please visit the project's GitHub page at
https://github.com/falahati/HybridBridge