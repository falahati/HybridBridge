      Soroush Falahati's HybridBridge Library
------------------------------------------------------
This package is the Windows platform package of the HybridBridge library, expanding its functionalities
to the classic Windows environment supporting both Windows Forms and WPF project.
Using this library you can simply connect the WebBrowser control with your controller class:

new WebBrowserHybridServer(webBrowser).Add(new ClassBridge<MethodSamplesController>());

`WebBrowserHybridServer` class opens a local port and handles the HTTP requests. 

For more information about this project and  usage  samples, please visit the project's GitHub page  at
https://github.com/falahati/HybridBridge